using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace MCP_Server.Services
{
    public interface IToolService
    {
        List<object> GetToolDefinitions();
    }

    public class ToolService : IToolService
    {
        private readonly ILogger<ToolService> _logger;

        public ToolService(ILogger<ToolService> logger)
        {
            _logger = logger;
        }

        public List<object> GetToolDefinitions()
        {
            var tools = new List<object>();

            // Collect types safely from loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = new List<Type>();
            foreach (var asm in assemblies)
            {
                try
                {
                    allTypes.AddRange(asm.GetTypes());
                }
                catch (ReflectionTypeLoadException ex)
                {
                    if (ex.Types != null)
                        allTypes.AddRange(ex.Types.Where(t => t != null));
                }
                catch
                {
                    // ignore assemblies that fail to enumerate
                }
            }

            // Detect whether exact attribute types are present
            var hasExactToolTypeAttribute = allTypes.Any(t => t.Name == "McpServerToolTypeAttribute");
            var hasExactToolAttribute = allTypes.Any(t => t.Name == "McpServerToolAttribute");

            IEnumerable<Type> toolTypes;
            if (hasExactToolTypeAttribute && hasExactToolAttribute)
            {
                // Only honor explicit registrations when exact attribute types exist
                toolTypes = allTypes.Where(p => p.GetCustomAttribute<McpServerToolTypeAttribute>() != null);
            }
            else
            {
                // Fallback: detect by attribute-name fragments (works with shims or other packages)
                toolTypes = allTypes.Where(p => HasAttributeByName(p, "McpServerToolType") || p.GetCustomAttribute<McpServerToolTypeAttribute>() != null);
            }

            foreach (var toolType in toolTypes)
            {
                IEnumerable<MethodInfo> methods;
                if (hasExactToolTypeAttribute && hasExactToolAttribute)
                {
                    methods = toolType.GetMethods(BindingFlags.Public|BindingFlags.Instance)
                        .Where(m => m.GetCustomAttribute<McpServerToolAttribute>() != null);
                }
                else
                {
                    methods = toolType.GetMethods(BindingFlags.Public|BindingFlags.Instance)
                        .Where(m => HasAttributeByName(m, "McpServerTool") || m.GetCustomAttribute<McpServerToolAttribute>() != null);
                }

                foreach (var method in methods)
                {
                    var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "No description";
                    var parameters = method.GetParameters()
                        .Where(p => !IsInternalParameter(p))
                        .ToArray();

                    var toolDef = new
                    {
                        name = method.Name,
                        description = description,
                        parameters = new
                        {
                            type = "object",
                            properties = BuildParameterProperties(parameters),
                            required = parameters
                                .Where(p => !p.HasDefaultValue)
                                .Select(p => p.Name)
                                .ToArray()
                        }
                    };

                    tools.Add(toolDef);

                    _logger.LogInformation("Discovered tool: {Tool} with {ParamCount} parameters", method.Name, parameters.Length);
                }
            }

            return tools;
        }

        private Dictionary<string, object> BuildParameterProperties(ParameterInfo[] parameters)
        {
            var props = new Dictionary<string, object>();

                foreach (var param in parameters)
                {

                var description = param.GetCustomAttribute<DescriptionAttribute>()?.Description ?? param.Name;
                var typeName = param.ParameterType.Name.ToLower();

                var jsonType = typeName switch
                {
                    "int32" or "int64" => "integer",
                    "string" => "string",
                    "boolean" => "boolean",
                    "double" or "decimal" => "number",
                    _ => "string"
                };

                props[param.Name] = new
                {
                    type = jsonType,
                    description = description
                };
            }

            return props;
        }

        private static bool HasAttributeByName(MemberInfo member, string nameFragment)
        {
            return member.GetCustomAttributes(false)
                .Any(a => a.GetType().Name.IndexOf(nameFragment, System.StringComparison.OrdinalIgnoreCase) >= 0
                          || a.GetType().FullName?.IndexOf(nameFragment, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool HasAttributeByName(ParameterInfo parameter, string nameFragment)
        {
            return parameter.GetCustomAttributes(false)
                .Any(a => a.GetType().Name.IndexOf(nameFragment, System.StringComparison.OrdinalIgnoreCase) >= 0
                          || a.GetType().FullName?.IndexOf(nameFragment, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool IsInternalParameter(ParameterInfo p)
        {
            if (p == null) return false;

            var fullName = p.ParameterType.FullName ?? string.Empty;
            // Common internal types to hide from tool signatures
            var internalTypeFragments = new[]
            {
                "SourceClient",
                "HttpContext",
                "CancellationToken",
                "IServiceProvider",
                "ILogger",
                "HttpRequest",
                "HttpResponse"
            };

            if (internalTypeFragments.Any(f => fullName.IndexOf(f, System.StringComparison.OrdinalIgnoreCase) >= 0))
                return true;

            // Also hide by parameter name convention (e.g., client)
            if (string.Equals(p.Name, "client", System.StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
