using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Light
    {
        public string Name, ZoneName;
        public string scheduleName { get; set; }
        public Light() { }
        public string designLevelCalcMeth { get; set; }
        //public double lightingLevel { get; set; }
        public double wattsPerArea { get; set; }
        public double returnAirFraction { get; set; }
        public double fractionRadiant { get; set; }
        public double fractionVisible { get; set; }
        public double fractionReplaceable { get; set; }

        public Light(double wPA)
        {
            wattsPerArea = wPA;
            designLevelCalcMeth = "Watts/area";
            scheduleName = "Electric Equipment and Lighting Schedule";
            returnAirFraction = 0;
            fractionRadiant = 0.1;
            fractionVisible = 0.18;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "Lights,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(ZoneName , "Zone or ZoneList Name"),
                Utility.IDFLineFormatter(scheduleName , "Schedule Name"),
                Utility.IDFLineFormatter(designLevelCalcMeth , "Design Level Calculation Method"),
                Utility.IDFLineFormatter("", "Lighting Level {W}"),
                Utility.IDFLineFormatter(wattsPerArea, "Watts per Zone Floor Area {W/m2}"),
                Utility.IDFLineFormatter("","Watts per Person {W/person}"),
                Utility.IDFLineFormatter(returnAirFraction , "Return Air Fraction"),
                Utility.IDFLineFormatter(fractionRadiant , "Fraction Radiant"),
                Utility.IDFLineFormatter(fractionVisible , "Fraction Visible"),
                Utility.IDFLastLineFormatter("", "Fraction Replaceable")
            };
        }
    }
}
