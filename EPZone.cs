using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class EPZone
    {
        public float HeatingLoad, CoolingLoad, LightsLoad;
        public float[] HeatingLoadMonthly, CoolingLoadMonthly, LightsLoadMonthly;
        public float[] HeatingLoadHourly, CoolingLoadHourly, LightsLoadHourly;
        public EPZone()
        {
            
        }
        public string ToString(string time)
        {
            if (time == "Monthly") 
                return string.Join(",", HeatingLoadMonthly.ToCSVString(), CoolingLoadMonthly.ToCSVString(), LightsLoadMonthly.ToCSVString());
            if (time == "Hourly")
                return string.Join(",", HeatingLoadHourly.ToCSVString(), CoolingLoadHourly.ToCSVString(), LightsLoadHourly.ToCSVString()); 
            return string.Join(",", HeatingLoad, CoolingLoad, LightsLoad);
        }
        public static string Header()
        {
            List<string> vars = new List<string>() { "Heating Load", "Cooling Load", "Lights Load" };
            return string.Join(",", vars);
        }       
    }
}
