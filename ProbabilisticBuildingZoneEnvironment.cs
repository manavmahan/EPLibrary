using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticBuildingZoneEnvironment
    {
        public string Name = "Building";

        public ProbabilityDistributionFunction HeatingSetPoint, CoolingSetPoint;
        public ProbabilisticBuildingZoneEnvironment() { }
        public ProbabilisticBuildingZoneEnvironment(ProbabilityDistributionFunction HeatingSetPoint, ProbabilityDistributionFunction CoolingSetPoint)
        {
            this.HeatingSetPoint = HeatingSetPoint;
            this.CoolingSetPoint = CoolingSetPoint;
        }
        public BuildingZoneEnvironment GetAverage()
        {
            return new BuildingZoneEnvironment(this.HeatingSetPoint.Mean, this.CoolingSetPoint.Mean) 
            { Name = Name }; 
        }
        public string Header(string sep)
        {
            return GetAverage().Header(sep);
        }
        public string ToString(string sep)
        {
            return string.Join(sep, HeatingSetPoint, CoolingSetPoint);
        }
        
    }
}
