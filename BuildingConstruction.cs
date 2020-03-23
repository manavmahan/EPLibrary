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
        public double uWall, uGFloor, uRoof, uIFloor, uIWall, uWindow, gWindow, hcSlab, infiltration;
        public double hcWall, hcRoof, hcGFloor, hcIFloor, hcIWall;
        public BuildingConstruction() { }
        public string ToCSVString()
        {
            return string.Join(",", uWall, uGFloor, uRoof, uIFloor, uIWall, uWindow, gWindow, hcSlab, infiltration);
        }
        public string Header()
        {
            return string.Join(",", "u_Wall", "u_GFloor", "u_Roof", "u_IFloor", "u_IWall", "u_Window", "g_Window", "hc_Slab", "Infiltration");
        }
    }
}
