using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
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
        public List<BuildingZoneEnvironment> GetSamples(Random random, int samples)
        {
            List<BuildingZoneEnvironment> vals = new List<BuildingZoneEnvironment>();

            new List<ProbabilityDistributionFunction>()
            {
                 HeatingSetPoint, CoolingSetPoint
            }
            .Select(p => p.GetLHSSamples(random, samples))
            .ZipAll(v => vals.Add(new BuildingZoneEnvironment()
            {
                Name = Name,
                HeatingSetPoint = v.ElementAt(0),
                CoolingSetPoint = v.ElementAt(1),
            }));
            return vals;
        }
    }
}
