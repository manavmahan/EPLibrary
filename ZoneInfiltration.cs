using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ZoneInfiltration
    {
        public string Name, ZoneName;
        public float airChangesHour { get; set; }
        public float constantTermCoeff { get; set; }
        public float temperatureTermCoef { get; set; }
        public float velocityTermCoef { get; set; }
        public float velocitySquaredTermCoef { get; set; }

        public ZoneInfiltration(float acH)
        {
            airChangesHour = acH;
            constantTermCoeff = 0.606f;
            temperatureTermCoef = 0.036359996f;
            velocityTermCoef = 0.1177165f;
            velocitySquaredTermCoef = 0;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "ZoneInfiltration:DesignFlowRate,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(ZoneName, "Zone or ZoneList Name"),
                Utility.IDFLineFormatter("Space Infiltration Schedule", "Schedule Name"),
                Utility.IDFLineFormatter("AirChanges/Hour", "Design Flow Rate Calculation Method"),
                Utility.IDFLineFormatter("", "Design Flow Rate { m3 / s}"),
                Utility.IDFLineFormatter("", "Flow per Zone Floor Area { m3 / s - m2}"),
                Utility.IDFLineFormatter("", "Flow per Exterior Surface Area { m3 / s - m2}"),
                Utility.IDFLineFormatter(airChangesHour, "Air Changes per Hour { 1 / hr}"),
                Utility.IDFLineFormatter(constantTermCoeff, "Constant Term Coefficient"),
                Utility.IDFLineFormatter(temperatureTermCoef, "Temperature Term Coefficient"),
                Utility.IDFLineFormatter(velocityTermCoef, "!-Velocity Term Coefficient"),
                Utility.IDFLastLineFormatter(velocitySquaredTermCoef, "Velocity Squared Term Coefficient")
            };
        }
    }
}
