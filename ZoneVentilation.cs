using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ZoneVentilation
    {
        public string Name;
        public string ZoneName;
        public string scheduleName = "Ventilation Schedule";
        public string CalculationMethod = "AirChanges/Hour";
        public double DesignFlowRate = 0;
        public double FlowRateZoneArea = 0.001;
        public double FlowRatePerson = 0.00944;
        public double airChangesHour = 0;
        public string VentilationType = "Balanced";
        public double FanPressure = 1;
        public double FanEfficiency = 1;
        public double ConstantCoefficient = 1;
        public double TemperatureCoefficient = 0;
        public double VelocityCoefficient = 0;
        public double VelocitySqCoefficient = 0;
        public double minIndoorTemp = -100;
        public double maxIndoorTemp = 100;
        public string minIndoorTempSchedule = " ";
        public string maxIndoorTempSchedule = " ";
        public double deltaC = 1;
        public ZoneVentilation(double acH)
        {
            scheduleName = "Ventilation Schedule";
            airChangesHour = acH;
        }
        public ZoneVentilation()
        {

        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "ZoneVentilation:DesignFlowRate,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(ZoneName, "Zone or ZoneList Name"),
                Utility.IDFLineFormatter(scheduleName, "Schedule Name"),
                Utility.IDFLineFormatter(CalculationMethod, "Design Flow Rate Calculation Method"),
                Utility.IDFLineFormatter(DesignFlowRate, "Design Flow Rate {m3/s}"),
                Utility.IDFLineFormatter(FlowRateZoneArea, "Flow Rate per Zone Floor Area {m3/s-m2}"),
                Utility.IDFLineFormatter(FlowRatePerson, "Flow Rate per Person {m3/s-person}"),
                Utility.IDFLineFormatter(airChangesHour, "Air Changes per Hour {1/hr}"),
                Utility.IDFLineFormatter(VentilationType, "Ventilation Type"),
                Utility.IDFLineFormatter(FanPressure, "Fan Pressure Rise {Pa}"),
                Utility.IDFLineFormatter(FanEfficiency, "Fan Total Efficiency"),
                Utility.IDFLineFormatter(ConstantCoefficient, "Constant Term Coefficient"),
                Utility.IDFLineFormatter(TemperatureCoefficient, "Temperature Term Coefficient"),
                Utility.IDFLineFormatter(VelocityCoefficient, "Velocity Term Coefficient"),
                Utility.IDFLineFormatter(VelocitySqCoefficient, "Velocity Squared Term Coefficient"),
                Utility.IDFLineFormatter(minIndoorTemp, "Minimum Indoor Temperature {C}"),
                Utility.IDFLineFormatter(minIndoorTempSchedule, "Maximum Indoor Temperature Schedule"),
                Utility.IDFLineFormatter(maxIndoorTemp, "Maximum Indoor Temperature {C}"),
                Utility.IDFLineFormatter(maxIndoorTempSchedule, "Maximum Indoor Temperature Schedule"),
                Utility.IDFLastLineFormatter(deltaC, "Delta Temperature { deltaC}")
            };
        }
    }
}
