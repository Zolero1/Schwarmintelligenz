using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using GlobalUsings;

namespace DynamicMapService
{
    public class DynamicMapController : Controller
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
                                surroundingPoints.Add(new Point() { X = newX, Y = newY, Z = newZ });
                            }
                        }
                    }
                }
            }


            return Ok(surroundingPoints);
        }

        [HttpPut("/dynamicmap/point")]
        public IActionResult UpdateLocation([FromBody] UpdateLocationDto updateLocation)
        {
            // Validate new and old position coordinates
            if (updateLocation.NewPosition.X < 0 || updateLocation.NewPosition.X >= 200 ||
                updateLocation.NewPosition.Y < 0 || updateLocation.NewPosition.Y >= 200 ||
                updateLocation.OldPosition.X < 0 || updateLocation.OldPosition.X >= 200 ||
                updateLocation.OldPosition.Y < 0 || updateLocation.OldPosition.Y >= 200)
            {
                return BadRequest("Coordinates are out of bounds.");
            }


            // Remove from the old position if it exists
           if (_dynamicMap.Map[updateLocation.OldPosition.X, updateLocation.OldPosition.Y].Contains(updateLocation.OldPosition.Z)) //remove old position
            {
                _dynamicMap.Map[updateLocation.OldPosition.X, updateLocation.OldPosition.Y].Remove(updateLocation.OldPosition.Z);
            }

            // Add 'z' to the new position
            _dynamicMap.Map[updateLocation.NewPosition.X, updateLocation.NewPosition.Y].Add(updateLocation.NewPosition.Z);
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
            var locationList = _dynamicMap.Map[x, y];

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
