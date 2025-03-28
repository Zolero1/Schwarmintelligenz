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
        throw new NotImplementedException();
        
        //update current position 
        // aggregation towards goal
    }

    public Point GetStartingPosition()
    {
        throw new NotImplementedException();
    }

    public void Subscribe() // wird am anfang schon gemacht wenn die drone erstellt wird, sie subsribt der queue
    {
        throw new NotImplementedException();
    }

    public void HandleEvent() //saves the new Message and waits for the Handler
    {
        throw new NotImplementedException();
    }
}