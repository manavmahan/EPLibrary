using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class IDFFile
    {
        public string name = "IDFFile_0";

        //IDF Objects as appear in Energy Plus File
        public Version version = new Version();
        public SimulationControl sControl = new SimulationControl();
        public Timestep tStep = new Timestep(6);
        public ConvergenceLimits cLimits = new ConvergenceLimits();
        public SiteLocation sLocation = new SiteLocation(Location.MUNICH_DEU);
        public List<SizingPeriodDesignDay> SDesignDay;
        public RunPeriod rPeriod = new RunPeriod();
        public SiteGroundTemperature gTemperature = new SiteGroundTemperature();
        public GlobalGeometryRules geomRules = new GlobalGeometryRules();
        public Location Location;
        //Building - contain schedules, material, constructions, zones, zoneLists, 
        public Building building = new Building();
        public Output output;
        public IDFFile(Location location) 
        {
            Location = location;
            sLocation = new SiteLocation(location);
            SDesignDay = Utility.CreateDesignDays(location);
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
            info.AddRange(gTemperature.WriteInfo(Location));
            info.AddRange(geomRules.WriteInfo());

            building.schedulelimits.ForEach(s => info.AddRange(s.WriteInfo()));
            building.schedulescomp.ForEach(s => info.AddRange(s.WriteInfo()));

            info.AddRange(writeMaterial());
            info.AddRange(WriteWindowMaterial());
            info.AddRange(writeConstruction());
            info.AddRange(writeZone());
            info.AddRange(writeZoneList());
            info.AddRange(writeBuildingSurfaceList());
            building.zones.SelectMany(z=>z.iMasses).ToList().ForEach(m => info.AddRange(m.WriteInfo()));
            info.AddRange(writeFenestrationSurfaceList());
            info.AddRange(writeShading());

            info.AddRange(WriteDayLightControl());
            info.AddRange(writePeople());
            info.AddRange(writeLights());
            info.AddRange(writeElectricEquipment()); info.AddRange(writeZoneInfiltration()); info.AddRange(writeZoneVentilation());
            info.AddRange(writeHVACTemplate());

            info.AddRange(WritePVPanels());
            building.ZoneLists.Where(z => z.ZoneVentilationNatural != null).ToList().ForEach(z => info.AddRange(z.ZoneVentilationNatural.WriteInfo()));

            info.AddRange(output.writeInfo());
            return info;
        }

        private IEnumerable<string> WritePVPanels()
        {
            List<string> info = new List<string>();
            if (building.electricLoadCenterDistribution != null)
            {
                ElectricLoadCenterDistribution dist = building.electricLoadCenterDistribution;
                info.AddRange(dist.WriteInfo());
                info.AddRange(dist.GeneratorList.WriteInfo());
                dist.GeneratorList.Generator.ForEach(g => info.AddRange(g.WriteInfo()));
                dist.GeneratorList.Generator.Select(g => g.pperformance).Distinct().ToList().ForEach(p => info.AddRange(p.WriteInfo()));
            }
            return info;
        }
        private List<string> WriteDayLightControl()
        {
            List<string> info = new List<string>();
            building.zones.Where(z => z.DayLightControl != null).ToList().ForEach(z =>
            {
                z.DayLightControl.ReferencePoints.ForEach(p => info.AddRange(p.WriteInfo()));
                info.AddRange(z.DayLightControl.WriteInfo());
            });
            return info;
        }
        private List<string> writeMaterial()
        {
            List<string> info = new List<string>();
            info.Add("!-   =========== ALL OBJECTS IN CLASS: MATERIAL ===========");
            foreach (Material l in building.materials)
            {
                info.AddRange(l.WriteInfo());
            }
            return info;
        }
        private List<string> WriteWindowMaterial()
        {
            List<string> info = new List<string>();
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: WINDOWMATERIAL:SIMPLEGLAZINGSYSTEM ===========");
            building.windowMaterials.ForEach(wm => info.AddRange(wm.writeInfo()));
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: WINDOWMATERIAL:SHADING ===========");
            building.windowMaterialShades.ForEach(sh => info.AddRange(sh.writeInfo()));
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: SHADINGCONTROL ===========");
            List<Surface> bSurfaces = building.zones.SelectMany(z => z.Surfaces).ToList();
            bSurfaces.Where(s => s.Fenestrations != null && s.Fenestrations.Count > 0)
                .SelectMany(s => s.Fenestrations).Where(f => f.ShadingControl != null).Select(f => f.ShadingControl)
                .ToList().ForEach(shc => info.AddRange(shc.WriteInfo()));
            return info;
        }
        private List<string> writeConstruction()
        {
            List<string> info = new List<string>();
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: CONSTRUCTION ===========");
            building.constructions.ForEach(c => info.AddRange(c.WriteInfo()));
            return info;
        }
        private List<string> writeZone()
        {
            List<string> info = new List<string>();
            info.Add("\r!-   ===========  ALL OBJECTS IN CLASS: ZONE ===========");
            building.zones.ForEach(z => info.Add("Zone,\r\t" + z.Name + ";\t\t\t\t\t\t!-Name"));
            return info;
        }
        private List<string> writeZoneList()
        {
            List<string> idfString = new List<string>();

            idfString.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ZONELIST ===========\r\n");

            foreach (ZoneList zl in building.ZoneLists)
            {
                idfString.Add("ZoneList,");
                idfString.Add(Utility.IDFLineFormatter(zl.Name, "Name"));

                foreach (string z in zl.ZoneNames)
                {
                    idfString.Add(Utility.IDFLineFormatter(z, "Zone " + (zl.ZoneNames.IndexOf(z) + 1) + " Name"));
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
                foreach (Surface bSur in z.Surfaces)
                {
                    info.AddRange(bSur.SurfaceInfo());
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
                foreach (Surface bSur in z.Surfaces)
                {
                    if (bSur.Fenestrations != null)
                    {
                        foreach (Fenestration fen in bSur.Fenestrations)
                        {
                            info.AddRange(fen.WriteInfo());
                        }
                    }
                }
            }
            return info;
        }
        private List<string> writePeople()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: PEOPLE ===========\r\n");
            foreach (ZoneList zList in building.ZoneLists)
            {
                info.AddRange(zList.People.WriteInfo());
            }
            return info;
        }
        private List<string> writeZoneVentilation()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ZONEVENTILATION:DESIGNFLOWRATE ===========\r\n");
            foreach (ZoneList zList in building.ZoneLists)
            {
                info.AddRange(zList.ZoneVentilation.WriteInfo());
            }
            return info;
        }
        private List<string> writeLights()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: LIGHTS ===========\r\n");
            foreach (ZoneList zList in building.ZoneLists)
            {
                info.AddRange(zList.Light.WriteInfo());
            }
            return info;
        }
        private List<string> writeElectricEquipment()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ELECTRICEQUIPMENT ===========\r\n");
            foreach (ZoneList zList in building.ZoneLists)
            {
                info.AddRange(zList.ElectricEquipment.WriteInfo());
            }
            return info;
        }
        private List<string> writeHVACThermostat()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: HVACTEMPLATE:THERMOSTAT ===========\r\n");

            foreach (Thermostat t in building.ZoneLists.Select(z=>z.Thermostat))
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
            switch (building.Parameters.Service.HVACSystem)
            {
                case HVACSystem.BaseboardHeating:
                    building.zones.ForEach(z => info.AddRange((z.ZoneBBH).writeInfo()));
                    info.AddRange(building.hWaterLoop.writeInfo());
                    info.AddRange(building.boiler.writeInfo());
                    break;
                case HVACSystem.FCU:
                    building.zones.ForEach(z => info.AddRange((z.ZoneFCU).writeInfo()));
                    info.AddRange(building.cWaterLoop.writeInfo());
                    info.AddRange(building.chiller.writeInfo());
                    info.AddRange(building.tower.writeInfo());
                    info.AddRange(building.hWaterLoop.writeInfo());
                    info.AddRange(building.boiler.writeInfo());
                    break;
                case HVACSystem.HeatPumpWBoiler:
                    building.zones.ForEach(z => info.AddRange((z.ZoneHP).WriteInfo()));
                    info.AddRange(building.mWaterLoop.WriteInfo());
                    info.AddRange(building.tower.writeInfo());
                    info.AddRange(building.boiler.writeInfo());
                    break;
                case HVACSystem.VAV:
                    building.zones.ForEach(z => info.AddRange((z.ZoneVAV).writeInfo())); info.AddRange(building.vav.writeInfo());           
                    info.AddRange(building.cWaterLoop.writeInfo());
                    info.AddRange(building.chiller.writeInfo());
                    info.AddRange(building.tower.writeInfo());
                    info.AddRange(building.hWaterLoop.writeInfo());
                    info.AddRange(building.boiler.writeInfo());
                    break;
                case HVACSystem.IdealLoad:
                    building.zones.ForEach(z => info.AddRange((z.ZoneIL).writeInfo()));
                    break;
            }
            return info;
        }
        public List<string> writeZoneInfiltration()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ZONEINFILTRATION:DESIGNFLOWRATE ===========\r\n");

            foreach (ZoneList z in building.ZoneLists)
            {
                info.AddRange(z.ZoneInfiltration.WriteInfo());
            }
            return info;
        }
        public List<string> writeShading()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: SHADING:ZONE:DETAILED ===========\r\n");

            try
            {
                info.AddRange((building.zones.SelectMany(z => z.Surfaces.SelectMany(s => s.Shading))).SelectMany(s => s.shadingInfo()));
            }
            catch { }

            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: SHADING:OVERHANG:PROJECTION ===========\r\n");

            try
            {
                info.AddRange((building.zones.SelectMany(z => z.Surfaces.SelectMany(s => s.Fenestrations.Select(f => f.Overhang)))).SelectMany(s => s.OverhangInfo()));
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
        public void GenerateOutput(string frequency)
        {
            Dictionary<string, string> outputvars = new Dictionary<string, string>();
            outputvars.Add("Zone Air System Sensible Heating Rate", frequency);
            outputvars.Add("Zone Air System Sensible Cooling Rate", frequency);
            outputvars.Add("Zone Lights Electric Power", frequency);

            outputvars.Add("Zone Water to Air Heat Pump Electric Energy", frequency);
            outputvars.Add("Boiler Electric Energy", frequency);
            outputvars.Add("Boiler Gas Energy", frequency);          
            outputvars.Add("Chiller Electric Energy", frequency);
            outputvars.Add("Cooling Tower Fan Electric Energy", frequency);

            outputvars.Add("Facility Total Purchased Electric Energy", frequency);

            outputvars.Add("Zone Infiltration Total Heat Loss Energy", frequency);
            outputvars.Add("Zone Infiltration Total Heat Gain Energy", frequency);
            outputvars.Add("Surface Window Net Heat Transfer Rate", frequency);
            outputvars.Add("Surface Inside Face Conduction Heat Transfer Rate", frequency);
            outputvars.Add("Surface Outside Face Incident Solar Radiation Rate per Area", frequency);
            
            output = new Output(outputvars);
        }
    }
}
