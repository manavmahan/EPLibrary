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
        public float UWall, UGFloor, URoof, UIFloor, UIWall, UWindow, GWindow, HCSlab, Infiltration, Permeability, InternalMass, UCWall;
        public float hcWall, hcRoof, hcGFloor, hcIFloor, hcIWall, hcInternalMass;
        public BuildingConstruction() { }
        public  string ToString(string sep)
        {
            return string.Join(sep, UWall, UGFloor, URoof, UIFloor, UIWall, UWindow, GWindow, HCSlab, Infiltration, Permeability, InternalMass);
        }
        public string Header(string sep)
        {
            return string.Join(sep, "u-Value (Wall)", "u-Value (Ground Floor)", "u-Value (Roof)", "u-Value (Internal Floor)",
                "u-Value (Internal Wall)", "u-Value (Windows)", "g-Value (Windows)", "Heat Capacity (Floor Slabs)", "Infiltration", 
                "Permeability", "Internal Mass");
        }
    }
}
