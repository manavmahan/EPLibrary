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
            StartTime = new ProbabilityDistributionFunction("Start Time", "") { Mean = 8, VariationOrSD = 0, Distribution = PDF.unif },
            OperatingHours = new ProbabilityDistributionFunction("Operating Hours", "Hours") { Mean = 8, VariationOrSD = 0, Distribution = PDF.unif },
            LHG = new ProbabilityDistributionFunction("Light Heat Gain","W/m\u00b2") { Mean = 6, VariationOrSD = 0, Distribution = PDF.unif },
            EHG = new ProbabilityDistributionFunction("Equipment Heat Gain", "W/m\u00b2") { Mean = 12, VariationOrSD = 0, Distribution = PDF.unif },
            Occupancy = new ProbabilityDistributionFunction("Occupancy", "m\u00b2/Person") { Mean = 24, VariationOrSD = 0, Distribution = PDF.unif },
            HeatingSetpoint = new ProbabilityDistributionFunction("Heating Setpoint","\u00b0") { Mean = 20, VariationOrSD = 0, Distribution = PDF.unif },
            CoolingSetpoint = new ProbabilityDistributionFunction("Cooling Setpoint", "\u00b0") { Mean = 25, VariationOrSD = 0, Distribution = PDF.unif };

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
            this.Occupancy = AreaPerPerson;
            this.HeatingSetpoint = HeatingSetpoint;
            this.CoolingSetpoint = CoolingSetpoint;
        }
        public ZoneConditions GetAverage()
        {
            return new ZoneConditions(StartTime.Mean, OperatingHours.Mean, LHG.Mean, EHG.Mean, Occupancy.Mean,
                HeatingSetpoint.Mean, CoolingSetpoint.Mean)
            { Name = Name };
        }
        public string ToString(string sep)
        {        
            return string.Join(sep, StartTime, OperatingHours, LHG, EHG, Occupancy, HeatingSetpoint, CoolingSetpoint);
        }
        public string Header(string sep)
        {
            return GetAverage().Header(sep);
        }
       
    }
}
