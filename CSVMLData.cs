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
        static readonly string[] ZoneChr = new string[] { "Area", "Height", "Volume", "Ground Floor", "Roof",
                "Light Heat Gain", "Equipment Heat Gain", "Occupant Load", "Internal Heat Gain",
                "Infiltration", "Air Permeability",
                "External Wall Area", "Ground Floor Area", "Roof Area", "Window Area",
                "Internal Wall Area", "Internal Floor Area",
                "u External Wall", "u Ground Floor", "u Roof", "u Window", "g Window",
                "u Internal Wall", "u Internal Floor",
                "Total Heat Capacity",
                "Opening Time", "Operating Hours",
                "Heating Set Point", "Cooling Set Point", "WWR_North", "WWR_East", "WWR_West", "WWR_South"
        };
        static readonly string[] ZoneChr1 = new string[] { "Area", "Height", "Volume",
                "Internal Heat Gain",
                "Infiltration",              
                "Total Heat Capacity",
                "Operating Hours"
        };
        static readonly string[] BuildingChr = new string[] { "Area", "Height", "Volume", "NFloors",
                "Light Heat Gain", "Equipment Heat Gain", "Occupant Load", "Internal Heat Gain",
                "Infiltration", "Permeability",
                "u External Wall", "u Ground Floor", "u Roof", "u Window", "g Window",
                "u Internal Wall", "u Internal Floor",
                "hc_Slab", "Internal Mass", "Total Heat Capacity",
                "Opening Time", "Operating Hours",
                "Heating Set Point", "Cooling Set Point", new BuildingService().Header(","), new BuildingWWR().Header(",")
        };

        public string 
            WallWindow = string.Join(",", "File", "Zone Name", "Name", "Wall Area", "Window Area", "Orientation", "U Wall", "U Window", "g Value", "Solar Radiation", "Heat Flow"), 
            GFloor = string.Join(",", "File", "Zone Name", "Name", "Area", "U Value", "Heat Capacity", "Heat Flow"), 
            Roof = string.Join(",", "File", "Zone Name", "Name", "Area", "U Value", "Heat Capacity", "Heat Flow"), 
            Infiltration = string.Join(",", "File", "Zone Name", "Name", "Area", "Height", "Volume", "Infiltration", "Air Permeability", "Internal Heat Gain",
                "Total Heat Capacity", "Heat Flow"), 
            
            Zone = string.Join(",", "File", "Name", string.Join(",", ZoneChr),
                        "WallWindow Heat Flow", "GFloor Heat Flow", "Roof Heat Flow", "Infiltration Heat Flow", "Solar Radiation", 
                        EPZone.Header()), 
            
            Building = string.Join(",", "File", string.Join(",", BuildingChr), EPBuilding.Header());

        public List<string> WallWindowData = new List<string>(),
            GFloorData = new List<string>(),
            RoofData = new List<string>(),
            InfiltrationData = new List<string>(),
            ZoneData = new List<string>(), 
            BuildingData = new List<string>();

        public CSVMLData() { }
        public object[] GetSpaceChr(Building building, Zone z)
        {
            ZoneList zList = building.ZoneLists.First(zL => zL.ZoneNames.Contains(z.Name));
            double light = zList.Light.wattsPerArea,
                equipment = zList.ElectricEquipment.wattsPerArea,
                occupancy = zList.Conditions.Occupancy,
                opT = zList.Conditions.StartTime,
                hours = zList.Conditions.OperatingHours,
                spH = zList.Conditions.HeatingSetpoint,
                spC = zList.Conditions.CoolingSetpoint,
                infiltration = zList.ZoneInfiltration.airChangesHour;
                
            BuildingConstruction buiCons = building.Parameters.Construction;
            return new object[] {
                z.Area, z.Height, z.Volume,z.totalGFloorArea >0?1:0, z.totalRoofArea>0?1:0,
                light, equipment, occupancy, light+equipment+125/occupancy,
                infiltration, buiCons.Permeability,
                z.totalWallArea, z.totalGFloorArea, z.totalRoofArea, z.totalWindowArea,
                z.totalIWallArea, z.totalIFloorArea,
                buiCons.UWall, buiCons.UGFloor, buiCons.URoof, buiCons.UWindow, buiCons.GWindow,
                buiCons.UIWall, buiCons.UIFloor,
                z.TotalHeatCapacity,
                opT, hours,
                spH, spC, building.Parameters.WWR.ToString(",")
            };

        }
        public object[] GetBuildingChr(Building building)
        {
            ZoneList zList = building.ZoneLists.First();
            double light = zList.Light.wattsPerArea,
                equipment = zList.ElectricEquipment.wattsPerArea,
                occupancy = zList.Conditions.Occupancy,
                opT = zList.Conditions.StartTime,
                hours = zList.Conditions.OperatingHours,
                spH = zList.Conditions.HeatingSetpoint,
                spC = zList.Conditions.CoolingSetpoint,
                infiltration = zList.ZoneInfiltration.airChangesHour;
            
            BuildingConstruction buiCons = building.Parameters.Construction;
            return new object[] {
                building.zones.Select(z=>z.Area).Sum(), building.Parameters.Geometry.Height, building.zones.Select(z=>z.Volume).Sum(),
                building.Parameters.Geometry.NFloors,
                light, equipment, occupancy, light+equipment+125/occupancy,
                infiltration, buiCons.Permeability,
                buiCons.UWall, buiCons.UGFloor, buiCons.URoof, buiCons.UWindow, buiCons.GWindow,
                buiCons.UIWall, buiCons.UIFloor, buiCons.HCSlab, buiCons.InternalMass,
                building.zones.Select(z=>z.TotalHeatCapacityDeDuplicatingIntSurfaces).Sum(),
                opT, hours,
                spH, spC, building.Parameters.Service.ToString(","), building.Parameters.WWR.ToString(",")
            };

        }
        public CSVMLData(Building building)
        {
            Dictionary<string, IList<string>> CSVData = new Dictionary<string, IList<string>>();
            string idfFile = building.name;
            
            BuildingConstruction buildingConstruction = building.Parameters.Construction;
            foreach (Zone z in building.zones)
            {
                object[] spaChr = GetSpaceChr(building, z);
                foreach (Surface s in z.Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors"))
                {
                    WallWindowData.Add(string.Join(",", idfFile, z.Name, s.Name, s.GrossArea*(1-s.WWR), s.GrossArea * s.WWR,
                    s.Orientation, buildingConstruction.UWall, buildingConstruction.UWindow,
                    buildingConstruction.GWindow, s.SolarRadiation,
                    s.HeatFlow));
                }
                z.Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").ToList().ForEach(
                    s => GFloorData.Add(string.Join(",", idfFile, z.Name, s.Name, s.Area, buildingConstruction.UGFloor, buildingConstruction.hcGFloor,
                    s.HeatFlow)));
                z.Surfaces.Where(w => w.surfaceType == SurfaceType.Roof).ToList().ForEach(
                    s => RoofData.Add(string.Join(",", idfFile, z.Name, s.Name, s.Area, buildingConstruction.URoof, buildingConstruction.hcRoof,
                    s.HeatFlow)));
                InfiltrationData.Add(string.Join(",", idfFile, z.Name, z.Name, z.Area, z.Height, z.Volume, 
                    buildingConstruction.Infiltration, buildingConstruction.Permeability, spaChr[8], spaChr[24], z.infiltrationFlow));
                string zString = string.Join(",", idfFile, z.Name, string.Join(",", spaChr),
                    z.wallWindowHeatFlow, z.gFloorHeatFlow, z.roofHeatFlow, z.infiltrationFlow, z.SolarRadiation,
                    z.EP.ToString(""));
                ZoneData.Add(zString);
            }
            string bString = string.Join(",", idfFile,
                            string.Join(",", GetBuildingChr(building)), building.EP.ToString(""), building.EP.ThermalEnergy);
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
