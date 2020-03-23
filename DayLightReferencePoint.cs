using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class DayLightReferencePoint
    {
        public string Name;
        public XYZ Point;
        public Zone Zone;
        public double PartControlled;
        public double Illuminance;
        public DayLightReferencePoint() { }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>()
            {
                "Daylighting:ReferencePoint,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(Zone.Name, "Zone Name"),
                Utility.IDFLineFormatter(Point, "XYZ of Point")
            };
            return info.ReplaceLastComma();
        }

    }
}
