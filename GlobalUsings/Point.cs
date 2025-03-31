
namespace GlobalUsings;

public class Point
{
    public int x { get; set; }
    public int y { get; set; }
    public int z { get; set; }
    
    public double DistanceTo(Point other, Point goal)
    {
        return Math.Sqrt(
            Math.Pow(goal.x - other.x, 2) +
            Math.Pow(goal.y - other.y, 2) +
            Math.Pow(goal.z - other.z, 2)
        );
    }

    public override string ToString() => $"X:{x}, Y:{y}, Z:{z}";
    
        
    public double DistanceTo(Point goal)
    {
        return Math.Sqrt(
            Math.Pow(goal.x - this.x, 2) +
            Math.Pow(goal.y - this.y, 2) +
            Math.Pow(goal.z - this.z, 2)
        );
    }
    
}