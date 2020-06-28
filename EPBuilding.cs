using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class EPBuilding
    {
        public double ThermalEnergy, OperationalEnergy,
            ZoneHeatingLoad, ZoneCoolingLoad, ZoneLightsLoad, EUI;
        
        public double[] ThermalEnergyMonthly, OperationalEnergyMonthly,
            ZoneHeatingLoadMonthly, ZoneCoolingLoadMonthly, ZoneLightsLoadMonthly, EUIMonthly;
        public EPBuilding() { }
        public static string Header (bool Monthly)
        {
            List<string> vars = new List<string>() { "Heating Load", "Cooling Load", "Lights Load",
                "Thermal Energy", "Operational Energy", "EUI"};

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
        public string ToString(bool Monthly)
        {
            if (Monthly) 
            {
                return string.Join(",", 
                    ZoneHeatingLoadMonthly.ToCSVString(), 
                    ZoneCoolingLoadMonthly.ToCSVString(), 
                    ZoneLightsLoadMonthly.ToCSVString(),
                    ThermalEnergyMonthly.ToCSVString(), 
                    OperationalEnergyMonthly.ToCSVString(), 
                    EUIMonthly.ToCSVString()) ;
            }
            else
            {
                return string.Join(",",ZoneHeatingLoad, ZoneCoolingLoad, ZoneLightsLoad, 
                    ThermalEnergy, OperationalEnergy, EUI);                
            }
        }
    }    
}
