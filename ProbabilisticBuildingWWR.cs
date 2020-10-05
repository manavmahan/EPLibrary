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
        public bool EachWallSeparately;
        public BuildingWWR Average;
        public ProbabilityDistributionFunction
            North = new ProbabilityDistributionFunction("WWR (North)", ""),
            East = new ProbabilityDistributionFunction("WWR (East)", ""),
            West = new ProbabilityDistributionFunction("WWR (West)", ""),
            South = new ProbabilityDistributionFunction("WWR (South)", "");
            
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
            return new BuildingWWR(North.Mean, East.Mean, West.Mean, South.Mean) { EachWallSeparately = EachWallSeparately }; 
        }
        public string Header(string sep) 
        {
            return GetAverage().Header( sep);
        }
        public string ToString(string sep)
        {
            return string.Join(sep, North, East, West, South);
        }     
    }
}
