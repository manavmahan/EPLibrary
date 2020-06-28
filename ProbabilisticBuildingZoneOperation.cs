using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticBuildingZoneOperation
    {
        public string Name = "Building";

        public ProbabilityDistributionFunction 
            StartTime = new ProbabilityDistributionFunction(),
            OperatingHours = new ProbabilityDistributionFunction(), 
            LHG = new ProbabilityDistributionFunction(), 
            EHG = new ProbabilityDistributionFunction();
        public ProbabilisticBuildingZoneOperation() { }

        public ProbabilisticBuildingZoneOperation(
            ProbabilityDistributionFunction StartTime,
            ProbabilityDistributionFunction OperatingHours, 
            ProbabilityDistributionFunction LHG, 
            ProbabilityDistributionFunction EHG) 
        {
            this.StartTime = StartTime;
            this.OperatingHours = OperatingHours; 
            this.LHG = LHG; 
            this.EHG = EHG;          
        }
        public BuildingZoneOperation GetAverage()
        {
            return new BuildingZoneOperation(StartTime.Mean, OperatingHours.Mean, LHG.Mean, EHG.Mean)
            { Name = Name };
        }
        public string ToString(string sep)
        {        
            return string.Join(sep, StartTime, OperatingHours, LHG, EHG);
        }
        public string Header(string sep)
        {
            return GetAverage().Header(sep);
        }
       
    }
}
