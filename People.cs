using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class People
    {
        public People() { }

        public string Name, ZoneName;
        public string scheduleName { get; set; }
        public string calculationMethod { get; set; }
        //public double numberOfPeople { get; set; }
        //public double peoplePerArea { get; set; }
        public double areaPerPerson { get; set; }
        public double fractionRadiant { get; set; }
        public double sensibleHeatFraction { get; set; }
        public string activityLvlSchedName { get; set; }
        public double c02genRate { get; set; }
        public string enableComfortWarnings { get; set; }
        public string meanRadiantTempCalcType { get; set; }
        public string surfaceName { get; set; }
        public string workEffSchedule { get; set; }
        public string clothingInsulationCalcMeth { get; set; }
        public string clothingInsulationCalcMethSched { get; set; }
        public string clothingInsulationSchedName { get; set; }
        public string airVelSchedName { get; set; }
        public string thermalComfModel1t { get; set; }
        public People(double aPP)
        {
            areaPerPerson = aPP;
            calculationMethod = "Area/Person";
            scheduleName = "Occupancy Schedule";
            fractionRadiant = 0.1;
            activityLvlSchedName = "People Activity Schedule";
            c02genRate = double.Parse("3.82E-8");
            enableComfortWarnings = "";
            meanRadiantTempCalcType = "ZoneAveraged";
            surfaceName = "";
            workEffSchedule = "Work Eff Sch";
            clothingInsulationCalcMeth = "DynamicClothingModelASHRAE55";
            clothingInsulationCalcMethSched = "";
            clothingInsulationSchedName = "";
            airVelSchedName = "Air Velo Sch";
            thermalComfModel1t = "Fanger";
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "People,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(ZoneName, "Zone or ZoneList Name"),
                Utility.IDFLineFormatter(scheduleName, "Schedule Name"),
                Utility.IDFLineFormatter(calculationMethod, "Number of People Calculation Method"),
                Utility.IDFLineFormatter("", "Number of People"),
                Utility.IDFLineFormatter("","People per Zone Floor Area {person/m2}"),
                Utility.IDFLineFormatter(areaPerPerson, "Zone Floor Area per Person {m2/person}"),
                Utility.IDFLineFormatter(fractionRadiant, "Fraction Radiant"),
                Utility.IDFLineFormatter("", "Sensible Heat Fraction"),
                Utility.IDFLineFormatter(activityLvlSchedName, "Activity Level Schedule Name"),
                Utility.IDFLineFormatter(c02genRate, "Carbon Dioxide Generation Rate {m3/s-W}"),
                Utility.IDFLineFormatter("", "Enable ASHRAE 55 Comfort Warnings"),
                Utility.IDFLineFormatter(meanRadiantTempCalcType, "Mean Radiant Temperature Calculation Type"),
                Utility.IDFLineFormatter("", "Surface Name/Angle Factor List Name"),
                Utility.IDFLineFormatter("Work Eff Sch", "Work Efficiency Schedule Name"),
                Utility.IDFLineFormatter(clothingInsulationCalcMeth, "Clothing Insulation Calculation Method"),
                Utility.IDFLineFormatter("", "Clothing Insulation Calculation Method Schedule Name"),
                Utility.IDFLineFormatter("", "Clothing Insulation Schedule Name"),
                Utility.IDFLineFormatter("Air Velo Sch", "Air Velocity Schedule Name"),
                Utility.IDFLastLineFormatter(thermalComfModel1t, "Thermal Comfort Model 1 Type")
            };
        }
    }
}
