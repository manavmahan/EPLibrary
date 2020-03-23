using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ZoneBaseBoardHeat : ZoneHVAC
    {
        public ZoneBaseBoardHeat() { }
        Zone zone;

        public ZoneBaseBoardHeat(Zone z, Thermostat t)
        {
            zone = z;
            thermostat = t;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Zone:BaseBoardHeat,",
                zone.Name + ", !-Zone Name",
                thermostat.name + ", !-Template Thermostat Name",
                "1.2, !-Zone Heating Sizing Factor",
                "Electric, !-Baseboard Heating Type",
                ", !-Baseboard Heating Availability Schedule Name",
                "Autosize, !-Baseboard Heating Capacity { W}",
                ", !-Dedicated Outdoor Air System Name",
                "flow/person , !-Outdoor Air Method",
                "0.00944, !-Outdoor Air Flow Rate per Person { m3 / s}",
                "0.0, !-Outdoor Air Flow Rate per Zone Floor Area { m3 / s - m2}",
                "0.0, !-Outdoor Air Flow Rate per Zone { m3 / s}",
                ", !-Design Specification Outdoor Air Object Name",
                "; !-Design Specification Zone Air Distribution Object Name"
            };

            return info;
        }
    }
}
