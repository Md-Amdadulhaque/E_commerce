namespace MCP_Server.Models
{
    public class ChatResponse
    {
        public string? Reply { get; set; } = string.Empty;
        public string? ConversationId { get; set; }
        public string? ToolUsed { get; set; } = string.Empty;
        public object? ToolResult { get; set; }
    }
}
