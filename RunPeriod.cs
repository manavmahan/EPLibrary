using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class RunPeriod
    {
        public string name = "Run Period 1",
            dayOfWeekForStartDay = "",
            useWeatherFileHolidaysAndSpecialDays = "No",
            useWeatherFileDaylightSavingPeriod = "No",
            WeekendHolidayRule = "No",
            useWeatherFileRainIndicators = "Yes",
            useWeatherFileSnowIndicators = "Yes",
            actualWeather = "No";

        public int beginMonth = 1,
        beginDayMonth = 1,
            endMonth = 12,
            endDayOfMonth = 31;


        public RunPeriod()
        {

        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "RunPeriod,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(beginMonth, "Begin Month"),
                Utility.IDFLineFormatter(beginDayMonth, "Begin Day of Month"),
                Utility.IDFLineFormatter("", "Begin Year"),
                Utility.IDFLineFormatter(endMonth, "End Month"),
                Utility.IDFLineFormatter(endDayOfMonth, "End Day of Month"),
                 Utility.IDFLineFormatter("", "End year"),
                Utility.IDFLineFormatter(dayOfWeekForStartDay, "Day of Week for Start Day"),
                Utility.IDFLineFormatter(useWeatherFileHolidaysAndSpecialDays, "Use Weather File Holidays and Special Days"),
                Utility.IDFLineFormatter(useWeatherFileDaylightSavingPeriod, "Use Weather File Daylight Saving Period"),
                Utility.IDFLineFormatter(WeekendHolidayRule, "Apply Weekend Holiday Rule"),
                Utility.IDFLineFormatter(useWeatherFileRainIndicators, "Use Weather File Rain Indicators"),
                Utility.IDFLineFormatter(useWeatherFileSnowIndicators, "Use Weather File Snow Indicators"),
                Utility.IDFLastLineFormatter(actualWeather, "Actual Weather")
            };
        }
    }
}
