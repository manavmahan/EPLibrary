using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class BuildingZoneOperation
    {
        public string Name = "Building";

        public double StartTime, OperatingHours, LHG, EHG; 
        public BuildingZoneOperation() { }

        public BuildingZoneOperation(double StartTime, double OperatingHours, double LHG, double EHG)
        {
            this.StartTime = StartTime;
            this.OperatingHours = OperatingHours; 
            this.LHG = LHG; 
            this.EHG = EHG;
        }
        public double[] GetStartEndTime(double midHour)
        {
            return new double[] { midHour - .5 * OperatingHours, midHour + .5 * OperatingHours };
        }
        public string ToString(string sep)
        {
            return string.Join(sep, StartTime, OperatingHours, LHG, EHG);
        }
        public string Header(string sep)
        {
            return string.Format(
                "{0}Start Time{1}" +
                "{0}Operating Hours{1}" +
                "{0}Light Heat Gain{1}" +
                "{0}Equipment Heat Gain",
                Name+":", sep);
        }
    }
}
