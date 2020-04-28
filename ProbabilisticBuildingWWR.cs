using DocumentFormat.OpenXml.Presentation;
using IronPython.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticBuildingWWR
    {
        public BuildingWWR Average;
        public ProbabilityDistributionFunction 
            North = new ProbabilityDistributionFunction(),
            East = new ProbabilityDistributionFunction(),
            West = new ProbabilityDistributionFunction(),
            South = new ProbabilityDistributionFunction();
        public ProbabilisticBuildingWWR() { }
        public ProbabilisticBuildingWWR(
            ProbabilityDistributionFunction north,
            ProbabilityDistributionFunction east,
            ProbabilityDistributionFunction west,
            ProbabilityDistributionFunction south)
        {
            North = north; East = east; West = west; South = south;
        }
        public BuildingWWR GetAverage()
        {
            return new BuildingWWR(North.Mean, East.Mean, West.Mean, South.Mean); 
        }
        public string Header(string sep) 
        {
            return GetAverage().Header( sep);
        }
        public string ToString(string sep)
        {
            return string.Join(sep, North, East, West, South);
        }
        public List<BuildingWWR> GetSamples(Random random, int samples)
        {
            List<BuildingWWR> vals = new List<BuildingWWR>();

            new List<ProbabilityDistributionFunction>()
            {
                North, East, West, South
            }
            .Select(p => p.GetLHSSamples(random, samples))
            .ZipAll (v => vals.Add(new BuildingWWR(
                v.ElementAt(0), 
                v.ElementAt(1), 
                v.ElementAt(2), 
                v.ElementAt(3))));
            return vals;
        }
    }
}
