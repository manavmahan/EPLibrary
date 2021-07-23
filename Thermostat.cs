using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Thermostat
    {
        public Thermostat() { }
        public string name { get; set; }
        public float heatingSetPoint { get; set; }
        public float coolingSetPoint { get; set; }
        public ScheduleCompact ScheduleHeating { get; set; }
        public ScheduleCompact ScheduleCooling { get; set; }

        public Thermostat(string n, float heatingSP, float coolingSP, ScheduleCompact scheduleHeating, ScheduleCompact scheduleCooling)
        {
            heatingSetPoint = heatingSP; ScheduleHeating = scheduleHeating; ScheduleCooling = scheduleCooling;
            coolingSetPoint = coolingSP;
            name = n;
        }
    }
}
