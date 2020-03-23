using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Chiller
    {
        string name = "Main Chiller";
        double chillerCOP;
        public Chiller() { }

        public Chiller(double COP)
        {
            chillerCOP = COP;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Plant:Chiller,",
                name + ",            !-Name",
                "ElectricCentrifugalChiller,  !-Chiller Type",
                "autosize,                !-Capacity { W}",
                chillerCOP + ",                     !-Nominal COP { W / W}",
                "WaterCooled,             !-Condenser Type",
                "1,                       !-Priority",
                "1,                       !-Sizing Factor",
                "0.1,                     !-Minimum Part Load Ratio",
                "1.1,                     !-Maximum Part Load Ratio",
                "0.9,                     !-Optimum Part Load Ratio",
                "0.2,                     !-Minimum Unloading Ratio",
                "2; !-Leaving Chilled Water Lower Temperature Limit { C}"
            };
            return info;
        }

    }
}
