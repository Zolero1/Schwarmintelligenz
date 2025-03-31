using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using GlobalUsings;

namespace DynamicMapService
{
    public class DynamicMapController : ControllerBase
    {
        //TODO Schauen wie wair die ganzen Endpoints nennen, weil die Namen gehen so gar nicht
        private readonly DynamicMap _dynamicMap;
                

        // Dependency Injection for DynamicMap (Singleton)
        public DynamicMapController(DynamicMap dynamicMap)
        {
            _dynamicMap = dynamicMap;
        }


        [HttpGet("/dynamicmap/freepoints")]
        public IActionResult GetFreePoints([FromQuery] int x, [FromQuery] int y, [FromQuery] int z)
        {
            // Validate bounds
            if (x < 0 || x >= 200 || y < 0 || y >= 200)
                return BadRequest("Coordinates are out of bounds.");

            List<Point> surroundingPoints = new List<Point>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dz = -1; dz <= 1; dz++) // Add z-axis loop
                    {
                        int newX = x + dx;
                        int newY = y + dy;
                        int newZ = z + dz;

                        if (newX >= 0 && newX < 200 &&
                            newY >= 0 && newY < 200 &&
                            newZ >= 0 && newZ < 200) // Ensure within bounds
                        {
                            if (_dynamicMap.Map[newX, newY].Contains(newZ))
                            {
                                surroundingPoints.Add(new Point() { x = newX, y = newY, z = newZ });
                            }
                        }
                    }
                }
            }


            return Ok(surroundingPoints);
        }

        [HttpPut("/dynamicmap/points")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationDto updateLocation)
        {
            // Validate new and old position coordinates
            if (updateLocation.NewPosition.x < 0 || updateLocation.NewPosition.x >= 200 ||
                updateLocation.NewPosition.y < 0 || updateLocation.NewPosition.y >= 200 ||
                updateLocation.OldPosition.x < 0 || updateLocation.OldPosition.x >= 200 ||
                updateLocation.OldPosition.y < 0 || updateLocation.OldPosition.y >= 200)
            {
                return BadRequest("Coordinates are out of bounds.");
            }


            // Remove from the old position if it exists
           if (_dynamicMap.Map[updateLocation.OldPosition.x, updateLocation.OldPosition.y].Contains(updateLocation.OldPosition.z)) //remove old position
            {
                _dynamicMap.Map[updateLocation.OldPosition.x, updateLocation.OldPosition.y].Remove(updateLocation.OldPosition.z);
            }

            // Add 'z' to the new position
            _dynamicMap.Map[updateLocation.NewPosition.x, updateLocation.NewPosition.y].Add(updateLocation.NewPosition.z);
            return Ok();
        }

        [HttpGet("/dynamicmap/freepoints/isfree")]
        public async Task<ActionResult<bool>> GetFreeLocationAsync([FromQuery] int x, [FromQuery] int y, [FromQuery] int z)
        {
            // Check bounds
            if (x < 0 || x >= 200 || y < 0 || y >= 200)
            {
                return BadRequest("Coordinates are out of bounds.");
            }

            return await Task.Run(() =>
            {
                // Get the list of items at the specified location
                var locationList = _dynamicMap.Map[x, y];

                // Return true if the location is free, otherwise false
                return Ok(!locationList.Contains(z));
            });
        }

    }
}
