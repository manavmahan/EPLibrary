using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.JScript;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace IDFObjects
{
    public enum LevelExposure
    {
        Ground, Intermediate, Top
    }
    public enum Location
    {
        [Description("Brussels, Belgium")] BRUSSELS_BEL,
        [Description("Munich, Germany")] MUNICH_DEU,
    }
    public enum Month
    {
        Jan, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec
    }
    public enum PDF { unif, tria, norm }
    public enum SurfaceType { Floor, Ceiling, Wall, Roof };
    public enum HVACSystem { HeatPumpWBoiler, FCU, BaseboardHeating, VAV, IdealLoad };
    public enum Direction { North, East, South, West };
    public enum ControlType { none, Continuous, Stepped, ContinuousOff }
    public static class Utility
    {
        public static FieldInfo GetMonthlyFieldInfo<T>(FieldInfo fieldInfo)
        {
            return typeof(T).GetFields().First(x => x.Name == fieldInfo.Name + "Monthly");
        }
        public static string ToCSVString(this double[] array)
        {
            return string.Join(",", array.Select(x=>x));
        }
        public static void SumAverageMonthlyValues<T>(this T obj)
        {
            foreach(FieldInfo x in typeof(T).GetFields().Where(f => f.FieldType == typeof(double)))
            {
                if (x.Name.Contains("Load"))
                     x.SetValue(obj, (GetMonthlyFieldInfo<T> (x).GetValue(obj) as double[]).Average());
                else
                     x.SetValue(obj, (GetMonthlyFieldInfo<T>(x).GetValue(obj) as double[]).Sum());
            }
            
        }
        public static T GetAverage<T>(this List<T> obj) where T : new()
        {
            T val = new T();
            typeof(T).GetFields().Where(x=>x.FieldType == typeof(double)).ToList().ForEach(x=>
            {
                x.SetValue(val, obj.Select(o => (double)x.GetValue(o)).ToArray().Average());               
            });
            
            return val;
        }
        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().FirstOrDefault();
        }
        public static T GetValue<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null && attribute.Description == description)
                    return (T)field.GetValue(null);
            }
            throw new ArgumentException("Not found.", nameof(description));
        }
        public static void ZipAll<T>(this IEnumerable<IEnumerable<T>> all, Action<IEnumerable<T>> action)
        {
            var enumerators = all.Select(e => e.GetEnumerator()).ToList();
            try
            {
                while (enumerators.All(e => e.MoveNext()))
                    action(enumerators.Select(e => e.Current));
            }
            finally
            {
                foreach (var e in enumerators)
                    e.Dispose();
            }
        }
        public static ZoneGeometryInformation GetZoneGeometryInformation(
            List<XYZ> groundPoints, double heightFl, string zoneName)
        {
            List<XYZ[]> externalEdgesIDF = GetExternalEdges(groundPoints);
            ZoneGeometryInformation zInfo = new ZoneGeometryInformation();
            zInfo.Name = zoneName;
            zInfo.Height = heightFl;
            zInfo.FloorPoints = new XYZList(groundPoints);
            externalEdgesIDF.ForEach(w => zInfo.WallCreationData.Add(w, "Outdoors"));
            return zInfo;
        }
        public static List<XYZ> GetOffset(List<XYZ> Loop, double offsetDist)
        {
            List<XYZ[]> offsetLines = new List<XYZ[]>();
            List<XYZ[]> perimeterLines = GetExternalEdges(Loop);
            for (int i = 0; i < Loop.Count(); i++)
            {
                XYZ[] c = perimeterLines[i], c1, c2;

                try { c2 = perimeterLines.ElementAt(i + 1); }
                catch { c2 = perimeterLines.First(); }

                try { c1 = perimeterLines.ElementAt(i - 1); }
                catch { c1 = perimeterLines.Last(); }

                XYZ[] offsetLine = GetOffset(c, c1, c2, offsetDist);
                if (offsetLine != null) { offsetLines.Add(offsetLine); }
            }
            return offsetLines.Select(p => p[0]).ToList();
        }      
        public static XYZ RotateToNormal(XYZ[] line, int corner)
        {
            XYZ point = line[corner];
            XYZ centerPoint = corner == 1 ? line[0] : line[1];
            XYZ translatePoint = point.Subtract(centerPoint);
            XYZ rotatedTranslatePoint = new XYZ(translatePoint.X * Math.Cos(Math.PI / 2) - translatePoint.Y * Math.Sin(Math.PI / 2),
                                                translatePoint.X * Math.Sin(Math.PI / 2) + translatePoint.Y * Math.Cos(Math.PI / 2),
                                                translatePoint.Z);
            return rotatedTranslatePoint.Add(centerPoint);
        }
        public static XYZ Direction(this XYZ[] line)
        {
            XYZ d = line[1].Subtract(line[0]);
            double mod = d.AbsoluteValue();
            return d.Multiply(1 / mod);
        }
        public static double GetAngle(XYZ[] c1, XYZ[] c2)
        {
            return c1.Direction().AngleOnPlaneTo(c2.Direction(), new XYZ(0, 0, -1));
        }
        public static XYZ[] GetOffset(XYZ[] line, XYZ[] prevLine, XYZ[] nextLine, double offsetDistance)
        {
            XYZ p1 = line[0].MovePoint(RotateToNormal(line, 1), offsetDistance),
            p2 = line[1].MovePoint(RotateToNormal(line, 0), (-1) * offsetDistance);

            XYZ dir1 = new XYZ[] { p1, p2}.Direction();

            p1 = p1.MovePoint( p2, Math.Sin(GetAngle(prevLine, line)) * (-1) * offsetDistance);
            p2 = p2.MovePoint( p1, Math.Sin(GetAngle(line, nextLine)) * (-1) * offsetDistance);

            try
            {
                XYZ[] l1 = new XYZ[] { p1, p2 };
                return l1.Direction().IsAlmostEqual(dir1) ? l1 : null;
            }
            catch
            {
                return null;
            }
        }
        public static List<ZoneGeometryInformation> GetZoneGeometryInformation(
            Dictionary<string, List<XYZ[]>> allRoomSegmentsIDF,
            List<XYZ[]> externalEdgesIDF, double heightFl)
        {
            List<ZoneGeometryInformation> zoneInfoList = new List<ZoneGeometryInformation>();
            List<string> zNames = allRoomSegmentsIDF.Select(x => x.Key).ToList();
            foreach (string zName in zNames)
            {
                ZoneGeometryInformation zInfo = new ZoneGeometryInformation();
                zInfo.Name = zName;
                zInfo.Height = heightFl;

                List<XYZ[]> thisRoomSegments = allRoomSegmentsIDF[zName];
                List<XYZ> floorPoints = thisRoomSegments.Select(x => x[0]).ToList();
                XYZList flPointList = new XYZList(floorPoints);
                flPointList.RemoveCollinearPoints();
                zInfo.FloorPoints = flPointList;
                zoneInfoList.Add(zInfo);
            }

            foreach (ZoneGeometryInformation zone in zoneInfoList)
            {
                Dictionary<string, List<XYZ[]>> allRoomSegmentsNotThisRoom = new Dictionary<string, List<XYZ[]>>();
                List<XYZ[]> thisRoomSegments = allRoomSegmentsIDF[zone.Name];
                foreach (KeyValuePair<string, List<XYZ[]>> exRoomSegment in allRoomSegmentsIDF.Where(x => x.Key != zone.Name))
                {
                    allRoomSegmentsNotThisRoom.Add(exRoomSegment.Key, exRoomSegment.Value);
                }

                foreach (XYZ[] c in thisRoomSegments)
                {
                    try
                    {
                        KeyValuePair<string, List<XYZ[]>> matchingCurve = allRoomSegmentsNotThisRoom
                            .First(x => x.Value.Any(y => CompareCurves(c, y)));
                        zone.WallCreationData.Add(c, matchingCurve.Key);

                        List<XYZ[]> matchingZoneSegments = allRoomSegmentsIDF[matchingCurve.Key];
                        matchingZoneSegments.Remove(matchingZoneSegments.First(x => CompareCurves(c, x)));
                    }
                    catch
                    {
                        if (externalEdgesIDF.Any(y => IsCollinear(y, c)))
                        { zone.WallCreationData.Add(c, "Outdoors"); }
                        else { zone.WallCreationData.Add(c, "Adiabatic"); }
                    }
                }
            }
            return zoneInfoList;
        }
        public static List<ZoneGeometryInformation> GetZoneGeometryInformation(Dictionary<string, int> zoneLevels,
            Dictionary<string, List<XYZ[]>> allRoomSegmentsIDF, List<XYZ[]> externalEdgesIDF, double heightFl)
        {
            List<ZoneGeometryInformation> zoneInfoList = new List<ZoneGeometryInformation>();
            List<string> zNames = allRoomSegmentsIDF.Select(x => x.Key).ToList();
            foreach (string zName in zNames)
            {
                ZoneGeometryInformation zInfo = new ZoneGeometryInformation();
                zInfo.Name = zName;
                zInfo.Height = heightFl;
                zInfo.Level = zoneLevels[zName];

                List<XYZ[]> thisRoomSegments = allRoomSegmentsIDF[zName];
                List<XYZ> floorPoints = thisRoomSegments.Select(x => x[0]).ToList();
                XYZList flPointList = new XYZList(floorPoints);
                flPointList.RemoveCollinearPoints();
                zInfo.FloorPoints = flPointList;
                zoneInfoList.Add(zInfo);
            }

            foreach (ZoneGeometryInformation zone in zoneInfoList)
            {
                Dictionary<string, List<XYZ[]>> allRoomSegmentsNotThisRoom = new Dictionary<string, List<XYZ[]>>();
                List<XYZ[]> thisRoomSegments = allRoomSegmentsIDF[zone.Name];
                foreach (KeyValuePair<string, List<XYZ[]>> exRoomSegment in allRoomSegmentsIDF.Where(x => x.Key != zone.Name))
                {
                    allRoomSegmentsNotThisRoom.Add(exRoomSegment.Key, exRoomSegment.Value);
                }

                foreach (XYZ[] c in thisRoomSegments)
                {
                    try
                    {
                        KeyValuePair<string, List<XYZ[]>> matchingCurve = allRoomSegmentsNotThisRoom
                            .First(x => x.Value.Any(y => CompareCurves(c, y)));
                        zone.WallCreationData.Add(c, matchingCurve.Key);

                        List<XYZ[]> matchingZoneSegments = allRoomSegmentsIDF[matchingCurve.Key];
                        matchingZoneSegments.Remove(matchingZoneSegments.First(x => CompareCurves(c, x)));
                    }
                    catch
                    {
                        if (externalEdgesIDF.Any(y => IsCollinear(y, c)))
                        { zone.WallCreationData.Add(c, "Outdoors"); }
                        else { zone.WallCreationData.Add(c, "Adiabatic"); }
                    }
                }
            }
            return zoneInfoList;
        }
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
            return new XYZ(x / dist, y / dist, z / dist);
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
        public static double GetPerpendicularDistance(XYZ[] WallEdge, XYZ p3)
        {
            XYZ p1 = WallEdge[0], p2 = WallEdge[1];
            double d = p2.Subtract(p1).CrossProduct(p1.Subtract(p3)).AbsoluteValue() / p2.DistanceTo(p1);
            return d;
        }
        public static IDFObjects.XYZ CentroidOfTriangle(List<IDFObjects.XYZ> points)
        {
            return new IDFObjects.XYZ()
            {
                X = Math.Round(points.Select(p => p.X).Average(),2),
                Y = Math.Round(points.Select(p => p.Y).Average(),2),
                Z = Math.Round(points.Select(p => p.Z).Average(),2),
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
            bool returnVal = area < 0;
            return returnVal;
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
        public static void CreateZoneWalls(Zone z, Dictionary<XYZ[], string> wallsData, double baseZ)
        {
            foreach (KeyValuePair<XYZ[], string> wallData in wallsData)
            {
                XYZ p1 = wallData.Key[0].OffsetHeight(baseZ), p2 = wallData.Key[1].OffsetHeight(baseZ), p3 = p2.OffsetHeight(z.Height), p4 = p1.OffsetHeight(z.Height);
                XYZList wallPoints = new XYZList(new List<XYZ>() { p1, p2, p3, p4 });
                double area = p1.DistanceTo(p2) * z.Height;
                Surface wall = new Surface(z, wallPoints, area, SurfaceType.Wall);
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
                    wall.Fenestrations = null;
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
        public static Dictionary<string, double[]> ReadSampleFile(string parFile, out int count)
        {
            List<string> rawFile = File.ReadAllLines(parFile).Where(s => s[0] != '#').ToList();

            int nSamples = rawFile.Count;
            count = nSamples - 1;
            Dictionary<string, double[]> returnData = new Dictionary<string, double[]>();

            IEnumerable<string> header = rawFile[0].Split(',').ToList();

            for (int i = 0; i < header.Count(); i++)
            {
                double[] sampleData = new double[nSamples - 1];
                for (int s = 1; s < nSamples; s++)
                {
                    sampleData[s - 1] = double.Parse(rawFile[s].Split(',').ElementAt(i));
                }
                returnData.Add(header.ElementAt(i), sampleData);
            }
            return returnData;
        }
        public static ProbabilityDistributionFunction GetPDF(string[] data)
        {
            return new ProbabilityDistributionFunction(double.Parse(data[0]), double.Parse(data[1]), data[2]);
        } 
        public static ProbabilityDistributionFunction GetProbabilisticParameter(this Dictionary<string, string[]> DataDictionary, string Parameter)
        {
            if (DataDictionary.ContainsKey(Parameter))
            {
                return GetPDF(DataDictionary[Parameter]);
            }
            else
            {
                return new ProbabilityDistributionFunction();
            }
        }
        public static ProbabilisticBuildingDesignParameters ReadProbabilisticBuildingDesignParameters(string dataFile)
        {
            IEnumerable<string[]> allLines = File.ReadAllLines(dataFile)
                .Where(s => s.First() != '#').Select(s => s.Split(','));

            List<string> parameters = allLines.Select(s => s.First()).ToList();
            List<string[]> data = allLines.Select(s => s.Skip(1).ToArray()).ToList();

            Dictionary<string, string[]> dataDict = new Dictionary<string, string[]>();
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Count() > 2)
                {
                    dataDict.Add(parameters[i], data[i]);
                }
            }

            List<string> zoneListNames = parameters.Where(p => p.Contains(':'))
                .Select(p => p.Split(':')[0]).Distinct().ToList();

            ProbabilisticBuildingDesignParameters value = new ProbabilisticBuildingDesignParameters()
            {
                pGeometry = new ProbabilisticBuildingGeometry()
                {
                    Length = dataDict.GetProbabilisticParameter("Length"),
                    Width = dataDict.GetProbabilisticParameter("Width"),
                    Height = dataDict.GetProbabilisticParameter("Height"),
                    FloorArea = dataDict.GetProbabilisticParameter("Floor Area"),
                    NFloors = dataDict.GetProbabilisticParameter("NFloors"),
                    Shape = dataDict.GetProbabilisticParameter("Shape"),
                    ARatio = dataDict.GetProbabilisticParameter("L/W Ratio"),
                    Orientation = dataDict.GetProbabilisticParameter("Orientation"),
                },
                pConstruction = new ProbabilisticBuildingConstruction()
                {
                    InternalMass = dataDict.GetProbabilisticParameter("Internal Mass"),
                    UWall = dataDict.GetProbabilisticParameter("u_Wall"),
                    UGFloor = dataDict.GetProbabilisticParameter("u_GFloor"),
                    URoof = dataDict.GetProbabilisticParameter("u_Roof"),
                    UWindow = dataDict.GetProbabilisticParameter("u_Window"),
                    GWindow = dataDict.GetProbabilisticParameter("g_Window"),
                    Infiltration = dataDict.GetProbabilisticParameter("Infiltration"),
                    UIFloor = dataDict.GetProbabilisticParameter("u_IFloor"),
                    UIWall = dataDict.GetProbabilisticParameter("u_IWall"),
                    HCSlab = dataDict.GetProbabilisticParameter("hc_Slab")
                },
                pWWR = new ProbabilisticBuildingWWR()
                {
                    North = dataDict.GetProbabilisticParameter("WWR_North"),
                    East = dataDict.GetProbabilisticParameter("WWR_East"),
                    West = dataDict.GetProbabilisticParameter("WWR_West"),
                    South = dataDict.GetProbabilisticParameter("WWR_South")
                },
                pService = new ProbabilisticBuildingService()
                {
                    BoilerEfficiency = dataDict.GetProbabilisticParameter("Boiler Efficiency"),
                    HeatingCOP = dataDict.GetProbabilisticParameter("Heating COP"),
                    CoolingCOP = dataDict.GetProbabilisticParameter("Cooling COP")
                }
            };

            foreach (string zlN in zoneListNames)
            {
                value.zOperations.Add(new ProbabilisticBuildingZoneOperation()
                {
                    Name = zlN,
                    LHG = dataDict.GetProbabilisticParameter(zlN + ":Light Heat Gain"),
                    EHG = dataDict.GetProbabilisticParameter(zlN + ":Equipment Heat Gain"),
                    StartTime = dataDict.GetProbabilisticParameter(zlN + ":Start Time"),
                    OperatingHours = dataDict.GetProbabilisticParameter(zlN + ":Operating Hours")
                });
                value.zOccupants.Add(new ProbabilisticBuildingZoneOccupant()
                {
                    Name = zlN,
                    AreaPerPerson = dataDict.GetProbabilisticParameter(zlN + ":Area Per Person")
                });
                value.zEnvironments.Add(new ProbabilisticBuildingZoneEnvironment()
                {
                    Name = zlN,
                    HeatingSetPoint = dataDict.GetProbabilisticParameter(zlN + ":Heating Setpoint"),
                    CoolingSetPoint = dataDict.GetProbabilisticParameter(zlN + ":Cooling Setpoint")
                });
            }
            return value;
        }
        public static double GetSamplesValues(this Dictionary<string, double[]> DataDictionary, string Parameter, int Number)
        {
            if (DataDictionary.ContainsKey(Parameter))
            {
                return DataDictionary[Parameter][Number];
            }
            else
            {
                return 0;
            }
        }
        public static List<BuildingDesignParameters> ReadBuildingDesignParameters(string dataFile)
        {
            Dictionary<string, double[]> samples = ReadSampleFile(dataFile, out int Count);
            List<string> zoneListNames = samples.Keys.Where(p => p.Contains(':'))
                .Select(s => s.Split(':')[0]).Distinct().ToList();

            List<BuildingDesignParameters> values = new List<BuildingDesignParameters>();
            for (int s = 0; s < Count; s++)
            {
                BuildingDesignParameters value = new BuildingDesignParameters();
                value.Geometry = new BuildingGeometry()
                {
                    Length = samples.GetSamplesValues("Length", s),
                    Width = samples.GetSamplesValues("Width", s),
                    Height = samples.GetSamplesValues("Height", s),
                    FloorArea = samples.GetSamplesValues("Floor Area", s),
                    NFloors = (int)samples.GetSamplesValues("NFloors", s),
                    Shape = (int)samples.GetSamplesValues("Shape", s),
                    ARatio = samples.GetSamplesValues("ARatio", s),
                    Orientation = (int) Math.Round(samples.GetSamplesValues("Orientation", s)),

                    rLenA = samples.GetSamplesValues("rLenA", s),
                    rWidA = samples.GetSamplesValues("rWidA", s),
                    BasementDepth = samples.GetSamplesValues("Basement Depth", s),
                };
                value.Construction = new BuildingConstruction()
                {
                    InternalMass = samples.GetSamplesValues("Internal Mass", s),
                    UWall = samples.GetSamplesValues("u_Wall", s),
                    UGFloor = samples.GetSamplesValues("u_GFloor", s),
                    URoof = samples.GetSamplesValues("u_Roof", s),
                    UWindow = samples.GetSamplesValues("u_Window", s),
                    GWindow = samples.GetSamplesValues("g_Window", s),
                    Infiltration = samples.GetSamplesValues("Infiltration", s),

                    UIFloor = samples.GetSamplesValues("u_IFloor", s),
                    UIWall = samples.GetSamplesValues("u_IWall", s),
                    HCSlab = samples.GetSamplesValues("hc_Slab", s),
                    UCWall = samples.GetSamplesValues("u_CWall", s)
                };
                value.WWR = new BuildingWWR()
                {
                    North = samples.GetSamplesValues("WWR_North", s),
                    East = samples.GetSamplesValues("WWR_East", s),
                    West = samples.GetSamplesValues("WWR_West", s),
                    South = samples.GetSamplesValues("WWR_South", s),
                };
                value.Service = new BuildingService()
                {
                    BoilerEfficiency = samples.GetSamplesValues("Boiler Efficiency", s),
                    HeatingCOP = samples.GetSamplesValues("Heating COP", s),
                    CoolingCOP = samples.GetSamplesValues("Cooling COP", s)
                };

                foreach (string zlN in zoneListNames)
                {
                    value.Operations.Add(new BuildingZoneOperation()
                    {
                        Name = zlN,
                        LHG = samples.GetSamplesValues(zlN + ":Light Heat Gain", s),
                        EHG = samples.GetSamplesValues(zlN + ":Equipment Heat Gain", s),
                        StartTime = samples.GetSamplesValues(zlN + ":Start Time", s),
                        OperatingHours = samples.GetSamplesValues(zlN + ":Operating Hours", s),
                    });
                    value.Occupants.Add(new BuildingZoneOccupant()
                    {
                        Name = zlN,
                        AreaPerPerson = samples.GetSamplesValues(zlN + ":Area Per Person", s)
                    });
                    value.Environments.Add(new BuildingZoneEnvironment()
                    {
                        Name = zlN,
                        HeatingSetPoint = samples.GetSamplesValues(zlN + ":Heating Setpoint", s),
                        CoolingSetPoint = samples.GetSamplesValues(zlN + ":Cooling Setpoint", s)
                    });
                }
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
        public static void Serialise<T>(this T obj, string filePath)
        {
            TextWriter tW = File.CreateText(filePath);
            new JsonSerializer() { Formatting = Formatting.Indented }.Serialize(tW, obj);
            tW.Close();
        }
        public static T DeSerialise<T>(string filePath)
        {
            TextReader tR = File.OpenText(filePath);

            T val = new JsonSerializer().Deserialize<T>(new JsonTextReader(tR));
            tR.Close();
            return val;
        }
        public static double ConvertKWhfromJoule(this double d) { return d * 2.7778E-7; }
        public static double ConvertWfromJoule(this double d) { return d * 2.7778E-4 / 8760; }
        public static double[] ConvertWfromJoule(this double[] dArray) { return dArray.Select(d => d.ConvertWfromJoule()).ToArray(); }
        public static double[] ConvertKWhfromJoule(this double[] dArray) { return dArray.Select(d => d.ConvertKWhfromJoule()).ToArray(); }
        public static double[] FillZeroes(this double[] Array, int length)
        {
            int count = Array.Count();
            IEnumerable<double> newList = Array;

            for (int i = count; i < length; i++) { newList = newList.Append(0); }
            return newList.ToArray();
        }
        public static double ConvertKWhafromW(this double d)
        {
            return d*8.76;
        }
        public static double ConvertKWhafromWm(this double d)
        {
            return d * 8.76/12;
        }
        public static double[] ConvertKWhafromW(this double[] dArray)
        {
            return dArray.Select(d => d.ConvertKWhafromW()).ToArray();
        }
        public static double[] ConvertKWhafromWm(this double[] dArray)
        {
            return dArray.Select(d => d.ConvertKWhafromWm()).ToArray();
        }
        public static double[] MultiplyBy(this double[] dArray, double factor)
        {
            return dArray.Select(d => d*factor).ToArray();
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
        public static T2 GetSobolSample<T, T2>(this T pars, double[] sample) where T2 : new()
        {
            int i = 0;
            T2 r = new T2();
            foreach (FieldInfo fi in typeof(T).GetFields().Where(x => x.FieldType == typeof(ProbabilityDistributionFunction)
            && (x.GetValue(pars) as ProbabilityDistributionFunction).Range > 0))
            {
                ProbabilityDistributionFunction v = fi.GetValue(pars) as ProbabilityDistributionFunction;
                double val = 0;
                switch (v.Distribution)
                {
                    case PDF.unif:
                        val = v.Min + v.Range * sample[i];
                        break;
                    case PDF.norm:
                        val = v.Mean + v.VariationOrSD * sample[i];
                        break;
                }
                FieldInfo f = typeof(T2).GetFields().First(x => x.Name == fi.Name);
                if (f.FieldType == typeof(int))
                    f.SetValue(r, (int) Math.Floor(val));
                else
                    f.SetValue(r, val);
                i++;
            };
            foreach (FieldInfo fi in typeof(T).GetFields().Where(x => x.FieldType == typeof(ProbabilityDistributionFunction)
            && (x.GetValue(pars) as ProbabilityDistributionFunction).Range == 0))
            {
                ProbabilityDistributionFunction v = fi.GetValue(pars) as ProbabilityDistributionFunction;
                double val = v.Mean;
                FieldInfo f = typeof(T2).GetFields().First(x => x.Name == fi.Name);
                if (f.FieldType == typeof(int))
                    f.SetValue(r, (int)Math.Floor(val));
                else
                    f.SetValue(r, val);
            };
            return r;
        }
        
        internal static List<ProbabilityDistributionFunction> GetValidPDFs<T>(this T pars)
        {
            return typeof(T).GetFields().Where(x => x.FieldType == typeof(ProbabilityDistributionFunction)
             && (x.GetValue(pars) as ProbabilityDistributionFunction).VariationOrSD > 0).
             Select(x=>x.GetValue(pars) as ProbabilityDistributionFunction).ToList();
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
        public static List<SizingPeriodDesignDay> CreateDesignDays(Location location)
        {
            SizingPeriodDesignDay winterday, summerday, summerday1, summerday2, summerday3, summerday4;
            switch (location)
            {
                case Location.MUNICH_DEU:
                default:
                    winterday = new SizingPeriodDesignDay("MUNICH Ann Htg 99.6% Condns DB", 2, 21, "WinterDesignDay",
                        -12.8, 0.0, -13.9, 0.0, 95900.0, 1.0, 130.0, "No", "No", "No", "AshraeClearSky", 0.0);

                    summerday = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns Enth=>MDB", 7, 21, "SummerDesignDay",
                        31.5, 10.9, 17.8, 0.0, 95300.0, 1.5, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);

                    summerday1 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 6)", 6, 21, "SummerDesignDay",
                        29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);

                    summerday2 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 7)", 7, 21, "SummerDesignDay",
                        29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);

                    summerday3 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 8)", 8, 21, "SummerDesignDay",
                        29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);

                    summerday4 = new SizingPeriodDesignDay("MUNICH Ann Clg .4% Condns DB=>MWB (month 9)", 9, 21, "SummerDesignDay",
                        29.0, 10.9, 13.9, 0.0, 95200.0, 1.0, 240.0, "No", "No", "No", "AshraeClearSky", 1.0);
                    break;
                case Location.BRUSSELS_BEL:
                    winterday = new SizingPeriodDesignDay("BRUSSELS Ann Htg 99.6% Condns DB", 1, 21, "WinterDesignDay",
                        -4.9, 0.0, -6.2, 0.0, 102600.0, 1.0, 70.0, "No", "No", "No", "AshraeClearSky", 0.0);

                    summerday = new SizingPeriodDesignDay("BRUSSELS Ann Clg .4% Condns Enth=>MDB", 7, 21, "SummerDesignDay",
                        33.6, 8.4, 19.5, 0.0, 100600.0, 4.4, 90.0, "No", "No", "No", "AshraeClearSky", 1.0);

                    summerday1 = new SizingPeriodDesignDay("BRUSSELS Ann Clg .4 % Condns DB => MWB(month 6)", 6, 21, "SummerDesignDay",
                        28.7, 8.4, 13.6, 0, 101300, 3.0, 290, "No", "No", "No", "AshraeClearSky", 1.0);

                    summerday2 = new SizingPeriodDesignDay("BRUSSELS Ann Clg .4 % Condns DB => MWB(month 7)", 7, 21, "SummerDesignDay",
                        28.7, 8.4, 13.6, 0, 101300, 3.0, 290, "No", "No", "No", "AshraeClearSky", 1.0);

                    summerday3 = new SizingPeriodDesignDay("BRUSSELS Ann Clg .4 % Condns DB => MWB(month 8)", 8, 21, "SummerDesignDay",
                       28.7, 8.4, 13.6, 0, 101300, 3.0, 290, "No", "No", "No", "AshraeClearSky", 1.0);

                    summerday4 = new SizingPeriodDesignDay("BRUSSELS Ann Clg .4 % Condns DB => MWB(month 9)", 9, 21, "SummerDesignDay",
                       28.7, 8.4, 13.6, 0, 101300, 3.0, 290, "No", "No", "No", "AshraeClearSky", 1.0);
                    break;
            }
            return new List<SizingPeriodDesignDay>() { winterday, summerday, summerday1, summerday2, summerday3, summerday4 };
        }
    }
}
