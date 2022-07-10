using DocumentFormat.OpenXml.Drawing;
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
        public List<string> ZoneNames = new List<string>();
        public ZoneConditions Conditions;

        public People People;
        public ZoneVentilation ZoneVentilation;
        public ZoneVentilation ZoneVentilationNatural;
        public ZoneInfiltration ZoneInfiltration;
        public Light Light;
        public ElectricEquipment ElectricEquipment;
        public Thermostat Thermostat;
        public List<ScheduleCompact> Schedules = new List<ScheduleCompact>();
        public string Name { get; set; }
        public ZoneList() { }
        public ZoneList(string name)
        {
            Name = name;
        }
        void CreateZoneSchedules()
        {
            Schedules = new List<ScheduleCompact>();
            int[] vals= Conditions.GetStartEndTime(13);
            int hour1 = vals[0], minutes1 = vals[1], hour2 = vals[2], minutes2 = vals[3];
            float[] heatingSetPoints = new float[]
            {
                Conditions.HeatingSetpoint,
                Conditions.HeatingSetpoint - 5
            };

            float[] coolingSetPoints = new float[]
            {
                Conditions.CoolingSetpoint,
                Conditions.CoolingSetpoint + 5
            };

            float heatingSetpoint1 = heatingSetPoints[0];
            float heatingSetpoint2 = heatingSetPoints[1];

            float coolingSetpoint1 = coolingSetPoints[0];
            float coolingSetpoint2 = coolingSetPoints[1];

            //60 minutes earlier
            int hour1b = hour1 - 1;
            int minutes1b = minutes1;

            Dictionary<string, Dictionary<string, float>> heatSP = new Dictionary<string, Dictionary<string, float>>(),
                coolSP = new Dictionary<string, Dictionary<string, float>>(),
                heatSP18 = new Dictionary<string, Dictionary<string, float>>(),
                occupancyS = new Dictionary<string, Dictionary<string, float>>(),
                ventilS = new Dictionary<string, Dictionary<string, float>>(),
                leHeatGain = new Dictionary<string, Dictionary<string, float>>();

            string days1 = "WeekDays SummerDesignDay WinterDesignDay CustomDay1 CustomDay2";
            string days2 = "Weekends Holiday";

            Dictionary<string, float> heatSPV1 = new Dictionary<string, float>(), heatSPV2 = new Dictionary<string, float>();
            heatSPV1.Add(hour1b + ":" + minutes1b, heatingSetpoint2);
            heatSPV1.Add(hour2 + ":" + minutes2, heatingSetpoint1);
            heatSPV1.Add("24:00", heatingSetpoint2);
            heatSP.Add(days1, heatSPV1);
            heatSPV2.Add("24:00", heatingSetpoint2);
            heatSP.Add(days2, heatSPV2);
            ScheduleCompact heatingSP = new ScheduleCompact()
            {
                name = Name + "_HeatingSetPointSchedule",
                daysTimeValue = heatSP
            };

            Dictionary<string, float> coolSPV1 = new Dictionary<string, float>(), coolSPV2 = new Dictionary<string, float>();
            coolSPV1.Add(hour1b + ":" + minutes1b, coolingSetpoint2);
            coolSPV1.Add(hour2 + ":" + minutes2, coolingSetpoint1);
            coolSPV1.Add("24:00", coolingSetpoint2);
            coolSP.Add(days1, coolSPV1);
            coolSPV2.Add("24:00", coolingSetpoint2);
            coolSP.Add(days2, coolSPV2);
            ScheduleCompact coolingSP = new ScheduleCompact()
            {
                name = Name + "_CoolingSetPointSchedule",
                daysTimeValue = coolSP
            };

            Dictionary<string, float> occupV1 = new Dictionary<string, float>(), occupV2 = new Dictionary<string, float>();
            occupV1.Add(hour1 + ":" + minutes1, 0);
            occupV1.Add(hour2 + ":" + minutes2, 1);
            occupV1.Add("24:00", 0);
            occupancyS.Add(days1, occupV1);
            occupV2.Add("24:00", 0);
            occupancyS.Add(days2, occupV2);
            ScheduleCompact occupSchedule = new ScheduleCompact()
            {
                name = Name + "_OccupancySchedule",
                daysTimeValue = occupancyS
            };

            Dictionary<string, float> ventilV1 = new Dictionary<string, float>(), ventilV2 = new Dictionary<string, float>();
            ventilV1.Add(hour1 + ":" + minutes1, 0);
            ventilV1.Add(hour2 + ":" + minutes2, 1);
            ventilV1.Add("24:00", 0);
            ventilS.Add(days1, ventilV1);
            ventilV2.Add("24:00", 0);
            ventilS.Add(days2, ventilV2);
            ScheduleCompact ventilSchedule = new ScheduleCompact()
            {
                name = Name + "_VentilationSchedule",
                daysTimeValue = ventilS
            };
            
            float equipOffsetFraction = .1f;
            Dictionary<string, float> lehgV1 = new Dictionary<string, float>(), lehgV2 = new Dictionary<string, float>();
            lehgV1.Add(hour1 + ":" + minutes1, equipOffsetFraction);
            lehgV1.Add(hour2 + ":" + minutes2, 1);
            lehgV1.Add("24:00", equipOffsetFraction);
            leHeatGain.Add(days1, lehgV1);
            lehgV2.Add("24:00", equipOffsetFraction);
            leHeatGain.Add(days2, lehgV2);
            ScheduleCompact lSchedule = new ScheduleCompact()
            {
                name = Name + "_LightSchedule",
                daysTimeValue = leHeatGain
            };

            ScheduleCompact eSchedule = new ScheduleCompact()
            {
                name = Name + "_EquipmentSchedule",
                daysTimeValue = leHeatGain
            };
           
            ScheduleCompact activity = new ScheduleCompact()
            {
                name = Name + "_ActivitySchedule",
                daysTimeValue = new Dictionary<string, Dictionary<string, float>>() {
                    { "AllDays", new Dictionary<string, float>() {{"24:00", 125} } } }
            };

            Schedules.Add(heatingSP);
            Schedules.Add(coolingSP);
            Schedules.Add(occupSchedule);
            Schedules.Add(ventilSchedule);
            Schedules.Add(lSchedule);
            Schedules.Add(eSchedule);
            Schedules.Add(activity);
        }
        public void UpdateDayLightControlSchedule(Building building)
        {
            List<Zone> zones = building.zones.Where(z => ZoneNames.Contains(z.Name)).ToList();
            zones.Where(z => z.DayLightControl != null).ToList().ForEach(z => z.DayLightControl.AvailabilitySchedule = Schedules.First(s=>s.name.Contains("Occupancy")).name);
        }
        public void CreateVentialtionNatural(float coolingSP)
        {
            ZoneVentilationNatural = new ZoneVentilation()
            {
                Name = "NaturalVentilation_" + Name,
                ZoneName = Name,
                VentilationType = "Natural",
                airChangesHour = 2,
                minIndoorTemp = 23,
                maxIndoorTemp = coolingSP-.1f,
                scheduleName = Schedules.First(s => s.name.Contains("Ventilation")).name,
            };
        }
        public void GeneratePeopleLightEquipmentVentilationInfiltrationThermostat(Building building)
        {
            Conditions = building.Parameters.ZConditions.First(o => o.Name == Name);

            CreateZoneSchedules();
            
            People = new People(Conditions.Occupancy)
            {
                Name = "People_" + Name,
                ZoneName = Name,
                scheduleName = Schedules.First(s=>s.name.Contains("Occupancy")).name,
                activityLvlSchedName = Schedules.First(s => s.name.Contains("Activity")).name
            };
            if (building.Parameters.Service.HVACSystem != HVACSystem.HeatPumpWBoiler)
            {
                ZoneVentilation = new ZoneVentilation()
                {
                    Name = "Ventilation_" + Name,
                    ZoneName = Name,
                    scheduleName = Schedules.First(s => s.name.Contains("Ventilation")).name,
                    CalculationMethod = "Flow/Person"
                };
            }
            Light = new Light(Conditions.LHG)
            {
                Name = "Light_" + Name,
                ZoneName = Name,
                scheduleName = Schedules.First(s => s.name.Contains("Light")).name
            };
            ElectricEquipment = new ElectricEquipment(Conditions.EHG)
            {
                Name = "Equipment_" + Name,
                ZoneName = Name,
                scheduleName = Schedules.First(s => s.name.Contains("Equipment")).name
            };
            Thermostat = new Thermostat()
            {
                name = Name + "_Thermostat",
                ScheduleHeating = Schedules.First(s => s.name.Contains("Heating")),
                ScheduleCooling = Schedules.First(s => s.name.Contains("Cooling"))
            };
            ZoneInfiltration = new ZoneInfiltration(building.Parameters.Construction.Infiltration)
            {
                Name = "Infiltration_" + Name,
                ZoneName = Name
            };
        }
    }
}