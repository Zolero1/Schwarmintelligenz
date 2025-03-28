using MovementService;

namespace DynamicMapService;

public record updateLocationDto
{
    public Point newPosition { get; set; }
    
    public Point oldPosition { get; set; }
}