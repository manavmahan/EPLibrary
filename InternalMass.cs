using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class InternalMass
    {
        public string name, construction;
        public Zone zone;
        public double area;
        public bool IsWall = false;
        public InternalMass() { }
        public InternalMass(Zone z, double area, string construction, bool IsWall)
        {
            this.construction = construction; this.area = area; zone = z;
            name = zone.Name + ":InternalMass:" + zone.iMasses.Count + 1;
            this.IsWall = IsWall;
            zone.iMasses.Add(this);
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "InternalMass,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(construction, "Construction Name"),
                Utility.IDFLineFormatter(zone.Name, "Zone Name"),
                Utility.IDFLastLineFormatter(area, "Area")
            };
        }
    }
}
