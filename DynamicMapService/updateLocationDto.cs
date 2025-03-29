using GlobalUsings;
using MovementService;

namespace DynamicMapService;

public record UpdateLocationDto
{
    public required Point NewPosition { get; set; }
    
    public required Point OldPosition { get; set; }
}