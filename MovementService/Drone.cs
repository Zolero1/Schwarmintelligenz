using System.Text;
using System.Text.Json;
using GlobalUsings;

namespace MovementService;

public class Drone
{
    public Point CurrentPosition { get; set; }
    public Point GoalPosition { get; set; }
    
    private RabbitMqSubscriber _subscriber;
    
    public string Name{ get; private set; } = "Drone";
    
    private readonly HttpClient _httpClient = new HttpClient();
    

    public Drone()
    {
        
    }
    public Drone(string name)
    {
        Name = name;
        // finds a place to spawn
    }

    // TODO PANI darf man den Service hier so rein knallen
    public async Task Initialize()
    {
        Subscribe();
        CurrentPosition = await GetStartingPosition();
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await MoveAsync();
                await Task.Delay(1000); // Avoid CPU overuse
            }
        });
    }
    public async Task MoveAsync()
    {
        // Request available positions from API
        var availablePositions = await GetAvailablePositionsAsync();
        if (availablePositions == null || availablePositions.Count == 0)
            return;
        
        // Find the best next position
        CurrentPosition = FindClosestPoint(GoalPosition, availablePositions);
        
        Console.WriteLine($"[Drone {Name}] Moved to: {CurrentPosition}");
        
        //schauen wo was frei ist 
        List<Point> pointsList = await GetAvailablePositionsAsync();
        pointsList = pointsList.OrderBy(p => DistanceTo(p, GoalPosition)).ToList();
        List<Point> sealevelList = await GetMapSealevelsAsync(); //alle seh höhen
         
        Point? newPoint = pointsList.SkipWhile(p => p.z <= sealevelList.First(s => s.x == p.x && s.y == p.y).z).First();

        if (newPoint is not null)
        {
            UpdateLocationDto updateLocation = new UpdateLocationDto()
            {
                OldPosition = CurrentPosition,  // Assign the existing position
                NewPosition = newPoint         // Assign the new point
            };


            string json = JsonSerializer.Serialize(updateLocation);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PutAsync("http://localhost:5150/dynamicmap/point", content);
        
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Location updated successfully!");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        } // wenn man nirgends hin kann weil alles voll ist 
        else
        {
            Task.Delay(5000).Wait();
        }
    }
    
    

    private async Task<List<Point>> GetAvailablePositionsAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("http://localhost:5150/dynamicmap/freepoints");
            return JsonSerializer.Deserialize<List<Point>>(response) ?? new List<Point>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Drone {Name}] Error fetching available positions: {ex.Message}");
            return new List<Point>();
        }
    }
    
    private async Task<List<Point>> GetMapSealevelsAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("http://localhost:5117/map/sealevels");
            return JsonSerializer.Deserialize<List<Point>>(response) ?? new List<Point>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Drone {Name}] Error fetching available positions: {ex.Message}");
            return new List<Point>();
        }
    }

        
        
    

    public async Task<Point> GetStartingPosition()
    {
        Point startingPosition = new Point();
        int direction = 0;
        int round = 0;
        int x = 100, y = 100;
        List<Point> areaPoints = new List<Point>();
        while (startingPosition.x ==0 && startingPosition.y == 0 && startingPosition.z ==0)
        {
            switch (direction)
            {
                case 0: y+=round; break;    // N
                case 1: x+=round; y+=round; break; // NE
                case 2: x+=round; break;    // E
                case 3: x+=round; y-=round; break; // SE
                case 4: y-=round; break;    // S
                case 5: x-=round; y-=round; break; // SW
                case 6: x-=round; break;    // W
                case 7: x-=round; y+=round; break; // NW
                default: throw new ArgumentOutOfRangeException();
            }
            var response = await _httpClient.GetAsync($"http://localhost:5117/map/sealevel?x={x}&y={y}");
            string jsonResponse = await response.Content.ReadAsStringAsync();
            areaPoints = JsonSerializer.Deserialize<List<Point>>(jsonResponse);
            
            foreach (Point point in areaPoints)
            {
                var resp = await _httpClient.GetAsync($"http://localhost:5150/dynamicmap/free?x={point.x}&y={point.y}&z={point.z+1}");
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

    public void Subscribe() // wird am anfang schon gemacht wenn die drone erstellt wird, sie subsribt der queue
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
    
    //TODO PANI Wo soll man distance to hin tun? Point oder Drone? Wäre ja dumm wenn der punkt rausfinden soll wie weiter er zum kollegen hat
    
    public double DistanceTo(Point other, Point goal)
    {
        return Math.Sqrt(
            Math.Pow(goal.x - other.x, 2) +
            Math.Pow(goal.y - other.y, 2) +
            Math.Pow(goal.z - other.z, 2)
        );
    }
    
    
    public Point FindClosestPoint(Point goal, List<Point> points)
    {
        return points
            .Select(p => new { Point = p, Distance = DistanceTo(p, goal) }) // Create an anonymous type with point & distance
            .OrderBy(p => p.Distance) // Sort by distance (ascending)
            .First().Point; // Select the closest point
    }
}