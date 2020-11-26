using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticBuildingConstruction
    {
        public ProbabilityDistributionFunction 
            UWall = new ProbabilityDistributionFunction("u-Value (Wall)","W/m\u00b2K"), 
            UGFloor = new ProbabilityDistributionFunction("u-Value (Ground Floor)", "W/m\u00b2K"),
            URoof = new ProbabilityDistributionFunction("u-Value (Roof)", "W/m\u00b2K"),
            UIFloor = new ProbabilityDistributionFunction("u-Value (Internal Floor)", "W/m\u00b2K"),
            UIWall = new ProbabilityDistributionFunction("u-Value (Internal Wall)", "W/m\u00b2K"),
            UWindow = new ProbabilityDistributionFunction("u-Value (Windows)", "W/m\u00b2K"),
            GWindow = new ProbabilityDistributionFunction("g-Value (Windows)",""), 
            HCSlab = new ProbabilityDistributionFunction("Heat Capacity (Floor Slabs)","J/kgK"), 
            Infiltration = new ProbabilityDistributionFunction("Infiltration", "ACH"),
            Permeability=new ProbabilityDistributionFunction("Permeability", "m\u00b3/(h·m\u00b2)"),
            InternalMass = new ProbabilityDistributionFunction("Internal Mass", "kJ/m\u00b2K");
        public ProbabilisticBuildingConstruction() { }
        public ProbabilisticBuildingConstruction(
            ProbabilityDistributionFunction UWall, 
            ProbabilityDistributionFunction UGFloor, 
            ProbabilityDistributionFunction URoof, 
            ProbabilityDistributionFunction UIFloor, 
            ProbabilityDistributionFunction UIWall, 
            ProbabilityDistributionFunction UWindow, 
            ProbabilityDistributionFunction GWindow, 
            ProbabilityDistributionFunction HCSlab, 
            ProbabilityDistributionFunction InternalMass)
        {
            this.UWall = UWall;
            this.UGFloor = UGFloor;
            this.URoof = URoof;
            this.UIFloor = UIFloor;
            this.UIWall = UIWall;
            this.UWindow = UWindow;
            this.GWindow = GWindow;
            this.HCSlab = HCSlab;
            this.InternalMass = InternalMass;         
        }
        public BuildingConstruction GetAverage()
        {
            return new BuildingConstruction()
            {
                UWall = UWall.Mean,
                UGFloor = UGFloor.Mean,
                URoof = URoof.Mean,
                UIFloor = UIFloor.Mean,
                UIWall = UIWall.Mean,
                UWindow = UWindow.Mean,
                GWindow = GWindow.Mean,
                HCSlab = HCSlab.Mean,
                Infiltration = Infiltration.Mean,
                Permeability = Permeability.Mean,
                InternalMass = InternalMass.Mean
            };
        }
        public string Header(string sep)
        {
            return GetAverage().Header(sep); 
        }
        public string ToString(string sep)
        {
            return string.Join(sep, UWall, UGFloor, URoof, UIFloor, UIWall, UWindow, GWindow, 
                HCSlab, Infiltration, Permeability, InternalMass);
        }
    }
}
