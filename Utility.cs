using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace IDFObjects
{ 
    public enum SurfaceType { Floor, Ceiling, Wall, Roof };
    public enum HVACSystem { FCU, BaseboardHeating, VAV, IdealLoad };
    public enum Direction { North, East, South, West };
    public enum ControlType { none, Continuous, Stepped, ContinuousOff }
    public static class Utility
    {
        public static bool CompareCurves(IDFObjects.XYZ[] c1, IDFObjects.XYZ[] c2)
        {
            return (c2[0].Equals(c1[0]) && c2[1].Equals(c1[1])) || (c2[1].Equals(c1[0]) && c2[0].Equals(c1[1]));
        }
        public static bool IsCollinear(IDFObjects.XYZ[] c1, IDFObjects.XYZ[] c2)
        {
            IDFObjects.XYZ p1 = c1[0], p2 = c1[1], p3 = c2[0], p4 = c2[1];
            return Math.Round(new IDFObjects.XYZList(new List<IDFObjects.XYZ>() { p1, p2, p3 }).CalculateArea(), 4) == 0 &&
            Math.Round(new IDFObjects.XYZList(new List<IDFObjects.XYZ>() { p1, p2, p4 }).CalculateArea(), 4) == 0;
        }
        public static XYZ GetDirection(IDFObjects.XYZ[] Line)
        {
            double x = Line[1].X - Line[0].X, y = Line[1].Y - Line[0].Y, z = Line[1].Z - Line[0].Z;
            double dist = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            return new XYZ(x / dist, y / dist, z/dist);
        }
        public static IDFObjects.XYZList GetDayLightPointsXYZList(IDFObjects.XYZList FloorFacePoints, List<IDFObjects.XYZ[]> ExWallEdges)
        {
            List<IDFObjects.XYZ> floorPoints = FloorFacePoints.xyzs;
            List<IDFObjects.XYZ[]> WallEdges = GetExternalEdges(floorPoints);
            List<IDFObjects.XYZ> CentersOfMass = TriangleAndCentroid(floorPoints);
            IDFObjects.XYZList DLList = new IDFObjects.XYZList(CentersOfMass.Where(p => RayCastToCheckIfIsInside(WallEdges, p) &&
            CheckMinimumDistance(ExWallEdges, p, 0.2)).ToList());
            return DLList;
        }
        public static bool RayCastToCheckIfIsInside(List<IDFObjects.XYZ[]> WallEdges, IDFObjects.XYZ point)
        {
            int intersections = 0;
            foreach (IDFObjects.XYZ[] WallEdge in WallEdges)
            {
                double r = (point.Y - WallEdge[1].Y) / (WallEdge[0].Y - WallEdge[1].Y);
                if (r >= 0 && r < 1)
                {
                    double Xvalue = r * (WallEdge[0].X - WallEdge[1].X) + WallEdge[1].X;
                    if (point.X < Xvalue)
                    {
                        intersections++;
                    }
                }
            }
            return intersections % 2 != 0;
        }
        public static bool CheckMinimumDistance(List<IDFObjects.XYZ[]> WallEdges, IDFObjects.XYZ point, double distance)
        {
            return WallEdges.All(wEdge => GetPerpendicularDistance(wEdge, point) > distance);
        }

        public static double GetPerpendicularDistance(IDFObjects.XYZ[] WallEdge, IDFObjects.XYZ a)
        {
            IDFObjects.XYZ b = WallEdge[0], c = WallEdge[1];
            IDFObjects.XYZ d = b.Subtract(c).Multiply(1 / b.DistanceTo(c));
            IDFObjects.XYZ v = b.Subtract(a);
            double t = v.DotProduct(d);
            IDFObjects.XYZ P = b.Add(d.Multiply(t));
            return P.DistanceTo(a);
        }
        public static IDFObjects.XYZ CentroidOfTriangle(List<IDFObjects.XYZ> points)
        {
            return new IDFObjects.XYZ()
            {
                X = points.Select(p => p.X).Average(),
                Y = points.Select(p => p.Y).Average(),
                Z = points.Select(p => p.Z).Average(),
            };
        }
        public static List<IDFObjects.XYZ[]> GetExternalEdges(List<IDFObjects.XYZ> groundPoints)
        {
            List<IDFObjects.XYZ[]> wallEdges = new List<IDFObjects.XYZ[]>();
            for (int i = 0; i < groundPoints.Count; i++)
            {
                try
                {
                    wallEdges.Add(new IDFObjects.XYZ[] { groundPoints[i], groundPoints[i + 1] });
                }
                catch
                {
                    wallEdges.Add(new IDFObjects.XYZ[] { groundPoints[i], groundPoints[0] });
                }
            }
            return wallEdges;
        }
        
        public static List<IDFObjects.XYZ> TriangleAndCentroid(List<IDFObjects.XYZ> AllPoints)
        {
            List<IDFObjects.XYZ> CentersOfMass = new List<IDFObjects.XYZ>();

            if (AllPoints.Count() > 3)
            {
                for (int i = 0; i < AllPoints.Count(); i++)
                {
                    IDFObjects.XYZ p, p1, p2;
                    p = AllPoints[i];
                    try { p1 = AllPoints[i - 1]; } catch { p1 = AllPoints.Last(); }
                    try { p2 = AllPoints[i + 1]; } catch { p2 = AllPoints.First(); }
                    CentersOfMass.Add(CentroidOfTriangle(new List<IDFObjects.XYZ>() { p1, p, p2 }));
                }
            }
            CentersOfMass.Add(CentroidOfTriangle(AllPoints));
            return CentersOfMass;
        }
        public static string GetHeader(string comp)
        {
            string info = "";
            string[] spaceChar = new string[] { "Zone Area", "Zone Height", "Zone Volume",
                "Light Heat Gain", "Equipment Heat Gain", "Internal Heat Gain", "Infiltration",
                "Operating Hours", "Solar Radiation", "Total Heat capacity",
                "Total Wall Area", "Total Window Area", "Total Roof Area", "Total Ground Floor Area", "Total Internal Floor Area", "Total Internal Wall Area",
                "uWall", "uWindow", "gWindow", "uRoof", "uGFloor", "uIFloor", "uIWall"};
            string[] adSpaceChar = spaceChar.Select(p => "Adjacent_" + p).ToArray();

            switch (comp)
            {
                case "Wall":
                    info = string.Join(",", "File", "Zone Name", "Name", "Area", "Orientation", "WWR", "U Value", "Heat Capacity", "Radiation",
                                    string.Join(",", spaceChar), "Heat Flow");
                    break;
                case "Window":
                    info = string.Join(",", "File", "Zone Name", "Name", "Area", "Orientation", "U Value", "g Value", "Radiation",
                                        string.Join(",", spaceChar), "Heat Flow");
                    break;
                case "GFloor":
                    info = string.Join(",", "File", "Zone Name", "Name", "Area", "U Value", "Heat Capacity",
                        string.Join(",", spaceChar), "Heat Flow");
                    break;
                case "Roof":
                    info = string.Join(",", "File", "Zone Name", "Name", "Area", "U Value", "Heat Capacity", "Radiation",
                        string.Join(",", spaceChar), "Heat Flow");
                    break;
                case "IFloor":
                case "IWall":
                    info = string.Join(",", "File", "Zone Name", "Name", "Adjacent Zone", "Area", "U Value",
                        string.Join(",", spaceChar), string.Join(",", adSpaceChar), "Heat Flow");
                    break;
                case "Infiltration":
                    info = string.Join(",", "File", "Zone Name", "Name", string.Join(",", spaceChar), "Heat Flow");
                    break;
                case "Zone":
                    info = string.Join(",", "File", "Name", string.Join(",", spaceChar),
                        "Wall Heat Flow", "Window Heat Flow", "GFloor Heat Flow", "Roof Heat Flow", "IFloor Heat Flow", "IWall Heat Flow",
                         "Infiltration Heat Flow", "Building Element Heat Flow", "Total Heat Flow",
                        "Heating Energy", "Cooling Energy", "Lighting Energy");
                    break;
                case "Building":
                    info = string.Join(",", "File", "Total Floor Area", "Floor Height", "Total Volume",
                        "Total Wall Area", "Total Window Area", "Total Roof Area", "Total Ground Floor Area", "Total Internal Floor Area", "Total Internal Wall Area",
                    "uWall", "uWindow", "gWindow", "uRoof", "uGFloor", "uIFloor", "uIWall", "Infiltration", "Total heat capacity", "Operating Hours", "Light Heat Gain", "Equipment Heat Gain",
                    "Internal Heat Gain",

                        "Boiler Efficiency", "ChillerCOP", "Lighting Energy", "Zone Heating Energy", "Zone Cooling Energy",
                        "Heating Energy", "Cooling Energy", "Thermal Energy", "Operational Energy");
                    break;
            }
            return info;
        }
        public static GridPoint GetDirection(List<GridPoint> Line)
        {
            double x = Line[1].x - Line[0].x; double y = Line[1].y - Line[0].y;
            double dist = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            return new GridPoint(x / dist, y / dist);
        }
        public static bool IsCounterClockWise(List<GridPoint> points)
        {
            double area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                GridPoint point = points[i];
                double x1 = point.x, y1 = point.y;
                GridPoint nextPoint = new GridPoint();
                try
                {
                    nextPoint = points[i + 1];
                }
                catch
                {
                    nextPoint = points[0];
                }
                double x2 = nextPoint.x, y2 = nextPoint.y;
                area += (x2 - x1) * (y2 + y1);
            }
            bool returnVal = area > 0;
            return returnVal;
        }
        public static double[] GetSpaceChr(Building building, Zone z)
        {
            //"Zone Area", "Zone Height", "Zone Volume",
            //    "Light Heat Gain", "Equipment Heat Gain", "Internal Heat Gain", "Infiltration",
            //    "Operating Hours", "Solar Radiation", "Total heat capacity"
            //    "Total Wall Area", "Total Window Area", "Total Roof Area", "Total Ground Floor Area", "Total Internal Floor Area", "Total Internal Wall Area",
            //    "uWall", "uWindow", "gWindow", "uRoof", "uGFloor", "uIFloor", "uIWall"
            
            ZoneList zList = building.zoneLists.First(zL => zL.zoneNames.Contains(z.Name));
            double light = zList.Light.wattsPerArea, equipment = zList.ElectricEquipment.wattsPerArea,
            infiltration = zList.ZoneInfiltration.airChangesHour;           
            
            BuildingConstruction buiCons = building.buildingConstruction;
            return new double[] {
                z.Area, z.Height, z.Volume, 
                light, equipment, light+equipment, infiltration,
                building.buildingOperation.operatingHours, z.SolarRadiation, z.TotalHeatCapacity,
                z.totalWallArea, z.totalWindowArea, z.totalRoofArea, z.totalGFloorArea, z.totalIFloorArea, z.totalIWallArea,
                buiCons.uWall, buiCons.uWindow, buiCons.gWindow, buiCons.uRoof, buiCons.uGFloor, buiCons.uIFloor, buiCons.uIWall
            };
        }
        public static Dictionary<string, IList<string>> GetMLCSVLines(Building building)
        {
            Dictionary<string, IList<string>> CSVData = new Dictionary<string, IList<string>>();
            string idfFile = building.name;
            CSVData.Add("Wall", new List<string>());
            CSVData.Add("Window", new List<string>());
            CSVData.Add("GFloor", new List<string>());
            CSVData.Add("Roof", new List<string>());
            CSVData.Add("Infiltration", new List<string>());
            CSVData.Add("Zone", new List<string>());
            CSVData.Add("Building", new List<string>());

            BuildingConstruction buildingConstruction = building.buildingConstruction;

            foreach (Zone z in building.zones)
            {
                double[] spaChr = Utility.GetSpaceChr(building, z);
                z.Surfaces.Where(w => w.surfaceType == SurfaceType.Wall && w.OutsideCondition == "Outdoors").ToList().ForEach(
                    s => CSVData["Wall"].Add(string.Join(",", idfFile, z.Name, s.Name, s.Area, s.Orientation, s.WWR, buildingConstruction.uWall, buildingConstruction.hcWall, s.SolarRadiation,
                    string.Join(",", spaChr), s.HeatFlow)));
                z.Surfaces.Where(w => w.Fenestrations != null).SelectMany(w => w.Fenestrations).ToList().
                    ForEach(s => CSVData["Window"].Add(string.Join(",", idfFile, z.Name, s.Name, s.Area, s.Orientation,
                    buildingConstruction.uWindow, buildingConstruction.gWindow, s.SolarRadiation,
                    string.Join(",", spaChr), s.HeatFlow)));
                z.Surfaces.Where(w => w.surfaceType == SurfaceType.Floor && w.OutsideCondition == "Ground").ToList().ForEach(
                    s => CSVData["GFloor"].Add(string.Join(",", idfFile, z.Name, s.Name, s.Area, buildingConstruction.uGFloor, buildingConstruction.hcGFloor,
                    string.Join(",", spaChr), s.HeatFlow)));
                z.Surfaces.Where(w => w.surfaceType == SurfaceType.Roof).ToList().ForEach(
                    s => CSVData["Roof"].Add(string.Join(",", idfFile, z.Name, s.Name, s.Area, buildingConstruction.uRoof, buildingConstruction.hcRoof, s.SolarRadiation,
                    string.Join(",", spaChr), s.HeatFlow)));
                CSVData["Infiltration"].Add(string.Join(",", idfFile, z.Name, z.Name, string.Join(",", spaChr), z.infiltrationFlow));
                CSVData["Zone"].Add(string.Join(",", idfFile, z.Name, string.Join(",", spaChr),
                    z.wallHeatFlow, z.windowHeatFlow, z.gFloorHeatFlow, z.roofHeatFlow, z.iFloorHeatFlow, z.iWallHeatFlow, z.infiltrationFlow,
                    z.TotalHeatFlows - z.infiltrationFlow, z.TotalHeatFlows,
                    z.HeatingEnergy, z.CoolingEnergy, z.LightingEnergy));
            }

            CSVData["Building"].Add(string.Join(",", idfFile,
                            building.zones.Select(z => z.Area).Sum(),
                            building.FloorHeight,
                            building.zones.Select(z => z.Volume).Sum(),
                            building.zones.Select(z => z.totalWallArea).Sum(),
                            building.zones.Select(z => z.totalWindowArea).Sum(),
                            building.zones.Select(z => z.totalRoofArea).Sum(),
                            building.zones.Select(z => z.totalGFloorArea).Sum(),
                            building.zones.Select(z => z.totalIFloorAreaExOther).Sum(),
                            building.zones.Select(z => z.totalIWallAreaExOther).Sum(),
                            buildingConstruction.uWall,
                            buildingConstruction.uWindow,
                            buildingConstruction.gWindow,
                            buildingConstruction.uRoof,
                            buildingConstruction.uGFloor,
                            buildingConstruction.uIFloor,
                            buildingConstruction.uIWall,
                            buildingConstruction.infiltration,
                            building.zones.Select(z => z.TotalHeatCapacityDeDuplicatingIntSurfaces).Sum(),
                            building.buildingOperation.operatingHours,
                            building.buildingOperation.lightHeatGain,
                            building.buildingOperation.equipmentHeatGain,
                            building.buildingOperation.lightHeatGain + building.buildingOperation.equipmentHeatGain,
                            building.buildingOperation.boilerEfficiency,
                            building.buildingOperation.chillerCOP,
                            building.LightingEnergy,
                            building.ZoneHeatingEnergy,
                            building.ZoneCoolingEnergy,
                            building.BoilerEnergy,
                            building.ChillerEnergy,
                            building.ThermalEnergy,
                            building.OperationalEnergy));
            return CSVData;
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
        public static void CreateZoneWalls(Zone z, Dictionary<XYZ[], string> wallsData, double baseZ, double height)
        {
            foreach (KeyValuePair<XYZ[], string> wallData in wallsData)
            {
                XYZ p1 = wallData.Key[0].OffsetHeight(baseZ), p2 = wallData.Key[1].OffsetHeight(baseZ), p3 = p2.OffsetHeight(height), p4 = p1.OffsetHeight(height);
                XYZList wallPoints = new XYZList(new List<XYZ>() { p1, p2, p3, p4 });
                double area = p1.DistanceTo(p2) * p2.DistanceTo(p3);
                BuildingSurface wall = new BuildingSurface(z, wallPoints, area, SurfaceType.Wall);
                if (wallData.Value != "Outdoors")
                {
                    if (wallData.Value == "Adiabatic")
                    {
                        wall.OutsideCondition = "Adiabatic"; wall.OutsideObject = "";
                    }
                    else
                    {
                        wall.OutsideCondition = "Zone"; wall.OutsideObject = wallData.Value + ":" + z.Level;
                    }
                    wall.ConstructionName = "InternalWall";
                    wall.SunExposed = "NoSun"; wall.WindExposed = "NoWind"; wall.Fenestrations = new List<Fenestration>();
                }
            }
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

            int iLen = -1, iWid = -1, iArea = -1, iNFloors = -1, iShape = -1, iARatio = -1;
            try
            {
                iShape = parameters.FindIndex(s => s.Contains("Shape"));
            }
            catch { }
            try
            {
                iARatio = parameters.FindIndex(s => s.Contains("L/W Ratio"));
            }
            catch { }
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
                if (iARatio != -1) { value.ARatio = sample[iARatio]; }
                if (iNFloors != -1) { value.NFloors = (int)sample[iNFloors]; }
                if (iLen != -1) { value.Length = sample[iLen]; value.Width = sample[iWid]; }
                else { value.FloorArea = sample[iArea]; }

                if (iShape != -1) { value.Shape = "Shape" + Math.Floor(sample[iShape]); }
                value.Height = sample[iHeight];
                value.Orientation = Math.Round(sample[iOrientation]/9) * Math.PI / 20;
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
                value.wwr = new WWR(sample[iWWRN], sample[iWWRE], sample[iWWRS], sample[iWWRW]);
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
        public static void Seialise<T>(this T obj, string filePath)
        {
            TextWriter tW = File.CreateText(filePath);
            new JsonSerializer() { Formatting = Formatting.Indented }.Serialize(tW, obj);
            tW.Close(); 
        }
        public static T DeSeialise<T>(string filePath)
        {
            TextReader tR = File.OpenText(filePath);
           
            T val = new JsonSerializer().Deserialize<T>(new JsonTextReader(tR));
            tR.Close();
            return val ;
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
        public static int[] HourToHHMM(double hours)
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
        public static string IDFLineFormatter(object attribute, string definition)
        {
            if (attribute != null) { return (attribute.ToString() + ",\t\t\t\t\t\t ! - " + definition); }
            else { return (",\t\t\t\t\t\t ! - " + definition); }
        }
        public static string IDFLastLineFormatter(object attribute, string definition)
        {
            return (attribute.ToString() + ";\t\t\t\t\t\t ! - " + definition);
        }
        public static List<SizingPeriodDesignDay> CreateDesignDays(string location)
        {
            switch (location)
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
}
