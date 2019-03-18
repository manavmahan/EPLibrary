using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace IDFFile
{
    public enum SurfaceType { Floor, Ceiling, Wall, Roof, InternalWall, Window };
    public enum Direction { North, East, South, West};
    public static class Utility
    {
        public static int[] hourToHHMM(double hours)
        {
            double h = hours / 2;
            double h1 = 13 - h;
            double h2 = 13 + h;

            int hour1 = int.Parse(Math.Truncate(h1).ToString());
            int min1 = int.Parse((Math.Round(Math.Round((h1 - hour1) * 6)) * 10).ToString());
            int hour2 = int.Parse(Math.Truncate(h2).ToString());
            int min2 = int.Parse((Math.Round(Math.Round((h2 - hour2) * 6)) * 10).ToString());

            if (min1 == 60)
            {
                hour1++;
                min1 = 0;
            }
            if (min2 == 60)
            {
                hour2++;
                min2 = 0;
            }
            return (new int[4] { hour1, min1, hour2, min2 });
        }
        public static List<string> ReplaceLastComma(this List<string> info)
        {
            string lastLine = info.Last();
            string[] splitLine = lastLine.Split(',');

            string joinedLine = string.Join(",", splitLine.Take(splitLine.Count()-1));
            joinedLine = joinedLine + ";" + splitLine.Last();
            info[info.Count - 1] = joinedLine;
            return info;
        }
        public static XYZ Transform(this XYZ xyz, double angle)
        {
            double x1 = xyz.X * Math.Cos(angle) - xyz.Y * Math.Sin(angle);
            double y1 = xyz.X * Math.Sin(angle) + xyz.Y * Math.Cos(angle);
            return new XYZ(x1, y1, xyz.Z);
        }
        public static XYZ OffsetHeight(this XYZ xyz, double height)
        {
            return (new XYZ(xyz.X, xyz.Y, xyz.Z + height));
        }

        public static XYZ Copy(this XYZ xyz)
        {
            return new XYZ(xyz.X, xyz.Y, xyz.Z);
        }
        public static string IDFLineFormatter (object attribute, string definition)
        {
            //Console.WriteLine(attribute.ToString() + ",\t\t\t\t\t\t ! - " + definition);
            //Console.ReadKey();
            if (attribute != null) { return (attribute.ToString() + ",\t\t\t\t\t\t ! - " + definition); }
            else { return (",\t\t\t\t\t\t ! - " + definition); }
        }
        public static string IDFLastLineFormatter(object attribute, string definition)
        {
            return (attribute.ToString() + ";\t\t\t\t\t\t ! - " + definition);
        }
        public static List<SizingPeriodDesignDay> CreateDesignDays(string place)
        {
            switch (place)
            {
                case "MUNICH_DEU":
                default:
                    SizingPeriodDesignDay winterday = new SizingPeriodDesignDay("MUNICH Ann Htg 99.6% Condns DB", 2, 21, "WinterDesignDay", -12.8, 0.0, -13.9, 0.0, 95900.0, 1.0, 130.0, "No", "No", "No", "AshraeClearSky", 0.0);
                    SizingPeriodDesignDay summerday = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns Enth=>MDB", 7, 21, "SummerDesignDay", 31.5, 10.9, 17.8, 0.0, 95300.0, 1.5, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);
                    SizingPeriodDesignDay summerday1 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 6)", 6, 21, "SummerDesignDay", 29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);
                    SizingPeriodDesignDay summerday2 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 7)", 7, 21, "SummerDesignDay", 29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);
                    SizingPeriodDesignDay summerday3 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 8)", 8, 21, "SummerDesignDay", 29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);
                    SizingPeriodDesignDay summerday4 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 9)", 9, 21, "SummerDesignDay", 29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);
                    return new List<SizingPeriodDesignDay>() { winterday, summerday, summerday1, summerday2, summerday3, summerday4 };
            }
        }
    }
    [Serializable]
    public class IDFFile
    {
        public string name = "IDFFile_0";
        public string WeatherLocation = "MUNICH_DEU";

        //IDF Objects as appear in Energy Plus File
        public Version version = new Version();
        public SimulationControl sControl = new SimulationControl();
        public Timestep tStep = new Timestep(6);
        public ConvergenceLimits cLimits = new ConvergenceLimits();
        public SiteLocation sLocation = new SiteLocation("MUNICH_DEU");
        public List<SizingPeriodDesignDay> SDesignDay = Utility.CreateDesignDays("MUNICH_DEU");
        public RunPeriod rPeriod = new RunPeriod();
        public SiteGroundTemperature gTemperature = new SiteGroundTemperature("MUNICH_DEU");
        public GlobalGeometryRules geomRules = new GlobalGeometryRules();

        //Building - contain schedules, material, constructions, zones, zoneLists, 
        public Building building = new Building();

        public Output output;

        public IDFFile(){ }
        public IDFFile deepCopy(string name)
        {
            if (!typeof(IDFFile).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(this, null))
            {
                return this;
            }
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                IDFFile other = (IDFFile)formatter.Deserialize(stream);
                other.name = name;
                return other;
            }
        }
        
        public List<string> WriteFile()
        {
            //Version, Simulation Control, Building, TimeStep, ConvergenceLimits, Site:Location, SizingPeriod, RunPeriod, GroundTemperature
            //GlobalGeometryRules
            //ScheduleLimits, Schedule
            //Material, WindowMaterial, Construction
            //Zone, ZoneList, BuildingSurface
            //People, Light, ElectricEquipment, Infiltration, Ventillation
            //Thermostat, HVACZone, HWLoop, CWLoop, Boiler, Chiller, Tower
            List<string> info = new List<string>();
            info.AddRange(version.WriteInfo());
            info.AddRange(sControl.WriteInfo());
            info.AddRange(building.WriteInfo());
            info.AddRange(tStep.WriteInfo());
            info.AddRange(cLimits.WriteInfo());
            info.AddRange(sLocation.WriteInfo());
            SDesignDay.ForEach(s => info.AddRange(s.WriteInfo()));
            info.AddRange(rPeriod.WriteInfo());
            info.AddRange(gTemperature.WriteInfo());
            info.AddRange(geomRules.WriteInfo());

            building.schedulelimits.ForEach(s => info.AddRange(s.WriteInfo()));
            building.schedulescomp.ForEach(s => info.AddRange(s.WriteInfo()));

            info.AddRange(writeMaterial());
            info.AddRange(writeWindowMaterial());
            info.AddRange(writeConstruction());
            info.AddRange(writeZone());
            info.AddRange(writeZoneList()); 
            info.AddRange(writeBuildingSurfaceList());
            info.AddRange(writeFenestrationSurfaceList());
            info.AddRange(writeShading());

            info.AddRange(WriteDayLightControl());
            info.AddRange(writePeople());
            info.AddRange(writeLights());
            info.AddRange(writeElectricEquipment()); info.AddRange(writeZoneInfiltration()); info.AddRange(writeZoneVentilation());
            info.AddRange(writeHVACTemplate());

            info.AddRange(output.writeInfo());
            return info;
        }
        private List<string> WriteDayLightControl()
        {
            List<string> info = new List<string>();
            if (building.zones.Where(z => z.DayLightControl != null).Count() != 0)
            {
                building.zones.ForEach(z => z.DayLightControl.ReferencePoints.ForEach(p => info.AddRange(p.WriteInfo())));
                building.zones.ForEach(z => info.AddRange(z.DayLightControl.WriteInfo()));
            }
            return info;
        }

        private List<string> writeMaterial()
        {
            List<string> info = new List<string>();
            info.Add("!-   =========== ALL OBJECTS IN CLASS: MATERIAL ===========");
            foreach (Material l in building.materials)
            {
                info.AddRange(l.writeInfo());
            }
            return info;
        }
        private List<string> writeWindowMaterial()
        {
            List<string> info = new List<string>();
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: WINDOWMATERIAL:SIMPLEGLAZINGSYSTEM ===========");
            building.windowMaterials.ForEach(wm => info.AddRange(wm.writeInfo()));
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: WINDOWMATERIAL:SHADING ===========");
            building.windowMaterialShades.ForEach(sh => info.AddRange(sh.writeInfo()));
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: SHADINGCONTROL ===========");
            building.shadingControls.ForEach(shc => info.AddRange(shc.WriteInfo()));
            return info;
        }
        private List<string> writeConstruction()
        {
            List<string> info = new List<string>();
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: CONSTRUCTION ===========");
            building.constructions.ForEach(c=> info.AddRange(c.WriteInfo()));
            return info;
        }
        private List<string> writeZone()
        {
            List<string> info = new List<string>();
            info.Add("\r!-   ===========  ALL OBJECTS IN CLASS: ZONE ===========");
            building.zones.ForEach(z => info.Add("Zone,\r\t" + z.name + ";\t\t\t\t\t\t!-Name"));        
            return info;
        }
        private List<string> writeZoneList()
        {
            List<string> idfString = new List<string>();

            idfString.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ZONELIST ===========\r\n");

            foreach (ZoneList zl in building.zoneLists)
            {
                idfString.Add("ZoneList,");
                idfString.Add(Utility.IDFLineFormatter(zl.name, "Name"));

                foreach (Zone z in zl.listZones)
                {
                    idfString.Add(Utility.IDFLineFormatter(z.name, "Zone " + (zl.listZones.IndexOf(z) + 1) + " Name"));
                }
                idfString.ReplaceLastComma();
            }          
            return idfString;
        }
        private List<String> writeBuildingSurfaceList()
        {
            List<String> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: BUILDINGSURFACE:DETAILED ===========\r\n");
            foreach (Zone z in building.zones)
            {
                foreach (BuildingSurface bSur in z.surfaces)
                {
                    info.AddRange(bSur.surfaceInfo());
                }
            }
            return info;
        }
        private List<String> writeFenestrationSurfaceList()
        {
            List<String> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: FENESTRATIONSURFACE:DETAILED ===========\r\n");
            foreach (Zone z in building.zones)
            {
                foreach (BuildingSurface bSur in z.surfaces)
                {
                    if (bSur.fenestrations != null)
                    {
                        foreach (Fenestration fen in bSur.fenestrations)
                        {
                            info.AddRange(fen.WriteInfo());
                        }
                    }
                }
            }
            return info;
        }
        private List<String> writePeople()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: PEOPLE ===========\r\n");
            foreach (Zone z in building.zones)
            {
                People p = z.people;
                info.Add("People,");
                info.Add("\tPeople " + z.name + ",\t\t\t\t\t\t!- Name");
                info.Add("\t" + z.name + ",\t\t\t\t\t\t!- Zone or ZoneList Name");
                info.Add("\t" + p.scheduleName + ",\t\t\t\t\t\t!- Number of People Schedule Name"); //sched
                info.Add("\t" + p.calculationMethod + ",\t\t\t\t\t\t!- Number of People Calculation Method");
                info.Add("\t" + ",\t\t\t\t\t\t!- Number of People");
                info.Add("\t" + ",\t\t\t\t\t\t!- People per Zone Floor Area {person/m2}");
                info.Add("\t" + p.areaPerPerson + ",\t\t\t\t\t\t!- Zone Floor Area per Person {m2/person}");
                info.Add("\t" + p.fractionRadiant + ",\t\t\t\t\t\t!- Fraction Radiant");
                info.Add("\t" + ",\t\t\t\t\t\t!- Sensible Heat Fraction ");
                info.Add("\t" + "People Activity Schedule" + ",\t\t\t\t\t\t!- Activity Level Schedule Name"); //sched
                info.Add("\t" + p.c02genRate + ",\t\t\t\t\t\t!- Carbon Dioxide Generation Rate {m3/s-W}");
                info.Add("\t" + ",\t\t\t\t\t\t!- Enable ASHRAE 55 Comfort Warnings");
                info.Add("\t" + p.meanRadiantTempCalcType + ",\t\t\t\t\t\t!- Mean Radiant Temperature Calculation Type");
                info.Add("\t" + ",\t\t\t\t\t\t!- Surface Name/Angle Factor List Name");
                info.Add("\t" + "Work Eff Sch" + ",\t\t\t\t\t\t!- Work Efficiency Schedule Name"); //sched
                info.Add("\t" + p.clothingInsulationCalcMeth + ",\t\t\t\t\t\t!- Clothing Insulation Calculation Method");
                info.Add("\t" + ",\t\t\t\t\t\t!- Clothing Insulation Calculation Method Schedule Name");
                info.Add("\t" + ",\t\t\t\t\t\t!- Clothing Insulation Schedule Name");
                info.Add("\t" + "Air Velo Sch" + ",\t\t\t\t\t\t!- Air Velocity Schedule Name"); //sched
                info.Add("\t" + p.thermalComfModel1t + ";\t\t\t\t\t\t!- Thermal Comfort Model 1 Type");
            }
            return info;
        }
        private List<string> writeZoneVentilation()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ZONEVENTILATION:DESIGNFLOWRATE ===========\r\n");
            foreach (Zone z in building.zones)
            {
                ZoneVentilation v = z.vent;
                info.Add("ZoneVentilation:DesignFlowRate,");
                info.Add("\tZoneVentilation-aim0014 " + z.name + ",\t\t\t\t\t\t!- Name");
                info.Add("\t" + z.name + ",\t\t\t\t\t\t!- Zone or ZoneList Name");
                info.Add("\t" + v.scheduleName + ",\t\t\t\t\t\t!- Schedule Name");
                info.Add("\tFlow/Person" + ",\t\t\t\t\t\t!- Design Flow Rate Calculation Method");
                info.Add("\t" + ",\t\t\t\t\t\t!- Design Flow Rate {m3/s}");
                info.Add("\t" + ",\t\t\t\t\t\t!- Flow Rate per Zone Floor Area {m3/s-m2}");
                info.Add("\t" + "0.00944,\t\t\t\t\t\t!- Flow Rate per Person {m3/s-person}");
                info.Add("\t" + ",\t\t\t\t\t\t!- Air Changes per Hour {1/hr}");
                info.Add("\tBalanced" + ",\t\t\t\t\t\t!- Ventilation Type");
                info.Add("\t0.0" + ",\t\t\t\t\t\t!- Fan Pressure Rise {Pa}");
                info.Add("\t1.0" + ",\t\t\t\t\t\t!- Fan Total Efficiency");
                info.Add("\t1" + ",\t\t\t\t\t\t!- Constant Term Coefficient");
                info.Add("\t0" + ",\t\t\t\t\t\t!- Temperature Term Coefficient");
                info.Add("\t0" + ",\t\t\t\t\t\t!- Velocity Term Coefficient");
                info.Add("\t0" + ",\t\t\t\t\t\t!- Velocity Squared Term Coefficient");
                info.Add("\t18.0" + ",\t\t\t\t\t\t!- Minimum Indoor Temperature {C}");
                info.Add("\t" + ",\t\t\t\t\t\t!- Minimum Indoor Temperature Schedule Name");
                info.Add("\t" + ",\t\t\t\t\t\t!- Maximum Indoor Temperature {C}");
                info.Add("\t" + ",\t\t\t\t\t\t!- Maximum Indoor Temperature Schedule Name");
                info.Add("\t1.0" + ";\t\t\t\t\t\t!- Delta Temperature {deltaC}");
            }
            return info;
        }
        private List<string> writeLights()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: LIGHTS ===========\r\n");
            foreach (Zone z in building.zones)
            {
                Light l = z.lights;
                info.Add("Lights,");
                info.Add("\tLights " + z.name + ",\t\t\t\t\t\t!- Name");
                info.Add("\t" + z.name + ",\t\t\t\t\t\t!- Zone or ZoneList Name");
                info.Add("\t" + l.scheduleName + ",\t\t\t\t\t\t!- Schedule Name");
                info.Add("\t" + l.designLevelCalcMeth + ",\t\t\t\t\t\t!- Design Level Calculation Method");
                info.Add("\t" + ",\t\t\t\t\t\t!- Lighting Level {W}");
                info.Add("\t" + l.wattsPerArea + "" + ",\t\t\t\t\t\t!- Watts per Zone Floor Area {W/m2}");
                info.Add("\t" + ",\t\t\t\t\t\t!- Watts per Person {W/person}");
                info.Add("\t" + l.returnAirFraction + ",\t\t\t\t\t\t!- Return Air Fraction");
                info.Add("\t" + l.fractionRadiant + ",\t\t\t\t\t\t!- Fraction Radiant");
                info.Add("\t" + l.fractionVisible + ",\t\t\t\t\t\t!- Fraction Visible");
                info.Add("\t" + ";\t\t\t\t\t\t!- Fraction Replaceable");
            }
            return info;
        }
        private List<string> writeElectricEquipment()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ELECTRICEQUIPMENT ===========\r\n");
            foreach (Zone z in building.zones)
            {
                ElectricEquipment e = z.equipment;
                info.Add("ElectricEquipment,");
                info.Add("\tElectric Equipment " + z.name + ",\t\t\t\t\t\t!- Name");
                info.Add("\t" + z.name + ",\t\t\t\t\t\t!- Zone or ZoneList Name");
                info.Add("\t" + e.scheduleName + ",\t\t\t\t\t\t!- Schedule Name");
                info.Add("\t" + e.designLevelCalcMeth + ",\t\t\t\t\t\t!- Design Level Calculation Method");
                info.Add("\t" + ",\t\t\t\t\t\t!- Design Level {W}");
                info.Add("\t" + e.wattsPerArea + "" + ",\t\t\t\t\t\t!- Watts per Zone Floor Area {W/m2}");
                info.Add("\t" + ",\t\t\t\t\t\t!- Watts per Person {W/person}");
                info.Add("\t" + ",\t\t\t\t\t\t!- Fraction Latent");
                info.Add("\t" + e.fractionRadiant + ",\t\t\t\t\t\t!- Fraction Radiant");
                info.Add("\t" + ";\t\t\t\t\t\t!- Fraction Lost");
            }
            return info;
        }
        private List<string> writeHVACThermostat()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: HVACTEMPLATE:THERMOSTAT ===========\r\n");

            foreach (Thermostat t in building.tStats)
            {
                info.Add("HVACTemplate:Thermostat,");
                info.Add("\t" + t.name + ", \t\t\t\t!- Name");
                info.Add("\t" + t.ScheduleHeating.name + ",\t\t\t\t!- Heating Setpoint Schedule Name");
                info.Add("\t" + ", \t\t\t\t!- Constant Heating Setpoint {C}");
                info.Add("\t" + t.ScheduleCooling.name + ",\t\t\t\t!- Cooling Setpoint Schedule Name");
                info.Add("\t" + "; \t\t\t\t!- Constant Cooling Setpoint {C}");
            }

            return info;
        }
        public List<String> writeHVACTemplate()
        {
            List<string> info = writeHVACThermostat();

            try
            {
                try{ building.zones.ForEach(z =>
                { ZoneFanCoilUnit zFCU = z.hvac as ZoneFanCoilUnit; info.AddRange(zFCU.writeInfo()); }); }
                catch { building.zones.ForEach(z => info.AddRange((z.hvac as ZoneVAV).writeInfo())); info.AddRange(building.vav.writeInfo()); }
                info.AddRange(building.cWaterLoop.writeInfo());
                info.AddRange(building.chiller.writeInfo());
                info.AddRange(building.tower.writeInfo());
                info.AddRange(building.hWaterLoop.writeInfo());
                info.AddRange(building.boiler.writeInfo()); 
            }
            catch
            {
                building.zones.ForEach(z => info.AddRange((z.hvac as ZoneIdealLoad).writeInfo()));
            }
            return info;
        }
        public List<String> writeZoneInfiltration()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ZONEINFILTRATION:DESIGNFLOWRATE ===========\r\n");

            foreach (Zone z in building.zones)
            {
                ZoneInfiltration i = z.infiltration;
                info.Add("ZoneInfiltration:DesignFlowRate,");
                info.Add("\tSpace Infiltration Design Flow Rate " + z.name + ",\t\t\t\t\t\t!- Name");
                info.Add("\t" + z.name + ",\t\t\t\t\t\t!- Zone or ZoneList Name");
                info.Add("\tSpace Infiltration Schedule, \t\t\t\t!- Schedule Name");
                info.Add("\tAirChanges/Hour,\t\t\t\t!-Design Flow Rate Calculation Method");
                info.Add("\t,\t\t\t\t!-Design Flow Rate { m3 / s}");
                info.Add("\t,\t\t\t\t!-Flow per Zone Floor Area { m3 / s - m2}");
                info.Add("\t,\t\t\t\t!-Flow per Exterior Surface Area { m3 / s - m2}");
                info.Add("\t" + i.airChangesHour + ",\t\t\t\t!-Air Changes per Hour { 1 / hr}");
                info.Add("\t" + i.constantTermCoeff + ",\t\t\t\t!-Constant Term Coefficient");
                info.Add("\t" + i.temperatureTermCoef + ",\t\t\t\t!-Temperature Term Coefficient");
                info.Add("\t" + i.velocityTermCoef + ",\t\t\t\t!-Velocity Term Coefficient");
                info.Add("\t" + i.velocitySquaredTermCoef + "; \t\t\t\t!-Velocity Squared Term Coefficient");
            }
            return info;
        }
        public List<string> writeShading()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: SHADING:ZONE:DETAILED ===========\r\n");

            try
            {
                info.AddRange((building.zones.SelectMany(z => z.surfaces.SelectMany(s => s.shading))).SelectMany(s => s.shadingInfo()));
            }
            catch { }
            
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: SHADING:OVERHANG:PROJECTION ===========\r\n");

            try
            {
                info.AddRange((building.zones.SelectMany(z => z.surfaces.SelectMany(s => s.fenestrations.Select(f=>f.overhang)))).SelectMany(s => s.OverhangInfo()));
            }
            catch { }

            return info;
        }
        public List<string> writeSchedules()
        {
            List<string> info = new List<string>();

            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: SCHEDULETYPELIMITS ===========\r\n");
            foreach (ScheduleLimits sched in building.schedulelimits)
            {
                info.AddRange(sched.WriteInfo());
            }

            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: SCHEDULE:COMPACT ===========\r\n");
            foreach (ScheduleCompact sched in building.schedulescomp)
            {
                info.AddRange(sched.WriteInfo());
            }

            return info;
        }

        public void GenerateOutput(bool heatFlow, string frequency)
        {
            Dictionary<string, string> outputvars = new Dictionary<string, string>();
            outputvars.Add("Zone Air System Sensible Heating Energy", frequency);
            outputvars.Add("Zone Air System Sensible Cooling Energy", frequency);

            if (building.boiler.fuelType.Contains("Electricity")) { outputvars.Add("Boiler Electric Energy", frequency); }
            else { outputvars.Add("Boiler Gas Energy", frequency); }

            outputvars.Add("Chiller Electric Energy", frequency);
            outputvars.Add("Cooling Tower Fan Electric Energy", frequency);

            outputvars.Add("Zone Lights Electric Energy", frequency);
            outputvars.Add("Zone Electric Equipment Electric Energy", frequency);
           
            if (heatFlow)
            {
                outputvars.Add("Zone Infiltration Total Heat Loss Energy", frequency);
                outputvars.Add("Zone Infiltration Total Heat Gain Energy", frequency);
                outputvars.Add("Surface Window Net Heat Transfer Energy", frequency);
                outputvars.Add("Surface Inside Face Conduction Heat Transfer Energy", frequency);
                outputvars.Add("Surface Outside Face Incident Solar Radiation Rate per Area", frequency);          
            }
              
            output = new Output(outputvars);
        }
    }
    [Serializable]
    public class Building
    {
        public string name = "Building1";
        public double northAxis = 0;
        public string terrain = String.Empty;
        public double loadConvergenceTolerance = 0.04;
        public double tempConvergenceTolerance = 0.4;
        public string solarDistribution = "FullExterior";
        public double maxNWarmUpDays = 25;
        public double minNWarmUpDays = 6;

        //Probablistic Attributes
        public ProbabilisticBuildingConstruction pBuildingConstruction;
        public ProbabilisticWWR pWWR;
        public double[] pOperatingHours = new double[2];
        public double[] pInfiltration = new double[2];
        public double[] pInternalHeatGain = new double[2];
        public double[] pBoilerEfficiency = new double[2];
        public double[] pChillerCOP = new double[2];

        //Probabilistic Energy Load
        public double[] annualHeatingLoad = new double[4];
        public double[] annualCoolingLoad = new double[4];

        //Probabilistic Operational Energy
        public double[] annualThermalEnergy = new double[4];
        public double[] annualLEEbergy = new double[4];
        public double[] annualEnergy = new double[4];

        //EnergyPlus Output
        public double BoilerEnergy, ChillerEnergy, TotalEnergy;
        public double TotalHeatingEnergy, TotalCoolingEnergy, LightingEnergy, EquipmentEnergy;
        public double TotalArea, TotalVolume;

        //Deterministic Attributes
        public BuildingConstruction buildingConstruction;
        public double FloorHeight;
        public WWR WWR { get; set; } = new WWR(0.5, 0.5, 0.5, 0.5); //North, West, South, East
        public ShadingLength shadingLength { get; set; } = new ShadingLength(0, 0, 0, 0); //North, West, South, East
        public double operatingHours = 8;
        public double infiltration = .75;
        public double LightHeatGain = 10;
        public double ElectricHeatGain = 10;
        public double boilerEfficiency = 0.8;
        public double chillerCOP = 4;

        //Schedules Limits and Schedule
        public List<ScheduleLimits> schedulelimits { get; set; } = new List<ScheduleLimits>();
        public List<ScheduleCompact> schedulescomp { get; set; } = new List<ScheduleCompact>();

        //Material, WindowMaterial, Shade, Shading Control, Constructions and Window Constructions
        public List<Material> materials = new List<Material>();
        public List<WindowMaterial> windowMaterials = new List<WindowMaterial>();
        public List<WindowMaterialShade> windowMaterialShades = new List<WindowMaterialShade>();
        public List<ShadingControl> shadingControls = new List<ShadingControl>();
        public List<Construction> constructions = new List<Construction>();

        //Zone, ZoneList, BuidlingSurface, ShadingZones, ShadingOverhangs
        public List<Zone> zones = new List<Zone>();
        public List<ZoneList> zoneLists = new List<ZoneList>();
        public List<BuildingSurface> bSurfaces = new List<BuildingSurface>();
        public List<ShadingZone> shadingZones = new List<ShadingZone>();
        public List<ShadingOverhang> shadingOverhangs = new List<ShadingOverhang>();

        //Defined at zone level - should be extracted from zone
        public List<People> peoples = new List<People>();
        public List<Light> lights = new List<Light>();
        public List<ElectricEquipment> eEquipments = new List<ElectricEquipment>();
        public List<ZoneVentilation> zVentillation = new List<ZoneVentilation>();
        public List<ZoneInfiltration> zInfiltration = new List<ZoneInfiltration>();

        //HVAC Template - should be extracted from zone
        public List<Thermostat> tStats = new List<Thermostat>();
        public List<ZoneHVAC> HVACS = new List<ZoneHVAC>();
        public VAV vav;
        public ChilledWaterLoop cWaterLoop;
        public Chiller chiller;
        public Tower tower;
        public HotWaterLoop hWaterLoop;
        public Boiler boiler;


        public void CreateSchedules(double[] heatingSetpoints, double[] coolingSetpoints, double equipmentoffFract)
        {
            int[] time = Utility.hourToHHMM(operatingHours);
            int hour1 = time[0]; int minutes1 = time[1]; int hour2 = time[2]; int minutes2 = time[3];
            operatingHours = (hour2 * 60 + minutes2 - (hour1 * 60 + minutes1)) / 60;

            double EquipmentOff = equipmentoffFract;//0.25;
            double heatingSetpoint1 = heatingSetpoints[0];//16;
            double heatingSetpoint2 = heatingSetpoints[1];//20;

            double coolingSetpoint1 = coolingSetpoints[0];//28;
            double coolingSetpoint2 = coolingSetpoints[1];//25;

            //60 minutes earlier
            int hour1b = hour1 - 1;
            int minutes1b = minutes1;

            schedulelimits = new List<ScheduleLimits>();
            schedulescomp = new List<ScheduleCompact>();

            ScheduleLimits activityLevel = new ScheduleLimits();
            activityLevel.name = "activityLevel";
            activityLevel.lowerLimit = 0;
            activityLevel.unitType = "activitylevel";
            schedulelimits.Add(activityLevel);

            ScheduleLimits fractional = new ScheduleLimits();
            fractional.name = "Fractional Sch Limits";
            fractional.lowerLimit = 0;
            fractional.upperLimit = 1;
            schedulelimits.Add(fractional);

            ScheduleLimits temp = new ScheduleLimits();
            temp.name = "Temp";
            temp.lowerLimit = 10;
            temp.upperLimit = 35;
            schedulelimits.Add(temp);

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
                name = "Heating Set Point Schedule",
                scheduleLimits = temp,
                daysTimeValue = heatSP
            };

            Dictionary<string, double> heatSP18V1 = new Dictionary<string, double>(), heatSP18V2 = new Dictionary<string, double>();
            heatSP18V1.Add(hour1b + ":" + minutes1b, heatingSetpoint1);
            heatSP18V1.Add(hour2 + ":" + minutes2, 18);
            heatSP18V1.Add("24:00", heatingSetpoint1);
            heatSP18.Add(days1, heatSP18V1);
            heatSP18V2.Add("24:00", heatingSetpoint1);
            heatSP18.Add(days2, heatSP18V2);
            ScheduleCompact heatingSP18 = new ScheduleCompact()
            {
                name = "Heating Set Point Schedule 18",
                scheduleLimits = temp,
                daysTimeValue = heatSP
            };

            Dictionary<string, double> coolSPV1 = new Dictionary<string, double>(), coolSPV2 = new Dictionary<string, double>();
            coolSPV1.Add(hour1b + ":" + minutes1b, coolingSetpoint1);
            coolSPV1.Add(hour2 + ":" + minutes2, coolingSetpoint2);
            coolSPV1.Add("24:00", coolingSetpoint1);
            coolSP.Add(days1, coolSPV1);
            coolSPV2.Add("24:00", coolingSetpoint1);
            coolSP.Add(days2, coolSPV2);
            ScheduleCompact coolingSP = new ScheduleCompact()
            {
                name = "Cooling Set Point Schedule",
                scheduleLimits = temp,
                daysTimeValue = coolSP
            };

            Dictionary<string, double> occupV1 = new Dictionary<string, double>(), occupV2 = new Dictionary<string, double>();
            occupV1.Add(hour1 + ":" + minutes1, 0);
            occupV1.Add(hour2 + ":" + minutes2, 1);
            occupV1.Add("24:00", 0);
            occupancyS.Add(days1, occupV1);
            occupV2.Add("24:00", 0);
            occupancyS.Add(days2, occupV2);
            ScheduleCompact occupSchedule = new ScheduleCompact()
            {
                name = "Occupancy Schedule",
                scheduleLimits = fractional,
                daysTimeValue = occupancyS
            };

            Dictionary<string, double> ventilV1 = new Dictionary<string, double>(), ventilV2 = new Dictionary<string, double>();
            ventilV1.Add(hour1 + ":" + minutes1, 0);
            ventilV1.Add(hour2 + ":" + minutes2, 1);
            ventilV1.Add("24:00", 0);
            ventilS.Add(days1, ventilV1);
            ventilV2.Add("24:00", 0);
            ventilS.Add(days2, ventilV2);
            ScheduleCompact ventilSchedule = new ScheduleCompact()
            {
                name = "Ventilation Schedule",
                scheduleLimits = fractional,
                daysTimeValue = ventilS
            };

            Dictionary<string, double> lehgV1 = new Dictionary<string, double>(), lehgV2 = new Dictionary<string, double>();
            lehgV1.Add(hour1 + ":" + minutes1, equipmentoffFract);
            lehgV1.Add(hour2 + ":" + minutes2, 1);
            lehgV1.Add("24:00", equipmentoffFract);
            leHeatGain.Add(days1, lehgV1);
            lehgV2.Add("24:00", equipmentoffFract);
            leHeatGain.Add(days2, lehgV2);
            ScheduleCompact lehgSchedule = new ScheduleCompact()
            {
                name = "Electric Equipment and Lighting Schedule",
                scheduleLimits = fractional,
                daysTimeValue = leHeatGain
            };

            ScheduleCompact nocooling = new ScheduleCompact()
            {
                name = "No Cooling",
                scheduleLimits = temp,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 35} } } }
            };

            ScheduleCompact activity = new ScheduleCompact()
            {
                name = "People Activity Schedule",
                scheduleLimits = activityLevel,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 125} } } }
            };

            ScheduleCompact workEff = new ScheduleCompact()
            {
                name = "Work Eff Sch",
                scheduleLimits = fractional,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 1} } } }
            };

            ScheduleCompact airVelo = new ScheduleCompact()
            {
                name = "Air Velo Sch",
                scheduleLimits = fractional,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", .1} } } }
            };

            //infiltration
            ScheduleCompact infiltration = new ScheduleCompact()
            {
                name = "Space Infiltration Schedule",
                scheduleLimits = fractional,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 1} } } }
            };

            schedulescomp.Add(heatingSP);           
            schedulescomp.Add(coolingSP);
            schedulescomp.Add(heatingSP18);
            schedulescomp.Add(nocooling);
            schedulescomp.Add(occupSchedule);
            schedulescomp.Add(ventilSchedule);
            schedulescomp.Add(lehgSchedule);           
            schedulescomp.Add(activity);
            schedulescomp.Add(workEff);
            schedulescomp.Add(airVelo);
            schedulescomp.Add(infiltration);
        }
        public void GenerateConstructionWithIComponentsU()
        {
            double uWall = buildingConstruction.uWall, uGFloor = buildingConstruction.uGFloor, uIFloor = buildingConstruction.uIFloor,
                uRoof = buildingConstruction.uRoof, uIWall = buildingConstruction.uIWall, uWindow = buildingConstruction.uWindow,
                gWindow = buildingConstruction.gWindow, hcSlab = buildingConstruction.hcSlab;
            double lambda_Wall = (uWall * 0.075) / (1 - uWall * (0.2 / 0.5 + 0.012 / 0.16));
            double lambda_gFloor = (uGFloor * 0.075) / (1 - uGFloor * (0.1 / 1.95));
            double lambda_iFloor = (uIFloor * 0.075) / (1 - uIFloor * (0.1 / 2));
            double lambda_Roof = (uRoof * 0.08) / (1 - uRoof * (0.175 / 0.165 + 0.025 / 0.075 + 0.15 / 0.55));
            double lambda_IWall = (uIWall * 2 * 0.012);
            
            if (lambda_gFloor <= 0 || lambda_Wall <= 0 || lambda_Roof <= 0 || lambda_iFloor <= 0 || lambda_IWall<=0)
            {
                Console.WriteLine("Check U Values {0}, {1}, {2}, {3}, {4}", lambda_gFloor, lambda_Wall, lambda_Roof, lambda_iFloor, lambda_IWall);
                Console.ReadKey();
            }

            //roof layers
            Material layer_F13 = new Material("F13", "Smooth", 0.175, 0.165, 1120, 1465, 0.9, 0.4, 0.7);
            Material layer_G03 = new Material("G03", "Smooth", 0.025, 0.075, 400, 1300, 0.9, 0.7, 0.7);
            Material layer_I03 = new Material("I03", "Smooth", 0.08, lambda_Roof, 45, 1200, 0.9, 0.7, 0.7);
            Material layer_M11 = new Material("M11", "Smooth", 0.15, 0.55, 1200, 850, 0.9, 0.7, 0.7);

            //wall layers
            Material layer_M03 = new Material("M03", "Smooth", 0.2, 0.5, 500, 900, 0.9, 0.4, 0.7);
            Material layer_I04 = new Material("I04", "Smooth", 0.075, lambda_Wall, 20, 1000, 0.9, 0.7, 0.7);
            Material layer_G01 = new Material("G01", "Smooth", 0.012, 0.16, 800, 1100, 0.9, 0.7, 0.7);

            //gFloor & iFloor layers
            Material layer_floorSlab = new Material("Concrete_Floor_Slab", "Smooth", 0.10, 1.95, 2250, hcSlab, 0.9, 0.7, 0.7);
            Material layer_gFloorInsul = new Material("gFloor_Insulation", "Smooth", 0.075, lambda_gFloor, 20, 1000, 0.9, 0.7, 0.7);
            Material layer_iFloorInsul = new Material("iFloor_Insulation", "Smooth", 0.075, lambda_iFloor, 20, 1000, 0.9, 0.7, 0.7);

            //internal wall layers
            Material layer_Plasterboard = new Material("Plasterboard", "Rough", 0.012, lambda_IWall, 800, 800, 0.9, 0.6, 0.6);

            //airgap
            //Layer layer_airgap = new Layer("Material Air Gap 1", "", 0.1, 0.1, 0.1, 0.1, 9, 7, 7); //airgap layer

            materials = new List<Material>() { layer_F13, layer_G03, layer_I03, layer_M11, layer_M03, layer_I04, layer_G01, layer_floorSlab, layer_gFloorInsul, layer_iFloorInsul, layer_Plasterboard };

            List<Material> layerListRoof = new List<Material>() { layer_F13, layer_G03, layer_I03, layer_M11 };
            buildingConstruction.hcRoof = layerListRoof.Select(l => l.thickness * l.sHC * l.density).Sum();
            Construction constructionRoof = new Construction("Up Roof Concrete", layerListRoof);

            List<Material> layerListWall = new List<Material>() { layer_M03, layer_I04, layer_G01 };

            Construction construction_Wall = new Construction("Wall ConcreteBlock", layerListWall);
            buildingConstruction.hcWall = layerListWall.Select(l => l.thickness * l.sHC * l.density).Sum();
            List<Material> layerListInternallWall = new List<Material>() { layer_Plasterboard, layer_Plasterboard };
            Construction construction_internalWall = new Construction("InternalWall", layerListInternallWall);
            buildingConstruction.hcIWall = layerListInternallWall.Select(l => l.thickness * l.sHC * l.density).Sum();
            List<Material> layerListGfloor = new List<Material>() { layer_floorSlab, layer_gFloorInsul };
            Construction construction_gFloor = new Construction("Slab_Floor", layerListGfloor);
            buildingConstruction.hcGFloor = layerListGfloor.Select(l => l.thickness * l.sHC * l.density).Sum();
            List<Material> layerListIFloor = new List<Material>() { layer_floorSlab, layer_iFloorInsul };
            Construction construction_floor = new Construction("General_Floor_Ceiling", layerListIFloor);
            buildingConstruction.hcIFloor = layerListIFloor.Select(l => l.thickness * l.sHC * l.density).Sum();
            constructions = new List<Construction>() { constructionRoof, construction_Wall, construction_gFloor, construction_floor, construction_internalWall };

            //window construction
            WindowMaterial windowLayer = new WindowMaterial("Glazing Material", uWindow, gWindow, 0.1);
            windowMaterials.Add(windowLayer);
            windowMaterials = new List<WindowMaterial>() { windowLayer };
            Construction window = new Construction("Glazing", new List<WindowMaterial>() { windowLayer });
            constructions.Add(window);

            //window shades
            windowMaterialShades.Add(new WindowMaterialShade());
            shadingControls.Add(new ShadingControl());

        }
        public void GeneratePeopleLightingElectricEquipment()
        {
            zones.ForEach(z => { People p = new People(10);
                Light l = new Light(LightHeatGain);
                ElectricEquipment e = new ElectricEquipment(ElectricHeatGain); peoples.Add(p); lights.Add(l); eEquipments.Add(e);
                z.people = p; z.equipment = e; z.lights = l;
            });
        }
        public void GenerateInfiltraitionAndVentillation()
        {
            zones.ForEach(z => {
                ZoneInfiltration i = new ZoneInfiltration(infiltration);
                ZoneVentilation v = new ZoneVentilation();
                z.infiltration = i; z.vent = v;
            });
        }
        public void GenerateHVAC(bool IsFCU, bool IsVAV, bool IsIdeal)
        {
            Thermostat officeT = new Thermostat("Office Thermostat", 20, 25, schedulescomp.First(s => s.name.Contains("Heating Set Point Schedule")),
                schedulescomp.First(s => s.name.Contains("Cooling Set Point Schedule")));
            tStats.Add(officeT);

            Thermostat otherAreasT = new Thermostat("Other Areas Thermostat", 18, 30, schedulescomp.First(s => s.name.Contains("Heating Set Point Schedule 18")),
                schedulescomp.First(s => s.name.Contains("No Cooling")));
            tStats.Add(otherAreasT);

            zoneLists.FirstOrDefault(zl => zl.name.Contains("Office")).listZones.ForEach(z => z.thermostat = officeT);
            zoneLists.Where(zl => !zl.name.Contains("Office")).ToList().ForEach(zl=>zl.listZones.ForEach(z => z.thermostat = otherAreasT));
            if (IsFCU)
            {
                zones.ForEach(z => { ZoneFanCoilUnit zFCU = new ZoneFanCoilUnit(z, z.thermostat); HVACS.Add(zFCU); z.hvac = zFCU; });
                GenerateWaterLoopsAndSystem();
            }
            if (IsVAV)
            {
                VAV vav = new VAV();
                zones.ForEach(z => { ZoneVAV zVAV = new ZoneVAV(vav, z, z.thermostat); HVACS.Add(zVAV); z.hvac = zVAV; });
                GenerateWaterLoopsAndSystem();
            }
            if (IsIdeal)
            {
                zones.ForEach(z => { ZoneIdealLoad zIdeal = new ZoneIdealLoad(z, z.thermostat); HVACS.Add(zIdeal); z.hvac = zIdeal; });
            }
        }

        private void GenerateWaterLoopsAndSystem()
        {
            hWaterLoop = new HotWaterLoop();
            cWaterLoop = new ChilledWaterLoop();
            boiler = new Boiler(boilerEfficiency, "Electricity");
            chiller = new Chiller(chillerCOP);
            tower = new Tower();
        }
        

        public Building() { }

        public void AssociateEnergyPlusResults(IList<string> outputHeader, List<double[]> data)
        {
            foreach (BuildingSurface surf in bSurfaces)
            {
                if (surf.surfaceType == SurfaceType.Wall || surf.surfaceType == SurfaceType.Roof)
                {
                    if (surf.surfaceType == SurfaceType.Wall)
                    {
                        Fenestration win = surf.fenestrations[0];
                        string strWinRad = outputHeader.First(a => a.Contains(win.name.ToUpper()) && a.Contains("Surface Outside Face Incident Solar Radiation Rate per Area"));
                        win.SolarRadiation = data[outputHeader.IndexOf(strWinRad)].Sum();
                        //Console.WriteLine(string.Join(" - ", win.name, win.face.zone.name, win.area, win.SolarRadiation));
                        string winHeatFlow = outputHeader.First(s => s.Contains(win.name.ToUpper()) && s.Contains("Surface Window Net Heat Transfer Energy"));
                        win.HeatFlow = data[outputHeader.IndexOf(winHeatFlow)].Sum(); ;
                    }
                    string radStr = outputHeader.First(s => s.Contains(surf.name.ToUpper()) && s.Contains("Surface Outside Face Incident Solar Radiation Rate per Area") && !s.Contains("WINDOW"));
                    surf.SolarRadiation = data[outputHeader.IndexOf(radStr)].Sum();
                }
                string heatStr = outputHeader.First(s => s.Contains(surf.name.ToUpper()) && s.Contains("Surface Inside Face Conduction Heat Transfer Energy"));
                surf.HeatFlow = data[outputHeader.IndexOf(heatStr)].Sum();
            }
            foreach (Zone zone in zones)
            {
                zone.CalcAreaVolumeHeatCapacity(); zone.AssociateEnergyPlusResults(outputHeader, data);          
            }
            TotalArea = zones.Select(z => z.area).Sum(); TotalVolume = zones.Select(z => z.volume).Sum();
            TotalHeatingEnergy = zones.Select(z => z.HeatingEnergy).Sum(); TotalCoolingEnergy = zones.Select(z => z.CoolingEnergy).Sum();
            LightingEnergy = zones.Select(z => z.LightingEnergy).Sum(); EquipmentEnergy = zones.Select(z => z.EquipmentEnergy).Sum();

            BoilerEnergy = data[outputHeader.IndexOf(outputHeader.First(a => a.Contains("Boiler Electric Energy")))].Sum();
            ChillerEnergy = data[outputHeader.IndexOf(outputHeader.First(a => a.Contains("Chiller Electric Energy")))].Sum();
            ChillerEnergy += data[outputHeader.IndexOf(outputHeader.First(a => a.Contains("Cooling Tower Fan Electric Energy")))].Sum();
            TotalEnergy = ChillerEnergy + BoilerEnergy + LightingEnergy + EquipmentEnergy;
        }

        public Building AddZone(Zone zone)
        {
            zones.Add(zone);
            bSurfaces.AddRange(zone.surfaces);
            try { shadingOverhangs.AddRange(zone.surfaces.Select(f => f.shading).SelectMany(i => i)); } catch { }
            return this;
        }
        public Building AddZone(List<Zone> zones) { zones.ForEach(z => AddZone(z)); return this; }
        public Building AddZoneList(ZoneList zoneList) { zoneLists.Add(zoneList); return this; }
        public Building DeepCopy(string name)
        {
            if (!typeof(IDFFile).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(this, null))
            {
                return this;
            }
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                Building other = (Building)formatter.Deserialize(stream);
                other.name = name;
                return other;
            }
        }
        public Building Transform(double angle)
        {
            zones.ForEach(z => z.surfaces.ForEach(bSurf => {
                bSurf.verticesList.Transform(angle);
                bSurf.fenestrations.ForEach(fen => fen.verticesList.Transform(angle));
                bSurf.shading.ForEach(shading => shading.listVertice.Transform(angle));
            }));

            return this;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "Building,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(northAxis, "North Axis"),
                Utility.IDFLineFormatter(terrain, "Terrain"),
                Utility.IDFLineFormatter(loadConvergenceTolerance, "Loads Convergence Tolerance Value"),
                Utility.IDFLineFormatter(tempConvergenceTolerance, "Temperature Convergence Tolerance Value {deltaC}"),
                Utility.IDFLineFormatter(solarDistribution, "Solar Distribution"),
                Utility.IDFLineFormatter(maxNWarmUpDays, "Maximum Number of Warmup Days"),
                Utility.IDFLastLineFormatter(minNWarmUpDays, "Minimum Number of Warmup Days")
            };
        }
    }
    [Serializable]
    public class WWR
    {
        public double north;
        public double east;
        public double south;
        public double west;

        public WWR() { }

        public WWR(double north, double east, double south, double west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }

    }
    [Serializable]
    public class ShadingLength
    {
        public double north;
        public double east;
        public double south;
        public double west;

        public ShadingLength() { }

        public ShadingLength(double north, double east, double south, double west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }

    }
    [Serializable]
    public class ProbabilisticWWR
    {
        public double[] north;
        public double[] east;
        public double[] south;
        public double[] west;

        public ProbabilisticWWR() { }

        public ProbabilisticWWR(double[] north, double[] east, double[] south, double[] west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }

    }
    [Serializable]
    public class BuildingConstruction
    {
        //To store the values from samples
        public double uWall;
        public double uGFloor;
        public double uRoof;
        public double uIFloor;
        public double uIWall;
        public double uWindow;
        public double gWindow;

        public double hcWall, hcRoof, hcGFloor, hcIFloor, hcSlab, hcIWall;

        public BuildingConstruction() { }
        public BuildingConstruction(double uWall, double uGFloor, double uRoof, double uWindow, double gWindow, double uIFloor, double uIWall, double hcSlab)
        {
            this.uWall = uWall; this.uGFloor = uGFloor; this.uRoof = uRoof; this.uWindow = uWindow; this.gWindow = gWindow; this.uIFloor = uIFloor; this.uIWall = uIWall; this.hcSlab = hcSlab;
        }
    }
    [Serializable]
    public class ProbabilisticBuildingConstruction
    {
        //To store the values from samples
        public double[] uWall;
        public double[] uGFloor;
        public double[] uRoof;
        public double[] uIFloor;
        public double[] uIWall;
        public double[] uWindow;
        public double[] gWindow;
        public double[] HCFloor;

        public ProbabilisticBuildingConstruction() { }
        public ProbabilisticBuildingConstruction(double[] uWall, double[] uGFloor, double[] uRoof, double[] uIFloor, double[] uIWall, double[] uWindow, double[] gWindow, double[] HCFloor)
        {
            this.uWall = uWall;
            this.uGFloor = uGFloor;
            this.uRoof = uRoof;
            this.uIFloor = uIFloor;
            this.uIWall = uIWall;
            this.uWindow = uWindow;
            this.gWindow = gWindow;
            this.HCFloor = HCFloor;
        }
    }
    [Serializable]
    public class ShadingZone
    {
    }
    [Serializable]
    public class ShadingControl
    {
        public string name = "CONTROL ON ZONE TEMP";
        public string shadingType = "InteriorShade";
        public string cShadingName = "";
        public string shadingControlType = "OnIfHighZoneAirTemperature";
        public string scehduleName = "";
        public double setPoint = 23;
        public string scheduled = "NO";
        public string glareControl = "NO";
        public string shadingMatrial = "ROLL SHADE";
        public string angleControl = "";
        public string slatSchedule = "";

        public ShadingControl() { }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "WindowProperty:ShadingControl,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(shadingType, "Shading Type"),
                Utility.IDFLineFormatter(cShadingName, "Construction with Shading Name"),
                Utility.IDFLineFormatter(shadingControlType, "Shading Control Type"),
                Utility.IDFLineFormatter(scehduleName, "Schedule Name"),
                Utility.IDFLineFormatter(setPoint, "Setpoint {W/m2, W or deg C}"),
                Utility.IDFLineFormatter(scheduled, "Shading Control Is Scheduled"),
                Utility.IDFLineFormatter(glareControl, "Glare Control Is Active"),
                Utility.IDFLineFormatter(shadingMatrial, "Shading Device Material Name"),
                Utility.IDFLineFormatter(angleControl, "Type of Slat Angle Control for Blinds"),
                Utility.IDFLastLineFormatter(slatSchedule, "Slat Angle Schedule Name")
            };
        }
    }
    [Serializable]
    public class WindowMaterialShade
    {
        public string name = "ROLL SHADE";
        public double sTransmittance = 0.3;
        public double sReflectance = 0.5;
        public double vTransmittance = 0.3;
        public double vReflectance = 0.5;
        public double infraEmissivity = 0.9;
        public double infraTransmittance = 0.05;
        public double thickness = 0.003;
        public double conductivity = 0.1;
        public double disShades = 0.05;
        public double tMultiplier = 0;
        public double bMultiplier = 0.5;
        public double lMultiplier = 0.5;
        public double rMultiplier = 0;
        public string airPermeability ="";

        public WindowMaterialShade() { }
        public List<string> writeInfo()
        {
            return new List<string>()
            {
                "WindowMaterial:Shade,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(sTransmittance, "Solar Transmittance { dimensionless }"),
                Utility.IDFLineFormatter(sReflectance, "Solar Reflectance { dimensionless }"),
                Utility.IDFLineFormatter(vTransmittance, "Visible Transmittance { dimensionless }"),
                Utility.IDFLineFormatter(vReflectance, "Visible Reflectance { dimensionless }"),
                Utility.IDFLineFormatter(infraEmissivity, "Infrared Hemispherical Emissivity { dimensionless }"),
                Utility.IDFLineFormatter(infraTransmittance, "Infrared Transmittance { dimensionless }"),
                Utility.IDFLineFormatter(thickness, "Thickness { m }"),
                Utility.IDFLineFormatter(conductivity, "Conductivity { W / m - K }"),
                Utility.IDFLineFormatter(disShades, "Shade to Glass Distance { m }"),
                Utility.IDFLineFormatter(tMultiplier, "Top Opening Multiplier"),
                Utility.IDFLineFormatter(bMultiplier, "Bottom Opening Multiplier"),
                Utility.IDFLineFormatter(lMultiplier, "Left - Side Opening Multiplier"),
                Utility.IDFLineFormatter(rMultiplier, "Right - Side Opening Multiplier"),
                Utility.IDFLastLineFormatter(airPermeability, "Airflow Permeability { dimensionless}")
            };
        }
    }
    [Serializable]
    public class Zone
    {
        public double HeatingEnergy, CoolingEnergy, LightingEnergy, EquipmentEnergy;
        public List<BuildingSurface> surfaces { get; set; }
        public double area;
        public double volume;
        public double height;
        public DayLighting DayLightControl;
        public People people { get; set; }
        public ElectricEquipment equipment { get; set; }
        public Light lights { get; set; }
        public ZoneHVAC hvac { get; set; }
        public Thermostat thermostat { get; set; }
        public ZoneVentilation vent { get; set; }
        public ZoneInfiltration infiltration { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public Building building;

        public double totalWallArea, totalWindowArea, totalGFloorArea, totalIFloorArea, totalIWallArea, totalRoofArea, 
            heatCapacity, SurfAreaU, SolarRadiation, TotalHeatFlows, ExSurfAreaU, GSurfAreaU, ISurfAreaU, 
            wallAreaU, windowAreaU, gFloorAreaU, roofAreaU, iFloorAreaU, iWallAreaU,
            wallHeatFlow, windowHeatFlow, gFloorHeatFlow, iFloorHeatFlow, iWallHeatFlow, roofHeatFlow, infiltrationFlow;
        public List<BuildingSurface> walls, gFloors, iWalls, iFloors, roofs, izFloors, izWalls;
        public List<Fenestration> windows;

        public void CalcAreaVolumeHeatCapacity()
        {
            IEnumerable<BuildingSurface> floors = surfaces.Where(a => a.surfaceType == SurfaceType.Floor);
            area = floors.Select(a => a.area).ToArray().Sum();
            volume = area * height;

            walls = surfaces.Where(w => w.surfaceType == SurfaceType.Wall).ToList();
            windows = (walls.Where(w=>w.fenestrations.Count!=0)).SelectMany(w => w.fenestrations).ToList();
            gFloors = surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").ToList();
            roofs = surfaces.Where(w => w.surfaceType == SurfaceType.Roof).ToList();
            iFloors = surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition != "Ground").ToList();
            iWalls = surfaces.Where(w => w.surfaceType == SurfaceType.InternalWall).ToList();
            izFloors = building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Floor && iF.OutsideObject == name).ToList();
            izWalls = building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.InternalWall && iF.OutsideObject == name).ToList();

            totalWallArea = walls.Select(w => w.area).Sum();
            totalGFloorArea = gFloors.Select(gF => gF.area).Sum();
            totalIFloorArea = iFloors.Select(iF => iF.area).Sum()+ izFloors.Select(iF => iF.area).Sum();
            totalIWallArea = iWalls.Select(iF => iF.area).Sum() + izWalls.Select(iF => iF.area).Sum();
            totalRoofArea = roofs.Select(r => r.area).Sum();
            totalWindowArea = windows.Select(wi => wi.area).Sum();

            heatCapacity = totalWallArea * building.buildingConstruction.hcWall + totalGFloorArea * building.buildingConstruction.hcGFloor + totalIFloorArea * building.buildingConstruction.hcIFloor +
                                  totalIWallArea * building.buildingConstruction.hcIWall + totalRoofArea * building.buildingConstruction.hcRoof;
            wallAreaU = totalWallArea * building.buildingConstruction.uWall;
            gFloorAreaU = totalGFloorArea * building.buildingConstruction.uGFloor;
            iFloorAreaU = totalIFloorArea * building.buildingConstruction.uIFloor;
            windowAreaU = totalWindowArea * building.buildingConstruction.uWindow;
            iWallAreaU = totalIWallArea * building.buildingConstruction.uIWall;
            roofAreaU = totalRoofArea * building.buildingConstruction.uRoof;

            ExSurfAreaU = wallAreaU + windowAreaU + roofAreaU;
            GSurfAreaU = gFloorAreaU;
            ISurfAreaU = iFloorAreaU + iWallAreaU;
            SurfAreaU = ExSurfAreaU + GSurfAreaU + ISurfAreaU;
        }

        public void AssociateEnergyPlusResults(IList<string> outputHeader, List<double[]> data)
        {
            wallHeatFlow = walls.Select(s => s.HeatFlow).Sum();
            gFloorHeatFlow = gFloors.Select(s => s.HeatFlow).Sum();
            iFloorHeatFlow = iFloors.Select(s => s.HeatFlow).Sum();
            iWallHeatFlow = iWalls.Select(s => s.HeatFlow).Sum();
            windowHeatFlow = walls.Select(s => s.fenestrations[0]).Select(s => s.HeatFlow).Sum();
            roofHeatFlow = roofs.Select(s => s.HeatFlow).Sum();

            iFloorHeatFlow -= izFloors.Select(s => s.HeatFlow).Sum();
            iWallHeatFlow -= izWalls.Select(s => s.HeatFlow).Sum();
            TotalHeatFlows = wallHeatFlow + gFloorHeatFlow + iFloorHeatFlow + iWallHeatFlow + windowHeatFlow + roofHeatFlow;
            SolarRadiation = (walls.SelectMany(w => w.fenestrations).Select(f => f.area * f.SolarRadiation)).Sum();

            infiltrationFlow = data[outputHeader.IndexOf(outputHeader.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Gain Energy")))].Sum() -
                    data[outputHeader.IndexOf(outputHeader.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Loss Energy")))].Sum();
            HeatingEnergy = data[outputHeader.IndexOf(outputHeader.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Air System Sensible Heating Energy")))].Sum();
            CoolingEnergy = data[outputHeader.IndexOf(outputHeader.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Air System Sensible Cooling Energy")))].Sum();
            LightingEnergy = data[outputHeader.IndexOf(outputHeader.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Lights Electric Energy")))].Sum();
            EquipmentEnergy = data[outputHeader.IndexOf(outputHeader.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Electric Equipment Electric Energy")))].Sum();
        }
        
        public Zone() { surfaces = new List<BuildingSurface>(); }
        public Zone(Building building, string n, int l)
        {
            this.building = building;
            name = n;
            level = l;
            surfaces = new List<BuildingSurface>();
            height = building.FloorHeight;
        }

        public Zone Clone()
        {
            Zone other = (Zone)this.MemberwiseClone();
            other.surfaces = new List<BuildingSurface>();
            foreach (BuildingSurface bSurf in surfaces)
            {
                other.surfaces.Add(bSurf.Clone(name));
            }
            return other;
        }
    }
    [Serializable]
    public class BuildingSurface
    {
        public double[] annualHeatFlow;
        public XYZList verticesList;
        public string name { get; set; }
        public Zone zone { get; set; }
        public SurfaceType surfaceType { get; set; }
        public string ConstructionName { get; set; }
        public Direction direction { get; set; }
        public double orientation { get; set; }
        public List<Fenestration> fenestrations { get; set; }
        public List<ShadingOverhang> shading { get; set; }
        public string OutsideCondition { get; set; }
        public string OutsideObject { get; set; }
        public string SunExposed { get; set; }
        public string WindExposed { get; set; }
        public double area { get; set; }
        public double wWR { get; set; } = 0;
        public double sl { get; set; } = 0;//shaderlength
        public double df { get; set; }
        public double SolarRadiation;
        public double HeatFlow;
        private void addName()
        {
            name = zone.name + ":" + zone.level + ":" + ConstructionName + "_" + (zone.surfaces.Count + 1);
            if (surfaceType == SurfaceType.Wall)
            {
                name = zone.name + ":" + zone.level + ":" + direction + ":" + ConstructionName + "_" + (zone.surfaces.Count + 1);
            }
        }
        public void AssociateWWRandShadingLength()
        {
            orientation = verticesList.GetWallDirection();

            if (orientation <= 45 || orientation > 315)
            {
                wWR = zone.building.WWR.north;
                sl = zone.building.shadingLength.north;
                direction = Direction.North;
            }

            if (orientation > 45 && orientation <= 135)
            {
                wWR = zone.building.WWR.east;
                sl = zone.building.shadingLength.east;
                direction = Direction.East;
            }

            if (orientation > 135 && orientation <= 225)
            {
                wWR = zone.building.WWR.south;
                sl = zone.building.shadingLength.south;
                direction = Direction.South;
            }

            if (orientation > 225 && orientation <= 315)
            {
                wWR = zone.building.WWR.west;
                sl = zone.building.shadingLength.west;
                direction = Direction.West;
            }
        }

        public BuildingSurface(Zone zone1, XYZList pointList1, double areaN, SurfaceType surfaceType1)
        {
            zone = zone1;
            area = areaN;
            verticesList = pointList1;
            surfaceType = surfaceType1;

            List<XYZ> pointList = pointList1.xyzs;
            switch (surfaceType)
            {
                case (SurfaceType.Floor):
                    ConstructionName = "Slab_Floor";
                    OutsideCondition = "Ground";
                    OutsideObject = "";
                    SunExposed = "NoSun";
                    WindExposed = "NoWind";
                    break;
                case (SurfaceType.Wall):
                    OutsideObject = "";
                    OutsideCondition = "Outdoors";
                    SunExposed = "SunExposed";
                    WindExposed = "WindExposed";
                    AssociateWWRandShadingLength();                   
                    ConstructionName = "Wall ConcreteBlock";
                    break;
                case (SurfaceType.InternalWall):
                    OutsideObject = "Surface";
                    OutsideCondition = "";
                    SunExposed = "NoSun";
                    WindExposed = "NoWind";
                    ConstructionName = "Internal Wall";  //?
                    break;
                case (SurfaceType.Ceiling):
                    pointList.Reverse();
                    ConstructionName = "General_Floor_Ceiling";
                    OutsideCondition = "Adiabatic";
                    SunExposed = "NoSun";
                    WindExposed = "NoWind";
                    break;
                case (SurfaceType.Roof):
                    pointList.Reverse();
                    ConstructionName = "Up Roof Concrete";
                    OutsideObject = "";
                    OutsideCondition = "Outdoors";
                    SunExposed = "SunExposed";
                    WindExposed = "WindExposed";
                    break;
            }
            addName();
            if (wWR != 0) { CreateFenestration(1); }
            if (sl != 0) { shading = new List<ShadingOverhang>() { new ShadingOverhang(this) }; }
            zone1.surfaces.Add(this);
        }

        public BuildingSurface(Zone zone1, XYZList pointList1, double areaN, SurfaceType surfaceType1, bool fromRevit)
        {
            zone = zone1;
            area = areaN;
            verticesList = pointList1;
            surfaceType = surfaceType1;

            List<XYZ> pointList = pointList1.xyzs;
            switch (surfaceType)
            {
                case (SurfaceType.Floor):
                    ConstructionName = "Slab_Floor";
                    OutsideCondition = "Ground";
                    OutsideObject = "";
                    SunExposed = "NoSun";
                    WindExposed = "NoWind";
                    break;
                case (SurfaceType.Wall):
                    OutsideObject = "";
                    OutsideCondition = "Outdoors";
                    SunExposed = "SunExposed";
                    WindExposed = "WindExposed";
                    ConstructionName = "Concrete Block";
                    break;
                case (SurfaceType.InternalWall):
                    OutsideObject = "Surface";
                    OutsideCondition = "";
                    SunExposed = "NoSun";
                    WindExposed = "NoWind";
                    ConstructionName = "InternalWall";
                    break;
                case (SurfaceType.Ceiling):
                    pointList.Reverse();
                    ConstructionName = "General_Floor_Ceiling";
                    OutsideCondition = "Zone";
                    SunExposed = "NoSun";
                    WindExposed = "NoWind";
                    break;
                case (SurfaceType.Roof):
                    pointList.Reverse();
                    ConstructionName = "Up Roof Concrete";
                    OutsideObject = "";
                    OutsideCondition = "Outdoors";
                    SunExposed = "SunExposed";
                    WindExposed = "WindExposed";
                    break;
            }
            addName();
            zone1.surfaces.Add(this);
        }
        public List<string> surfaceInfo()
        {
            List<string> info = new List<string>();
            info.Add("BuildingSurface:Detailed,");
            info.Add("\t" + name + ",\t\t!- Name");
            info.Add("\t" + surfaceType + ",\t\t\t\t\t!-Surface Type");
            info.Add("\t" + ConstructionName + ",\t\t\t\t!-Construction Name");
            info.Add("\t" + zone.name + ",\t\t\t\t\t\t!-Zone Name");
            info.Add("\t" + OutsideCondition + ",\t\t\t\t\t!-Outside Boundary Condition");
            info.Add("\t" + OutsideObject + ",\t\t\t\t\t\t!-Outside Boundary Condition Object");
            info.Add("\t" + SunExposed + ",\t\t\t\t\t\t!-Sun Exposure");
            info.Add("\t" + WindExposed + ",\t\t\t\t\t\t!-Wind Exposure");
            info.Add("\t" + ",\t\t\t\t\t\t!-View Factor to Ground");
            info.AddRange(verticesList.verticeInfo());
            return info;
        }

        public List<Fenestration> CreateFenestration(int count)
        {
            List<Fenestration> fenestrationList = new List<Fenestration>();          
            for (int i = 0; i < count; i++)
            {
                Fenestration fen = new Fenestration(this);
                XYZ P1 = verticesList.xyzs.ElementAt(0);
                XYZ P2 = verticesList.xyzs.ElementAt(1);
                XYZ P3 = verticesList.xyzs.ElementAt(2);
                XYZ P4 = verticesList.xyzs.ElementAt(3);
                double openingFactor = Math.Sqrt(wWR / count);

                XYZ pMid = new XYZ((P1.X + P3.X) / (count -i + 1), (P1.Y + P3.Y) / (count - i + 1), (P1.Z + P3.Z) / 2);

                fen.verticesList = new XYZList(verticesList.xyzs.Select(v => new XYZ(pMid.X + (v.X - pMid.X) * openingFactor,
                                                            pMid.Y + (v.Y - pMid.Y) * openingFactor,
                                                            pMid.Z + (v.Z - pMid.Z) * openingFactor)).ToList());
                fen.area = area * wWR / count;
                fenestrationList.Add(fen);
            }
            fenestrations = fenestrationList;
            area = area * (1-wWR);
            return fenestrationList;
        }

        public BuildingSurface Clone(string name)
        {
            BuildingSurface other = (BuildingSurface)this.MemberwiseClone();
            other.name = name + "_" + other.name;
            other.verticesList = new XYZList(verticesList.xyzs.Select(v => v.Copy()).ToList());

            if (surfaceType == SurfaceType.Wall)
            {
                other.fenestrations = new List<Fenestration>();
                if (fenestrations != null) { other.fenestrations.AddRange(fenestrations.Select(f => f.Clone(name))); }
            }
            return other;
        }
    }
    [Serializable]
    public class XYZList
    {
        public List<XYZ> xyzs;

        public XYZList OffsetHeight(double height)
        {
            List<XYZ> newVertices = new List<XYZ>();
            foreach (XYZ v in xyzs)
            {
                XYZ v1 = v.OffsetHeight(height);
                newVertices.Add(v1);
            }
            return (new XYZList(newVertices));
        }
        public XYZList(List<XYZ> list)
        {
            xyzs = list;
        }
        public XYZList reverse()
        {
            xyzs.Reverse();
            return this;
        }
        public List<string> verticeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\t" + ",\t\t\t\t\t\t!- Number of Vertices");
            xyzs.ForEach(xyz => info.Add(string.Join(",", xyz.X, xyz.Y, xyz.Z) + ", !- X Y Z of Point"));
            return info.ReplaceLastComma();
        }
        public List<BuildingSurface> createWalls(Zone z, double height)
        {
            List<BuildingSurface> walls = new List<BuildingSurface>();
            foreach (XYZ v1 in xyzs)
            {
                XYZ v2 = new XYZ(0, 0, 0);
                if (!(v1 == xyzs.Last()))
                { v2 = xyzs.ElementAt((xyzs.IndexOf(v1) + 1)); }
                else { v2 = xyzs.First(); }

                XYZ v3 = v2.OffsetHeight(height);
                XYZ v4 = v1.OffsetHeight(height);

                XYZList vList = new XYZList(new List<XYZ>() { v4, v3, v2.Copy(), v1.Copy() });
                BuildingSurface wall = new BuildingSurface(z, vList, v1.DistanceTo(v2)*height, SurfaceType.Wall);
                walls.Add(wall);
            }

            return walls;
        }
        public List<BuildingSurface> createWalls(Zone z, double height, double basementDepth)
        {
            List<BuildingSurface> walls = new List<BuildingSurface>();
            foreach (XYZ v1 in xyzs)
            {
                XYZ v2 = new XYZ(0, 0, 0);
                if (!(v1 == xyzs.Last()))
                { v2 = xyzs.ElementAt((xyzs.IndexOf(v1) + 1)); }
                else { v2 = xyzs.First(); }

                XYZ v3 = v2.OffsetHeight(basementDepth);
                XYZ v4 = v1.OffsetHeight(basementDepth);

                XYZ v5 = v2.OffsetHeight(height);
                XYZ v6 = v1.OffsetHeight(height);

                XYZList vList1 = new XYZList(new List<XYZ>() { v4, v3, v2.Copy(), v1.Copy() });
                BuildingSurface wall1 = new BuildingSurface(z, vList1, v1.DistanceTo(v2) * height, SurfaceType.Wall);
                wall1.fenestrations = new List<Fenestration>();
                wall1.OutsideCondition = "Ground";
                wall1.OutsideObject = "";
                wall1.SunExposed = "NoSun";
                wall1.WindExposed = "NoWind";

                XYZList vList2 = new XYZList(new List<XYZ>() { v6, v5, v3, v4 });
                BuildingSurface wall2 = new BuildingSurface(z, vList2, v3.DistanceTo(v4) * height, SurfaceType.Wall);
                walls.Add(wall1);
                walls.Add(wall2);
            }

            return walls;
        }
        public void Transform(double angle)
        {
            List<XYZ> newXYZ = new List<XYZ>();
            xyzs.ForEach(v => newXYZ.Add(v.Transform(angle)));
            xyzs = newXYZ;
        }
        public double GetWallDirection()
        {
            XYZ v1 = xyzs[0]; XYZ v2 = xyzs[1]; XYZ v3 = xyzs[2];
            XYZ nVector1 = v2.Subtract(v1).CrossProduct(v3.Subtract(v1));
            return nVector1.AngleOnPlaneTo(new XYZ(0,1,0), new XYZ(0,0,1));
        }
        public XYZList ChangeZValue(double newZ)
        {
            List<XYZ> newVertices = new List<XYZ>();
            xyzs.ForEach(p => newVertices.Add(new XYZ(p.X, p.Y, newZ)));
            return new XYZList(newVertices);
        }
    }
    [Serializable]
    public class XYZ
    {
        public double X = 0, Y = 0, Z = 0;
        public XYZ() { }
        public XYZ(double x, double y, double z) { X = x; Y = y; Z = z; }
        public XYZ Subtract(XYZ newXYZ) { return new XYZ(newXYZ.X - X, newXYZ.Y - Y, newXYZ.Z - Z); }
        public override string ToString()
        {
            return string.Join(",", X, Y, Z);
        }
        public double DotProduct(XYZ newXYZ)
        {
            return X * newXYZ.X + Y * newXYZ.Y + Z * newXYZ.Z;
        }
        public XYZ CrossProduct(XYZ newXYZ)
        {
            return new XYZ(Y * newXYZ.Z - Z * newXYZ.Y, Z * newXYZ.X - X * newXYZ.Z, X * newXYZ.Y - Y * newXYZ.X);
        }
        public double AngleOnPlaneTo(XYZ right, XYZ normalPlane)
        {
            double nDouble = DotProduct(right);
            double anglePI = Math.Atan2(CrossProduct(right).DotProduct(normalPlane), nDouble - (right.DotProduct(normalPlane))*DotProduct(normalPlane));
            if (anglePI < 0) { anglePI = Math.PI * 2 + anglePI; }
            return Math.Round(180 * anglePI / Math.PI);
        }
        public double AngleBetweenVectors(XYZ newXYZ)
        {
            return (Math.Round(Math.Acos((X * newXYZ.X + Y * newXYZ.Y + Z * newXYZ.Z) / (AbsoluteValue() * newXYZ.AbsoluteValue())), 2));
        }
        public double DistanceTo(XYZ newXYZ)
        {
            return Math.Sqrt(Math.Pow(X-newXYZ.X,2) + Math.Pow(Y-newXYZ.Y,2) + Math.Pow(Z-newXYZ.Z,2));
        }
        public double AbsoluteValue()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }   
    }
    [Serializable]
    public class Fenestration
    {
        public double[] annualHeatGain, annualHeatLoss;
        public double area;
        public double SolarRadiation;
        public double HeatFlow;
        public BuildingSurface face { get; set; }
        public XYZList verticesList { get; set; }
        public string constructionName { get; set; }
        public SurfaceType surfaceType { get; set; }
        public string name { get; set; }
        public string shadingControl { get; set; }
        public OverhangProjection overhang { get; set; }
        public Fenestration(BuildingSurface wallFace)
        {
            face = wallFace;
            surfaceType = SurfaceType.Window;            
            constructionName = "Glazing";
            switch (face.direction)
            {
                case Direction.North:
                    shadingControl = "";
                    break;
                case Direction.East:
                case Direction.South:
                case Direction.West:
                    shadingControl = "CONTROL ON ZONE TEMP";
                    break;             
            }
            name = surfaceType + "_On_" + face.name;
            verticesList = new XYZList(new List<XYZ>());
        }        

        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("FenestrationSurface:Detailed,");
                info.Add("\t" + surfaceType + "_On_" + face.name + ",\t!- Name");
                info.Add("\t" + surfaceType + ",\t\t\t\t\t\t!- Surface Type");
                info.Add("\t" + constructionName + ",\t\t\t\t\t\t!- Construction Name");
                info.Add("\t" + face.name + ",\t!-Building Surface Name)");
                info.Add("\t,\t\t\t\t\t\t!-Outside Boundary Condition Object");
                info.Add("\t,\t\t\t\t\t\t!-View Factor to Ground");
                info.Add("\t" + shadingControl +",\t\t\t\t\t\t!- Shading Control Name");
                info.Add("\t,\t\t\t\t\t\t!- Frame and Divider Name");
                info.Add("\t,\t\t\t\t\t\t!-Multiplier");

                info.AddRange(verticesList.verticeInfo());
            return info;
        }

        public Fenestration Clone(string name)
        {
            Fenestration other = (Fenestration)this.MemberwiseClone();
            other.name = name + "_" + other.name;
            other.verticesList = new XYZList(verticesList.xyzs.Select(v => v.Copy()).ToList());
            
            return other;
        }
    }
    public enum ControlType {none, Continuous, Stepped, ContinuousOff}
    [Serializable]
    public class DayLightRefPoint
    {
        public string Name;
        public XYZ Point;
        public Zone Zone;
        public double PartControlled;
        public double Illuminance;
        public DayLightRefPoint() { }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>()
            {
                "Daylighting:ReferencePoint,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(Zone.name, "Zone Name"),
                Utility.IDFLineFormatter(Point, "XYZ of Point")
            };
            return info.ReplaceLastComma();
        }
        
    }
    [Serializable]
    public class DayLighting
    {
        public string Name;
        public Zone Zone { get; set; }
        public string DLMethod = "SplitFlux";
        public List<DayLightRefPoint> ReferencePoints = new List<DayLightRefPoint>();

        public ControlType CType = ControlType.Continuous;
        public double GlareCalcAngle = 180;
        public double DiscomGlare = 20;
        public double MinPower = 0.3;
        public double MinLight = 0.3;
        public int NStep = 3;
        public double ProbabilityManual = 1;
        public ScheduleCompact AvailabilitySchedule { get; set; }
        public double DELightGridResolution = 2;
        
        public DayLighting() { }
        public List<DayLightRefPoint> CreateZoneDayLightReferencePoints(Zone zone, List<XYZ> points, double illuminance)
        {
            List<DayLightRefPoint> dlRefPoints = new List<DayLightRefPoint>();
            double totalPoints = points.Count();
            double pControlled = Math.Round(.99 / totalPoints, 5);
            points.ForEach(p => dlRefPoints.Add(new DayLightRefPoint()
            {
                Zone = zone, Point = p, Name = "Day Light Reference Point " + (points.IndexOf(p) + 1) + " for " + zone.name,
                Illuminance = illuminance, PartControlled = pControlled
            }));
            return dlRefPoints;
        }
        public DayLighting(Zone zone, ScheduleCompact schedule, List<XYZ> points, double illuminance)
        {
            Name = "DayLight Control For " + zone.name;
            Zone = zone;
            AvailabilitySchedule = schedule;
            ReferencePoints = CreateZoneDayLightReferencePoints(zone, points, illuminance);
            zone.DayLightControl = this;
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>()
            {
                "Daylighting:Controls,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(Zone.name, "Zone Name"),
                Utility.IDFLineFormatter(DLMethod, "Daylighting Method"),
                Utility.IDFLineFormatter(AvailabilitySchedule.name, "Availability Schedule Name"),
                Utility.IDFLineFormatter(CType, "Lighting control type {1=continuous,2=stepped,3=continuous/off}"),
                Utility.IDFLineFormatter(MinPower, "Minimum input power fraction for continuous dimming control"),
                Utility.IDFLineFormatter(MinLight, "Minimum light output fraction for continuous dimming control"),
                Utility.IDFLineFormatter(NStep, "Number of steps, excluding off, for stepped control"),
                Utility.IDFLineFormatter(ProbabilityManual, "Probability electric lighting will be reset when needed"),
                Utility.IDFLineFormatter(null, "Glare Calculation Reference Point Name"),
                Utility.IDFLineFormatter(GlareCalcAngle, "Azimuth angle of view direction for glare calculation {deg}"),
                Utility.IDFLineFormatter(DiscomGlare, "Maximum discomfort glare index for window shade control"),
                Utility.IDFLineFormatter(DELightGridResolution, "DE Light Gridding Resolution")                
            };
            ReferencePoints.ForEach(p => info.AddRange(new List<string>() {
                Utility.IDFLineFormatter(p.Name, "Reference Point"),
                Utility.IDFLineFormatter(p.PartControlled, "Part Controlled"),
                Utility.IDFLineFormatter(p.Illuminance, "Illuminance Setpoint")
            }));
            return info.ReplaceLastComma();
        }
    }
    [Serializable]
    public class Material
    {
        public string name { get; set; }
        public string roughness { get; set; }
        public double thickness { get; set; }
        public double conductivity { get; set; }
        public double density { get; set; }
        public double sHC { get; set; }
        public double tAbsorptance { get; set; }
        public double sAbsorptance { get; set; }
        public double vAbsorptance { get; set; }
        public Material(string name, string rough, double th, double conduct, double dense, double sH, double tAbsorp, double sAbsorp, double vAbsorp)
        {
            this.name = name;
            roughness = rough;
            thickness = th;
            conductivity = conduct;
            density = dense;
            sHC = sH;
            tAbsorptance = tAbsorp;
            sAbsorptance = sAbsorp;
            vAbsorptance = vAbsorp;
        }
        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("Material,");
            info.Add(name + ",          !-Name");
            info.Add(roughness + ",            !-Roughness");
            info.Add(thickness + ",                    !-Thickness { m}");
            info.Add(conductivity + ",               !-Conductivity { W / m - K}");
            info.Add(density + ",                !-Density { kg / m3}");
            info.Add(sHC + ",                 !-Specific Heat { J / kg - K}");
            info.Add(tAbsorptance + ",                    !-Thermal Absorptance");
            info.Add(sAbsorptance + ",                    !-Solar Absorptance");
            info.Add(vAbsorptance + "; !-Visible Absorptance");
            return info;
        }
    }
    [Serializable]
    public class WindowMaterial
    {
        public string name { get; set; }
        public double uValue { get; set; }
        public double gValue { get; set; }
        public double vTransmittance { get; set; }
        public WindowMaterial(string n, double u, double g, double transmittance)
        {
            name = n; uValue = u; gValue = g; vTransmittance = transmittance;
        }
        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("WindowMaterial:SimpleGlazingSystem,");
            info.Add(name + ",  !- Name");
            info.Add(uValue + ",                 !- U-Factor {W/m2-K}");
            info.Add(gValue + ",                 !- Solar Heat Gain Coefficient");
            info.Add(vTransmittance + ";                     !- Visible Transmittance");
            return info;
        }
    }
    [Serializable]
    public class Construction
    {
        public string name { get; set; }
        public List<Material> layers { get; set; }
        public List<WindowMaterial> wLayers { get; set; }
        public double heatCapacity { get; set; }
        public Construction(string n, List<Material> layers)
        {
            name = n; this.layers = layers; wLayers = new List<WindowMaterial>();
            heatCapacity = layers.Select(la => la.thickness * la.sHC * la.density).Sum();
        }
        public Construction(string n, List<WindowMaterial> layers)
        {
            name = n; wLayers = layers; this.layers = new List<Material>();
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("Construction,");
            info.Add(name + ",   !- Name");

            if (wLayers.Count == 0)
            {
                for (int i = 0; i < layers.Count; i++)
                {
                    Material l = layers[i];
                    if (i != layers.Count - 1)
                    { info.Add(l.name + ",     !- Outside Layer"); }
                    else
                    { info.Add(l.name + ";     !- Outside Layer"); }
                }
            }
            else
            {
                for (int i = 0; i < wLayers.Count; i++)
                {
                    WindowMaterial l = wLayers[i];
                    if (i != wLayers.Count - 1)
                    { info.Add(l.name + ",     !- Outside Layer"); }
                    else
                    { info.Add(l.name + ";     !- Outside Layer"); }
                }
            }
            return info;
        }
    }
    [Serializable]
    public class WindowConstruction
    {
        public string name { get; set; }
        public List<WindowMaterial> layers { get; set; }
        public double uValue { get; set; }
        public double gValue { get; set; }
        public WindowConstruction(string n, List<WindowMaterial> l)
        {
            name = n; layers = l;
        }
        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("Construction,");
            info.Add(name + ",   !- Name");
            foreach (WindowMaterial l in layers)
            {
                if (l != layers.Last())
                { info.Add(l.name + ",     !- Outside Layer"); }
                else
                { info.Add(l.name + ";     !- Outside Layer"); }
            }
            return info;
        }

    }
    [Serializable]
    public class ZoneList
    {
        public List<Zone> listZones;
        public string name { get; set; }

        public ZoneList(string n)
        {
            name = n;
            listZones = new List<Zone>();
        }
    }
    [Serializable]
    public class People
    {
        public string scheduleName { get; set; }
        public string calculationMethod { get; set; }
        //public double numberOfPeople { get; set; }
        //public double peoplePerArea { get; set; }
        public double areaPerPerson { get; set; }

        public double fractionRadiant { get; set; }
        public double sensibleHeatFraction { get; set; }
        public string activityLvlSchedName { get; set; }
        public double c02genRate { get; set; }
        public string enableComfortWarnings { get; set; }
        public string meanRadiantTempCalcType { get; set; }
        public string surfaceName { get; set; }
        public string workEffSchedule { get; set; }

        public string clothingInsulationCalcMeth { get; set; }
        public string clothingInsulationCalcMethSched { get; set; }
        public string clothingInsulationSchedName { get; set; }

        public string airVelSchedName { get; set; }
        public string thermalComfModel1t { get; set; }

        public People(double aPP)
        {
            areaPerPerson = aPP;
            calculationMethod = "Area/Person";
            scheduleName = "Occupancy Schedule";
            fractionRadiant = 0.1;
            activityLvlSchedName = "People Activity Schedule";
            c02genRate = double.Parse("3.82E-8");

            enableComfortWarnings = "";
            meanRadiantTempCalcType = "ZoneAveraged";
            surfaceName = "";

            workEffSchedule = "Work Eff Sch";
            clothingInsulationCalcMeth = "DynamicClothingModelASHRAE55";
            clothingInsulationCalcMethSched = "";
            clothingInsulationSchedName = "";
            airVelSchedName = "Air Velo Sch";
            thermalComfModel1t = "Fanger";
        }
    }
    [Serializable]
    public class Light
    {
        public string scheduleName { get; set; }

        public string designLevelCalcMeth { get; set; }
        //public double lightingLevel { get; set; }
        public double wattsPerArea { get; set; }

        public double returnAirFraction { get; set; }
        public double fractionRadiant { get; set; }
        public double fractionVisible { get; set; }
        public double fractionReplaceable { get; set; }

        public Light(double wPA)
        {
            //zone = z;
            wattsPerArea = wPA;
            designLevelCalcMeth = "Watts/area";
            scheduleName = "Electric Equipment and Lighting Schedule";

            //name = "Lights " + zone.name;
            

            returnAirFraction = 0;
            fractionRadiant = 0.1;
            fractionVisible = 0.18;

            //zone.lights = this;
        }
    }
    [Serializable]
    public class ElectricEquipment
    {
        public string scheduleName { get; set; }

        public string designLevelCalcMeth { get; set; }
        //public double lightingLevel { get; set; }
        public double wattsPerArea { get; set; }

        public double fractionLatent { get; set; }
        public double fractionRadiant { get; set; }
        public double fractionLost { get; set; }

        public ElectricEquipment(double wPA)
        {
            wattsPerArea = wPA;
            designLevelCalcMeth = "Watts/area";
            scheduleName = "Electric Equipment and Lighting Schedule";
            fractionRadiant = 0.1;
        }

    }
    [Serializable]
    public abstract class ZoneHVAC
    {
        public Thermostat thermostat { get; set; }
        public ZoneHVAC(Thermostat thermostat)
        {
            this.thermostat = thermostat;
        }
    }
    [Serializable]
    public class ZoneVAV:ZoneHVAC
    {
        VAV vav;
        Zone zone;
        public ZoneVAV(VAV v, Zone z, Thermostat t):base(t)
        {
            zone = z;
            vav = v;
            thermostat = t;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>();
            

            info.Add("\r\nHVACTemplate:Zone:VAV,");
            info.Add("\t" + zone.name + ", \t\t\t\t!- Zone Name");
            info.Add("\t" + vav.name +",\t\t\t\t!-Template VAV System Name");
            info.Add("\t" + thermostat.name + ",\t\t\t\t!-Template Thermostat Name");
            info.Add("\tautosize" + ",\t\t\t\t!-Supply Air Maximum Flow Rate {m3/s}");
            info.Add("\t" + ",\t\t\t\t!-Zone Heating Sizing Factor");
            info.Add("\t" + ",\t\t\t\t!-Zone Cooling Sizing Factor");
            info.Add("\tConstant" + ",\t\t\t\t!-Zone Minimum Air Flow Input Method");
            info.Add("\t0.2" + ",\t\t\t\t!-Constant Minimum Air Flow Fraction");
            info.Add("\t" + ",\t\t\t\t!-Fixed Minimum Air Flow Rate {m3/s}");
            info.Add("\t" + ",\t\t\t\t!-Minimum Air Flow Fraction Schedule Name");
            info.Add("\tFlow/Person" + ",\t\t\t\t!-Outdoor Air Method");
            info.Add("\t0.00944" + ",\t\t\t\t!-Outdoor Air Flow Rate per Person {m3/s}");
            info.Add("\t" + ",\t\t\t\t!-Outdoor Air Flow Rate per Zone Floor Area {m3/s-m2}");
            info.Add("\t" + ",\t\t\t\t!-Outdoor Air Flow Rate per Zone {m3/s}");
            info.Add("\tHotWater" + ",\t\t\t\t!-Reheat Coil Type");
            info.Add("\t" + ",\t\t\t\t!-Reheat Coil Availability Schedule Name");
            info.Add("\tReverse" + ",\t\t\t\t!-Damper Heating Action");
            info.Add("\t" + ",\t\t\t\t!-Maximum Flow per Zone Floor Area During Reheat {m3/s-m2}");
            info.Add("\t" + ",\t\t\t\t!-Maximum Flow Fraction During Reheat");
            info.Add("\t" + ",\t\t\t\t!-Maximum Reheat Air Temperature {C}");
            info.Add("\t" + ",\t\t\t\t!-Design Specification Outdoor Air Object Name for Control");
            info.Add("\t" + ",\t\t\t\t!-Supply Plenum Name");
            info.Add("\t" + ",\t\t\t\t!-Return Plenum Name");
            info.Add("\tNone" + ",\t\t\t\t!-Baseboard Heating Type");
            info.Add("\t" + ",\t\t\t\t!-Baseboard Heating Availability Schedule Name");
            info.Add("\tautosize" + ",\t\t\t\t!-Baseboard Heating Capacity {W}");
            info.Add("\tSystemSupplyAirTemperature" + ",\t\t\t\t!-Zone Cooling Design Supply Air Temperature Input Method");
            info.Add("\t" + ",\t\t\t\t!-Zone Cooling Design Supply Air Temperature {C}");
            info.Add("\t" + ",\t\t\t\t!-Zone Cooling Design Supply Air Temperature Difference {deltaC}");
            info.Add("\tSupplyAirTemperature" + ",\t\t\t\t!-Zone Heating Design Supply Air Temperature Input Method");
            info.Add("\t50" + ",\t\t\t\t!-Zone Heating Design Supply Air Temperature {C}");
            info.Add("\t" + ";\t\t\t\t!-Zone Heating Design Supply Air Temperature Difference {deltaC}");
            


            return info;
        }
    }
    [Serializable]
    public class ZoneFanCoilUnit:ZoneHVAC
    {
        Zone zone;
        public ZoneFanCoilUnit(Zone z, Thermostat thermostat): base(thermostat)
        {
            zone = z;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Zone:FanCoil,",
                zone.name + ",                  !- Zone Name",
                thermostat.name + ",              !- Template Thermostat Name",
                "autosize,                !- Supply Air Maximum Flow Rate {m3/s}",
                ",                        !- Zone Heating Sizing Factor",
                ",                        !- Zone Cooling Sizing Factor",
                "flow/person,             !- Outdoor Air Method",
                "0.00944,                 !- Outdoor Air Flow Rate per Person {m3/s}",
                "0.0,                     !- Outdoor Air Flow Rate per Zone Floor Area {m3/s-m2}",
                "0.0,                     !- Outdoor Air Flow Rate per Zone {m3/s}",
                ",                        !- System Availability Schedule Name",
                "0.7,                     !- Supply Fan Total Efficiency",
                "75,                      !- Supply Fan Delta Pressure {Pa}",
                "0.9,                     !- Supply Fan Motor Efficiency",
                "1,                       !- Supply Fan Motor in Air Stream Fraction",
                "ChilledWater,            !- Cooling Coil Type",
                ",                        !- Cooling Coil Availability Schedule Name",
                "12.5,                    !- Cooling Coil Design Setpoint {C}",
                "HotWater,                !- Heating Coil Type",
                ",                        !- Heating Coil Availability Schedule Name",
                "50,                      !- Heating Coil Design Setpoint {C}",
                ",                        !- Dedicated Outdoor Air System Name",
                "SupplyAirTemperature,    !- Zone Cooling Design Supply Air Temperature Input Method",
                ",                        !- Zone Cooling Design Supply Air Temperature Difference {deltaC}",
                "SupplyAirTemperature,    !- Zone Heating Design Supply Air Temperature Input Method",
                ",                        !- Zone Heating Design Supply Air Temperature Difference {deltaC}",
                ",                        !- Design Specification Outdoor Air Object Name",
                ",                        !- Design Specification Zone Air Distribution Object Name",
                "ConstantFanVariableFlow, !- Capacity Control Method",
                ",                        !- Low Speed Supply Air Flow Ratio",
                ",                        !- Medium Speed Supply Air Flow Ratio",
                "Occupancy Schedule,      !- Outdoor Air Schedule Name",
                "None,                    !- Baseboard Heating Type",
                ",                        !- Baseboard Heating Availability Schedule Name",
                "Autosize; !-Baseboard Heating Capacity { W}"
            };
            
            return info;
        }
    }
    [Serializable]
    public class ZoneBaseBoardHeat
    {
        Zone zone;
        Thermostat thermostat;

        public ZoneBaseBoardHeat(Zone z, Thermostat t)
        {
            zone = z;
            thermostat = t;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Zone:BaseBoardHeat,",
                zone.name + ", !-Zone Name",
                thermostat.name + ", !-Template Thermostat Name",
                "1.2, !-Zone Heating Sizing Factor",
                "Electric, !-Baseboard Heating Type",
                ", !-Baseboard Heating Availability Schedule Name",
                "Autosize, !-Baseboard Heating Capacity { W}",
                ", !-Dedicated Outdoor Air System Name",
                "flow/person , !-Outdoor Air Method",
                "0.00944, !-Outdoor Air Flow Rate per Person { m3 / s}",
                "0.0, !-Outdoor Air Flow Rate per Zone Floor Area { m3 / s - m2}",
                "0.0, !-Outdoor Air Flow Rate per Zone { m3 / s}",
                ", !-Design Specification Outdoor Air Object Name",
                "; !-Design Specification Zone Air Distribution Object Name"             
            };

            return info;
        }
    }
    [Serializable]
    public class ZoneIdealLoad:ZoneHVAC
    {
        Zone zone;
        public ZoneIdealLoad(Zone z, Thermostat thermostat):base(thermostat)
        {
            zone = z;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("HVACTemplate:Zone:IdealLoadsAirSystem,");
            info.Add("\t" + zone.name + ",\t\t\t\t\t\t!- Zone Name");
            info.Add("\t" + thermostat.name + ", \t\t\t\t!- Template Thermostat Name");
            info.Add("\t, \t\t\t\t!- System Availability Schedule Name");
            info.Add("\t50, \t\t\t\t!- Maximum Heating Supply Air Temperature {C}");
            info.Add("\t13, \t\t\t\t!- Minimum Cooling Supply Air Temperature {C}");
            info.Add("\t0.0156, \t\t\t\t!- Maximum Heating Supply Air Humidity Ratio {kgWater/kgDryAir}");
            info.Add("\t0.0077, \t\t\t\t!- Minimum Cooling Supply Air Humidity Ratio {kgWater/kgDryAir}");
            info.Add("\tNoLimit, \t\t\t\t!- Heating Limit");
            info.Add("\t, \t\t\t\t!- Maximum Heating Air Flow Rate {m3/s}");
            info.Add("\t, \t\t\t\t!- Maximum Sensible Heating Capacity {W}");
            info.Add("\tNoLimit, \t\t\t\t!- Cooling Limit");
            info.Add("\t, \t\t\t\t!- Maximum Cooling Air Flow Rate {m3/s}");
            info.Add("\t, \t\t\t\t!- Maximum Total Cooling Capacity {W}");
            info.Add("\t, \t\t\t\t!- Heating Availability Schedule Name");
            info.Add("\t, \t\t\t\t!- Cooling Availability Schedule Name");
            info.Add("\tConstantSensibleHeatRatio, \t\t\t\t!- Dehumidification Control Type");
            info.Add("\t0.7, \t\t\t\t!- Cooling Sensible Heat Ratio {dimensionless}");
            info.Add("\t60, \t\t\t\t!- Dehumidification Setpoint {percent}");
            info.Add("\tNone, \t\t\t\t!- Humidification Control Type");
            info.Add("\t30, \t\t\t\t!- Humidification Setpoint {percent}");
            info.Add("\tNone, \t\t\t\t!- Outdoor Air Method");
            info.Add("\t0.00944, \t\t\t\t!- Outdoor Air Flow Rate per Person {m3/s}");
            info.Add("\t, \t\t\t\t!- Outdoor Air Flow Rate per Zone Floor Area {m3/s-m2}");
            info.Add("\t, \t\t\t\t!- Outdoor Air Flow Rate per Zone {m3/s}");
            info.Add("\t, \t\t\t\t!- Design Specification Outdoor Air Object Name");
            info.Add("\tNone, \t\t\t\t!- Demand Controlled Ventilation Type");
            info.Add("\tNoEconomizer, \t\t\t\t!- Outdoor Air Economizer Type");
            info.Add("\tNone, \t\t\t\t!- Heat Recovery Type");
            info.Add("\t0.7, \t\t\t\t!- Sensible Heat Recovery Effectiveness {dimensionless}");
            info.Add("\t0.65; \t\t\t\t!- Latent Heat Recovery Effectiveness {dimensionless} \r\n");
            return info;
        }
    }
    [Serializable]
    public class VAV
    {
        public string name;
        public VAV()
        {
            name = "VAV";
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: HVACTEMPLATE:SYSTEM:VAV ===========\r\n");

            info.Add("\r\nHVACTemplate:System:VAV,");
            info.Add("\t" + name + ", \t\t\t\t!- Name");
            info.Add("\t" + ", \t\t\t\t!- System Availability Schedule Name");
            info.Add("\tautosize" + ", \t\t\t\t!- Supply Fan Maximum Flow Rate {m3/s}");
            info.Add("\tautosize" + ", \t\t\t\t!- Supply Fan Minimum Flow Rate {m3/s}");
            info.Add("\t0.7" + ", \t\t\t\t!- Supply Fan Total Efficiency");
            info.Add("\t1000" + ", \t\t\t\t!- Supply Fan Delta Pressure {Pa}");
            info.Add("\t0.9" + ", \t\t\t\t!- Supply Fan Motor Efficiency");
            info.Add("\t1" + ", \t\t\t\t!- Supply Fan Motor in Air Stream Fraction");
            info.Add("\tChilledWater" + ", \t\t\t\t!- Cooling Coil Type");
            info.Add("\t" + ", \t\t\t\t!- Cooling Coil Availability Schedule Name");
            info.Add("\t" + ", \t\t\t\t!- Cooling Coil Setpoint Schedule Name");
            info.Add("\t12.8" + ", \t\t\t\t!- Cooling Coil Design Setpoint {C}");
            info.Add("\tHotWater" + ", \t\t\t\t!- Heating Coil Type");
            info.Add("\t" + ", \t\t\t\t!- Heating Coil Availability Schedule Name");
            info.Add("\t" + ", \t\t\t\t!- Heating Coil Setpoint Schedule Name");
            info.Add("\t10" + ", \t\t\t\t!- Heating Coil Design Setpoint {C}");
            info.Add("\t0.8" + ", \t\t\t\t!- Gas Heating Coil Efficiency");
            info.Add("\t" + ", \t\t\t\t!- Gas Heating Coil Parasitic Electric Load {W}");
            info.Add("\tNone" + ", \t\t\t\t!- Preheat Coil Type");
            info.Add("\t" + ", \t\t\t\t!- Preheat Coil Availability Schedule Name");
            info.Add("\t" + ", \t\t\t\t!- Preheat Coil Setpoint Schedule Name");
            info.Add("\t7.2" + ", \t\t\t\t!- Preheat Coil Design Setpoint {C}");
            info.Add("\t0.8" + ", \t\t\t\t!- Gas Preheat Coil Efficiency");
            info.Add("\t" + ", \t\t\t\t!- Gas Preheat Coil Parasitic Electric Load {W}");
            info.Add("\tautosize" + ", \t\t\t\t!- Maximum Outdoor Air Flow Rate {m3/s}");
            info.Add("\tautosize" + ", \t\t\t\t!- Minimum Outdoor Air Flow Rate {m3/s}");
            info.Add("\tProportionalMinimum" + ", \t\t\t\t!- Minimum Outdoor Air Control Type");
            info.Add("\t" + ", \t\t\t\t!- Minimum Outdoor Air Schedule Name");
            info.Add("\tNoEconomizer" + ", \t\t\t\t!- Economizer Type");
            info.Add("\tNoLockout" + ", \t\t\t\t!- Economizer Lockout");
            info.Add("\t" + ", \t\t\t\t!- Economizer Upper Temperature Limit {C}");
            info.Add("\t" + ", \t\t\t\t!- Economizer Lower Temperature Limit {C}");
            info.Add("\t" + ", \t\t\t\t!- Economizer Upper Enthalpy Limit {J/kg}");
            info.Add("\t" + ", \t\t\t\t!- Economizer Maximum Limit Dewpoint Temperature {C}");
            info.Add("\t" + ", \t\t\t\t!- Supply Plenum Name");
            info.Add("\t" + ", \t\t\t\t!- Return Plenum Name");
            info.Add("\tDrawThrough" + ", \t\t\t\t!- Supply Fan Placement");
            info.Add("\tInletVaneDampers" + ", \t\t\t\t!- Supply Fan Part-Load Power Coefficients");
            info.Add("\tStayOff" + ", \t\t\t\t!- Night Cycle Control");
            info.Add("\t" + ", \t\t\t\t!- Night Cycle Control Zone Name");
            info.Add("\tNone" + ", \t\t\t\t!- Heat Recovery Type");
            info.Add("\t0.7" + ", \t\t\t\t!- Sensible Heat Recovery Effectiveness");
            info.Add("\t0.65" + ", \t\t\t\t!- Latent Heat Recovery Effectiveness");
            info.Add("\tNone" + ", \t\t\t\t!- Cooling Coil Setpoint Reset Type");
            info.Add("\tNone" + ", \t\t\t\t!- Heating Coil Setpoint Reset Type");
            info.Add("\tNone" + ", \t\t\t\t!- Dehumidification Control Type");
            info.Add("\t" + ", \t\t\t\t!- Dehumidification Control Zone Name");
            info.Add("\t60" + ", \t\t\t\t!- Dehumidification Setpoint {percent}");
            info.Add("\tNone" + ", \t\t\t\t!- Humidifier Type");
            info.Add("\t" + ", \t\t\t\t!- Humidifier Availability Schedule Name");
            info.Add("\t0.000001" + ", \t\t\t\t!- Humidifier Rated Capacity {m3/s}");
            info.Add("\tautosize" + ", \t\t\t\t!- Humidifier Rated Electric Power {W}");
            info.Add("\t" + ", \t\t\t\t!- Humidifier Control Zone Name");
            info.Add("\t30" + ", \t\t\t\t!- Humidifier Setpoint {percent}");
            info.Add("\tNonCoincident" + ", \t\t\t\t!- Sizing Option");
            info.Add("\tNo" + ", \t\t\t\t!- Return Fan");
            info.Add("\t0.7" + ", \t\t\t\t!- Return Fan Total Efficiency");
            info.Add("\t500" + ", \t\t\t\t!- Return Fan Delta Pressure {Pa}");
            info.Add("\t0.9" + ", \t\t\t\t!- Return Fan Motor Efficiency");
            info.Add("\t1" + ", \t\t\t\t!- Return Fan Motor in Air Stream Fraction");
            info.Add("\tInletVaneDampers" + "; \t\t\t\t!- Return Fan Part-Load Power Coefficients");
            
            return info;
        }


    }
    [Serializable]
    public class ChilledWaterLoop
    {
        string name;

        public ChilledWaterLoop()
        {
            name = "Chilled Water Loop";
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Plant:ChilledWaterLoop,",
                name + ",      !- Name",
                ",                        !- Pump Schedule Name",
                "Intermittent,            !- Pump Control Type",
                "Default,                 !- Chiller Plant Operation Scheme Type",
                ",                        !- Chiller Plant Equipment Operation Schemes Name",
                ",                        !- Chilled Water Setpoint Schedule Name",
                "7.22,                    !- Chilled Water Design Setpoint {C}",
                "VariablePrimaryNoSecondary,  !- Chilled Water Pump Configuration",
                "179352,                  !- Primary Chilled Water Pump Rated Head {Pa}",
                "179352,                  !- Secondary Chilled Water Pump Rated Head {Pa}",
                "Default,                 !- Condenser Plant Operation Scheme Type",
                ",                        !- Condenser Equipment Operation Schemes Name",
                "OutdoorWetBulbTemperature,  !- Condenser Water Temperature Control Type",
                ",                        !- Condenser Water Setpoint Schedule Name",
                "29.4,                    !- Condenser Water Design Setpoint {C}",
                "179352,                  !- Condenser Water Pump Rated Head {Pa}",
                "None,                    !- Chilled Water Setpoint Reset Type",
                "12.2,                    !- Chilled Water Setpoint at Outdoor Dry-Bulb Low {C}",
                "15.6,                    !- Chilled Water Reset Outdoor Dry-Bulb Low {C}",
                "6.7,                     !- Chilled Water Setpoint at Outdoor Dry-Bulb High {C}",
                "26.7,                    !- Chilled Water Reset Outdoor Dry-Bulb High {C}",
                "SinglePump,              !- Chilled Water Primary Pump Type",
                "SinglePump,              !- Chilled Water Secondary Pump Type",
                "SinglePump,              !- Condenser Water Pump Type",
                "Yes,                     !- Chilled Water Supply Side Bypass Pipe",
                "Yes,                     !- Chilled Water Demand Side Bypass Pipe",
                "Yes,                     !- Condenser Water Supply Side Bypass Pipe",
                "Yes,                     !- Condenser Water Demand Side Bypass Pipe",
                "Water,                   !- Fluid Type",
                "6.67,                    !- Loop Design Delta Temperature {deltaC}",
                ",                        !- Minimum Outdoor Dry Bulb Temperature {C}",
                "SequentialLoad,          !- Chilled Water Load Distribution Scheme",
                "SequentialLoad; !-Condenser Water Load Distribution Scheme"
            };
            
            return info;
        }

    }
    [Serializable]
    public class Chiller
    {
        string name;
        double chillerCOP;

        public Chiller(double COP)
        {
            name = "Main Chiller";
            chillerCOP = COP;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Plant:Chiller,",
                name + ",            !-Name",
                "ElectricCentrifugalChiller,  !-Chiller Type",
                "autosize,                !-Capacity { W}",
                chillerCOP + ",                     !-Nominal COP { W / W}",
                "WaterCooled,             !-Condenser Type",
                "1,                       !-Priority",
                "1,                       !-Sizing Factor",
                "0.1,                     !-Minimum Part Load Ratio",
                "1.1,                     !-Maximum Part Load Ratio",
                "0.9,                     !-Optimum Part Load Ratio",
                "0.2,                     !-Minimum Unloading Ratio",
                "2; !-Leaving Chilled Water Lower Temperature Limit { C}"
            };
            return info;
        }

    }
    [Serializable]
    public class Tower
    {
        string name; 

        public Tower()
        {
            name = "Main Tower";
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "\r\nHVACTemplate:Plant:Tower,",
                "\t" + name + ", \t\t\t\t!- Name",
                "\tSingleSpeed" + ",\t\t\t\t!-Tower Type",
                "\tautosize" + ", \t\t\t\t!-High Speed Nominal Capacity {W}",
                "\tautosize" + ",\t\t\t\t!-High Speed Fan Power {W}",
                "\tautosize" + ", \t\t\t\t!-Low Speed Nominal Capacity {W}",
                "\tautosize" + ", \t\t\t\t!-Low Speed Fan Power {W}",
                "\tautosize" + ", \t\t\t\t!-Free Convection Capacity {W}",
                "\t1" + ", \t\t\t\t!-Priority",
                "\t1.2" + "; \t\t\t\t!-Sizing Factor"
            };
            return info;
        }

    }
    [Serializable]
    public class HotWaterLoop
    {
        string name;

        public HotWaterLoop()
        {
            name = "Hot Water Loop";
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>() {
                "HVACTemplate:Plant:HotWaterLoop,",
                name + ",          !- Name",
                ",                        !- Pump Schedule Name",
                "Intermittent,            !- Pump Control Type",
                "Default,                 !- Hot Water Plant Operation Scheme Type",
                ",                        !- Hot Water Plant Equipment Operation Schemes Name",
                ",                        !- Hot Water Setpoint Schedule Name",
                "82,                      !- Hot Water Design Setpoint {C}",
                "VariableFlow,            !- Hot Water Pump Configuration",
                "179352,                  !- Hot Water Pump Rated Head {Pa}",
                "None,                    !- Hot Water Setpoint Reset Type",
                "82.2,                    !- Hot Water Setpoint at Outdoor Dry-Bulb Low {C}",
                "-6.7,                    !- Hot Water Reset Outdoor Dry-Bulb Low {C}",
                "65.6,                    !- Hot Water Setpoint at Outdoor Dry-Bulb High {C}",
                "10,                      !- Hot Water Reset Outdoor Dry-Bulb High {C}",
                "SinglePump,              !- Hot Water Pump Type",
                "Yes,                     !- Supply Side Bypass Pipe",
                "Yes,                     !- Demand Side Bypass Pipe",
                "Water,                   !- Fluid Type",
                "11,                      !- Loop Design Delta Temperature {deltaC}",
                ",                        !- Maximum Outdoor Dry Bulb Temperature {C}",
                "SequentialLoad; !-Load Distribution Scheme"
            };

            return info;
        }
    }
    [Serializable]
    public class Boiler
    {
        public string name;
        public double boilerEfficiency;
        public string fuelType;

        public Boiler(double efficiency, string fuelType)
        {
            name = "Main Boiler";
            boilerEfficiency = efficiency;
            this.fuelType = fuelType;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>() {
                "HVACTemplate:Plant:Boiler,",
                name + ",             !-Name",
                "HotWaterBoiler,          !-Boiler Type",
                "autosize,                !-Capacity { W}",
                boilerEfficiency + ",                     !-Efficiency",
                Utility.IDFLineFormatter(fuelType,  "Fuel Type"),
                "1,                       !-Priority",
                "1.2,                     !-Sizing Factor",
                "0.1,                     !-Minimum Part Load Ratio",
                "1.1,                     !-Maximum Part Load Ratio",
                "0.9,                     !-Optimum Part Load Ratio",
                "99.9; !-Water Outlet Upper Temperature Limit { C}"
            };

            return info;
        }
    }
    [Serializable]
    public class Thermostat
    {
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
    [Serializable]
    public class ZoneVentilation
    {
        public double airChangesHour { get; set; }
        public string scheduleName { get; set; }
        public ZoneVentilation(double acH)
        {
            scheduleName = "Ventilation Schedule";
            airChangesHour = acH;

        }
        public ZoneVentilation()
        {
            scheduleName = "Ventilation Schedule";
            airChangesHour = 0;
        }
    }
    [Serializable]
    public class ZoneInfiltration
    {
        public double airChangesHour { get; set; }
        public double constantTermCoeff { get; set; }
        public double temperatureTermCoef { get; set; }
        public double velocityTermCoef { get; set; }
        public double velocitySquaredTermCoef { get; set; }

        public ZoneInfiltration(double acH)
        {
            airChangesHour = acH;

            constantTermCoeff = 0.606;
            temperatureTermCoef = 0.036359996;
            velocityTermCoef = 0.1177165;
            velocitySquaredTermCoef = 0;

        }
    }
    [Serializable]
    public class ShadingOverhang
    {
        public BuildingSurface face { get; set; }
        public XYZList listVertice { get; set; }

        public ShadingOverhang(BuildingSurface face1)
        {
            face = face1;

            switch (face.direction)
            {
                case Direction.North:
                    listVertice = createShadingY(face.verticesList, face.sl).reverse();
                    break;
                case Direction.South:
                    listVertice = createShadingY(face.verticesList, -face.sl).reverse();
                    break;
                case Direction.East:
                    listVertice = createShadingX(face.verticesList, face.sl).reverse();
                    break;
                case Direction.West:
                    listVertice = createShadingX(face.verticesList, -face.sl).reverse();
                    break;
            }

            XYZList createShadingY(XYZList listVertices, double sl)
            {
                XYZ P1 = listVertices.xyzs.ElementAt(0);
                XYZ P2 = listVertices.xyzs.ElementAt(1);
                XYZ P3 = listVertices.xyzs.ElementAt(2);
                XYZ P4 = listVertices.xyzs.ElementAt(3);

                double shadingLength = sl;

                XYZ pmid = new XYZ((P1.X + P3.X) / 2, P1.Y, (P1.Z + P3.Z) / 2);
                double Y = pmid.Y;
                double Z = P1.Z;

                XYZ Vertice1 = new XYZ(P2.X, Y, Z);
                XYZ Vertice2 = new XYZ(P2.X + shadingLength, Y + shadingLength, Z);
                XYZ Vertice3 = new XYZ(P1.X - shadingLength, Y + shadingLength, Z);
                XYZ Vertice4 = new XYZ(P1.X, Y, Z);

                return new XYZList(new List<XYZ>() { Vertice1, Vertice2, Vertice3, Vertice4 });
            }

            XYZList createShadingX(XYZList listVertices, double sl)
            {
                XYZ P1 = listVertices.xyzs.ElementAt(0);
                XYZ P2 = listVertices.xyzs.ElementAt(1);
                XYZ P3 = listVertices.xyzs.ElementAt(2);
                XYZ P4 = listVertices.xyzs.ElementAt(3);

                double shadingLength = sl;

                XYZ pmid = new XYZ((P1.X + P3.X) / 2, P1.Y, (P1.Z + P3.Z) / 2);
                double X = pmid.X;
                double Z = P1.Z;

                XYZ Vertice1 = new XYZ(X, P2.Y, Z);
                XYZ Vertice2 = new XYZ(X + shadingLength, P2.Y - shadingLength, Z);
                XYZ Vertice3 = new XYZ(X + shadingLength, P1.Y + shadingLength, Z);
                XYZ Vertice4 = new XYZ(X, P1.Y, Z);

                return new XYZList(new List<XYZ>() { Vertice1, Vertice2, Vertice3, Vertice4 });
            }
        }



        public List<string> shadingInfo()
        {

            List<string> info = new List<string>();

            info.Add("Shading:Zone:Detailed,");
            info.Add("\t" + "Shading_On_" + face.name + ",\t!- Name");
            info.Add("\t" + face.name + ",\t!-Base Surface Name)");
            info.Add("\t,\t\t\t\t\t\t!-Transmittance Schedule Name");



            info.AddRange(listVertice.verticeInfo());

            return info;
        }
    }
    [Serializable]
    public class OverhangProjection
    {
        public Fenestration window;
        public double depthf;

        public OverhangProjection(Fenestration win, double df)
        {
            window = win;
            depthf = df;

        }

        public List<string> OverhangInfo()
        {

            List<string> info = new List<string>();

            info.Add("Shading:Overhang:Projection,");
            info.Add("\t" + "Shading_On_" + window.surfaceType + "_On_" + window.face.name + ",\t!- Name");
            info.Add("\t" + window.surfaceType + "_On_" + window.face.name + ",\t!-Window or Door Name");
            info.Add("\t0,\t\t\t\t\t\t!-Height above Window or Door {m}");
            info.Add("\t90,\t\t\t\t\t\t!-Tilt Angle from Window/Door {deg}");
            info.Add("\t.2,\t\t\t\t\t\t!-Left extension from Window/Door Width {m}");
            info.Add("\t.2,\t\t\t\t\t\t!-Right extension from Window/Door Width {m}");
            info.Add("\t" + depthf +";\t\t\t\t\t\t!-Depth as Fraction of Window/Door Height {m}");


            return info;
        }

    }
    [Serializable]
    public class ScheduleYearly
    {
        public string name { get; set; }
        public ScheduleWeekly scheduleWeekly { get; set; }
        public ScheduleLimits scheduleLimits { get; set; }
        public int startDay { get; set; }
        public int startMonth { get; set; }
        public int endDay { get; set;}
        public int endMonth { get; set; }
        public List<string> writeSchedule()
        {
            List<string> info = new List<string>();
            info.Add("Schedule:Year,");
            info.Add(name + ",\t\t\t\t!-Name");

            if (scheduleLimits != null) info.Add(scheduleLimits.name + ", \t\t\t\t!-Schedule Type Limits Name");
            else info.Add(", \t\t\t\t!-Schedule Type Limits Name");

            info.Add(scheduleWeekly.name + ",  \t\t\t\t!- Schedule:Week Name 1");
            info.Add(startMonth + ",  \t\t\t\t!- Start Month 1");
            info.Add(startDay + ",  \t\t\t\t!- Start Day 1");
            info.Add(endMonth + ",  \t\t\t\t!- End Month 1");
            info.Add(endDay + ";  \t\t\t\t!- End Day 1");
            return info;
        }
        public ScheduleYearly(ScheduleWeekly schedule)
        {
            scheduleWeekly = schedule;
            startDay = 1; startMonth = 1;
            endMonth = 12; endDay = 31;
        }
    }
    [Serializable]
    public class ScheduleWeekly
    {
        public string name { get; set; }
        public ScheduleDaily weekday { get; set; }
        public ScheduleDaily saturday { get; set; }
        public ScheduleDaily sunday { get; set; }
        public ScheduleDaily holiday { get; set; }
        public ScheduleDaily summerDesignday { get; set; }
        public ScheduleDaily winterDesignday { get; set; }
        public ScheduleDaily customDay1 { get; set; }
        public ScheduleDaily customDay2 { get; set; }

        public ScheduleWeekly(ScheduleDaily schedule)
        {
            weekday = schedule; saturday = schedule; sunday = schedule;
            holiday = schedule; summerDesignday = schedule; winterDesignday = schedule;
            customDay1 = schedule; customDay2 = schedule;
        }

        public ScheduleWeekly(ScheduleDaily week, ScheduleDaily weekend, ScheduleDaily hol)
        {
            weekday = week; saturday = weekend; sunday = weekend;
            holiday = hol;
            summerDesignday = week; winterDesignday = week;
            customDay1 = hol; customDay2 = hol;
        }


        public List<string> writeSchedule()
        {
            List<string> info = new List<string>();
            info.Add("Schedule:Week:Daily,");
            info.Add(name + ", \t\t\t\t!-Name");
            info.Add(sunday.name + ", \t\t\t\t!- Sunday Schedule:Day Name");
            info.Add(weekday.name + ",  \t\t\t\t!- Monday Schedule:Day Name");
            info.Add(weekday.name + ",  \t\t\t\t!- Tuesday Schedule:Day Name");
            info.Add(weekday.name + ",  \t\t\t\t!- Wednesday Schedule:Day Name");
            info.Add(weekday.name + ",  \t\t\t\t!- Thursday Schedule:Day Name");
            info.Add(weekday.name + ",  \t\t\t\t!- Friday Schedule:Day Name");
            info.Add(saturday.name + ",  \t\t\t\t!- Saturday Schedule:Day Name");
            info.Add(holiday.name + ", \t\t\t\t!- Holiday Schedule:Day Name");
            info.Add(summerDesignday.name + ", \t\t\t\t!- SummerDesignDay Schedule:Day Name");
            info.Add(winterDesignday.name + ",  \t\t\t\t!- WinterDesignDay Schedule:Day Name");
            info.Add(customDay1.name + ",  \t\t\t\t!- CustomDay1 Schedule:Day Name");
            info.Add(customDay2.name + ";  \t\t\t\t!- CustomDay2 Schedule:Day Name");
            return info;
        }
    }
    [Serializable]
    public class ScheduleDaily
    {
        public string name { get; set; }
        public ScheduleLimits scheduleLimits { get; set; }
        public string interpolate { get; set; }
        public int hour1 { get; set; }
        public int minutes1 { get; set; }
        public double value1 { get; set; }
        public int hour2 { get; set; }
        public int minutes2 { get; set; }
        public double value2 { get; set; }

        public ScheduleDaily()
        {
            interpolate = "No";
            hour1 = 0; minutes1 = 0; hour2 = 0; minutes2 = 0;
        }

        public List<String> writeSchedule()
        {
            List<string> info = new List<string>();
            info.Add("Schedule:Day:Interval,");
            info.Add(name + ",\t\t\t\t!-Name");

            if (scheduleLimits != null) info.Add(scheduleLimits.name + ", \t\t\t\t!-Schedule Type Limits Name");
            else info.Add(", \t\t\t\t!-Schedule Type Limits Name");

            info.Add(interpolate + ",  \t\t\t\t!- Interpolate to Timestep");

            if (!(hour1 == 0 && minutes1 == 0))
            {
                if (minutes1 < 10)
                {
                    info.Add(hour1 + ":0" + minutes1 + ",  \t\t\t\t!- Time 1 {hh:mm}");
                }
                else
                {
                    info.Add(hour1 + ":" + minutes1 + ",  \t\t\t\t!- Time 1 {hh:mm}");
                }
                info.Add(value1 + ",  \t\t\t\t!- Value Until Time 1");
            }
            if (!(hour2 == 0 && minutes2 == 0))
            {
                if (minutes2 < 10)
                {
                    info.Add(hour2 + ":0" + minutes2 + ",  \t\t\t\t!- Time 2 {hh:mm}");
                }
                else
                {
                    info.Add(hour2 + ":" + minutes2 + ",  \t\t\t\t!- Time 2 {hh:mm}");
                }
                info.Add(value2 + ",  \t\t\t\t!- Value Until Time 2");
            }
            info.Add("24:00" + ",  \t\t\t\t!- Time end {hh:mm}");
            info.Add(value1 + ";  \t\t\t\t!- Value Until Time end");
            return info;
        }
    
    }
    [Serializable]
    public class ScheduleLimits
    {
        public string name { get; set; }
        public double lowerLimit { get; set; }
        public double upperLimit { get; set; }
        public string numericType { get; set; }
        public string unitType { get; set; }

        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("ScheduleTypeLimits,");
            info.Add(name + ",\t\t\t\t!-Name");
            info.Add(lowerLimit + ", \t\t\t\t!- Lower Limit Value");

            if(upperLimit > lowerLimit) info.Add(upperLimit + ",  \t\t\t\t!- Upper Limit Value");
            else info.Add(",  \t\t\t\t!- Upper Limit Value");

            info.Add(numericType + ",  \t\t\t\t!- Numeric Type");
            info.Add(unitType + ";  \t\t\t\t!- Unit Type");
            return info;
        }
        public ScheduleLimits()
        {
            numericType = "Continuous";
            unitType = "";
        }
    }
    [Serializable]
    public class ScheduleCompact
    {
        public string name { get; set; }
        public ScheduleLimits scheduleLimits { get; set; }
        public double value { get; set; }
        public Dictionary<string, Dictionary<string, double>> daysTimeValue;

        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("Schedule:compact,");
            info.Add(name + ",\t\t\t\t!-Name");
            if (scheduleLimits != null) { info.Add(scheduleLimits.name + ", \t\t\t\t!-Schedule Type Limits Name"); }
            else { info.Add(", \t\t\t\t!-Schedule Type Limits Name"); }
            info.Add("Through: 12/31,  \t\t\t\t!-Field1");

            foreach(KeyValuePair<string, Dictionary<string, double>> kV in daysTimeValue)
            {
                info.Add("For: " + kV.Key + ",");
                foreach (KeyValuePair<string, double> tValue in kV.Value)
                {
                    info.Add("Until: " + tValue.Key + "," + tValue.Value + ",");
                }
            }
            string lastRow = info.Last();
            lastRow = lastRow.Remove(lastRow.Length - 1, 1) + ";";
            info.RemoveAt(info.Count - 1);
            info.Add(lastRow);
            return info;
        }
    }
    [Serializable]
    public class Version
    {
        public double VersionIdentifier { get; set; }
        public Version()
        {
            VersionIdentifier = 8.7;
        }
        public List<string> WriteInfo()
        {
            return new List<string>() { "Version,", Utility.IDFLastLineFormatter(VersionIdentifier, "Version Identifier") };
        }
    }
    [Serializable]
    public class SimulationControl
    {
        public string doZoneSizingCalculation { get; set; }
        public string doSystemSizingCalculation { get; set; }
        public string doPlantSizingCalculation { get; set; }
        public string runSimulationForSizingPeriods { get; set; }
        public string runSimulationForWeatherFileRunPeriods { get; set; }

        public SimulationControl()
        {
            doZoneSizingCalculation = "Yes";
            doSystemSizingCalculation = "Yes";
            doPlantSizingCalculation = "Yes";
            runSimulationForSizingPeriods = "No";
            runSimulationForWeatherFileRunPeriods = "Yes";
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "SimulationControl,",
                Utility.IDFLineFormatter(doZoneSizingCalculation, "Do Zone Sizing Calculation"),
                Utility.IDFLineFormatter(doSystemSizingCalculation, "Do System Sizing Calculation"),
                Utility.IDFLineFormatter(doPlantSizingCalculation, "Do Plant Sizing Calculation"),
                Utility.IDFLineFormatter(runSimulationForSizingPeriods, "Run Simulation for Sizing Periods"),
                Utility.IDFLastLineFormatter(runSimulationForWeatherFileRunPeriods, "Run Simulation for Sizing Periods"),
            };
        }
    }
    [Serializable]
    public class Timestep
    {
        public int NumberOfTimestepsPerHour { get; set; }

        public Timestep(int numberOfTimestepsPerHour)
        {
            this.NumberOfTimestepsPerHour = numberOfTimestepsPerHour;
        }
        public List<string> WriteInfo()
        {
            return new List<string>() { "Timestep,", Utility.IDFLastLineFormatter(NumberOfTimestepsPerHour, "Number of Timesteps per Hour") };
        }
    }
    [Serializable]
    public class ConvergenceLimits
    {
        public int minimumSystemTimestep { get; set; }
        public int maximumHVACIterations { get; set; }
        public int minimumPlantIterations { get; set; }
        public int maximumPlantIterations { get; set; }

        public ConvergenceLimits()
        {
            minimumSystemTimestep = 0;
            maximumHVACIterations = 20;
            minimumPlantIterations = 2;
            maximumPlantIterations = 8;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "ConvergenceLimits,",
                Utility.IDFLineFormatter(minimumSystemTimestep, "Minimum System Timestep {minutes}"),
                Utility.IDFLineFormatter(maximumHVACIterations, "Maximum HVAC Iterations"),
                Utility.IDFLineFormatter(minimumPlantIterations, "Minimum Plant Iterations"),
                Utility.IDFLastLineFormatter(maximumPlantIterations, "Maximum Plant Iterations")
            };
        }
    }
    [Serializable]
    public class SiteLocation
    {
        public string name { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double timeZone { get; set; }
        public double elevation { get; set; }

        public SiteLocation(string location)
        {
            switch (location)
            {
                case "MUNICH_DEU":
                    name = "MUNICH_DEU";
                    latitude = 48.13;
                    longitude = 11.7;
                    timeZone = 1.0;
                    elevation = 529.0;
                    break;
                default: 
                    name = "MUNICH_DEU";
                    latitude = 48.13;
                    longitude = 11.7;
                    timeZone = 1.0;
                    elevation = 529.0;
                    break;
            }
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "Site:Location,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(latitude, "Latitude {deg}"),
                Utility.IDFLineFormatter(longitude, "Longitude {deg}"),
                Utility.IDFLineFormatter(timeZone, "Time Zone {hr}"),
                Utility.IDFLastLineFormatter(elevation, "Elevation {m}")
            };
        }
    }
    [Serializable]
    public class SizingPeriodDesignDay
    {
        public string name { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public string dayType { get; set; }

        public double maxDryBulbT { get; set; }
        public double dailyDryBulbTR { get; set; }
        public string dryBulbTRModifierType { get; set; }

        public string humidityConditionType { get; set; }
        public double wetbulbOrDawPointAtMaxDryBulb { get; set; }

        public double enthalpyAtMaxDryBulb { get; set; }

        public double baromPress { get; set; }
        public double windspeed { get; set; }
        public double windDir { get; set; }

        public string rainInd { get; set; }
        public string snowInd { get; set; }
        public string daylightSavTimeInd { get; set; }
        public string solarModelInd { get; set; }

        public double skyClearness { get; set; }

        public SizingPeriodDesignDay(string name, int month, int day, string dayType, double maxDryBulbT, double dailyDryBulbTR, double wetbulbOrDawPointAtMaxDryBulb, double enthalpyAtMaxDryBulb, double baromPress,
            double windspeed, double windDir,string rainInd, string snowInd, string daylightSavTimeInd, string solarModelInd, double skyClearness)
        {
            this.name = name;
            this.month = month;
            this.day = day;
            this.dayType = dayType;
            this.maxDryBulbT = maxDryBulbT;
            this.dailyDryBulbTR = dailyDryBulbTR;
            dryBulbTRModifierType = "DefaultMultipliers";
            humidityConditionType = "DewPoint";
            this.wetbulbOrDawPointAtMaxDryBulb = wetbulbOrDawPointAtMaxDryBulb;
            this.enthalpyAtMaxDryBulb = enthalpyAtMaxDryBulb;
            this.baromPress = baromPress;
            this.windspeed = windspeed;
            this.windDir = windDir;
            this.rainInd = rainInd;
            this.snowInd = snowInd;
            this.daylightSavTimeInd = daylightSavTimeInd;
            this.solarModelInd = solarModelInd;
            this.skyClearness = skyClearness;
        }


        public List<String> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nSizingPeriod:DesignDay,");
            info.Add("\t" +name +                 ",\t\t\t\t!-Name");
            info.Add("\t" + month +  ", \t\t\t\t!-Month");
            info.Add("\t" + day +                 ",\t\t\t\t!-Day of Month");
            info.Add("\t" + dayType +  ", \t\t\t\t!-Day Type");
            info.Add("\t" + maxDryBulbT +                 ", \t\t\t\t!-Maximum Dry-Bulb Temperature {C}");
            info.Add("\t" + dailyDryBulbTR + ", \t\t\t\t!-Daily Dry-Bulb Temperature Range {deltaC}");
            info.Add("\t" + dryBulbTRModifierType + ",\t\t\t\t!-Dry-Bulb Temperature Range Modifier Type");
            info.Add( ",\t\t\t\t!-Dry-Bulb Temperature Range Modifier Day Schedule Name");
            info.Add("\t" + humidityConditionType + ",\t\t\t\t!-Humidity Condition Type");
            info.Add("\t" + wetbulbOrDawPointAtMaxDryBulb + ",\t\t\t\t!-Wetbulb or DewPoint at Maximum Dry-Bulb {C}");
            info.Add( ",\t\t\t\t!-Humidity Condition Day Schedule Name");
            info.Add( ",\t\t\t\t!-Humidity Ratio at Maximum Dry-Bulb {kgWater/kgDryAir}");
            info.Add(enthalpyAtMaxDryBulb + ",\t\t\t\t!-Enthalpy at Maximum Dry-Bulb {J/kg}");
            info.Add( ",\t\t\t\t!-Daily Wet-Bulb Temperature Range {deltaC}");
            info.Add("\t" + baromPress + ",\t\t\t\t!-Barometric Pressure {Pa}");
            info.Add("\t" + windspeed + ",\t\t\t\t!-Wind Speed {m/s}");
            info.Add("\t" + windDir + ",\t\t\t\t!-Wind Direction {deg}");
            info.Add("\t" + rainInd + ",\t\t\t\t!-Rain Indicator");
            info.Add("\t" + snowInd + ",\t\t\t\t!-Snow Indicator");
            info.Add("\t" + daylightSavTimeInd + ",\t\t\t\t!-Daylight Saving Time Indicator");
            info.Add("\t" + solarModelInd + ",\t\t\t\t!-Solar Model Indicator");
            info.Add( ",\t\t\t\t!-Beam Solar Day Schedule Name");
            info.Add(",\t\t\t\t!-Diffuse Solar Day Schedule Name");
            info.Add(",\t\t\t\t!-ASHRAE Clear Sky Optical Depth for Beam Irradiance (taub) {dimensionless}");
            info.Add( ",\t\t\t\t!-ASHRAE Clear Sky Optical Depth for Diffuse Irradiance (taud) {dimensionless}");
            info.Add("\t" + skyClearness + ";\t\t\t\t!-Sky Clearness");

            return info;
        }

    }
    [Serializable]
    public class RunPeriod
    {
        public string name { get; set; }
        public int beginMonth { get; set; }
        public int beginDayMonth { get; set; }
        public int endMonth { get; set; }
        public int endDayOfMonth { get; set; }
        public string dayOfWeekForStartDay { get; set; }
        public string useWeatherFileHolidaysAndSpecialDays { get; set; }
        public string useWeatherFileDaylightSavingPeriod { get; set; }
        public string WeekendHolidayRule { get; set; }
        public string useWeatherFileRainIndicators { get; set; }
        public string useWeatherFileSnowIndicators { get; set; }
        public int numberOfTimesRunperiodRepeated { get; set; }

        public RunPeriod()
        {
            name = "Run Period 1";
            beginMonth = 1;
            beginDayMonth = 1;
            endMonth = 12;
            endDayOfMonth = 31;
            dayOfWeekForStartDay = "";
            useWeatherFileHolidaysAndSpecialDays = "No";
            useWeatherFileDaylightSavingPeriod = "No";
            WeekendHolidayRule = "No";
            useWeatherFileRainIndicators = "Yes";
            useWeatherFileSnowIndicators = "Yes";
            numberOfTimesRunperiodRepeated = 1;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "RunPeriod,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(beginMonth, "Begin Month"),
                Utility.IDFLineFormatter(beginDayMonth, "Begin Day of Month"),
                Utility.IDFLineFormatter(endMonth, "End Month"),
                Utility.IDFLineFormatter(endDayOfMonth, "End Day of Month"),
                Utility.IDFLineFormatter(dayOfWeekForStartDay, "Day of Week for Start Day"),
                Utility.IDFLineFormatter(useWeatherFileHolidaysAndSpecialDays, "Use Weather File Holidays and Special Days"),
                Utility.IDFLineFormatter(useWeatherFileDaylightSavingPeriod, "Use Weather File Daylight Saving Period"),
                Utility.IDFLineFormatter(WeekendHolidayRule, "Apply Weekend Holiday Rule"),
                Utility.IDFLineFormatter(useWeatherFileRainIndicators, "Use Weather File Rain Indicators"),
                Utility.IDFLineFormatter(useWeatherFileSnowIndicators, "Use Weather File Snow Indicators"),
                Utility.IDFLastLineFormatter(numberOfTimesRunperiodRepeated, "Number of Times Runperiod to be Repeated")
            };
        }
    }
    [Serializable]
    public class SiteGroundTemperature
    {
        public double jan { get; set; }
        public double feb { get; set; }
        public double mar { get; set; }
        public double apr { get; set; }
        public double may { get; set; }
        public double jun { get; set; }
        public double jul { get; set; }
        public double aug { get; set; }
        public double sep { get; set; }
        public double oct { get; set; }
        public double nov { get; set; }
        public double dec { get; set; }

        public SiteGroundTemperature(string place)
        {
            switch (place)
            {
                case "MUNICH_DEU":
                    jan = 6.17; feb = 5.07; mar = 5.33; apr = 6.27; may = 9.35; jun = 12.12;
                    jul = 14.32; aug = 15.48; sep = 15.20; oct = 13.62; nov = 11.08; dec = 8.41;
                    break;
                default:
                    jan = 6.17; feb = 5.07; mar = 5.33; apr = 6.27; may = 9.35; jun = 12.12;
                    jul = 14.32; aug = 15.48; sep = 15.20; oct = 13.62; nov = 11.08; dec = 8.41;
                    break;
            }   
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>() { "Site:GroundTemperature:BuildingSurface," };
            info.AddRange(new List<string>() { string.Join(",", jan, feb, mar, apr, may, jun, jul, aug, sep, oct, nov, dec) + "; ! - Site Ground Temperatures" });
            return info;
        }
    }
    [Serializable]
    public class GlobalGeometryRules
    {
        public string startingVertexPosition { get; set; }
        public string vertexEntryDirection { get; set; }
        public string coordinateSystem { get; set; }
        public string daylightingRefPointCoordSyst { get; set; }
        public string rectSurfaceCoordSyst { get; set; }

        public GlobalGeometryRules()
        {
            startingVertexPosition = "UpperLeftCorner";
            vertexEntryDirection = "Counterclockwise";
            coordinateSystem = "Relative";
            daylightingRefPointCoordSyst = "Relative";
            rectSurfaceCoordSyst = "Relative";
        }

        internal List<string> WriteInfo()
        {
            return new List<string>()
            {
                "GlobalGeometryRules,",
                Utility.IDFLineFormatter(startingVertexPosition, "Starting Vertex Position"),
                Utility.IDFLineFormatter(vertexEntryDirection, "Vertex Entry Direction"),
                Utility.IDFLineFormatter(coordinateSystem, "Coordinate System"),
                Utility.IDFLineFormatter(daylightingRefPointCoordSyst, "Daylighting Reference Point Coordinate System"),
                Utility.IDFLastLineFormatter(rectSurfaceCoordSyst, "Rectangular Surface Coordinate System")
            };
        }
    }
    [Serializable]
    public class Output
    {
        public OutputVariableDictionary varDict;
        public Report report;
        public OutputTableSummaryReports tableSumReports;
        public OutputcontrolTableStyle tableStyle;
        public List<OutputVariable> vars;
        public OutputDiagnostics diagn;
        public List<OutputPreProcessorMessage> preprocessormess;

        public Output(Dictionary<string,string> variables)
        {
            varDict = new OutputVariableDictionary();
            report = new Report();
            tableSumReports = new OutputTableSummaryReports();
            tableStyle = new OutputcontrolTableStyle();
            diagn = new OutputDiagnostics();

            preprocessormess = new List<OutputPreProcessorMessage>();
            preprocessormess.Add(new OutputPreProcessorMessage(new List<String>(new string[] { "Cannot find Energy +.idd as specified in Energy +.ini." })));
            preprocessormess.Add(new OutputPreProcessorMessage(new List<String>(new string[] { "Since the Energy+.IDD file cannot be read no range or choice checking was","performed." })));

            vars = new List<OutputVariable>();
            foreach (string key in variables.Keys)
            { vars.Add(new OutputVariable(key, variables[key])); }
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:VARIABLEDICTIONARY ===========");
            info.AddRange(varDict.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:SURFACES:DRAWING ===========");

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: REPORT ===========");
            info.AddRange(report.writeInfo());
            
            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:TABLE:SUMMARYREPORTS ===========");
            info.AddRange(tableSumReports.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUTCONTROL:TABLE:STYLE ===========");
            info.AddRange(tableStyle.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:VARIABLE ===========");
            foreach(OutputVariable var in vars)
            { info.AddRange(var.writeInfo()); }

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:DIAGNOSTICS ===========");
            info.AddRange(diagn.writeInfo()); 

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:PREPROCESSORMESSAGE ===========");
            foreach (OutputPreProcessorMessage mes in preprocessormess)
            { info.AddRange(mes.writeInfo()); }

            return info;
        }
            
    }
    [Serializable]
    public class OutputVariableDictionary
    {
        public string keyField { get; set; }
        public OutputVariableDictionary()
        {
            keyField = "idf";
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:VariableDictionary,");
            info.Add("\t" + keyField + ";\t\t\t\t!-Key Field");

            return info;
        }
    }
    [Serializable]
    public class Report
    {
        public string reportType { get; set; }
        public Report()
        {
            reportType = "dxf";
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Surfaces:Drawing,");
            info.Add("\t" + reportType + ";\t\t\t\t!-Report Type");

            return info;
        }
    }
    [Serializable]
    public class OutputTableSummaryReports
    {
        public string report1;
        public string report2;
        public string report3;
        public string report4;
        public string report5;
        public string report6;
        public string report7;

        public OutputTableSummaryReports()
        {
            report1 = "ZoneComponentLoadSummary";
            report2 = "ComponentSizingSummary";
            report3 = "EquipmentSummary";
            report4 = "HVACSizingSummary";
            report5 = "ClimaticDataSummary";
            report6 = "OutdoorAirSummary";
            report7 = "EnvelopeSummary";
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Table:SummaryReports,");
            info.Add("\t" + report1 + ",\t\t\t\t!-Report 1 Name");
            info.Add("\t" + report2 + ",\t\t\t\t!-Report 2 Name");
            info.Add("\t" + report3 + ",\t\t\t\t!-Report 3 Name");
            info.Add("\t" + report4 + ",\t\t\t\t!-Report 4 Name");
            info.Add("\t" + report5 + ",\t\t\t\t!-Report 5 Name");
            info.Add("\t" + report6 + ",\t\t\t\t!-Report 6 Name");
            info.Add("\t" + report7 + ";\t\t\t\t!-Report 7 Name");

            return info;
        }


    }
    [Serializable]
    public class OutputcontrolTableStyle
    {
        public string columnSeparator { get; set; }
        public OutputcontrolTableStyle()
        {
            columnSeparator = "XMLandHTML";
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutputControl:Table:Style,");
            info.Add("\t" + columnSeparator + ";\t\t\t\t!-Column Separator");

            return info;
        }
    }
    [Serializable]
    public class OutputVariable
    {
        public string keyValue { get; set; }
        public string variableName { get; set; }
        public string reportingFrequency { get; set; }

        public OutputVariable(string varName, string reportfreq)
        {
            keyValue = "*";
            variableName = varName;
            reportingFrequency = reportfreq;
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Variable,");
            info.Add("\t" + keyValue + ",\t\t\t\t!-Key Value");
            info.Add("\t" + variableName + ",\t\t\t\t!-Variable Name");
            info.Add("\t" + reportingFrequency + ";\t\t\t\t!-Reporting Frequency");

            return info;
        }
    }
    [Serializable]
    public class OutputDiagnostics
    {
        public string Key1 { get; set; }
        public OutputDiagnostics()
        {
            Key1 = "DisplayAdvancedReportVariables";
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Diagnostics,");
            info.Add("\t" + Key1 + ";\t\t\t\t!-Key 1");

            return info;
        }
    }
    [Serializable]
    public class OutputPreProcessorMessage
    {
        public string preprocessorName;
        public string errorSeverity;
        public List<string> messageLines;

        public OutputPreProcessorMessage(List<string> messageLines)
        {
            preprocessorName = "ExpandObjects";
            errorSeverity = "Warning";
            this.messageLines = messageLines;
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:PreprocessorMessage,");
            info.Add("\t" + preprocessorName + ",\t\t\t\t!-Preprocessor Name");
            info.Add("\t" + errorSeverity + ",\t\t\t\t!-Error Severity");
            for(int i = 1; i<messageLines.Count; i++)
            {
                info.Add("\t" + messageLines[i - 1] + ",\t\t\t\t!-Message Line " + i);
            }
            info.Add("\t" + messageLines[messageLines.Count-1] + ";\t\t\t\t!-Message Line " + messageLines.Count);


            return info;
        }

    }
}


