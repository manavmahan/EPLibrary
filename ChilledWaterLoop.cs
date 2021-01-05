using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ChilledWaterLoop
    {
        public string name = "Chilled Water Loop";

        public ChilledWaterLoop()
        {

        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Plant:ChilledWaterLoop,",
                name + ",      !- Name",
                ",                        !- Pump Schedule Name",
                "Intermittent,            !- Pump Control Type",
                "Default,                 !- Chiller Plant Operation Scheme Type",
                ",                        !- Chiller Plant Equipment Operation Schemes Name",
                ",                        !- Chilled Water Setpoint Schedule Name",
                "7.22,                    !- Chilled Water Design Setpoint {C}",
                "VariablePrimaryNoSecondary,  !- Chilled Water Pump Configuration",
                "179352,                  !- Primary Chilled Water Pump Rated Head {Pa}",
                "179352,                  !- Secondary Chilled Water Pump Rated Head {Pa}",
                "Default,                 !- Condenser Plant Operation Scheme Type",
                ",                        !- Condenser Equipment Operation Schemes Name",
                "OutdoorWetBulbTemperature,  !- Condenser Water Temperature Control Type",
                ",                        !- Condenser Water Setpoint Schedule Name",
                "29.4,                    !- Condenser Water Design Setpoint {C}",
                "179352,                  !- Condenser Water Pump Rated Head {Pa}",
                "None,                    !- Chilled Water Setpoint Reset Type",
                "12.2,                    !- Chilled Water Setpoint at Outdoor Dry-Bulb Low {C}",
                "15.6,                    !- Chilled Water Reset Outdoor Dry-Bulb Low {C}",
                "6.7,                     !- Chilled Water Setpoint at Outdoor Dry-Bulb High {C}",
                "26.7,                    !- Chilled Water Reset Outdoor Dry-Bulb High {C}",
                "SinglePump,              !- Chilled Water Primary Pump Type",
                "SinglePump,              !- Chilled Water Secondary Pump Type",
                "SinglePump,              !- Condenser Water Pump Type",
                "Yes,                     !- Chilled Water Supply Side Bypass Pipe",
                "Yes,                     !- Chilled Water Demand Side Bypass Pipe",
                "Yes,                     !- Condenser Water Supply Side Bypass Pipe",
                "Yes,                     !- Condenser Water Demand Side Bypass Pipe",
                "Water,                   !- Fluid Type",
                "6.67,                    !- Loop Design Delta Temperature {deltaC}",
                ",                        !- Minimum Outdoor Dry Bulb Temperature {C}",
                "SequentialLoad,          !- Chilled Water Load Distribution Scheme",
                "SequentialLoad; !-Condenser Water Load Distribution Scheme"
            };

            return info;
        }

    }
}
