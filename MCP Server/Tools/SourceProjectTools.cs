using System.ComponentModel;
using ModelContextProtocol.Server;
using MCP_Server.Services;
using System.Text.Json;
namespace MCP_Server.Tools
{
    [McpServerToolType]
    public class SourceProjectTools
    {

        private readonly SourceClient _client;

        public SourceProjectTools(SourceClient sourceClient)
        {
            _client = sourceClient;
        }
        [McpServerTool]
        [Description("Get customer information by ID")]
        public async Task<string> GetCustomer(
            [Description("The customer ID")] string customerId)
        {
            var result = await _client.GetAsync<object>($"/api/customers/{customerId}");
            return System.Text.Json.JsonSerializer.Serialize(result);
        }

        [McpServerTool]
        [Description("Get top cheapest products from database")]
        public  async Task<string> GetCheapestProducts(
            [Description("Number of cheapest products to return")] int count = 5)
        {
            var result = await _client.GetAsync<object>($"/api/product/cheapest?count={count}");
            return System.Text.Json.JsonSerializer.Serialize(result);
        }

        // [McpServerTool]
        // [Description("Get all laptops from database")]
        // public static async Task<string> GetAllLaptops(
        //     SourceClient client)
        // {
        //     var result = await client.GetAsync<object>("/api/products?category=laptop");
        //     return System.Text.Json.JsonSerializer.Serialize(result);
        // }

        // [McpServerTool]
        // [Description("Get all Mac laptops from database")]
        // public static async Task<string> GetMacLaptops(
        //     SourceClient client)
        // {
        //     var result = await client.GetAsync<object>("/api/products?category=mac");
        //     return System.Text.Json.JsonSerializer.Serialize(result);
        // }

        // [McpServerTool]
        // [Description("Get all mobile phones from database")]
        // public static async Task<string> GetAllMobiles(
        //     SourceClient client)
        // {
        //     var result = await client.GetAsync<object>("/api/products?category=mobile");
        //     return System.Text.Json.JsonSerializer.Serialize(result);
        // }

        [McpServerTool]
        [Description("Get products by category (laptop, mac, mobile, etc)")]
        public  async Task<object?> GetProductsByCategory(
            [Description("Category name (e.g., laptop, mac, mobile)")] string category)
        {
            var result = await _client.GetAsync<object>($"/api/product/category/{category}");
            return result;
        }

        [McpServerTool]
        [Description("Get all categori from Api")]
        public async Task<object?> GetAllCategory()
        {
            var result = await _client.GetAsync<object>($"/api/Category");
            return result;
        }

        [McpServerTool]
        [Description("Get list of all available product categories")]
        public Task<string> CategoriesList()
        {
            var categories = new List<string>
            {
                "laptop",
                "mac",
                "mobile",
                "tablet",
                "accessories"
            };
            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(categories));
        }
    }
}
