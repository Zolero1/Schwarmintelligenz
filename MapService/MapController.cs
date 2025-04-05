using GlobalUsings;
using Microsoft.AspNetCore.Mvc;

namespace MapService;

[ApiController]
[Route("[controller]")]
public class MapController : ControllerBase
{
    private readonly Map<int> _map;

    public MapController(Map<int> map)
    {
        _map = map;
    }
    
    //TODO Alles async machen + rückgabe param 
    // TODO MATTHI anschauen 
    [HttpGet("/map/sealevels")]
    public async Task<IActionResult> GetSeaLevelAround([FromQuery] int x, [FromQuery] int y)
    {
        // Validate bounds
        if (x < 0 || x >= 200 || y < 0 || y >= 200)
            return BadRequest("Coordinates are out of bounds.");

        List<Point> surroundingPoints = new List<Point>();

        // Get the 8 surrounding points (and the center)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int newX = x + dx;
                int newY = y + dy;

                if (newX >= 0 && newX < 200 && newY >= 0 && newY < 200) // Ensure within bounds
                {
                    int seaLevel = _map.MapArray[newX, newY];
                    Console.WriteLine(seaLevel);
                    surroundingPoints.Add(new Point(){x = newX,y = newY,z =  seaLevel});
                }
            }
        }

        return Ok(surroundingPoints);
    }
    
    [HttpGet("/map/sealevel/")]
    public async Task<IActionResult> GetSeaLevel([FromQuery] int x, [FromQuery] int y)
    {
        // Validate bounds
        if (x < 0 || x >= 200 || y < 0 || y >= 200)
            return BadRequest("Coordinates are out of bounds.");
        int seaLevel = _map.MapArray[x,y];

        return Ok(seaLevel);
    }
}