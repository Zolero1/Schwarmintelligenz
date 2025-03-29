using GlobalUsings;


public record UpdateLocationDto
{
    public required Point NewPosition { get; set; }
    
    public required Point OldPosition { get; set; }
}