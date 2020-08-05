using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticBuildingService
    {
        public HVACSystem HVACSystem;
        public ProbabilityDistributionFunction 
            BoilerEfficiency = new ProbabilityDistributionFunction(),
            HeatingCOP = new ProbabilityDistributionFunction(),
            CoolingCOP = new ProbabilityDistributionFunction();
        public ProbabilisticBuildingService() { }
        public string ToString(string sep)
        {
            return string.Join(sep, BoilerEfficiency, HeatingCOP, CoolingCOP);
        }
        public BuildingService GetAverage()
        {
            return new BuildingService(BoilerEfficiency.Mean, HeatingCOP.Mean, CoolingCOP.Mean) { HVACSystem = HVACSystem };
        }
        public string Header(string sep)
        {
            return GetAverage().Header(sep);
        }
    }
}
