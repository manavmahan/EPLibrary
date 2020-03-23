using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class HotWaterLoop
    {
        string name = "Hot Water Loop";

        public HotWaterLoop()
        {

        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>() {
                "HVACTemplate:Plant:HotWaterLoop,",
                name + ",          !- Name",
                ",                        !- Pump Schedule Name",
                "Intermittent,            !- Pump Control Type",
                "Default,                 !- Hot Water Plant Operation Scheme Type",
                ",                        !- Hot Water Plant Equipment Operation Schemes Name",
                ",                        !- Hot Water Setpoint Schedule Name",
                "82,                      !- Hot Water Design Setpoint {C}",
                "VariableFlow,            !- Hot Water Pump Configuration",
                "179352,                  !- Hot Water Pump Rated Head {Pa}",
                "None,                    !- Hot Water Setpoint Reset Type",
                "82.2,                    !- Hot Water Setpoint at Outdoor Dry-Bulb Low {C}",
                "-6.7,                    !- Hot Water Reset Outdoor Dry-Bulb Low {C}",
                "65.6,                    !- Hot Water Setpoint at Outdoor Dry-Bulb High {C}",
                "10,                      !- Hot Water Reset Outdoor Dry-Bulb High {C}",
                "SinglePump,              !- Hot Water Pump Type",
                "Yes,                     !- Supply Side Bypass Pipe",
                "Yes,                     !- Demand Side Bypass Pipe",
                "Water,                   !- Fluid Type",
                "11,                      !- Loop Design Delta Temperature {deltaC}",
                ",                        !- Maximum Outdoor Dry Bulb Temperature {C}",
                "SequentialLoad; !-Load Distribution Scheme"
            };

            return info;
        }
    }
}
