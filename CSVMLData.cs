using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2013.Drawing.Chart;
using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IDFObjects
{
    [Serializable]
    public class CSVMLData
    {
        static readonly ProbabilisticBuildingDesignParameters parH = new ProbabilisticBuildingDesignParameters() { zConditions = new List<ProbabilisticZoneConditions>() { new ProbabilisticZoneConditions() } };
        static readonly string[] ZoneChr = new string[] { "Area", parH.pGeometry.Height.Label, "Volume",
                "External Wall Area", "Ground Floor Area", "Roof Area", "Window Area", "Internal Wall Area", "Internal Floor Area","Total Heat Capacity",
                parH.pConstruction.Header(","), parH.pWWR.Header(","), parH.zConditions[0].Header(","),     
        };

        static readonly string[] BuildingChr = new string[] { parH.pGeometry.FloorArea.Label, parH.pGeometry.Height.Label, "Volume", 
                parH.pGeometry.NFloors.Label,
                "External Wall Area", "Ground Floor Area", "Roof Area", "Window Area","Total Heat Capacity",
                parH.pConstruction.Header(","), parH.pWWR.Header(","), parH.zConditions[0].Header(","), parH.pService.Header(",")
        };

        public string
            Wall = string.Join(",", "File", "Zone Name", "Name", "Area", "Orientation", parH.pConstruction.UWall.Label, "Solar Radiation", "Heat Flow"),
            Window = string.Join(",", "File", "Zone Name", "Name", "Area", "Orientation", parH.pConstruction.UWindow.Label, parH.pConstruction.GWindow.Label, "Solar Radiation", "Heat Flow"),

            WallWindow = string.Join(",", "File", "Zone Name", "Name", "Wall Area", "Window Area", "Orientation",parH.pConstruction.UWall.Label, parH.pConstruction.UWindow.Label, parH.pConstruction.GWindow.Label, "Solar Radiation", "Heat Flow"), 
            GFloor = string.Join(",", "File", "Zone Name", "Name", "Area", parH.pConstruction.UGFloor.Label, "Heat Capacity", "Heat Flow"), 
            Roof = string.Join(",", "File", "Zone Name", "Name", "Area", parH.pConstruction.URoof.Label, "Heat Capacity", "Heat Flow"), 
            Infiltration = string.Join(",", "File", "Zone Name", "Name", "Area", "Height", "Volume", 
                parH.pConstruction.Infiltration.Label, parH.pConstruction.Permeability.Label, parH.zConditions[0].LHG.Label, parH.zConditions[0].EHG.Label, parH.zConditions[0].Occupancy.Label,
                "Total Heat Capacity", "Heat Flow"), 
            
            Zone = string.Join(",", "File", "Name", string.Join(",", ZoneChr),
                        "Wall Heat Flow","Window Heat Flow","WallWindow Heat Flow", "GFloor Heat Flow", "Roof Heat Flow", "Infiltration Heat Flow", "Solar Radiation", 
                        EPZone.Header()), 
            
            Building = string.Join(",", "File", string.Join(",", BuildingChr), EPBuilding.Header());

        public List<string> WallData=new List<string>(), WindowData=new List<string>(), 
            WallWindowData = new List<string>(),
            GFloorData = new List<string>(),
            RoofData = new List<string>(),
            InfiltrationData = new List<string>(),
            ZoneData = new List<string>(), 
            BuildingData = new List<string>();

        public CSVMLData() { }
        public object[] GetSpaceChr(Building building, Zone z)
        {
            ZoneList zList = building.ZoneLists.First(zL => zL.ZoneNames.Contains(z.Name));
            return new object[] {
                z.Area, z.Height, z.Volume,
                z.totalWallArea, z.totalGFloorArea, z.totalRoofArea, z.totalWindowArea,z.totalIWallArea, z.totalIFloorArea,z.TotalHeatCapacity,
                building.Parameters.Construction.ToString(","), building.Parameters.WWR.ToString(","), zList.Conditions.ToString(","),
            };

        }
        public object[] GetBuildingChr(Building building)
        {
            ZoneList zList = building.ZoneLists.First();
            BuildingConstruction buiCons = building.Parameters.Construction;
            return new object[] {
                building.zones.Select(z=>z.Area).Sum(), building.Parameters.Geometry.Height, building.zones.Select(z=>z.Volume).Sum(),
                building.Parameters.Geometry.NFloors,
                building.zones.Select(z=>z.totalWallArea).Sum(),
                building.zones.Select(z=>z.totalGFloorArea).Sum(),
                building.zones.Select(z=>z.totalRoofArea).Sum(),
                building.zones.Select(z=>z.totalWindowArea).Sum(),
                building.zones.Select(z=>z.TotalHeatCapacityDeDuplicatingIntSurfaces).Sum(),
                building.Parameters.Construction.ToString(","), building.Parameters.WWR.ToString(","), zList.Conditions.ToString(","), building.Parameters.Service.ToString(",")
            };
        }
        public CSVMLData(Building building)
        {
            Dictionary<string, IList<string>> CSVData = new Dictionary<string, IList<string>>();
            string idfFile = building.name;
            
            BuildingConstruction buildingConstruction = building.Parameters.Construction;
            foreach (Zone z in building.zones)
            {
                ZoneList zList = building.ZoneLists.First(zL => zL.ZoneNames.Contains(z.Name));
                object[] spaChr = GetSpaceChr(building, z);
                foreach (Surface s in z.Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors"))
                {
                    WallWindowData.Add(string.Join(",", idfFile, z.Name, s.Name, s.Area, s.GrossArea * s.WWR,
                        s.Orientation, buildingConstruction.UWall, buildingConstruction.UWindow,
                        buildingConstruction.GWindow, s.SolarRadiation,
                        s.HeatFlow));

                    double wallHeatFlow = s.HeatFlow, solarRadiation = s.SolarRadiation;
                    if (s.Fenestrations != null && s.Fenestrations.Count > 0)
                    {
                        foreach (Fenestration f in s.Fenestrations)
                        {
                            WindowData.Add(string.Join(",", idfFile, z.Name, f.Name, f.Area,
                                  f.Orientation, buildingConstruction.UWindow, buildingConstruction.GWindow, f.SolarRadiation,
                                  f.HeatFlow));
                            wallHeatFlow -= f.HeatFlow;
                            solarRadiation -= f.SolarRadiation;
                        }
                    }
                    WallData.Add(string.Join(",", idfFile, z.Name, s.Name, s.Area,
                        s.Orientation, buildingConstruction.UWall, solarRadiation, wallHeatFlow));
                }
                z.Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").ToList().ForEach(
                    s => GFloorData.Add(string.Join(",", idfFile, z.Name, s.Name, s.Area, buildingConstruction.UGFloor, buildingConstruction.hcGFloor,
                    s.HeatFlow)));
                z.Surfaces.Where(w => w.surfaceType == SurfaceType.Roof).ToList().ForEach(
                    s => RoofData.Add(string.Join(",", idfFile, z.Name, s.Name, s.Area, buildingConstruction.URoof, buildingConstruction.hcRoof,
                    s.HeatFlow)));
                InfiltrationData.Add(string.Join(",", idfFile, z.Name, z.Name, z.Area, z.Height, z.Volume, 
                    buildingConstruction.Infiltration, buildingConstruction.Permeability, zList.Conditions.LHG, zList.Conditions.EHG, zList.Conditions.Occupancy, z.TotalHeatCapacity, z.infiltrationFlow));
                string zString = string.Join(",", idfFile, z.Name, string.Join(",", spaChr),
                    z.WallHeatFlow, z.WindowHeatFlow, z.wallWindowHeatFlow, z.gFloorHeatFlow, z.roofHeatFlow, z.infiltrationFlow, z.SolarRadiation,
                    z.EP.ToString("")); 
                ZoneData.Add(zString);
            }
            string bString = string.Join(",", idfFile,
                            string.Join(",", GetBuildingChr(building)), building.EP.ToString(""));
            BuildingData.Add(bString);   
        }
        public CSVMLData(Building building, string time)
        {
            Dictionary<string, IList<string>> CSVData = new Dictionary<string, IList<string>>();
            string idfFile = building.name;
            //int co = time == "monthly" ? 12 : 8760;


            BuildingConstruction buildingConstruction = building.Parameters.Construction;
            foreach (Zone z in building.zones)
            {
                object[] spaChr = GetSpaceChr(building, z);
                foreach (Surface s in z.Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors"))
                {
                    WallWindowData.Add(string.Join(",", idfFile, z.Name, s.Name, s.GrossArea * (1 - s.WWR), s.GrossArea * s.WWR,
                    s.Orientation, buildingConstruction.UWall, buildingConstruction.UWindow,
                    buildingConstruction.GWindow, s.h_SolarRadiation.ToCSVString(),
                    s.h_HeatFlow.ToCSVString()));
                }
                z.Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").ToList().ForEach(
                    s => GFloorData.Add(string.Join(",", idfFile, z.Name, s.Name, s.Area, buildingConstruction.UGFloor, buildingConstruction.hcGFloor,
                    s.h_HeatFlow.ToCSVString())));
                z.Surfaces.Where(w => w.surfaceType == SurfaceType.Roof).ToList().ForEach(
                    s => RoofData.Add(string.Join(",", idfFile, z.Name, s.Name, s.Area, buildingConstruction.URoof, buildingConstruction.hcRoof,
                    s.h_HeatFlow.ToCSVString())));
                InfiltrationData.Add(string.Join(",", idfFile, z.Name, z.Name, z.Area, z.Height, z.Volume,
                    buildingConstruction.Infiltration, buildingConstruction.Permeability, spaChr[8], spaChr[24], z.h_infiltrationFlow.ToCSVString()));

                ZoneData.Add(string.Join(",", idfFile, z.Name, string.Join(",", spaChr),
                    z.h_wallwindowHeatFlow.ToCSVString(), z.h_gFloorHeatFlow.ToCSVString(), z.h_roofHeatFlow.ToCSVString(), z.h_infiltrationFlow.ToCSVString(), z.h_SolarRadiation.ToCSVString(),
                    z.EP.ToString(time)));
            }
            BuildingData.Add(string.Join(",", idfFile,
                            string.Join(",", GetBuildingChr(building)), building.EP.ToString(time)));
        }
    }
}
