using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticZoneConditions
    {
        public string Name = "Building";

        public ProbabilityDistributionFunction
            StartTime = new ProbabilityDistributionFunction() { Mean = 8, VariationOrSD = 0, Distribution = PDF.unif },
            OperatingHours = new ProbabilityDistributionFunction() { Mean = 8, VariationOrSD = 0, Distribution = PDF.unif },
            LHG = new ProbabilityDistributionFunction() { Mean = 6, VariationOrSD = 0, Distribution = PDF.unif },
            EHG = new ProbabilityDistributionFunction() { Mean = 12, VariationOrSD = 0, Distribution = PDF.unif },
            AreaPerPerson = new ProbabilityDistributionFunction() { Mean = 24, VariationOrSD = 0, Distribution = PDF.unif },
            HeatingSetpoint = new ProbabilityDistributionFunction() { Mean = 20, VariationOrSD = 0, Distribution = PDF.unif },
            CoolingSetpoint = new ProbabilityDistributionFunction() { Mean = 25, VariationOrSD = 0, Distribution = PDF.unif };

        public ProbabilisticZoneConditions() { }

        public ProbabilisticZoneConditions(
            ProbabilityDistributionFunction StartTime,
            ProbabilityDistributionFunction OperatingHours, 
            ProbabilityDistributionFunction LHG, 
            ProbabilityDistributionFunction EHG,
            ProbabilityDistributionFunction AreaPerPerson,
            ProbabilityDistributionFunction HeatingSetpoint,
            ProbabilityDistributionFunction CoolingSetpoint) 
        {
            this.StartTime = StartTime;
            this.OperatingHours = OperatingHours; 
            this.LHG = LHG; 
            this.EHG = EHG;
            this.AreaPerPerson = AreaPerPerson;
            this.HeatingSetpoint = HeatingSetpoint;
            this.CoolingSetpoint = CoolingSetpoint;
        }
        public ZoneConditions GetAverage()
        {
            return new ZoneConditions(StartTime.Mean, OperatingHours.Mean, LHG.Mean, EHG.Mean, AreaPerPerson.Mean,
                HeatingSetpoint.Mean, CoolingSetpoint.Mean)
            { Name = Name };
        }
        public string ToString(string sep)
        {        
            return string.Join(sep, StartTime, OperatingHours, LHG, EHG, AreaPerPerson, HeatingSetpoint, CoolingSetpoint);
        }
        public string Header(string sep)
        {
            return GetAverage().Header(sep);
        }
       
    }
}
