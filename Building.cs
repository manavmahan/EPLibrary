using System;
using System.Collections.Generic;
using System.Linq;

namespace IDFObjects
{
    [Serializable]
    public class Building
    {
        public bool Intialised = false;

        public string name = "Building1";
        public float northAxis = 0;
        public string terrain = String.Empty;
        public float loadConvergenceTolerance = 0.04f;
        public float tempConvergenceTolerance = 0.4f;
        public string solarDistribution = "FullExterior";
        public float maxNWarmUpDays = 25;
        public float minNWarmUpDays = 6;


        //EnergyPlus Output
        public EPBuilding EP = new EPBuilding();

        //Probabilistic Embedded Energy Output
        public float[] p_EmbeddedEnergy, p_PERT_EmbeddedEnergy, p_PENRT_EmbeddedEnergy,
            p_LCE_PERT, p_LCE_PENRT, p_LifeCycleEnergy;
        public float EmbeddedEnergy, PERT_EmbeddedEnergy, PENRT_EmbeddedEnergy,
            LCE_PERT, LCE_PENRT, LifeCycleEnergy;
        public float life, PERTFactor, PENRTFactor;

        //Deterministic Attributes
        public BuildingDesignParameters Parameters;
        public EmbeddedEnergyParameters EEParameters;
        public ShadingLength Shading { get; set; } = new ShadingLength(0, 0, 0, 0); //North, West, South, East

        //Schedules Limits and Schedule
        public List<ScheduleLimits> schedulelimits = new List<ScheduleLimits>();
        public List<ScheduleCompact> schedulescomp = new List<ScheduleCompact>();

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
        public List<XYZList> FloorPoints;
        public List<XYZList> RoofPoints;

        //HVAC Template - should be extracted from zone
        //public List<Thermostat> tStats = new List<Thermostat>();
        public VAV vav;
        public ChilledWaterLoop cWaterLoop; public MixedWaterLoop mWaterLoop;
        public Chiller chiller;
        public Tower tower;

        public float TotalExposedSurfaceArea, TotalVolume;
        public void GetSurfaceAreaVolume()
        {
            TotalExposedSurfaceArea = zones.SelectMany(z => z.Surfaces
                                    .Where(s => s.OutsideCondition == "Ground" || s.OutsideCondition == "Outdoors")
                                    .Select(s => s.GrossArea)).Sum();
            TotalVolume = zones.Select(z => z.Volume).Sum();
        }
        public void CreateZoneLists()
        {
            var names = Parameters.ZConditions.Select(o => o.Name);
            foreach(string name in names)
            {
                var conditions = Parameters.ZConditions.First(o => o.Name == name);
                
                AddZoneList(new ZoneList()
                {
                    Name = name,
                    Conditions = conditions                 
                });
            }
        }

        public HotWaterLoop hWaterLoop;
        public Boiler boiler;

        //PV Panel on Roof
        public ElectricLoadCenterDistribution electricLoadCenterDistribution;

        public void UpdateBuildingConstructionWWROperations(Location location)
        {
            if (location == Location.BRUSSELS_BEL)
                GenerateConstructionBelgium();
            else
                GenerateConstructionMunich();
            CreateInternalMass();
            UpdateFenestrations();
            CreateSchedules();
            GetSurfaceAreaVolume();
            GeneratePeopleLightEquipmentInfiltrationVentilationThermostat();
            ZoneLists.ForEach(zl=>zl.UpdateDayLightControlSchedule(this));
            GenerateHVAC();
            UpdateZoneInfo();
        } 
        public void UpdateFenestrations()
        {
            foreach (Zone zone in zones)
            {
                var walls = zone.Surfaces.Where(s => s.SurfaceType == SurfaceType.Wall &&
                                                        s.OutsideCondition == "Outdoors");
                foreach (var wall in walls)
                    wall.CreateWindowsShadingControlShadingOverhang(zone, Parameters.WWR, Shading);
                
                if (zone.Surfaces.All(s => s.Fenestrations == null))
                    zone.DayLightControl = null;
            }
        }
        
        public void UpdateZoneInfo()
        {
            zones.ForEach(z => z.CalcAreaVolumeHeatCapacity(this));
        }
        public void CreateInternalMass(float percentArea, bool IsWall)
        {
            zones.ForEach(z =>
            {
                z.CalcAreaVolume();
                InternalMass mass = new InternalMass(z, percentArea * z.Area * z.Height, "InternalWall", IsWall);
            });

        }
        public void CreateInternalMass()
        {
            float hcIWall = Parameters.Construction.hcInternalMass;
            if (Parameters.Construction.InternalMass > 0) { 
                zones.ForEach(z =>
                {
                    z.CalcAreaVolume();
                    InternalMass mass = new InternalMass(z, 1000f * Parameters.Construction.InternalMass * 
                        z.Area / hcIWall, "InternalMass", false);
                });
            }
        }
        public void CreatePVPanelsOnRoof()
        {
            PhotovoltaicPerformanceSimple photovoltaicPerformanceSimple = new PhotovoltaicPerformanceSimple();
            List<GeneratorPhotovoltaic> listPVs = new List<GeneratorPhotovoltaic>();
            List<Surface> bSurfaces = zones.SelectMany(z => z.Surfaces).ToList();
            List<Surface> roofs = bSurfaces.FindAll(s => s.SurfaceType == SurfaceType.Roof);

            roofs.ForEach(s => listPVs.Add(new GeneratorPhotovoltaic(s, photovoltaicPerformanceSimple, "AlwaysOn")));
            ElectricLoadCenterGenerators electricLoadCenterGenerators = new ElectricLoadCenterGenerators(listPVs);
            electricLoadCenterDistribution = new ElectricLoadCenterDistribution(electricLoadCenterGenerators);
        }
       
