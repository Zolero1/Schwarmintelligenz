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
    // TODO MATTHI anschauen -> finds unnötig
    [HttpGet("/map/sealevels")]
    public async Task<ActionResult<IEnumerable<Point>>> GetSeaLevelAround([FromQuery] int x, [FromQuery] int y)
    {
        if (x < 0 || x >= 200 || y < 0 || y >= 200)
            return BadRequest("Coordinates are out of bounds.");

        var surroundingPoints = new List<Point>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int newX = x + dx;
                int newY = y + dy;

                if (newX >= 0 && newX < 200 && newY >= 0 && newY < 200)
                {
                    int seaLevel = _map.MapArray[newX, newY];
                    surroundingPoints.Add(new Point { x = newX, y = newY, z = seaLevel });
                }
            }
        }

        return Ok(surroundingPoints);
    }
}