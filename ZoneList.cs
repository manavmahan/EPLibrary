﻿using System;
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
        public BuildingZoneEnvironment Environment;
        public BuildingZoneOccupant Occupant;
        public BuildingZoneOperation Operation;

        public People People;
        public ZoneVentilation ZoneVentilation;
        public ZoneInfiltration ZoneInfiltration;
        public Light Light;
        public ElectricEquipment ElectricEquipment;
        public Thermostat Thermostat;
        public List<ScheduleCompact> Schedules;
        public string Name { get; set; }
        public ZoneList() { }
        public ZoneList(string name)
        {
            Name = name;
        }
        void CreateZoneSchedules(Building building, double startTime, double endTime)
        {
            Schedules = new List<ScheduleCompact>();
            int hour1, hour2, minutes1, minutes2;
            hour1 = (int)Math.Truncate(startTime);
            hour2 = (int)Math.Truncate(endTime);

            minutes1 = (int)Math.Round(Math.Round((startTime - hour1) * 6)) * 10;
            minutes2 = (int)Math.Round(Math.Round((endTime - hour2) * 6)) * 10;

            if (minutes1 == 60)
            {
                minutes1 = 0; hour1++;
            }
            if(minutes2 == 60)
            {
                minutes2 = 0; hour2++;
            }

            double[] heatingSetPoints = new double[]
            {
                building.Parameters.Environments[0].HeatingSetPoint,
                building.Parameters.Environments[0].HeatingSetPoint -5 
            };

            double[] coolingSetPoints = new double[]
            {
                building.Parameters.Environments[0].CoolingSetPoint,
                building.Parameters.Environments[0].HeatingSetPoint + 5
            };

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
                name = Name + "_Heating Set Point Schedule",
                daysTimeValue = heatSP
            };
            Schedules.Add(heatingSP);

            Dictionary<string, double> coolSPV1 = new Dictionary<string, double>(), coolSPV2 = new Dictionary<string, double>();
            coolSPV1.Add(hour1b + ":" + minutes1b, coolingSetpoint1);
            coolSPV1.Add(hour2 + ":" + minutes2, coolingSetpoint2);
            coolSPV1.Add("24:00", coolingSetpoint1);
            coolSP.Add(days1, coolSPV1);
            coolSPV2.Add("24:00", coolingSetpoint1);
            coolSP.Add(days2, coolSPV2);
            ScheduleCompact coolingSP = new ScheduleCompact()
            {
                name = Name + "_Cooling Set Point Schedule",
                daysTimeValue = coolSP
            };
            Schedules.Add(coolingSP);

            Dictionary<string, double> occupV1 = new Dictionary<string, double>(), occupV2 = new Dictionary<string, double>();
            occupV1.Add(hour1 + ":" + minutes1, 0);
            occupV1.Add(hour2 + ":" + minutes2, 1);
            occupV1.Add("24:00", 0);
            occupancyS.Add(days1, occupV1);
            occupV2.Add("24:00", 0);
            occupancyS.Add(days2, occupV2);
            ScheduleCompact occupSchedule = new ScheduleCompact()
            {
                name = Name + "_Occupancy Schedule",
                daysTimeValue = occupancyS
            };
            Schedules.Add(occupSchedule);

            Dictionary<string, double> ventilV1 = new Dictionary<string, double>(), ventilV2 = new Dictionary<string, double>();
            ventilV1.Add(hour1 + ":" + minutes1, 0);
            ventilV1.Add(hour2 + ":" + minutes2, 1);
            ventilV1.Add("24:00", 0);
            ventilS.Add(days1, ventilV1);
            ventilV2.Add("24:00", 0);
            ventilS.Add(days2, ventilV2);
            ScheduleCompact ventilSchedule = new ScheduleCompact()
            {
                name = Name + "_Ventilation Schedule",
                daysTimeValue = ventilS
            };
            Schedules.Add(ventilSchedule);

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
                name = Name + "_Lighting Schedule",
                daysTimeValue = leHeatGain
            };
            Schedules.Add(lSchedule);

            ScheduleCompact eSchedule = new ScheduleCompact()
            {
                name = Name + "_Electric Equipment Schedule",
                daysTimeValue = leHeatGain
            };
            Schedules.Add(eSchedule);

            ScheduleCompact activity = new ScheduleCompact()
            {
                name = Name + "_People Activity Schedule",
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 125} } } }
            };
            Schedules.Add(activity);
        }
        public void UpdateDayLightControlSchedule(Building building)
        {
            List<Zone> zones = building.zones.Where(z => ZoneNames.Contains(z.Name)).ToList();
            zones.Where(z => z.DayLightControl != null).ToList().ForEach(z => z.DayLightControl.AvailabilitySchedule = Schedules.First(s=>s.name.Contains("Occupancy")).name);
        }
        public void GeneratePeopleLightEquipmentVentilationInfiltrationThermostat(Building building, double[] time, double areaPerPerson, double lHG, double eHG, double infil)
        {
            double startTime = time[0], endTime = time[1];
            CreateZoneSchedules(building, startTime, endTime);
            
            People = new People(areaPerPerson)
            {
                Name = "People_" + Name,
                ZoneName = Name,
                scheduleName = Schedules.First(s=>s.name.Contains("Occupancy")).name,
                activityLvlSchedName = Schedules.First(s => s.name.Contains("Activity")).name
            };
            ZoneVentilation = new ZoneVentilation()
            {
                Name = "Ventilation_" + Name,
                ZoneName = Name,
                scheduleName = Schedules.First(s => s.name.Contains("Ventilation")).name,
                CalculationMethod = "Flow/Person"
            };

            Light = new Light(lHG)
            {
                Name = "Light_" + Name,
                ZoneName = Name,
                scheduleName = Schedules.First(s => s.name.Contains("Light")).name
            };
            ElectricEquipment = new ElectricEquipment(eHG)
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
            ZoneInfiltration = new ZoneInfiltration(infil)
            {
                Name = "Infiltration_" + Name,
                ZoneName = Name
            };
        }
    }
}