using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ScheduleLimits
    {
        public string name { get; set; }
        public float lowerLimit { get; set; }
        public float upperLimit { get; set; }
        public string numericType { get; set; }
        public string unitType { get; set; }

        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("ScheduleTypeLimits,");
            info.Add(name + ",\t\t\t\t!-Name");
            info.Add(lowerLimit + ", \t\t\t\t!- Lower Limit Value");

            if (upperLimit > lowerLimit) info.Add(upperLimit + ",  \t\t\t\t!- Upper Limit Value");
            else info.Add(",  \t\t\t\t!- Upper Limit Value");

            info.Add(numericType + ",  \t\t\t\t!- Numeric Type");
            info.Add(unitType + ";  \t\t\t\t!- Unit Type");
            return info;
        }
        public ScheduleLimits()
        {
            numericType = "Continuous";
            unitType = "";
        }
    }
}
