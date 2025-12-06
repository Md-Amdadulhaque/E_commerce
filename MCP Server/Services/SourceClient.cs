using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace MCP_Server.Services
{
    public class SourceClient
    {
        private readonly HttpClient _httpClient;
        public SourceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task<T?> GetDataByModelAsync<T>(string endpoint,object data)
        {
            var query = string.Join("&", data.GetType().GetProperties()
           .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(data)?.ToString() ?? "")}"));

            // Call GET endpoint with query string
            var response = await _httpClient.GetAsync($"{endpoint}?{query}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}
