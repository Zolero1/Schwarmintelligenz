namespace DynamicMapService;

public class DynamicMapService
{
    public List<int>[,] DynamicMap = new List<int>[200, 200];
    
    
    public DynamicMapService()
    {
        for (int i = 0; i < 200; i++)
        {
            for (int j = 0; j < 200; j++)
            {
                DynamicMap[i, j] = new List<int>();  // Initialize each list.
            }
        }
    }

}