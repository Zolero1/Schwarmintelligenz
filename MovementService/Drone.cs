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
    
    private static readonly HttpClient _httpClient = new HttpClient();
    

    public Drone()
    {
        Subscribe();
        //GetStartingPosition();
        // finds a place to spawn
    }
    public Drone(string name)
    {
        
        Name = name;
        Subscribe();
        //GetStartingPosition();
        // finds a place to spawn
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
         
        Point? newPoint = pointsList.SkipWhile(p => p.Z <= sealevelList.First(s => s.X == p.X && s.Y == p.Y).Z).First();

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

        
        
    

    public Point GetStartingPosition()
    {
        throw new NotImplementedException();
    }

    public void Subscribe() // wird am anfang schon gemacht wenn die drone erstellt wird, sie subsribt der queue
    {
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
    
    
    //TODO Wo soll man distance to hin tun? Point oder Drone? Wäre ja dumm wenn der punkt rausfinden soll wie weiter er zum kollegen hat
    
    public double DistanceTo(Point other, Point goal)
    {
        return Math.Sqrt(
            Math.Pow(goal.X - other.X, 2) +
            Math.Pow(goal.Y - other.Y, 2) +
            Math.Pow(goal.Z - other.Z, 2)
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