using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class VAV
    {
        public string name = "VAV";
        public VAV()
        {
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: HVACTEMPLATE:SYSTEM:VAV ===========\r\n");

            info.Add("\r\nHVACTemplate:System:VAV,");
            info.Add("\t" + name + ", \t\t\t\t!- Name");
            info.Add("\t" + ", \t\t\t\t!- System Availability Schedule Name");
            info.Add("\tautosize" + ", \t\t\t\t!- Supply Fan Maximum Flow Rate {m3/s}");
            info.Add("\tautosize" + ", \t\t\t\t!- Supply Fan Minimum Flow Rate {m3/s}");
            info.Add("\t0.7" + ", \t\t\t\t!- Supply Fan Total Efficiency");
            info.Add("\t1000" + ", \t\t\t\t!- Supply Fan Delta Pressure {Pa}");
            info.Add("\t0.9" + ", \t\t\t\t!- Supply Fan Motor Efficiency");
            info.Add("\t1" + ", \t\t\t\t!- Supply Fan Motor in Air Stream Fraction");
            info.Add("\tChilledWater" + ", \t\t\t\t!- Cooling Coil Type");
            info.Add("\t" + ", \t\t\t\t!- Cooling Coil Availability Schedule Name");
            info.Add("\t" + ", \t\t\t\t!- Cooling Coil Setpoint Schedule Name");
            info.Add("\t12.8" + ", \t\t\t\t!- Cooling Coil Design Setpoint {C}");
            info.Add("\tHotWater" + ", \t\t\t\t!- Heating Coil Type");
            info.Add("\t" + ", \t\t\t\t!- Heating Coil Availability Schedule Name");
            info.Add("\t" + ", \t\t\t\t!- Heating Coil Setpoint Schedule Name");
            info.Add("\t10" + ", \t\t\t\t!- Heating Coil Design Setpoint {C}");
            info.Add("\t0.8" + ", \t\t\t\t!- Gas Heating Coil Efficiency");
            info.Add("\t" + ", \t\t\t\t!- Gas Heating Coil Parasitic Electric Load {W}");
            info.Add("\tNone" + ", \t\t\t\t!- Preheat Coil Type");
            info.Add("\t" + ", \t\t\t\t!- Preheat Coil Availability Schedule Name");
            info.Add("\t" + ", \t\t\t\t!- Preheat Coil Setpoint Schedule Name");
            info.Add("\t7.2" + ", \t\t\t\t!- Preheat Coil Design Setpoint {C}");
            info.Add("\t0.8" + ", \t\t\t\t!- Gas Preheat Coil Efficiency");
            info.Add("\t" + ", \t\t\t\t!- Gas Preheat Coil Parasitic Electric Load {W}");
            info.Add("\tautosize" + ", \t\t\t\t!- Maximum Outdoor Air Flow Rate {m3/s}");
            info.Add("\tautosize" + ", \t\t\t\t!- Minimum Outdoor Air Flow Rate {m3/s}");
            info.Add("\tProportionalMinimum" + ", \t\t\t\t!- Minimum Outdoor Air Control Type");
            info.Add("\t" + ", \t\t\t\t!- Minimum Outdoor Air Schedule Name");
            info.Add("\tNoEconomizer" + ", \t\t\t\t!- Economizer Type");
            info.Add("\tNoLockout" + ", \t\t\t\t!- Economizer Lockout");
            info.Add("\t" + ", \t\t\t\t!- Economizer Upper Temperature Limit {C}");
            info.Add("\t" + ", \t\t\t\t!- Economizer Lower Temperature Limit {C}");
            info.Add("\t" + ", \t\t\t\t!- Economizer Upper Enthalpy Limit {J/kg}");
            info.Add("\t" + ", \t\t\t\t!- Economizer Maximum Limit Dewpoint Temperature {C}");
            info.Add("\t" + ", \t\t\t\t!- Supply Plenum Name");
            info.Add("\t" + ", \t\t\t\t!- Return Plenum Name");
            info.Add("\tDrawThrough" + ", \t\t\t\t!- Supply Fan Placement");
            info.Add("\tInletVaneDampers" + ", \t\t\t\t!- Supply Fan Part-Load Power Coefficients");
            info.Add("\tStayOff" + ", \t\t\t\t!- Night Cycle Control");
            info.Add("\t" + ", \t\t\t\t!- Night Cycle Control Zone Name");
            info.Add("\tNone" + ", \t\t\t\t!- Heat Recovery Type");
            info.Add("\t0.7" + ", \t\t\t\t!- Sensible Heat Recovery Effectiveness");
            info.Add("\t0.65" + ", \t\t\t\t!- Latent Heat Recovery Effectiveness");
            info.Add("\tNone" + ", \t\t\t\t!- Cooling Coil Setpoint Reset Type");
            info.Add("\tNone" + ", \t\t\t\t!- Heating Coil Setpoint Reset Type");
            info.Add("\tNone" + ", \t\t\t\t!- Dehumidification Control Type");
            info.Add("\t" + ", \t\t\t\t!- Dehumidification Control Zone Name");
            info.Add("\t60" + ", \t\t\t\t!- Dehumidification Setpoint {percent}");
            info.Add("\tNone" + ", \t\t\t\t!- Humidifier Type");
            info.Add("\t" + ", \t\t\t\t!- Humidifier Availability Schedule Name");
            info.Add("\t0.000001" + ", \t\t\t\t!- Humidifier Rated Capacity {m3/s}");
            info.Add("\tautosize" + ", \t\t\t\t!- Humidifier Rated Electric Power {W}");
            info.Add("\t" + ", \t\t\t\t!- Humidifier Control Zone Name");
            info.Add("\t30" + ", \t\t\t\t!- Humidifier Setpoint {percent}");
            info.Add("\tNonCoincident" + ", \t\t\t\t!- Sizing Option");
            info.Add("\tNo" + ", \t\t\t\t!- Return Fan");
            info.Add("\t0.7" + ", \t\t\t\t!- Return Fan Total Efficiency");
            info.Add("\t500" + ", \t\t\t\t!- Return Fan Delta Pressure {Pa}");
            info.Add("\t0.9" + ", \t\t\t\t!- Return Fan Motor Efficiency");
            info.Add("\t1" + ", \t\t\t\t!- Return Fan Motor in Air Stream Fraction");
            info.Add("\tInletVaneDampers" + "; \t\t\t\t!- Return Fan Part-Load Power Coefficients");

            return info;
        }


    }
}
