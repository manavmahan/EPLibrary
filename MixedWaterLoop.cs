using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class MixedWaterLoop
    {
        public string name = "Chilled Water Loop";
        public MixedWaterLoop() { }
        public List<string> WriteInfo()
        {
            return new List<string>(){
                "HVACTemplate:Plant:MixedWaterLoop,",
                "Only Water Loop , !-Name",
                ", !-Pump Schedule Name",
                "Intermittent , !-Pump Control Type",
                "Default , !-Operation Scheme Type",
                ", !-Equipment Operation Schemes Name",
                ", !-High Temperature Setpoint Schedule Name",
                "34, !-High Temperature Design Setpoint { C}",
                ", !-Low Temperature Setpoint Schedule Name",
                "20, !-Low Temperature Design Setpoint { C}",
                "ConstantFlow , !-Water Pump Configuration", 
                "179352, !-Water Pump Rated Head { Pa}",
                "SinglePump , !-Water Pump Type",
                "Yes , !-Supply Side Bypass Pipe",
                "Yes, !-Demand Side Bypass Pipe",
                "Water, !-Fluid Type",
                "6, !-Loop Design Delta Temperature { deltaC}",
                "SequentialLoad; !-Load Distribution Scheme",
            };
        }
    }
}
