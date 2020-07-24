﻿using DocumentFormat.OpenXml.Drawing.Diagrams;
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
        public List<EPBuilding> EPP = new List<EPBuilding>();

        //EnergyPlus Output
        public EPBuilding EP = new EPBuilding();

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
        public ChilledWaterLoop cWaterLoop; public MixedWaterLoop mWaterLoop;
        public Chiller chiller;
        public Tower tower;

        public void CreateZoneLists()
        {
            IEnumerable<string> zoneListNames = Parameters.ZConditions.Select(o => o.Name);
            foreach(string zoneListName in zoneListNames)
            {
                ZoneConditions con = Parameters.ZConditions.First(o => o.Name == zoneListName);
                
                AddZoneList(new ZoneList()
                {
                    Name = zoneListName,
                    Conditions = con                 
                }) ;
            }
        }

        public HotWaterLoop hWaterLoop;
        public Boiler boiler;

        //PV Panel on Roof
        public ElectricLoadCenterDistribution electricLoadCenterDistribution;

        public void UpdateBuildingConstructionWWROperations()
        {
            GenerateConstructionMunich();
            CreateInternalMass();
            UpdateFenestrations();
            CreateSchedules();
            GeneratePeopleLightEquipmentInfiltrationVentilationThermostat();
            ZoneLists.ForEach(zl=>zl.UpdateDayLightControlSchedule(this));
            GenerateHVAC();
            UpdateZoneInfo();
        } 
        public void UpdateFenestrations()
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
            double hcIWall = Parameters.Construction.hcInternalMass;
            if (Parameters.Construction.InternalMass > 0) { 
                zones.ForEach(z =>
                {
                    z.CalcAreaVolume();
                    InternalMass mass = new InternalMass(z, 1000 * Parameters.Construction.InternalMass * 
                        z.Area / hcIWall, "InternalMass", false);
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
        void GenerateConstructionMunich()
        {
            double uWall = Parameters.Construction.UWall, uGFloor = Parameters.Construction.UGFloor,
                uIFloor = Parameters.Construction.UIFloor,
                uRoof = Parameters.Construction.URoof, uIWall = Parameters.Construction.UIWall, uWindow = Parameters.Construction.UWindow,
                gWindow = Parameters.Construction.GWindow, hcSlab = Parameters.Construction.HCSlab;

            double lambda_insulation = 0.035;

            //wall plaster layer
            Material wall_plasterlayer = new Material("wall_plasterlayer", "Smooth", 0.02, 0.7, 1400, 1000, 0.9, 0.4, 0.7);

            //wall0 layers
            Material wall_structure = new Material("wall_structure", "Smooth", 0.18, 2.3, 2300, 1100, 0.9, 0.7, 0.7);
            Material wall_insulation = new Material("wall_insulation", "Smooth", 0.18, lambda_insulation, 25, 1000, 0.9, 0.7, 0.7);

            //wall1 layers
            Material wall1_structure = new Material("wall1_structure", "Smooth", 0.3, 0.09, 650, 1100, 0.9, 0.7, 0.7);
            Material wall1_insulation = new Material("wall1_insulation", "Smooth", 0.06, lambda_insulation, 25, 1000, 0.9, 0.7, 0.7);

            //wall2 layers
            Material wall2_structure = new Material("wall2_structure", "Smooth", 0.175, 1.4, 2000, 1100, 0.9, 0.7, 0.7);
            Material wall2_insulation = new Material("wall2_insulation", "Smooth", 0.18, lambda_insulation, 25, 1000, 0.9, 0.7, 0.7);

            //internal wall layers
            Material plasterboard = new Material("plasterboard", "Rough", 0.05, 0.16, 800, 800, 0.9, 0.6, 0.6);
            Material iwall_insulation = new Material("iwall_insulation", "Rough", 0.025, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);

            //gFloor & iFloor layers
            Material pInsulation = new Material("pInsulation", "Smooth", 0.05, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);
            Material floor_structure = new Material("floor_structure", "Smooth", 0.2, 2.3, 2300, hcSlab, 0.9, 0.7, 0.7);
            Material gfloor_insulation = new Material("gfloor_insulation", "Smooth", 0.053, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);
            Material floor_cementscreed = new Material("floor_cementscreed", "Smooth", 0.08, 1.4, 2000, 1000, 0.9, 0.7, 0.7);

            Material ifloor_insulation = new Material("ifloor_insulation", "Smooth", 0.025, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);

            //Roof layers
            Material roof_insulation = new Material("roof_insulation", "Smooth", 0.23, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);
            Material roof_structure = new Material("roof_structure", "Smooth", 0.25, 2.3, 2300, hcSlab, 0.9, 0.7, 0.7);
            Material roof_plaster = new Material("roof_plaster", "Smooth", 0.02, 0.7, 1400, 1000, 0.9, 0.7, 0.7);

            List<Material> layerListRoof = new List<Material>() { pInsulation, roof_insulation, roof_structure, roof_plaster };
            Parameters.Construction.hcRoof = layerListRoof.Select(l => l.thickness * l.sHC * l.density).Sum();
            Construction construction_Roof = new Construction("Up Roof Concrete", layerListRoof);

            List<Material> layerListWall = new List<Material>() { wall_plasterlayer, wall_insulation, wall_structure, wall_plasterlayer };
            Construction construction_Wall = new Construction("Wall ConcreteBlock", layerListWall);
            Parameters.Construction.hcWall = layerListWall.Select(l => l.thickness * l.sHC * l.density).Sum();
            
            List<Material> layerListInternallWall = new List<Material>() { plasterboard, iwall_insulation, plasterboard };
            Construction construction_internalWall = new Construction("InternalWall", layerListInternallWall);
            Parameters.Construction.hcIWall = layerListInternallWall.Select(l => l.thickness * l.sHC * l.density).Sum();

            List<Material> layerListGfloor = new List<Material>() { pInsulation, floor_structure, gfloor_insulation, floor_cementscreed };
            Construction construction_gFloor = new Construction("Slab_Floor", layerListGfloor);
            Parameters.Construction.hcGFloor = layerListGfloor.Select(l => l.thickness * l.sHC * l.density).Sum();

            List<Material> layerListIFloor = new List<Material>() { floor_structure, ifloor_insulation, floor_cementscreed };
            Construction construction_ifloor = new Construction("General_Floor_Ceiling", layerListIFloor);
            Parameters.Construction.hcIFloor = layerListIFloor.Select(l => l.thickness * l.sHC * l.density).Sum();

            Material InternalMaterial = new Material(name: "InternalMaterial", rough: "Smooth", th: 0.12, conduct: 2.3, dense: 500, sH: 1000, tAbsorp: 0.85, sAbsorp: 0.85, vAbsorp: 0.7);
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
            WindowMaterial windowLayer = new WindowMaterial("Glazing Material", uWindow, gWindow, 0.1);
            windowMaterials = new List<WindowMaterial>() { windowLayer };
            Construction window = new Construction("Glazing", new List<WindowMaterial>() { windowLayer });
            constructions.Add(window);

            //window shades
            windowMaterialShades = new List<WindowMaterialShade>() { new WindowMaterialShade() };
        }
        void GenerateConstructionTausendpfund()
        {
            double uWall = Parameters.Construction.UWall, uGFloor = Parameters.Construction.UGFloor,
                uIFloor = Parameters.Construction.UIFloor,
                uRoof = Parameters.Construction.URoof, uIWall = Parameters.Construction.UIWall, uWindow = Parameters.Construction.UWindow,
                gWindow = Parameters.Construction.GWindow, hcSlab = Parameters.Construction.HCSlab;

            double lambda_insulation = 0.035;

            //wall plaster layer
            Material wall_plasterlayer = new Material("wall_plasterlayer", "Smooth", 0.02, 0.7, 1400, 900, 0.9, 0.4, 0.7);

            //wall0 layers
            Material wall_structure = new Material("wall_structure", "Smooth", 0.18, 2.3, 2300, 1100, 0.9, 0.7, 0.7);
            Material wall_insulation = new Material("wall_insulation", "Smooth", 0.18, lambda_insulation, 25, 1000, 0.9, 0.7, 0.7);         
            
            //wall1 layers
            Material wall1_structure = new Material("wall1_structure", "Smooth", 0.3, 0.09, 650, 1100, 0.9, 0.7, 0.7);
            Material wall1_insulation = new Material("wall1_insulation", "Smooth", 0.06, lambda_insulation, 25, 1000, 0.9, 0.7, 0.7);

            //wall2 layers
            Material wall2_structure = new Material("wall2_structure", "Smooth", 0.175, 1.4, 2000, 1100, 0.9, 0.7, 0.7);
            Material wall2_insulation = new Material("wall2_insulation", "Smooth", 0.18, lambda_insulation, 25, 1000, 0.9, 0.7, 0.7);

            //internal wall layers
            Material plasterboard = new Material("plasterboard", "Rough", 0.05, 0.16, 800, 800, 0.9, 0.6, 0.6);
            Material iwall_insulation = new Material("iwall_insulation", "Rough", 0.025, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);

            //gFloor & iFloor layers
            Material pInsulation = new Material("pInsulation", "Smooth", 0.12, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);
            Material floor_structure = new Material("floor_structure", "Smooth", 0.2, 2.3, 2300, hcSlab, 0.9, 0.7, 0.7);
            Material gfloor_insulation = new Material("gfloor_insulation", "Smooth", 0.053, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);
            Material floor_cementscreed = new Material("floor_cementscreed", "Smooth", 0.08, 1.4, 2000, 2500, 0.9, 0.7, 0.7);
            
            Material ifloor_insulation = new Material("ifloor_insulation", "Smooth", 0.025, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);

            //Roof layers
            Material roof_insulation = new Material("roof_insulation", "Smooth", 0.23, lambda_insulation, 20, 1000, 0.9, 0.7, 0.7);
            Material roof_structure = new Material("roof_structure", "Smooth", 0.25, 2.3, 2300, hcSlab, 0.9, 0.7, 0.7);
            Material roof_plaster = new Material("roof_plaster", "Smooth", 0.02, 0.7, 1400, 2500, 0.9, 0.7, 0.7);
         
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

            Material InternalMaterial = new Material(name: "InternalMaterial", rough: "Smooth", th: 0.12, conduct: 2.3, dense: 500, sH: 1000, tAbsorp: 0.85, sAbsorp: 0.85, vAbsorp: 0.7);
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
            WindowMaterial windowLayer = new WindowMaterial("Glazing Material", uWindow, gWindow, 0.1);
            windowMaterials = new List<WindowMaterial>() { windowLayer };
            Construction window = new Construction("Glazing", new List<WindowMaterial>() { windowLayer });
            constructions.Add(window);

            //window shades
            windowMaterialShades = new List<WindowMaterialShade>() { new WindowMaterialShade()};
        }
        public void GenerateConstructionBelgium()
        {
            double uWall = Parameters.Construction.UWall,
                uCWall = Parameters.Construction.UCWall,
                uGFloor = Parameters.Construction.UGFloor,
                uIFloor = Parameters.Construction.UIFloor,
                uRoof = Parameters.Construction.URoof,
                uIWall = Parameters.Construction.UIWall,
                uWindow = Parameters.Construction.UWindow,
                gWindow = Parameters.Construction.GWindow,
                hcSlab = Parameters.Construction.HCSlab;
            //InternalMass
            Material InternalMaterial = new Material(name: "InternalMaterial", rough: "Smooth", th: 0.12, conduct: 2.3, dense: 2300, sH: 1000, tAbsorp: 0.85, sAbsorp: 0.85, vAbsorp: 0.7);
            Construction InternalMass = new Construction("InternalMass", new List<Material>() { InternalMaterial });
            Parameters.Construction.hcInternalMass = new List<Material>() { InternalMaterial }.Select(l => l.thickness * l.sHC * l.density).Sum();
            //Roof
            Material Bitumen = new Material(name: "Bitumen", rough: "Smooth", th: 0.01, conduct: 0.230, dense: 1100, sH: 1000, tAbsorp: 0.9, sAbsorp: 0.9, vAbsorp: 0.7);
            Material PUR_Roof = new Material(name: "PUR_Roof", rough: "Smooth", th: 0.06, conduct: 0.026, dense: 35, sH: 1400, tAbsorp: 0.85, sAbsorp: 0.85, vAbsorp: 0.7);
            Material Screed = new Material(name: "Screed", rough: "Smooth", th: 0.06, conduct: 1.3, dense: 1600, sH: 1000, tAbsorp: 0.85, sAbsorp: 0.85, vAbsorp: 0.7);
            Material RConcrete1 = new Material(name: "RConcrete1", rough: "Smooth", th: 0.12, conduct: 2.3, dense: 2300, sH: 1000, tAbsorp: 0.85, sAbsorp: 0.85, vAbsorp: 0.7);
            Material Gypsum1 = new Material(name: "Gypsum1", rough: "Smooth", th: 0.01, conduct: 0.4, dense: 800, sH: 1000, tAbsorp: 0.85, sAbsorp: 0.4, vAbsorp: 0.7);
            Construction Roof = new Construction("Roof", new List<Material>(){
                Bitumen, PUR_Roof, Screed, RConcrete1, Gypsum1});

            //Exterior Wall
            Material Brick = new Material(name: "Brick", rough: "Smooth", th: 0.1, conduct: 0.75, dense: 1400, sH: 840, tAbsorp: 0.88, sAbsorp: 0.55, vAbsorp: 0.7);
            Material Cavity = new Material(name: "Cavity", rough: "Smooth", th: 0.03, conduct: 0.0258, dense: 1.2, sH: 1006, tAbsorp: 0.9, sAbsorp: 0.9, vAbsorp: 0.7);
            Material MWool_ExWall = new Material(name: "MWool_ExWall", rough: "Smooth", th: 0.05, conduct: 0.05, dense: 100, sH: 1030, tAbsorp: 0.9, sAbsorp: 0.7, vAbsorp: 0.7);
            Material Brickwork1 = new Material(name: "Brickwork1", rough: "Smooth", th: 0.14, conduct: 0.32, dense: 840, sH: 1300, tAbsorp: 0.88, sAbsorp: 0.55, vAbsorp: 0.7);
            Material Gypsum2 = new Material(name: "Gypsum2", rough: "Smooth", th: 0.02, conduct: 0.4, dense: 800, sH: 1000, tAbsorp: 0.85, sAbsorp: 0.4, vAbsorp: 0.7);
            Construction ExWall = new Construction("ExWall", new List<Material>(){
                Brick, Cavity, MWool_ExWall, Brickwork1, Gypsum2});

            //InWall
            Material MWool_InWall = new Material(name: "MWool_InWall", rough: "Smooth", th: 0.05, conduct: 0.05, dense: 100, sH: 1030, tAbsorp: 0.9, sAbsorp: 0.7, vAbsorp: 0.7);
            Material Brickwork2 = new Material(name: "Brickwork2", rough: "Smooth", th: 0.14, conduct: 0.27, dense: 850, sH: 840, tAbsorp: 0.85, sAbsorp: 0.55, vAbsorp: 0.7);
            Construction InWall = new Construction("InWall", new List<Material>(){
                 Gypsum1, MWool_InWall, Brickwork2, Gypsum1 });

            //GFloor
            Material FloorTiles = new Material(name: "FloorTiles", rough: "Smooth", th: 0.04, conduct: 0.44, dense: 1500, sH: 733, tAbsorp: 0.85, sAbsorp: 0.55, vAbsorp: 0.6);
            Material PUR_GFloor = new Material(name: "PUR_GFloor", rough: "Smooth", th: 0.06, conduct: 0.026, dense: 35, sH: 1400, tAbsorp: 0.85, sAbsorp: 0.85, vAbsorp: 0.7);
            Material RConcrete2 = new Material(name: "RConcrete2", rough: "Smooth", th: 0.15, conduct: 2.3, dense: 2300, sH: 1000, tAbsorp: 0.85, sAbsorp: 0.85, vAbsorp: 0.7);
            Construction GFloor = new Construction("GFloor", new List<Material>(){
                 FloorTiles, Screed, PUR_GFloor, RConcrete2 });


            //Ceiling
            Material PUR_Ceiling = new Material(name: "PUR_Ceiling", rough: "Smooth", th: 0.06, conduct: 0.026, dense: 35, sH: 1400, tAbsorp: 0.85, sAbsorp: 0.85, vAbsorp: 0.7);
            Construction Ceiling = new Construction("Ceiling", new List<Material>(){
                 FloorTiles, Screed, PUR_Ceiling, RConcrete1, Gypsum1 });

            //CommonWall
            Material EPS_CommonWall = new Material(name: "EPS", rough: "Smooth", th: 0.02, conduct: 0.035, dense: 20, sH: 1450, tAbsorp: 0.85, sAbsorp: 0.85, vAbsorp: 0.7);
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
            WindowMaterial windowLayer = new WindowMaterial("Glazing Material", uWindow, gWindow, 0.1);
            windowMaterials = new List<WindowMaterial>() { windowLayer };
            Construction window = new Construction("Glazing", new List<WindowMaterial>() { windowLayer });
            constructions.Add(window);

            //window shades
            windowMaterialShades = new List<WindowMaterialShade>() { (new WindowMaterialShade()) };
        }
        public void GeneratePeopleLightEquipmentInfiltrationVentilationThermostat()
        {
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
                        COP = new double[2] { Parameters.Service.HeatingCOP,Parameters.Service.CoolingCOP}
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
            Parameters.Service.BoilerEfficiency = Math.Min(0.98, Parameters.Service.BoilerEfficiency);
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
        public void AssociateEnergyResults(Dictionary<string, double[]> data)
        {
            //exception for using ML predictions
            try
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
                            win.HeatFlow = data[data.Keys.First(s => s.Contains(win.Name.ToUpper()) && s.Contains("Surface Window Net Heat Transfer Rate"))].Average();
                        }
                        surf.SolarRadiation = data[data.Keys.First(s => s.Contains(surf.Name.ToUpper()) && s.Contains("Surface Outside Face Incident Solar Radiation Rate per Area") && !s.Contains("WINDOW"))].Average();
                    }
                    surf.HeatFlow = data[data.Keys.First(s => s.Contains(surf.Name.ToUpper()) && s.Contains("Surface Inside Face Conduction Heat Transfer Rate"))].Average();
                }
            }
            catch { }
            foreach (Zone zone in zones)
            {
                zone.CalcAreaVolumeHeatCapacity(this); zone.AssociateEnergyResults(data);
            }
            if (data.First().Value.Length == 12)
            {
                EP = new EPBuilding()
                {
                    ZoneHeatingLoadMonthly = zones.Select(z => z.EP.HeatingLoadMonthly.ToArray()).ToList().AddArrayElementWise(),
                    ZoneCoolingLoadMonthly = zones.Select(z => z.EP.CoolingLoadMonthly.ToArray()).ToList().AddArrayElementWise(),
                    ZoneLightsLoadMonthly = zones.Select(z => z.EP.LightsLoadMonthly.ToArray()).ToList().AddArrayElementWise(),
                };
                double[] BoilerEnergy = new double[12], ChillerEnergy = new double[12], TowerEnergy = new double[12], HeatPumpEnergy = new double[12];
                
                if (data.Keys.Any(k=>k.Contains("Boiler")))
                    BoilerEnergy = data[data.Keys.First(a => a.Contains("Boiler"))].ConvertKWhfromJoule(); 
                if (data.Keys.Any(k=>k.Contains("Chiller")))
                    ChillerEnergy = data[data.Keys.First(a => a.Contains("Chiller"))].ConvertKWhfromJoule();
                if (data.Keys.Any(k => k.Contains("Cooling Tower")))
                    TowerEnergy = data[data.Keys.First(a => a.Contains("Cooling Tower"))].ConvertKWhfromJoule();
                if (data.Keys.Any(k => k.Contains("Heat Pump")))
                    TowerEnergy = data.Keys.Where(a => a.Contains("Heat Pump")).Select(k=>data[k]).ToList().AddArrayElementWise().ConvertKWhfromJoule();
                
                EP.ThermalEnergyMonthly = new List<double[]> { BoilerEnergy, ChillerEnergy, TowerEnergy, HeatPumpEnergy }.AddArrayElementWise();
                EP.OperationalEnergyMonthly = new List<double[]>() { EP.ThermalEnergyMonthly, EP.ZoneLightsLoadMonthly.ConvertKWhafromWm() }
                                    .AddArrayElementWise(); 
                if (data.Keys.Any(k => k.Contains("Thermal Energy")))
                {
                    EP.ThermalEnergyMonthly = data[data.Keys.First(a => a.Contains("Thermal Energy"))];
                    EP.OperationalEnergyMonthly = new List<double[]>() { EP.ThermalEnergyMonthly, EP.ZoneLightsLoadMonthly.ConvertKWhafromWm() }
                                    .AddArrayElementWise();
                }
                if (data.Keys.Any(k => k.Contains("Operational Energy")))
                {
                    EP.OperationalEnergyMonthly = data[data.Keys.First(a => a.Contains("Operational Energy"))];
                    EP.ThermalEnergyMonthly = EP.OperationalEnergyMonthly.SubtractArrayElementWise(EP.ZoneLightsLoadMonthly.ConvertKWhafromW());
                }
                EP.EUIMonthly = EP.OperationalEnergyMonthly.MultiplyBy(1/zones.Select(z => z.Area).Sum());
                EP.SumAverageMonthlyValues();
            }
            else
            {
                EP = new EPBuilding()
                {
                    ZoneHeatingLoad = zones.Select(z => z.EP.HeatingLoad).Sum(),
                    ZoneCoolingLoad = zones.Select(z => z.EP.CoolingLoad).Sum(),
                    ZoneLightsLoad = zones.Select(z => z.EP.LightsLoad).Sum()          
                };
                double BoilerEnergy = 0, ChillerEnergy = 0, TowerEnergy = 0, HeatPumpEnergy =0;

                if (data.Keys.Any(k => k.Contains("Boiler")))
                    BoilerEnergy = data[data.Keys.First(a => a.Contains("Boiler"))].Sum().ConvertKWhfromJoule();
                if (data.Keys.Any(k => k.Contains("Chiller")))
                    ChillerEnergy = data[data.Keys.First(a => a.Contains("Chiller"))].Sum().ConvertKWhfromJoule();
                if (data.Keys.Any(k => k.Contains("Cooling Tower")))
                    TowerEnergy = data[data.Keys.First(a => a.Contains("Cooling Tower"))].Sum().ConvertKWhfromJoule();
                if (data.Keys.Any(k => k.Contains("Heat Pump")))
                    TowerEnergy = data.Keys.Where(a => a.Contains("Heat Pump")).SelectMany(k => data[k]).Sum().ConvertKWhfromJoule();

                EP.ThermalEnergy = BoilerEnergy + ChillerEnergy + TowerEnergy + HeatPumpEnergy;

                if (data.Keys.Any(k => k.Contains("Thermal Energy"))) 
                {
                    EP.ThermalEnergy = data[data.Keys.First(a => a.Contains("Thermal Energy"))].Sum();
                    EP.OperationalEnergy = EP.ThermalEnergy + EP.ZoneLightsLoad.ConvertKWhafromWm();
                }

                if (data.Keys.Any(k => k.Contains("Operational Energy")))
                {
                    EP.OperationalEnergy = data[data.Keys.First(a => a.Contains("Operational Energy"))].Sum();
                    EP.ThermalEnergy = EP.OperationalEnergy - EP.ZoneLightsLoad.ConvertKWhafromW();
                }             
                EP.EUI = EP.OperationalEnergy / zones.Select(z => z.Area).Sum();
            }
        }
        public void AssociateProbabilisticEnergyResults(Dictionary<string, double[]> resultsDF)
        {
            int count = resultsDF.First().Value.Length;
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
                zone.CalcAreaVolumeHeatCapacity(this); zone.AssociateProbabilisticEnergyResults(resultsDF);
            }

            for (int i =0; i<count; i++)
            {
                EPBuilding E= new EPBuilding()
                {
                    ZoneHeatingLoad = zones.Select(z => z.EPP[i].HeatingLoad).ToList().Sum(),
                    ZoneCoolingLoad = zones.Select(z => z.EPP[i].CoolingLoad).ToList().Sum(),
                    ZoneLightsLoad = zones.Select(z => z.EPP[i].LightsLoad).ToList().Sum(),                   
                };
                try
                {
                    E.ThermalEnergy = (resultsDF[resultsDF.Keys.First(a => a.Contains("Boiler"))][i] +
                        resultsDF[resultsDF.Keys.First(a => a.Contains("Chiller"))][i] +
                        resultsDF[resultsDF.Keys.First(a => a.Contains("Cooling Tower"))][i]).ConvertKWhfromJoule();
                    E.OperationalEnergy = E.ThermalEnergy + E.ZoneLightsLoad * 8.76;
                }
                catch
                {
                    try
                    {
                        E.ThermalEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains("Thermal Energy"))][i];
                        E.OperationalEnergy = E.ThermalEnergy + E.ZoneLightsLoad * 8.76;
                    }
                    catch
                    {
                        E.OperationalEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains("Operational Energy"))][i];
                        E.ThermalEnergy = E.OperationalEnergy - E.ZoneLightsLoad * 8.76;
                    }
                }
                E.EUI = EP.OperationalEnergy / zones.Select(z => z.Area).Sum();
                EPP.Add(E);
            }
            EP = EPP.GetAverage();
        }
        public void AssociateProbabilisticEmbeddedEnergyResults(Dictionary<string, double[]> resultsDF)
        {
            //p_PERT_EmbeddedEnergy = resultsDF["PERT"];
            //p_PENRT_EmbeddedEnergy = resultsDF["PENRT"];
            //p_EmbeddedEnergy = new List<double[]>() { p_PENRT_EmbeddedEnergy, p_PENRT_EmbeddedEnergy }.AddArrayElementWise();

            //p_LCE_PENRT = new List<double[]>() { p_PENRT_EmbeddedEnergy, ProbablisticBEnergyPerformance.OperationalEnergy.Select((double x) => x * life * PENRTFactor).ToArray() }.AddArrayElementWise();
            //p_LCE_PERT = new List<double[]>() { p_PERT_EmbeddedEnergy, ProbablisticBEnergyPerformance.OperationalEnergy.Select(x => x * life * PERTFactor).ToArray() }.AddArrayElementWise();
            //p_LifeCycleEnergy = new List<double[]>() { p_LCE_PENRT, p_LCE_PERT }.AddArrayElementWise();

            //PERT_EmbeddedEnergy = p_PERT_EmbeddedEnergy.Average();
            //PENRT_EmbeddedEnergy = p_PENRT_EmbeddedEnergy.Average();
            //EmbeddedEnergy = p_EmbeddedEnergy.Average();
            //LCE_PENRT = p_LCE_PENRT.Average();
            //LCE_PERT = p_LCE_PERT.Average();
            //LifeCycleEnergy = p_LifeCycleEnergy.Average();
        }
        public void AssociateEmbeddedEnergyResults(Dictionary<string, double> resultsDF)
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
       
        public void InitialiseBuilding_SameFloorPlan(List<ZoneGeometryInformation> zonesInformation,
            BuildingDesignParameters parameters)
        {
            Parameters = parameters;
            CreateZoneLists();
            foreach (ZoneGeometryInformation zoneInfo in zonesInformation)
            {
                Zone zone = new Zone(zoneInfo.Height, zoneInfo.Name, zoneInfo.Level);
                XYZList floorPoints = zoneInfo.FloorPoints;                 
                if (zoneInfo.Level == 0)
                {
                    new Surface(zone, floorPoints.Reverse(), floorPoints.CalculateArea(), SurfaceType.Floor);
                }
                else
                {
                    new Surface(zone, floorPoints.Reverse(), floorPoints.CalculateArea(), SurfaceType.Floor)
                    {
                        ConstructionName = "General_Floor_Ceiling",
                        OutsideCondition = "Zone",
                        OutsideObject = zoneInfo.Name.Remove(zoneInfo.Name.LastIndexOf(':')+1) + (zoneInfo.Level - 1)
                    };
                }
                Utility.CreateZoneWalls(zone, zoneInfo.WallCreationData, zoneInfo.CeilingPoints);
                if (zoneInfo.Level == zonesInformation.Select(z=>z.Level).Max())
                {
                    zoneInfo.CeilingPoints.ForEach(ro=>
                    new Surface(zone, ro, ro.CalculateArea(), SurfaceType.Roof));
                }
                zone.CreateDaylighting(500);
                AddZone(zone);
                try { ZoneLists.First(zList => zList.Name == zone.Name.Split(':').First()).ZoneNames.Add(zone.Name); }
                catch { ZoneLists.FirstOrDefault().ZoneNames.Add(zone.Name); }
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
                
                Utility.CreateZoneWalls(zone, zoneInfo.WallCreationData, zoneInfo.CeilingPoints);
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