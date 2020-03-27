using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ZoneIdealLoad
    {
        string ZoneName;
        public string ThermostatName;
        public ZoneIdealLoad() { }
        public ZoneIdealLoad(Zone z, string thermostat)
        {
            ZoneName = z.Name;
            ThermostatName = thermostat;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("HVACTemplate:Zone:IdealLoadsAirSystem,");
            info.Add("\t" + ZoneName + ",\t\t\t\t\t\t!- Zone Name");
            info.Add("\t" + ThermostatName + ", \t\t\t\t!- Template Thermostat Name");
            info.Add("\t, \t\t\t\t!- System Availability Schedule Name");
            info.Add("\t50, \t\t\t\t!- Maximum Heating Supply Air Temperature {C}");
            info.Add("\t13, \t\t\t\t!- Minimum Cooling Supply Air Temperature {C}");
            info.Add("\t0.0156, \t\t\t\t!- Maximum Heating Supply Air Humidity Ratio {kgWater/kgDryAir}");
            info.Add("\t0.0077, \t\t\t\t!- Minimum Cooling Supply Air Humidity Ratio {kgWater/kgDryAir}");
            info.Add("\tNoLimit, \t\t\t\t!- Heating Limit");
            info.Add("\t, \t\t\t\t!- Maximum Heating Air Flow Rate {m3/s}");
            info.Add("\t, \t\t\t\t!- Maximum Sensible Heating Capacity {W}");
            info.Add("\tNoLimit, \t\t\t\t!- Cooling Limit");
            info.Add("\t, \t\t\t\t!- Maximum Cooling Air Flow Rate {m3/s}");
            info.Add("\t, \t\t\t\t!- Maximum Total Cooling Capacity {W}");
            info.Add("\t, \t\t\t\t!- Heating Availability Schedule Name");
            info.Add("\t, \t\t\t\t!- Cooling Availability Schedule Name");
            info.Add("\tConstantSensibleHeatRatio, \t\t\t\t!- Dehumidification Control Type");
            info.Add("\t0.7, \t\t\t\t!- Cooling Sensible Heat Ratio {dimensionless}");
            info.Add("\t60, \t\t\t\t!- Dehumidification Setpoint {percent}");
            info.Add("\tNone, \t\t\t\t!- Humidification Control Type");
            info.Add("\t30, \t\t\t\t!- Humidification Setpoint {percent}");
            info.Add("\tNone, \t\t\t\t!- Outdoor Air Method");
            info.Add("\t0.00944, \t\t\t\t!- Outdoor Air Flow Rate per Person {m3/s}");
            info.Add("\t, \t\t\t\t!- Outdoor Air Flow Rate per Zone Floor Area {m3/s-m2}");
            info.Add("\t, \t\t\t\t!- Outdoor Air Flow Rate per Zone {m3/s}");
            info.Add("\t, \t\t\t\t!- Design Specification Outdoor Air Object Name");
            info.Add("\tNone, \t\t\t\t!- Demand Controlled Ventilation Type");
            info.Add("\tNoEconomizer, \t\t\t\t!- Outdoor Air Economizer Type");
            info.Add("\tNone, \t\t\t\t!- Heat Recovery Type");
            info.Add("\t0.7, \t\t\t\t!- Sensible Heat Recovery Effectiveness {dimensionless}");
            info.Add("\t0.65; \t\t\t\t!- Latent Heat Recovery Effectiveness {dimensionless} \r\n");
            return info;
        }
    }
}
