using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Zone
    {
        public double HeatingEnergy, CoolingEnergy, LightingEnergy;

        public double[] p_HeatingEnergy, p_CoolingEnergy, p_LightingEnergy;
        public List<BuildingSurface> Surfaces { get; set; }
        public List<InternalMass> iMasses = new List<InternalMass>();
        public double Area, Volume, Height;
        public DayLighting DayLightControl;
        public People people { get; set; }
        public ElectricEquipment equipment { get; set; }
        public Light Lights { get; set; }
        public ZoneHVAC HVAC { get; set; }
        public Thermostat Thermostat { get; set; }
        public ZoneVentilation Ventilation, NaturalVentiallation;
        public ZoneInfiltration Infiltration;
        public string Name { get; set; }
        public int Level { get; set; }
        public Building building;

        public double totalWallArea, totalWindowArea, totalGFloorArea, totalIFloorArea, totalIWallArea, totalIFloorAreaExOther, totalIWallAreaExOther, totalRoofArea,
            TotalHeatCapacity, TotalHeatCapacityDeDuplicatingIntSurfaces, SurfAreaU, SolarRadiation, TotalHeatFlows, ExSurfAreaU, GSurfAreaU, ISurfAreaU,
            wallAreaU, windowAreaU, gFloorAreaU, roofAreaU, iFloorAreaU, iWallAreaU,
            wallHeatFlow, windowHeatFlow, gFloorHeatFlow, iFloorHeatFlow, iWallHeatFlow, roofHeatFlow, infiltrationFlow, windowAreaG;

        public double[] p_TotalHeatFlows, p_wallHeatFlow, p_windowHeatFlow, p_gFloorHeatFlow, p_iFloorHeatFlow, p_iWallHeatFlow, p_roofHeatFlow, p_infiltrationFlow, p_SolarRadiation;

        internal void CalcAreaVolume()
        {
            IEnumerable<BuildingSurface> floors = Surfaces.Where(a => a.surfaceType == SurfaceType.Floor);
            Area = floors.Select(a => a.Area).Sum();
            Volume = Area * Height;
        }
        internal void CalcAreaVolumeHeatCapacity()
        {
            CalcAreaVolume();

            totalWallArea = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").Select(w => w.Area).Sum();
            totalGFloorArea = Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").Select(gF => gF.Area).Sum();

            totalIFloorAreaExOther = Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition != "Ground").Select(iF => iF.Area).Sum();
            totalIFloorArea = totalIFloorAreaExOther +
                building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Floor && iF.OutsideCondition != "Ground" && iF.OutsideObject == Name).Select(iF => iF.Area).Sum();

            totalIWallArea = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition != "Outdoors").Select(iF => iF.Area).Sum() + iMasses.Where(i => i.IsWall).Select(i => i.area).Sum();
            totalIWallAreaExOther = totalIWallArea +
            building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Wall && iF.OutsideCondition != "Outdoors" && iF.OutsideObject == Name).Select(iF => iF.Area).Sum() +
                iMasses.Where(i => i.IsWall).Select(i => i.area).Sum();

            totalRoofArea = Surfaces.Where(w => w.surfaceType == SurfaceType.Roof).Select(r => r.Area).Sum();
            totalWindowArea = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                .Where(w => w.Fenestrations.Count != 0).SelectMany(w => w.Fenestrations).Select(wi => wi.Area).Sum();

            TotalHeatCapacity = totalWallArea * building.buildingConstruction.hcWall + totalGFloorArea * building.buildingConstruction.hcGFloor +
                totalIFloorArea * building.buildingConstruction.hcIFloor +
                totalIWallArea * building.buildingConstruction.hcIWall + totalRoofArea * building.buildingConstruction.hcRoof +
                iMasses.Select(m => m.area * building.buildingConstruction.hcIWall).Sum();

            TotalHeatCapacityDeDuplicatingIntSurfaces = totalWallArea * building.buildingConstruction.hcWall + totalGFloorArea * building.buildingConstruction.hcGFloor +
                totalIFloorAreaExOther * building.buildingConstruction.hcIFloor +
                totalIWallAreaExOther * building.buildingConstruction.hcIWall + totalRoofArea * building.buildingConstruction.hcRoof +
                iMasses.Select(m => m.area * building.buildingConstruction.hcIWall).Sum();

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
            windowAreaG = totalWindowArea * building.buildingConstruction.gWindow;
        }
        public void AssociateEnergyPlusResults(Dictionary<string, double[]> resultsDF)
        {
            wallHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").Select(s => s.HeatFlow).Sum();
            gFloorHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").Select(s => s.HeatFlow).Sum();
            iFloorHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition != "Ground").Select(s => s.HeatFlow).Sum();
            iWallHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition != "Outdoors").Select(s => s.HeatFlow).Sum();
            windowHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                .Where(w => w.Fenestrations.Count != 0).SelectMany(w => w.Fenestrations).Select(s => s.HeatFlow).Sum();
            roofHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Roof).Select(s => s.HeatFlow).Sum();

            iFloorHeatFlow -= building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Floor && iF.OutsideObject == Name).Select(s => s.HeatFlow).Sum();
            iWallHeatFlow -= building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Wall && iF.OutsideCondition != "Outdoors" && iF.OutsideObject == Name).Select(s => s.HeatFlow).Sum();

            SolarRadiation = building.buildingConstruction.gWindow * Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                .Where(w => w.Fenestrations.Count != 0).SelectMany(w => w.Fenestrations).Select(f => f.Area * f.SolarRadiation).Sum();

            infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Gain Energy"))].SubtractArrayElementWise(
                     resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Loss Energy"))]).Average().ConvertKWhfromJoule();
            TotalHeatFlows = wallHeatFlow + gFloorHeatFlow + windowHeatFlow + roofHeatFlow + infiltrationFlow;

            HeatingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Zone Air System Sensible Heating Energy"))].ConvertKWhfromJoule().Average();
            CoolingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Zone Air System Sensible Cooling Energy"))].ConvertKWhfromJoule().Average();
            LightingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Zone Lights Electric Energy"))].ConvertKWhfromJoule().Average();

        }
        public void AssociateProbabilisticEnergyPlusResults(Dictionary<string, double[]> resultsDF)
        {
            p_wallHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            p_gFloorHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            p_iFloorHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition != "Ground").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            p_iWallHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition != "Outdoors").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            try
            {
                p_windowHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
              .Where(w => w.Fenestrations.Count != 0).SelectMany(w => w.Fenestrations).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            }
            catch { }
            p_roofHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Roof).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();

            p_iFloorHeatFlow = p_iFloorHeatFlow.SubtractArrayElementWise(building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Floor && iF.OutsideObject == Name).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise());
            p_iWallHeatFlow = p_iWallHeatFlow.SubtractArrayElementWise(building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Wall && iF.OutsideCondition != "Outdoors" && iF.OutsideObject == Name).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise());

            p_SolarRadiation = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                .Where(w => w.Fenestrations.Count != 0).SelectMany(w => w.Fenestrations).Select(f => f.p_SolarRadiation).ToList().AddArrayElementWise();

            p_infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Gain Energy"))].SubtractArrayElementWise(
                     resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Loss Energy"))]).ConvertKWhfromJoule();
            p_TotalHeatFlows = new List<double[]>() { p_wallHeatFlow, p_gFloorHeatFlow, p_iFloorHeatFlow, p_iWallHeatFlow, p_windowHeatFlow, p_roofHeatFlow, p_infiltrationFlow }.AddArrayElementWise();

            p_HeatingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Zone Air System Sensible Heating Energy"))].ConvertKWhfromJoule();
            p_CoolingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Zone Air System Sensible Cooling Energy"))].ConvertKWhfromJoule();
            p_LightingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Zone Lights Electric Energy"))].ConvertKWhfromJoule();

        }
        public void AssociateProbabilisticMLResults(Dictionary<string, double[]> resultsDF)
        {
            p_wallHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            p_gFloorHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();

            p_windowHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                .Where(w => w.Fenestrations.Count != 0).SelectMany(w => w.Fenestrations).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            p_roofHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Roof).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();

            p_infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a == Name + "_Infiltration")];
            p_TotalHeatFlows = new List<double[]>() { p_wallHeatFlow, p_gFloorHeatFlow, p_windowHeatFlow, p_roofHeatFlow, p_infiltrationFlow }.AddArrayElementWise();

            p_HeatingEnergy = resultsDF[resultsDF.Keys.First(a => a == Name + "_Heating Energy")];
            p_CoolingEnergy = resultsDF[resultsDF.Keys.First(a => a == Name + "_Cooling Energy")];

            p_LightingEnergy = resultsDF[resultsDF.Keys.First(a => a == Name + "_Lighting Energy")];
            LightingEnergy = p_LightingEnergy.Average();

        }

        public Zone() { }
        public Zone(Building building, string n, int l)
        {
            this.building = building;
            Name = n;
            Level = l;
            Surfaces = new List<BuildingSurface>();
            Height = building.FloorHeight;
        }
        public void CreateNaturalVentillation()
        {
            NaturalVentiallation = new ZoneVentilation(this, true);
        }
    }

}
