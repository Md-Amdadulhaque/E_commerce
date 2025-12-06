using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using ModelContextProtocol.Server;
using MCP_Server.Models;
using MCP_Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMcpServer.Services;

namespace MCP_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly LLMClient _llmClient;
        private readonly ToolExecutor _toolExecutor;
        private readonly IToolService _toolService;

        public ChatController(LLMClient llmClient, ToolExecutor toolExecutor, IToolService toolService)
        {
            _llmClient = llmClient;
            _toolExecutor = toolExecutor;
            _toolService = toolService;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
        {
            // Auto-discover tools from ToolService
            var tools = _toolService.GetToolDefinitions();


           // var toolResult = await _toolExecutor.ExecuteAsync("GetProductsByCategory", { "Laptop"});

            var llmResponse = await _llmClient
                .ProcessMessageAsync(request, tools);

            if (llmResponse.ToolUse != null)
            {
                var toolName = llmResponse.ToolUse;
                var toolInput = llmResponse.ToolInput;
                var toolResult = await _toolExecutor.ExecuteAsync(toolName, toolInput);
                return Ok(new ChatResponse
                {
                    ToolUsed = toolResult?.ToolUsed,
                    ToolResult = toolResult?.Result,
                    Reply = "Found Product"
                });
            }
            return Ok(new ChatResponse
            {
                Reply = "No ToolFound"
            });
        }
    }
}
