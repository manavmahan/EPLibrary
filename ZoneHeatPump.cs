using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ZoneHeatPump
    {
        public ZoneHeatPump() { }
        public string ThermostatName;
        public string ZoneName;
        public string OccupancyScheduleName;

		public float[] COP;
        public List<string> WriteInfo()
        {
			return new List<string>()
			{
				"HVACTemplate:Zone:WaterToAirHeatPump,",
				Utility.IDFLineFormatter(ZoneName, "Zone Name"),
				Utility.IDFLineFormatter(ThermostatName, "Template Thermostat Name"),
				"autosize, !- Cooling Supply Air Flow Rate {m3/s}",
				"autosize, !- Heating Supply Air Flow Rate {m3/s}",
				", !- No Load Supply Air Flow Rate {m3/s}",
				"1.2, !- Zone Heating Sizing Factor",
				"1.2, !- Zone Cooling Sizing Factor",
				"Flow/Person, !- Outdoor Air Method",
				"0.00944, !- Outdoor Air Flow Rate per Person {m3/s}",
				", !- Outdoor Air Flow Rate per Zone Floor Area {m3/s-m2}",
				", !- Outdoor Air Flow Rate per Zone {m3/s}",
				", !- System Availability Schedule Name",
				", !- Supply Fan Operating Mode Schedule Name",
				"DrawThrough , !- Supply Fan Placement",
				"0.7, !- Supply Fan Total Efficiency",
				"75, !- Supply Fan Delta Pressure {Pa}",
				"0.9, !- Supply Fan Motor Efficiency",
				"Coil:Cooling:WaterToAirHeatPump:EquationFit, !- Cooling Coil Type",
				"autosize, !- Cooling Coil Gross Rated Total Capacity {W}",
				"autosize, !- Cooling Coil Gross Rated Sensible Heat Ratio",
				Utility.IDFLineFormatter(COP[1], "Cooling COP"),
				"Coil:Heating:WaterToAirHeatPump:EquationFit, !- HPump Heating Coil Type",
				"autosize, !- Heat Pump Heating Coil Gross Rated Capacity {W}",
				Utility.IDFLineFormatter(COP[0], "Heating COP"),
				", !- Supplemental Heating Coil Availability Schedule Name",
				"autosize, !- Supplemental Heating Coil Capacity {W}",
				"2.5, !- Maximum Cycling Rate {cycles/hr}",
				"60, !- Heat Pump Time Constant {s}",
				"0.01, !- Fraction of On-Cycle Power Use",
				"60, !- Heat Pump Fan Delay Time {s}",
				", !- Dedicated Outdoor Air System Name",
				"Electric, !- Supplemental Heating Coil Type",
				"SupplyAirTemperature, !- Zone Cooling Design Supply Air Temperature Input Method",
				"12.5, !- Zone Cooling Design Supply Air Temperature {C]",
				", !- Zone Cooling Design Supply Air Temperature Difference {deltaC]",
				"SupplyAirTemperature, !- Zone Heating Design Supply Air Temperature Input Method",
				"50.0, !- Zone Heating Design Supply Air Temperature {C]",
				", !- Zone Heating Design Supply Air Temperature Difference {deltaC]",
				", !- Design Specification Outdoor Air Object Name",
				"; !-Design Specification Zone Air Distribution Object Nametio Curve Name"
			};
        }
    }
}
