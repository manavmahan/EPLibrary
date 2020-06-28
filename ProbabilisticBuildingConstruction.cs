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
            UWall = new ProbabilityDistributionFunction(), 
            UGFloor = new ProbabilityDistributionFunction(), 
            URoof = new ProbabilityDistributionFunction(), 
            UIFloor = new ProbabilityDistributionFunction(), 
            UIWall = new ProbabilityDistributionFunction(), 
            UWindow = new ProbabilityDistributionFunction(), 
            GWindow = new ProbabilityDistributionFunction(), 
            HCSlab = new ProbabilityDistributionFunction(), 
            Infiltration = new ProbabilityDistributionFunction(), 
            InternalMass = new ProbabilityDistributionFunction();
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
                HCSlab, Infiltration, InternalMass);
        }
    }
}
