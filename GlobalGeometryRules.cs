using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class GlobalGeometryRules
    {
        public string startingVertexPosition { get; set; }
        public string vertexEntryDirection { get; set; }
        public string coordinateSystem { get; set; }
        public string daylightingRefPointCoordSyst { get; set; }
        public string rectSurfaceCoordSyst { get; set; }

        public GlobalGeometryRules()
        {
            startingVertexPosition = "UpperLeftCorner";
            vertexEntryDirection = "Counterclockwise";
            coordinateSystem = "Relative";
            daylightingRefPointCoordSyst = "Relative";
            rectSurfaceCoordSyst = "Relative";
        }

        internal List<string> WriteInfo()
        {
            return new List<string>()
            {
                "GlobalGeometryRules,",
                Utility.IDFLineFormatter(startingVertexPosition, "Starting Vertex Position"),
                Utility.IDFLineFormatter(vertexEntryDirection, "Vertex Entry Direction"),
                Utility.IDFLineFormatter(coordinateSystem, "Coordinate System"),
                Utility.IDFLineFormatter(daylightingRefPointCoordSyst, "Daylighting Reference Point Coordinate System"),
                Utility.IDFLastLineFormatter(rectSurfaceCoordSyst, "Rectangular Surface Coordinate System")
            };
        }
    }
}
