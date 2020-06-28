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
        public EPZone()
        {
            
        }
        public string ToString(bool monthly)
        {
            if (monthly)
                return string.Join(",", HeatingLoadMonthly.ToCSVString(), CoolingLoadMonthly.ToCSVString(), LightsLoadMonthly.ToCSVString());
            else
                return string.Join(",", HeatingLoad, CoolingLoad, LightsLoad);
        }
        public static string Header(bool Monthly)
        {
            List<string> vars = new List<string>() { "Heating Load", "Cooling Load", "Lights Load" };

            if (Monthly)
            {
                string mVars = string.Empty;
                foreach (string var in vars)
                {
                    mVars += string.Join(",", Enum.GetNames(typeof(Month)).Select(m => string.Format("{0} ({1})", var, m)));
                }
                return mVars;
            }
            else
                return string.Join(",", vars);
        }       
    }
}
