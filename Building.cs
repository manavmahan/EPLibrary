using IDFObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public ProbabilisticBuildingOperation pBuildingOperation;
        public ProbabilisticWWR pWWR;
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
        public BuildingConstruction buildingConstruction;
        public BuildingOperation buildingOperation;
        public EmbeddedEnergyParameters EEParameters;

        public double FloorHeight;
        public WWR WWR { get; set; } = new WWR(0.5, 0.5, 0.5, 0.5); //North, West, South, East
        public ShadingLength shadingLength { get; set; } = new ShadingLength(0, 0, 0, 0); //North, West, South, East

        public double[] heatingSetPoints = new double[] { 10, 20 };
        public double[] coolingSetPoints = new double[] { 28, 24 };
        public double equipOffsetFraction = 0.1;

        //Schedules Limits and Schedule
        public List<ScheduleLimits> schedulelimits { get; set; } = new List<ScheduleLimits>();
        public List<ScheduleCompact> schedulescomp { get; set; } = new List<ScheduleCompact>();

        //Material, WindowMaterial, Shade, Shading Control, Constructions and Window Constructions
        public List<Material> materials = new List<Material>();
        public List<WindowMaterial> windowMaterials = new List<WindowMaterial>();
        public List<WindowMaterialShade> windowMaterialShades = new List<WindowMaterialShade>();
        //public List<WindowShadingControl> shadingControls = new List<WindowShadingControl>();
        public List<Construction> constructions = new List<Construction>();

        //Zone, ZoneList, BuidlingSurface, ShadingOverhangs
        public List<Zone> zones = new List<Zone>();
        public List<ZoneList> zoneLists = new List<ZoneList>();
        //public List<BuildingSurface> bSurfaces = new List<BuildingSurface>();
        //public List<InternalMass> iMasses = new List<InternalMass>();
        //public List<ShadingOverhang> shadingOverhangs = new List<ShadingOverhang>();
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
        public void UpdateBuildingConstructionWWROperations(BuildingConstruction construction, WWR wWR, BuildingOperation bOperation)
        {
            buildingConstruction = Utility.DeepClone(construction);
            WWR = Utility.DeepClone(wWR);
            buildingOperation = Utility.DeepClone(bOperation);

            GenerateConstructionWithIComponentsU();
            UpdateFenestrations();
            UpdateBuildingOperations();
            UpdateZoneInfo();
        }
        void UpdateBuildingOperations()
        {
            CreateSchedules();
            GeneratePeopleLightEquipmentInfiltrationVentilation();
            GenerateHVAC();
        }
        void UpdateFenestrations()
        {            
            foreach (Zone zone in zones)
            {
                foreach (BuildingSurface toupdate in zone.Surfaces.Where(s => s.surfaceType == SurfaceType.Wall && s.OutsideCondition == "Outdoors"))
                {
                    toupdate.CreateWindowsShadingControlShadingOverhang(zone, WWR, shadingLength);
                }
                if (!zone.Surfaces.Any(s => s.Fenestrations!=null)) 
                {
                    zone.DayLightControl = null; 
                }
            }
        }
        void UpdateZoneInfo()
        {
            zones.ForEach(z => z.CalcAreaVolumeHeatCapacity(this));
        }
        public void CreateInternalMass(double percentArea, bool IsWall)
        {
            zones.ForEach(z =>
            {
                z.CalcAreaVolume();
                InternalMass mass = new InternalMass(z, percentArea * z.Area * FloorHeight, "InternalWall", IsWall);
            });

        }           
        public void CreatePVPanelsOnRoof()
        {
            PhotovoltaicPerformanceSimple photovoltaicPerformanceSimple = new PhotovoltaicPerformanceSimple();
            List<GeneratorPhotovoltaic> listPVs = new List<GeneratorPhotovoltaic>();
            List<BuildingSurface> bSurfaces = zones.SelectMany(z => z.Surfaces).ToList();
            List<BuildingSurface> roofs = bSurfaces.FindAll(s => s.surfaceType == SurfaceType.Roof);

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
            int hour1, hour2, minutes1, minutes2;
            if (buildingOperation.startTime == 0)
            {
                int[] time = Utility.HourToHHMM(buildingOperation.operatingHours);
                hour1 = time[0]; minutes1 = time[1]; hour2 = time[2]; minutes2 = time[3];

                buildingOperation.operatingHours = (hour2 * 60d + minutes2 - (hour1 * 60d + minutes1)) / 60;
            }
            else
            {
                hour1 = (int)Math.Truncate(buildingOperation.startTime);
                hour2 = (int)Math.Truncate(buildingOperation.endTime);
                minutes1 = (int)Math.Round(Math.Round((buildingOperation.startTime - hour1) * 6)) * 10;
                minutes2 = (int)Math.Round(Math.Round((buildingOperation.endTime - hour2) * 6)) * 10;
            }
            if (buildingOperation.heatingSetPoints != null)
            {
                heatingSetPoints = buildingOperation.heatingSetPoints;
                coolingSetPoints = buildingOperation.coolingSetPoints;
            }
            double heatingSetpoint1 = heatingSetPoints[0];//16;
            double heatingSetpoint2 = heatingSetPoints[1];//20;

            double coolingSetpoint1 = coolingSetPoints[0];//28;
            double coolingSetpoint2 = coolingSetPoints[1];//25;

            //60 minutes earlier
            int hour1b = hour1 - 1;
            int minutes1b = minutes1;

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
                scheduleLimitName = temp.name,
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
                scheduleLimitName = temp.name,
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
                scheduleLimitName = temp.name,
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
                scheduleLimitName = fractional.name,
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
                scheduleLimitName = fractional.name,
                daysTimeValue = ventilS
            };

            Dictionary<string, double> lehgV1 = new Dictionary<string, double>(), lehgV2 = new Dictionary<string, double>();
            lehgV1.Add(hour1 + ":" + minutes1, equipOffsetFraction);
            lehgV1.Add(hour2 + ":" + minutes2, 1);
            lehgV1.Add("24:00", equipOffsetFraction);
            leHeatGain.Add(days1, lehgV1);
            lehgV2.Add("24:00", equipOffsetFraction);
            leHeatGain.Add(days2, lehgV2);
            ScheduleCompact lehgSchedule = new ScheduleCompact()
            {
                name = "Electric Equipment and Lighting Schedule",
                scheduleLimitName = fractional.name,
                daysTimeValue = leHeatGain
            };

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
            schedulescomp.Add(occupSchedule);
            schedulescomp.Add(workEff);
            schedulescomp.Add(airVelo);
            schedulescomp.Add(infiltration);
        }
        void GenerateConstructionWithIComponentsU()
        {
            double uWall = buildingConstruction.uWall, uGFloor = buildingConstruction.uGFloor, uIFloor = buildingConstruction.uIFloor,
                uRoof = buildingConstruction.uRoof, uIWall = buildingConstruction.uIWall, uWindow = buildingConstruction.uWindow,
                gWindow = buildingConstruction.gWindow, hcSlab = buildingConstruction.hcSlab;

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
            buildingConstruction.hcRoof = layerListRoof.Select(l => l.thickness * l.sHC * l.density).Sum();
            Construction constructionRoof = new Construction("Up Roof Concrete", layerListRoof);

            List<Material> layerListWall = new List<Material>() { layer_M03, layer_I04, layer_G01 };

            Construction construction_Wall = new Construction("Wall ConcreteBlock", layerListWall);
            buildingConstruction.hcWall = layerListWall.Select(l => l.thickness * l.sHC * l.density).Sum();
            List<Material> layerListInternallWall = new List<Material>() { layer_Plasterboard, layer_iWallInsul, layer_Plasterboard };
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
            windowMaterials = new List<WindowMaterial>() { windowLayer };
            Construction window = new Construction("Glazing", new List<WindowMaterial>() { windowLayer });
            constructions.Add(window);

            //window shades
            windowMaterialShades = new List<WindowMaterialShade>() { (new WindowMaterialShade()) };
        }
        void GeneratePeopleLightEquipmentInfiltrationVentilation()
        {
            double startTime = 13 - .5 * buildingOperation.operatingHours;
            double endTime = 13 + .5 * buildingOperation.operatingHours;
            foreach (ZoneList zList in zoneLists)
            {
                zList.GeneratePeopleLightEquipmentVentilationInfiltrationThermostat(this, startTime, endTime, buildingOperation.areaPerPeople,
                    buildingOperation.lightHeatGain, buildingOperation.equipmentHeatGain, buildingConstruction.infiltration);
                schedulescomp.AddRange(zList.Schedules.Values);
            }
        }
        public void GenerateHVAC()
        {
            zones.ForEach(z => z.ThermostatName = zoneLists.First(zl => zl.zoneNames.Contains(z.Name)).Thermostat.name);
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
            boiler = new Boiler(buildingOperation.boilerEfficiency, "Electricity");
            chiller = new Chiller(buildingOperation.chillerCOP);
            tower = new Tower();
        }
        public Building() { }
        public void AssociateEnergyPlusResults(Dictionary<string, double[]> data)
        {
            List<BuildingSurface> bSurfaces = zones.SelectMany(z => z.Surfaces).ToList();
            foreach (BuildingSurface surf in bSurfaces)
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
            List<BuildingSurface> bSurfaces = zones.SelectMany(z => z.Surfaces).ToList();
            foreach (BuildingSurface surf in bSurfaces)
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
            List<BuildingSurface> bSurfaces = zones.SelectMany(z => z.Surfaces).ToList();
            foreach (BuildingSurface surf in bSurfaces)
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
