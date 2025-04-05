using System.Text;
using System.Text.Json;
using GlobalUsings;

namespace MovementService
{

    public class HttpService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<List<Point>> GetAvailablePositionsAsync(int x, int y, int z)
        {
            return await GetAsync<List<Point>>($"http://localhost:5150/dynamicmap/freepoints?x={x}&y={y}&z={z}") ?? new();
        }

        public async Task<List<Point>> GetMapSealevelsAsync(int x, int y)
        {
            return await GetAsync<List<Point>>($"http://localhost:5117/map/sealevels?x={x}&y={y}") ?? new();
        }

        public async Task<List<Point>> GetSealevelAtAsync(int x, int y)
        {
            return await GetAsync<List<Point>>($"http://localhost:5117/map/sealevel?x={x}&y={y}") ?? new();
        }

        public async Task<bool> IsPositionFreeAsync(int x, int y, int z)
        {
            return await GetAsync<bool>($"http://localhost:5150/dynamicmap/freepoints/isfree?x={x}&y={y}&z={z}");
        }

        public async Task UpdateLocationAsync(UpdateLocationDto updateLocation)
        {
            string json = JsonSerializer.Serialize(updateLocation);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("http://localhost:5150/dynamicmap/points", content);

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
}