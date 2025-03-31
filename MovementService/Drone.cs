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

    private static readonly HttpClient _httpClient = new HttpClient();


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

        var newPoint =
            pointsList.FirstOrDefault(p => p.z > sealevelList.FirstOrDefault(s => s.x == p.x && s.y == p.y)?.z);
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





    public async Task<Point> GetStartingPosition()
    {
        Point startingPosition = new Point();
        int direction = 0;
        int round = 0;
        int x = 100, y = 100;
        List<Point> areaPoints = new List<Point>();
        while (startingPosition.x == 0 && startingPosition.y == 0 && startingPosition.z == 0)
        {
            switch (direction)
            {
                case 0: y += round; break; // N
                case 1:
                    x += round;
                    y += round;
                    break; // NE
                case 2: x += round; break; // E
                case 3:
                    x += round;
                    y -= round;
                    break; // SE
                case 4: y -= round; break; // S
                case 5:
                    x -= round;
                    y -= round;
                    break; // SW
                case 6: x -= round; break; // W
                case 7:
                    x -= round;
                    y += round;
                    break; // NW
                default: throw new ArgumentOutOfRangeException();
            }

            var response = await _httpClient.GetAsync($"http://localhost:5117/static/sealevel?x={x}&y={y}");
            string jsonResponse = await response.Content.ReadAsStringAsync();
            areaPoints = JsonSerializer.Deserialize<List<Point>>(jsonResponse);

            foreach (Point point in areaPoints)
            {
                var resp = await _httpClient.GetAsync(
                    $"http://localhost:5150/location/free/?x={point.x}&y={point.y}&z={point.z + 1}");
                if (resp.IsSuccessStatusCode)
                {
                    return point;
                }
            }

            round++;
            x = 100;
            y = 100;
            direction = (direction + 1) % 7;
        }

        return startingPosition;
    }

    private static (int x, int y) GetNextCoordinates(int x, int y, int direction, int round) => direction switch
    {

        {
            0 => (x, y + round), // N
        1 => (x + round, y + round), // NE
        2 => (x + round, y), // E
        3 => (x + round, y - round), // SE
        4 => (x, y - round), // S
        5 => (x - round, y - round), // SW
        6 => (x - round, y), // W
        7 => (x - round, y + round), // NW
        _ => throw new ArgumentOutOfRangeException()
    };
}
    
public void Subscribe()
{

//TODO MATTHI 
        _subscriber = new RabbitMqSubscriber(((sender, point) =>
        {
            try
            {
                GoalPosition = point;
                Console.WriteLine($"[Drone {Name}] has [{point.ToString()}] as New goal: {GoalPosition}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Drone {Name}] has[{point.ToString()}] Error parsing position: {ex.Message}");
            }
        }));
        _subscriber.StartListening();
        
    }
}
