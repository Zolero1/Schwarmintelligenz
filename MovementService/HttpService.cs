using System.Text;
using System.Text.Json;
using GlobalUsings;

public class HttpService
{
    private readonly HttpClient _httpClient = new HttpClient();
    
    public async Task<List<Point>> GetAvailablePositionsAsync()
    {
        return await GetAsync<List<Point>>("http://localhost:5150/dynamicmap/freepoints") ?? new();
    }
    
    public async Task<List<Point>> GetMapSealevelsAsync()
    {
        return await GetAsync<List<Point>>("http://localhost:5117/map/sealevels") ?? new();
    }
    
    public async Task<List<Point>> GetSealevelAtAsync(int x, int y)
    {
        return await GetAsync<List<Point>>($"http://localhost:5117/map/sealevel?x={x}&y={y}") ?? new();
    }
    
    public async Task<bool> IsPositionFreeAsync(int x, int y, int z)
    {
        return await GetAsync<bool>($"http://localhost:5150/dynamicmap/free?x={x}&y={y}&z={z}");
    }
    
    public async Task UpdateLocationAsync(UpdateLocationDto updateLocation)
    {
        string json = JsonSerializer.Serialize(updateLocation);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync("http://localhost:5150/dynamicmap/point", content);
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error updating location: {response.StatusCode}");
        }
    }
    
    private async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(url);
            return JsonSerializer.Deserialize<T>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data from {url}: {ex.Message}");
            return default;
        }
    }
}