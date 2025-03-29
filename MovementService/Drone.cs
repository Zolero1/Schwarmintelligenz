namespace MovementService;

public class Drone
{
    public Point CurrentPosition { get; set; }
    public Point GoalPosition { get; set; }
    
    private RabbitMqSubscriber _subscriber;
    
    public string Name{ get; private set; } = "Drone";

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
    
    public void Move() //Drone bewegen
    {
        //schauen wo was frei ist 
        
        
        //schauen was davon am ehesten zum ziel führt 
        
        
        //update current position 
        // aggregation towards goal
    }

    public Point GetStartingPosition()
    {
        throw new NotImplementedException();
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

    public void HandleEvent() //saves the new Message GoalPosition when the rabbit triggers
    {
        
    }
    
    
    
    /*public Point FindClosestPoint(Point goal, List<Point> points)
    {
        return points.OrderBy(p => p.DistanceTo(goal)).First();
    }*/
}