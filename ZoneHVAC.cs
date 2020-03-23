using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public abstract class ZoneHVAC
    {
        public ZoneHVAC() { }
        public Thermostat thermostat { get; set; }
        public ZoneHVAC(Thermostat thermostat)
        {
            this.thermostat = thermostat;
        }
    }
}
