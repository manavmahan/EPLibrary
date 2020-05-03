using DocumentFormat.OpenXml.Drawing.Diagrams;
using IDFObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDFObjects
{
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

        //Initialise blank building
        //Create zone with wall, floor, roof including orientation
        //run UpdateBuildingConstructionWWROperations
               
        
        //Probablistic Attributes
        public ProbabilisticBuildingDesignParameters p_Parameters;
        public ProbabilisticEmbeddedEnergyParameters p_EEParameters;

        //Probabilistic Operational Energy
        public ProbablisticBEnergyPerformance ProbablisticBEnergyPerformance;

        //EnergyPlus Output
        public BEnergyPerformance BEnergyPerformance;

        //Probabilistic Embedded Energy Output
        public double[] p_EmbeddedEnergy, p_PERT_EmbeddedEnergy, p_PENRT_EmbeddedEnergy,
            p_LCE_PERT, p_LCE_PENRT, p_LifeCycleEnergy;
        public double EmbeddedEnergy, PERT_EmbeddedEnergy, PENRT_EmbeddedEnergy,
            LCE_PERT, LCE_PENRT, LifeCycleEnergy;
        public double life, PERTFactor, PENRTFactor;

        //Deterministic Attributes
        public BuildingDesignParameters Parameters;
        public EmbeddedEnergyParameters EEParameters;
        public ShadingLength shadingLength { get; set; } = new ShadingLength(0, 0, 0, 0); //North, West, South, East

        //Schedules Limits and Schedule
        public List<ScheduleLimits> schedulelimits  = new List<ScheduleLimits>();
        public List<ScheduleCompact> schedulescomp  = new List<ScheduleCompact>();

        //Material, WindowMaterial, Shade, Shading Control, Constructions and Window Constructions
        public List<Material> materials = new List<Material>();
        public List<WindowMaterial> windowMaterials = new List<WindowMaterial>();
        public List<WindowMaterialShade> windowMaterialShades = new List<WindowMaterialShade>();
        public List<Construction> constructions = new List<Construction>();

        //Zone, ZoneList, BuidlingSurface, ShadingOverhangs
        public List<Zone> zones = new List<Zone>();
        public List<ZoneList> ZoneLists = new List<ZoneList>();
        public List<ShadingBuildingDetailed> DetachedShading = new List<ShadingBuildingDetailed>();

        //to generate building in Revit
        public XYZList GroundPoints;
        public int nFloors;

        //HVAC Template - should be extracted from zone
        //public List<Thermostat> tStats = new List<Thermostat>();
        public VAV vav;
        public ChilledWaterLoop cWaterLoop;
        public Chiller chiller;
        public Tower tower;

        public void CreateZoneLists()
        {
            IEnumerable<string> zoneListNames = Parameters.Operations.Select(o => o.Name);
            foreach(string zoneListName in zoneListNames)
            {
                BuildingZoneOperation op = Parameters.Operations.First(o => o.Name == zoneListName);
                BuildingZoneOccupant oc = Parameters.Occupants.First(o => o.Name == zoneListName);
                BuildingZoneEnvironment ev = Parameters.Environments.First(o => o.Name == zoneListName);

                AddZoneList(new ZoneList()
                {
                    Name = zoneListName,
                    Operation = op,
                    Occupant = oc,
                    Environment = ev
                }) ;
            }
        }

        public HotWaterLoop hWaterLoop;
        public Boiler boiler;

        //PV Panel on Roof
        public ElectricLoadCenterDistribution electricLoadCenterDistribution;

        public void UpdateBuildingConstructionWWROperations()
        {
            GenerateConstructionWithIComponentsU();
            CreateInternalMass();
            UpdateFenestrations();
            CreateSchedules();
            if (ZoneLists.Any(zl=>zl.Schedules==null)) { GeneratePeopleLightEquipmentInfiltrationVentilation(); }
            ZoneLists.ForEach(zl=>zl.UpdateDayLightControlSchedule(this));
            GenerateHVAC();
            UpdateZoneInfo();
        } 
        void UpdateFenestrations()
        {            
            foreach (Zone zone in zones)
            {
                foreach (Surface toupdate in zone.Surfaces.Where(s => s.surfaceType == SurfaceType.Wall && s.OutsideCondition == "Outdoors"))
                {
                    toupdate.CreateWindowsShadingControlShadingOverhang(zone, Parameters.WWR, shadingLength);
                }
                if (!zone.Surfaces.Any(s => s.Fenestrations!=null)) 
                {
                    zone.DayLightControl = null; 
                }
            }
        }
        
        public void UpdateZoneInfo()
        {
            zones.ForEach(z => z.CalcAreaVolumeHeatCapacity(this));
        }
        public void CreateInternalMass(double percentArea, bool IsWall)
        {
            zones.ForEach(z =>
            {
                z.CalcAreaVolume();
                InternalMass mass = new InternalMass(z, percentArea * z.Area * z.Height, "InternalWall", IsWall);
            });

        }
        public void CreateInternalMass()
        {
            double hcIWall = Parameters.Construction.hcIWall;
            if (Parameters.Construction.InternalMass > 0) { 
                zones.ForEach(z =>
                {
                    z.CalcAreaVolume();
                    InternalMass mass = new InternalMass(z, 1000 * Parameters.Construction.InternalMass * 
                        z.Area / hcIWall, "InternalWall", false);
                });
            }
        }
        public void CreatePVPanelsOnRoof()
        {
            PhotovoltaicPerformanceSimple photovoltaicPerformanceSimple = new PhotovoltaicPerformanceSimple();
            List<GeneratorPhotovoltaic> listPVs = new List<GeneratorPhotovoltaic>();
            List<Surface> bSurfaces = zones.SelectMany(z => z.Surfaces).ToList();
            List<Surface> roofs = bSurfaces.FindAll(s => s.surfaceType == SurfaceType.Roof);

            roofs.ForEach(s => listPVs.Add(new GeneratorPhotovoltaic(s, photovoltaicPerformanceSimple, "AlwaysOn")));
            ElectricLoadCenterGenerators electricLoadCenterGenerators = new ElectricLoadCenterGenerators(listPVs);
            electricLoadCenterDistribution = new ElectricLoadCenterDistribution(electricLoadCenterGenerators);
        }
        public void CreateNaturalVentilation()
        {
            zones.ForEach(z => z.CreateNaturalVentillation());
        }
        public void CreateSchedules()
        {          
            schedulelimits = new List<ScheduleLimits>();
            //schedulescomp = new List<ScheduleCompact>();

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
            
            ScheduleCompact nocooling = new ScheduleCompact()
            {
                name = "No Cooling",
                scheduleLimitName = temp.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                { "AllDays", new Dictionary<string, double>() {{"24:00", 35} } } }
            };

            ScheduleCompact activity = new ScheduleCompact()
            {
                name = "People Activity Schedule",
                scheduleLimitName = activityLevel.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                { "AllDays", new Dictionary<string, double>() {{"24:00", 125} } } }
            };

            ScheduleCompact workEff = new ScheduleCompact()
            {
                name = "Work Eff Sch",
                scheduleLimitName = fractional.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                { "AllDays", new Dictionary<string, double>() {{"24:00", 1} } } }
            };

            ScheduleCompact airVelo = new ScheduleCompact()
            {
                name = "Air Velo Sch",
                scheduleLimitName = fractional.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                { "AllDays", new Dictionary<string, double>() {{"24:00", .1} } } }
            };

            //infiltration
            ScheduleCompact infiltration = new ScheduleCompact()
            {
                name = "Space Infiltration Schedule",
                scheduleLimitName = fractional.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                { "AllDays", new Dictionary<string, double>() {{"24:00", 1} } } }
            };
            ScheduleCompact alwaysOn = new ScheduleCompact()
            {
                name = "AlwaysOn",
                scheduleLimitName = fractional.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                { "AllDays", new Dictionary<string, double>() {{"24:00", 1} } } }
            };
            schedulescomp.Add(alwaysOn);
            schedulescomp.Add(workEff);
            schedulescomp.Add(airVelo);
            schedulescomp.Add(infiltration);
        }
        void GenerateConstructionWithIComponentsU()
        {
            double uWall = Parameters.Construction.UWall, uGFloor = Parameters.Construction.UGFloor, 
                uIFloor = Parameters.Construction.UIFloor,
                uRoof = Parameters.Construction.URoof, uIWall = Parameters.Construction.UIWall, uWindow = Parameters.Construction.UWindow,
                gWindow = Parameters.Construction.GWindow, hcSlab = Parameters.Construction.HCSlab;

            double lambda_insulation = 0.04;

            double th_insul_wall = lambda_insulation * (1 / uWall - (0.2 / 0.5 + 0.015 / 0.5));
            double th_insul_gFloor = lambda_insulation * (1 / uGFloor - (0.1 / 1.95));
            double th_insul_iFloor = lambda_insulation * (1 / uIFloor - (0.1 / 2));
            double th_insul_Roof = lambda_insulation * (1 / uRoof - (0.175 / 0.75 + 0.025 / 0.75 + 0.15 / 0.7));

            double th_insul_IWall = lambda_insulation * (1 / uIWall - (0.05 / 0.16 + 0.05 / 0.16));

            if (th_insul_wall <= 0 || th_insul_gFloor <= 0 || th_insul_iFloor <= 0 || th_insul_Roof <= 0 || th_insul_IWall <= 0)
            {
                Console.WriteLine("Check U Values {0}, {1}, {2}, {3}, {4}", th_insul_wall, th_insul_gFloor, th_insul_iFloor, th_insul_Roof, th_insul_IWall);
                Console.ReadKey();
            }

            //roof layers
            Material layer_F13 = new Material("F13", "Smooth", 0.175, 0.75, 1120, 1465, 0.9, 0.4, 0.7);
            Material layer_G03 = new Material("G03", "Smooth", 0.025, 0.75, 400, 1300, 0.9, 0.7, 0.7);
            Material layer_I03 = new Material("I03", "Smooth", th_insul_Roof, lambda_insulation, 45, 1200, 0.9, 0.7, 0.7);
            Material layer_M11 = new Material("M11", "Smooth", 0.15, 0.7, 1200, hcSlab, 0.9, 0.7, 0.7);

            //wall layers
            Material layer_M03 = new Material("M03", "Smooth", 0.2, 0.5, 500, 900, 0.9, 0.4, 0.7);
            Material layer_I04 = new Material("I04", "Smooth", th_insul_wall, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);
            Material layer_G01 = new Material("G01", "Smooth", 0.015, 0.5, 800, 1100, 0.9, 0.7, 0.7);

            //gFloor & iFloor layers
            Material layer_floorSlab = new Material("Concrete_Floor_Slab", "Smooth", 0.10, 1.95, 2250, hcSlab, 0.9, 0.7, 0.7);
            Material layer_gFloorInsul = new Material("gFloor_Insulation", "Smooth", th_insul_gFloor, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);
            Material layer_iFloorInsul = new Material("iFloor_Insulation", "Smooth", th_insul_iFloor, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);

            //internal wall layers
            Material layer_Plasterboard = new Material("Plasterboard", "Rough", 0.05, 0.16, 800, 800, 0.9, 0.6, 0.6);
            Material layer_iWallInsul = new Material("iFloor_Insulation", "Rough", th_insul_IWall, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);

            //airgap
            //Layer layer_airgap = new Layer("Material Air Gap 1", "", 0.1, 0.1, 0.1, 0.1, 9, 7, 7); //airgap layer

            materials = new List<Material>() { layer_F13, layer_G03, layer_I03, layer_M11, layer_M03, layer_I04, layer_G01, layer_floorSlab, layer_gFloorInsul, layer_iFloorInsul, layer_Plasterboard };

            List<Material> layerListRoof = new List<Material>() { layer_F13, layer_G03, layer_I03, layer_M11 };
            Parameters.Construction.hcRoof = layerListRoof.Select(l => l.thickness * l.sHC * l.density).Sum();
            Construction constructionRoof = new Construction("Up Roof Concrete", layerListRoof);

            List<Material> layerListWall = new List<Material>() { layer_M03, layer_I04, layer_G01 };

            Construction construction_Wall = new Construction("Wall ConcreteBlock", layerListWall);
            Parameters.Construction.hcWall = layerListWall.Select(l => l.thickness * l.sHC * l.density).Sum();
            List<Material> layerListInternallWall = new List<Material>() { layer_Plasterboard, layer_iWallInsul, layer_Plasterboard };
            Construction construction_internalWall = new Construction("InternalWall", layerListInternallWall);
            Parameters.Construction.hcIWall = layerListInternallWall.Select(l => l.thickness * l.sHC * l.density).Sum();
            List<Material> layerListGfloor = new List<Material>() { layer_floorSlab, layer_gFloorInsul };
            Construction construction_gFloor = new Construction("Slab_Floor", layerListGfloor);
            Parameters.Construction.hcGFloor = layerListGfloor.Select(l => l.thickness * l.sHC * l.density).Sum();
            List<Material> layerListIFloor = new List<Material>() { layer_floorSlab, layer_iFloorInsul };
            Construction construction_floor = new Construction("General_Floor_Ceiling", layerListIFloor);
            Parameters.Construction.hcIFloor = layerListIFloor.Select(l => l.thickness * l.sHC * l.density).Sum();
            constructions = new List<Construction>() { constructionRoof, construction_Wall, construction_gFloor, construction_floor, construction_internalWall };

            //window construction
            WindowMaterial windowLayer = new WindowMaterial("Glazing Material", uWindow, gWindow, 0.1);
            windowMaterials = new List<WindowMaterial>() { windowLayer };
            Construction window = new Construction("Glazing", new List<WindowMaterial>() { windowLayer });
            constructions.Add(window);

            //window shades
            windowMaterialShades = new List<WindowMaterialShade>() { (new WindowMaterialShade()) };
        }
        void GeneratePeopleLightEquipmentInfiltrationVentilation()
        {
            foreach (ZoneList zList in ZoneLists)
            {
                BuildingZoneOperation operation = Parameters.Operations.First(o => o.Name == zList.Name);
                BuildingZoneOccupant occupant = Parameters.Occupants.First(o => o.Name == zList.Name);
                zList.GeneratePeopleLightEquipmentVentilationInfiltrationThermostat(this, 
                    operation.GetStartEndTime(13),
                    occupant.AreaPerPerson, operation.LHG, operation.EHG,
                    Parameters.Construction.Infiltration);

                schedulescomp.AddRange(zList.Schedules);
            }
            
        }
        public void GenerateHVAC()
        {
            zones.ForEach(z => z.ThermostatName = ZoneLists.First(zl => zl.ZoneNames.Contains(z.Name)).Thermostat.name);
            zones.ForEach(z => z.OccupancyScheduleName = ZoneLists.First(zl => zl.ZoneNames.Contains(z.Name)).
            Schedules.First(s=>s.name.Contains("Occupancy")).name);
            switch (Parameters.Service.HVACSystem)
            {
                case HVACSystem.FCU:
                    zones.ForEach(z => { ZoneFanCoilUnit zFCU = new ZoneFanCoilUnit(z, z.ThermostatName); });
                    GenerateWaterLoopsAndSystem();
                    break;

                case HVACSystem.BaseboardHeating:
                    zones.ForEach(z => { ZoneBaseBoardHeat zBBH = new ZoneBaseBoardHeat(z, z.ThermostatName); });
                    GenerateWaterLoopsAndSystem();
                    break;

                case HVACSystem.VAV:
                    vav = new VAV();
                    zones.ForEach(z => { ZoneVAV zVAV = new ZoneVAV(vav, z, z.ThermostatName); });
                    GenerateWaterLoopsAndSystem();
                    break;

                case HVACSystem.IdealLoad:
                default:
                    zones.ForEach(z => { ZoneIdealLoad zIdeal = new ZoneIdealLoad(z, z.ThermostatName); });
                    break;
            }
        }
        private void GenerateWaterLoopsAndSystem() 
        {
            hWaterLoop = new HotWaterLoop();
            cWaterLoop = new ChilledWaterLoop();
            boiler = new Boiler(Parameters.Service.BoilerEfficiency, "Electricity");
            chiller = new Chiller(Parameters.Service.ChillerCOP);
            tower = new Tower();
        }
        public Building() { }
        public void AssociateEnergyPlusResults(Dictionary<string, double[]> data)
        {
            List<Surface> bSurfaces = zones.SelectMany(z => z.Surfaces).ToList();
            foreach (Surface surf in bSurfaces)
            {
                if (surf.OutsideCondition == "Outdoors")
                {
                    if (surf.Fenestrations != null && surf.Fenestrations.Count != 0)
                    {
                        Fenestration win = surf.Fenestrations[0];
                        win.SolarRadiation = data[data.Keys.First(a => a.Contains(win.Name.ToUpper()) && a.Contains("Surface Outside Face Incident Solar Radiation Rate per Area"))].Average();
                        win.HeatFlow = data[data.Keys.First(s => s.Contains(win.Name.ToUpper()) && s.Contains("Surface Window Net Heat Transfer Energy"))].ConvertKWhfromJoule().Average();
                    }
                    surf.SolarRadiation = data[data.Keys.First(s => s.Contains(surf.Name.ToUpper()) && s.Contains("Surface Outside Face Incident Solar Radiation Rate per Area") && !s.Contains("WINDOW"))].Average();
                }
                surf.HeatFlow = data[data.Keys.First(s => s.Contains(surf.Name.ToUpper()) && s.Contains("Surface Inside Face Conduction Heat Transfer Energy"))].ConvertKWhfromJoule().Average();
            }
            foreach (Zone zone in zones)
            {
                zone.CalcAreaVolumeHeatCapacity(this); zone.AssociateEnergyPlusResults(this, data);
            }
            BEnergyPerformance = new BEnergyPerformance()
            {
                TotalArea = zones.Select(z => z.Area).Sum(),
                TotalVolume = zones.Select(z => z.Volume).Sum(),
                ZoneHeatingEnergy = zones.Select(z => z.HeatingEnergy).Sum(),
                ZoneCoolingEnergy = zones.Select(z => z.CoolingEnergy).Sum(),
                LightingEnergy = zones.Select(z => z.LightingEnergy).Sum(),

                BoilerEnergy = data[data.Keys.First(a => a.Contains("Boiler Electric Energy"))].ConvertKWhfromJoule().Sum(),
                ChillerEnergy = data[data.Keys.First(a => a.Contains("Chiller Electric Energy"))].ConvertKWhfromJoule().Sum() +
                    data[data.Keys.First(a => a.Contains("Cooling Tower Fan Electric Energy"))].ConvertKWhfromJoule().Sum(),
                
            };
            BEnergyPerformance.ThermalEnergy = BEnergyPerformance.ChillerEnergy + BEnergyPerformance.BoilerEnergy;
            BEnergyPerformance.OperationalEnergy = BEnergyPerformance.ThermalEnergy + BEnergyPerformance.LightingEnergy;
        }
        public void AssociateProbabilisticEnergyPlusResults(Dictionary<string, double[]> resultsDF)
        {
            List<Surface> bSurfaces = zones.SelectMany(z => z.Surfaces).ToList();
            foreach (Surface surf in bSurfaces)
            {
                if (surf.surfaceType == SurfaceType.Wall || surf.surfaceType == SurfaceType.Roof)
                {
                    if (surf.OutsideCondition == "Outdoors" && surf.Fenestrations != null && surf.Fenestrations.Count != 0)
                    {
                        Fenestration win = surf.Fenestrations[0];
                        win.p_SolarRadiation = resultsDF[resultsDF.Keys.First(a => a.Contains(win.Name.ToUpper()) && a.Contains("Surface Outside Face Incident Solar Radiation Rate per Area"))];
                        win.p_HeatFlow = resultsDF[resultsDF.Keys.First(s => s.Contains(win.Name.ToUpper()) && s.Contains("Surface Window Net Heat Transfer Energy"))].ConvertKWhfromJoule();
                        win.SolarRadiation = win.p_SolarRadiation.Average();
                        win.HeatFlow = win.p_HeatFlow.Average();
                        surf.p_SolarRadiation = resultsDF[resultsDF.Keys.First(s => s.Contains(surf.Name.ToUpper()) && s.Contains("Surface Outside Face Incident Solar Radiation Rate per Area") && !s.Contains("WINDOW"))];
                        surf.SolarRadiation = surf.p_SolarRadiation.Average();
                    }
                }
                surf.p_HeatFlow = resultsDF[resultsDF.Keys.First(s => s.Contains(surf.Name.ToUpper()) && s.Contains("Surface Inside Face Conduction Heat Transfer Energy"))].ConvertKWhfromJoule();
                surf.HeatFlow = surf.p_HeatFlow.Average();
            }
            foreach (Zone zone in zones)
            {
                zone.CalcAreaVolumeHeatCapacity(this); zone.AssociateProbabilisticEnergyPlusResults(this, resultsDF);
            }
            ProbablisticBEnergyPerformance = new ProbablisticBEnergyPerformance()
            {
                TotalArea = zones.Select(z => z.Area).Sum(),
                TotalVolume = zones.Select(z => z.Volume).Sum(),
                ZoneHeatingEnergy = zones.Select(z => z.p_HeatingEnergy).ToList().AddArrayElementWise(),
                ZoneCoolingEnergy = zones.Select(z => z.p_CoolingEnergy).ToList().AddArrayElementWise(),
                LightingEnergy = zones.Select(z => z.p_LightingEnergy).ToList().AddArrayElementWise(),
                BoilerEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains("Boiler Electric Energy"))].ConvertKWhfromJoule(),
                ChillerEnergy = new List<double[]>() { resultsDF[resultsDF.Keys.First(a => a.Contains("Chiller Electric Energy"))],
                resultsDF[resultsDF.Keys.First(a => a.Contains("Cooling Tower Fan Electric Energy"))] }.AddArrayElementWise().ConvertKWhfromJoule()             
            };
            ProbablisticBEnergyPerformance.ThermalEnergy = new List<double[]>() { 
                ProbablisticBEnergyPerformance.ChillerEnergy,
                ProbablisticBEnergyPerformance.BoilerEnergy }.AddArrayElementWise();

            ProbablisticBEnergyPerformance.OperationalEnergy = new List<double[]>() { ProbablisticBEnergyPerformance.ThermalEnergy,
                ProbablisticBEnergyPerformance.LightingEnergy}.AddArrayElementWise();
        }
        public void AssociateProbabilisticMLResults(Dictionary<string, double[]> resultsDF)
        {
            List<Surface> bSurfaces = zones.SelectMany(z => z.Surfaces).ToList();
            foreach (Surface surf in bSurfaces)
            {
                if (surf.surfaceType == SurfaceType.Wall && surf.OutsideCondition == "Outdoors" && surf.Fenestrations != null && surf.Fenestrations.Count != 0)
                {
                    Fenestration win = surf.Fenestrations[0];
                    win.p_HeatFlow = resultsDF[resultsDF.Keys.First(s => s.Contains(win.Name))];
                    win.HeatFlow = win.p_HeatFlow.Average();
                }
                if (!surf.OutsideObject.Contains("Zone"))
                {
                    surf.p_HeatFlow = resultsDF[resultsDF.Keys.First(s => s.Contains(surf.Name) && !s.Contains("Window"))];
                    surf.HeatFlow = surf.p_HeatFlow.Average();
                }
            }
            foreach (Zone zone in zones)
            {
                zone.CalcAreaVolumeHeatCapacity(this); zone.AssociateProbabilisticMLResults(resultsDF);
            }
            ProbablisticBEnergyPerformance = new ProbablisticBEnergyPerformance()
            {
                TotalArea = zones.Select(z => z.Area).Sum(),
                TotalVolume = zones.Select(z => z.Volume).Sum(),
                ZoneHeatingEnergy = zones.Select(z => z.p_HeatingEnergy).ToList().AddArrayElementWise(),
                ZoneCoolingEnergy = zones.Select(z => z.p_CoolingEnergy).ToList().AddArrayElementWise(),
                LightingEnergy = zones.Select(z => z.p_LightingEnergy).ToList().AddArrayElementWise(),
                OperationalEnergy = resultsDF[resultsDF.Keys.First(s => s.Contains("Operational Energy"))]
            };

            try
            {
                ProbablisticBEnergyPerformance.ThermalEnergy = resultsDF[resultsDF.Keys.First(s => s.Contains("Thermal Energy"))];
            }
            catch
            {

            }
        }
        public void AssociateProbabilisticEmbeddedEnergyResults(Dictionary<string, double[]> resultsDF)
        {
            p_PERT_EmbeddedEnergy = resultsDF["PERT"];
            p_PENRT_EmbeddedEnergy = resultsDF["PENRT"];
            p_EmbeddedEnergy = new List<double[]>() { p_PENRT_EmbeddedEnergy, p_PENRT_EmbeddedEnergy }.AddArrayElementWise();

            p_LCE_PENRT = new List<double[]>() { p_PENRT_EmbeddedEnergy, ProbablisticBEnergyPerformance.OperationalEnergy.Select((double x) => x * life * PENRTFactor).ToArray() }.AddArrayElementWise();
            p_LCE_PERT = new List<double[]>() { p_PERT_EmbeddedEnergy, ProbablisticBEnergyPerformance.OperationalEnergy.Select(x => x * life * PERTFactor).ToArray() }.AddArrayElementWise();
            p_LifeCycleEnergy = new List<double[]>() { p_LCE_PENRT, p_LCE_PERT }.AddArrayElementWise();

            PERT_EmbeddedEnergy = p_PERT_EmbeddedEnergy.Average();
            PENRT_EmbeddedEnergy = p_PENRT_EmbeddedEnergy.Average();
            EmbeddedEnergy = p_EmbeddedEnergy.Average();
            LCE_PENRT = p_LCE_PENRT.Average();
            LCE_PERT = p_LCE_PERT.Average();
            LifeCycleEnergy = p_LifeCycleEnergy.Average();
        }
        public void AssociateEmbeddedEnergyResults(Dictionary<string, double> resultsDF)
        {
            PERT_EmbeddedEnergy = resultsDF["PERT"];
            PENRT_EmbeddedEnergy = resultsDF["PENRT"];
            EmbeddedEnergy = PENRT_EmbeddedEnergy + PENRT_EmbeddedEnergy;

            LCE_PENRT = BEnergyPerformance.OperationalEnergy * life * PENRTFactor;
            LCE_PERT = BEnergyPerformance.OperationalEnergy * life * PERTFactor;
            LifeCycleEnergy = LCE_PENRT + LCE_PERT;
        }
        public Building AddZone(Zone zone)
        {
            zones.Add(zone);
            return this;
        }
        public void AddZone(List<Zone> zones) { zones.ForEach(z => AddZone(z)); }
        public void AddZoneList(ZoneList zoneList)
        {
            ZoneLists.Add(zoneList);
            try { schedulescomp.AddRange(zoneList.Schedules); } catch { }
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
       
        public void InitialiseBuilding_SameFloorPlan(List<ZoneGeometryInformation> zonesInformation,
            BuildingDesignParameters parameters)
        {
            Parameters = parameters;
            CreateZoneLists();

            for (int i = 0; i < parameters.Geometry.NFloors; i++)
            {
                foreach (ZoneGeometryInformation zoneInfo in zonesInformation)
                {
                    Zone zone = new Zone(zoneInfo.Height, zoneInfo.Name + ":" + i, i);
                    XYZList floorPoints = zoneInfo.FloorPoints.ChangeZValue(i * zoneInfo.Height);                 
                    if (i == 0)
                    {
                        new Surface(zone, floorPoints.Reverse(), floorPoints.CalculateArea(), SurfaceType.Floor);
                    }
                    else
                    {
                        new Surface(zone, floorPoints.Reverse(), floorPoints.CalculateArea(), SurfaceType.Floor)
                        {
                            ConstructionName = "General_Floor_Ceiling",
                            OutsideCondition = "Zone",
                            OutsideObject = zoneInfo.Name + ":" + (i - 1)
                        };
                    }
                    Utility.CreateZoneWalls(zone, zoneInfo.WallCreationData, floorPoints.xyzs.First().Z);
                    if (i == parameters.Geometry.NFloors - 1)
                    {
                        XYZList rfPoints = floorPoints.OffsetHeight(zoneInfo.Height);
                        new Surface(zone, rfPoints, rfPoints.CalculateArea(), SurfaceType.Roof);
                    }
                    zone.CreateDaylighting(500);
                    AddZone(zone);
                    try { ZoneLists.First(zList => zList.Name == zone.Name.Split(':').First()).ZoneNames.Add(zone.Name); }
                    catch { ZoneLists.FirstOrDefault().ZoneNames.Add(zone.Name); }
                }
            }
            UpdateBuildingConstructionWWROperations();
        }

        public void InitialiseBuilding(List<ZoneGeometryInformation> zonesInformation,
           BuildingDesignParameters parameters)
        {
            Parameters = parameters;
            CreateZoneLists();

            foreach (ZoneGeometryInformation zoneInfo in zonesInformation)
            {
                Zone zone = new Zone(zoneInfo.Height, zoneInfo.Name + ":" + zoneInfo.Level, zoneInfo.Level);
                XYZList floorPoints = zoneInfo.FloorPoints;
                if (zoneInfo.Level == 0)
                    new Surface(zone, floorPoints.Reverse(), floorPoints.CalculateArea(), SurfaceType.Floor);
                else
                    new Surface(zone, floorPoints.Reverse(), floorPoints.CalculateArea(), SurfaceType.Floor)
                    {
                        ConstructionName = "General_Floor_Ceiling",
                        OutsideCondition = "Adiabatic"
                    };
                
                Utility.CreateZoneWalls(zone, zoneInfo.WallCreationData, floorPoints.xyzs.First().Z);
                if (zoneInfo.Level == parameters.Geometry.NFloors - 1)
                    new Surface(zone, floorPoints.OffsetHeight(zoneInfo.Height), floorPoints.CalculateArea(), SurfaceType.Roof);
                else
                    new Surface(zone, floorPoints.OffsetHeight(zoneInfo.Height), floorPoints.CalculateArea(), SurfaceType.Floor)
                    {
                        ConstructionName = "General_Floor_Ceiling",
                        OutsideCondition = "Adiabatic"
                    };
                zone.CreateDaylighting(500);
                AddZone(zone);
                try { ZoneLists.First(zList => zList.Name == zone.Name.Split(':').First()).ZoneNames.Add(zone.Name); }
                catch { ZoneLists.FirstOrDefault().ZoneNames.Add(zone.Name); }
            }          
            UpdateBuildingConstructionWWROperations();
        }
    }    
}
