using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ZoneVAV
    {
        public ZoneVAV() { }
        public string ThermostatName;
        public VAV vav;
        string ZoneName;
        public ZoneVAV(VAV v, Zone z, string t)
        {
            ZoneName = z.Name;
            vav = v;
            ThermostatName = t;
            z.ZoneVAV = this;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>();


            info.Add("\r\nHVACTemplate:Zone:VAV,");
            info.Add("\t" + ZoneName + ", \t\t\t\t!- Zone Name");
            info.Add("\t" + vav.name + ",\t\t\t\t!-Template VAV System Name");
            info.Add("\t" + ThermostatName + ",\t\t\t\t!-Template Thermostat Name");
            info.Add("\tautosize" + ",\t\t\t\t!-Supply Air Maximum Flow Rate {m3/s}");
            info.Add("\t" + ",\t\t\t\t!-Zone Heating Sizing Factor");
            info.Add("\t" + ",\t\t\t\t!-Zone Cooling Sizing Factor");
            info.Add("\tConstant" + ",\t\t\t\t!-Zone Minimum Air Flow Input Method");
            info.Add("\t0.2" + ",\t\t\t\t!-Constant Minimum Air Flow Fraction");
            info.Add("\t" + ",\t\t\t\t!-Fixed Minimum Air Flow Rate {m3/s}");
            info.Add("\t" + ",\t\t\t\t!-Minimum Air Flow Fraction Schedule Name");
            info.Add("\tFlow/Person" + ",\t\t\t\t!-Outdoor Air Method");
            info.Add("\t0.00944" + ",\t\t\t\t!-Outdoor Air Flow Rate per Person {m3/s}");
            info.Add("\t" + ",\t\t\t\t!-Outdoor Air Flow Rate per Zone Floor Area {m3/s-m2}");
            info.Add("\t" + ",\t\t\t\t!-Outdoor Air Flow Rate per Zone {m3/s}");
            info.Add("\tHotWater" + ",\t\t\t\t!-Reheat Coil Type");
            info.Add("\t" + ",\t\t\t\t!-Reheat Coil Availability Schedule Name");
            info.Add("\tReverse" + ",\t\t\t\t!-Damper Heating Action");
            info.Add("\t" + ",\t\t\t\t!-Maximum Flow per Zone Floor Area During Reheat {m3/s-m2}");
            info.Add("\t" + ",\t\t\t\t!-Maximum Flow Fraction During Reheat");
            info.Add("\t" + ",\t\t\t\t!-Maximum Reheat Air Temperature {C}");
            info.Add("\t" + ",\t\t\t\t!-Design Specification Outdoor Air Object Name for Control");
            info.Add("\t" + ",\t\t\t\t!-Supply Plenum Name");
            info.Add("\t" + ",\t\t\t\t!-Return Plenum Name");
            info.Add("\tNone" + ",\t\t\t\t!-Baseboard Heating Type");
            info.Add("\t" + ",\t\t\t\t!-Baseboard Heating Availability Schedule Name");
            info.Add("\tautosize" + ",\t\t\t\t!-Baseboard Heating Capacity {W}");
            info.Add("\tSystemSupplyAirTemperature" + ",\t\t\t\t!-Zone Cooling Design Supply Air Temperature Input Method");
            info.Add("\t" + ",\t\t\t\t!-Zone Cooling Design Supply Air Temperature {C}");
            info.Add("\t" + ",\t\t\t\t!-Zone Cooling Design Supply Air Temperature Difference {deltaC}");
            info.Add("\tSupplyAirTemperature" + ",\t\t\t\t!-Zone Heating Design Supply Air Temperature Input Method");
            info.Add("\t50" + ",\t\t\t\t!-Zone Heating Design Supply Air Temperature {C}");
            info.Add("\t" + ";\t\t\t\t!-Zone Heating Design Supply Air Temperature Difference {deltaC}");



            return info;
        }
    }
}
