using System.Text.Json;
using MCP_Server.Services;
using MCP_Server.Models;
using ModelContextProtocol.Protocol;
using MCP_Server.Tools;
using System.Reflection;
namespace MyMcpServer.Services;

public static class ObjectExtensions
{
    public static T? ToModel<T>(this object? obj)
    {
        if (obj == null) return default;

        if (obj is T typed)
            return typed;

        if (obj is JsonElement json)
            return json.Deserialize<T>();

        var jsonString = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(jsonString);
    }
}
public class ToolExecutor
{
    private readonly SourceClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly SourceProjectTools _sourceProjectTools;
    public ToolExecutor(SourceClient client,SourceProjectTools sourceProjectTools)
    {
        _client = client;
        _sourceProjectTools = sourceProjectTools;
    }

    public async Task<ToolResult?> ExecuteAsync(string toolName, JsonElement input)
    {

        var type = typeof(SourceProjectTools);
        var method = type.GetMethod(toolName,
        BindingFlags.Public | BindingFlags.Instance);

        if (method == null) throw new Exception("Method not found");


        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];

            if (input.TryGetProperty(p.Name!, out var val))
            {
                args[i] = JsonSerializer.Deserialize(val.GetRawText(), p.ParameterType);
            }
            else
            {
                args[i] = p.HasDefaultValue ? p.DefaultValue : null;
            }
        }
        if (args == null)
        {
            return new ToolResult();
        }
        var now = toolName switch
        {
            "GetCustomer" => await _sourceProjectTools.GetCustomer(args[0].ToString()),
            "GetCheapestProducts" => await _sourceProjectTools.GetCheapestProducts(int.Parse(args[0].ToString())),
            "GetProductsByCategory" => await _sourceProjectTools.GetProductsByCategory(args[0].ToString()),
            "GetAllCategory" => await _sourceProjectTools.GetAllCategory(),
            _ => new object()
        };

        ToolResult result = new ToolResult();
        result.Result = now;
        return result;

        // var instance = Activator.CreateInstance(type,_client);
        //var invoked = method.Invoke(instance, args);

        //object? finalResult;

        //if (invoked is Task task)
        //{
        //    await task.ConfigureAwait(false);

        //    // If Task<T>, get Result
        //    var taskType = task.GetType();
        //    if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<object>))
        //    {
        //        dynamic dynTask = task;
        //        finalResult = dynTask.Result;  // this is now List<string>
        //    }
        //    else
        //    {
        //        finalResult = null; // Task with no return
        //    }
        //}
        //else
        //{
        //    // synchronous return
        //    finalResult = invoked;
        //}

        //return new ToolResult
        //{
        //    Result = finalResult
        //};

    }
}