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
        public double Area, Volume, Height;
        
        public EPZone EP = new EPZone();
        public List<EPZone> EPP;
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

        public double totalWallArea, totalWindowArea, totalGFloorArea, totalRoofArea, totalIFloorArea, totalIWallArea, 
            totalIFloorAreaExOther, totalIWallAreaExOther, 
            TotalHeatCapacity, TotalHeatCapacityDeDuplicatingIntSurfaces,            
            wallHeatFlow, windowHeatFlow, gFloorHeatFlow, roofHeatFlow, infiltrationFlow, 
            SolarRadiation;

        public double[] p_wallHeatFlow, p_windowHeatFlow, p_gFloorHeatFlow, p_roofHeatFlow, p_infiltrationFlow;
        internal string OccupancyScheduleName;

        public void CreateDaylighting(double lightingLux)
        {
            List<XYZ[]> exWallPoints = Surfaces.Where(s => s.surfaceType == SurfaceType.Wall &&
                       s.OutsideCondition == "Outdoors").Select(w => w.VerticesList.xyzs.Take(2).ToArray()).ToList();
            XYZList floorPoints = Surfaces.First(s => s.surfaceType == SurfaceType.Floor).VerticesList;
            if (exWallPoints != null && exWallPoints.Count > 0)
            {
                XYZList dlPoint = Utility.GetDayLightPointsXYZList(floorPoints, exWallPoints);
                new DayLighting(this, "Occupancy Schedule", dlPoint.OffsetHeight(0.9).xyzs, lightingLux);
            }
        }
        internal void CalcAreaVolume()
        {
            IEnumerable<Surface> floors = Surfaces.Where(a => a.surfaceType == SurfaceType.Floor);
            Area = floors.Select(a => a.Area).Sum();
            Volume = Area * Height;
        }
        internal void CalcAreaVolumeHeatCapacity(Building building)
        {
            CalcAreaVolume();
            List<Surface> bSurfaces = building.zones.SelectMany(z => z.Surfaces).ToList();
            totalWallArea = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").Select(w => w.Area).Sum();
            totalGFloorArea = Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").Select(gF => gF.Area).Sum();

            totalIFloorAreaExOther = Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition != "Ground").Select(iF => iF.Area).Sum();
            totalIFloorArea = totalIFloorAreaExOther +
                bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Floor && iF.OutsideCondition != "Ground" && iF.OutsideObject == Name).Select(iF => iF.Area).Sum();

            totalIWallAreaExOther = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition != "Outdoors").Select(iF => iF.Area).Sum() + iMasses.Where(i => i.IsWall).Select(i => i.area).Sum();
            totalIWallArea = totalIWallAreaExOther +
            bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Wall && iF.OutsideCondition != "Outdoors" && iF.OutsideObject == Name).Select(iF => iF.Area).Sum() +
                iMasses.Where(i => i.IsWall).Select(i => i.area).Sum();

            totalRoofArea = Surfaces.Where(w => w.surfaceType == SurfaceType.Roof).Select(r => r.Area).Sum();
            totalWindowArea = Surfaces.Where(w => w.Fenestrations != null).Select(wi => wi.Fenestrations.Select(f=>f.Area).Sum()).Sum();

            TotalHeatCapacity = totalWallArea * building.Parameters.Construction.hcWall + totalGFloorArea * building.Parameters.Construction.hcGFloor +
                totalIFloorArea * building.Parameters.Construction.hcIFloor +
                totalIWallArea * building.Parameters.Construction.hcIWall + totalRoofArea * building.Parameters.Construction.hcRoof +
                iMasses.Select(m => m.area * building.Parameters.Construction.hcInternalMass).Sum();

            TotalHeatCapacityDeDuplicatingIntSurfaces = totalWallArea * building.Parameters.Construction.hcWall + totalGFloorArea * building.Parameters.Construction.hcGFloor +
                totalIFloorAreaExOther * building.Parameters.Construction.hcIFloor +
                totalIWallAreaExOther * building.Parameters.Construction.hcIWall + totalRoofArea * building.Parameters.Construction.hcRoof +
                iMasses.Select(m => m.area * building.Parameters.Construction.hcInternalMass).Sum();          
        }
        public void AssociateEnergyResults(Dictionary<string, double[]> resultsDF)
        {
            wallHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").Select(s => s.HeatFlow).Sum();
            gFloorHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").Select(s => s.HeatFlow).Sum();
            windowHeatFlow = Surfaces.Where(w => w.Fenestrations != null)
                .SelectMany(w => w.Fenestrations).Select(s => s.HeatFlow).Sum();
            roofHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Roof).Select(s => s.HeatFlow).Sum();
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
        public void AssociateProbabilisticEnergyResults(Dictionary<string, double[]> resultsDF)
        {
            p_wallHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").
                Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            p_gFloorHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").
                Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            try
            {
                p_windowHeatFlow = Surfaces.Where(w => w.Fenestrations != null).SelectMany(w => w.Fenestrations).
                    Select(s => s.p_HeatFlow).ToList().AddArrayElementWise(); 
            }
            catch { }
            p_roofHeatFlow = Surfaces.Where(w => w.surfaceType == SurfaceType.Roof).
                Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
           
            try
            {
                p_infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && 
                    a.Contains("Zone Infiltration Total Heat Gain Energy"))].SubtractArrayElementWise(
                         resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && 
                         a.Contains("Zone Infiltration Total Heat Loss Energy"))]).ConvertWfromJoule();
            }
            catch 
            {
                p_infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Infiltration"))];
            }
                       
            EPP = new List<EPZone>();
            for (int i = 0; i < resultsDF.First().Value.Length; i++)
            {
                EPP[i].HeatingLoad = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Heating"))][i];
                EPP[i].CoolingLoad = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Cooling"))][i];
                EPP[i].LightsLoad = resultsDF[resultsDF.Keys.First(a => a.Contains(Name.ToUpper()) && a.Contains("Lights"))][i];
            }
        }
        public Zone() { }
        public Zone(double height, string name, int level)
        {
            Name = name;
            Level = level;
            Surfaces = new List<Surface>();
            Height = height;
        }
    }
}
