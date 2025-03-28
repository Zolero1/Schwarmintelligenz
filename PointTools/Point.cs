namespace MovementService;

public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    
    public double DistanceTo(Point other)
    {
        return Math.Sqrt(
            Math.Pow(this.X - other.X, 2) +
            Math.Pow(this.Y - other.Y, 2) +
            Math.Pow(this.Z - other.Z, 2)
        );
    }
}