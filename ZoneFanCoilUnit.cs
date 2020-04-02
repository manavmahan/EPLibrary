using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ZoneFanCoilUnit
    {
        public ZoneFanCoilUnit() { }
        public string ThermostatName;
        public string ZoneName;
        public string OccupancyScheduleName;
        public ZoneFanCoilUnit(Zone z, string thermostat)
        {
            ZoneName = z.Name;
            ThermostatName = thermostat;
            OccupancyScheduleName = z.OccupancyScheduleName;
            z.ZoneFCU = this;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Zone:FanCoil,",
                ZoneName + ",                  !- Zone Name",
                ThermostatName + ",              !- Template Thermostat Name",
                "autosize,                !- Supply Air Maximum Flow Rate {m3/s}",
                ",                        !- Zone Heating Sizing Factor",
                ",                        !- Zone Cooling Sizing Factor",
                "flow/person,             !- Outdoor Air Method",
                "0.00944,                 !- Outdoor Air Flow Rate per Person {m3/s}",
                "0.0,                     !- Outdoor Air Flow Rate per Zone Floor Area {m3/s-m2}",
                "0.0,                     !- Outdoor Air Flow Rate per Zone {m3/s}",
                ",                        !- System Availability Schedule Name",
                "0.7,                     !- Supply Fan Total Efficiency",
                "75,                      !- Supply Fan Delta Pressure {Pa}",
                "0.9,                     !- Supply Fan Motor Efficiency",
                "1,                       !- Supply Fan Motor in Air Stream Fraction",
                "ChilledWater,            !- Cooling Coil Type",
                ",                        !- Cooling Coil Availability Schedule Name",
                "12.5,                    !- Cooling Coil Design Setpoint {C}",
                "HotWater,                !- Heating Coil Type",
                ",                        !- Heating Coil Availability Schedule Name",
                "50,                      !- Heating Coil Design Setpoint {C}",
                ",                        !- Dedicated Outdoor Air System Name",
                "SupplyAirTemperature,    !- Zone Cooling Design Supply Air Temperature Input Method",
                ",                        !- Zone Cooling Design Supply Air Temperature Difference {deltaC}",
                "SupplyAirTemperature,    !- Zone Heating Design Supply Air Temperature Input Method",
                ",                        !- Zone Heating Design Supply Air Temperature Difference {deltaC}",
                ",                        !- Design Specification Outdoor Air Object Name",
                ",                        !- Design Specification Zone Air Distribution Object Name",
                "ConstantFanVariableFlow, !- Capacity Control Method",
                ",                        !- Low Speed Supply Air Flow Ratio",
                ",                        !- Medium Speed Supply Air Flow Ratio",
                Utility.IDFLineFormatter(OccupancyScheduleName, "Outdoor Air Schedule Name"),
                "None,                    !- Baseboard Heating Type",
                ",                        !- Baseboard Heating Availability Schedule Name",
                "Autosize; !-Baseboard Heating Capacity { W}"
            };

            return info;
        }
    }
}
