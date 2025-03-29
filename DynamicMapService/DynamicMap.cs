namespace DynamicMapService;

public class DynamicMap
{
    public List<int>[,] Map = new List<int>[200, 200];


    public DynamicMap()
    {
        for (int i = 0; i < 200; i++)
        {
            for (int j = 0; j < 200; j++)
            {
                Map[i, j] = new List<int>();  // Initialize each list.
            }
        }
    }

}
