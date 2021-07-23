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
        
        public float area;
        public bool IsWall = false;
        public string ZoneName;
        public InternalMass() { }
        public InternalMass(Zone z, float area, string construction, bool IsWall)
        {
            this.construction = construction; this.area = area; ZoneName = z.Name;
            name = z.Name + ":InternalMass:" + z.iMasses.Count + 1;
            this.IsWall = IsWall;
            z.iMasses.Add(this);
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "InternalMass,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(construction, "Construction Name"),
                Utility.IDFLineFormatter(ZoneName, "Zone Name"),
                Utility.IDFLastLineFormatter(area, "Area")
            };
        }
    }
}
