using GlobalUsings;

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
    public void Move() //Drone bewegen //immer ausführen 
    {
        //schauen wo was frei ist 
        //call request an dynamic 
        // call an static ->
        // schauen dass wir über seehöhe fliegen und nicht in den felsen einfahren
        // die erde ist ja kein brett
        
        
        //schauen was davon am ehesten zum ziel führt 
        
        // wenn gar nix weiter hilft mal bissl warten 
        
        
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