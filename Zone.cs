using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Zone
    {
        public float Area, Volume, Height;
        
        public EPZone EP = new EPZone();
        public List<EPZone> EPP;
        public List<EPZone> EPHourly;
        public List<Surface> Surfaces { get; set; }
        public List<InternalMass> iMasses = new List<InternalMass>();        
        public DayLighting DayLightControl;
        public ZoneFanCoilUnit ZoneFCU { get; set; }
        public ZoneBaseBoardHeat ZoneBBH;
        public ZoneHeatPump ZoneHP;
        public ZoneVAV ZoneVAV { get; set; }
        public ZoneIdealLoad ZoneIL;
        public string ThermostatName { get; set; }
        public ZoneVentilation NaturalVentiallation;
        public string Name { get; set; }
        public int Level { get; set; }

        public float totalWallArea, totalWindowArea, totalFloorArea, totalRoofArea, totalIFloorArea, totalIWallArea, 
            totalIFloorAreaExOther, totalIWallAreaExOther, 
            TotalHeatCapacity, TotalHeatCapacityDeDuplicatingIntSurfaces,            
            WallHeatFlow,WindowHeatFlow,wallWindowHeatFlow, FloorHeatFlow, RoofHeatFlow, infiltrationFlow, 
            SolarRadiation;

        public float[] h_wallwindowHeatFlow, h_wallHeatFlow, h_windowHeatFlow, h_gFloorHeatFlow, h_roofHeatFlow, h_infiltrationFlow, h_SolarRadiation;
        internal string OccupancyScheduleName;

        public void CreateDaylighting(float lightingLux)
        {
            List<XYZ[]> exWallPoints = Surfaces.Where(s => s.SurfaceType == SurfaceType.Wall &&
                       s.OutsideCondition == "Outdoors").Select(w => w.XYZList.XYZs.Take(2).ToArray()).ToList();
            var floorPoints = Surfaces.Where(s => s.SurfaceType == SurfaceType.Floor).
                                Select(f => f.XYZList);
            if (exWallPoints != null && exWallPoints.Count > 0)
            {
                XYZList dlPoint = Utility.GetDayLightPointsXYZList(floorPoints, exWallPoints);
                new DayLighting(this, "Occupancy Schedule", dlPoint.XYZs, lightingLux);
            }
        }
        internal void CalcAreaVolume()
        {
            IEnumerable<Surface> floors = Surfaces.Where(a => a.SurfaceType == SurfaceType.Floor);
            Area = floors.Select(a => a.Area).Sum();
            Volume = Area * Height;
        }
        internal void CalcAreaVolumeHeatCapacity(Building building)
        {
            CalcAreaVolume();
            List<Surface> bSurfaces = building.zones.SelectMany(z => z.Surfaces).ToList();
            totalWallArea = Surfaces.Where(w => w.SurfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").Select(w => w.Area).Sum();
            totalFloorArea = Surfaces.Where(w => w.SurfaceType == SurfaceType.Floor).Select(gF => gF.Area).Sum();

            totalIFloorAreaExOther = Surfaces.Where(w => w.SurfaceType == SurfaceType.Floor && w.OutsideCondition != "Ground").Select(iF => iF.Area).Sum();
            totalIFloorArea = totalIFloorAreaExOther +
                bSurfaces.Where(iF => iF.SurfaceType == SurfaceType.Floor && iF.OutsideCondition != "Ground" && iF.OutsideObject == Name).Select(iF => iF.Area).Sum();

            totalIWallAreaExOther = Surfaces.Where(w => w.SurfaceType == SurfaceType.Wall && w.OutsideCondition != "Outdoors").Select(iF => iF.Area).Sum() + iMasses.Where(i => i.IsWall).Select(i => i.area).Sum();
            totalIWallArea = totalIWallAreaExOther +
            bSurfaces.Where(iF => iF.SurfaceType == SurfaceType.Wall && iF.OutsideCondition != "Outdoors" && iF.OutsideObject == Name).Select(iF => iF.Area).Sum() +
                iMasses.Where(i => i.IsWall).Select(i => i.area).Sum();

            totalRoofArea = Surfaces.Where(w => w.SurfaceType == SurfaceType.Roof).Select(r => r.Area).Sum();
            totalWindowArea = Surfaces.Where(w => w.Fenestrations != null).Select(wi => wi.Fenestrations.Select(f=>f.Area).Sum()).Sum();

            TotalHeatCapacity = totalWallArea * building.Parameters.Construction.hcWall + totalFloorArea * building.Parameters.Construction.hcGFloor +
                totalIFloorArea * building.Parameters.Construction.hcIFloor +
                totalIWallArea * building.Parameters.Construction.hcIWall + totalRoofArea * building.Parameters.Construction.hcRoof +
                iMasses.Select(m => m.area * building.Parameters.Construction.hcInternalMass).Sum();

            TotalHeatCapacityDeDuplicatingIntSurfaces = totalWallArea * building.Parameters.Construction.hcWall + totalFloorArea * building.Parameters.Construction.hcGFloor +
                totalIFloorAreaExOther * building.Parameters.Construction.hcIFloor +
                totalIWallAreaExOther * building.Parameters.Construction.hcIWall + totalRoofArea * building.Parameters.Construction.hcRoof +
                iMasses.Select(m => m.area * building.Parameters.Construction.hcInternalMass).Sum();          
        }
        public void AssociateEnergyResultsAnnual(Dictionary<string, float[]> resultsDF)
        {
            WindowHeatFlow = 0;
            wallWindowHeatFlow = Surfaces.Where(w => w.SurfaceType == SurfaceType.Wall 
                && w.OutsideCondition == "Outdoors").Select(s => s.HeatFlow).Sum();
            try
            {
                WindowHeatFlow = Surfaces.Where(w => w.SurfaceType == SurfaceType.Wall 
                    && w.OutsideCondition == "Outdoors" && w.Fenestrations.Count > 0)
                    .SelectMany(s => s.Fenestrations).Select(w => w.HeatFlow).Sum();
            }
            catch { }
            WallHeatFlow = wallWindowHeatFlow - WindowHeatFlow;
            FloorHeatFlow = Surfaces.Where(w => w.SurfaceType == SurfaceType.Floor).Select(s => s.HeatFlow).Sum();
            RoofHeatFlow = Surfaces.Where(w => w.SurfaceType == SurfaceType.Roof).Select(s => s.HeatFlow).Sum();
            SolarRadiation = Surfaces.Where(w => w.Fenestrations != null).SelectMany(w => w.Fenestrations).Select(f => f.Area * f.SolarRadiation).Sum();
            try
            {
                try
                {
                    infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a.ToUpper().Contains(Name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Gain Energy"))].SubtractArrayElementWise(
                         resultsDF[resultsDF.Keys.First(a => a.ToUpper().Contains(Name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Loss Energy"))]).Average().ConvertWfromJoule();
                }
                catch
                {
                    infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a.ToUpper().Contains(Name.ToUpper()) && a.Contains("Infiltration"))].Average();
                }
            }
            catch { }
            if (resultsDF.First().Value.Length==12)
            {               
                EP.HeatingLoadMonthly = resultsDF[resultsDF.Keys.
                    First(a => a.ToUpper().Contains(Name.ToUpper()) && a.Contains("Heating"))];
                EP.CoolingLoadMonthly = resultsDF[resultsDF.Keys.
                    First(a => a.ToUpper().Contains(Name.ToUpper()) && a.Contains("Cooling"))];
                EP.LightsLoadMonthly = resultsDF[resultsDF.Keys.
                    First(a => a.ToUpper().Contains(Name.ToUpper()) && a.Contains("Lights"))];
                EP.SumAverageMonthlyValues();
            }
            else
            {                
                EP.HeatingLoad = resultsDF[resultsDF.Keys.First(a => a.ToUpper().Contains(Name.ToUpper()) && a.Contains("Heating"))].Average();
                EP.CoolingLoad = resultsDF[resultsDF.Keys.First(a => a.ToUpper().Contains(Name.ToUpper()) && a.Contains("Cooling"))].Average();
                EP.LightsLoad = resultsDF[resultsDF.Keys.First(a => a.ToUpper().Contains(Name.ToUpper()) && a.Contains("Lights"))].Average();
            }
        } 
        public void AssociateHourlyEnergyResults(Dictionary<string, float[]> resultsDF)
        {
            h_wallwindowHeatFlow = Surfaces.Where(w => w.SurfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").
                Select(s => s.h_HeatFlow).ToList().AddArrayElementWise();
            h_gFloorHeatFlow = Surfaces.Where(w => w.SurfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground")==null? new float[8760]:
                Surfaces.Where(w => w.SurfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").Select(s => s.h_HeatFlow).ToList().AddArrayElementWise();
            
            h_roofHeatFlow = Surfaces.Where(w => w.SurfaceType == SurfaceType.Roof) == null ? new float[8760] :
                Surfaces.Where(w => w.SurfaceType == SurfaceType.Roof).Select(s => s.h_HeatFlow).ToList().AddArrayElementWise();
            h_SolarRadiation = Surfaces.Where(w => w.Fenestrations != null).SelectMany(w => w.Fenestrations).Select(f =>  f.h_SolarRadiation.MultiplyBy(f.Area)).ToList().AddArrayElementWise();

            try
            {
                h_infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && 
                    a.Contains("Zone Infiltration Total Heat Gain Energy"))].SubtractArrayElementWise(
                         resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && 
                         a.Contains("Zone Infiltration Total Heat Loss Energy"))]).ConvertWfromJoule();
            }
            catch 
            {
                h_infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Infiltration"))];
            }
                       
            EP.HeatingLoadHourly = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Heating"))];
            EP.CoolingLoadHourly = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Cooling"))];
            EP.LightsLoadHourly = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Lights"))];
            
        }
        public Zone() { }
        public Zone(float height, string name, int level)
        {
            Name = name;
            Level = level;
            Surfaces = new List<Surface>();
            Height = height;
        }
    }
}
