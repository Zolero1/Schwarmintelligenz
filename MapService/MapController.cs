using Microsoft.AspNetCore.Mvc;

namespace MapService;

[Controller]
public class MapController : ControllerBase
{
    [HttpGet("/staticValue")]
    public IActionResult GetStaticValue([FromQuery]int x,[FromQuery]int y,[FromQuery]int z)
    {
        
        return null; //returns a list of all hights around 
    }
}