using MovementService;

namespace CentralService;

public static class DroneManager
{
    public static List<Drone> Drones { get;} = new List<Drone>();

    public static async Task CreateDrone()
    {
        Drone drone = new Drone();
        await drone.Initialize();
        Drones.Add(drone);
    }
}
