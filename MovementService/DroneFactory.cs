namespace MovementService;

public class DroneFactory
{
    private HttpClient httpClient;

    public DroneFactory()
    {
        httpClient = new HttpClient();
    }

    public async Task CreateDrone(string droneName)
    {
        Drone drone = new Drone(droneName);
        await drone.Initialize();
    }
}