namespace MovementService;

public class Drone
{
    public Point CurrentPosition { get; set; }
    public Point GoalPosition { get; set; }


    public Drone()
    {
        Subscribe();
        // finds a place to spawn
    }
    public void Move() //Drone bewegen
    {
        
        
        //update current position 
        // aggregation towards goal
    }

    public void Subscribe() // wird am anfang schon gemacht wenn die drone erstellt wird, sie subsribt der queue
    {
        
    }

    public void HandleEvent() //saves the new Message and waits for the Handler
    {
        
    }
}