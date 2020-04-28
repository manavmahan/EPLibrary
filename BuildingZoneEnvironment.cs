using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    public class BuildingZoneEnvironment
    {
        public string Name = "Building";
        public double HeatingSetPoint, CoolingSetPoint;
        public BuildingZoneEnvironment() { }
        public BuildingZoneEnvironment(double HeatingSetPoint, double CoolingSetPoint)
        {
            this.HeatingSetPoint = HeatingSetPoint;
            this.CoolingSetPoint = CoolingSetPoint;
        }
        public string Header(string sep)
        {
            return string.Format(
                "{0}Heating Setpoint{1}" +
                "{0}Cooling Setpoint",
                Name + ":", sep);
           
        }
        public string ToString(string sep)
        {
            return string.Join(sep, HeatingSetPoint, CoolingSetPoint);
        }
    }
}
