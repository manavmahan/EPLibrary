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
        public int[] GetStartEndTime(double midHour)
        {
            int hour1, hour2, minutes1, minutes2;
            double endTime;
            if (StartTime == 0)
            {
                StartTime = midHour - .5 * OperatingHours; endTime = midHour + .5 * OperatingHours;
            }
            else
            {
                endTime = StartTime + OperatingHours;
            }
            hour1 = (int)Math.Truncate(StartTime);
            hour2 = (int)Math.Truncate(endTime);

            minutes1 = (int)Math.Round(Math.Round((StartTime - hour1) * 6)) * 10;
            minutes2 = (int)Math.Round(Math.Round((endTime - hour2) * 6)) * 10;

            if (minutes1 == 60)
            {
                minutes1 = 0; hour1++;
            }
            if (minutes2 == 60)
            {
                minutes2 = 0; hour2++;
            }
            double hours = hour2 - hour1, min = minutes2 - minutes1;
            if (min > 0)
            {
                hours += min / 60;
            }
            else
            {
                hours -= min / 60;
            }
            OperatingHours = hours;
            return new int[] { hour1, minutes1, hour2, minutes2};           
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
