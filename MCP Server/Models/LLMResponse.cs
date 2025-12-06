using System.Text.Json;

namespace MCP_Server.Models
{
    public class LLMResponse
    {
        public string TextReply { get; set; } = "";
        public  string ?ToolUse { get; set; }
        public JsonElement ToolInput { get; set; }
    }
}