        public void CreateSchedules()
        {          
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
            
            ScheduleCompact nocooling = new ScheduleCompact()
            {
                name = "No Cooling",
                scheduleLimitName = temp.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, float>>() {
                { "AllDays", new Dictionary<string, float>() {{"24:00", 35} } } }
            };

            ScheduleCompact activity = new ScheduleCompact()
            {
                name = "People Activity Schedule",
                scheduleLimitName = activityLevel.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, float>>() {
                { "AllDays", new Dictionary<string, float>() {{"24:00", 125} } } }
            };

            ScheduleCompact workEff = new ScheduleCompact()
            {
                name = "Work Eff Sch",
                scheduleLimitName = fractional.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, float>>() {
                { "AllDays", new Dictionary<string, float>() {{"24:00", 1} } } }
            };

            ScheduleCompact airVelo = new ScheduleCompact()
            {
                name = "Air Velo Sch",
                scheduleLimitName = fractional.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, float>>() {
                { "AllDays", new Dictionary<string, float>() {{"24:00", .1f} } } }
            };

            //infiltration
            ScheduleCompact infiltration = new ScheduleCompact()
            {
                name = "Space Infiltration Schedule",
                scheduleLimitName = fractional.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, float>>() {
                { "AllDays", new Dictionary<string, float>() {{"24:00", 1} } } }
            };
            ScheduleCompact alwaysOn = new ScheduleCompact()
            {
                name = "AlwaysOn",
                scheduleLimitName = fractional.name,
                daysTimeValue = new Dictionary<string, Dictionary<string, float>>() {
                { "AllDays", new Dictionary<string, float>() {{"24:00", 1} } } }
            };
            schedulescomp.Add(alwaysOn);
            schedulescomp.Add(workEff);
            schedulescomp.Add(airVelo);
            schedulescomp.Add(infiltration);
        }
        void GenerateConstructionMunich()
        {
            float uWall = Parameters.Construction.UWall, uGFloor = Parameters.Construction.UGFloor,
                uIFloor = Parameters.Construction.UIFloor,
                uRoof = Parameters.Construction.URoof, uIWall = Parameters.Construction.UIWall, uWindow = Parameters.Construction.UWindow,
                gWindow = Parameters.Construction.GWindow, hcSlab = Parameters.Construction.HCSlab;

            float lambda_insulation = 0.035f;

            //wall plaster layer
            Material wall_plasterlayer = new Material("wall_plasterlayer", "Smooth", 0.02f, 0.7f, 1400, 1000, 0.9f, 0.4f, 0.7f);

            //wall0 layers
            Material wall_structure = new Material("wall_structure", "Smooth", 0.18f, 2.3f, 2300, 1100, 0.9f, 0.7f, 0.7f);
            Material wall_insulation = new Material("wall_insulation", "Smooth", 0.18f, lambda_insulation, 25, 1000, 0.9f, 0.7f, 0.7f);

            //wall1 layers
            Material wall1_structure = new Material("wall1_structure", "Smooth", 0.3f, 0.09f, 650, 1100, 0.9f, 0.7f, 0.7f);
            Material wall1_insulation = new Material("wall1_insulation", "Smooth", 0.06f, lambda_insulation, 25, 1000, 0.9f, 0.7f, 0.7f);

            //wall2 layers
            Material wall2_structure = new Material("wall2_structure", "Smooth", 0.175f, 1.4f, 2000, 1100, 0.9f, 0.7f, 0.7f);
            Material wall2_insulation = new Material("wall2_insulation", "Smooth", 0.18f, lambda_insulation, 25, 1000, 0.9f, 0.7f, 0.7f);

            //internal wall layers
            Material plasterboard = new Material("plasterboard", "Rough", 0.05f, 0.16f, 800, 800, 0.9f, 0.6f, 0.6f);
            Material iwall_insulation = new Material("iwall_insulation", "Rough", 0.025f, lambda_insulation, 20, 1000, 0.9f, 0.7f, 0.7f);

            //gFloor & iFloor layers
            Material pInsulation = new Material("pInsulation", "Smooth", 0.05f, lambda_insulation, 20, 1000, 0.9f, 0.7f, 0.7f);
            Material floor_structure = new Material("floor_structure", "Smooth", 0.2f, 2.3f, 2300, hcSlab, 0.9f, 0.7f, 0.7f);
            Material gfloor_insulation = new Material("gfloor_insulation", "Smooth", 0.053f, lambda_insulation, 20, 1000, 0.9f, 0.7f, 0.7f);
            Material floor_cementscreed = new Material("floor_cementscreed", "Smooth", 0.08f, 1.4f, 2000, 1000, 0.9f, 0.7f, 0.7f);

            Material ifloor_insulation = new Material("ifloor_insulation", "Smooth", 0.025f, lambda_insulation, 20, 1000, 0.9f, 0.7f, 0.7f);

            //Roof layers
            Material roof_insulation = new Material("roof_insulation", "Smooth", 0.23f, lambda_insulation, 20, 1000, 0.9f, 0.7f, 0.7f);
            Material roof_structure = new Material("roof_structure", "Smooth", 0.25f, 2.3f, 2300, hcSlab, 0.9f, 0.7f, 0.7f);
            Material roof_plaster = new Material("roof_plaster", "Smooth", 0.02f, 0.7f, 1400, 1000, 0.9f, 0.7f, 0.7f);

            List<Material> layerListRoof = new List<Material>() { pInsulation, roof_insulation, roof_structure, roof_plaster };
            Parameters.Construction.hcRoof = layerListRoof.Select(l => l.thickness * l.sHC * l.density).Sum();
            Construction construction_Roof = new Construction("Roof", layerListRoof);

            List<Material> layerListWall = new List<Material>() { wall_plasterlayer, wall_insulation, wall_structure, wall_plasterlayer };
            Construction construction_Wall = new Construction("ExternalWall", layerListWall);
            Parameters.Construction.hcWall = layerListWall.Select(l => l.thickness * l.sHC * l.density).Sum();
            
            List<Material> layerListInternallWall = new List<Material>() { plasterboard, iwall_insulation, plasterboard };
            Construction construction_internalWall = new Construction("InternalWall", layerListInternallWall);
            Parameters.Construction.hcIWall = layerListInternallWall.Select(l => l.thickness * l.sHC * l.density).Sum();

            List<Material> layerListGfloor = new List<Material>() { pInsulation, floor_structure, gfloor_insulation, floor_cementscreed };
            Construction construction_gFloor = new Construction("GroundFloor", layerListGfloor);
            Parameters.Construction.hcGFloor = layerListGfloor.Select(l => l.thickness * l.sHC * l.density).Sum();

            List<Material> layerListIFloor = new List<Material>() { floor_structure, ifloor_insulation, floor_cementscreed };
            Construction construction_ifloor = new Construction("Floor_Ceiling", layerListIFloor);
            Parameters.Construction.hcIFloor = layerListIFloor.Select(l => l.thickness * l.sHC * l.density).Sum();

            Material InternalMaterial = new Material(name: "InternalMaterial", rough: "Smooth", th: 0.12f, conduct: 2.3f, dense: 500, sH: 1000, tAbsorp: 0.85f, sAbsorp: 0.85f, vAbsorp: 0.7f);
            Construction InternalMass = new Construction("InternalMass", new List<Material>() { InternalMaterial });
            Parameters.Construction.hcInternalMass = new List<Material>() { InternalMaterial }.Select(l => l.thickness * l.sHC * l.density).Sum();

            construction_Roof.AdjustInsulation(uRoof, roof_insulation);
            construction_Wall.AdjustInsulation(uWall, wall_insulation);
            construction_gFloor.AdjustInsulation(uGFloor, gfloor_insulation);
            construction_ifloor.AdjustInsulation(uIFloor, ifloor_insulation);
            construction_internalWall.AdjustInsulation(uIWall, iwall_insulation);

            constructions = new List<Construction>() { InternalMass, construction_Roof, construction_Wall, construction_gFloor, construction_ifloor, construction_internalWall };
            materials = constructions.SelectMany(c => c.layers).Select(l => l.name).Distinct()
                .Select(n => constructions.SelectMany(c => c.layers).First(l => l.name == n)).ToList();

            //window construction
            WindowMaterial windowLayer = new WindowMaterial("Glazing Material", uWindow, gWindow, 0.1f);
            windowMaterials = new List<WindowMaterial>() { windowLayer };
            Construction window = new Construction("Glazing", new List<WindowMaterial>() { windowLayer });
            constructions.Add(window);

            //window shades
            windowMaterialShades = new List<WindowMaterialShade>() { new WindowMaterialShade() };
        }
        void GenerateConstructionTausendpfund()
        {
            float uWall = Parameters.Construction.UWall, uGFloor = Parameters.Construction.UGFloor,
                uIFloor = Parameters.Construction.UIFloor,
                uRoof = Parameters.Construction.URoof, uIWall = Parameters.Construction.UIWall, uWindow = Parameters.Construction.UWindow,
                gWindow = Parameters.Construction.GWindow, hcSlab = Parameters.Construction.HCSlab;

            float lambda_insulation = 0.035f;

            //wall plaster layer
            Material wall_plasterlayer = new Material("wall_plasterlayer", "Smooth", 0.02f, 0.7f, 1400, 900, 0.9f, 0.4f, 0.7f);

            //wall0 layers
            Material wall_structure = new Material("wall_structure", "Smooth", 0.18f, 2.3f, 2300, 1100, 0.9f, 0.7f, 0.7f);
            Material wall_insulation = new Material("wall_insulation", "Smooth", 0.18f, lambda_insulation, 25, 1000, 0.9f, 0.7f, 0.7f);         
            
            //wall1 layers
            Material wall1_structure = new Material("wall1_structure", "Smooth", 0.3f, 0.09f, 650, 1100, 0.9f, 0.7f, 0.7f);
            Material wall1_insulation = new Material("wall1_insulation", "Smooth", 0.06f, lambda_insulation, 25, 1000, 0.9f, 0.7f, 0.7f);

            //wall2 layers
            Material wall2_structure = new Material("wall2_structure", "Smooth", 0.175f, 1.4f, 2000, 1100, 0.9f, 0.7f, 0.7f);
            Material wall2_insulation = new Material("wall2_insulation", "Smooth", 0.18f, lambda_insulation, 25, 1000, 0.9f, 0.7f, 0.7f);

            //internal wall layers
            Material plasterboard = new Material("plasterboard", "Rough", 0.05f, 0.16f, 800, 800, 0.9f, 0.6f, 0.6f);
            Material iwall_insulation = new Material("iwall_insulation", "Rough", 0.025f, lambda_insulation, 20, 1000, 0.9f, 0.7f, 0.7f);

            //gFloor & iFloor layers
            Material pInsulation = new Material("pInsulation", "Smooth", 0.12f, lambda_insulation, 20, 1000, 0.9f, 0.7f, 0.7f);
            Material floor_structure = new Material("floor_structure", "Smooth", 0.2f, 2.3f, 2300, hcSlab, 0.9f, 0.7f, 0.7f);
            Material gfloor_insulation = new Material("gfloor_insulation", "Smooth", 0.053f, lambda_insulation, 20, 1000, 0.9f, 0.7f, 0.7f);
            Material floor_cementscreed = new Material("floor_cementscreed", "Smooth", 0.08f, 1.4f, 2000, 2500, 0.9f, 0.7f, 0.7f);

            Material ifloor_insulation = new Material("ifloor_insulation", "Smooth", 0.025f, lambda_insulation, 20, 1000, 0.9f, 0.7f, 0.7f);

            //Roof layers
            Material roof_insulation = new Material("roof_insulation", "Smooth", 0.23f, lambda_insulation, 20, 1000, 0.9f, 0.7f, 0.7f);
            Material roof_structure = new Material("roof_structure", "Smooth", 0.25f, 2.3f, 2300, hcSlab, 0.9f, 0.7f, 0.7f);
            Material roof_plaster = new Material("roof_plaster", "Smooth", 0.02f, 0.7f, 1400, 2500, 0.9f, 0.7f, 0.7f);

            List<Material> layerListRoof = new List<Material>() { pInsulation, roof_insulation, roof_structure, roof_plaster };
            Parameters.Construction.hcRoof = layerListRoof.Select(l => l.thickness * l.sHC * l.density).Sum();
            Construction construction_Roof = new Construction("Up Roof Concrete", layerListRoof);

            List<Material> layerListWall = new List<Material>() { wall_plasterlayer, wall_insulation, wall_structure, wall_plasterlayer };
            Construction construction_Wall = new Construction("Wall ConcreteBlock", layerListWall);
            Parameters.Construction.hcWall = layerListWall.Select(l => l.thickness * l.sHC * l.density).Sum();

            List<Material> layerListWall1 = new List<Material>() { wall_plasterlayer, wall1_insulation, wall1_structure, wall_plasterlayer };
            Construction construction_Wall1 = new Construction("Wall ConcreteBlock1", layerListWall);
            Parameters.Construction.hcWall = layerListWall.Select(l => l.thickness * l.sHC * l.density).Sum();

            List<Material> layerListWall2 = new List<Material>() { wall_plasterlayer, wall2_insulation, wall1_structure, wall_plasterlayer };
            Construction construction_Wall2 = new Construction("Wall ConcreteBlock2", layerListWall);
            Parameters.Construction.hcWall = layerListWall.Select(l => l.thickness * l.sHC * l.density).Sum();

            List<Material> layerListInternallWall = new List<Material>() { plasterboard, iwall_insulation, plasterboard };
            Construction construction_internalWall = new Construction("InternalWall", layerListInternallWall);
            Parameters.Construction.hcIWall = layerListInternallWall.Select(l => l.thickness * l.sHC * l.density).Sum();
            
            List<Material> layerListGfloor = new List<Material>() { pInsulation, floor_structure, gfloor_insulation, floor_cementscreed };
            Construction construction_gFloor = new Construction("Slab_Floor", layerListGfloor);
            Parameters.Construction.hcGFloor = layerListGfloor.Select(l => l.thickness * l.sHC * l.density).Sum();
            
            List<Material> layerListIFloor = new List<Material>() { floor_structure, ifloor_insulation, floor_cementscreed};
            Construction construction_ifloor = new Construction("General_Floor_Ceiling", layerListIFloor);
            Parameters.Construction.hcIFloor = layerListIFloor.Select(l => l.thickness * l.sHC * l.density).Sum();

            Material InternalMaterial = new Material(name: "InternalMaterial", rough: "Smooth", th: 0.12f, conduct: 2.3f, dense: 500, sH: 1000, tAbsorp: 0.85f, sAbsorp: 0.85f, vAbsorp: 0.7f);
            Construction InternalMass = new Construction("InternalMass", new List<Material>() { InternalMaterial });
            Parameters.Construction.hcInternalMass = new List<Material>() { InternalMaterial }.Select(l => l.thickness * l.sHC * l.density).Sum();

            construction_Roof.AdjustInsulation(uRoof, roof_insulation);
            construction_Wall.AdjustInsulation(uWall, wall_insulation);
            construction_gFloor.AdjustInsulation(uGFloor, gfloor_insulation);
            construction_ifloor.AdjustInsulation(uIFloor, ifloor_insulation);
            construction_internalWall.AdjustInsulation(uIWall, iwall_insulation);

            constructions = new List<Construction>() { InternalMass, construction_Roof, construction_Wall, construction_Wall1, construction_Wall2,
                construction_gFloor, construction_ifloor, construction_internalWall };
            materials = constructions.SelectMany(c => c.layers).Select(l => l.name).Distinct()
                .Select(n => constructions.SelectMany(c => c.layers).First(l => l.name == n)).ToList();

            //window construction
            WindowMaterial windowLayer = new WindowMaterial("Glazing Material", uWindow, gWindow, 0.1f);
            windowMaterials = new List<WindowMaterial>() { windowLayer };
            Construction window = new Construction("Glazing", new List<WindowMaterial>() { windowLayer });
            constructions.Add(window);

            //window shades
            windowMaterialShades = new List<WindowMaterialShade>() { new WindowMaterialShade()};
        }
        public void GenerateConstructionBelgium()
        {
            float uWall = Parameters.Construction.UWall,
                uCWall = Parameters.Construction.UCWall,
                uGFloor = Parameters.Construction.UGFloor,
                uIFloor = Parameters.Construction.UIFloor,
                uRoof = Parameters.Construction.URoof,
                uIWall = Parameters.Construction.UIWall,
                uWindow = Parameters.Construction.UWindow,
                gWindow = Parameters.Construction.GWindow,
                hcSlab = Parameters.Construction.HCSlab;
            //InternalMass
            Material InternalMaterial = new Material(name: "InternalMaterial", rough: "Smooth", th: 0.12f, conduct: 2.3f, dense: 2300, sH: 1000, tAbsorp: 0.85f, sAbsorp: 0.85f, vAbsorp: 0.7f);
            Construction InternalMass = new Construction("InternalMass", new List<Material>() { InternalMaterial });
            Parameters.Construction.hcInternalMass = new List<Material>() { InternalMaterial }.Select(l => l.thickness * l.sHC * l.density).Sum();
            //Roof
            Material Bitumen = new Material(name: "Bitumen", rough: "Smooth", th: 0.01f, conduct: 0.230f, dense: 1100, sH: 1000, tAbsorp: 0.9f, sAbsorp: 0.9f, vAbsorp: 0.7f);
            Material PUR_Roof = new Material(name: "PUR_Roof", rough: "Smooth", th: 0.06f, conduct: 0.026f, dense: 35, sH: 1400, tAbsorp: 0.9f, sAbsorp: 0.9f, vAbsorp: 0.7f);
            Material Screed = new Material(name: "Screed", rough: "Smooth", th: 0.06f, conduct: 1.3f, dense: 1600, sH: 1000, tAbsorp: 0.85f, sAbsorp: 0.85f, vAbsorp: 0.7f);
            Material RConcrete1 = new Material(name: "RConcrete1", rough: "Smooth", th: 0.12f, conduct: 2.3f, dense: 2300, sH: 1000, tAbsorp: 0.85f, sAbsorp: 0.85f, vAbsorp: 0.7f);
            Material Gypsum1 = new Material(name: "Gypsum1", rough: "Smooth", th: 0.01f, conduct: 0.4f, dense: 800, sH: 1000, tAbsorp: 0.85f, sAbsorp: 0.4f, vAbsorp: 0.7f);
            Construction Roof = new Construction("Roof", new List<Material>(){
                Bitumen, PUR_Roof, Screed, RConcrete1, Gypsum1});

            //Exterior Wall
            Material Brick = new Material(name: "Brick", rough: "Smooth", th: 0.1f, conduct: 0.75f, dense: 1400, sH: 840, tAbsorp: 0.88f, sAbsorp: 0.55f, vAbsorp: 0.7f);
            Material Cavity = new Material(name: "Cavity", rough: "Smooth", th: 0.03f, conduct: 0.0258f, dense: 1.2f, sH: 1006, tAbsorp: 0.9f, sAbsorp: 0.9f, vAbsorp: 0.7f);
            Material MWool_ExWall = new Material(name: "MWool_ExWall", rough: "Smooth", th: 0.05f, conduct: 0.05f, dense: 100, sH: 1030, tAbsorp: 0.9f, sAbsorp: 0.7f, vAbsorp: 0.7f);
            Material Brickwork1 = new Material(name: "Brickwork1", rough: "Smooth", th: 0.14f, conduct: 0.32f, dense: 840, sH: 1300, tAbsorp: 0.88f, sAbsorp: 0.55f, vAbsorp: 0.7f);
            Material Gypsum2 = new Material(name: "Gypsum2", rough: "Smooth", th: 0.02f, conduct: 0.4f, dense: 800, sH: 1000, tAbsorp: 0.85f, sAbsorp: 0.4f, vAbsorp: 0.7f);
            Construction ExWall = new Construction("ExternalWall", new List<Material>(){
                Brick, Cavity, MWool_ExWall, Brickwork1, Gypsum2});

            //InWall
            Material MWool_InWall = new Material(name: "MWool_InWall", rough: "Smooth", th: 0.05f, conduct: 0.05f, dense: 100, sH: 1030, tAbsorp: 0.9f, sAbsorp: 0.7f, vAbsorp: 0.7f);
            Material Brickwork2 = new Material(name: "Brickwork2", rough: "Smooth", th: 0.14f, conduct: 0.27f, dense: 850, sH: 840, tAbsorp: 0.85f, sAbsorp: 0.55f, vAbsorp: 0.7f);
            Construction InWall = new Construction("InternalWall", new List<Material>(){
                 Gypsum1, MWool_InWall, Brickwork2, Gypsum1 });

            //GFloor
            Material FloorTiles = new Material(name: "FloorTiles", rough: "Smooth", th: 0.04f, conduct: 0.44f, dense: 1500, sH: 733, tAbsorp: 0.85f, sAbsorp: 0.55f, vAbsorp: 0.6f);
            Material PUR_GFloor = new Material(name: "PUR_GFloor", rough: "Smooth", th: 0.06f, conduct: 0.026f, dense: 35, sH: 1400, tAbsorp: 0.85f, sAbsorp: 0.85f, vAbsorp: 0.7f);
            Material RConcrete2 = new Material(name: "RConcrete2", rough: "Smooth", th: 0.15f, conduct: 2.3f, dense: 2300, sH: 1000, tAbsorp: 0.85f, sAbsorp: 0.85f, vAbsorp: 0.7f);
            Construction GFloor = new Construction("GroundFloor", new List<Material>(){
                 FloorTiles, Screed, PUR_GFloor, RConcrete2 });


            //Ceiling
            Material PUR_Ceiling = new Material(name: "PUR_Ceiling", rough: "Smooth", th: 0.06f, conduct: 0.026f, dense: 35, sH: 1400, tAbsorp: 0.85f, sAbsorp: 0.85f, vAbsorp: 0.7f);
            Construction Ceiling = new Construction("Floor_Ceiling", new List<Material>(){
                 FloorTiles, Screed, PUR_Ceiling, RConcrete1, Gypsum1 });

            //CommonWall
            Material EPS_CommonWall = new Material(name: "EPS", rough: "Smooth", th: 0.02f, conduct: 0.035f, dense: 20, sH: 1450, tAbsorp: 0.85f, sAbsorp: 0.85f, vAbsorp: 0.7f);
            Construction CommonWall = new Construction("CommonWall", new List<Material>(){
                 Gypsum2, Brickwork2, EPS_CommonWall, Brickwork2, Gypsum2 });

            Roof.AdjustInsulation(uRoof, PUR_Roof);
                ExWall.AdjustInsulation(uWall, MWool_ExWall);
                InWall.AdjustInsulation(uIWall, MWool_InWall);
                GFloor.AdjustInsulation(uGFloor, PUR_GFloor);
                Ceiling.AdjustInsulation(uIFloor, PUR_Ceiling);
                CommonWall.AdjustInsulation(uCWall, EPS_CommonWall);


            constructions.AddRange(new List<Construction>() { InternalMass, Roof, ExWall, InWall, GFloor, Ceiling, CommonWall });
            materials.AddRange(constructions.SelectMany(c => c.layers).Select(l=>l.name).Distinct()
                .Select(n=> constructions.SelectMany(c => c.layers).First(l=>l.name==n)));
            //window construction
            WindowMaterial windowLayer = new WindowMaterial("Glazing Material", uWindow, gWindow, 0.1f);
            windowMaterials = new List<WindowMaterial>() { windowLayer };
            Construction window = new Construction("Glazing", new List<WindowMaterial>() { windowLayer });
            constructions.Add(window);

            //window shades
            windowMaterialShades = new List<WindowMaterialShade>() { (new WindowMaterialShade()) };
        }
        public void GeneratePeopleLightEquipmentInfiltrationVentilationThermostat()
        {
            if (Parameters.Construction.Permeability != 0)
                Parameters.Construction.Infiltration = (float) Math.Round(0.1 + 
                    Parameters.Construction.Permeability * 0.07 * 1.25 * TotalExposedSurfaceArea / TotalVolume, 3);
            foreach (ZoneList zList in ZoneLists)
            {
                zList.GeneratePeopleLightEquipmentVentilationInfiltrationThermostat(this);
                schedulescomp.AddRange(zList.Schedules);
            }
        }
        public void GenerateHVAC()
        {
            ZoneLists.ForEach(z => z.CreateVentialtionNatural(Parameters.ZConditions.First().CoolingSetpoint));
            zones.ForEach(z => z.ThermostatName = ZoneLists.First(zl => zl.ZoneNames.Contains(z.Name)).Thermostat.name);
            zones.ForEach(z => z.OccupancyScheduleName = ZoneLists.First(zl => zl.ZoneNames.Contains(z.Name)).
            Schedules.First(s=>s.name.Contains("Occupancy")).name);
            switch (Parameters.Service.HVACSystem)
            {
                case HVACSystem.HeatPumpWBoiler:
                    zones.ForEach(z => z.ZoneHP = new ZoneHeatPump()
                    {
                        ZoneName = z.Name,
                        ThermostatName = z.ThermostatName,
                        COP = new float[2] { Parameters.Service.HeatingCOP,Parameters.Service.CoolingCOP}
                    });
                    GenerateWaterLoopsAndSystem();
                    break;
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
            Parameters.Service.BoilerEfficiency = Math.Min(0.98f, Parameters.Service.BoilerEfficiency);
            if (Parameters.Service.HVACSystem == HVACSystem.HeatPumpWBoiler) 
            {
                mWaterLoop = new MixedWaterLoop();
                 
                boiler = new Boiler(Parameters.Service.BoilerEfficiency, "Electricity");
                tower = new Tower();
            }
            else
            {
                hWaterLoop = new HotWaterLoop();
                cWaterLoop = new ChilledWaterLoop();
                boiler = new Boiler(Parameters.Service.BoilerEfficiency, "Electricity");
                chiller = new Chiller(Parameters.Service.CoolingCOP);
                tower = new Tower();
            }
        }
        public Building() { }
        public void AssociateEnergyResultsAnnual(Dictionary<string, float[]> data)
        {
            try
            {
                List<Surface> bSurfaces = zones.SelectMany(z => z.Surfaces).ToList();
                foreach (Surface surf in bSurfaces)
                {
                    surf.HeatFlow = data[data.Keys.First(s => s.Contains(surf.Name.ToUpper()) && s.Contains("Surface Inside Face Conduction Heat Transfer Rate"))].Average();
                    if (surf.OutsideCondition == "Outdoors")
                    {
                        surf.SolarRadiation = data[data.Keys.First(s => s.Contains(surf.Name.ToUpper())
                                                && s.Contains("Surface Outside Face Incident Solar Radiation Rate per Area")
                                                && !s.Contains("WINDOW"))].Average() * surf.Area;
                        if (surf.Fenestrations != null && surf.Fenestrations.Count != 0)
                        {
                            Fenestration win = surf.Fenestrations[0];
                            win.SolarRadiation = win.Area * data[data.Keys.First(a => a.Contains(win.Name.ToUpper()) && a.Contains("Surface Outside Face Incident Solar Radiation Rate per Area"))].Average();
                            win.HeatFlow = data[data.Keys.First(s => s.Contains(win.Name.ToUpper()) && s.Contains("Surface Window Net Heat Transfer Rate"))].Average();
                            surf.HeatFlow += win.HeatFlow;
                            surf.SolarRadiation += win.SolarRadiation;
                        }                     
                    }                  
                }
            }
            catch { }
            try
            {
                foreach (Zone zone in zones)
                {
                    zone.CalcAreaVolumeHeatCapacity(this); zone.AssociateEnergyResultsAnnual(data);
                }
            }
            catch { }
            EP = new EPBuilding()
            {
                ZoneHeatingLoad = zones.Select(z => z.EP.HeatingLoad).Sum(),
                ZoneCoolingLoad = zones.Select(z => z.EP.CoolingLoad).Sum(),
                ZoneLightsLoad = zones.Select(z => z.EP.LightsLoad).Sum()
            };
            float BoilerEnergy = 0, ChillerEnergy = 0, TowerEnergy = 0, HeatPumpEnergy = 0;

            if (data.Keys.Any(k => k.Contains("Boiler")))
                BoilerEnergy = data[data.Keys.First(a => a.Contains("Boiler"))].Sum().ConvertKWhfromJoule();
            if (data.Keys.Any(k => k.Contains("Chiller")))
                ChillerEnergy = data[data.Keys.First(a => a.Contains("Chiller"))].Sum().ConvertKWhfromJoule();
            if (data.Keys.Any(k => k.Contains("Cooling Tower")))
                TowerEnergy = data[data.Keys.First(a => a.Contains("Cooling Tower"))].Sum().ConvertKWhfromJoule();
            if (data.Keys.Any(k => k.Contains("Heat Pump")))
                HeatPumpEnergy = data.Keys.Where(a => a.Contains("Heat Pump")).SelectMany(k => data[k]).Sum().ConvertKWhfromJoule();

            EP.ThermalEnergy = BoilerEnergy + ChillerEnergy + TowerEnergy + HeatPumpEnergy;
            EP.OperationalEnergy = EP.ThermalEnergy + EP.ZoneLightsLoad.ConvertKWhafromW();

            if (data.Keys.Any(k => k.Contains("Thermal Energy")))
            {
                EP.ThermalEnergy = data[data.Keys.First(a => a.Contains("Thermal Energy"))].Sum();
                EP.OperationalEnergy = EP.ThermalEnergy + EP.ZoneLightsLoad.ConvertKWhafromW();
            }

            if (data.Keys.Any(k => k.Contains("Operational Energy")))
            {
                EP.OperationalEnergy = data[data.Keys.First(a => a.Contains("Operational Energy"))].Sum();
                EP.ThermalEnergy = EP.OperationalEnergy - EP.ZoneLightsLoad.ConvertKWhafromW();
            }
            EP.EUI = EP.OperationalEnergy / zones.Select(z => z.Area).Sum();
        }

        public void AssociateEnergyResultsHourly(Dictionary<string, float[]> data)
        {
            List<Surface> bSurfaces = zones.SelectMany(z => z.Surfaces).ToList();
            foreach (Surface surf in bSurfaces)
            {
                surf.h_HeatFlow = data[data.Keys.First(s => s.Contains(surf.Name.ToUpper()) && s.Contains("Surface Inside Face Conduction Heat Transfer Rate"))];

                if (surf.OutsideCondition == "Outdoors")
                {
                    if (surf.Fenestrations != null && surf.Fenestrations.Count != 0)
                    {
                        Fenestration win = surf.Fenestrations[0];
                        win.h_SolarRadiation = data[data.Keys.First(a => a.Contains(win.Name.ToUpper()) && a.Contains("Surface Outside Face Incident Solar Radiation Rate per Area"))];
                        surf.h_HeatFlow = new List<float[]> { surf.h_HeatFlow, data[data.Keys.First(s => s.Contains(win.Name.ToUpper()) && s.Contains("Surface Window Net Heat Transfer Rate"))] }.AddArrayElementWise();
                    }
                    surf.h_SolarRadiation = data[data.Keys.First(s => s.Contains(surf.Name.ToUpper()) && s.Contains("Surface Outside Face Incident Solar Radiation Rate per Area") && !s.Contains("WINDOW"))];
                }
                
            }
            foreach (Zone zone in zones)
            {
                zone.CalcAreaVolumeHeatCapacity(this); zone.AssociateHourlyEnergyResults(data);
            }

            EP = new EPBuilding()
            {
                ZoneHeatingLoadHourly = zones.Select(z => z.EP.HeatingLoadHourly.ToArray()).ToList().AddArrayElementWise(),
                ZoneCoolingLoadHourly = zones.Select(z => z.EP.CoolingLoadHourly.ToArray()).ToList().AddArrayElementWise(),
                ZoneLightsLoadHourly = zones.Select(z => z.EP.LightsLoadHourly.ToArray()).ToList().AddArrayElementWise(),
            };
            float[] BoilerEnergy = new float[8760], ChillerEnergy = new float[8760], TowerEnergy = new float[8760], HeatPumpEnergy = new float[8760];

            if (data.Keys.Any(k => k.Contains("Boiler")))
                BoilerEnergy = data[data.Keys.First(a => a.Contains("Boiler"))].ConvertKWhfromJoule();
            if (data.Keys.Any(k => k.Contains("Chiller")))
                ChillerEnergy = data[data.Keys.First(a => a.Contains("Chiller"))].ConvertKWhfromJoule();
            if (data.Keys.Any(k => k.Contains("Cooling Tower")))
                TowerEnergy = data[data.Keys.First(a => a.Contains("Cooling Tower"))].ConvertKWhfromJoule();
            if (data.Keys.Any(k => k.Contains("Heat Pump")))
                TowerEnergy = data.Keys.Where(a => a.Contains("Heat Pump")).Select(k => data[k]).ToList().AddArrayElementWise().ConvertKWhfromJoule();

            EP.ThermalEnergyHourly = new List<float[]> { BoilerEnergy, ChillerEnergy, TowerEnergy, HeatPumpEnergy }.AddArrayElementWise();
            EP.OperationalEnergyHourly = new List<float[]>() { EP.ThermalEnergyHourly, EP.ZoneLightsLoadHourly.MultiplyBy(0.001f) }.AddArrayElementWise();
            EP.EUIHourly = EP.OperationalEnergyHourly.MultiplyBy(1 / zones.Select(z => z.Area).Sum());
        }

        public void AssociateProbabilisticEmbeddedEnergyResults(Dictionary<string, float[]> resultsDF)
        {
            //p_PERT_EmbeddedEnergy = resultsDF["PERT"];
            //p_PENRT_EmbeddedEnergy = resultsDF["PENRT"];
            //p_EmbeddedEnergy = new List<float[]>() { p_PENRT_EmbeddedEnergy, p_PENRT_EmbeddedEnergy }.AddArrayElementWise();

            //p_LCE_PENRT = new List<float[]>() { p_PENRT_EmbeddedEnergy, ProbablisticBEnergyPerformance.OperationalEnergy.Select((float x) => x * life * PENRTFactor).ToArray() }.AddArrayElementWise();
            //p_LCE_PERT = new List<float[]>() { p_PERT_EmbeddedEnergy, ProbablisticBEnergyPerformance.OperationalEnergy.Select(x => x * life * PERTFactor).ToArray() }.AddArrayElementWise();
            //p_LifeCycleEnergy = new List<float[]>() { p_LCE_PENRT, p_LCE_PERT }.AddArrayElementWise();

            //PERT_EmbeddedEnergy = p_PERT_EmbeddedEnergy.Average();
            //PENRT_EmbeddedEnergy = p_PENRT_EmbeddedEnergy.Average();
            //EmbeddedEnergy = p_EmbeddedEnergy.Average();
            //LCE_PENRT = p_LCE_PENRT.Average();
            //LCE_PERT = p_LCE_PERT.Average();
            //LifeCycleEnergy = p_LifeCycleEnergy.Average();
        }
        public void AssociateEmbeddedEnergyResults(Dictionary<string, float> resultsDF)
        {
            PERT_EmbeddedEnergy = resultsDF["PERT"];
            PENRT_EmbeddedEnergy = resultsDF["PENRT"];
            EmbeddedEnergy = PENRT_EmbeddedEnergy + PENRT_EmbeddedEnergy;

            LCE_PENRT = EP.OperationalEnergy * life * PENRTFactor;
            LCE_PERT = EP.OperationalEnergy * life * PERTFactor;
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
        public void AdjustScheduleFromFile(List<ScheduleCompact> schedules)
        {
            foreach(ScheduleCompact s in schedules)
            {
                try
                {
                    schedulescomp.Remove(schedulescomp.First(sc => sc.name == s.name));
                    schedulescomp.Add(s);
                }
                catch { }                
            }
            
        }
        public void RemoveEmptyZoneList()
        {
            List<ZoneList> remove = new List<ZoneList>();
            foreach (ZoneList zl in ZoneLists)
            {
                if (zl.ZoneNames.Count==0)
                    remove.Add(zl);
            }
            remove.ForEach(z => ZoneLists.Remove(z));
        }
        public void InitialiseBuilding(List<ZoneGeometryInformation> zonesInformation, Location location, float offsetDistance = 0)
        {
            if (Intialised)
                CleanBuilding();

            if (zonesInformation == null)
            {
                if (FloorPoints == null || FloorPoints.Count == 0)
                {
                    FloorPoints = Utility.GetRange(0, Parameters.Geometry.NFloors).Select(i =>
                                                    GroundPoints.ChangeZValue(i * Parameters.Geometry.Height)).ToList();
                    RoofPoints = new List<XYZList>() { GroundPoints.ChangeZValue(Parameters.Geometry.Height * Parameters.Geometry.NFloors + 1) };
                }
                zonesInformation = Utility.GetZoneGeometryInformation(Utility.GetAllRooms(FloorPoints[0], offsetDistance, Parameters.ZConditions.First().Name),
                    FloorPoints, RoofPoints);
            }
            
            CreateZoneLists();
            foreach (ZoneGeometryInformation zoneInfo in zonesInformation)
            {
                Zone zone = new Zone(zoneInfo.Height, zoneInfo.Name, zoneInfo.Level);
                
                foreach (var c in zoneInfo.FloorPoints)
                    new Surface(zone, c, SurfaceType.Floor);

                foreach (var c in zoneInfo.OverhangPoints)
                    new Surface(zone, c, SurfaceType.Floor)
                    {
                        OutsideCondition = "Outdoors",
                        SunExposed = "SunExposed",
                        WindExposed = "WindExposed"
                    };

                if (zoneInfo.Level != 0)
                {
                    foreach (var surface  in zone.Surfaces.
                        Where(s => s.SurfaceType == SurfaceType.Floor))
                    { 
                        surface.ConstructionName = "Floor_Ceiling"; 
                        surface.OutsideCondition = "Adiabatic"; 
                    };
                }
                
                Utility.CreateZoneWalls(zone, zoneInfo);

                foreach (var c in zoneInfo.CeilingPoints)
                    new Surface(zone, c.Reverse(true), SurfaceType.Ceiling);

                foreach (var c in zoneInfo.RoofPoints)
                    new Surface(zone, c.Reverse(true), SurfaceType.Roof);

                zone.CreateDaylighting(500);
                
                AddZone(zone);
                ZoneList zoneList;
                try 
                { 
                    zoneList = ZoneLists.
                        FirstOrDefault(zList => zList.Name == zone.Name.Split(':').First()); 
                }
                catch 
                { 
                    zoneList = ZoneLists.FirstOrDefault(); 
                }
                zoneList.ZoneNames.Add(zone.Name);
            }          
            UpdateBuildingConstructionWWROperations(location);
            RemoveEmptyZoneList();
            Intialised = true;
        }

        private void CleanBuilding()
        {
            //Material, WindowMaterial, Shade, Shading Control, Constructions and Window Constructions
            materials.Clear();
            windowMaterials.Clear();
            windowMaterialShades.Clear();
            constructions.Clear();

            //Zone, ZoneList, BuidlingSurface, ShadingOverhangs
            zones.Clear();
            ZoneLists.Clear();
            DetachedShading.Clear();
        }
    }    
}
