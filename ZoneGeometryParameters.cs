using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    public class ZoneGeometryInformation
    {
        public string Name;
        public double Height;
        public XYZList FloorPoints;
        public Dictionary<XYZ[], string> WallCreationData;
        public ZoneGeometryInformation() { }
    }
}
