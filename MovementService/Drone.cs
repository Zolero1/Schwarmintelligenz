using System.Text;
using System.Text.Json;
using GlobalUsings;

namespace MovementService;

public class Drone
{
    public Point CurrentPosition { get; set; }
    public Point GoalPosition { get; set; }
    
    private RabbitMqSubscriber _subscriber;
    public string Name { get; private set; } = "Drone";
    private readonly HttpService _httpService = new HttpService();
    
    public Drone() {}

    public Drone(string name)
    {
            Name = name;
    }
    
    //TODO MATTHI - was spricht dagegen das in den Konstruktor zu tun
    public async Task Initialize()
    {
        Subscribe();
        CurrentPosition = await GetStartingPositionAsync();
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await MoveAsync();
                await Task.Delay(1000);
            }
        });
    }
    
    public async Task MoveAsync()
    {
        var availablePositions = await _httpService.GetAvailablePositionsAsync();
        if (availablePositions.Count == 0) return;
        
        var sealevelList = await _httpService.GetMapSealevelsAsync();
        var pointsList = availablePositions.OrderBy(p => p.DistanceTo(GoalPosition)).ToList();
        
        var newPoint = pointsList.FirstOrDefault(p => p.z > sealevelList.FirstOrDefault(s => s.x == p.x && s.y == p.y)?.z);
        if (newPoint != null)
        {
            var updateLocation = new UpdateLocationDto { OldPosition = CurrentPosition, NewPosition = newPoint };
            await _httpService.UpdateLocationAsync(updateLocation);
            CurrentPosition = newPoint;
        }
        else
        {
            await Task.Delay(5000);
        }
    }
    
    private async Task<Point> GetStartingPositionAsync()
    {
        int x = 100, y = 100, direction = 0, round = 0;
        while (true)
        {
            (x, y) = GetNextCoordinates(x, y, direction, round);
            var areaPoints = await _httpService.GetSealevelAtAsync(x, y);
            
            foreach (var point in areaPoints)
            {
                if (await _httpService.IsPositionFreeAsync(point.x, point.y, point.z + 1))
                {
                    return point;
                }
            }
            
            round++;
            direction = (direction + 1) % 8;
        }
    }
    
    private static (int x, int y) GetNextCoordinates(int x, int y, int direction, int round) => direction switch
    {
        0 => (x, y + round),    // N
        1 => (x + round, y + round), // NE
        2 => (x + round, y),    // E
        3 => (x + round, y - round), // SE
        4 => (x, y - round),    // S
        5 => (x - round, y - round), // SW
        6 => (x - round, y),    // W
        7 => (x - round, y + round), // NW
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public void Subscribe()
    {
        _subscriber = new RabbitMqSubscriber((sender, point) =>
        {
            GoalPosition = point;
            Console.WriteLine($"[Drone {Name}] New goal: {GoalPosition}");
        });
        _subscriber.StartListening();
    }
}
