using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    public class ProbabilisticBuildingService
    {
        public ProbabilityDistributionFunction 
            BoilerEfficiency = new ProbabilityDistributionFunction(),
            ChillerCOP = new ProbabilityDistributionFunction();
        public ProbabilisticBuildingService() { }
        public string ToString(string sep)
        {
            return string.Join(sep, BoilerEfficiency, ChillerCOP);
        }
        public BuildingService GetAverage()
        {
            return new BuildingService(BoilerEfficiency.Mean, ChillerCOP.Mean);
        }
        public string Header(string sep)
        {
            return GetAverage().Header(sep);
        }
        public List<BuildingService> GetSamples(Random random, int samples)
        {
            List<BuildingService> vals = new List<BuildingService>();

            new List<ProbabilityDistributionFunction>()
            {
                BoilerEfficiency, ChillerCOP
            }
            .Select(p => p.GetLHSSamples(random, samples))
            .ZipAll(v => vals.Add(new BuildingService()
            {
                BoilerEfficiency = v.ElementAt(0),
                ChillerCOP = v.ElementAt(1),                
            }));
            return vals;
        }
    }
}
