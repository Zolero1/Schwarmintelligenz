using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DynamicMapService
{
    public class DynamicMapController : Controller
    {
        private readonly DynamicMapService _dynamicMapService;

        // Dependency Injection for DynamicMapService (Singleton)
        public DynamicMapController(DynamicMapService dynamicMapService)
        {
            _dynamicMapService = dynamicMapService;
        }

        [HttpPut("/location")]
        public IActionResult UpdateLocation([FromBody] updateLocationDto updateLocation)
        {
            // Validate new and old position coordinates
            if (updateLocation.newPosition.X < 0 || updateLocation.newPosition.X >= 200 ||
                updateLocation.newPosition.Y < 0 || updateLocation.newPosition.Y >= 200 ||
                updateLocation.oldPosition.X < 0 || updateLocation.oldPosition.X >= 200 ||
                updateLocation.oldPosition.Y < 0 || updateLocation.oldPosition.Y >= 200)
            {
                return BadRequest("Coordinates are out of bounds.");
            }


            // Remove from the old position if it exists
           if (_dynamicMapService.DynamicMap[updateLocation.oldPosition.X, updateLocation.oldPosition.Y].Contains(updateLocation.oldPosition.Z)) //remove old position
            {
                _dynamicMapService.DynamicMap[updateLocation.oldPosition.X, updateLocation.oldPosition.Y].Remove(updateLocation.oldPosition.Z);
            }

            // Add 'z' to the new position
            _dynamicMapService.DynamicMap[updateLocation.newPosition.X, updateLocation.newPosition.Y].Add(updateLocation.newPosition.Z);
            return Ok();
        }

        [HttpGet("/location/free")]
        public IActionResult GetFreeLocation([FromQuery] int x, [FromQuery] int y, [FromQuery] int z)
        {
            // Check bounds
            if (x < 0 || x >= 200 || y < 0 || y >= 200)
            {
                return BadRequest("Coordinates are out of bounds.");
            }

            // Get the list of items at the specified location
            var locationList = _dynamicMapService.DynamicMap[x, y];

            // Check if the list contains 'z'
            if (locationList.Contains(z))
            {
                return Ok("Location is not free.");
            }
            else
            {
                return Ok("Location is free.");
            }
        }
    }
}
