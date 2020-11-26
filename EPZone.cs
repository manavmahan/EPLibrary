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
        public double HeatingLoad, CoolingLoad, LightsLoad;
        public double[] HeatingLoadMonthly, CoolingLoadMonthly, LightsLoadMonthly;
        public double[] HeatingLoadHourly, CoolingLoadHourly, LightsLoadHourly;
        public EPZone()
        {
            
        }
        public string ToString(string time)
        {
            if (time == "monthly") 
                return string.Join(",", HeatingLoadMonthly.ToCSVString(), CoolingLoadMonthly.ToCSVString(), LightsLoadMonthly.ToCSVString());
            if (time == "hourly")
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
