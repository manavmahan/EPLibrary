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
        public double heatingSetPoint { get; set; }
        public double coolingSetPoint { get; set; }
        public ScheduleCompact ScheduleHeating { get; set; }
        public ScheduleCompact ScheduleCooling { get; set; }

        public Thermostat(string n, double heatingSP, double coolingSP, ScheduleCompact scheduleHeating, ScheduleCompact scheduleCooling)
        {
            heatingSetPoint = heatingSP; ScheduleHeating = scheduleHeating; ScheduleCooling = scheduleCooling;
            coolingSetPoint = coolingSP;
            name = n;
        }
    }
}
