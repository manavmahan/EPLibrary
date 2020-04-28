using IDFObjects;
using System;
using System.Collections.Generic;
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
        // run UpdateBuildingConstructionWWROperations
               
        
        //Probablistic Attributes
        public ProbabilisticBuildingConstruction pBuildingConstruction;
        public ProbabilisticBuildingZoneOperation pBuildingOperation;
        public ProbabilisticBuildingWWR pWWR;
        public ProbabilisticEmbeddedEnergyParameters p_EEParameters;

        //Probabilistic Operational Energy
        public double[] p_BoilerEnergy, p_ChillerEnergy, p_ThermalEnergy, p_OperationalEnergy;
        public double[] p_ZoneHeatingEnergy, p_ZoneCoolingEnergy, p_LightingEnergy;

        //EnergyPlus Output
        public double BoilerEnergy, ChillerEnergy, ThermalEnergy, OperationalEnergy;
        public double ZoneHeatingEnergy, ZoneCoolingEnergy, LightingEnergy;
        public double TotalArea, TotalVolume;

        //Probabilistic Embedded Energy Output
        public double[] p_EmbeddedEnergy, p_PERT_EmbeddedEnergy, p_PENRT_EmbeddedEnergy,
            p_LCE_PERT, p_LCE_PENRT, p_LifeCycleEnergy;
        public double EmbeddedEnergy, PERT_EmbeddedEnergy, PENRT_EmbeddedEnergy,
            LCE_PERT, LCE_PENRT, LifeCycleEnergy;
        public double life, PERTFactor, PENRTFactor;

        //Deterministic Attributes
        public BuildingConstruction Construction;
        public BuildingZoneOperation Operation;
        public BuildingZoneEnvironment Environment;
        public BuildingService Service;
        public BuildingZoneOccupant Occupants;

        public EmbeddedEnergyParameters EEParameters;

        public BuildingWWR WWR { get; set; } = new BuildingWWR(0, 0, 0, 0); //North, West, South, East
        public ShadingLength shadingLength { get; set; } = new ShadingLength(0, 0, 0, 0); //North, West, South, East

        public double[] heatingSetPoints = new double[] { 10, 20 };
        public double[] coolingSetPoints = new double[] { 28, 24 };
        public double equipOffsetFraction = 0.1;

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
        public List<ZoneList> zoneLists = new List<ZoneList>();
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
        public HotWaterLoop hWaterLoop;
        public Boiler boiler;

        //PV Panel on Roof
        public ElectricLoadCenterDistribution electricLoadCenterDistribution;

        //HVAC System
        public HVACSystem HVACSystem;
        public void UpdateBuildingConstructionWWROperations(BuildingConstruction construction, BuildingWWR wWR, 
            BuildingZoneOperation bOperation)
        {
            Construction = Utility.DeepClone(construction);
            WWR = Utility.DeepClone(wWR);
            Operation = Utility.DeepClone(bOperation);

            GenerateConstructionWithIComponentsU();
            CreateInternalMass(Construction.InternalMass);
            UpdateFenestrations();

            CreateSchedules();
            if (zoneLists.Any(zl=>zl.Schedules==null)) { GeneratePeopleLightEquipmentInfiltrationVentilation(); }
            zoneLists.ForEach(zl=>zl.UpdateDayLightControlSchedule(this));
            GenerateHVAC();
            UpdateZoneInfo();
        } 
        void UpdateFenestrations()
        {            
            foreach (Zone zone in zones)
            {
                foreach (Surface toupdate in zone.Surfaces.Where(s => s.surfaceType == SurfaceType.Wall && s.OutsideCondition == "Outdoors"))
                {
                    toupdate.CreateWindowsShadingControlShadingOverhang(zone, WWR, shadingLength);
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
        public void CreateInternalMass(double kJoulePerKgKm2)
        {
            double hcIWall = Construction.hcIWall;
            if (kJoulePerKgKm2 > 0) { 
                zones.ForEach(z =>
                {
                    z.CalcAreaVolume();
                    InternalMass mass = new InternalMass(z, 1000 * kJoulePerKgKm2 * z.Area / hcIWall, "InternalWall", false);
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
            double uWall = Construction.UWall, uGFloor = Construction.UGFloor, uIFloor = Construction.UIFloor,
                uRoof = Construction.URoof, uIWall = Construction.UIWall, uWindow = Construction.UWindow,
                gWindow = Construction.GWindow, hcSlab = Construction.HCSlab;

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
            Construction.hcRoof = layerListRoof.Select(l => l.thickness * l.sHC * l.density).Sum();
            Construction constructionRoof = new Construction("Up Roof Concrete", layerListRoof);

            List<Material> layerListWall = new List<Material>() { layer_M03, layer_I04, layer_G01 };

            Construction construction_Wall = new Construction("Wall ConcreteBlock", layerListWall);
            Construction.hcWall = layerListWall.Select(l => l.thickness * l.sHC * l.density).Sum();
            List<Material> layerListInternallWall = new List<Material>() { layer_Plasterboard, layer_iWallInsul, layer_Plasterboard };
            Construction construction_internalWall = new Construction("InternalWall", layerListInternallWall);
            Construction.hcIWall = layerListInternallWall.Select(l => l.thickness * l.sHC * l.density).Sum();
            List<Material> layerListGfloor = new List<Material>() { layer_floorSlab, layer_gFloorInsul };
            Construction construction_gFloor = new Construction("Slab_Floor", layerListGfloor);
            Construction.hcGFloor = layerListGfloor.Select(l => l.thickness * l.sHC * l.density).Sum();
            List<Material> layerListIFloor = new List<Material>() { layer_floorSlab, layer_iFloorInsul };
            Construction construction_floor = new Construction("General_Floor_Ceiling", layerListIFloor);
            Construction.hcIFloor = layerListIFloor.Select(l => l.thickness * l.sHC * l.density).Sum();
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
            foreach (ZoneList zList in zoneLists)
            {
                zList.GeneratePeopleLightEquipmentVentilationInfiltrationThermostat(this, Operation.GetStartEndTime(13), 
                    Occupants.AreaPerPerson, Operation.LHG, Operation.EHG,
                    Construction.Infiltration);

                schedulescomp.AddRange(zList.Schedules);
            }
            
        }
        public void GenerateHVAC()
        {
            zones.ForEach(z => z.ThermostatName = zoneLists.First(zl => zl.ZoneNames.Contains(z.Name)).Thermostat.name);
            zones.ForEach(z => z.OccupancyScheduleName = zoneLists.First(zl => zl.ZoneNames.Contains(z.Name)).
            Schedules.First(s=>s.name.Contains("Occupancy")).name);
            switch (HVACSystem)
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
            boiler = new Boiler(Service.BoilerEfficiency, "Electricity");
            chiller = new Chiller(Service.ChillerCOP);
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
            TotalArea = zones.Select(z => z.Area).Sum(); TotalVolume = zones.Select(z => z.Volume).Sum();
            ZoneHeatingEnergy = zones.Select(z => z.HeatingEnergy).Sum(); ZoneCoolingEnergy = zones.Select(z => z.CoolingEnergy).Sum();
            LightingEnergy = zones.Select(z => z.LightingEnergy).Sum();

            BoilerEnergy = data[data.Keys.First(a => a.Contains("Boiler Electric Energy"))].ConvertKWhfromJoule().Average();
            ChillerEnergy = data[data.Keys.First(a => a.Contains("Chiller Electric Energy"))].ConvertKWhfromJoule().Average();
            ChillerEnergy += data[data.Keys.First(a => a.Contains("Cooling Tower Fan Electric Energy"))].ConvertKWhfromJoule().Average();
            ThermalEnergy = ChillerEnergy + BoilerEnergy;
            OperationalEnergy = ThermalEnergy + LightingEnergy;
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
            TotalArea = zones.Select(z => z.Area).Sum(); TotalVolume = zones.Select(z => z.Volume).Sum();
            p_ZoneHeatingEnergy = zones.Select(z => z.p_HeatingEnergy).ToList().AddArrayElementWise();
            p_ZoneCoolingEnergy = zones.Select(z => z.p_CoolingEnergy).ToList().AddArrayElementWise();
            p_LightingEnergy = zones.Select(z => z.p_LightingEnergy).ToList().AddArrayElementWise();

            ZoneHeatingEnergy = p_ZoneHeatingEnergy.Average(); ZoneCoolingEnergy = p_ZoneCoolingEnergy.Average();
            LightingEnergy = p_LightingEnergy.Average();

            p_BoilerEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains("Boiler Electric Energy"))].ConvertKWhfromJoule();
            BoilerEnergy = p_BoilerEnergy.Average();
            p_ChillerEnergy = new List<double[]>() { resultsDF[resultsDF.Keys.First(a => a.Contains("Chiller Electric Energy"))],
            resultsDF[resultsDF.Keys.First(a => a.Contains("Cooling Tower Fan Electric Energy"))] }.AddArrayElementWise().ConvertKWhfromJoule();
            ChillerEnergy = p_ChillerEnergy.Average();

            p_ThermalEnergy = new List<double[]>() { p_ChillerEnergy, p_BoilerEnergy }.AddArrayElementWise();
            ThermalEnergy = p_ThermalEnergy.Average();
            p_OperationalEnergy = new List<double[]>() { p_ThermalEnergy, p_LightingEnergy }.AddArrayElementWise();
            OperationalEnergy = p_OperationalEnergy.Average();
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
            TotalArea = zones.Select(z => z.Area).Sum(); TotalVolume = zones.Select(z => z.Volume).Sum();
            p_ZoneHeatingEnergy = zones.Select(z => z.p_HeatingEnergy).ToList().AddArrayElementWise();
            p_ZoneCoolingEnergy = zones.Select(z => z.p_CoolingEnergy).ToList().AddArrayElementWise();

            ZoneHeatingEnergy = p_ZoneHeatingEnergy.Average(); ZoneCoolingEnergy = p_ZoneCoolingEnergy.Average();
            p_LightingEnergy = zones.Select(z => z.p_LightingEnergy).ToList().AddArrayElementWise();

            try
            {
                p_ThermalEnergy = resultsDF[resultsDF.Keys.First(s => s.Contains("Thermal Energy"))];
                ThermalEnergy = p_ThermalEnergy.Average();
            }
            catch
            {

            }

            p_OperationalEnergy = resultsDF[resultsDF.Keys.First(s => s.Contains("Operational Energy"))];
            OperationalEnergy = p_OperationalEnergy.Average();
            LightingEnergy = p_LightingEnergy.Average();
        }
        public void AssociateProbabilisticEmbeddedEnergyResults(Dictionary<string, double[]> resultsDF)
        {
            p_PERT_EmbeddedEnergy = resultsDF["PERT"];
            p_PENRT_EmbeddedEnergy = resultsDF["PENRT"];
            p_EmbeddedEnergy = new List<double[]>() { p_PENRT_EmbeddedEnergy, p_PENRT_EmbeddedEnergy }.AddArrayElementWise();

            p_LCE_PENRT = new List<double[]>() { p_PENRT_EmbeddedEnergy, p_OperationalEnergy.Select(x => x * life * PENRTFactor).ToArray() }.AddArrayElementWise();
            p_LCE_PERT = new List<double[]>() { p_PERT_EmbeddedEnergy, p_OperationalEnergy.Select(x => x * life * PERTFactor).ToArray() }.AddArrayElementWise();
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

            LCE_PENRT = OperationalEnergy * life * PENRTFactor;
            LCE_PERT = OperationalEnergy * life * PERTFactor;
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
            zoneLists.Add(zoneList);
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
       
    }    
}
