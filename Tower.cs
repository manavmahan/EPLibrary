using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Tower
    {
        string name = "Main Tower";

        public Tower()
        {

        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "\r\nHVACTemplate:Plant:Tower,",
                "\t" + name + ", \t\t\t\t!- Name",
                "\tSingleSpeed" + ",\t\t\t\t!-Tower Type",
                "\tautosize" + ", \t\t\t\t!-High Speed Nominal Capacity {W}",
                "\tautosize" + ",\t\t\t\t!-High Speed Fan Power {W}",
                "\tautosize" + ", \t\t\t\t!-Low Speed Nominal Capacity {W}",
                "\tautosize" + ", \t\t\t\t!-Low Speed Fan Power {W}",
                "\tautosize" + ", \t\t\t\t!-Free Convection Capacity {W}",
                "\t1" + ", \t\t\t\t!-Priority",
                "\t1.2" + "; \t\t\t\t!-Sizing Factor"
            };
            return info;
        }

    }
}
