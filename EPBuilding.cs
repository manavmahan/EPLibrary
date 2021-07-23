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
        public float ThermalEnergy, OperationalEnergy,
            ZoneHeatingLoad, ZoneCoolingLoad, ZoneLightsLoad, EUI;
        
        public float[] ThermalEnergyMonthly, OperationalEnergyMonthly,
            ZoneHeatingLoadMonthly, ZoneCoolingLoadMonthly, ZoneLightsLoadMonthly, EUIMonthly;

        public float[] ThermalEnergyHourly, OperationalEnergyHourly,
            ZoneHeatingLoadHourly, ZoneCoolingLoadHourly, ZoneLightsLoadHourly, EUIHourly;
        public EPBuilding() { }
        public static string Header ()
        {
            List<string> vars = new List<string>() { "Heating Load", "Cooling Load", "Lights Load",
                "Thermal Energy", "Operational Energy", "EUI"};

            return string.Join(",", vars);
        }
        public string ToString(string time)
        {
            if (time == "Monthly") 
            {
                return string.Join(",", 
                    ZoneHeatingLoadMonthly.ToCSVString(), 
                    ZoneCoolingLoadMonthly.ToCSVString(), 
                    ZoneLightsLoadMonthly.ToCSVString(),
                    ThermalEnergyMonthly.ToCSVString(), 
                    OperationalEnergyMonthly.ToCSVString(), 
                    EUIMonthly.ToCSVString()) ;
            }
            if (time == "Hourly")
            {
                return string.Join(",",
                    ZoneHeatingLoadHourly.ToCSVString(),
                    ZoneCoolingLoadHourly.ToCSVString(),
                    ZoneLightsLoadHourly.ToCSVString(),
                    ThermalEnergyHourly.ToCSVString(),
                    OperationalEnergyHourly.ToCSVString(),
                    EUIHourly.ToCSVString());
            }
            return string.Join(",", ZoneHeatingLoad, ZoneCoolingLoad, ZoneLightsLoad,
                   ThermalEnergy, OperationalEnergy, EUI);
        }
    }    
}
