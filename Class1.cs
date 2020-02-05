using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace IDFFile

{
    public enum SurfaceType { Floor, Ceiling, Wall, Roof };
    public enum HVACSystem { FCU, BaseboardHeating, VAV, IdealLoad };
    public enum Direction { North, East, South, West };
    public static class Utility
    {
        public static string GetHeader(string comp)
        {
            string info = "";
            string[] spaceChar = new string[] { "Zone Area", "Zone Height", "Zone Volume",
                "Light Heat Gain", "Equipment Heat Gain", "Infiltration",
                "Total Internal Heat Gain", "Total Infiltration", "Operating Hours", "Total Heat Capacity", "Solar Radiation",
                "Surface Area U", "Ex-Surface Area U", "G-Surface Area U", "I-Surface Area U",
                "Wall Area X U", "GFloor Area X U", "Roof Area X U", "Window Area X U", "IFloor Area X U", "IWall Area X U", "Window Area X g"};
            string[] adSpaceChar = spaceChar.Select(p => "Adjacent_" + p).ToArray();
            string[] heatFlow = new string[] { "Heat Flow" };
            switch (comp)
            {
                case "Wall":
                    info = string.Join(",", "File", "Zone Name", "Name", "Area", "Orientation", "WWR", "U Value", "Heat Capacity", "Radiation",
                                    string.Join(",", spaceChar), string.Join(",", heatFlow));
                    break;
                case "Window":
                    info = string.Join(",", "File", "Zone Name", "Name", "Area", "Orientation", "U Value", "g Value", "Radiation",
                                        string.Join(",", spaceChar), string.Join(",", heatFlow));
                    break;
                case "GFloor":
                    info = string.Join(",", "File", "Zone Name", "Name", "Area", "U Value", "Heat Capacity",
                        string.Join(",", spaceChar), string.Join(",", heatFlow));
                    break;
                case "Roof":
                    info = string.Join(",", "File", "Zone Name", "Name", "Area", "U Value", "Heat Capacity", "Radiation",
                        string.Join(",", spaceChar), string.Join(",", heatFlow));
                    break;
                case "IFloor":
                case "IWall":
                    info = string.Join(",", "File", "Zone Name", "Name", "Adjacent Zone", "Area", "U Value",
                        string.Join(",", spaceChar), string.Join(",", adSpaceChar), string.Join(",", heatFlow));
                    break;
                case "Infiltration":
                    info = string.Join(",", "File", "Zone Name", "Name", string.Join(",", spaceChar), string.Join(",", heatFlow));
                    break;
                case "Zone":
                    info = string.Join(",", "File", "Name", string.Join(",", spaceChar),
                        "Wall Heat Flow", "Window Heat Flow", "GFloor Heat Flow", "Roof Heat Flow", "IFloor Heat Flow", "IWall Heat Flow",
                         "Infiltration Heat Flows", "Building Element Heat Flows", "Total Heat Flows",
                        "Heating Energy", "Cooling Energy", "Lighting Energy");
                    break;
                case "Building":
                    info = string.Join(",", "File", "Total Floor Area", "Floor Height", "Total Volume", 
                        "Boiler Efficiecny", "ChillerCOP", "Lighting Energy", "Heating Energy", "Cooling Energy",
                        "Bolier Electric Energy", "Chiller Electric Energy", "Heating & Cooling Energy", "Operational Energy");                
                    break;
            }
            return info;
        }
        public static double[] GetSpaceChr(Zone z)
        {
            double light = 0, equipment = 0, infiltration = 0;
            //"Zone Area", "Zone Height", "Zone Volume",
            //    "Light & Equipment Heat Gain", "Infiltration", "Operating Hours", "Heat Capacity", "Solar Radiation",
            //    "Surface Area U", "Ex-Surface Area U", "G-Surface Area U", "I-Surface Area U",
            //    "Wall Area X U", "GFloor Area X U", "Roof Area X U", "Window Area X U", "IFloor Area X U", "IWall Area X U", "Window Area X g"

            
            try
            {
                ZoneList zList = z.building.zoneLists.First(
                zL => zL.listZones.Select(zone=>zone.name).Contains(z.name));               
                light = zList.Light.wattsPerArea; equipment = zList.ElectricEquipment.wattsPerArea;
                infiltration = zList.ZoneInfiltration.airChangesHour;
            }
            catch
            {
                light = z.lights.wattsPerArea; equipment = z.equipment.wattsPerArea;
                infiltration = z.infiltration.airChangesHour;
            }

            return new double[] {
                z.area, z.height, z.volume, light, equipment, infiltration,
                z.area*light + z.area*equipment, z.volume*infiltration, 
                z.building.buildingOperation.operatingHours, z.TotalHeatCapacity, z.SolarRadiation,
                z.SurfAreaU, z.ExSurfAreaU, z.GSurfAreaU, z.ISurfAreaU,
                z.wallAreaU, z.gFloorAreaU, z.roofAreaU, z.windowAreaU, z.iFloorAreaU, z.iWallAreaU, z.windowAreaG};
        }
        public static void GetMLCSVLines(Building bui, IList<string> wallString, IList<string> windowString, 
            IList<string> gFloorString, IList<string> roofString, IList<string> infiltrationString, 
            IList<string> zoneString, IList<string> buildingString)
        {
            BuildingConstruction cons = bui.buildingConstruction;
            string idfFile = bui.name;
            foreach (Zone z in bui.zones)
            {
                double[] spaChr = GetSpaceChr(z);

                z.surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").ToList().ForEach(
                    s => wallString.Add(string.Join(",", idfFile, z.name, s.name, s.area, s.orientation, s.wWR, cons.uWall, cons.hcWall, s.SolarRadiation,
                    string.Join(",", spaChr), s.HeatFlow)));
                z.surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                    .Where(w => w.fenestrations.Count != 0).SelectMany(w => w.fenestrations).ToList().
                    ForEach(s => windowString.Add(string.Join(",", idfFile, z.name, s.name, s.area, s.face.orientation, 
                    cons.uWindow, cons.gWindow, s.SolarRadiation,
                    string.Join(",", spaChr), s.HeatFlow)));
                z.surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").ToList().ForEach(
                    s => gFloorString.Add(string.Join(",", idfFile, z.name, s.name, s.area, cons.uGFloor, cons.hcGFloor,
                    string.Join(",", spaChr), s.HeatFlow)));
                z.surfaces.Where(w => w.surfaceType == SurfaceType.Roof).ToList().ForEach(
                    s => roofString.Add(string.Join(",", idfFile, z.name, s.name, s.area, cons.uRoof, cons.hcRoof, s.SolarRadiation,
                    string.Join(",", spaChr), s.HeatFlow)));

                //z.iWalls.ForEach(s => iWallString.Add(string.Join(",", idfFile, s.name, s.area, sample.construction.uIWall,
                //    string.Join(",", spaChr), s.HeatFlow)));
                //z.izWalls.ForEach(s => iWallString.Add(string.Join(",", idfFile, z.name, s.area, uIWall,
                //    string.Join(",", spaChr), -s.HeatFlow)));
                //z.iFloors.ForEach(s => iFloorString.Add(string.Join(",", idfFile, s.name, s.OutsideObject, s.area, sample.construction.uIWall,
                //    string.Join(",", spaChr), string.Join(",", GetSpaceChr(z.building.zones.First(zo=>zo.name == s.OutsideObject))), s.HeatFlow)));
                //z.izFloors.ForEach(s => iFloorString.Add(string.Join(",", idfFile, z.name, s.area, uIWall,
                //    string.Join(",", spaChr), -s.HeatFlow)));
                //"File", "Name", string.Join(",", spaceChar), string.Join(",", heatFlow)
                infiltrationString.Add(string.Join(",", idfFile, z.name, z.name, string.Join(",", spaChr), z.infiltrationFlow));
                //"File", "Name", string.Join(",", spaceChar),
                //"Wall Heat Flow", "Window Heat Flow", "GFloor Heat Flow", "Roof Heat Flow", "IFloor Heat Flow", "IWall Heat Flow", 
                //    "Heating Energy", "Cooling Energy", "Lighting Energy", "Electric Equipment Energy");
                zoneString.Add(string.Join(",", idfFile, z.name, string.Join(",", spaChr),
                    z.wallHeatFlow, z.windowHeatFlow, z.gFloorHeatFlow, z.roofHeatFlow, z.iFloorHeatFlow, z.iWallHeatFlow, z.infiltrationFlow,
                    z.TotalHeatFlows - z.infiltrationFlow, z.TotalHeatFlows,
                    z.HeatingEnergy, z.CoolingEnergy, z.LightingEnergy));
            }
            // "File", "Total Floor Area", "Floor Height", "Total Volume", "Boiler Efficiecny", "ChillerCOP",
            //"Lighting Energy", "Heating Energy", "Cooling Energy",
            //        "Bolier Electric Energy", "Chiller Electric Energy", "Thermal energy", "Operational Energy"
            buildingString.Add(string.Join(",", idfFile, bui.zones.Select(z => z.area).Sum(), bui.FloorHeight, bui.zones.Select(z => z.volume).Sum(),
                bui.buildingOperation.boilerEfficiency, bui.buildingOperation.chillerCOP,
                bui.LightingEnergy,
                bui.ZoneHeatingEnergy, bui.ZoneCoolingEnergy,
                bui.BoilerEnergy, bui.ChillerEnergy, bui.ThermalEnergy, bui.OperationalEnergy));

//            buildingOutput.Add(string.Join(",", idfFile,bui.TotalArea, bui.TotalVolume,
//                            bui.zones.Select(z => z.totalWallArea).Sum(),
//                            bui.zones.Select(z => z.totalWindowArea).Sum(),
//                            bui.zones.Select(z => z.totalRoofArea).Sum(),
//                            bui.zones.Select(z => z.windowAreaG).Sum(),
//-                           bui.zones.Select(z => z.iFloorAreaU).Sum(),
//                            bui.zones.Select(z => z.iWallAreaU).Sum(),bui.buildingConstruction.uWall,bui.buildingConstruction.gWindow, bui.buildingConstruction.uRoof, bui.buildingConstruction.infiltration,
//                            bui.buildingConstruction.uWindow, bui.buildingOperation.chillerCOP, bui.buildingOperation.operatingHours, bui.buildingOperation.lightHeatGain, bui.buildingOperation.equipmentHeatGain, bui.buildingOperation.boilerEfficiency)); ;

        }
        public static double FtToM(double value)
        {
            return (Math.Round(value * 0.3048, 4));
        }
        public static double MToFt(double value)
        {
            return (Math.Round(value * 3.2808399, 4));
        }
        public static double SqFtToSqM(double value)
        {
            return (Math.Round(value * 0.092903, 4));
        }
        public static double SqMToSqFt(double value)
        {
            return (Math.Round(value / 0.092903, 4));
        }
        public static string GetHeaderFeattureEngineering(string component)
        {
            if (component == "Building")
            {
                return string.Join(",", "File", "Total Floor Area", "Total Volume", "Total Wall Area", "Total Window Area", "Total Roof Area", "Total Ground Floor Area", "Total Internal Floor Area",
                    "uWall", "uWindow", "gWindow", "uRoof", "uGFloor", "uIFloor", "Infiltration", "Total heat capacity", "Operating Hours", "Light Heat Gain", "Equipment Heat Gain", 
                    "Boiler Efficiency", "ChillerCOP", "Thermal Energy", "Operational Energy");
            }
            else
            {
                return "";
            }
        }
        public static void GetMLCSVLinesFeatureEngineering(Building bui, IList<string> buildingString)
        {
            string idfFile = bui.name;
            buildingString.Add(string.Join(",", idfFile, bui.TotalArea, bui.TotalVolume,
                            bui.zones.Select(z => z.totalWallArea).Sum(),
                            bui.zones.Select(z => z.totalWindowArea).Sum(),
                            bui.zones.Select(z => z.totalRoofArea).Sum(),
                            bui.zones.Select(z=>z.totalGFloorArea).Sum(),
                            bui.zones.Select(z => z.totalIFloorArea).Sum(),
                            bui.buildingConstruction.uWall, bui.buildingConstruction.uWindow, bui.buildingConstruction.gWindow, bui.buildingConstruction.uRoof, bui.buildingConstruction.uGFloor,
                            bui.buildingConstruction.uIFloor, bui.buildingConstruction.infiltration, 
                            bui.zones.Select(z=>z.TotalHeatCapacity).Sum(),bui.buildingOperation.operatingHours,bui.buildingOperation.lightHeatGain, bui.buildingOperation.equipmentHeatGain, 
                       
                            bui.buildingOperation.boilerEfficiency, bui.buildingOperation.chillerCOP, bui.ThermalEnergy, bui.OperationalEnergy)); ;

        }
        public static Dictionary<string, double[]> ConvertToDataframe(IEnumerable<string> csvFile)
        {
            IEnumerable<string> header = csvFile.ElementAt(0).Split(',').Skip(1);
            Dictionary<string, double[]> data = new Dictionary<string, double[]>();

            for (int i = 0; i < header.Count(); i++)
            {
                data.Add(header.ElementAt(i), new double[csvFile.Count() - 1]);
            }

            int r = 0;
            foreach (string s in csvFile.Skip(1))
            {
                string[] row = s.Split(',').Skip(1).ToArray();
                for (int c = 0; c < header.Count(); c++)
                {
                    data.ElementAt(c).Value[r] = double.Parse(row[c]);
                }
                r++;
            }
            return data;
        }       
        public static List<List<double>> ReadSampleFile(string parFile, out List<string> header)
        {
            List<string> rawFile = File.ReadAllLines(parFile).ToList();
            int nSamples = rawFile.Count - 1;
            List<List<double>> returnData = new List<List<double>>();

            header = rawFile[0].Split(',').ToList();

            for (int i = 1; i < nSamples + 1; i++)
            {
                returnData.Add(rawFile[i].Split(',').Select(d => double.Parse(d)).ToList());
            }
            return returnData;
        }
        public static List<BuildingDesignParameters> ReadBuildingDesignParameters(string dataFile)
        {
            List<string> parameters = new List<string>();
            List<List<double>> samples = ReadSampleFile(dataFile, out parameters);

            int iLen = -1, iWid = -1, iArea = -1, iNFloors = -1;
            try
            {
                iLen = parameters.FindIndex(s => s.Contains("Length"));
                iWid = parameters.FindIndex(s => s.Contains("Width"));
            }
            catch { }
            try
            {
                iNFloors = parameters.FindIndex(s => s.Contains("Floors"));
            }
            catch { }
            try
            {
                iArea = parameters.FindIndex(s => s.Contains("Area"));
            }
            catch { }
            int iHeight = parameters.FindIndex(s => s.Contains("Height"));
            int iOrientation = parameters.FindIndex(s => s.Contains("Orientation"));
            int iUWall = parameters.FindIndex(s => s.Contains("u_Wall"));
            int iUGFloor = parameters.FindIndex(s => s.Contains("u_GFloor"));
            int iURoof = parameters.FindIndex(s => s.Contains("u_Roof"));
            int iUWindow = parameters.FindIndex(s => s.Contains("u_Window"));
            int igWindow = parameters.FindIndex(s => s.Contains("g_Window"));
            int iWWRN = parameters.FindIndex(s => s.Contains("WWR_N"));
            int iWWRE = parameters.FindIndex(s => s.Contains("WWR_E"));
            int iWWRW = parameters.FindIndex(s => s.Contains("WWR_W"));
            int iWWRS = parameters.FindIndex(s => s.Contains("WWR_S"));
            int iInfiltration = parameters.FindIndex(s => s.Contains("Infiltration"));
            int iOperatingHours = parameters.FindIndex(s => s.Contains("Operating Hours"));
            int ibEff = parameters.FindIndex(s => s.Contains("Boiler Efficiency"));
            int iCCOP = parameters.FindIndex(s => s.Contains("Chiller COP"));

            int irLenA = -1, irWidA = -1, iBDepth = -1, iHCIFloor = -1, iLEHG = -1, iLHG = -1, iEHG = -1, iUIWall = -1, iUIFloor = -1;
            try { irLenA = parameters.FindIndex(s => s.Contains("rLenA")); irWidA = parameters.FindIndex(s => s.Contains("rWidA")); } catch { }
            try { iBDepth = parameters.FindIndex(s => s.Contains("Basement Depth")); } catch { }
            try { iHCIFloor = parameters.FindIndex(s => s.Contains("hc_Slab")); } catch { }
            try { iLEHG = parameters.FindIndex(s => s.Contains("Light & Equipment Heat Gain")); }
            catch { }
            try
            {
                iLHG = parameters.FindIndex(s => s.Contains("Light Heat Gain"));
                iEHG = parameters.FindIndex(s => s.Contains("Equipment Heat Gain"));
            }
            catch { }
            try
            {
                iUIWall = parameters.FindIndex(s => s.Contains("u_IWall"));
                iUIFloor = parameters.FindIndex(s => s.Contains("u_IFloor"));
            }
            catch { }

            List<BuildingDesignParameters> values = new List<BuildingDesignParameters>();
            foreach (List<double> sample in samples)
            {
                double hcSlab, lhg, ehg, uIWall, uIFloor;
                BuildingDesignParameters value = new BuildingDesignParameters();

                if (irLenA != -1) { value.rLenA = sample[irLenA]; value.rWidA = sample[irWidA]; } else { value.rLenA = 0.5; value.rWidA = 0.5; }
                if (iBDepth != -1) { value.BasementDepth = sample[iBDepth]; }
                if (iHCIFloor != -1) { hcSlab = sample[iHCIFloor]; } else { hcSlab = 1050; }
                if (iLHG != -1) { lhg = sample[iLHG]; ehg = sample[iEHG]; } else { lhg = sample[iLEHG] * 0.5; ehg = sample[iLEHG] * 0.5; }
                if (iUIWall != -1) { uIWall = sample[iUIWall]; uIFloor = sample[iUIFloor]; } else { uIWall = 0.25; uIFloor = 0.25; }

                if (iNFloors != -1) { value.NFloors = (int) sample[iNFloors]; }
                if (iLen != -1) { value.Length = sample[iLen]; value.Width = sample[iWid]; }
                else { value.FloorArea = sample[iArea]; }
                
                value.Height = sample[iHeight];
                value.Orientation = sample[iOrientation] * Math.PI / 180;
                value.construction = new BuildingConstruction()
                {
                    uWall = sample[iUWall],
                    uGFloor = sample[iUGFloor],
                    uRoof = sample[iURoof],
                    uWindow = sample[iUWindow],
                    gWindow = sample[igWindow],
                    infiltration = sample[iInfiltration],

                    uIFloor = uIFloor,
                    uIWall = uIWall,
                    hcSlab = hcSlab
                };
                value.wwr = new WWR() { north = sample[iWWRN], east = sample[iWWRE], west = sample[iWWRW], south = sample[iWWRS] };
                value.operation = new BuildingOperation()
                {
                    operatingHours = sample[iOperatingHours],
                    boilerEfficiency = sample[ibEff],
                    chillerCOP = sample[iCCOP],
                    equipmentHeatGain = ehg,
                    lightHeatGain = lhg
                };
                values.Add(value);
            }
            return values;
        }
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }
        public static double ConvertKWhfromJoule(this double d) { return d * 2.7778E-7; }
        public static double[] ConvertKWhfromJoule(this double[] dArray) { return dArray.Select(d => d.ConvertKWhfromJoule()).ToArray(); }
        public static double[] FillZeroes(this double[] Array, int length)
        {
            int count = Array.Count();
            IEnumerable<double> newList = Array;

            for (int i = count; i < length; i++) { newList = newList.Append(0); }
            return newList.ToArray();
        }
        public static double[] AddArrayElementWise(this List<double[]> AllArrays)
        {
            AllArrays = AllArrays.Where(a => a != null).ToList();
            List<int> counts = AllArrays.Select(a => a.Count()).ToList();
            if (counts.Count == 0) { return new double[] { 0 }; }
            else
            {
                int n = counts.Max();

                AllArrays = AllArrays.Select(a => a.FillZeroes(n)).ToList();

                double[] array = new double[n];

                for (int i = 0; i < n; i++)
                {
                    array[i] = AllArrays.Select(a => a[i]).Sum();
                }
                return array;
            }

        }
        public static double[] SubtractArrayElementWise(this double[] FirstArray, double[] SecondArray)
        {
            List<int> counts = new List<int>() { FirstArray.Count(), SecondArray.Count() };
            int n = counts.Max();

            FirstArray = FirstArray.FillZeroes(n); SecondArray = SecondArray.FillZeroes(n);

            double[] array = new double[n];
            for (int i = 0; i < n; i++)
            {
                array[i] = FirstArray[i] - SecondArray[i];
            }
            return array;
        }
        public static int[] hourToHHMM(double hours)
        {
            double h = hours / 2;
            double h1 = 13 - h;
            double h2 = 13 + h;

            int hour1 = int.Parse(Math.Truncate(h1).ToString());
            int min1 = int.Parse((Math.Round(Math.Round((h1 - hour1) * 6)) * 10).ToString());
            int hour2 = int.Parse(Math.Truncate(h2).ToString());
            int min2 = int.Parse((Math.Round(Math.Round((h2 - hour2) * 6)) * 10).ToString());

            if (min1 == 60)
            {
                hour1++;
                min1 = 0;
            }
            if (min2 == 60)
            {
                hour2++;
                min2 = 0;
            }
            return (new int[4] { hour1, min1, hour2, min2 });
        }
        public static List<string> ReplaceLastComma(this List<string> info)
        {
            string lastLine = info.Last();
            string[] splitLine = lastLine.Split(',');

            string joinedLine = string.Join(",", splitLine.Take(splitLine.Count() - 1));
            joinedLine = joinedLine + ";" + splitLine.Last();
            info[info.Count - 1] = joinedLine;
            return info;
        }
        public static XYZ Transform(this XYZ xyz, double angle)
        {
            double x1 = xyz.X * Math.Cos(angle) - xyz.Y * Math.Sin(angle);
            double y1 = xyz.X * Math.Sin(angle) + xyz.Y * Math.Cos(angle);
            return new XYZ(x1, y1, xyz.Z);
        }
        public static XYZ Copy(this XYZ xyz)
        {
            return new XYZ(xyz.X, xyz.Y, xyz.Z);
        }
        public static string IDFLineFormatter(object attribute, string definition)
        {
            //Console.WriteLine(attribute.ToString() + ",\t\t\t\t\t\t ! - " + definition);
            //Console.ReadKey();
            if (attribute != null) { return (attribute.ToString() + ",\t\t\t\t\t\t ! - " + definition); }
            else { return (",\t\t\t\t\t\t ! - " + definition); }
        }
        public static string IDFLastLineFormatter(object attribute, string definition)
        {
            return (attribute.ToString() + ";\t\t\t\t\t\t ! - " + definition);
        }
        public static List<SizingPeriodDesignDay> CreateDesignDays(string place)
        {
            switch (place)
            {
                case "MUNICH_DEU":
                default:
                    SizingPeriodDesignDay winterday = new SizingPeriodDesignDay("MUNICH Ann Htg 99.6% Condns DB", 2, 21, "WinterDesignDay", -12.8, 0.0, -13.9, 0.0, 95900.0, 1.0, 130.0, "No", "No", "No", "AshraeClearSky", 0.0);
                    SizingPeriodDesignDay summerday = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns Enth=>MDB", 7, 21, "SummerDesignDay", 31.5, 10.9, 17.8, 0.0, 95300.0, 1.5, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);
                    SizingPeriodDesignDay summerday1 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 6)", 6, 21, "SummerDesignDay", 29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);
                    SizingPeriodDesignDay summerday2 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 7)", 7, 21, "SummerDesignDay", 29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);
                    SizingPeriodDesignDay summerday3 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 8)", 8, 21, "SummerDesignDay", 29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);
                    SizingPeriodDesignDay summerday4 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 9)", 9, 21, "SummerDesignDay", 29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);
                    return new List<SizingPeriodDesignDay>() { winterday, summerday, summerday1, summerday2, summerday3, summerday4 };
            }
        }
    }
    [Serializable]
    public class IDFFile
    {
        public string name = "IDFFile_0";
        public string WeatherLocation = "MUNICH_DEU";

        //IDF Objects as appear in Energy Plus File
        public Version version = new Version();
        public SimulationControl sControl = new SimulationControl();
        public Timestep tStep = new Timestep(6);
        public ConvergenceLimits cLimits = new ConvergenceLimits();
        public SiteLocation sLocation = new SiteLocation("MUNICH_DEU");
        public List<SizingPeriodDesignDay> SDesignDay = Utility.CreateDesignDays("MUNICH_DEU");
        public RunPeriod rPeriod = new RunPeriod();
        public SiteGroundTemperature gTemperature = new SiteGroundTemperature("MUNICH_DEU");
        public GlobalGeometryRules geomRules = new GlobalGeometryRules();

        //Building - contain schedules, material, constructions, zones, zoneLists, 
        public Building building = new Building();

        public Output output;

        public IDFFile() { }
        public IDFFile deepCopy(string name)
        {
            if (!typeof(IDFFile).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(this, null))
            {
                return this;
            }
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                IDFFile other = (IDFFile)formatter.Deserialize(stream);
                other.name = name;
                return other;
            }
        }

        public List<string> WriteFile()
        {
            //Version, Simulation Control, Building, TimeStep, ConvergenceLimits, Site:Location, SizingPeriod, RunPeriod, GroundTemperature
            //GlobalGeometryRules
            //ScheduleLimits, Schedule
            //Material, WindowMaterial, Construction
            //Zone, ZoneList, BuildingSurface
            //People, Light, ElectricEquipment, Infiltration, Ventillation
            //Thermostat, HVACZone, HWLoop, CWLoop, Boiler, Chiller, Tower
            List<string> info = new List<string>();
            info.AddRange(version.WriteInfo());
            info.AddRange(sControl.WriteInfo());
            info.AddRange(building.WriteInfo());
            info.AddRange(tStep.WriteInfo());
            info.AddRange(cLimits.WriteInfo());
            info.AddRange(sLocation.WriteInfo());
            SDesignDay.ForEach(s => info.AddRange(s.WriteInfo()));
            info.AddRange(rPeriod.WriteInfo());
            info.AddRange(gTemperature.WriteInfo());
            info.AddRange(geomRules.WriteInfo());

            building.schedulelimits.ForEach(s => info.AddRange(s.WriteInfo()));
            building.schedulescomp.ForEach(s => info.AddRange(s.WriteInfo()));

            info.AddRange(writeMaterial());
            info.AddRange(writeWindowMaterial());
            info.AddRange(writeConstruction());
            info.AddRange(writeZone());
            info.AddRange(writeZoneList());
            info.AddRange(writeBuildingSurfaceList());
            building.iMasses.ForEach(m => info.AddRange(m.WriteInfo()));
            info.AddRange(writeFenestrationSurfaceList());
            info.AddRange(writeShading());

            info.AddRange(WriteDayLightControl());
            info.AddRange(writePeople());
            info.AddRange(writeLights());
            info.AddRange(writeElectricEquipment()); info.AddRange(writeZoneInfiltration()); info.AddRange(writeZoneVentilation());
            info.AddRange(writeHVACTemplate());

            info.AddRange(WritePVPanels());
            building.zones.Where(z => z.NaturalVentiallation != null).ToList().ForEach(z => info.AddRange(z.NaturalVentiallation.WriteInfo()));

            info.AddRange(output.writeInfo());
            return info;
        }

        private IEnumerable<string> WritePVPanels()
        {
            List<string> info = new List<string>();
            if (building.electricLoadCenterDistribution != null)
            {
                ElectricLoadCenterDistribution dist = building.electricLoadCenterDistribution;
                info.AddRange(dist.WriteInfo());
                info.AddRange(dist.GeneratorList.WriteInfo());
                dist.GeneratorList.Generator.ForEach(g => info.AddRange(g.WriteInfo()));
                dist.GeneratorList.Generator.Select(g => g.pperformance).Distinct().ToList().ForEach(p => info.AddRange(p.WriteInfo()));
            }
            return info;
        }
        private List<string> WriteDayLightControl()
        {
            List<string> info = new List<string>();
            if (building.zones.Where(z => z.DayLightControl != null).Count() != 0)
            {
                building.zones.ForEach(z => z.DayLightControl.ReferencePoints.ForEach(p => info.AddRange(p.WriteInfo())));
                building.zones.ForEach(z => info.AddRange(z.DayLightControl.WriteInfo()));
            }
            return info;
        }
        private List<string> writeMaterial()
        {
            List<string> info = new List<string>();
            info.Add("!-   =========== ALL OBJECTS IN CLASS: MATERIAL ===========");
            foreach (Material l in building.materials)
            {
                info.AddRange(l.writeInfo());
            }
            return info;
        }
        private List<string> writeWindowMaterial()
        {
            List<string> info = new List<string>();
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: WINDOWMATERIAL:SIMPLEGLAZINGSYSTEM ===========");
            building.windowMaterials.ForEach(wm => info.AddRange(wm.writeInfo()));
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: WINDOWMATERIAL:SHADING ===========");
            building.windowMaterialShades.ForEach(sh => info.AddRange(sh.writeInfo()));
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: SHADINGCONTROL ===========");
            building.shadingControls.ForEach(shc => info.AddRange(shc.WriteInfo()));
            return info;
        }
        private List<string> writeConstruction()
        {
            List<string> info = new List<string>();
            info.Add("!-   ===========  ALL OBJECTS IN CLASS: CONSTRUCTION ===========");
            building.constructions.ForEach(c => info.AddRange(c.WriteInfo()));
            return info;
        }
        private List<string> writeZone()
        {
            List<string> info = new List<string>();
            info.Add("\r!-   ===========  ALL OBJECTS IN CLASS: ZONE ===========");
            building.zones.ForEach(z => info.Add("Zone,\r\t" + z.name + ";\t\t\t\t\t\t!-Name"));
            return info;
        }
        private List<string> writeZoneList()
        {
            List<string> idfString = new List<string>();

            idfString.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ZONELIST ===========\r\n");

            foreach (ZoneList zl in building.zoneLists)
            {
                idfString.Add("ZoneList,");
                idfString.Add(Utility.IDFLineFormatter(zl.name, "Name"));

                foreach (Zone z in zl.listZones)
                {
                    idfString.Add(Utility.IDFLineFormatter(z.name, "Zone " + (zl.listZones.IndexOf(z) + 1) + " Name"));
                }
                idfString.ReplaceLastComma();
            }
            return idfString;
        }
        private List<String> writeBuildingSurfaceList()
        {
            List<String> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: BUILDINGSURFACE:DETAILED ===========\r\n");
            foreach (Zone z in building.zones)
            {
                foreach (BuildingSurface bSur in z.surfaces)
                {
                    info.AddRange(bSur.surfaceInfo());
                }
            }
            return info;
        }
        private List<String> writeFenestrationSurfaceList()
        {
            List<String> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: FENESTRATIONSURFACE:DETAILED ===========\r\n");
            foreach (Zone z in building.zones)
            {
                foreach (BuildingSurface bSur in z.surfaces)
                {
                    if (bSur.fenestrations != null)
                    {
                        foreach (Fenestration fen in bSur.fenestrations)
                        {
                            info.AddRange(fen.WriteInfo());
                        }
                    }
                }
            }
            return info;
        }
        private List<string> writePeople()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: PEOPLE ===========\r\n");
            foreach (ZoneList zList in building.zoneLists)
            {
                info.AddRange(zList.People.WriteInfo());
            }
            return info;
        }
        private List<string> writeZoneVentilation()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ZONEVENTILATION:DESIGNFLOWRATE ===========\r\n");
            foreach (ZoneList zList in building.zoneLists)
            {
                info.AddRange(zList.ZoneVentilation.WriteInfo());
            }
            return info;
        }
        private List<string> writeLights()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: LIGHTS ===========\r\n");
            foreach (ZoneList zList in building.zoneLists)
            {
                info.AddRange(zList.Light.WriteInfo());
            }
            return info;
        }
        private List<string> writeElectricEquipment()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ELECTRICEQUIPMENT ===========\r\n");
            foreach (ZoneList zList in building.zoneLists)
            {
                info.AddRange(zList.ElectricEquipment.WriteInfo());
            }
            return info;
        }
        private List<string> writeHVACThermostat()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: HVACTEMPLATE:THERMOSTAT ===========\r\n");

            foreach (Thermostat t in building.tStats)
            {
                info.Add("HVACTemplate:Thermostat,");
                info.Add("\t" + t.name + ", \t\t\t\t!- Name");
                info.Add("\t" + t.ScheduleHeating.name + ",\t\t\t\t!- Heating Setpoint Schedule Name");
                info.Add("\t" + ", \t\t\t\t!- Constant Heating Setpoint {C}");
                info.Add("\t" + t.ScheduleCooling.name + ",\t\t\t\t!- Cooling Setpoint Schedule Name");
                info.Add("\t" + "; \t\t\t\t!- Constant Cooling Setpoint {C}");
            }

            return info;
        }
        public List<String> writeHVACTemplate()
        {
            List<string> info = writeHVACThermostat();
            try
            {
                try
                {
                    building.zones.ForEach(z => info.AddRange((z.hvac as ZoneFanCoilUnit).writeInfo()));
                }
                catch { building.zones.ForEach(z => info.AddRange((z.hvac as ZoneVAV).writeInfo())); info.AddRange(building.vav.writeInfo()); }
                info.AddRange(building.cWaterLoop.writeInfo());
                info.AddRange(building.chiller.writeInfo());
                info.AddRange(building.tower.writeInfo());
                info.AddRange(building.hWaterLoop.writeInfo());
                info.AddRange(building.boiler.writeInfo());
            }
            catch { }
            try
            {
                building.zones.ForEach(z => info.AddRange((z.hvac as ZoneIdealLoad).writeInfo()));
            }
            catch { }
            try 
            { 
                building.zones.ForEach(z => info.AddRange((z.hvac as ZoneBaseBoardHeat).writeInfo())); 
                info.AddRange(building.hWaterLoop.writeInfo());
                info.AddRange(building.boiler.writeInfo());
            } catch { }

            return info;
        }
        public List<String> writeZoneInfiltration()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: ZONEINFILTRATION:DESIGNFLOWRATE ===========\r\n");

            foreach (ZoneList z in building.zoneLists)
            {
                info.AddRange(z.ZoneInfiltration.WriteInfo());
            }
            return info;
        }
        public List<string> writeShading()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: SHADING:ZONE:DETAILED ===========\r\n");

            try
            {
                info.AddRange((building.zones.SelectMany(z => z.surfaces.SelectMany(s => s.shading))).SelectMany(s => s.shadingInfo()));
            }
            catch { }

            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: SHADING:OVERHANG:PROJECTION ===========\r\n");

            try
            {
                info.AddRange((building.zones.SelectMany(z => z.surfaces.SelectMany(s => s.fenestrations.Select(f => f.overhang)))).SelectMany(s => s.OverhangInfo()));
            }
            catch { }

            return info;
        }
        public List<string> writeSchedules()
        {
            List<string> info = new List<string>();

            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: SCHEDULETYPELIMITS ===========\r\n");
            foreach (ScheduleLimits sched in building.schedulelimits)
            {
                info.AddRange(sched.WriteInfo());
            }

            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: SCHEDULE:COMPACT ===========\r\n");
            foreach (ScheduleCompact sched in building.schedulescomp)
            {
                info.AddRange(sched.WriteInfo());
            }

            return info;
        }
        public void GenerateOutput(bool heatFlow, string frequency)
        {
            Dictionary<string, string> outputvars = new Dictionary<string, string>();
            outputvars.Add("Zone Air System Sensible Heating Energy", frequency);
            outputvars.Add("Zone Air System Sensible Cooling Energy", frequency);

            if (building.boiler != null)
            {
                if (building.boiler.fuelType.Contains("Electricity")) { outputvars.Add("Boiler Electric Energy", frequency); }
                else { outputvars.Add("Boiler Gas Energy", frequency); }
            }
            outputvars.Add("Chiller Electric Energy", frequency);
            outputvars.Add("Cooling Tower Fan Electric Energy", frequency);

            outputvars.Add("Zone Lights Electric Energy", frequency);
            outputvars.Add("Zone Electric Equipment Electric Energy", frequency);

            outputvars.Add("Facility Total Purchased Electric Energy", frequency);

            if (heatFlow)
            {
                outputvars.Add("Zone Infiltration Total Heat Loss Energy", frequency);
                outputvars.Add("Zone Infiltration Total Heat Gain Energy", frequency);
                outputvars.Add("Surface Window Net Heat Transfer Energy", frequency);
                outputvars.Add("Surface Inside Face Conduction Heat Transfer Energy", frequency);
                outputvars.Add("Surface Outside Face Incident Solar Radiation Rate per Area", frequency);
            }

            output = new Output(outputvars);
        }
    }

    public class BuildingDesignParameters
    {
        public double Length, Width, Height, rLenA, rWidA, BasementDepth, Orientation, FloorArea;
        public WWR wwr;
        public BuildingConstruction construction;
        public BuildingOperation operation;
        public BuildingDesignParameters() { }

        public int NFloors;
    }
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
        public List<WindowShadingControl> shadingControls = new List<WindowShadingControl>();
        public List<Construction> constructions = new List<Construction>();

        //Zone, ZoneList, BuidlingSurface, ShadingZones, ShadingOverhangs
        public List<Zone> zones = new List<Zone>();
        public List<ZoneList> zoneLists = new List<ZoneList>();
        public List<BuildingSurface> bSurfaces = new List<BuildingSurface>();
        public List<InternalMass> iMasses = new List<InternalMass>();
        public List<ShadingZone> shadingZones = new List<ShadingZone>();
        public List<ShadingOverhang> shadingOverhangs = new List<ShadingOverhang>();

        //HVAC Template - should be extracted from zone
        public List<Thermostat> tStats = new List<Thermostat>();
        public List<ZoneHVAC> HVACS = new List<ZoneHVAC>();
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
            foreach (BuildingSurface toupdate in bSurfaces.Where(s=>s.surfaceType == SurfaceType.Wall && s.ConstructionName != "InternalWall"))
            {
                toupdate.AssociateWWRandShadingLength();
                toupdate.CreateFenestration(1);       
            }
            CreateShadingControls();                
        }
        void UpdateZoneInfo()
        {
            zones.ForEach(z => z.CalcAreaVolumeHeatCapacity());
        }
        public void CreateInternalMass(double percentArea, bool IsWall)
        {
            zones.ForEach(z =>
            {
                z.CalcAreaVolume();
                InternalMass mass = new InternalMass(z, percentArea * z.area * FloorHeight, "InternalWall", IsWall);
                iMasses.Add(mass);
            });

        }
        void CreateShadingControls()
        {
            shadingControls = new List<WindowShadingControl>();
            foreach (BuildingSurface face in bSurfaces.Where(s => s.surfaceType == SurfaceType.Wall))
            {
                switch (face.direction)
                {
                    case Direction.North:
                        face.fenestrations.ForEach(f=>f.shadingControl = null);
                        break;
                    case Direction.East:
                    case Direction.South:
                    case Direction.West:
                        face.fenestrations.ForEach(f => f.shadingControl = new WindowShadingControl(f));
                        shadingControls.AddRange(face.fenestrations.Select(f=> f.shadingControl));
                        break;
                }
            }
            
        }
        public void CreatePVPanelsOnRoof()
        {
            PhotovoltaicPerformanceSimple photovoltaicPerformanceSimple = new PhotovoltaicPerformanceSimple();
            List<GeneratorPhotovoltaic> listPVs = new List<GeneratorPhotovoltaic>();
            List<BuildingSurface> roofs = bSurfaces.FindAll(s => s.surfaceType == SurfaceType.Roof);

            ScheduleCompact scheduleOn = schedulescomp.First(s => s.name.Contains("AlwaysOn"));
            roofs.ForEach(s => listPVs.Add(new GeneratorPhotovoltaic(s, photovoltaicPerformanceSimple, scheduleOn)));
            ElectricLoadCenterGenerators electricLoadCenterGenerators= new ElectricLoadCenterGenerators(listPVs);
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
                int[] time = Utility.hourToHHMM(buildingOperation.operatingHours);
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
                scheduleLimits = temp,
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
                scheduleLimits = temp,
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
                scheduleLimits = temp,
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
                scheduleLimits = fractional,
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
                scheduleLimits = fractional,
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
                scheduleLimits = fractional,
                daysTimeValue = leHeatGain
            };

            ScheduleCompact nocooling = new ScheduleCompact()
            {
                name = "No Cooling",
                scheduleLimits = temp,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 35} } } }
            };

            ScheduleCompact activity = new ScheduleCompact()
            {
                name = "People Activity Schedule",
                scheduleLimits = activityLevel,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 125} } } }
            };

            ScheduleCompact workEff = new ScheduleCompact()
            {
                name = "Work Eff Sch",
                scheduleLimits = fractional,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 1} } } }
            };

            ScheduleCompact airVelo = new ScheduleCompact()
            {
                name = "Air Velo Sch",
                scheduleLimits = fractional,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", .1} } } }
            };

            //infiltration
            ScheduleCompact infiltration = new ScheduleCompact()
            {
                name = "Space Infiltration Schedule",
                scheduleLimits = fractional,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 1} } } }
            };
            ScheduleCompact alwaysOn = new ScheduleCompact()
            {
                name = "AlwaysOn",
                scheduleLimits = fractional,
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 1} } } }
            };
            schedulescomp.Add(alwaysOn);
            //schedulescomp.Add(heatingSP);
            //schedulescomp.Add(coolingSP);
            //schedulescomp.Add(heatingSP18);
            //schedulescomp.Add(nocooling);
            schedulescomp.Add(occupSchedule);
            //schedulescomp.Add(ventilSchedule);
            //schedulescomp.Add(lehgSchedule);
            //schedulescomp.Add(activity);
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

            double th_insul_wall = lambda_insulation*(1/uWall- (0.2 / 0.5 + 0.015 / 0.5));
            double th_insul_gFloor = lambda_insulation * (1/ uGFloor - (0.1 / 1.95));
            double th_insul_iFloor = lambda_insulation *  (1/uIFloor - (0.1 / 2));
            double th_insul_Roof = lambda_insulation * (1/uRoof - (0.175 / 0.75 + 0.025 / 0.75 + 0.15 / 0.7));

            double th_insul_IWall = lambda_insulation * (1/uIWall - (0.05 / 0.16 + 0.05 / 0.16));

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
            List<Material> layerListInternallWall = new List<Material>() { layer_Plasterboard,layer_iWallInsul, layer_Plasterboard };
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
                zList.GeneratePeopleLightEquipmentVentilationInfiltrationThermostat(startTime, endTime, buildingOperation.areaPerPeople,
                    buildingOperation.lightHeatGain, buildingOperation.equipmentHeatGain, buildingConstruction.infiltration);
                schedulescomp.AddRange(zList.Schedules.Values);
                tStats.Add(zList.Thermostat);
            }
        }
        public void GenerateHVAC()
        {
            zoneLists.ForEach(zl => zl.listZones.ForEach(z => z.thermostat = zl.Thermostat));
            switch (HVACSystem)
            {
                case HVACSystem.FCU:
                    zones.ForEach(z => { ZoneFanCoilUnit zFCU = new ZoneFanCoilUnit(z, z.thermostat); HVACS.Add(zFCU); z.hvac = zFCU; });
                    GenerateWaterLoopsAndSystem();
                    break;

                case HVACSystem.BaseboardHeating:
                    zones.ForEach(z => { ZoneBaseBoardHeat zBBH = new ZoneBaseBoardHeat(z, z.thermostat); HVACS.Add(zBBH); z.hvac = zBBH; });
                    GenerateWaterLoopsAndSystem();
                    break;

                case HVACSystem.VAV:
                    vav = new VAV();
                    zones.ForEach(z => { ZoneVAV zVAV = new ZoneVAV(vav, z, z.thermostat); HVACS.Add(zVAV); z.hvac = zVAV; });                 
                    GenerateWaterLoopsAndSystem();
                    break;

                case HVACSystem.IdealLoad:
                default:
                    zones.ForEach(z => { ZoneIdealLoad zIdeal = new ZoneIdealLoad(z, z.thermostat); HVACS.Add(zIdeal); z.hvac = zIdeal; });
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
            foreach (BuildingSurface surf in bSurfaces)
            {
                if (surf.OutsideCondition == "Outdoors")
                {
                    if (surf.fenestrations != null && surf.fenestrations.Count != 0)
                    {
                        Fenestration win = surf.fenestrations[0];
                        win.SolarRadiation = data[data.Keys.First(a => a.Contains(win.name.ToUpper()) && a.Contains("Surface Outside Face Incident Solar Radiation Rate per Area"))].Average();
                        win.HeatFlow = data[data.Keys.First(s => s.Contains(win.name.ToUpper()) && s.Contains("Surface Window Net Heat Transfer Energy"))].ConvertKWhfromJoule().Average();
                    }
                    surf.SolarRadiation = data[data.Keys.First(s => s.Contains(surf.name.ToUpper()) && s.Contains("Surface Outside Face Incident Solar Radiation Rate per Area") && !s.Contains("WINDOW"))].Average();
                }                
                surf.HeatFlow = data[data.Keys.First(s => s.Contains(surf.name.ToUpper()) && s.Contains("Surface Inside Face Conduction Heat Transfer Energy"))].ConvertKWhfromJoule().Average();
            }
            foreach (Zone zone in zones)
            {
                zone.CalcAreaVolumeHeatCapacity(); zone.AssociateEnergyPlusResults(data);
            }
            TotalArea = zones.Select(z => z.area).Sum(); TotalVolume = zones.Select(z => z.volume).Sum();
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
            foreach (BuildingSurface surf in bSurfaces)
            {
                if (surf.surfaceType == SurfaceType.Wall || surf.surfaceType == SurfaceType.Roof)
                { 
                    if (surf.OutsideCondition=="Outdoors" && surf.fenestrations!=null && surf.fenestrations.Count!=0)
                    {
                        Fenestration win = surf.fenestrations[0];
                        win.p_SolarRadiation = resultsDF[resultsDF.Keys.First(a => a.Contains(win.name.ToUpper()) && a.Contains("Surface Outside Face Incident Solar Radiation Rate per Area"))];
                        win.p_HeatFlow = resultsDF[resultsDF.Keys.First(s => s.Contains(win.name.ToUpper()) && s.Contains("Surface Window Net Heat Transfer Energy"))].ConvertKWhfromJoule();
                        win.SolarRadiation = win.p_SolarRadiation.Average();
                        win.HeatFlow = win.p_HeatFlow.Average();
                        surf.p_SolarRadiation = resultsDF[resultsDF.Keys.First(s => s.Contains(surf.name.ToUpper()) && s.Contains("Surface Outside Face Incident Solar Radiation Rate per Area") && !s.Contains("WINDOW"))];
                        surf.SolarRadiation = surf.p_SolarRadiation.Average();
                    }
                }
                surf.p_HeatFlow = resultsDF[resultsDF.Keys.First(s => s.Contains(surf.name.ToUpper()) && s.Contains("Surface Inside Face Conduction Heat Transfer Energy"))].ConvertKWhfromJoule();
                surf.HeatFlow = surf.p_HeatFlow.Average();
            }
            foreach (Zone zone in zones)
            {
                zone.CalcAreaVolumeHeatCapacity(); zone.AssociateProbabilisticEnergyPlusResults(resultsDF);
            }
            TotalArea = zones.Select(z => z.area).Sum(); TotalVolume = zones.Select(z => z.volume).Sum();
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
            p_OperationalEnergy = new List<double[]>() { p_ThermalEnergy, p_LightingEnergy}.AddArrayElementWise();
            OperationalEnergy = p_OperationalEnergy.Average();
        }
        public void AssociateProbabilisticMLResults(Dictionary<string, double[]> resultsDF)
        {
            foreach (BuildingSurface surf in bSurfaces)
            {
                if (surf.surfaceType == SurfaceType.Wall && surf.OutsideCondition == "Outdoors" && surf.fenestrations != null && surf.fenestrations.Count != 0)
                {
                    Fenestration win = surf.fenestrations[0];
                    win.p_HeatFlow = resultsDF[resultsDF.Keys.First(s => s.Contains(win.name))];
                    win.HeatFlow = win.p_HeatFlow.Average();
                }
                if (!surf.OutsideObject.Contains("Zone"))
                {
                    surf.p_HeatFlow = resultsDF[resultsDF.Keys.First(s => s.Contains(surf.name) && !s.Contains("Window"))];
                    surf.HeatFlow = surf.p_HeatFlow.Average();
                }
            }
            foreach (Zone zone in zones)
            {
                zone.CalcAreaVolumeHeatCapacity(); zone.AssociateProbabilisticMLResults(resultsDF);
            }
            TotalArea = zones.Select(z => z.area).Sum(); TotalVolume = zones.Select(z => z.volume).Sum();
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

            LCE_PENRT = OperationalEnergy*life*PENRTFactor;
            LCE_PERT = OperationalEnergy * life * PERTFactor;
            LifeCycleEnergy = LCE_PENRT+LCE_PERT;
        }

        public Building AddZone(Zone zone)
        {
            zones.Add(zone);
            bSurfaces.AddRange(zone.surfaces);
            try { shadingOverhangs.AddRange(zone.surfaces.Select(f => f.shading).SelectMany(i => i)); } catch { }
            return this;
        }
        public Building AddZone(List<Zone> zones) { zones.ForEach(z => AddZone(z)); return this; }
        public Building AddZoneList(ZoneList zoneList) 
        { 
            zoneLists.Add(zoneList);
            return this; 
        }
        public Building Transform(double angle)
        {
            zones.ForEach(z => z.surfaces.ForEach(bSurf => {
                bSurf.verticesList.Transform(angle);
                bSurf.fenestrations.ForEach(fen => fen.verticesList.Transform(angle));
                bSurf.shading.ForEach(shading => shading.listVertice.Transform(angle));
            }));
            return this;
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
    [Serializable]
    public class EmbeddedEnergyParameters
    {
        public double th_ExtWall, th_IntWall, th_GFloor, th_IFloor, th_Roof, Reinforcement;

        public EmbeddedEnergyParameters() { }

        public EmbeddedEnergyParameters(double th_ExtWall, double th_IntWall, double th_GFloor, double th_IFloor, double th_Roof, double Reinforcement)
        {
            this.th_ExtWall = th_ExtWall;
            this.th_IntWall = th_IntWall;
            this.th_GFloor = th_GFloor;
            this.th_IFloor = th_IFloor;
            this.th_Roof = th_Roof;
            this.Reinforcement = Reinforcement;
        }      
        public List<string> ToCSVString()
        {
            return new List<string>(){
                string.Join(",",  th_ExtWall,  th_IntWall,  th_GFloor,  th_IFloor,  th_Roof, Reinforcement)
            };
        }
        public string Header()
        {
            return "Const_Thickness_ExtWall, Const_Thickness_Iwall, Const_Thickness_Gfloor, Const_Thickness_Ifloor, Const_Thickness_Roof, Reinforcement";
        }
    }
    [Serializable]
    public class ProbabilisticEmbeddedEnergyParameters
    {
        public double[] th_ExtWall, th_IntWall, th_GFloor, th_IFloor, th_Roof, Reinforcement;

        public ProbabilisticEmbeddedEnergyParameters() { }

        public ProbabilisticEmbeddedEnergyParameters(double[] th_ExtWall, double[] th_IntWall, double[] th_GFloor, double[] th_IFloor, double[] th_Roof, double[] Reinforcement)
        {
            this.th_ExtWall = th_ExtWall;
            this.th_IntWall = th_IntWall;
            this.th_GFloor = th_GFloor;
            this.th_IFloor = th_IFloor;
            this.th_Roof = th_Roof;
            this.Reinforcement = Reinforcement;
        }
        public EmbeddedEnergyParameters GetAverage()
        {
            return new EmbeddedEnergyParameters(th_ExtWall.Average(), th_IntWall.Average(), th_GFloor.Average(), th_IFloor.Average(), th_Roof.Average(), Reinforcement.Average());          
        }
        public List<string> ToCSVString()
        {
            return new List<string>(){
                string.Join(",", th_ExtWall[0], th_IntWall[0], th_GFloor[0], th_IFloor[0], th_Roof[0], Reinforcement[0]),
                string.Join(",", th_ExtWall[1], th_IntWall[1], th_GFloor[1], th_IFloor[1], th_Roof[0], Reinforcement[1])
            };
        }
    }
    [Serializable]
    public class InternalMass
    {
        //        InternalMass ,
        //Zn002:IntM001 , !- Surface Name
        //INTERIOR , !- Construction Name
        //DORM ROOMS AND COMMON AREAS , !- Zone
        //408.7734; !- Total area exposed to Zone {m2
        public string name, construction;
        public Zone zone;
        public double area;
        public bool IsWall = false;

        public InternalMass() { }
        public InternalMass(Zone z, double area, string construction, bool IsWall)
        {
            this.construction = construction; this.area = area; zone = z;
            name = zone.name + ":IntM" + zone.iMasses.Count + 1;
            this.IsWall = IsWall;
            zone.iMasses.Add(this);
            
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "InternalMass,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(construction, "Construction Name"),
                Utility.IDFLineFormatter(zone.name, "Zone Name"),
                Utility.IDFLastLineFormatter(area, "Area")
            };
        }
    }
    [Serializable]
    public class WWR
    {
        public double north;
        public double east;
        public double south;
        public double west;

        public WWR() { }

        public WWR(double north, double east, double south, double west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }
        public string ToCSVString()
        {
            return string.Join(",", north, east, west, south);
        }
        public string Header()
        {
            return string.Join(",", "WWR_North", "WWR_East", "WWR_West", "WWR_South");
        }
    }
    [Serializable]
    public class ShadingLength
    {
        public double north;
        public double east;
        public double south;
        public double west;

        public ShadingLength() { }

        public ShadingLength(double north, double east, double south, double west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }


    }
    [Serializable]
    public class ProbabilisticWWR
    {
        public double[] north;
        public double[] east;
        public double[] south;
        public double[] west;

        public ProbabilisticWWR() { }

        public ProbabilisticWWR(double[] north, double[] east, double[] south, double[] west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }
        public WWR GetAverage()
        {
            return new WWR()
            {
                north = north.Average(),
                east = east.Average(),
                west = west.Average(),
                south = south.Average()
            };
        }
        public List<string> ToCSVString()
        {
            return new List<string>(){
                string.Join(",", north[0], east[0], west[0], south[0]),
                string.Join(",", north[1], east[1], west[1], south[1])
            };
        }
    }
    [Serializable]
    public class BuildingConstruction
    {
        //To store the values from samples
        public double uWall, uGFloor, uRoof, uIFloor, uIWall, uWindow, gWindow, hcSlab, infiltration;

        public double hcWall, hcRoof, hcGFloor, hcIFloor, hcIWall;

        public BuildingConstruction() { }
        public BuildingConstruction(double uWall, double uGFloor, double uRoof, double uWindow, double gWindow, double uIFloor, double uIWall, double hcSlab)
        {
            this.uWall = uWall; this.uGFloor = uGFloor; this.uRoof = uRoof; this.uWindow = uWindow; this.gWindow = gWindow; this.uIFloor = uIFloor; this.uIWall = uIWall; this.hcSlab = hcSlab;
        }
        public string ToCSVString()
        {
            return string.Join(",", uWall, uGFloor, uRoof, uIFloor, uIWall, uWindow, gWindow, hcSlab, infiltration);
        }
        public string Header()
        {
            return string.Join(",", "u_Wall", "u_GFloor", "u_Roof", "u_IFloor", "u_IWall", "u_Window", "g_Window", "hc_Slab", "Infiltration");
        }
    }
    [Serializable]
    public class ProbabilisticBuildingOperation
    {
        public double[] operatingHours, startTime, endTime, areaPerPeople, ventilation, lightHeatGain, equipmentHeatGain, boilerEfficiency, chillerCOP;
        public ProbabilisticBuildingOperation() { }

        public BuildingOperation GetAverage()
        {
            BuildingOperation boP = new BuildingOperation();
            try
            {
                boP.startTime = startTime.Average();
                boP.endTime = endTime.Average();
                boP.areaPerPeople = areaPerPeople.Average();
                boP.ventillation = ventilation.Average();
                boP.operatingHours = boP.endTime - boP.startTime;
            }
            catch { }

            try { boP.operatingHours = operatingHours.Average(); } catch { }
            boP.lightHeatGain = lightHeatGain.Average();
            boP.equipmentHeatGain = equipmentHeatGain.Average();
            boP.boilerEfficiency = boilerEfficiency.Average();
            boP.chillerCOP = chillerCOP.Average();
            return boP;
        }
        public List<string> ToCSVString()
        {
            try
            {
                return new List<string>(){
                string.Join(",", operatingHours[0], lightHeatGain[0], equipmentHeatGain[0], boilerEfficiency[0], chillerCOP[0]),
                string.Join(",", operatingHours[1], lightHeatGain[1], equipmentHeatGain[1], boilerEfficiency[1], chillerCOP[1]) };
            }
            
            catch
            {
                return new List<string>(){
                string.Join(",", startTime[0], endTime[0], lightHeatGain[0], equipmentHeatGain[0], boilerEfficiency[0], chillerCOP[0]),
                string.Join(",", startTime[1], endTime[1], lightHeatGain[1], equipmentHeatGain[1], boilerEfficiency[1], chillerCOP[1]) };
            }
        }

    }
    [Serializable]
    public class BuildingOperation
    {
        public double operatingHours = 0, lightHeatGain = 0 , equipmentHeatGain = 0, boilerEfficiency = 0, chillerCOP = 0;
        public double areaPerPeople = 10, ventillation = 0;
        public double startTime = 0 , endTime = 0;
        public double[] heatingSetPoints, coolingSetPoints;
        public BuildingOperation() { }
        public string ToCSVString()
        {
            return string.Join(",", operatingHours, lightHeatGain, equipmentHeatGain, boilerEfficiency, chillerCOP);
        }
        public string Header()
        {
            return string.Join(",", "Operating Hours", "Light Heat Gain", "Equipment Heat Gain", "Boiler Efficiency", "Chiller COP");
        }
    }
    [Serializable]
    public class ProbabilisticBuildingConstruction
    {
        //To store the values from samples
        public double[] uWall, uGFloor, uRoof, uIFloor, uIWall, uWindow, gWindow, hcSlab, infiltration;

        public ProbabilisticBuildingConstruction() { }
        public ProbabilisticBuildingConstruction(double[] uWall, double[] uGFloor, double[] uRoof, double[] uIFloor, double[] uIWall, double[] uWindow, double[] gWindow, double[] HCFloor)
        {
            this.uWall = uWall;
            this.uGFloor = uGFloor;
            this.uRoof = uRoof;
            this.uIFloor = uIFloor;
            this.uIWall = uIWall;
            this.uWindow = uWindow;
            this.gWindow = gWindow;
            this.hcSlab = HCFloor;
        }
        public BuildingConstruction GetAverage()
        {
            return new BuildingConstruction()
            {
                uWall = uWall.Average(),
                uGFloor = uGFloor.Average(),
                uRoof = uRoof.Average(),
                uIFloor = uIFloor.Average(),
                uIWall = uIWall.Average(),
                uWindow = uWindow.Average(),
                gWindow = gWindow.Average(),
                hcSlab = hcSlab.Average(),
                infiltration = infiltration.Average()
                
            };
        }
        public  List<string> ToCSVString()
        {
            return new List<string>(){
                string.Join(",", uWall[0], uGFloor[0], uRoof[0], uIFloor[0], uIWall[0], uWindow[0], gWindow[0], hcSlab[0], infiltration[0]),
                string.Join(",", uWall[1], uGFloor[1], uRoof[1], uIFloor[1], uIWall[1], uWindow[1], gWindow[1], hcSlab[1], infiltration[1])
            };
        }
    }
    [Serializable]
    public class ShadingZone
    {
        public ShadingZone() { }
    }
    [Serializable]
    public class WindowShadingControl
    {
        public string name = "CONTROL ON ZONE TEMP";
        public Zone zone;
        public string sequenceNumber = "";
        public string shadingType = "InteriorShade";
        public string construction = "";

        public string shadingControlType = "OnIfHighZoneAirTemperature";
        public string scehduleName = "";
        public double setPoint = 23;
        public string scheduled = "NO";
        public string glareControl = "NO";

        public string material = "ROLL SHADE";
        public string angleControl = "";
        public string slatSchedule = "";
      
        public string setPoint2 = "";
        public DayLighting daylightcontrolobjectname;
        public string multipleSurfaceControlType = "";
        public Fenestration fenestration;

        public WindowShadingControl() { }
        public WindowShadingControl(Fenestration fenestration)
        {
            this.fenestration = fenestration; zone = fenestration.face.zone;
            name = string.Format("CONTROL ON ZONE TEMP {0}", fenestration.name);
            daylightcontrolobjectname = zone.DayLightControl;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "WindowShadingControl,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(zone.name, "Zone Name"),
                Utility.IDFLineFormatter(sequenceNumber, "Sequence Number"),
                Utility.IDFLineFormatter(shadingType, "Shading Type"),
                Utility.IDFLineFormatter(construction, "Construction with Shading Name"),
                Utility.IDFLineFormatter(shadingControlType, "Shading Control Type"),
                Utility.IDFLineFormatter(scehduleName, "Schedule Name"),
                Utility.IDFLineFormatter(setPoint, "Setpoint {W/m2, W or deg C}"),
                Utility.IDFLineFormatter(scheduled, "Shading Control Is Scheduled"),
                Utility.IDFLineFormatter(glareControl, "Glare Control Is Active"),
                Utility.IDFLineFormatter(material, "Shading Device Material Name"),
                Utility.IDFLineFormatter(angleControl, "Type of Slat Angle Control for Blinds"),
                Utility.IDFLineFormatter(slatSchedule, "Slat Angle Schedule Name"),
                Utility.IDFLineFormatter(setPoint2, "Setpoint 2"),
                Utility.IDFLineFormatter(daylightcontrolobjectname==null? "": daylightcontrolobjectname.Name , "Daylight Control Object Name"),
                Utility.IDFLineFormatter(multipleSurfaceControlType, "Multiple Control Type"),
                Utility.IDFLastLineFormatter(fenestration.name, "Fenestration")
                
            };
        }
    }
    [Serializable]
    public class WindowMaterialShade
    {
        public string name = "ROLL SHADE";
        public double sTransmittance = 0.3;
        public double sReflectance = 0.5;
        public double vTransmittance = 0.3;
        public double vReflectance = 0.5;
        public double infraEmissivity = 0.9;
        public double infraTransmittance = 0.05;
        public double thickness = 0.003;
        public double conductivity = 0.1;
        public double disShades = 0.05;
        public double tMultiplier = 0;
        public double bMultiplier = 0.5;
        public double lMultiplier = 0.5;
        public double rMultiplier = 0;
        public string airPermeability = "";

        public WindowMaterialShade() { }
        public List<string> writeInfo()
        {
            return new List<string>()
            {
                "WindowMaterial:Shade,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(sTransmittance, "Solar Transmittance { dimensionless }"),
                Utility.IDFLineFormatter(sReflectance, "Solar Reflectance { dimensionless }"),
                Utility.IDFLineFormatter(vTransmittance, "Visible Transmittance { dimensionless }"),
                Utility.IDFLineFormatter(vReflectance, "Visible Reflectance { dimensionless }"),
                Utility.IDFLineFormatter(infraEmissivity, "Infrared Hemispherical Emissivity { dimensionless }"),
                Utility.IDFLineFormatter(infraTransmittance, "Infrared Transmittance { dimensionless }"),
                Utility.IDFLineFormatter(thickness, "Thickness { m }"),
                Utility.IDFLineFormatter(conductivity, "Conductivity { W / m - K }"),
                Utility.IDFLineFormatter(disShades, "Shade to Glass Distance { m }"),
                Utility.IDFLineFormatter(tMultiplier, "Top Opening Multiplier"),
                Utility.IDFLineFormatter(bMultiplier, "Bottom Opening Multiplier"),
                Utility.IDFLineFormatter(lMultiplier, "Left - Side Opening Multiplier"),
                Utility.IDFLineFormatter(rMultiplier, "Right - Side Opening Multiplier"),
                Utility.IDFLastLineFormatter(airPermeability, "Airflow Permeability { dimensionless}")
            };
        }
    }
    [Serializable]
    public class Zone
    {
        public double HeatingEnergy, CoolingEnergy, LightingEnergy;

        public double[] p_HeatingEnergy, p_CoolingEnergy, p_LightingEnergy;
        public List<BuildingSurface> surfaces { get; set; }
        public List<InternalMass> iMasses = new List<InternalMass>();
        public double area;
        public double volume;
        public double height;
        public DayLighting DayLightControl;
        public People people { get; set; }
        public ElectricEquipment equipment { get; set; }
        public Light lights { get; set; }
        public ZoneHVAC hvac { get; set; }
        public Thermostat thermostat { get; set; }
        public ZoneVentilation vent { get; set; }
        public ZoneVentilation NaturalVentiallation { get; set; }
        public ZoneInfiltration infiltration { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public Building building;

        public double totalWallArea, totalWindowArea, totalGFloorArea, totalIFloorArea, totalIWallArea, totalRoofArea,
            TotalHeatCapacity, SurfAreaU, SolarRadiation, TotalHeatFlows, ExSurfAreaU, GSurfAreaU, ISurfAreaU,
            wallAreaU, windowAreaU, gFloorAreaU, roofAreaU, iFloorAreaU, iWallAreaU,
            wallHeatFlow, windowHeatFlow, gFloorHeatFlow, iFloorHeatFlow, iWallHeatFlow, roofHeatFlow, infiltrationFlow, windowAreaG;

        public double[] p_TotalHeatFlows, p_wallHeatFlow, p_windowHeatFlow, p_gFloorHeatFlow, p_iFloorHeatFlow, p_iWallHeatFlow, p_roofHeatFlow, p_infiltrationFlow, p_SolarRadiation;

        internal void CalcAreaVolume()
        {
            IEnumerable<BuildingSurface> floors = surfaces.Where(a => a.surfaceType == SurfaceType.Floor);
            area = floors.Select(a => a.area).ToArray().Sum();
            volume = area * height;
        }
        internal void CalcAreaVolumeHeatCapacity()
        {
            CalcAreaVolume();       

            totalWallArea = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").Select(w => w.area).Sum();
            totalGFloorArea = surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").Select(gF => gF.area).Sum();

            totalIFloorArea = surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition != "Ground").Select(iF => iF.area).Sum();

            totalIWallArea = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition != "Outdoors").Select(iF => iF.area).Sum() +
                building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Wall && iF.OutsideCondition != "Outdoors" && iF.OutsideObject == name).Select(iF => iF.area).Sum() + 
                iMasses.Where(i=>i.IsWall).Select(i=>i.area).Sum();

            totalRoofArea = surfaces.Where(w => w.surfaceType == SurfaceType.Roof).Select(r => r.area).Sum();
            totalWindowArea = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                .Where(w => w.fenestrations.Count != 0).SelectMany(w => w.fenestrations).Select(wi => wi.area).Sum();

            TotalHeatCapacity = totalWallArea * building.buildingConstruction.hcWall + totalGFloorArea * building.buildingConstruction.hcGFloor + 
                totalIFloorArea * building.buildingConstruction.hcIFloor +
                totalIWallArea * building.buildingConstruction.hcIWall + totalRoofArea * building.buildingConstruction.hcRoof +
                iMasses.Select(m=>m.area* building.buildingConstruction.hcIWall).Sum();

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
            wallHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").Select(s => s.HeatFlow).Sum();
            gFloorHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").Select(s => s.HeatFlow).Sum();
            iFloorHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition != "Ground").Select(s => s.HeatFlow).Sum();
            iWallHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition != "Outdoors").Select(s => s.HeatFlow).Sum();
            windowHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                .Where(w => w.fenestrations.Count != 0).SelectMany(w => w.fenestrations).Select(s => s.HeatFlow).Sum();
            roofHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Roof).Select(s => s.HeatFlow).Sum();

            iFloorHeatFlow -= building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Floor && iF.OutsideObject == name).Select(s => s.HeatFlow).Sum();
            iWallHeatFlow -= building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Wall && iF.OutsideCondition != "Outdoors" && iF.OutsideObject == name).Select(s => s.HeatFlow).Sum();

            SolarRadiation = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                .Where(w => w.fenestrations.Count != 0).SelectMany(w => w.fenestrations).Select(f => f.area * f.SolarRadiation).Sum();

            infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Gain Energy"))].SubtractArrayElementWise(
                     resultsDF[resultsDF.Keys.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Loss Energy"))]).Average().ConvertKWhfromJoule();
            TotalHeatFlows = wallHeatFlow + gFloorHeatFlow + windowHeatFlow + roofHeatFlow + infiltrationFlow;

            HeatingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Air System Sensible Heating Energy"))].ConvertKWhfromJoule().Average();
            CoolingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Air System Sensible Cooling Energy"))].ConvertKWhfromJoule().Average();
            LightingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Lights Electric Energy"))].ConvertKWhfromJoule().Average();
            
        }
        public void AssociateProbabilisticEnergyPlusResults(Dictionary<string, double[]> resultsDF)
        {
            p_wallHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            p_gFloorHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            p_iFloorHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition != "Ground").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            p_iWallHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition != "Outdoors").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            try { p_windowHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                .Where(w => w.fenestrations.Count != 0).SelectMany(w => w.fenestrations).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise(); } catch { }
            p_roofHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Roof).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();

            p_iFloorHeatFlow = p_iFloorHeatFlow.SubtractArrayElementWise(building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Floor && iF.OutsideObject == name).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise());
            p_iWallHeatFlow  = p_iWallHeatFlow.SubtractArrayElementWise(building.bSurfaces.Where(iF => iF.surfaceType == SurfaceType.Wall && iF.OutsideCondition != "Outdoors" && iF.OutsideObject == name).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise());

            p_SolarRadiation = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                .Where(w => w.fenestrations.Count != 0).SelectMany(w => w.fenestrations).Select(f => f.p_SolarRadiation).ToList().AddArrayElementWise();

            p_infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Gain Energy"))].SubtractArrayElementWise(
                     resultsDF[resultsDF.Keys.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Infiltration Total Heat Loss Energy"))]).ConvertKWhfromJoule();
            p_TotalHeatFlows = new List<double[]>() { p_wallHeatFlow, p_gFloorHeatFlow, p_iFloorHeatFlow, p_iWallHeatFlow, p_windowHeatFlow, p_roofHeatFlow, p_infiltrationFlow }.AddArrayElementWise();

            p_HeatingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Air System Sensible Heating Energy"))].ConvertKWhfromJoule();
            p_CoolingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Air System Sensible Cooling Energy"))].ConvertKWhfromJoule();
            p_LightingEnergy = resultsDF[resultsDF.Keys.First(a => a.Contains(name.ToUpper()) && a.Contains("Zone Lights Electric Energy"))].ConvertKWhfromJoule();
            
        }
        public void AssociateProbabilisticMLResults(Dictionary<string, double[]> resultsDF)
        {
            p_wallHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            p_gFloorHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
           
            p_windowHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors")
                .Where(w => w.fenestrations.Count != 0).SelectMany(w => w.fenestrations).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            p_roofHeatFlow = surfaces.Where(w => w.surfaceType == SurfaceType.Roof).Select(s => s.p_HeatFlow).ToList().AddArrayElementWise();
            
            p_infiltrationFlow = resultsDF[resultsDF.Keys.First(a => a == name+"_Infiltration")];
            p_TotalHeatFlows = new List<double[]>() { p_wallHeatFlow, p_gFloorHeatFlow, p_windowHeatFlow, p_roofHeatFlow, p_infiltrationFlow }.AddArrayElementWise();

            p_HeatingEnergy = resultsDF[resultsDF.Keys.First(a => a == name + "_Heating Energy")];
            p_CoolingEnergy = resultsDF[resultsDF.Keys.First(a => a == name + "_Cooling Energy")];

            p_LightingEnergy = resultsDF[resultsDF.Keys.First(a => a == name + "_Lighting Energy")];
            LightingEnergy = p_LightingEnergy.Average();
               
        }

        public Zone() { }
        public Zone(Building building, string n, int l)
        {
            this.building = building;
            name = n;
            level = l;
            surfaces = new List<BuildingSurface>();
            height = building.FloorHeight;
        }
        public void CreateNaturalVentillation()
        {
            NaturalVentiallation = new ZoneVentilation(this, true);
        }
    }
    [Serializable]
    public class BuildingSurface
    {
        private double GrossArea;
        public double[] p_HeatFlow;

        public XYZList verticesList;
        public string name { get; set; }
        public Zone zone { get; set; }
        public SurfaceType surfaceType { get; set; }
        public string ConstructionName { get; set; }
        public Direction direction { get; set; }
        public double orientation { get; set; }
        public List<Fenestration> fenestrations { get; set; }
        public List<ShadingOverhang> shading { get; set; }
        public string OutsideCondition { get; set; }
        public string OutsideObject { get; set; }
        public string SunExposed { get; set; }
        public string WindExposed { get; set; }
        public double area { get; set; }
        public double wWR { get; set; } = 0;
        public double sl { get; set; } = 0;//shaderlength
        public double df { get; set; }
        public double SolarRadiation;
        public double HeatFlow;
        public double[] p_SolarRadiation;

        public BuildingSurface() { }
        private void addName()
        {
            name = zone.name + ":" + zone.level + ":" + ConstructionName + "_" + (zone.surfaces.Count + 1);
            if (surfaceType == SurfaceType.Wall)
            {
                name = zone.name + ":" + zone.level + ":" + direction + ":" + ConstructionName + "_" + (zone.surfaces.Count + 1);
            }
        }
        internal void AssociateWWRandShadingLength()
        {
            orientation = verticesList.GetWallDirection();

            if (orientation < 45 || orientation >= 315)
            {
                wWR = zone.building.WWR.north;
                sl = zone.building.shadingLength.north;
                direction = Direction.North;
            }

            if (orientation >= 45 && orientation < 135)
            {
                wWR = zone.building.WWR.east;
                sl = zone.building.shadingLength.east;
                direction = Direction.East;
            }

            if (orientation >= 135 && orientation < 225)
            {
                wWR = zone.building.WWR.south;
                sl = zone.building.shadingLength.south;
                direction = Direction.South;
            }

            if (orientation >= 225 && orientation < 315)
            {
                wWR = zone.building.WWR.west;
                sl = zone.building.shadingLength.west;
                direction = Direction.West;
            }
        }
        public BuildingSurface(Zone zone1, XYZList pointList1, double areaN, SurfaceType surfaceType1)
        {
            zone = zone1;
            area = areaN;
            GrossArea = areaN;
            verticesList = pointList1;
            surfaceType = surfaceType1;

            List<XYZ> pointList = pointList1.xyzs;
            switch (surfaceType)
            {
                case (SurfaceType.Floor):
                    ConstructionName = "Slab_Floor";
                    OutsideCondition = "Ground";
                    OutsideObject = "";
                    SunExposed = "NoSun";
                    WindExposed = "NoWind";
                    break;
                case (SurfaceType.Wall):
                    OutsideObject = "";
                    OutsideCondition = "Outdoors";
                    SunExposed = "SunExposed";
                    WindExposed = "WindExposed";
                    AssociateWWRandShadingLength();
                    ConstructionName = "Wall ConcreteBlock";
                    break;
                case (SurfaceType.Ceiling):
                    ConstructionName = "General_Floor_Ceiling";
                    OutsideCondition = "Zone";
                    SunExposed = "NoSun";
                    WindExposed = "NoWind";
                    break;
                case (SurfaceType.Roof):
                    ConstructionName = "Up Roof Concrete";
                    OutsideObject = "";
                    OutsideCondition = "Outdoors";
                    SunExposed = "SunExposed";
                    WindExposed = "WindExposed";
                    break;
            }
            addName();
            //if (wWR != 0) { CreateFenestration(1); }
            //if (sl != 0) { shading = new List<ShadingOverhang>() { new ShadingOverhang(this) }; }
            zone1.surfaces.Add(this);
        }

        public List<string> surfaceInfo()
        {
            List<string> info = new List<string>();
            info.Add("BuildingSurface:Detailed,");
            info.Add("\t" + name + ",\t\t!- Name");
            info.Add("\t" + surfaceType + ",\t\t\t\t\t!-Surface Type");
            info.Add("\t" + ConstructionName + ",\t\t\t\t!-Construction Name");
            info.Add("\t" + zone.name + ",\t\t\t\t\t\t!-Zone Name");
            info.Add("\t" + OutsideCondition + ",\t\t\t\t\t!-Outside Boundary Condition");
            info.Add("\t" + OutsideObject + ",\t\t\t\t\t\t!-Outside Boundary Condition Object");
            info.Add("\t" + SunExposed + ",\t\t\t\t\t\t!-Sun Exposure");
            info.Add("\t" + WindExposed + ",\t\t\t\t\t\t!-Wind Exposure");
            info.Add("\t" + ",\t\t\t\t\t\t!-View Factor to Ground");
            info.AddRange(verticesList.verticeInfo());
            return info;
        }

        internal void CreateFenestration(int count)
        {
            List<Fenestration> fenestrationList = new List<Fenestration>();
            for (int i = 0; i < count; i++)
            {
                Fenestration fen = new Fenestration(this);
                XYZ P1 = verticesList.xyzs.ElementAt(0);
                XYZ P2 = verticesList.xyzs.ElementAt(1);
                XYZ P3 = verticesList.xyzs.ElementAt(2);
                XYZ P4 = verticesList.xyzs.ElementAt(3);
                double openingFactor = Math.Sqrt(wWR / count);

                XYZ pMid = new XYZ((P1.X + P3.X) / (count - i + 1), (P1.Y + P3.Y) / (count - i + 1), (P1.Z + P3.Z) / 2);

                fen.verticesList = new XYZList(verticesList.xyzs.Select(v => new XYZ(pMid.X + (v.X - pMid.X) * openingFactor,
                                                            pMid.Y + (v.Y - pMid.Y) * openingFactor,
                                                            pMid.Z + (v.Z - pMid.Z) * openingFactor)).ToList());
                fen.area = GrossArea * wWR / count;
                fenestrationList.Add(fen);
            }
            fenestrations = fenestrationList;
            area = GrossArea * (1 - wWR);
        }    
    }
    [Serializable]
    public class XYZList
    {
        public List<XYZ> xyzs;
        public XYZList() { }

        public List<XYZ> OffsetAllPoints(double distance)
        {
            List<XYZ> newXYZ = new List<XYZ>();
            for (int i =0; i< xyzs.Count; i++)
            {
                XYZ v1, p, v2, newP;
                p = xyzs[i];
                try { v1 = xyzs[i - 1]; } catch { v1 = xyzs.Last(); }
                try { v2 = xyzs[i + 1]; } catch { v2 = xyzs.First(); }
                if(v1.Subtract(p).AngleOnPlaneTo(v2.Subtract(p), new XYZ(0, 0, 1))>=180)
                {
                    newP = p.MovePoint(-distance, v1, v2);
                }
                else
                {
                    newP = p.MovePoint(distance, v1, v2);
                }
                newXYZ.Add(newP);
            }
            return newXYZ;

        }
        public XYZList OffsetHeight(double height)
        {
            List<XYZ> newVertices = new List<XYZ>();
            foreach (XYZ v in xyzs)
            {
                XYZ v1 = v.OffsetHeight(height);
                newVertices.Add(v1);
            }
            return (new XYZList(newVertices));
        }
        public XYZList(List<XYZ> list)
        {
            xyzs = list;
        }
        public XYZList reverse()
        {
            XYZList newList = Utility.DeepClone(this);
            newList.xyzs.Reverse();
            return newList;
        }
        public List<string> verticeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\t" + ",\t\t\t\t\t\t!- Number of Vertices");
            xyzs.ForEach(xyz => info.Add(string.Join(",", xyz.X, xyz.Y, xyz.Z) + ", !- X Y Z of Point"));
            return info.ReplaceLastComma();
        }
        public List<BuildingSurface> createWalls(Zone z, double height)
        {
            List<BuildingSurface> walls = new List<BuildingSurface>();
            foreach (XYZ v1 in xyzs)
            {
                XYZ v2 = new XYZ(0, 0, 0);
                if (!(v1 == xyzs.Last()))
                { v2 = xyzs.ElementAt((xyzs.IndexOf(v1) + 1)); }
                else { v2 = xyzs.First(); }

                XYZ v3 = v2.OffsetHeight(height);
                XYZ v4 = v1.OffsetHeight(height);

                XYZList vList = new XYZList(new List<XYZ>() { v4, v3, v2.Copy(), v1.Copy() });
                BuildingSurface wall = new BuildingSurface(z, vList, v1.DistanceTo(v2) * height, SurfaceType.Wall);
                walls.Add(wall);
            }

            return walls;
        }
        public List<BuildingSurface> createWalls(Zone z, double height, double basementDepth)
        {
            List<BuildingSurface> walls = new List<BuildingSurface>(); 
            foreach (XYZ v1 in xyzs)
            {
                XYZ v2 = new XYZ(0, 0, 0);
                if (!(v1 == xyzs.Last()))
                { v2 = xyzs.ElementAt((xyzs.IndexOf(v1) + 1)); }
                else { v2 = xyzs.First(); }

                XYZ v3 = v2.OffsetHeight(basementDepth);
                XYZ v4 = v1.OffsetHeight(basementDepth);

                XYZ v5 = v2.OffsetHeight(height);
                XYZ v6 = v1.OffsetHeight(height);

                XYZList vList1 = new XYZList(new List<XYZ>() { v4, v3, v2.Copy(), v1.Copy() });
                BuildingSurface wall1 = new BuildingSurface(z, vList1, v1.DistanceTo(v2) * height, SurfaceType.Wall);
                wall1.fenestrations = new List<Fenestration>();
                wall1.OutsideCondition = "Ground";
                wall1.OutsideObject = "";
                wall1.SunExposed = "NoSun";
                wall1.WindExposed = "NoWind";

                XYZList vList2 = new XYZList(new List<XYZ>() { v6, v5, v3, v4 });
                BuildingSurface wall2 = new BuildingSurface(z, vList2, v3.DistanceTo(v4) * height, SurfaceType.Wall);
                walls.Add(wall1);
                walls.Add(wall2);
            }

            return walls;
        }
        public void Transform(double angle)
        {
            List<XYZ> newXYZ = new List<XYZ>();
            xyzs.ForEach(v => newXYZ.Add(v.Transform(angle)));
            xyzs = newXYZ;
        }
        public double GetWallDirection()
        {
            XYZ v1 = xyzs[0]; XYZ v2 = xyzs[1]; XYZ v3 = xyzs[2];
            XYZ nVector1 = v2.Subtract(v1).CrossProduct(v3.Subtract(v1));
            return nVector1.AngleOnPlaneTo(new XYZ(0, 1, 0), new XYZ(0, 0, 1));
        }
        public XYZList ChangeZValue(double newZ)
        {
            List<XYZ> newVertices = new List<XYZ>();
            xyzs.ForEach(p => newVertices.Add(new XYZ(p.X, p.Y, newZ)));
            return new XYZList(newVertices);
        }
    }
    [Serializable]
    public class XYZ  : IEquatable<XYZ>
    {
        public double X = 0, Y = 0, Z = 0;
        public XYZ() { }
        public XYZ(double x, double y, double z) { X = x; Y = y; Z = z; }
        public XYZ Subtract(XYZ newXYZ) { return new XYZ(newXYZ.X - X, newXYZ.Y - Y, newXYZ.Z - Z); }
        
        public bool Equals(XYZ point1)
        {
            return (X == point1.X && Y == point1.Y && Z == point1.Z);
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public XYZ OffsetHeight(double height)
        {
            return new XYZ(X, Y, Z + height);
        }
        public override string ToString()
        {
            return string.Join(",", X, Y, Z);
        }
        public double DotProduct(XYZ newXYZ)
        {
            return X * newXYZ.X + Y * newXYZ.Y + Z * newXYZ.Z;
        }
        public XYZ CrossProduct(XYZ newXYZ)
        {
            return new XYZ(Y * newXYZ.Z - Z * newXYZ.Y, Z * newXYZ.X - X * newXYZ.Z, X * newXYZ.Y - Y * newXYZ.X);
        }
        public double AngleOnPlaneTo(XYZ right, XYZ normalPlane)
        {
            double nDouble = DotProduct(right);
            double anglePI = Math.Atan2(CrossProduct(right).DotProduct(normalPlane), nDouble - (right.DotProduct(normalPlane)) * DotProduct(normalPlane));
            if (anglePI < 0) { anglePI = Math.PI * 2 + anglePI; }
            return Math.Round(180 * anglePI / Math.PI);
        }
        public double AngleBetweenVectors(XYZ newXYZ)
        {
            return (Math.Round(Math.Acos((X * newXYZ.X + Y * newXYZ.Y + Z * newXYZ.Z) / (AbsoluteValue() * newXYZ.AbsoluteValue())), 2));
        }
        public double DistanceTo(XYZ newXYZ)
        {
            return Math.Sqrt(Math.Pow(X - newXYZ.X, 2) + Math.Pow(Y - newXYZ.Y, 2) + Math.Pow(Z - newXYZ.Z, 2));
        }
        public double AbsoluteValue()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }
        public XYZ MovePoint(double d, XYZ tp1, XYZ tp2)
        {
            double d1 = 2* d / DistanceTo(tp1);
            double d2 = 2*d / DistanceTo(tp2);

            XYZ dir1 = Subtract(tp1);
            XYZ p1 = new XYZ(X + d1 * dir1.X, Y + d1 * dir1.Y, Z + d1 * dir1.Z);

            XYZ dir2 = Subtract(tp2);
            XYZ p2 = new XYZ(X + d2 * dir2.X, Y + d2 * dir2.Y, Z + d2 * dir2.Z);

            return new XYZ((p1.X + p2.X)*.5, (p1.Y + p2.Y)*.5, (p1.Z + p2.Z)*.5);
        }
    }
    [Serializable]
    public class Fenestration
    {
        public Fenestration() { }
        public double area;
        public double SolarRadiation;
        public double HeatFlow;
        public double[] p_HeatFlow;
        public BuildingSurface face { get; set; }
        public XYZList verticesList { get; set; }
        public string constructionName { get; set; }
        public string surfaceType { get; set; }
        public string name { get; set; }
        public WindowShadingControl shadingControl { get; set; }
        public OverhangProjection overhang { get; set; }
        public double[] p_SolarRadiation { get; set; }

        internal Fenestration(BuildingSurface wallFace)
        {
            face = wallFace;
            surfaceType = "Window";
            constructionName = "Glazing";
            name = surfaceType + "_On_" + face.name;           
            verticesList = new XYZList(new List<XYZ>());
        }

        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("FenestrationSurface:Detailed,");
            info.Add(Utility.IDFLineFormatter(name, "Subsurface Name"));
            
            info.Add("\t" + surfaceType + ",\t\t\t\t\t\t!- Surface Type");
            info.Add("\t" + constructionName + ",\t\t\t\t\t\t!- Construction Name");

            info.Add("\t" + face.name + ",\t!-Building Surface Name)");
            info.Add("\t,\t\t\t\t\t\t!-Outside Boundary Condition Object");

            info.Add("\t,\t\t\t\t\t\t!-View Factor to Ground");
            info.Add("\t,\t\t\t\t\t\t!- Frame and Divider Name");
            info.Add("\t,\t\t\t\t\t\t!-Multiplier");

            info.AddRange(verticesList.verticeInfo());
            return info;
        }
    }
    public enum ControlType { none, Continuous, Stepped, ContinuousOff }
    [Serializable]
    public class DayLightRefPoint
    {
        public string Name;
        public XYZ Point;
        public Zone Zone;
        public double PartControlled;
        public double Illuminance;
        public DayLightRefPoint() { }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>()
            {
                "Daylighting:ReferencePoint,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(Zone.name, "Zone Name"),
                Utility.IDFLineFormatter(Point, "XYZ of Point")
            };
            return info.ReplaceLastComma();
        }

    }
    [Serializable]
    public class DayLighting
    {
        public string Name;
        public Zone Zone { get; set; }
        public string DLMethod = "SplitFlux";
        public List<DayLightRefPoint> ReferencePoints = new List<DayLightRefPoint>();

        public ControlType CType = ControlType.Continuous;
        public double GlareCalcAngle = 180;
        public double DiscomGlare = 20;
        public double MinPower = 0.3;
        public double MinLight = 0.3;
        public int NStep = 3;
        public double ProbabilityManual = 1;
        public string AvailabilitySchedule;
        public double DELightGridResolution = 2;

        public DayLighting() { }
        public List<DayLightRefPoint> CreateZoneDayLightReferencePoints(Zone zone, List<XYZ> points, double illuminance)
        {
            List<DayLightRefPoint> dlRefPoints = new List<DayLightRefPoint>();
            double totalPoints = points.Count();
            double pControlled = Math.Round(.99 / totalPoints, 5);
            points.ForEach(p => dlRefPoints.Add(new DayLightRefPoint()
            {
                Zone = zone, Point = p, Name = "Day Light Reference Point " + (points.IndexOf(p) + 1) + " for " + zone.name,
                Illuminance = illuminance, PartControlled = pControlled
            }));
            return dlRefPoints;
        }
        public DayLighting(Zone zone, string schedule, List<XYZ> points, double illuminance)
        {
            Name = "DayLight Control For " + zone.name;
            Zone = zone;
            AvailabilitySchedule = schedule;
            ReferencePoints = CreateZoneDayLightReferencePoints(zone, points, illuminance);
            zone.DayLightControl = this;
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>()
            {
                "Daylighting:Controls,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(Zone.name, "Zone Name"),
                Utility.IDFLineFormatter(DLMethod, "Daylighting Method"),
                Utility.IDFLineFormatter(AvailabilitySchedule, "Availability Schedule Name"),
                Utility.IDFLineFormatter(CType, "Lighting control type {1=continuous,2=stepped,3=continuous/off}"),
                Utility.IDFLineFormatter(MinPower, "Minimum input power fraction for continuous dimming control"),
                Utility.IDFLineFormatter(MinLight, "Minimum light output fraction for continuous dimming control"),
                Utility.IDFLineFormatter(NStep, "Number of steps, excluding off, for stepped control"),
                Utility.IDFLineFormatter(ProbabilityManual, "Probability electric lighting will be reset when needed"),
                Utility.IDFLineFormatter(null, "Glare Calculation Reference Point Name"),
                Utility.IDFLineFormatter(GlareCalcAngle, "Azimuth angle of view direction for glare calculation {deg}"),
                Utility.IDFLineFormatter(DiscomGlare, "Maximum discomfort glare index for window shade control"),
                Utility.IDFLineFormatter(DELightGridResolution, "DE Light Gridding Resolution")
            };
            ReferencePoints.ForEach(p => info.AddRange(new List<string>() {
                Utility.IDFLineFormatter(p.Name, "Reference Point"),
                Utility.IDFLineFormatter(p.PartControlled, "Part Controlled"),
                Utility.IDFLineFormatter(p.Illuminance, "Illuminance Setpoint")
            }));
            return info.ReplaceLastComma();
        }
    }
    [Serializable]
    public class Material
    {
        public string name { get; set; }
        public string roughness { get; set; }
        public double thickness { get; set; }
        public double conductivity { get; set; }
        public double density { get; set; }
        public double sHC { get; set; }
        public double tAbsorptance { get; set; }
        public double sAbsorptance { get; set; }
        public double vAbsorptance { get; set; }
        public Material() { }
        public Material(string name, string rough, double th, double conduct, double dense, double sH, double tAbsorp, double sAbsorp, double vAbsorp)
        {
            this.name = name;
            roughness = rough;
            thickness = th;
            conductivity = conduct;
            density = dense;
            sHC = sH;
            tAbsorptance = tAbsorp;
            sAbsorptance = sAbsorp;
            vAbsorptance = vAbsorp;
        }
        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("Material,");
            info.Add(name + ",          !-Name");
            info.Add(roughness + ",            !-Roughness");
            info.Add(thickness + ",                    !-Thickness { m}");
            info.Add(conductivity + ",               !-Conductivity { W / m - K}");
            info.Add(density + ",                !-Density { kg / m3}");
            info.Add(sHC + ",                 !-Specific Heat { J / kg - K}");
            info.Add(tAbsorptance + ",                    !-Thermal Absorptance");
            info.Add(sAbsorptance + ",                    !-Solar Absorptance");
            info.Add(vAbsorptance + "; !-Visible Absorptance");
            return info;
        }
    }
    [Serializable]
    public class WindowMaterial
    {
        public string name { get; set; }
        public double uValue { get; set; }
        public double gValue { get; set; }
        public double vTransmittance { get; set; }
        public WindowMaterial()
        {
        }
        public WindowMaterial(string n, double u, double g, double transmittance)
        {
            name = n; uValue = u; gValue = g; vTransmittance = transmittance;
        }
        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("WindowMaterial:SimpleGlazingSystem,");
            info.Add(name + ",  !- Name");
            info.Add(uValue + ",                 !- U-Factor {W/m2-K}");
            info.Add(gValue + ",                 !- Solar Heat Gain Coefficient");
            info.Add(vTransmittance + ";                     !- Visible Transmittance");
            return info;
        }
    }
    [Serializable]
    public class Construction
    {
        public Construction() { }
        public string name { get; set; }
        public List<Material> layers { get; set; }
        public List<WindowMaterial> wLayers { get; set; }
        public double heatCapacity { get; set; }
        public Construction(string n, List<Material> layers)
        {
            name = n; this.layers = layers; wLayers = new List<WindowMaterial>();
            heatCapacity = layers.Select(la => la.thickness * la.sHC * la.density).Sum();
        }
        public Construction(string n, List<WindowMaterial> layers)
        {
            name = n; wLayers = layers; this.layers = new List<Material>();
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("Construction,");
            info.Add(name + ",   !- Name");

            if (wLayers.Count == 0)
            {
                for (int i = 0; i < layers.Count; i++)
                {
                    Material l = layers[i];
                    if (i != layers.Count - 1)
                    { info.Add(l.name + ",     !- Outside Layer"); }
                    else
                    { info.Add(l.name + ";     !- Outside Layer"); }
                }
            }
            else
            {
                for (int i = 0; i < wLayers.Count; i++)
                {
                    WindowMaterial l = wLayers[i];
                    if (i != wLayers.Count - 1)
                    { info.Add(l.name + ",     !- Outside Layer"); }
                    else
                    { info.Add(l.name + ";     !- Outside Layer"); }
                }
            }
            return info;
        }
    }
    [Serializable]
    public class WindowConstruction
    {
        public string name { get; set; }
        public List<WindowMaterial> layers { get; set; }
        public double uValue { get; set; }
        public double gValue { get; set; }
        public WindowConstruction() { }
        public WindowConstruction(string n, List<WindowMaterial> l)
        {
            name = n; layers = l;
        }
        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("Construction,");
            info.Add(name + ",   !- Name");
            foreach (WindowMaterial l in layers)
            {
                if (l != layers.Last())
                { info.Add(l.name + ",     !- Outside Layer"); }
                else
                { info.Add(l.name + ";     !- Outside Layer"); }
            }
            return info;
        }

    }
    [Serializable]
    public class ZoneList
    {
        public List<Zone> listZones;
        public People People;
        public ZoneVentilation ZoneVentilation;
        public ZoneInfiltration ZoneInfiltration;
        public Light Light;
        public ElectricEquipment ElectricEquipment;
        public Thermostat Thermostat;
        public Dictionary<string, ScheduleCompact> Schedules = new Dictionary<string, ScheduleCompact>();
        public string name { get; set; }
        public ZoneList() { }
        public ZoneList(string n)
        {
            name = n;
            listZones = new List<Zone>();
        }
        public void CreateSchedules(double startTime, double endTime)
        {
            Schedules = new Dictionary<string, ScheduleCompact>();
            int hour1, hour2, minutes1, minutes2;
            hour1 = (int)Math.Truncate(startTime);
            hour2 = (int)Math.Truncate(endTime);
            minutes1 = (int)Math.Round(Math.Round((startTime - hour1) * 6)) * 10;
            minutes2 = (int)Math.Round(Math.Round((endTime - hour2) * 6)) * 10;


            double[] heatingSetPoints = new double[] { 10, 20 };
            double[] coolingSetPoints = new double[] { 28, 24 };

            double heatingSetpoint1 = heatingSetPoints[0];//16;
            double heatingSetpoint2 = heatingSetPoints[1];//20;

            double coolingSetpoint1 = coolingSetPoints[0];//28;
            double coolingSetpoint2 = coolingSetPoints[1];//25;

            //60 minutes earlier
            int hour1b = hour1 - 1;
            int minutes1b = minutes1;

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
                name = name + "_Heating Set Point Schedule",
                daysTimeValue = heatSP
            };
            Schedules.Add("HeatingSP", heatingSP);

            Dictionary<string, double> coolSPV1 = new Dictionary<string, double>(), coolSPV2 = new Dictionary<string, double>();
            coolSPV1.Add(hour1b + ":" + minutes1b, coolingSetpoint1);
            coolSPV1.Add(hour2 + ":" + minutes2, coolingSetpoint2);
            coolSPV1.Add("24:00", coolingSetpoint1);
            coolSP.Add(days1, coolSPV1);
            coolSPV2.Add("24:00", coolingSetpoint1);
            coolSP.Add(days2, coolSPV2);
            ScheduleCompact coolingSP = new ScheduleCompact()
            {
                name = name + "_Cooling Set Point Schedule",
                daysTimeValue = coolSP
            };
            Schedules.Add("CoolingSP", coolingSP);

            Dictionary<string, double> occupV1 = new Dictionary<string, double>(), occupV2 = new Dictionary<string, double>();
            occupV1.Add(hour1 + ":" + minutes1, 0);
            occupV1.Add(hour2 + ":" + minutes2, 1);
            occupV1.Add("24:00", 0);
            occupancyS.Add(days1, occupV1);
            occupV2.Add("24:00", 0);
            occupancyS.Add(days2, occupV2);
            ScheduleCompact occupSchedule = new ScheduleCompact()
            {
                name = name + "_Occupancy Schedule",
                daysTimeValue = occupancyS
            };
            Schedules.Add("Occupancy", occupSchedule);

            Dictionary<string, double> ventilV1 = new Dictionary<string, double>(), ventilV2 = new Dictionary<string, double>();
            ventilV1.Add(hour1 + ":" + minutes1, 0);
            ventilV1.Add(hour2 + ":" + minutes2, 1);
            ventilV1.Add("24:00", 0);
            ventilS.Add(days1, ventilV1);
            ventilV2.Add("24:00", 0);
            ventilS.Add(days2, ventilV2);
            ScheduleCompact ventilSchedule = new ScheduleCompact()
            {
                name = name + "_Ventilation Schedule",
                daysTimeValue = ventilS
            };
            Schedules.Add("Ventilation", ventilSchedule);

            double equipOffsetFraction = .1;
            Dictionary<string, double> lehgV1 = new Dictionary<string, double>(), lehgV2 = new Dictionary<string, double>();
            lehgV1.Add(hour1 + ":" + minutes1, equipOffsetFraction);
            lehgV1.Add(hour2 + ":" + minutes2, 1);
            lehgV1.Add("24:00", equipOffsetFraction);
            leHeatGain.Add(days1, lehgV1);
            lehgV2.Add("24:00", equipOffsetFraction);
            leHeatGain.Add(days2, lehgV2);
            ScheduleCompact lSchedule = new ScheduleCompact()
            {
                name = name + "_Lighting Schedule",
                daysTimeValue = leHeatGain
            };
            Schedules.Add("Light", lSchedule);

            ScheduleCompact eSchedule = new ScheduleCompact()
            {
                name = name + "_Electric Equipment Schedule",
                daysTimeValue = leHeatGain
            };
            Schedules.Add("Equipment", eSchedule);

            ScheduleCompact activity = new ScheduleCompact()
            {
                name = name + "_People Activity Schedule",
                daysTimeValue = new Dictionary<string, Dictionary<string, double>>() {
                    { "AllDays", new Dictionary<string, double>() {{"24:00", 125} } } }
            };
            Schedules.Add("Activity", activity);
        }
        public void GeneratePeopleLightEquipmentVentilationInfiltrationThermostat(double startTime, double endTime, double areaPerPerson, double lHG, double eHG, double infil)
        {
            CreateSchedules(startTime, endTime);
            listZones.ForEach(z => z.DayLightControl.AvailabilitySchedule = Schedules["Occupancy"].name);
            People = new People(areaPerPerson)
            {
                Name = "People_" + name,
                ZoneName = name,
                scheduleName = Schedules["Occupancy"].name,
                activityLvlSchedName = Schedules["Activity"].name
            };
            ZoneVentilation = new ZoneVentilation()
            {
                Name = "Ventilation_" + name,
                ZoneName = name,
                scheduleName = Schedules["Ventilation"].name,
                CalculationMethod = "Flow/Person"
            };
            
            Light = new Light(lHG)
            {
                Name = "Light_" + name,
                ZoneName = name,
                scheduleName = Schedules["Light"].name
            };
            ElectricEquipment = new ElectricEquipment(eHG)
            {
                Name = "Equipment_" + name,
                ZoneName = name,
                scheduleName = Schedules["Equipment"].name
            };
            Thermostat = new Thermostat()
            {
                name = name + "_Thermostat",
                ScheduleHeating = Schedules["HeatingSP"],
                ScheduleCooling = Schedules["CoolingSP"]
            };
            ZoneInfiltration = new ZoneInfiltration(infil)
            {
                Name = "Infiltration_" + name,
                ZoneName = name
            };
        }
    }
    [Serializable]
    public class People
    {
        public People() { }
        
        public string Name, ZoneName;
        public string scheduleName { get; set; }
        public string calculationMethod { get; set; }
        //public double numberOfPeople { get; set; }
        //public double peoplePerArea { get; set; }
        public double areaPerPerson { get; set; }
        public double fractionRadiant { get; set; }
        public double sensibleHeatFraction { get; set; }
        public string activityLvlSchedName { get; set; }
        public double c02genRate { get; set; }
        public string enableComfortWarnings { get; set; }
        public string meanRadiantTempCalcType { get; set; }
        public string surfaceName { get; set; }
        public string workEffSchedule { get; set; }
        public string clothingInsulationCalcMeth { get; set; }
        public string clothingInsulationCalcMethSched { get; set; }
        public string clothingInsulationSchedName { get; set; }
        public string airVelSchedName { get; set; }
        public string thermalComfModel1t { get; set; }
        public People(double aPP)
        {
            areaPerPerson = aPP;
            calculationMethod = "Area/Person";
            scheduleName = "Occupancy Schedule";
            fractionRadiant = 0.1;
            activityLvlSchedName = "People Activity Schedule";
            c02genRate = double.Parse("3.82E-8");
            enableComfortWarnings = "";
            meanRadiantTempCalcType = "ZoneAveraged";
            surfaceName = "";
            workEffSchedule = "Work Eff Sch";
            clothingInsulationCalcMeth = "DynamicClothingModelASHRAE55";
            clothingInsulationCalcMethSched = "";
            clothingInsulationSchedName = "";
            airVelSchedName = "Air Velo Sch";
            thermalComfModel1t = "Fanger";
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "People,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(ZoneName, "Zone or ZoneList Name"),
                Utility.IDFLineFormatter(scheduleName, "Schedule Name"),
                Utility.IDFLineFormatter(calculationMethod, "Number of People Calculation Method"),
                Utility.IDFLineFormatter("", "Number of People"),
                Utility.IDFLineFormatter("","People per Zone Floor Area {person/m2}"),
                Utility.IDFLineFormatter(areaPerPerson, "Zone Floor Area per Person {m2/person}"),
                Utility.IDFLineFormatter(fractionRadiant, "Fraction Radiant"),
                Utility.IDFLineFormatter("", "Sensible Heat Fraction"),
                Utility.IDFLineFormatter(activityLvlSchedName, "Activity Level Schedule Name"),
                Utility.IDFLineFormatter(c02genRate, "Carbon Dioxide Generation Rate {m3/s-W}"),
                Utility.IDFLineFormatter("", "Enable ASHRAE 55 Comfort Warnings"),
                Utility.IDFLineFormatter(meanRadiantTempCalcType, "Mean Radiant Temperature Calculation Type"),
                Utility.IDFLineFormatter("", "Surface Name/Angle Factor List Name"),
                Utility.IDFLineFormatter("Work Eff Sch", "Work Efficiency Schedule Name"),
                Utility.IDFLineFormatter(clothingInsulationCalcMeth, "Clothing Insulation Calculation Method"),
                Utility.IDFLineFormatter("", "Clothing Insulation Calculation Method Schedule Name"),
                Utility.IDFLineFormatter("", "Clothing Insulation Schedule Name"),
                Utility.IDFLineFormatter("Air Velo Sch", "Air Velocity Schedule Name"),
                Utility.IDFLastLineFormatter(thermalComfModel1t, "Thermal Comfort Model 1 Type")
            };
        }
    }
    [Serializable]
    public class Light
    {
        public string Name, ZoneName;
        public string scheduleName { get; set; }
        public Light() { }
        public string designLevelCalcMeth { get; set; }
        //public double lightingLevel { get; set; }
        public double wattsPerArea { get; set; }
        public double returnAirFraction { get; set; }
        public double fractionRadiant { get; set; }
        public double fractionVisible { get; set; }
        public double fractionReplaceable { get; set; }

        public Light(double wPA)
        {
            wattsPerArea = wPA;
            designLevelCalcMeth = "Watts/area";
            scheduleName = "Electric Equipment and Lighting Schedule";
            returnAirFraction = 0;
            fractionRadiant = 0.1;
            fractionVisible = 0.18;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "Lights,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(ZoneName , "Zone or ZoneList Name"),
                Utility.IDFLineFormatter(scheduleName , "Schedule Name"),
                Utility.IDFLineFormatter(designLevelCalcMeth , "Design Level Calculation Method"),
                Utility.IDFLineFormatter("", "Lighting Level {W}"),
                Utility.IDFLineFormatter(wattsPerArea, "Watts per Zone Floor Area {W/m2}"),
                Utility.IDFLineFormatter("","Watts per Person {W/person}"),
                Utility.IDFLineFormatter(returnAirFraction , "Return Air Fraction"),
                Utility.IDFLineFormatter(fractionRadiant , "Fraction Radiant"),
                Utility.IDFLineFormatter(fractionVisible , "Fraction Visible"),
                Utility.IDFLastLineFormatter("", "Fraction Replaceable")
            };
        }
    }
    [Serializable]
    public class ElectricEquipment
    {
        public ElectricEquipment() { }
        public string Name, ZoneName, scheduleName;

        public string designLevelCalcMeth { get; set; }
        //public double lightingLevel { get; set; }
        public double wattsPerArea { get; set; }

        public double fractionLatent { get; set; }
        public double fractionRadiant { get; set; }
        public double fractionLost { get; set; }

        public ElectricEquipment(double wPA)
        {
            wattsPerArea = wPA;
            designLevelCalcMeth = "Watts/area";
            scheduleName = "Electric Equipment and Lighting Schedule";
            fractionRadiant = 0.1;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "ElectricEquipment,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(ZoneName, "Zone or ZoneList Name"),
                Utility.IDFLineFormatter(scheduleName, "Schedule Name"),
                Utility.IDFLineFormatter(designLevelCalcMeth, "Design Level Calculation Method"),
                Utility.IDFLineFormatter("", "Design Level {W}"),
                Utility.IDFLineFormatter(wattsPerArea + "", "Watts per Zone Floor Area {W/m2}"),
                Utility.IDFLineFormatter("", "Watts per Person {W/person}"),
                Utility.IDFLineFormatter("", "Fraction Latent"),
                Utility.IDFLineFormatter(fractionRadiant, "Fraction Radiant"),
                Utility.IDFLastLineFormatter("", "Fraction Lost")
            };
        }

    }
    [Serializable]
    public abstract class ZoneHVAC
    {
        public ZoneHVAC() { }
        public Thermostat thermostat { get; set; }
        public ZoneHVAC(Thermostat thermostat)
        {
            this.thermostat = thermostat;
        }
    }
    [Serializable]
    public class ZoneVAV : ZoneHVAC
    {
        public ZoneVAV() { }
        VAV vav;
        Zone zone;
        public ZoneVAV(VAV v, Zone z, Thermostat t) : base(t)
        {
            zone = z;
            vav = v;
            thermostat = t;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>();


            info.Add("\r\nHVACTemplate:Zone:VAV,");
            info.Add("\t" + zone.name + ", \t\t\t\t!- Zone Name");
            info.Add("\t" + vav.name + ",\t\t\t\t!-Template VAV System Name");
            info.Add("\t" + thermostat.name + ",\t\t\t\t!-Template Thermostat Name");
            info.Add("\tautosize" + ",\t\t\t\t!-Supply Air Maximum Flow Rate {m3/s}");
            info.Add("\t" + ",\t\t\t\t!-Zone Heating Sizing Factor");
            info.Add("\t" + ",\t\t\t\t!-Zone Cooling Sizing Factor");
            info.Add("\tConstant" + ",\t\t\t\t!-Zone Minimum Air Flow Input Method");
            info.Add("\t0.2" + ",\t\t\t\t!-Constant Minimum Air Flow Fraction");
            info.Add("\t" + ",\t\t\t\t!-Fixed Minimum Air Flow Rate {m3/s}");
            info.Add("\t" + ",\t\t\t\t!-Minimum Air Flow Fraction Schedule Name");
            info.Add("\tFlow/Person" + ",\t\t\t\t!-Outdoor Air Method");
            info.Add("\t0.00944" + ",\t\t\t\t!-Outdoor Air Flow Rate per Person {m3/s}");
            info.Add("\t" + ",\t\t\t\t!-Outdoor Air Flow Rate per Zone Floor Area {m3/s-m2}");
            info.Add("\t" + ",\t\t\t\t!-Outdoor Air Flow Rate per Zone {m3/s}");
            info.Add("\tHotWater" + ",\t\t\t\t!-Reheat Coil Type");
            info.Add("\t" + ",\t\t\t\t!-Reheat Coil Availability Schedule Name");
            info.Add("\tReverse" + ",\t\t\t\t!-Damper Heating Action");
            info.Add("\t" + ",\t\t\t\t!-Maximum Flow per Zone Floor Area During Reheat {m3/s-m2}");
            info.Add("\t" + ",\t\t\t\t!-Maximum Flow Fraction During Reheat");
            info.Add("\t" + ",\t\t\t\t!-Maximum Reheat Air Temperature {C}");
            info.Add("\t" + ",\t\t\t\t!-Design Specification Outdoor Air Object Name for Control");
            info.Add("\t" + ",\t\t\t\t!-Supply Plenum Name");
            info.Add("\t" + ",\t\t\t\t!-Return Plenum Name");
            info.Add("\tNone" + ",\t\t\t\t!-Baseboard Heating Type");
            info.Add("\t" + ",\t\t\t\t!-Baseboard Heating Availability Schedule Name");
            info.Add("\tautosize" + ",\t\t\t\t!-Baseboard Heating Capacity {W}");
            info.Add("\tSystemSupplyAirTemperature" + ",\t\t\t\t!-Zone Cooling Design Supply Air Temperature Input Method");
            info.Add("\t" + ",\t\t\t\t!-Zone Cooling Design Supply Air Temperature {C}");
            info.Add("\t" + ",\t\t\t\t!-Zone Cooling Design Supply Air Temperature Difference {deltaC}");
            info.Add("\tSupplyAirTemperature" + ",\t\t\t\t!-Zone Heating Design Supply Air Temperature Input Method");
            info.Add("\t50" + ",\t\t\t\t!-Zone Heating Design Supply Air Temperature {C}");
            info.Add("\t" + ";\t\t\t\t!-Zone Heating Design Supply Air Temperature Difference {deltaC}");



            return info;
        }
    }
    [Serializable]
    public class ZoneFanCoilUnit : ZoneHVAC
    {
        public ZoneFanCoilUnit() { }
        Zone zone;
        public ZoneFanCoilUnit(Zone z, Thermostat thermostat) : base(thermostat)
        {
            zone = z;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Zone:FanCoil,",
                zone.name + ",                  !- Zone Name",
                thermostat.name + ",              !- Template Thermostat Name",
                "autosize,                !- Supply Air Maximum Flow Rate {m3/s}",
                ",                        !- Zone Heating Sizing Factor",
                ",                        !- Zone Cooling Sizing Factor",
                "flow/person,             !- Outdoor Air Method",
                "0.00944,                 !- Outdoor Air Flow Rate per Person {m3/s}",
                "0.0,                     !- Outdoor Air Flow Rate per Zone Floor Area {m3/s-m2}",
                "0.0,                     !- Outdoor Air Flow Rate per Zone {m3/s}",
                ",                        !- System Availability Schedule Name",
                "0.7,                     !- Supply Fan Total Efficiency",
                "75,                      !- Supply Fan Delta Pressure {Pa}",
                "0.9,                     !- Supply Fan Motor Efficiency",
                "1,                       !- Supply Fan Motor in Air Stream Fraction",
                "ChilledWater,            !- Cooling Coil Type",
                ",                        !- Cooling Coil Availability Schedule Name",
                "12.5,                    !- Cooling Coil Design Setpoint {C}",
                "HotWater,                !- Heating Coil Type",
                ",                        !- Heating Coil Availability Schedule Name",
                "50,                      !- Heating Coil Design Setpoint {C}",
                ",                        !- Dedicated Outdoor Air System Name",
                "SupplyAirTemperature,    !- Zone Cooling Design Supply Air Temperature Input Method",
                ",                        !- Zone Cooling Design Supply Air Temperature Difference {deltaC}",
                "SupplyAirTemperature,    !- Zone Heating Design Supply Air Temperature Input Method",
                ",                        !- Zone Heating Design Supply Air Temperature Difference {deltaC}",
                ",                        !- Design Specification Outdoor Air Object Name",
                ",                        !- Design Specification Zone Air Distribution Object Name",
                "ConstantFanVariableFlow, !- Capacity Control Method",
                ",                        !- Low Speed Supply Air Flow Ratio",
                ",                        !- Medium Speed Supply Air Flow Ratio",
                "Occupancy Schedule,      !- Outdoor Air Schedule Name",
                "None,                    !- Baseboard Heating Type",
                ",                        !- Baseboard Heating Availability Schedule Name",
                "Autosize; !-Baseboard Heating Capacity { W}"
            };

            return info;
        }
    }
    [Serializable]
    public class ZoneBaseBoardHeat:ZoneHVAC 
    {
        public ZoneBaseBoardHeat() { }
        Zone zone;

        public ZoneBaseBoardHeat(Zone z, Thermostat t)
        {
            zone = z;
            thermostat = t;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Zone:BaseBoardHeat,",
                zone.name + ", !-Zone Name",
                thermostat.name + ", !-Template Thermostat Name",
                "1.2, !-Zone Heating Sizing Factor",
                "Electric, !-Baseboard Heating Type",
                ", !-Baseboard Heating Availability Schedule Name",
                "Autosize, !-Baseboard Heating Capacity { W}",
                ", !-Dedicated Outdoor Air System Name",
                "flow/person , !-Outdoor Air Method",
                "0.00944, !-Outdoor Air Flow Rate per Person { m3 / s}",
                "0.0, !-Outdoor Air Flow Rate per Zone Floor Area { m3 / s - m2}",
                "0.0, !-Outdoor Air Flow Rate per Zone { m3 / s}",
                ", !-Design Specification Outdoor Air Object Name",
                "; !-Design Specification Zone Air Distribution Object Name"
            };

            return info;
        }
    }
    [Serializable]
    public class ZoneIdealLoad : ZoneHVAC
    {
        Zone zone;
        public ZoneIdealLoad() { }
        public ZoneIdealLoad(Zone z, Thermostat thermostat) : base(thermostat)
        {
            zone = z;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("HVACTemplate:Zone:IdealLoadsAirSystem,");
            info.Add("\t" + zone.name + ",\t\t\t\t\t\t!- Zone Name");
            info.Add("\t" + thermostat.name + ", \t\t\t\t!- Template Thermostat Name");
            info.Add("\t, \t\t\t\t!- System Availability Schedule Name");
            info.Add("\t50, \t\t\t\t!- Maximum Heating Supply Air Temperature {C}");
            info.Add("\t13, \t\t\t\t!- Minimum Cooling Supply Air Temperature {C}");
            info.Add("\t0.0156, \t\t\t\t!- Maximum Heating Supply Air Humidity Ratio {kgWater/kgDryAir}");
            info.Add("\t0.0077, \t\t\t\t!- Minimum Cooling Supply Air Humidity Ratio {kgWater/kgDryAir}");
            info.Add("\tNoLimit, \t\t\t\t!- Heating Limit");
            info.Add("\t, \t\t\t\t!- Maximum Heating Air Flow Rate {m3/s}");
            info.Add("\t, \t\t\t\t!- Maximum Sensible Heating Capacity {W}");
            info.Add("\tNoLimit, \t\t\t\t!- Cooling Limit");
            info.Add("\t, \t\t\t\t!- Maximum Cooling Air Flow Rate {m3/s}");
            info.Add("\t, \t\t\t\t!- Maximum Total Cooling Capacity {W}");
            info.Add("\t, \t\t\t\t!- Heating Availability Schedule Name");
            info.Add("\t, \t\t\t\t!- Cooling Availability Schedule Name");
            info.Add("\tConstantSensibleHeatRatio, \t\t\t\t!- Dehumidification Control Type");
            info.Add("\t0.7, \t\t\t\t!- Cooling Sensible Heat Ratio {dimensionless}");
            info.Add("\t60, \t\t\t\t!- Dehumidification Setpoint {percent}");
            info.Add("\tNone, \t\t\t\t!- Humidification Control Type");
            info.Add("\t30, \t\t\t\t!- Humidification Setpoint {percent}");
            info.Add("\tNone, \t\t\t\t!- Outdoor Air Method");
            info.Add("\t0.00944, \t\t\t\t!- Outdoor Air Flow Rate per Person {m3/s}");
            info.Add("\t, \t\t\t\t!- Outdoor Air Flow Rate per Zone Floor Area {m3/s-m2}");
            info.Add("\t, \t\t\t\t!- Outdoor Air Flow Rate per Zone {m3/s}");
            info.Add("\t, \t\t\t\t!- Design Specification Outdoor Air Object Name");
            info.Add("\tNone, \t\t\t\t!- Demand Controlled Ventilation Type");
            info.Add("\tNoEconomizer, \t\t\t\t!- Outdoor Air Economizer Type");
            info.Add("\tNone, \t\t\t\t!- Heat Recovery Type");
            info.Add("\t0.7, \t\t\t\t!- Sensible Heat Recovery Effectiveness {dimensionless}");
            info.Add("\t0.65; \t\t\t\t!- Latent Heat Recovery Effectiveness {dimensionless} \r\n");
            return info;
        }
    }
    [Serializable]
    public class VAV
    {
        public string name = "VAV";
        public VAV()
        {
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\r\n!-   ===========  ALL OBJECTS IN CLASS: HVACTEMPLATE:SYSTEM:VAV ===========\r\n");

            info.Add("\r\nHVACTemplate:System:VAV,");
            info.Add("\t" + name + ", \t\t\t\t!- Name");
            info.Add("\t" + ", \t\t\t\t!- System Availability Schedule Name");
            info.Add("\tautosize" + ", \t\t\t\t!- Supply Fan Maximum Flow Rate {m3/s}");
            info.Add("\tautosize" + ", \t\t\t\t!- Supply Fan Minimum Flow Rate {m3/s}");
            info.Add("\t0.7" + ", \t\t\t\t!- Supply Fan Total Efficiency");
            info.Add("\t1000" + ", \t\t\t\t!- Supply Fan Delta Pressure {Pa}");
            info.Add("\t0.9" + ", \t\t\t\t!- Supply Fan Motor Efficiency");
            info.Add("\t1" + ", \t\t\t\t!- Supply Fan Motor in Air Stream Fraction");
            info.Add("\tChilledWater" + ", \t\t\t\t!- Cooling Coil Type");
            info.Add("\t" + ", \t\t\t\t!- Cooling Coil Availability Schedule Name");
            info.Add("\t" + ", \t\t\t\t!- Cooling Coil Setpoint Schedule Name");
            info.Add("\t12.8" + ", \t\t\t\t!- Cooling Coil Design Setpoint {C}");
            info.Add("\tHotWater" + ", \t\t\t\t!- Heating Coil Type");
            info.Add("\t" + ", \t\t\t\t!- Heating Coil Availability Schedule Name");
            info.Add("\t" + ", \t\t\t\t!- Heating Coil Setpoint Schedule Name");
            info.Add("\t10" + ", \t\t\t\t!- Heating Coil Design Setpoint {C}");
            info.Add("\t0.8" + ", \t\t\t\t!- Gas Heating Coil Efficiency");
            info.Add("\t" + ", \t\t\t\t!- Gas Heating Coil Parasitic Electric Load {W}");
            info.Add("\tNone" + ", \t\t\t\t!- Preheat Coil Type");
            info.Add("\t" + ", \t\t\t\t!- Preheat Coil Availability Schedule Name");
            info.Add("\t" + ", \t\t\t\t!- Preheat Coil Setpoint Schedule Name");
            info.Add("\t7.2" + ", \t\t\t\t!- Preheat Coil Design Setpoint {C}");
            info.Add("\t0.8" + ", \t\t\t\t!- Gas Preheat Coil Efficiency");
            info.Add("\t" + ", \t\t\t\t!- Gas Preheat Coil Parasitic Electric Load {W}");
            info.Add("\tautosize" + ", \t\t\t\t!- Maximum Outdoor Air Flow Rate {m3/s}");
            info.Add("\tautosize" + ", \t\t\t\t!- Minimum Outdoor Air Flow Rate {m3/s}");
            info.Add("\tProportionalMinimum" + ", \t\t\t\t!- Minimum Outdoor Air Control Type");
            info.Add("\t" + ", \t\t\t\t!- Minimum Outdoor Air Schedule Name");
            info.Add("\tNoEconomizer" + ", \t\t\t\t!- Economizer Type");
            info.Add("\tNoLockout" + ", \t\t\t\t!- Economizer Lockout");
            info.Add("\t" + ", \t\t\t\t!- Economizer Upper Temperature Limit {C}");
            info.Add("\t" + ", \t\t\t\t!- Economizer Lower Temperature Limit {C}");
            info.Add("\t" + ", \t\t\t\t!- Economizer Upper Enthalpy Limit {J/kg}");
            info.Add("\t" + ", \t\t\t\t!- Economizer Maximum Limit Dewpoint Temperature {C}");
            info.Add("\t" + ", \t\t\t\t!- Supply Plenum Name");
            info.Add("\t" + ", \t\t\t\t!- Return Plenum Name");
            info.Add("\tDrawThrough" + ", \t\t\t\t!- Supply Fan Placement");
            info.Add("\tInletVaneDampers" + ", \t\t\t\t!- Supply Fan Part-Load Power Coefficients");
            info.Add("\tStayOff" + ", \t\t\t\t!- Night Cycle Control");
            info.Add("\t" + ", \t\t\t\t!- Night Cycle Control Zone Name");
            info.Add("\tNone" + ", \t\t\t\t!- Heat Recovery Type");
            info.Add("\t0.7" + ", \t\t\t\t!- Sensible Heat Recovery Effectiveness");
            info.Add("\t0.65" + ", \t\t\t\t!- Latent Heat Recovery Effectiveness");
            info.Add("\tNone" + ", \t\t\t\t!- Cooling Coil Setpoint Reset Type");
            info.Add("\tNone" + ", \t\t\t\t!- Heating Coil Setpoint Reset Type");
            info.Add("\tNone" + ", \t\t\t\t!- Dehumidification Control Type");
            info.Add("\t" + ", \t\t\t\t!- Dehumidification Control Zone Name");
            info.Add("\t60" + ", \t\t\t\t!- Dehumidification Setpoint {percent}");
            info.Add("\tNone" + ", \t\t\t\t!- Humidifier Type");
            info.Add("\t" + ", \t\t\t\t!- Humidifier Availability Schedule Name");
            info.Add("\t0.000001" + ", \t\t\t\t!- Humidifier Rated Capacity {m3/s}");
            info.Add("\tautosize" + ", \t\t\t\t!- Humidifier Rated Electric Power {W}");
            info.Add("\t" + ", \t\t\t\t!- Humidifier Control Zone Name");
            info.Add("\t30" + ", \t\t\t\t!- Humidifier Setpoint {percent}");
            info.Add("\tNonCoincident" + ", \t\t\t\t!- Sizing Option");
            info.Add("\tNo" + ", \t\t\t\t!- Return Fan");
            info.Add("\t0.7" + ", \t\t\t\t!- Return Fan Total Efficiency");
            info.Add("\t500" + ", \t\t\t\t!- Return Fan Delta Pressure {Pa}");
            info.Add("\t0.9" + ", \t\t\t\t!- Return Fan Motor Efficiency");
            info.Add("\t1" + ", \t\t\t\t!- Return Fan Motor in Air Stream Fraction");
            info.Add("\tInletVaneDampers" + "; \t\t\t\t!- Return Fan Part-Load Power Coefficients");

            return info;
        }


    }
    [Serializable]
    public class ChilledWaterLoop
    {
        string name = "Chilled Water Loop";

        public ChilledWaterLoop()
        {
            
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Plant:ChilledWaterLoop,",
                name + ",      !- Name",
                ",                        !- Pump Schedule Name",
                "Intermittent,            !- Pump Control Type",
                "Default,                 !- Chiller Plant Operation Scheme Type",
                ",                        !- Chiller Plant Equipment Operation Schemes Name",
                ",                        !- Chilled Water Setpoint Schedule Name",
                "7.22,                    !- Chilled Water Design Setpoint {C}",
                "VariablePrimaryNoSecondary,  !- Chilled Water Pump Configuration",
                "179352,                  !- Primary Chilled Water Pump Rated Head {Pa}",
                "179352,                  !- Secondary Chilled Water Pump Rated Head {Pa}",
                "Default,                 !- Condenser Plant Operation Scheme Type",
                ",                        !- Condenser Equipment Operation Schemes Name",
                "OutdoorWetBulbTemperature,  !- Condenser Water Temperature Control Type",
                ",                        !- Condenser Water Setpoint Schedule Name",
                "29.4,                    !- Condenser Water Design Setpoint {C}",
                "179352,                  !- Condenser Water Pump Rated Head {Pa}",
                "None,                    !- Chilled Water Setpoint Reset Type",
                "12.2,                    !- Chilled Water Setpoint at Outdoor Dry-Bulb Low {C}",
                "15.6,                    !- Chilled Water Reset Outdoor Dry-Bulb Low {C}",
                "6.7,                     !- Chilled Water Setpoint at Outdoor Dry-Bulb High {C}",
                "26.7,                    !- Chilled Water Reset Outdoor Dry-Bulb High {C}",
                "SinglePump,              !- Chilled Water Primary Pump Type",
                "SinglePump,              !- Chilled Water Secondary Pump Type",
                "SinglePump,              !- Condenser Water Pump Type",
                "Yes,                     !- Chilled Water Supply Side Bypass Pipe",
                "Yes,                     !- Chilled Water Demand Side Bypass Pipe",
                "Yes,                     !- Condenser Water Supply Side Bypass Pipe",
                "Yes,                     !- Condenser Water Demand Side Bypass Pipe",
                "Water,                   !- Fluid Type",
                "6.67,                    !- Loop Design Delta Temperature {deltaC}",
                ",                        !- Minimum Outdoor Dry Bulb Temperature {C}",
                "SequentialLoad,          !- Chilled Water Load Distribution Scheme",
                "SequentialLoad; !-Condenser Water Load Distribution Scheme"
            };

            return info;
        }

    }
    [Serializable]
    public class Chiller
    {
        string  name = "Main Chiller";
        double chillerCOP;
        public Chiller() { }

        public Chiller(double COP)
        {
            chillerCOP = COP;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "HVACTemplate:Plant:Chiller,",
                name + ",            !-Name",
                "ElectricCentrifugalChiller,  !-Chiller Type",
                "autosize,                !-Capacity { W}",
                chillerCOP + ",                     !-Nominal COP { W / W}",
                "WaterCooled,             !-Condenser Type",
                "1,                       !-Priority",
                "1,                       !-Sizing Factor",
                "0.1,                     !-Minimum Part Load Ratio",
                "1.1,                     !-Maximum Part Load Ratio",
                "0.9,                     !-Optimum Part Load Ratio",
                "0.2,                     !-Minimum Unloading Ratio",
                "2; !-Leaving Chilled Water Lower Temperature Limit { C}"
            };
            return info;
        }

    }
    [Serializable]
    public class Tower
    {
        string name = "Main Tower";

        public Tower()
        {
            
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>()
            {
                "\r\nHVACTemplate:Plant:Tower,",
                "\t" + name + ", \t\t\t\t!- Name",
                "\tSingleSpeed" + ",\t\t\t\t!-Tower Type",
                "\tautosize" + ", \t\t\t\t!-High Speed Nominal Capacity {W}",
                "\tautosize" + ",\t\t\t\t!-High Speed Fan Power {W}",
                "\tautosize" + ", \t\t\t\t!-Low Speed Nominal Capacity {W}",
                "\tautosize" + ", \t\t\t\t!-Low Speed Fan Power {W}",
                "\tautosize" + ", \t\t\t\t!-Free Convection Capacity {W}",
                "\t1" + ", \t\t\t\t!-Priority",
                "\t1.2" + "; \t\t\t\t!-Sizing Factor"
            };
            return info;
        }

    }
    [Serializable]
    public class HotWaterLoop
    {
        string name = "Hot Water Loop";

        public HotWaterLoop()
        {
            
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>() {
                "HVACTemplate:Plant:HotWaterLoop,",
                name + ",          !- Name",
                ",                        !- Pump Schedule Name",
                "Intermittent,            !- Pump Control Type",
                "Default,                 !- Hot Water Plant Operation Scheme Type",
                ",                        !- Hot Water Plant Equipment Operation Schemes Name",
                ",                        !- Hot Water Setpoint Schedule Name",
                "82,                      !- Hot Water Design Setpoint {C}",
                "VariableFlow,            !- Hot Water Pump Configuration",
                "179352,                  !- Hot Water Pump Rated Head {Pa}",
                "None,                    !- Hot Water Setpoint Reset Type",
                "82.2,                    !- Hot Water Setpoint at Outdoor Dry-Bulb Low {C}",
                "-6.7,                    !- Hot Water Reset Outdoor Dry-Bulb Low {C}",
                "65.6,                    !- Hot Water Setpoint at Outdoor Dry-Bulb High {C}",
                "10,                      !- Hot Water Reset Outdoor Dry-Bulb High {C}",
                "SinglePump,              !- Hot Water Pump Type",
                "Yes,                     !- Supply Side Bypass Pipe",
                "Yes,                     !- Demand Side Bypass Pipe",
                "Water,                   !- Fluid Type",
                "11,                      !- Loop Design Delta Temperature {deltaC}",
                ",                        !- Maximum Outdoor Dry Bulb Temperature {C}",
                "SequentialLoad; !-Load Distribution Scheme"
            };

            return info;
        }
    }
    [Serializable]
    public class Boiler
    {
        public string name;
        public double boilerEfficiency;
        public string fuelType;

        public Boiler() { }
        public Boiler(double efficiency, string fuelType)
        {
            name = "Main Boiler";
            boilerEfficiency = efficiency;
            this.fuelType = fuelType;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>() {
                "HVACTemplate:Plant:Boiler,",
                name + ",             !-Name",
                "HotWaterBoiler,          !-Boiler Type",
                "autosize,                !-Capacity { W}",
                boilerEfficiency + ",                     !-Efficiency",
                Utility.IDFLineFormatter(fuelType,  "Fuel Type"),
                "1,                       !-Priority",
                "1.2,                     !-Sizing Factor",
                "0.1,                     !-Minimum Part Load Ratio",
                "1.1,                     !-Maximum Part Load Ratio",
                "0.9,                     !-Optimum Part Load Ratio",
                "99.9; !-Water Outlet Upper Temperature Limit { C}"
            };

            return info;
        }
    }
    [Serializable]
    public class Thermostat
    {
        public Thermostat() { }
        public string name { get; set; }
        public double heatingSetPoint { get; set; }
        public double coolingSetPoint { get; set; }
        public ScheduleCompact ScheduleHeating { get; set; }
        public ScheduleCompact ScheduleCooling { get; set; }

        public Thermostat(string n, double heatingSP, double coolingSP, ScheduleCompact scheduleHeating, ScheduleCompact scheduleCooling)
        {
            heatingSetPoint = heatingSP; ScheduleHeating = scheduleHeating; ScheduleCooling = scheduleCooling;
            coolingSetPoint = coolingSP;
            name = n;
        }
    }
    [Serializable]
    public class ZoneVentilation
    {
        public string Name;
        public string ZoneName;
        public string scheduleName = "Ventilation Schedule";
        public string CalculationMethod = "AirChanges/Hour";
        public double DesignFlowRate = 0;
        public double FlowRateZoneArea = 0;
        public double FlowRatePerson = 0.00944;
        public double airChangesHour = 1;
        public string VentilationType = "Balanced";
        public double FanPressure = 1;
        public double FanEfficiency = 1;
        public double ConstantCoefficient = 1;
        public double TemperatureCoefficient = 0;
        public double VelocityCoefficient = 0;
        public double VelocitySqCoefficient = 0;
        public double minIndoorTemp = -100;
        public double maxIndoorTemp = 100;
        public string minIndoorTempSchedule = " ";
        public string maxIndoorTempSchedule = " ";
        public double deltaC = 1;
        public ZoneVentilation(double acH)
        {
            scheduleName = "Ventilation Schedule";
            airChangesHour = acH;
        }
        public ZoneVentilation()
        {

        }

        public ZoneVentilation(Zone zone, bool natural)
        {
            if (natural)
            {
                Name = zone.name + "-Natural Ventilation";
                ZoneName = zone.name;
                VentilationType = "Natural";
                minIndoorTemp = 22;
                maxIndoorTemp = 26;
            }
        }

        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "ZoneVentilation:DesignFlowRate,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(ZoneName, "Zone or ZoneList Name"),
                Utility.IDFLineFormatter(scheduleName, "Schedule Name"),
                Utility.IDFLineFormatter(CalculationMethod, "Design Flow Rate Calculation Method"),
                Utility.IDFLineFormatter(DesignFlowRate, "Design Flow Rate {m3/s}"),
                Utility.IDFLineFormatter(FlowRateZoneArea, "Flow Rate per Zone Floor Area {m3/s-m2}"),
                Utility.IDFLineFormatter(FlowRatePerson, "Flow Rate per Person {m3/s-person}"),
                Utility.IDFLineFormatter(airChangesHour, "Air Changes per Hour {1/hr}"),
                Utility.IDFLineFormatter(VentilationType, "Ventilation Type"),
                Utility.IDFLineFormatter(FanPressure, "Fan Pressure Rise {Pa}"),
                Utility.IDFLineFormatter(FanEfficiency, "Fan Total Efficiency"),
                Utility.IDFLineFormatter(ConstantCoefficient, "Constant Term Coefficient"),
                Utility.IDFLineFormatter(TemperatureCoefficient, "Temperature Term Coefficient"),
                Utility.IDFLineFormatter(VelocityCoefficient, "Velocity Term Coefficient"),
                Utility.IDFLineFormatter(VelocitySqCoefficient, "Velocity Squared Term Coefficient"),
                Utility.IDFLineFormatter(minIndoorTemp, "Minimum Indoor Temperature {C}"),
                Utility.IDFLineFormatter(minIndoorTempSchedule, "Maximum Indoor Temperature Schedule"),
                Utility.IDFLineFormatter(maxIndoorTemp, "Maximum Indoor Temperature {C}"),
                Utility.IDFLineFormatter(maxIndoorTempSchedule, "Maximum Indoor Temperature Schedule"),
                Utility.IDFLastLineFormatter(deltaC, "Delta Temperature { deltaC}")
            };
        }
    }
    [Serializable]
    public class ZoneInfiltration
    {
        public string Name, ZoneName;
        public double airChangesHour { get; set; }
        public double constantTermCoeff { get; set; }
        public double temperatureTermCoef { get; set; }
        public double velocityTermCoef { get; set; }
        public double velocitySquaredTermCoef { get; set; }

        public ZoneInfiltration(double acH)
        {
            airChangesHour = acH;
            constantTermCoeff = 0.606;
            temperatureTermCoef = 0.036359996;
            velocityTermCoef = 0.1177165;
            velocitySquaredTermCoef = 0;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "ZoneInfiltration:DesignFlowRate,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(ZoneName, "Zone or ZoneList Name"),
                Utility.IDFLineFormatter("Space Infiltration Schedule", "Schedule Name"),
                Utility.IDFLineFormatter("AirChanges/Hour", "Design Flow Rate Calculation Method"),
                Utility.IDFLineFormatter("", "Design Flow Rate { m3 / s}"),
                Utility.IDFLineFormatter("", "Flow per Zone Floor Area { m3 / s - m2}"),
                Utility.IDFLineFormatter("", "Flow per Exterior Surface Area { m3 / s - m2}"),
                Utility.IDFLineFormatter(airChangesHour, "Air Changes per Hour { 1 / hr}"),
                Utility.IDFLineFormatter(constantTermCoeff, "Constant Term Coefficient"),
                Utility.IDFLineFormatter(temperatureTermCoef, "Temperature Term Coefficient"),
                Utility.IDFLineFormatter(velocityTermCoef, "!-Velocity Term Coefficient"),
                Utility.IDFLastLineFormatter(velocitySquaredTermCoef, "Velocity Squared Term Coefficient")
            };
        }
    }
    [Serializable]
    public class ShadingOverhang
    {
        public ShadingOverhang() { }
        public BuildingSurface face { get; set; }
        public XYZList listVertice { get; set; }

        public ShadingOverhang(BuildingSurface face1)
        {
            face = face1;

            switch (face.direction)
            {
                case Direction.North:
                    listVertice = createShadingY(face.verticesList, face.sl).reverse();
                    break;
                case Direction.South:
                    listVertice = createShadingY(face.verticesList, -face.sl).reverse();
                    break;
                case Direction.East:
                    listVertice = createShadingX(face.verticesList, face.sl).reverse();
                    break;
                case Direction.West:
                    listVertice = createShadingX(face.verticesList, -face.sl).reverse();
                    break;
            }

            XYZList createShadingY(XYZList listVertices, double sl)
            {
                XYZ P1 = listVertices.xyzs.ElementAt(0);
                XYZ P2 = listVertices.xyzs.ElementAt(1);
                XYZ P3 = listVertices.xyzs.ElementAt(2);
                XYZ P4 = listVertices.xyzs.ElementAt(3);

                double shadingLength = sl;

                XYZ pmid = new XYZ((P1.X + P3.X) / 2, P1.Y, (P1.Z + P3.Z) / 2);
                double Y = pmid.Y;
                double Z = P1.Z;

                XYZ Vertice1 = new XYZ(P2.X, Y, Z);
                XYZ Vertice2 = new XYZ(P2.X + shadingLength, Y + shadingLength, Z);
                XYZ Vertice3 = new XYZ(P1.X - shadingLength, Y + shadingLength, Z);
                XYZ Vertice4 = new XYZ(P1.X, Y, Z);

                return new XYZList(new List<XYZ>() { Vertice1, Vertice2, Vertice3, Vertice4 });
            }

            XYZList createShadingX(XYZList listVertices, double sl)
            {
                XYZ P1 = listVertices.xyzs.ElementAt(0);
                XYZ P2 = listVertices.xyzs.ElementAt(1);
                XYZ P3 = listVertices.xyzs.ElementAt(2);
                XYZ P4 = listVertices.xyzs.ElementAt(3);

                double shadingLength = sl;

                XYZ pmid = new XYZ((P1.X + P3.X) / 2, P1.Y, (P1.Z + P3.Z) / 2);
                double X = pmid.X;
                double Z = P1.Z;

                XYZ Vertice1 = new XYZ(X, P2.Y, Z);
                XYZ Vertice2 = new XYZ(X + shadingLength, P2.Y - shadingLength, Z);
                XYZ Vertice3 = new XYZ(X + shadingLength, P1.Y + shadingLength, Z);
                XYZ Vertice4 = new XYZ(X, P1.Y, Z);

                return new XYZList(new List<XYZ>() { Vertice1, Vertice2, Vertice3, Vertice4 });
            }
        }



        public List<string> shadingInfo()
        {

            List<string> info = new List<string>();

            info.Add("Shading:Zone:Detailed,");
            info.Add("\t" + "Shading_On_" + face.name + ",\t!- Name");
            info.Add("\t" + face.name + ",\t!-Base Surface Name)");
            info.Add("\t,\t\t\t\t\t\t!-Transmittance Schedule Name");



            info.AddRange(listVertice.verticeInfo());

            return info;
        }
    }
    [Serializable]
    public class OverhangProjection
    {
        public Fenestration window;
        public double depthf;

        public OverhangProjection() { }
        public OverhangProjection(Fenestration win, double df)
        {
            window = win;
            depthf = df;

        }

        public List<string> OverhangInfo()
        {

            List<string> info = new List<string>();

            info.Add("Shading:Overhang:Projection,");
            info.Add("\t" + "Shading_On_" + window.surfaceType + "_On_" + window.face.name + ",\t!- Name");
            info.Add("\t" + window.surfaceType + "_On_" + window.face.name + ",\t!-Window or Door Name");
            info.Add("\t0,\t\t\t\t\t\t!-Height above Window or Door {m}");
            info.Add("\t90,\t\t\t\t\t\t!-Tilt Angle from Window/Door {deg}");
            info.Add("\t.2,\t\t\t\t\t\t!-Left extension from Window/Door Width {m}");
            info.Add("\t.2,\t\t\t\t\t\t!-Right extension from Window/Door Width {m}");
            info.Add("\t" + depthf + ";\t\t\t\t\t\t!-Depth as Fraction of Window/Door Height {m}");


            return info;
        }

    }
    [Serializable]
    public class ScheduleYearly
    {
        public ScheduleYearly() { }

        public string name { get; set; }
        public ScheduleWeekly scheduleWeekly { get; set; }
        public ScheduleLimits scheduleLimits { get; set; }
        public int startDay { get; set; }
        public int startMonth { get; set; }
        public int endDay { get; set; }
        public int endMonth { get; set; }
        public List<string> writeSchedule()
        {
            List<string> info = new List<string>();
            info.Add("Schedule:Year,");
            info.Add(name + ",\t\t\t\t!-Name");

            if (scheduleLimits != null) info.Add(scheduleLimits.name + ", \t\t\t\t!-Schedule Type Limits Name");
            else info.Add(", \t\t\t\t!-Schedule Type Limits Name");

            info.Add(scheduleWeekly.name + ",  \t\t\t\t!- Schedule:Week Name 1");
            info.Add(startMonth + ",  \t\t\t\t!- Start Month 1");
            info.Add(startDay + ",  \t\t\t\t!- Start Day 1");
            info.Add(endMonth + ",  \t\t\t\t!- End Month 1");
            info.Add(endDay + ";  \t\t\t\t!- End Day 1");
            return info;
        }
        public ScheduleYearly(ScheduleWeekly schedule)
        {
            scheduleWeekly = schedule;
            startDay = 1; startMonth = 1;
            endMonth = 12; endDay = 31;
        }
    }
    [Serializable]
    public class ScheduleWeekly
    {
        public ScheduleWeekly() { }
        public string name { get; set; }
        public ScheduleDaily weekday { get; set; }
        public ScheduleDaily saturday { get; set; }
        public ScheduleDaily sunday { get; set; }
        public ScheduleDaily holiday { get; set; }
        public ScheduleDaily summerDesignday { get; set; }
        public ScheduleDaily winterDesignday { get; set; }
        public ScheduleDaily customDay1 { get; set; }
        public ScheduleDaily customDay2 { get; set; }

        public ScheduleWeekly(ScheduleDaily schedule)
        {
            weekday = schedule; saturday = schedule; sunday = schedule;
            holiday = schedule; summerDesignday = schedule; winterDesignday = schedule;
            customDay1 = schedule; customDay2 = schedule;
        }

        public ScheduleWeekly(ScheduleDaily week, ScheduleDaily weekend, ScheduleDaily hol)
        {
            weekday = week; saturday = weekend; sunday = weekend;
            holiday = hol;
            summerDesignday = week; winterDesignday = week;
            customDay1 = hol; customDay2 = hol;
        }


        public List<string> writeSchedule()
        {
            List<string> info = new List<string>();
            info.Add("Schedule:Week:Daily,");
            info.Add(name + ", \t\t\t\t!-Name");
            info.Add(sunday.name + ", \t\t\t\t!- Sunday Schedule:Day Name");
            info.Add(weekday.name + ",  \t\t\t\t!- Monday Schedule:Day Name");
            info.Add(weekday.name + ",  \t\t\t\t!- Tuesday Schedule:Day Name");
            info.Add(weekday.name + ",  \t\t\t\t!- Wednesday Schedule:Day Name");
            info.Add(weekday.name + ",  \t\t\t\t!- Thursday Schedule:Day Name");
            info.Add(weekday.name + ",  \t\t\t\t!- Friday Schedule:Day Name");
            info.Add(saturday.name + ",  \t\t\t\t!- Saturday Schedule:Day Name");
            info.Add(holiday.name + ", \t\t\t\t!- Holiday Schedule:Day Name");
            info.Add(summerDesignday.name + ", \t\t\t\t!- SummerDesignDay Schedule:Day Name");
            info.Add(winterDesignday.name + ",  \t\t\t\t!- WinterDesignDay Schedule:Day Name");
            info.Add(customDay1.name + ",  \t\t\t\t!- CustomDay1 Schedule:Day Name");
            info.Add(customDay2.name + ";  \t\t\t\t!- CustomDay2 Schedule:Day Name");
            return info;
        }
    }
    [Serializable]
    public class ScheduleDaily
    {
        public string name { get; set; }
        public ScheduleLimits scheduleLimits { get; set; }
        public string interpolate { get; set; }
        public int hour1 { get; set; }
        public int minutes1 { get; set; }
        public double value1 { get; set; }
        public int hour2 { get; set; }
        public int minutes2 { get; set; }
        public double value2 { get; set; }

        public ScheduleDaily()
        {
            
        }

        public List<String> writeSchedule()
        {
            List<string> info = new List<string>();
            info.Add("Schedule:Day:Interval,");
            info.Add(name + ",\t\t\t\t!-Name");

            if (scheduleLimits != null) info.Add(scheduleLimits.name + ", \t\t\t\t!-Schedule Type Limits Name");
            else info.Add(", \t\t\t\t!-Schedule Type Limits Name");

            info.Add(interpolate + ",  \t\t\t\t!- Interpolate to Timestep");

            if (!(hour1 == 0 && minutes1 == 0))
            {
                if (minutes1 < 10)
                {
                    info.Add(hour1 + ":0" + minutes1 + ",  \t\t\t\t!- Time 1 {hh:mm}");
                }
                else
                {
                    info.Add(hour1 + ":" + minutes1 + ",  \t\t\t\t!- Time 1 {hh:mm}");
                }
                info.Add(value1 + ",  \t\t\t\t!- Value Until Time 1");
            }
            if (!(hour2 == 0 && minutes2 == 0))
            {
                if (minutes2 < 10)
                {
                    info.Add(hour2 + ":0" + minutes2 + ",  \t\t\t\t!- Time 2 {hh:mm}");
                }
                else
                {
                    info.Add(hour2 + ":" + minutes2 + ",  \t\t\t\t!- Time 2 {hh:mm}");
                }
                info.Add(value2 + ",  \t\t\t\t!- Value Until Time 2");
            }
            info.Add("24:00" + ",  \t\t\t\t!- Time end {hh:mm}");
            info.Add(value1 + ";  \t\t\t\t!- Value Until Time end");
            return info;
        }

    }
    [Serializable]
    public class ScheduleLimits
    {
        public string name { get; set; }
        public double lowerLimit { get; set; }
        public double upperLimit { get; set; }
        public string numericType { get; set; }
        public string unitType { get; set; }

        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("ScheduleTypeLimits,");
            info.Add(name + ",\t\t\t\t!-Name");
            info.Add(lowerLimit + ", \t\t\t\t!- Lower Limit Value");

            if (upperLimit > lowerLimit) info.Add(upperLimit + ",  \t\t\t\t!- Upper Limit Value");
            else info.Add(",  \t\t\t\t!- Upper Limit Value");

            info.Add(numericType + ",  \t\t\t\t!- Numeric Type");
            info.Add(unitType + ";  \t\t\t\t!- Unit Type");
            return info;
        }
        public ScheduleLimits()
        {
            numericType = "Continuous";
            unitType = "";
        }
    }
    [Serializable]
    public class ScheduleCompact
    {
        public ScheduleCompact() { }
        public string name { get; set; }
        public ScheduleLimits scheduleLimits { get; set; }
        public double value { get; set; }
        public Dictionary<string, Dictionary<string, double>> daysTimeValue;

        public ScheduleCompact(string name, List<string> fileData)
        {
            this.name = name;
            List<string> days = fileData.Where(x => x.ToLower().Contains("day") || x.ToLower().Contains("weekends")).ToList();
            daysTimeValue = new Dictionary<string, Dictionary<string, double>>();
            for (int i = 0; i<days.Count; i++)
            {
                int start = fileData.IndexOf(days[i]) + 1;
                int end = 0;
                if (i < days.Count - 1)
                {
                    end = fileData.IndexOf(days[i + 1]);
                }
                else
                {
                    end = fileData.Count;
                }
                Dictionary<string, double> dayValues = new Dictionary<string, double>();
                for (int x = start; x < end; x++)
                {
                    List<string> lineValue = fileData[x].Split(',').ToList();
                    dayValues.Add(lineValue[0], double.Parse(lineValue[1]));
                }
                daysTimeValue.Add(days[i], dayValues);
            }
        }
        public List<string> WriteInfo()
        {
            string sLimitName = scheduleLimits == null ? "" : scheduleLimits.name;

            List<string> info = new List<string>();
            info.Add("Schedule:compact,");
            info.Add(Utility.IDFLineFormatter (name, "Name"));           
            info.Add(Utility.IDFLineFormatter(sLimitName, "Schedule Type Limits Name"));
            info.Add(Utility.IDFLineFormatter("Through: 12/31", "Field1"));

            foreach (KeyValuePair<string, Dictionary<string, double>> kV in daysTimeValue)
            {
                info.Add("For: " + kV.Key + ",");
                foreach (KeyValuePair<string, double> tValue in kV.Value)
                {
                    info.Add("Until: " + tValue.Key + "," + tValue.Value + ",");
                }
            }
            info[info.Count-1] = info.Last().Remove(info.Last().Count()-1) + ';';
            return info;
        }
    }
    [Serializable]
    public class Version
    {
        public double VersionIdentifier = 9.2;
        public Version()
        {
            
        }
        public List<string> WriteInfo()
        {
            return new List<string>() { "Version,", Utility.IDFLastLineFormatter(VersionIdentifier, "Version Identifier") };
        }
    }
    [Serializable]
    public class SimulationControl
    {
        public string doZoneSizingCalculation = "Yes",
        doSystemSizingCalculation = "Yes",
            doPlantSizingCalculation = "Yes",
            runSimulationForSizingPeriods = "No",
            runSimulationForWeatherFileRunPeriods = "Yes";

        public SimulationControl()
        {
           
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "SimulationControl,",
                Utility.IDFLineFormatter(doZoneSizingCalculation, "Do Zone Sizing Calculation"),
                Utility.IDFLineFormatter(doSystemSizingCalculation, "Do System Sizing Calculation"),
                Utility.IDFLineFormatter(doPlantSizingCalculation, "Do Plant Sizing Calculation"),
                Utility.IDFLineFormatter(runSimulationForSizingPeriods, "Run Simulation for Sizing Periods"),
                Utility.IDFLastLineFormatter(runSimulationForWeatherFileRunPeriods, "Run Simulation for Sizing Periods"),
            };
        }
    }
    [Serializable]
    public class Timestep
    {
        public Timestep() { }
        public int NumberOfTimestepsPerHour { get; set; }

        public Timestep(int numberOfTimestepsPerHour)
        {
            this.NumberOfTimestepsPerHour = numberOfTimestepsPerHour;
        }
        public List<string> WriteInfo()
        {
            return new List<string>() { "Timestep,", Utility.IDFLastLineFormatter(NumberOfTimestepsPerHour, "Number of Timesteps per Hour") };
        }
    }
    [Serializable]
    public class ConvergenceLimits
    {
        public int minimumSystemTimestep = 0,
        maximumHVACIterations = 20,
            minimumPlantIterations = 2,
            maximumPlantIterations = 8;

        public ConvergenceLimits()
        {
            
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "ConvergenceLimits,",
                Utility.IDFLineFormatter(minimumSystemTimestep, "Minimum System Timestep {minutes}"),
                Utility.IDFLineFormatter(maximumHVACIterations, "Maximum HVAC Iterations"),
                Utility.IDFLineFormatter(minimumPlantIterations, "Minimum Plant Iterations"),
                Utility.IDFLastLineFormatter(maximumPlantIterations, "Maximum Plant Iterations")
            };
        }
    }
    [Serializable]
    public class SiteLocation
    {
        public string name { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double timeZone { get; set; }
        public double elevation { get; set; }

        public SiteLocation() { }
        public SiteLocation(string location)
        {
            switch (location)
            {
                case "MUNICH_DEU":
                    name = "MUNICH_DEU";
                    latitude = 48.13;
                    longitude = 11.7;
                    timeZone = 1.0;
                    elevation = 529.0;
                    break;
                default:
                    name = "MUNICH_DEU";
                    latitude = 48.13;
                    longitude = 11.7;
                    timeZone = 1.0;
                    elevation = 529.0;
                    break;
            }
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "Site:Location,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(latitude, "Latitude {deg}"),
                Utility.IDFLineFormatter(longitude, "Longitude {deg}"),
                Utility.IDFLineFormatter(timeZone, "Time Zone {hr}"),
                Utility.IDFLastLineFormatter(elevation, "Elevation {m}")
            };
        }
    }
    [Serializable]
    public class SizingPeriodDesignDay
    {
        public SizingPeriodDesignDay() { }
        public string name { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public string dayType { get; set; }

        public double maxDryBulbT { get; set; }
        public double dailyDryBulbTR { get; set; }
        public string dryBulbTRModifierType { get; set; }

        public string humidityConditionType { get; set; }
        public double wetbulbOrDawPointAtMaxDryBulb { get; set; }

        public double enthalpyAtMaxDryBulb { get; set; }

        public double baromPress { get; set; }
        public double windspeed { get; set; }
        public double windDir { get; set; }

        public string rainInd { get; set; }
        public string snowInd { get; set; }
        public string daylightSavTimeInd { get; set; }
        public string solarModelInd { get; set; }

        public double skyClearness { get; set; }

        public SizingPeriodDesignDay(string name, int month, int day, string dayType, double maxDryBulbT, double dailyDryBulbTR, double wetbulbOrDawPointAtMaxDryBulb, double enthalpyAtMaxDryBulb, double baromPress,
            double windspeed, double windDir, string rainInd, string snowInd, string daylightSavTimeInd, string solarModelInd, double skyClearness)
        {
            this.name = name;
            this.month = month;
            this.day = day;
            this.dayType = dayType;
            this.maxDryBulbT = maxDryBulbT;
            this.dailyDryBulbTR = dailyDryBulbTR;
            dryBulbTRModifierType = "DefaultMultipliers";
            humidityConditionType = "DewPoint";
            this.wetbulbOrDawPointAtMaxDryBulb = wetbulbOrDawPointAtMaxDryBulb;
            this.enthalpyAtMaxDryBulb = enthalpyAtMaxDryBulb;
            this.baromPress = baromPress;
            this.windspeed = windspeed;
            this.windDir = windDir;
            this.rainInd = rainInd;
            this.snowInd = snowInd;
            this.daylightSavTimeInd = daylightSavTimeInd;
            this.solarModelInd = solarModelInd;
            this.skyClearness = skyClearness;
        }


        public List<String> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nSizingPeriod:DesignDay,");
            info.Add("\t" + name + ",\t\t\t\t!-Name");
            info.Add("\t" + month + ", \t\t\t\t!-Month");
            info.Add("\t" + day + ",\t\t\t\t!-Day of Month");
            info.Add("\t" + dayType + ", \t\t\t\t!-Day Type");
            info.Add("\t" + maxDryBulbT + ", \t\t\t\t!-Maximum Dry-Bulb Temperature {C}");
            info.Add("\t" + dailyDryBulbTR + ", \t\t\t\t!-Daily Dry-Bulb Temperature Range {deltaC}");
            info.Add("\t" + dryBulbTRModifierType + ",\t\t\t\t!-Dry-Bulb Temperature Range Modifier Type");
            info.Add(",\t\t\t\t!-Dry-Bulb Temperature Range Modifier Day Schedule Name");
            info.Add("\t" + humidityConditionType + ",\t\t\t\t!-Humidity Condition Type");
            info.Add("\t" + wetbulbOrDawPointAtMaxDryBulb + ",\t\t\t\t!-Wetbulb or DewPoint at Maximum Dry-Bulb {C}");
            info.Add(",\t\t\t\t!-Humidity Condition Day Schedule Name");
            info.Add(",\t\t\t\t!-Humidity Ratio at Maximum Dry-Bulb {kgWater/kgDryAir}");
            info.Add(enthalpyAtMaxDryBulb + ",\t\t\t\t!-Enthalpy at Maximum Dry-Bulb {J/kg}");
            info.Add(",\t\t\t\t!-Daily Wet-Bulb Temperature Range {deltaC}");
            info.Add("\t" + baromPress + ",\t\t\t\t!-Barometric Pressure {Pa}");
            info.Add("\t" + windspeed + ",\t\t\t\t!-Wind Speed {m/s}");
            info.Add("\t" + windDir + ",\t\t\t\t!-Wind Direction {deg}");
            info.Add("\t" + rainInd + ",\t\t\t\t!-Rain Indicator");
            info.Add("\t" + snowInd + ",\t\t\t\t!-Snow Indicator");
            info.Add("\t" + daylightSavTimeInd + ",\t\t\t\t!-Daylight Saving Time Indicator");
            info.Add("\t" + solarModelInd + ",\t\t\t\t!-Solar Model Indicator");
            info.Add(",\t\t\t\t!-Beam Solar Day Schedule Name");
            info.Add(",\t\t\t\t!-Diffuse Solar Day Schedule Name");
            info.Add(",\t\t\t\t!-ASHRAE Clear Sky Optical Depth for Beam Irradiance (taub) {dimensionless}");
            info.Add(",\t\t\t\t!-ASHRAE Clear Sky Optical Depth for Diffuse Irradiance (taud) {dimensionless}");
            info.Add("\t" + skyClearness + ";\t\t\t\t!-Sky Clearness");

            return info;
        }

    }
    [Serializable]
    public class RunPeriod
    {
        public string name = "Run Period 1",     
            dayOfWeekForStartDay = "",
            useWeatherFileHolidaysAndSpecialDays = "No",
            useWeatherFileDaylightSavingPeriod = "No",
            WeekendHolidayRule = "No",
            useWeatherFileRainIndicators = "Yes",
            useWeatherFileSnowIndicators = "Yes",
            actualWeather = "No";
        
        public int beginMonth = 1,
        beginDayMonth = 1,
            endMonth = 12,
            endDayOfMonth = 31;
        

        public RunPeriod()
        {
            
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "RunPeriod,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(beginMonth, "Begin Month"),
                Utility.IDFLineFormatter(beginDayMonth, "Begin Day of Month"),
                Utility.IDFLineFormatter("", "Begin Year"),
                Utility.IDFLineFormatter(endMonth, "End Month"),
                Utility.IDFLineFormatter(endDayOfMonth, "End Day of Month"),
                 Utility.IDFLineFormatter("", "End year"),
                Utility.IDFLineFormatter(dayOfWeekForStartDay, "Day of Week for Start Day"),
                Utility.IDFLineFormatter(useWeatherFileHolidaysAndSpecialDays, "Use Weather File Holidays and Special Days"),
                Utility.IDFLineFormatter(useWeatherFileDaylightSavingPeriod, "Use Weather File Daylight Saving Period"),
                Utility.IDFLineFormatter(WeekendHolidayRule, "Apply Weekend Holiday Rule"),
                Utility.IDFLineFormatter(useWeatherFileRainIndicators, "Use Weather File Rain Indicators"),
                Utility.IDFLineFormatter(useWeatherFileSnowIndicators, "Use Weather File Snow Indicators"),
                Utility.IDFLastLineFormatter(actualWeather, "Actual Weather")
            };
        }
    }
    [Serializable]
    public class SiteGroundTemperature
    {
        public double jan { get; set; }
        public double feb { get; set; }
        public double mar { get; set; }
        public double apr { get; set; }
        public double may { get; set; }
        public double jun { get; set; }
        public double jul { get; set; }
        public double aug { get; set; }
        public double sep { get; set; }
        public double oct { get; set; }
        public double nov { get; set; }
        public double dec { get; set; }
        public SiteGroundTemperature()
        { }
        public SiteGroundTemperature(string place)
        {
            switch (place)
            {
                case "MUNICH_DEU":
                    jan = 6.17; feb = 5.07; mar = 5.33; apr = 6.27; may = 9.35; jun = 12.12;
                    jul = 14.32; aug = 15.48; sep = 15.20; oct = 13.62; nov = 11.08; dec = 8.41;
                    break;
                default:
                    jan = 6.17; feb = 5.07; mar = 5.33; apr = 6.27; may = 9.35; jun = 12.12;
                    jul = 14.32; aug = 15.48; sep = 15.20; oct = 13.62; nov = 11.08; dec = 8.41;
                    break;
            }
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>() { "Site:GroundTemperature:BuildingSurface," };
            info.AddRange(new List<string>() { string.Join(",", jan, feb, mar, apr, may, jun, jul, aug, sep, oct, nov, dec) + "; ! - Site Ground Temperatures" });
            return info;
        }
    }
    [Serializable]
    public class GlobalGeometryRules
    {
        public string startingVertexPosition { get; set; }
        public string vertexEntryDirection { get; set; }
        public string coordinateSystem { get; set; }
        public string daylightingRefPointCoordSyst { get; set; }
        public string rectSurfaceCoordSyst { get; set; }

        public GlobalGeometryRules()
        {
            startingVertexPosition = "UpperLeftCorner";
            vertexEntryDirection = "Counterclockwise";
            coordinateSystem = "Relative";
            daylightingRefPointCoordSyst = "Relative";
            rectSurfaceCoordSyst = "Relative";
        }

        internal List<string> WriteInfo()
        {
            return new List<string>()
            {
                "GlobalGeometryRules,",
                Utility.IDFLineFormatter(startingVertexPosition, "Starting Vertex Position"),
                Utility.IDFLineFormatter(vertexEntryDirection, "Vertex Entry Direction"),
                Utility.IDFLineFormatter(coordinateSystem, "Coordinate System"),
                Utility.IDFLineFormatter(daylightingRefPointCoordSyst, "Daylighting Reference Point Coordinate System"),
                Utility.IDFLastLineFormatter(rectSurfaceCoordSyst, "Rectangular Surface Coordinate System")
            };
        }
    }
    [Serializable]
    public class Output
    {
        public Output() { }
        public OutputVariableDictionary varDict;
        public Report report;
        public OutputTableSummaryReports tableSumReports;
        public OutputcontrolTableStyle tableStyle;
        public List<OutputVariable> vars;
        public OutputDiagnostics diagn;
        public List<OutputPreProcessorMessage> preprocessormess;

        public Output(Dictionary<string, string> variables)
        {
            varDict = new OutputVariableDictionary();
            report = new Report();
            tableSumReports = new OutputTableSummaryReports();
            tableStyle = new OutputcontrolTableStyle();
            diagn = new OutputDiagnostics();

            preprocessormess = new List<OutputPreProcessorMessage>();
            preprocessormess.Add(new OutputPreProcessorMessage(new List<String>(new string[] { "Cannot find Energy +.idd as specified in Energy +.ini." })));
            preprocessormess.Add(new OutputPreProcessorMessage(new List<String>(new string[] { "Since the Energy+.IDD file cannot be read no range or choice checking was", "performed." })));

            vars = new List<OutputVariable>();
            foreach (string key in variables.Keys)
            { vars.Add(new OutputVariable(key, variables[key])); }
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:VARIABLEDICTIONARY ===========");
            info.AddRange(varDict.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:SURFACES:DRAWING ===========");

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: REPORT ===========");
            info.AddRange(report.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:TABLE:SUMMARYREPORTS ===========");
            info.AddRange(tableSumReports.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUTCONTROL:TABLE:STYLE ===========");
            info.AddRange(tableStyle.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:VARIABLE ===========");
            foreach (OutputVariable var in vars)
            { info.AddRange(var.writeInfo()); }

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:DIAGNOSTICS ===========");
            info.AddRange(diagn.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:PREPROCESSORMESSAGE ===========");
            foreach (OutputPreProcessorMessage mes in preprocessormess)
            { info.AddRange(mes.writeInfo()); }

            return info;
        }

    }
    [Serializable]
    public class OutputVariableDictionary
    {
        public string keyField = "idf";
        public OutputVariableDictionary()
        {
            
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:VariableDictionary,");
            info.Add("\t" + keyField + ";\t\t\t\t!-Key Field");

            return info;
        }
    }
    [Serializable]
    public class Report
    {
        public string reportType = "dxf";
        public Report()
        {
           
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Surfaces:Drawing,");
            info.Add("\t" + reportType + ";\t\t\t\t!-Report Type");

            return info;
        }
    }
    [Serializable]
    public class OutputTableSummaryReports
    {
        public string report1 = "ZoneComponentLoadSummary",
        report2 = "ComponentSizingSummary",
            report3 = "EquipmentSummary",
            report4 = "HVACSizingSummary",
            report5 = "ClimaticDataSummary",
            report6 = "OutdoorAirSummary",
            report7 = "EnvelopeSummary";

        public OutputTableSummaryReports()
        {
            
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Table:SummaryReports,");
            info.Add("\t" + report1 + ",\t\t\t\t!-Report 1 Name");
            info.Add("\t" + report2 + ",\t\t\t\t!-Report 2 Name");
            info.Add("\t" + report3 + ",\t\t\t\t!-Report 3 Name");
            info.Add("\t" + report4 + ",\t\t\t\t!-Report 4 Name");
            info.Add("\t" + report5 + ",\t\t\t\t!-Report 5 Name");
            info.Add("\t" + report6 + ",\t\t\t\t!-Report 6 Name");
            info.Add("\t" + report7 + ";\t\t\t\t!-Report 7 Name");

            return info;
        }


    }
    [Serializable]
    public class OutputcontrolTableStyle
    {
        public string columnSeparator = "XMLandHTML";
        public OutputcontrolTableStyle()
        {
            
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutputControl:Table:Style,");
            info.Add("\t" + columnSeparator + ";\t\t\t\t!-Column Separator");

            return info;
        }
    }
    [Serializable]
    public class OutputVariable
    {
        public string keyValue { get; set; }
        public string variableName { get; set; }
        public string reportingFrequency { get; set; }

        public OutputVariable() { }
        public OutputVariable(string varName, string reportfreq)
        {
            keyValue = "*";
            variableName = varName;
            reportingFrequency = reportfreq;
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Variable,");
            info.Add("\t" + keyValue + ",\t\t\t\t!-Key Value");
            info.Add("\t" + variableName + ",\t\t\t\t!-Variable Name");
            info.Add("\t" + reportingFrequency + ";\t\t\t\t!-Reporting Frequency");

            return info;
        }
    }
    [Serializable]
    public class OutputDiagnostics
    {
        public string Key1 = "DisplayAdvancedReportVariables";
        public OutputDiagnostics()
        {
            
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Diagnostics,");
            info.Add("\t" + Key1 + ";\t\t\t\t!-Key 1");

            return info;
        }
    }
    [Serializable]
    public class OutputPreProcessorMessage
    {
        public string preprocessorName;
        public string errorSeverity;
        public List<string> messageLines;

        public OutputPreProcessorMessage() { }
        public OutputPreProcessorMessage(List<string> messageLines)
        {
            preprocessorName = "ExpandObjects";
            errorSeverity = "Warning";
            this.messageLines = messageLines;
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:PreprocessorMessage,");
            info.Add("\t" + preprocessorName + ",\t\t\t\t!-Preprocessor Name");
            info.Add("\t" + errorSeverity + ",\t\t\t\t!-Error Severity");
            for (int i = 1; i < messageLines.Count; i++)
            {
                info.Add("\t" + messageLines[i - 1] + ",\t\t\t\t!-Message Line " + i);
            }
            info.Add("\t" + messageLines[messageLines.Count - 1] + ";\t\t\t\t!-Message Line " + messageLines.Count);


            return info;
        }

    }
    [Serializable]
    public class PhotovoltaicPerformanceSimple
    {
        public string Name = "Simple Flat PV";
        public string Type = "PhotovoltaicPerformance:Simple";
        public double FractionSurface = 0.7;
        public string ConversionEff = "FIXED";
        public double CellEff = 0.12;
        public PhotovoltaicPerformanceSimple() { }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "PhotovoltaicPerformance:Simple,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(FractionSurface, "Fraction of Surface Area with Active Solar Cells {dimensionless}"),
                Utility.IDFLineFormatter(ConversionEff, "Conversion Efficiency Input Mode"),
                Utility.IDFLastLineFormatter(CellEff, "Value for Cell Efficiency if Fixed")
            };
        }
    }
    [Serializable]
    public class GeneratorPhotovoltaic
    {
        public string Name;
        public string Type = "Generator:Photovoltaic";
        public BuildingSurface bSurface;
        public string PhotovoltaicPerformanceObjectType;
        public PhotovoltaicPerformanceSimple pperformance;
        public string HeatTransferIntegrationMode = "Decoupled";

        public double GeneratorPowerOutput = 50000;
        public ScheduleCompact Schedule;
        public double RatedThermalElectricalPowerRatio = 0;


        public GeneratorPhotovoltaic() { }
        public GeneratorPhotovoltaic(BuildingSurface Surface, PhotovoltaicPerformanceSimple Performance, ScheduleCompact scheduleOn)
        {
            Name = "PV on " + Surface.name;
            bSurface = Surface;
            pperformance = Performance;
            PhotovoltaicPerformanceObjectType = Performance.Type;
            Schedule = scheduleOn;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "Generator:Photovoltaic,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(bSurface.name, "Surface Name"),
                Utility.IDFLineFormatter(PhotovoltaicPerformanceObjectType, "Photovoltaic Performance Object Type"),
                Utility.IDFLineFormatter(pperformance.Name, "Module Performance Name"),
                Utility.IDFLastLineFormatter(HeatTransferIntegrationMode, "Heat Transfer Integration Mode")
            };
        }
    }
    [Serializable]
    public class ElectricLoadCenterGenerators
    {
        public string Name = "Supplementary Generator";
        public List<GeneratorPhotovoltaic> Generator = new List<GeneratorPhotovoltaic>();

        public ElectricLoadCenterGenerators() { }

        public ElectricLoadCenterGenerators(List<GeneratorPhotovoltaic> Generators)
        {
            Generator = Generators;
        }
        public List<string> WriteInfo()
        {
            List<string> GeneratorInfo = new List<string>
            {
                "ElectricLoadCenter:Generators,",
                Utility.IDFLineFormatter(Name, "Name")
            };
            Generator.ForEach
            (
                g => GeneratorInfo.AddRange(new List<string>()
                {
                    Utility.IDFLineFormatter(g.Name, "Generator Name"),
                    Utility.IDFLineFormatter(g.Type, "Generator Type"),
                    Utility.IDFLineFormatter(g.GeneratorPowerOutput, "Generator Power Output"),
                    Utility.IDFLineFormatter(g.Schedule.name, "Generator Schedule"),
                    Utility.IDFLineFormatter(g.RatedThermalElectricalPowerRatio, "Generator Rated Thermal to Electrical Power Ratio")
                })
            );
            Utility.ReplaceLastComma(GeneratorInfo);
            return GeneratorInfo;
        }
    }
    [Serializable]
    public class ElectricLoadCenterDistribution
    {
        public string Name = "Electric Load Center";
        public ElectricLoadCenterGenerators GeneratorList;
        public string GeneratorOperationSchemeType = "DemandLimit";
        public double DemandLimitSchemePurchasedElectricDemandLimit = 100000;
        public string TrackScheduleNameSchemeScheduleName = " ";
        public string TrackMeterSchemeMeterName = " ";
        public string ElectricalBussType = "AlternatingCurrent";
        public string InverterObjectName = " ";
        public string ElectricalStorageObjectName = " ";

        public ElectricLoadCenterDistribution() { }
        public ElectricLoadCenterDistribution(ElectricLoadCenterGenerators GeneratorList1)
        {
            GeneratorList = GeneratorList1;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "ElectricLoadCenter:Distribution,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(GeneratorList.Name,  "Generator List Name"),
                Utility.IDFLineFormatter(GeneratorOperationSchemeType, "Generator Operation Scheme Type"),
                Utility.IDFLineFormatter(DemandLimitSchemePurchasedElectricDemandLimit, "Demand Limit Scheme Purchased Electric Demand Limit {W}"),
                Utility.IDFLineFormatter(TrackScheduleNameSchemeScheduleName, "Track Schedule Name Scheme Schedule Name"),
                Utility.IDFLineFormatter(TrackMeterSchemeMeterName, "Track Meter Scheme Meter Name"),
                Utility.IDFLineFormatter(ElectricalBussType, "Electrical Buss Type"),
                Utility.IDFLineFormatter(InverterObjectName, "Inverter Object Name"),
                Utility.IDFLastLineFormatter(ElectricalStorageObjectName, "Electrical Storage Object Name")
            };
        }
    }

}
