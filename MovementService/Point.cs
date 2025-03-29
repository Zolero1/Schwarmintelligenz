namespace MovementService;
public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    
    public double DistanceTo(Point other, Point goal)
    {
        return Math.Sqrt(
            Math.Pow(goal.X - other.X, 2) +
            Math.Pow(goal.Y - other.Y, 2) +
            Math.Pow(goal.Z - other.Z, 2)
        );
    }

    public override string ToString() => $"X:{X}, Y:{Y}, Z:{Z}";
}