using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class BuildingConstruction
    {
        //To store the values from samples
        public double UWall, UGFloor, URoof, UIFloor, UIWall, UWindow, GWindow, HCSlab, Infiltration, InternalMass, UCWall;
        public double hcWall, hcRoof, hcGFloor, hcIFloor, hcIWall, hcInternalMass;
        public BuildingConstruction() { }
        public  string ToString(string sep)
        {
            return string.Join(sep, UWall, UGFloor, URoof, UIFloor, UIWall, UWindow, GWindow, HCSlab, Infiltration, InternalMass);
        }
        public string Header(string sep)
        {
            return string.Join(sep, "u_Wall", "u_GFloor", "u_Roof", "u_IFloor", "u_IWall", "u_Window", "g_Window", "hc_Slab", "Infiltration", "Internal Mass");
        }
    }
}
