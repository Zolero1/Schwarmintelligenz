namespace MovementService;

public class Drone
{
    public Point CurrentPosition { get; set; }
    public Point GoalPosition { get; set; }


    public Drone()
    {
        GetStartingPosition();
        Subscribe();
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
    }

    public void HandleEvent() //saves the new Message GoalPosition when the rabbit triggers
    {
        
    }
    
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
        return points.OrderBy(p => p.DistanceTo(goal)).First();
    }
}