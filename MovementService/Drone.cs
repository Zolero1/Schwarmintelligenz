using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CentralService;
using GlobalUsings;

namespace MovementService;

public class Drone
{
    public Point CurrentPosition { get; set; } = new Point(){x=100, y=100, z=70};
    public Point? GoalPosition { get; set; }
    
    private RabbitMqSubscriber _subscriber;
    
    private RabbitQueueSender _sender;
    
    private bool goalReached = false;
    
    public string Name{ get; private set; } = "Drone";
    
    private static readonly HttpClient _httpClient = new HttpClient();
    

    public Drone()
    {
        _sender = new RabbitQueueSender();
    }
    public Drone(string name)
    {
        Name = name;
        _sender = new RabbitQueueSender();
        // finds a place to spawn
    }

    public async Task Initialize()
    {
        Subscribe();
        CurrentPosition = await GetStartingPosition();
        await MoveAsync();

    }
    public async Task MoveAsync()
    {
        Console.WriteLine("Drone moving");
        while (true)
        {
            if (GoalPosition != null && goalReached == false)
            {
                //schauen wo was frei ist 
                List<Point?> pointsList = await GetAvailablePositionsAsync();
                List<Point> sealevelList = await GetMapSealevelsAsync(); //alle seh höhen
                
                pointsList = pointsList
                    .Where(p => sealevelList.Any(s => s.x == p.x && s.y == p.y && s.z < p.z))
                    .ToList();             
                pointsList = pointsList.OrderBy(p => DistanceTo(p, GoalPosition)).ToList();
                Point? newPoint = pointsList[0]; //= pointsList.SkipWhile(p => p.z <= sealevelList.First(s => s.x == p.x && s.y == p.y).z)
                    //.FirstOrDefault();

                if (newPoint is not null)
                {
                    UpdateLocationDto updateLocation = new UpdateLocationDto()
                    {
                        OldPosition = CurrentPosition, // Assign the existing position
                        NewPosition = newPoint // Assign the new point
                    };
                    CurrentPosition = newPoint;
                    Console.WriteLine($"[Drone {Name}] Moved to: {CurrentPosition}");


                    string json = JsonSerializer.Serialize(updateLocation);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response =
                        await _httpClient.PutAsync("http://localhost:5150/dynamicmap/point", content);

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
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (goalReached)
                    {
                        break;
                    }
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (goalReached)
                        {
                            break;
                        }
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int newX = newPoint.x + dx;
                            int newY = newPoint.y + dy;
                            int newZ = newPoint.z + dz;

                            if (GoalPosition.x - newX >= -1 && GoalPosition.x - newX <= 1 &&
                                GoalPosition.y - newY >= -1 && GoalPosition.x - newY <= 1 && 
                                GoalPosition.z - newZ <= 1 && GoalPosition.z - newZ >= -1) // Ensure within bounds
                            {
                                _sender.SendToCentral($"[Drone {Name}] Reached: {GoalPosition}");
                                Console.WriteLine($"[Drone {Name}] Reached: {GoalPosition}");
                                goalReached = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private async Task<List<Point>> GetAvailablePositionsAsync()
    {
        Console.WriteLine("Getting available positions");
        try
        {
            string requestUri =
                $"http://localhost:5150/dynamicmap/freepoints?x={CurrentPosition.x}&y={CurrentPosition.y}&z={CurrentPosition.z}";
            var response = await _httpClient.GetAsync(requestUri);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            List<Point> temp  = JsonSerializer.Deserialize<List<Point>>(jsonResponse) ?? new List<Point>();
            return temp;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Drone {Name}] Error fetching available positions: {ex.Message}");
            return new List<Point>();
        }
    }
    
    private async Task<List<Point>> GetMapSealevelsAsync()
    {
        Console.WriteLine("Getting map sealevels");
        try
        {
            var response = await _httpClient.GetStringAsync($"http://localhost:5117/static/sealevel?x={CurrentPosition.x}&y={CurrentPosition.y}");
            return JsonSerializer.Deserialize<List<Point>>(response) ?? new List<Point>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Drone {Name}] Error fetching available positions: {ex.Message}");
            return new List<Point>();
        }
        
    }
    
    private async Task<Point> GetStartingPosition()
    {
        Console.WriteLine("Getting start position");
        Point startingPosition = new Point();
        int direction = 0;
        int round = 1;
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
            var response = await _httpClient.GetAsync($"http://localhost:5117/static/sealevel?x={x}&y={y}");
            string jsonResponse = await response.Content.ReadAsStringAsync();
            areaPoints = JsonSerializer.Deserialize<List<Point>>(jsonResponse);
            
            foreach (Point point in areaPoints)
            {
                var resp = await _httpClient.GetAsync($"http://localhost:5150/location/free/?x={point.x}&y={point.y}&z={point.z+1}");
                if (resp.IsSuccessStatusCode)
                {
                    return point;
                }
                UpdateLocationDto updateLocation = new UpdateLocationDto()
                {
                    OldPosition = new Point(),
                    NewPosition = point
                    
                };

                string json = JsonSerializer.Serialize(updateLocation);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await _httpClient.PutAsync("http://localhost:5150/dynamicmap/point", content);                if (resp.IsSuccessStatusCode)
                
                return point;
                
            }
            
            x = 100;
            y = 100;
            direction = (direction + 1) % 7;
            if (direction == 0)
            {
                round+=3;
            }
        }
        return startingPosition;
    }

    public void Subscribe() // wird am anfang schon gemacht wenn die drone erstellt wird, sie subsribt der queue
    {
        _subscriber = new RabbitMqSubscriber(((sender, point) =>
        {
            try
            {
                GoalPosition = point;
                goalReached = false;
                Console.WriteLine($"[Drone {Name}] has [{point.ToString()}] as New goal: {GoalPosition}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Drone {Name}] has[{point.ToString()}] Error parsing position: {ex.Message}");
            }
        }));
        _subscriber.StartListening();
        
    }
    
    //TODO Wo soll man distance to hin tun? Point oder Drone? Wäre ja dumm wenn der punkt rausfinden soll wie weiter er zum kollegen hat
    
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