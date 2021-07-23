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
        public string ZoneName;
        public float PartControlled;
        public float Illuminance;
        public DayLightReferencePoint() { }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>()
            {
                "Daylighting:ReferencePoint,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(ZoneName, "Zone Name"),
                Utility.IDFLineFormatter(Point, "XYZ of Point")
            };
            return info.ReplaceLastComma();
        }

    }
}
