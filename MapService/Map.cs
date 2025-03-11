namespace MapService;

public class Map
{
    public Place[,] MapArray { get; set; } = new Place[200, 200];
    
    
    public Map()
    {
        //read map from csv and save to MapArray
    }
    
    
}