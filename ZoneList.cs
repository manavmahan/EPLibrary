using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ZoneList
    {
        public List<string> zoneNames;
        public People People;
        public ZoneVentilation ZoneVentilation;
        public ZoneInfiltration ZoneInfiltration;
        public Light Light;
        public ElectricEquipment ElectricEquipment;
        public Thermostat Thermostat;
        public Dictionary<string, ScheduleCompact> Schedules = new Dictionary<string, ScheduleCompact>();
        public string name { get; set; }
        public ZoneList() { }
        public ZoneList(string n)
        {
            name = n;
            zoneNames = new List<string>();
        }
        void CreateZoneSchedules(double startTime, double endTime)
        {
            Schedules = new Dictionary<string, ScheduleCompact>();
            int hour1, hour2, minutes1, minutes2;
            hour1 = (int)Math.Truncate(startTime);
            hour2 = (int)Math.Truncate(endTime);
            minutes1 = (int)Math.Round(Math.Round((startTime - hour1) * 6)) * 10;
            minutes2 = (int)Math.Round(Math.Round((endTime - hour2) * 6)) * 10;

            double[] heatingSetPoints = new double[] { 10, 20 };
            double[] coolingSetPoints = new double[] { 28, 24 };

            double heatingSetpoint1 = heatingSetPoints[0];//16;
            double heatingSetpoint2 = heatingSetPoints[1];//20;

            double coolingSetpoint1 = coolingSetPoints[0];//28;
            double coolingSetpoint2 = coolingSetPoints[1];//25;

            //60 minutes earlier
            int hour1b = hour1 - 1;
            int minutes1b = minutes1;

            Dictionary<string, Dictionary<string, double>> heatSP = new Dictionary<string, Dictionary<string, double>>(),
                coolSP = new Dictionary<string, Dictionary<string, double>>(),
                heatSP18 = new Dictionary<string, Dictionary<string, double>>(),
                occupancyS = new Dictionary<string, Dictionary<string, double>>(),
                ventilS = new Dictionary<string, Dictionary<string, double>>(),
                leHeatGain = new Dictionary<string, Dictionary<string, double>>();

            string days1 = "WeekDays SummerDesignDay WinterDesignDay CustomDay1 CustomDay2";
            string days2 = "Weekends Holiday";

            Dictionary<string, double> heatSPV1 = new Dictionary<string, double>(), heatSPV2 = new Dictionary<string, double>();
            heatSPV1.Add(hour1b + ":" + minutes1b, heatingSetpoint1);
            heatSPV1.Add(hour2 + ":" + minutes2, heatingSetpoint2);
            heatSPV1.Add("24:00", heatingSetpoint1);
            heatSP.Add(days1, heatSPV1);
            heatSPV2.Add("24:00", heatingSetpoint1);
            heatSP.Add(days2, heatSPV2);
            ScheduleCompact heatingSP = new ScheduleCompact()
            {
                name = name + "_Heating Set Point Schedule",
                daysTimeValue = heatSP
            };
            Schedules.Add("HeatingSP", heatingSP);

            Dictionary<string, double> coolSPV1 = new Dictionary<string, double>(), coolSPV2 = new Dictionary<string, double>();
            coolSPV1.Add(hour1b + ":" + minutes1b, coolingSetpoint1);
            coolSPV1.Add(hour2 + ":" + minutes2, coolingSetpoint2);
            coolSPV1.Add("24:00", coolingSetpoint1);
            coolSP.Add(days1, coolSPV1);
            coolSPV2.Add("24:00", coolingSetpoint1);
            coolSP.Add(days2, coolSPV2);
            ScheduleCompact coolingSP = new ScheduleCompact()
            {
                name = name + "_Cooling Set Point Schedule",
                daysTimeValue = coolSP
            };
            Schedules.Add("CoolingSP", coolingSP);

            Dictionary<string, double> occupV1 = new Dictionary<string, double>(), occupV2 = new Dictionary<string, double>();
            occupV1.Add(hour1 + ":" + minutes1, 0);
            occupV1.Add(hour2 + ":" + minutes2, 1);
            occupV1.Add("24:00", 0);
            occupancyS.Add(days1, occupV1);
            occupV2.Add("24:00", 0);
            occupancyS.Add(days2, occupV2);
            ScheduleCompact occupSchedule = new ScheduleCompact()
            {
                name = name + "_Occupancy Schedule",
                daysTimeValue = occupancyS
            };
            Schedules.Add("Occupancy", occupSchedule);

            Dictionary<string, double> ventilV1 = new Dictionary<string, double>(), ventilV2 = new Dictionary<string, double>();
            ventilV1.Add(hour1 + ":" + minutes1, 0);
            ventilV1.Add(hour2 + ":" + minutes2, 1);
            ventilV1.Add("24:00", 0);
            ventilS.Add(days1, ventilV1);
            ventilV2.Add("24:00", 0);
            ventilS.Add(days2, ventilV2);
            ScheduleCompact ventilSchedule = new ScheduleCompact()
            {
                name = name + "_Ventilation Schedule",
                daysTimeValue = ventilS
            };
            Schedules.Add("Ventilation", ventilSchedule);

            double equipOffsetFraction = .1;
            Dictionary<string, double> lehgV1 = new Dictionary<string, double>(), lehgV2 = new Dictionary<string, double>();
            lehgV1.Add(hour1 + ":" + minutes1, equipOffsetFraction);
            lehgV1.Add(hour2 + ":" + minutes2, 1);
            lehgV1.Add("24:00", equipOffsetFraction);
            leHeatGain.Add(days1, lehgV1);
            lehgV2.Add("24:00", equipOffsetFraction);
            leHeatGain.Add(days2, lehgV2);
            ScheduleCompact lSchedule = new ScheduleCompact()
            {
                name = name + "_Lighting Schedule",
                daysTimeValue = leHeatGain
            };
            Schedules.Add("Light", lSchedule);

            ScheduleCompact eSchedule = new ScheduleCompact()
            {
                name = name + "_Electric Equipment Schedule",
                daysTimeValue = leHeatGain
            };
            Schedules.Add("Equipment", eSchedule);

            ScheduleCompact activity = new ScheduleCompact()
            {
                name = name + "_People Activity Schedule",
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 125} } } }
            };
            Schedules.Add("Activity", activity);
        }
        public void GeneratePeopleLightEquipmentVentilationInfiltrationThermostat(Building building, double startTime, double endTime, double areaPerPerson, double lHG, double eHG, double infil)
        {
            CreateZoneSchedules(startTime, endTime);
            List<Zone> zones = building.zones.Where(z=>zoneNames.Contains(z.Name)).ToList();
            zones.Where(z => z.DayLightControl != null).ToList().ForEach(z => z.DayLightControl.AvailabilitySchedule = Schedules["Occupancy"].name);
            People = new People(areaPerPerson)
            {
                Name = "People_" + name,
                ZoneName = name,
                scheduleName = Schedules["Occupancy"].name,
                activityLvlSchedName = Schedules["Activity"].name
            };
            ZoneVentilation = new ZoneVentilation()
            {
                Name = "Ventilation_" + name,
                ZoneName = name,
                scheduleName = Schedules["Ventilation"].name,
                CalculationMethod = "Flow/Person"
            };

            Light = new Light(lHG)
            {
                Name = "Light_" + name,
                ZoneName = name,
                scheduleName = Schedules["Light"].name
            };
            ElectricEquipment = new ElectricEquipment(eHG)
            {
                Name = "Equipment_" + name,
                ZoneName = name,
                scheduleName = Schedules["Equipment"].name
            };
            Thermostat = new Thermostat()
            {
                name = name + "_Thermostat",
                ScheduleHeating = Schedules["HeatingSP"],
                ScheduleCooling = Schedules["CoolingSP"]
            };
            ZoneInfiltration = new ZoneInfiltration(infil)
            {
                Name = "Infiltration_" + name,
                ZoneName = name
            };
        }
    }
}
