using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Formatting = Newtonsoft.Json.Formatting;

namespace IDFObjects
{
    public enum LevelOfDevelopment
    {
        [Description("Level 1")] LOD1 = 1,
        [Description("Level 2")] LOD2 = 2,
        [Description("Level 3")] LOD3 = 3,
    }
    public enum OutputFrequency
    {
        [Description("Hourly")] Hourly = 1,
        [Description("Annual")] Annual = 2,
    }
    public enum LevelExposure
    {
        Ground, Intermediate, Top
    }
    public enum Location
    {
        [Description("Brussels, Belgium")] BRUSSELS_BEL = 0,
        [Description("Munich, Germany")] MUNICH_DEU = 1,
        [Description("Berlin, Germany")] BERLIN_DEU = 2,
    }
    public enum Month
    {
        Jan, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec
    }
    public enum PDF {
        [Description("unif")] unif = 0,
        [Description("tria")] tria = 1, 
        [Description("norm")] norm = 2
    }
    public enum SurfaceType { Floor, Ceiling, Wall, Roof };
    public enum HVACSystem {[Description("Heat Pump with Boiler")] HeatPumpWBoiler = 1,
                            [Description("Fan coil unit")] FCU =2, 
                            [Description("Baseboard Heating")] BaseboardHeating=3, 
                            [Description("Variable-Air-Volume")] VAV=4,
                            [Description("Ideal Air Load System")] IdealLoad = 0};
    public enum Direction { North, East, South, West };
    public enum ControlType { none, Continuous, Stepped, ContinuousOff }
    public enum SamplingScheme
    {
        [Description("Latin Hypercube Sampling")] LHS = 0,
        [Description("Monte Carlo Sampling")] MonteCarlo =2,
        [Description("Sobol Sampling")] Sobol =1
    }
    public enum AnalysisMode
    { 
        [Description("Generate Data for ML")] MLDataGeneration,
        [Description("Compare Options")] CompareOptions,
        [Description("Generate Options")] GenerateSeveralOptions,
        [Description("Analyse Option")] AnalyseOption,
        [Description("Sensitivity Analysis")] SensAna,
        [Description("Null")] Null
    }
    public enum SimulationTool
    {
        [Description("EnegryPlus Simulation")] EnergyPlus = 0,
        [Description("Machine Learning Model")] MLModel =1,
    }
    public static class Utility
    {
        public static List<int> GetRange(int n1, int n2)
        {
            List<int> r = new List<int>();
            for (int i = n1; i < n2; i++)
            {
                r.Add(i);
            }
            return r;
        }
        public static List<XYZList> FindTopRoof(List<XYZList> allRoofs)
        {
            float topRoofBase = 0;
            foreach (XYZList roof in allRoofs)
            {
                float mi = roof.XYZs.Select(p => p.Z).Min();
                if (mi > topRoofBase)
                    topRoofBase = mi;
            }
            return allRoofs.Where(r => r.XYZs.Select(ro => ro.Z).Any(Z=>Z == topRoofBase)).ToList();
        }
        public static List<XYZ> GetIntersections(this XYZList firstLoop, XYZList secondLoop)
        {
            List<XYZ> intersections = new List<XYZ>();
            foreach (Line l in firstLoop.Loop)
            {
                foreach(Line sl in secondLoop.Loop)
                {
                    if (l.GetIntersection(sl, out var intersection))
                        intersections.Add(intersection);
                }               
            }
            return intersections;
        }
        public static bool IsFirstLoopInsideSecondLoop(this XYZList firstLoop, XYZList secondLoop) 
        {
            return firstLoop.XYZs.All(p=> secondLoop.IsPointInside(p)); 
        }
        public static FieldInfo GetMonthlyFieldInfo<T>(FieldInfo fieldInfo)
        {
            return typeof(T).GetFields().First(x => x.Name == fieldInfo.Name + "Monthly");
        }
        public static string ToCSVString(this float[] array)
        {
            return string.Join("; ", array.Select(x=>x));
        }
        public static void SumAverageMonthlyValues<T>(this T obj)
        {
            foreach(FieldInfo x in typeof(T).GetFields().Where(f => f.FieldType == typeof(float)))
            {
                if (x.Name.Contains("Load"))
                     x.SetValue(obj, (GetMonthlyFieldInfo<T> (x).GetValue(obj) as float[]).Average());
                else
                     x.SetValue(obj, (GetMonthlyFieldInfo<T>(x).GetValue(obj) as float[]).Sum());
            }
            
        }
        public static T GetAverage<T>(this List<T> obj) where T : new()
        {
            T val = new T();
            typeof(T).GetFields().Where(x=>x.FieldType == typeof(float)).ToList().ForEach(x=>
            {
                x.SetValue(val, obj.Select(o => (float)x.GetValue(o)).ToArray().Average());               
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
        public static List<string> GetAllVaryingPDFs(this ProbabilisticBuildingDesignParameters p)
        {
            List<string> val = new List<string>();
            typeof(ProbabilisticBuildingGeometry).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a=> {
                    ProbabilityDistributionFunction pdf = a.GetValue(p.pGeometry) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0)
                        val.Add(pdf.Label);
                });

            typeof(ProbabilisticBuildingConstruction).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(p.pConstruction) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0)
                        val.Add(pdf.Label);
                });

            typeof(ProbabilisticBuildingService).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(p.pService) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0)
                        val.Add(pdf.Label);
                });

            typeof(ProbabilisticBuildingWWR).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(p.pWWR) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0)
                        val.Add(pdf.Label);
                });

            p.zConditions.ForEach(z => {
                typeof(ProbabilisticZoneConditions).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(z) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0)
                        val.Add($"{z.Name}:{pdf.Label}");
                });
            });
            return val;

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
        public static List<Line> GetOffset(IEnumerable<Line> perimeterLines, float offsetDist)
        {
            List<Line> offsetLines = new List<Line>();
            int n = perimeterLines.Count();
            for (int i = 0; i < n; i++)
            {
                Line c = perimeterLines.ElementAt(i);

                var c2 = perimeterLines.ElementAt((i + 1) % n);
                var c1 = perimeterLines.ElementAt(i-1<0 ? n-1 : i-1);

                Line offsetLine = GetOffset(c, c1, c2, offsetDist);
                if (offsetLine != null) { offsetLines.Add(offsetLine); }
            }
            return offsetLines;
        }
        public static XYZList GetOffset(XYZList perimeterPoints, float offsetDist)
        {
            var offPoints = GetOffset(perimeterPoints.Loop, offsetDist);
            return new XYZList(offPoints.Select(l => l.P0));          
        }
        public static XYZ RotateToNormal(Line line, int corner)
        {
            XYZ point = line.GetCorner(corner);
            XYZ centerPoint = corner == 0 ? line.GetCorner(1) : line.GetCorner(0);
            XYZ translatePoint = point.Subtract(centerPoint);
            XYZ rotatedTranslatePoint = new XYZ(translatePoint.X * (float) Math.Cos(Math.PI / 2) - translatePoint.Y * (float) Math.Sin(Math.PI / 2),
                                                translatePoint.X * (float) Math.Sin(Math.PI / 2) + translatePoint.Y * (float) Math.Cos(Math.PI / 2),
                                                translatePoint.Z);
            return rotatedTranslatePoint.Add(centerPoint);
        }
        
        public static float GetAngle(Line c1, Line c2)
        {
            return c1.Direction.AngleOnPlaneTo(c2.Direction, new XYZ(0, 0, -1))*((float) Math.PI/180);
        }
        public static Line GetOffset(Line line, Line prevLine, Line nextLine, float offsetDistance)
        {
            var p0 = line.P0.MovePoint(RotateToNormal(line, 1), (-1) * offsetDistance);
            var p1 = line.P1.MovePoint(RotateToNormal(line, 0), offsetDistance);

            XYZ dir1 = new Line(p0, p1).Direction;

            p0 = p0.MovePoint( p1, (float) Math.Sin(GetAngle(prevLine, line)) * offsetDistance);
            p1 = p1.MovePoint( p0, (float) Math.Sin(GetAngle(line, nextLine)) * offsetDistance);

            try
            {
                Line l1 = new Line (p0, p1);
                var x = Math.Abs(l1.Direction.AngleBetweenVectors(dir1));
                if ( x.Equals( float.NaN ) || x <= 1E-3 )
                    return l1;
                throw new Exception("Direction not same!");
            }
            catch
            {
                return null;
            }
        }
        public static Dictionary<ZoneGeometryInformation, List<Line>> ExpandRoomSegmentForEachFloor(Dictionary<string, List<Line>> rooms,
            List<ZoneGeometryInformation> zones)
        {
            Dictionary<ZoneGeometryInformation, List<Line>> allRoomSegment = new Dictionary<ZoneGeometryInformation, List<Line>>();
            foreach (ZoneGeometryInformation zone in zones)
            {
                float baseZ = zone.FloorPoints.ElementAt(0).XYZs.First().Z;
                string cRoom = zone.Name.Remove(zone.Name.LastIndexOf(":"));
                List<Line> walls = rooms.First(ro => ro.Key == cRoom).Value;
                allRoomSegment.Add(zone, walls.Select(l=>l.ChangeZValue(baseZ)).ToList());
            }           
            return allRoomSegment;
        }
        public static List<ZoneGeometryInformation> GetZoneGeometryInformation(
            Dictionary<string, List<Line>> allRooms, List<XYZList> floors, List<XYZList> roofs)
        {
            List<ZoneGeometryInformation> zoneInfoList = new List<ZoneGeometryInformation>();
            List<string> zNames = allRooms.Select(x => x.Key).ToList();
            List<Line> externalEdgesIDF = floors[0].Loop;
            for (int f = 0; f < floors.Count; f++)
            {
                foreach (string zName in zNames) 
                {
                    ZoneGeometryInformation zInfo = new ZoneGeometryInformation();
                    zInfo.Name = zName+':'+f;
                    List<Line> thisRoomSegments = allRooms[zName];
                    var floorPoints = thisRoomSegments.Select(x => x.P0).ToArray();
                    XYZList flPointList = new XYZList(floorPoints);

                    zInfo.FloorPoints = zInfo.FloorPoints.Append(flPointList.ChangeZValue(floors[f].XYZs.First().Z)).ToList();
                    List<XYZList> ceilingOrRoof = new List<XYZList>();
                    try
                    {
                        ceilingOrRoof = new List<XYZList>() { floors[f + 1] };                        
                    }
                    catch
                    {
                        ceilingOrRoof = roofs;
                    }
                    List<XYZList> rOrc = ceilingOrRoof.Select(c =>
                        new XYZList(flPointList.XYZs.Select(p => p.GetVerticalProjection(c)))).ToList();

                    if (f != floors.Count - 1)
                        zInfo.CeilingPoints = rOrc;
                    else
                        zInfo.RoofPoints = rOrc;

                    zInfo.Level = f;

                    zInfo.Height =
                        rOrc.SelectMany(ro => ro.XYZs.Select(p => p.Z)).Average() - zInfo.FloorPoints.ElementAt(0).XYZs.First().Z;
                    zoneInfoList.Add(zInfo);
                }
            }
            Dictionary<ZoneGeometryInformation, List<Line>> allRoomSegments = ExpandRoomSegmentForEachFloor(allRooms, zoneInfoList);
            foreach (ZoneGeometryInformation zone in zoneInfoList)
            {
                Dictionary<ZoneGeometryInformation, List<Line>> roomSegmentsFloors = allRoomSegments.Where(z=>z.Key.Level==zone.Level)
                    .ToDictionary(x => x.Key, x => x.Value);
                Dictionary<ZoneGeometryInformation, List<Line>> allRoomSegmentsNotThisRoom =
                    roomSegmentsFloors.Where(x => x.Key.Name !=zone.Name).ToDictionary(x => x.Key, x => x.Value);
                List<Line> thisRoomSegments = roomSegmentsFloors[zone];
                
                foreach (Line c in thisRoomSegments)
                {
                    string con = "Adiabatic";
                    ZoneGeometryInformation exZ = null;
                    try
                    {
                        exZ = allRoomSegmentsNotThisRoom.First(x => x.Value.Any(y => CompareCurves(c, y))).Key;
                    }
                    catch { }
                    if (exZ!=null)
                    {                      
                        con = exZ.Name;
                        allRoomSegments[exZ].Remove(allRoomSegments[exZ].First(x => CompareCurves(c, x)));
                    }
                    else
                    {
                        if (externalEdgesIDF.Any(y => IsCollinear(y, c)))
                            con = "Outdoors";
                    }
                    zone.WallGeometryData.Append(new KeyValuePair<string, Line>(con, c.ChangeZValue(zone.FloorPoints.ElementAt(0).XYZs.First().Z)));
                }
            }
            return zoneInfoList;
        }
        public static List<ZoneGeometryInformation> GetZoneGeometryInformation(Dictionary<string, int> zoneLevels,
            Dictionary<string, List<Line>> allRoomSegmentsIDF, List<Line> externalEdgesIDF, List<XYZList> massFloors, List<XYZList> roofs)
        {
            List<ZoneGeometryInformation> zoneInfoList = new List<ZoneGeometryInformation>();
            List<string> zNames = allRoomSegmentsIDF.Select(x => x.Key).ToList();

            Dictionary<string, List<Line>> newAllRoomSegmentsIDF = new Dictionary<string, List<Line>>();
            foreach (string zName in zNames)
            {
                ZoneGeometryInformation zInfo = new ZoneGeometryInformation();
                int level =  zoneLevels[zName];
                zInfo.Name = zName + ":" + level;
                zInfo.Level = level;
                XYZList zMassFloor = massFloors[level];
                XYZList ceiling = new XYZList();
                
                try { ceiling = massFloors[level + 1];  } catch { ceiling = roofs.First(); }

                List<Line> thisRoomSegments = allRoomSegmentsIDF[zName];
                var floorPoints = thisRoomSegments.Select(x => x.P0).ToArray();
                XYZList flPointList = new XYZList(floorPoints);

                zInfo.FloorPoints = zInfo.FloorPoints.Append(flPointList).ToList();

                float heightFl = ceiling.XYZs.First().Z - floorPoints.First().Z;
                zInfo.Height = heightFl;
                            
                zInfo.CeilingPoints = new List<XYZList>();
                zInfo.CeilingPoints = zInfo.CeilingPoints.Append(flPointList.OffsetHeight(heightFl)).ToList();
                zoneInfoList.Add(zInfo);
                newAllRoomSegmentsIDF.Add(zInfo.Name, thisRoomSegments);
            }

            
            foreach (ZoneGeometryInformation zone in zoneInfoList)
            {
                Dictionary<string, List<Line>> allRoomSegmentsNotThisRoom = new Dictionary<string, List<Line>>();
                List<Line> thisRoomSegments = newAllRoomSegmentsIDF[zone.Name];
                foreach (KeyValuePair<string, List<Line>> exRoomSegment in newAllRoomSegmentsIDF.Where(x => x.Key != zone.Name))
                {
                    allRoomSegmentsNotThisRoom.Add(exRoomSegment.Key, exRoomSegment.Value);
                }

                foreach (Line c in thisRoomSegments)
                {
                    try
                    {
                        KeyValuePair<string, List<Line>> matchingCurve = allRoomSegmentsNotThisRoom
                            .First(x => x.Value.Any(y => CompareCurves(c, y)));
                        zone.AddWallGeometryData(matchingCurve.Key, c);

                        List<Line> matchingZoneSegments = newAllRoomSegmentsIDF[matchingCurve.Key];
                        matchingZoneSegments.Remove(matchingZoneSegments.First(x => CompareCurves(c, x)));
                    }
                    catch
                    {
                        if (externalEdgesIDF.Any(y => IsCollinear(y, c)))
                        { zone.AddWallGeometryData("Outdoors", c); }
                        else { zone.AddWallGeometryData("Adiabatic", c); }
                    }
                }
            }
            return zoneInfoList;
        }
        public static bool CompareCurves(Line c1, Line c2)
        {
            return (c2.P0.Equals(c1.P0) && c2.P1.Equals(c1.P1)) || (c2.P1.Equals(c1.P0) && c2.P0.Equals(c1.P1));
        }       
        public static bool IsCollinear(Line c1, Line c2)
        {
            XYZ p1 = c1.P0, p2 = c1.P1, p3 = c2.P0, p4 = c2.P1;
            return new XYZList( p1, p2, p3 ).CalculateArea() == 0 &&
                new XYZList( p1, p2, p4 ).CalculateArea() == 0;
        }
        public static XYZ GetDirection(Line Line)
        {
            float x = Line.P1.X - Line.P0.X, y = Line.P1.Y - Line.P0.Y, z = Line.P1.Z - Line.P0.Z;
            float dist = (float) Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            return new XYZ(x / dist, y / dist, z / dist);
        }
        public static XYZList GetDayLightPointsXYZList(IEnumerable<XYZList> FloorFacePoints, List<XYZ[]> ExWallEdges)
        {
            XYZList dlList = new XYZList();
            foreach (XYZList f in FloorFacePoints)
            {
                List<XYZ> floorPoints = f.XYZs;
                List<Line> WallEdges = f.Loop;
                List<XYZ> CentersOfMass = TriangleAndCentroid(floorPoints);
                dlList = new XYZList(CentersOfMass.Where(p => f.IsPointInside(p) &&
                CheckMinimumDistance(ExWallEdges, p, 0.2f)).Distinct());
            }
            if (dlList.XYZs == null)
                return new XYZList();
            return dlList.ChangeZValue(FloorFacePoints.First().XYZs.FirstOrDefault().Z + 0.9f);
        }
        public static Dictionary<string, List<Line>> GetAllRooms(XYZList groundPoints,
            float offsetDistance, string zoneName)
        {
            Dictionary<string, List<Line>> roomSegments = new Dictionary<string, List<Line>>();
            if (offsetDistance == 0)
            {
                roomSegments.Add(zoneName, groundPoints.Loop);
                return roomSegments;
            }
            else
            {
                List<Line> perimeterLines = groundPoints.Loop;
                List<Line> offsetLines = GetOffset(perimeterLines, offsetDistance);

                if (offsetLines.Count == perimeterLines.Count)
                {
                    for (int i = 0; i < perimeterLines.Count; i++)
                    {
                        Line[] cardinalLines = CreateDiagonalLines(perimeterLines[i], offsetLines[i]);
                        roomSegments.Add(string.Format("{0}:{1}", zoneName, i + 1),
                                            new List<Line>(){
                                                perimeterLines[i],
                                                cardinalLines[0],
                                                offsetLines[i].Reverse(),
                                                cardinalLines[1]
                                            });
                    }
                    roomSegments.Add(string.Format("{0}:0", zoneName), offsetLines);
                }
                else
                {
                    roomSegments.Add(string.Format("{0}:0", zoneName), perimeterLines);
                }
                return roomSegments;
            }
        }
        public static Line[] CreateDiagonalLines(Line perimeterLine, Line coreLine)
        {
            return new Line[]
            {
                new Line(perimeterLine.P1, coreLine.P1),
                new Line( coreLine.P0, perimeterLine.P0)
            };
        }
        
        public static bool ArePointsInside(this XYZList boundaryPoints, List<XYZ> points)
        {
            return points.
                    Select(p => p.ChangeZValue(0)).
                    Distinct().
                    All(po => boundaryPoints.IsPointInside(po));
        }

        public static bool CheckMinimumDistance(List<IDFObjects.XYZ[]> WallEdges, IDFObjects.XYZ point, float distance)
        {
            return WallEdges.All(wEdge => GetPerpendicularDistance(wEdge, point) > distance);
        }
        public static float GetPerpendicularDistance(XYZ[] WallEdge, XYZ p3)
        {
            XYZ p1 = WallEdge[0], p2 = WallEdge[1];
            float d = p2.Subtract(p1).CrossProduct(p1.Subtract(p3)).AbsoluteValue() / p2.DistanceTo(p1);
            return d;
        }
        public static XYZ CentroidOfTriangle(List<IDFObjects.XYZ> points)
        {
            return new XYZ(points.Select(p => p.X).Average(),
                points.Select(p => p.Y).Average(),
                points.Select(p => p.Z).Average());
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
            float x = Line[1].x - Line[0].x; float y = Line[1].y - Line[0].y;
            float dist = (float) Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            return new GridPoint(x / dist, y / dist);
        }
        public static bool IsCounterClockWise(List<GridPoint> points)
        {
            float area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                GridPoint point = points[i];
                float x1 = point.x, y1 = point.y;
                GridPoint nextPoint = new GridPoint();
                try
                {
                    nextPoint = points[i + 1];
                }
                catch
                {
                    nextPoint = points[0];
                }
                float x2 = nextPoint.x, y2 = nextPoint.y;
                area += (x2 - x1) * (y2 + y1);
            }
            bool returnVal = area < 0;
            return returnVal;
        }      
        public static float FtToM(float value)
        {
            return ((float) Math.Round(value * 0.3048, 4));
        }
        public static float MToFt(float value)
        {
            return ((float) Math.Round(value * 3.2808399, 4));
        }
        public static float SqFtToSqM(float value)
        {
            return ((float) Math.Round(value * 0.092903, 4));
        }
        public static float SqMToSqFt(float value)
        {
            return ((float) Math.Round(value / 0.092903, 4));
        }
        public static XYZ GetVerticalProjection(this XYZ point, IEnumerable<XYZList> faces)
        {
            return point.GetVerticalProjection(faces.FirstOrDefault(f => f.XYZs != null));
        }
        public static XYZ GetVerticalProjection(this XYZ point, XYZList face)
        {
            return point.ChangeZValue(face.XYZs.FirstOrDefault().Z);
        }
        public static void CreateZoneWalls(Zone z, ZoneGeometryInformation info)
        {
            for (int i=0; i < info.WallGeometryData.Count(); i++)
            {
                var roofCeilings = info.CeilingPoints.Union(info.RoofPoints);

                XYZ p1 = info.WallGeometryData.ElementAt(i).Value.P0,
                    p2 = info.WallGeometryData.ElementAt(i).Value.P1,
                    p3 = p2.GetVerticalProjection(roofCeilings), 
                    p4 = p1.GetVerticalProjection(roofCeilings);
                XYZList wallPoints = new XYZList( p1, p2, p3, p4 );
                Surface wall = new Surface(z, wallPoints, SurfaceType.Wall, p1.DistanceTo(p2) * p2.DistanceTo(p3));

                if (info.WallGeometryData.ElementAt(i).Key != "Outdoors")
                {
                    if (info.WallGeometryData.ElementAt(i).Key == "Adiabatic")
                    {
                        wall.OutsideCondition = "Adiabatic"; wall.OutsideObject = "";
                    }
                    else{
                        wall.OutsideCondition = "Zone"; wall.OutsideObject 
                            = info.WallGeometryData.ElementAt(i).Key;
                    }

                    wall.ConstructionName = "InternalWall";
                    wall.SunExposed = "NoSun"; wall.WindExposed = "NoWind";
                    wall.Fenestrations = null;
                }
            }
        }
        public static FieldInfo GetFieldByName<T>( string name)
        {
            return typeof(T).GetFields().First(a => a.Name == name);
        }
        public static void AdjustPDF(this ProbabilityDistributionFunction pdf, float fractionToKeep)
        {
            var sign = pdf.Sensitivity > 0 ? -1 : 1;
            pdf.Mean += sign * (1-fractionToKeep) * pdf.VariationOrSD;
            pdf.VariationOrSD *= fractionToKeep;
        }
        public static ProbabilisticBuildingDesignParameters GetProbabilisticParametersReduced(this ProbabilisticBuildingDesignParameters pars, float lim, float fractionToKeep)
        {
            ProbabilisticBuildingDesignParameters p = DeepClone(pars);
            typeof(ProbabilisticBuildingGeometry).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(p.pGeometry) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0 && pdf.Distribution != PDF.norm && Math.Abs(pdf.Sensitivity) > lim )
                        pdf.AdjustPDF(fractionToKeep);
                });
            typeof(ProbabilisticBuildingConstruction).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(p.pConstruction) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0 && pdf.Distribution != PDF.norm && Math.Abs(pdf.Sensitivity) > lim)
                        pdf.AdjustPDF(fractionToKeep);
                });
            typeof(ProbabilisticBuildingWWR).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(p.pWWR) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0 && pdf.Distribution != PDF.norm && Math.Abs(pdf.Sensitivity) > lim)
                        pdf.AdjustPDF(fractionToKeep);                   
                });
            typeof(ProbabilisticBuildingService).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(p.pService) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0 && pdf.Distribution != PDF.norm && Math.Abs(pdf.Sensitivity) > lim)
                        pdf.AdjustPDF(fractionToKeep);
                    
                });
            for (int i = 0; i < pars.zConditions.Count; i++)
            {
                typeof(ProbabilisticZoneConditions).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                   ForEach(a =>
                   {
                       ProbabilityDistributionFunction pdf = a.GetValue(pars.zConditions[i]) as ProbabilityDistributionFunction;
                       if (pdf.VariationOrSD != 0 && pdf.Distribution != PDF.norm && Math.Abs(pdf.Sensitivity) > lim)
                           pdf.AdjustPDF(fractionToKeep);
                   });
            }
            return p;
        }
        public static void GetProbabilisticParametersBasedOnSamples(this ProbabilisticBuildingDesignParameters pars, List<BuildingDesignParameters> samples)
        {
            typeof(ProbabilisticBuildingGeometry).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(pars.pGeometry) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0 && pdf.Distribution != PDF.norm)
                    {
                        float[] values = samples.Select(s => (float)GetFieldByName<BuildingGeometry>(a.Name).GetValue(s.Geometry)).ToArray();
                        pdf.Mean = 0.5f * (values.Max() + values.Min()) ;
                        pdf.VariationOrSD = 0.5f * (values.Max() - values.Min());
                    }
                });

            typeof(ProbabilisticBuildingConstruction).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(pars.pConstruction) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0 && pdf.Distribution != PDF.norm)
                    {
                        float[] values = samples.Select(s => (float)GetFieldByName<BuildingConstruction>(a.Name).GetValue(s.Construction)).ToArray();
                        pdf.Mean = 0.5f * (values.Max() + values.Min());
                        pdf.VariationOrSD = 0.5f * (values.Max() - values.Min());
                    }
                });

            typeof(ProbabilisticBuildingWWR).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(pars.pWWR) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0 && pdf.Distribution != PDF.norm)
                    {
                        float[] values = samples.Select(s => (float)GetFieldByName<BuildingWWR>(a.Name).GetValue(s.WWR)).ToArray();
                        pdf.Mean = 0.5f * (values.Max() + values.Min());
                        pdf.VariationOrSD = 0.5f * (values.Max() - values.Min());
                    }
                });

            typeof(ProbabilisticBuildingService).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                ForEach(a => {
                    ProbabilityDistributionFunction pdf = a.GetValue(pars.pService) as ProbabilityDistributionFunction;
                    if (pdf.VariationOrSD != 0 && pdf.Distribution != PDF.norm)
                    {
                        float[] values = samples.Select(s => (float)GetFieldByName<BuildingService>(a.Name).GetValue(s.Service)).ToArray();
                        pdf.Mean = 0.5f * (values.Max() + values.Min());
                        pdf.VariationOrSD = 0.5f * (values.Max() - values.Min());
                    }
                });

            for (int i = 0; i < pars.zConditions.Count; i++)
            {
                typeof(ProbabilisticZoneConditions).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().
                   ForEach(a =>
                   {
                       ProbabilityDistributionFunction pdf = a.GetValue(pars.zConditions[i]) as ProbabilityDistributionFunction;
                       if (pdf.VariationOrSD != 0 && pdf.Distribution != PDF.norm)
                       {
                           float[] values = samples.Select(s => (float)GetFieldByName<ZoneConditions>(a.Name).
                                                GetValue(s.ZConditions.First(zo=>zo.Name== pars.zConditions[i].Name))).ToArray();
                           pdf.Mean = 0.5f * (values.Max() + values.Min());
                           pdf.VariationOrSD = 0.5f * (values.Max() - values.Min());
                       }
                   });
            }
        }
        public static Dictionary<string, float[]> ConvertToDataframe(IEnumerable<string> csvFile, int round)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            IEnumerable<string> header = csvFile.ElementAt(0).Split(',').Skip(1);
            Dictionary<string, float[]> data = new Dictionary<string, float[]>();

            for (int i = 0; i < header.Count(); i++)
            {
                data.Add(header.ElementAt(i), new float[csvFile.Count() - 1]);
            }

            int r = 0;
            foreach (string s in csvFile.Skip(1)) 
            {
                string[] row = s.Split(',').Skip(1).ToArray();
                for (int c = 0; c < header.Count(); c++)
                {
                    data.ElementAt(c).Value[r] = (float) Math.Round(float.Parse(row[c]), round);
                }
                r++;
            }
            return data;
        }
        public static Dictionary<string, float[]> ReadSampleFile(string parFile, out int count)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            List<string> rawFile = File.ReadAllLines(parFile).Where(s => s[0] != '#').ToList();

            int nSamples = rawFile.Count;
            count = nSamples - 1;
            Dictionary<string, float[]> returnData = new Dictionary<string, float[]>();

            IEnumerable<string> header = rawFile[0].Split(',').ToList();

            for (int i = 0; i < header.Count(); i++)
            {
                float[] sampleData = new float[nSamples - 1];
                for (int s = 1; s < nSamples; s++)
                {
                    sampleData[s - 1] = float.Parse(rawFile[s].Split(',').ElementAt(i));
                }
                returnData.Add(header.ElementAt(i), sampleData);
            }
            return returnData;
        }
        public static PDFValues GetPDF(string[] data)
        { 
            return new PDFValues(float.Parse(data[0]), float.Parse(data[1]), GetValue<PDF>( data[2]));
        } 
        public static PDFValues GetProbabilisticParameter(this Dictionary<string, string[]> DataDictionary, string Parameter)
        {
            if (DataDictionary.ContainsKey(Parameter))
            {
                return GetPDF(DataDictionary[Parameter]);
            }
            else
            {
                return new PDFValues();
            }
        }
        public static float GetSensitivityValueForParameter(this Dictionary<string, float[]> DataDictionary, string Parameter)
        {
            if (DataDictionary.ContainsKey(Parameter))
                return DataDictionary[Parameter][0];
            else
                return 0;
            
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

            ProbabilisticBuildingDesignParameters value = new ProbabilisticBuildingDesignParameters();
            value.pGeometry = new ProbabilisticBuildingGeometry();
            value.pGeometry.Length.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pGeometry.Length.Label));
            value.pGeometry.Width.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pGeometry.Width.Label));
            value.pGeometry.Height.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pGeometry.Height.Label));
            value.pGeometry.FloorArea.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pGeometry.FloorArea.Label));
            value.pGeometry.NFloors.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pGeometry.NFloors.Label));
            value.pGeometry.Shape.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pGeometry.Shape.Label));
            value.pGeometry.ARatio.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pGeometry.ARatio.Label));
            value.pGeometry.Orientation.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pGeometry.Orientation.Label));

            value.pConstruction = new ProbabilisticBuildingConstruction();
            value.pConstruction.InternalMass.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pConstruction.InternalMass.Label));
            value.pConstruction.UWall.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pConstruction.UWall.Label));
            value.pConstruction.UGFloor.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pConstruction.UGFloor.Label));
            value.pConstruction.URoof.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pConstruction.URoof.Label));
            value.pConstruction.UWindow.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pConstruction.UWindow.Label));
            value.pConstruction.GWindow.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pConstruction.GWindow.Label));
            value.pConstruction.Infiltration.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pConstruction.Infiltration.Label));
            value.pConstruction.Permeability.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pConstruction.Permeability.Label));
            value.pConstruction.UIFloor.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pConstruction.UIFloor.Label));
            value.pConstruction.UIWall.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pConstruction.UIWall.Label));
            value.pConstruction.HCSlab.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pConstruction.HCSlab.Label));

            value.pWWR = new ProbabilisticBuildingWWR();
            value.pWWR.North.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pWWR.North.Label));
            value.pWWR.East.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pWWR.East.Label));
            value.pWWR.West.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pWWR.West.Label));
            value.pWWR.South.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pWWR.South.Label));

            value.pService = new ProbabilisticBuildingService();               
            value.pService.BoilerEfficiency.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pService.BoilerEfficiency.Label));
            value.pService.HeatingCOP.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pService.HeatingCOP.Label));
            value.pService.CoolingCOP.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(value.pService.CoolingCOP.Label));

            foreach (string zlN in zoneListNames)
            {                
                ProbabilisticZoneConditions zc = new ProbabilisticZoneConditions();
                zc.Name = zlN;
                zc.LHG.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(zlN + ":" + zc.LHG.Label));
                zc.EHG.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(zlN + ":" + zc.EHG.Label));
                zc.StartTime.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(zlN + ":" + zc.StartTime.Label));
                zc.OperatingHours.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(zlN + ":" + zc.OperatingHours.Label));
                zc.Occupancy.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(zlN + ":" + zc.Occupancy.Label));
                zc.HeatingSetpoint.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(zlN + ":" + zc.HeatingSetpoint.Label));
                zc.CoolingSetpoint.UpdateProbabilityDistributionFunction(dataDict.GetProbabilisticParameter(zlN + ":" + zc.CoolingSetpoint.Label));
                value.zConditions.Add(zc);
            }
            return value;
        }
        
        public static List<ScheduleCompact> GetSchedulesFromFolder(string folder)
        {
            string zoneListName = new DirectoryInfo(folder).Name;
            List<ScheduleCompact> schedules = new List<ScheduleCompact>();
            foreach(string f in Directory.EnumerateFiles(folder, "*.csv"))
            {
                try
                {
                    schedules.Add(new ScheduleCompact(string.Format("{0}_{1}Schedule", zoneListName, new FileInfo(f).Name.Split('.')[0]), File.ReadAllLines(f).ToList()));                    
                }
                catch
                {

                }
            }
            return schedules;
        }
        public static List<List<List<GridPoint>>> ParseTextFileForGridPoints(string textFile)
        {
            List<List<List<GridPoint>>> rValues = new List<List<List<GridPoint>>>();

            using (StreamReader reader = new StreamReader(textFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "start")
                    {
                        List<string[]> pInfo = new List<string[]>();
                        while ((line = reader.ReadLine()) != "end")
                        {
                            pInfo.Add(line.Split('\t').Skip(1).ToArray());
                        }
                        rValues.Add(GetPointInfo(pInfo));
                    }
                }
            }

            return rValues;
        }

        static List<List<GridPoint>> GetPointInfo(List<string[]> text)
        {
            text.Reverse();
            List<List<GridPoint>> points = new List<List<GridPoint>>();
            for (int f = 0; f < text.Select(t=>t.Count()).Max(); f++)
                points.Add(new List<GridPoint>());

            for (int r = 0; r < text.Count; r++)
            {
                var line = text[r];

                int f = 0;
                foreach (string vals in line)
                {
                    int c = 0;
                    foreach (char ch in vals)
                    {
                        if (ch == 'X')
                            points[f].Add(new GridPoint(c, r));
                        c++;
                    }
                    f++;
                }
            }
            return points;
        }
        public static List<ScheduleCompact> GetSchedulesFromFolderWithZoneNames(string folder)
        { 
            string[] zoneListName = Directory.EnumerateDirectories(folder).ToArray();
            List<ScheduleCompact> schedules = new List<ScheduleCompact>();
            foreach (string f in zoneListName)
            {
                schedules.AddRange(GetSchedulesFromFolder(f));
            }
            return schedules;
        }
        public static float GetSamplesValues(this Dictionary<string, float[]> DataDictionary, string Parameter, int Number)
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
        public static Queue<BuildingDesignParameters> ReadBuildingDesignParameters(string dataFile)
        {
            Dictionary<string, float[]> samples = ReadSampleFile(dataFile, out int Count);
            List<string> zoneListNames = samples.Keys.Where(z => z.Contains(':'))
                .Select(s => s.Split(':')[0]).Distinct().ToList();

            var values = new Queue<BuildingDesignParameters>();
            ProbabilisticBuildingGeometry pGeometry = new ProbabilisticBuildingGeometry();
            ProbabilisticBuildingConstruction pConstruction = new ProbabilisticBuildingConstruction();
            ProbabilisticBuildingWWR pWWR = new ProbabilisticBuildingWWR();
            ProbabilisticBuildingService pService = new ProbabilisticBuildingService();
            ProbabilisticZoneConditions zCondition = new ProbabilisticZoneConditions();
            for (int s = 0; s < Count; s++)
            {
                BuildingDesignParameters value = new BuildingDesignParameters();
                value.Geometry = new BuildingGeometry()
                {
                    Length = samples.GetSamplesValues(pGeometry.Length.Label, s),
                    Width = samples.GetSamplesValues(pGeometry.Width.Label, s),
                    Height = samples.GetSamplesValues(pGeometry.Height.Label, s),
                    FloorArea = samples.GetSamplesValues(pGeometry.FloorArea.Label, s),
                    NFloors = (int)Math.Round(samples.GetSamplesValues(pGeometry.NFloors.Label, s)),
                    Shape = (int)samples.GetSamplesValues(pGeometry.Shape.Label, s),
                    ARatio = samples.GetSamplesValues(pGeometry.ARatio.Label, s),
                    Orientation = (int)Math.Round(samples.GetSamplesValues(pGeometry.Orientation.Label, s)),

                    rLenA = samples.GetSamplesValues(pGeometry.rLenA.Label, s),
                    rWidA = samples.GetSamplesValues(pGeometry.rWidA.Label, s),
                    BasementDepth = samples.GetSamplesValues(pGeometry.BasementDepth.Label, s),
                };
                value.Construction = new BuildingConstruction()
                {
                    InternalMass = samples.GetSamplesValues(pConstruction.InternalMass.Label, s),
                    UWall = samples.GetSamplesValues(pConstruction.UWall.Label, s),
                    UGFloor = samples.GetSamplesValues(pConstruction.UGFloor.Label, s),
                    URoof = samples.GetSamplesValues(pConstruction.URoof.Label, s),
                    UWindow = samples.GetSamplesValues(pConstruction.UWindow.Label, s),
                    GWindow = samples.GetSamplesValues(pConstruction.GWindow.Label, s),
                    Infiltration = samples.GetSamplesValues(pConstruction.Infiltration.Label, s),
                    Permeability = samples.GetSamplesValues(pConstruction.Permeability.Label, s),
                    UIFloor = samples.GetSamplesValues(pConstruction.UIFloor.Label, s),
                    UIWall = samples.GetSamplesValues(pConstruction.UIWall.Label, s),
                    HCSlab = samples.GetSamplesValues(pConstruction.HCSlab.Label, s),
                    UCWall = samples.GetSamplesValues(pConstruction.UWall.Label, s)
                };
                value.WWR = new BuildingWWR()
                {
                    North = samples.GetSamplesValues(pWWR.North.Label, s),
                    East = samples.GetSamplesValues(pWWR.East.Label, s),
                    West = samples.GetSamplesValues(pWWR.West.Label, s),
                    South = samples.GetSamplesValues(pWWR.South.Label, s),
                };
                value.Service = new BuildingService()
                {
                    BoilerEfficiency = samples.GetSamplesValues(pService.BoilerEfficiency.Label, s),
                    HeatingCOP = samples.GetSamplesValues(pService.HeatingCOP.Label, s),
                    CoolingCOP = samples.GetSamplesValues(pService.CoolingCOP.Label, s)
                };
                foreach (string zlN in zoneListNames)
                {
                    value.ZConditions.Add(new ZoneConditions()
                    {
                        Name = zlN,
                        LHG = samples.GetSamplesValues(zlN + ":" + zCondition.LHG.Label, s),
                        EHG = samples.GetSamplesValues(zlN + ":" + zCondition.EHG.Label, s),
                        StartTime = samples.GetSamplesValues(zlN + ":" + zCondition.StartTime.Label, s),
                        OperatingHours = samples.GetSamplesValues(zlN + ":" + zCondition.OperatingHours.Label, s),
                        Occupancy = samples.GetSamplesValues(zlN + ":" + zCondition.Occupancy.Label, s),
                        HeatingSetpoint = samples.GetSamplesValues(zlN + ":" + zCondition.HeatingSetpoint.Label, s),
                        CoolingSetpoint = samples.GetSamplesValues(zlN + ":" + zCondition.CoolingSetpoint.Label, s)
                    });
                }
                value.Name = $"Shape_{value.Geometry.Shape}_{s:000000}";
                values.Enqueue( value );
            }
            return values;
        }

        public static T DeepClone<T>(this T obj)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
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
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (File.Exists(filePath)) { File.Delete(filePath); }
            TextWriter tW = File.CreateText(filePath);
            new JsonSerializer() { Formatting = Formatting.Indented }.Serialize(tW, obj);
            tW.Close();
        }
        public static T DeSerialise<T>(string filePath)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            using (var tR = File.OpenText(filePath))
            {
                T val = new JsonSerializer().Deserialize<T>(new JsonTextReader(tR));
                return val;
            }
        }
        public static float ConvertKWhfromJoule(this float d) { return d * 2.7778E-7f; }
        public static float ConvertWfromJoule(this float d) { return d * 2.7778E-4f / 8760; }
        public static float[] ConvertWfromJoule(this float[] dArray) { return dArray.Select(d => d.ConvertWfromJoule()).ToArray(); }
        public static float[] ConvertKWhfromJoule(this float[] dArray) { return dArray.Select(d => d.ConvertKWhfromJoule()).ToArray(); }
        public static float[] FillZeroes(this float[] Array, int length)
        {
            int count = Array.Count();
            IEnumerable<float> newList = Array;

            for (int i = count; i < length; i++) { newList = newList.Append(0); }
            return newList.ToArray();
        }
        public static float ConvertKWhafromW(this float d)
        {
            return d*8.76f;
        }
        public static float ConvertKWhafromWm(this float d)
        {
            return d * 8.76f/12f;
        }

        public static float[] ConvertKWhafromW(this float[] dArray)
        {
            return dArray.Select(d => d.ConvertKWhafromW()).ToArray();
        }
        public static float[] ConvertKWhafromWm(this float[] dArray)
        {
            return dArray.Select(d => d.ConvertKWhafromWm()).ToArray();
        }
        public static float[] MultiplyBy(this float[] dArray, float factor)
        {
            return dArray.Select(d => d*factor).ToArray();
        }
        public static float[] AddArrayElementWise(this List<float[]> AllArrays)
        {
            AllArrays = AllArrays.Where(a => a != null).ToList();
            List<int> counts = AllArrays.Select(a => a.Count()).ToList();
            if (counts.Count == 0) { return new float[] { 0 }; }
            else
            {
                int n = counts.Max();

                AllArrays = AllArrays.Select(a => a.FillZeroes(n)).ToList();

                float[] array = new float[n];

                for (int i = 0; i < n; i++)
                {
                    array[i] = AllArrays.Select(a => a[i]).Sum();
                }
                return array;
            }

        }
        public static float[] SubtractArrayElementWise(this float[] FirstArray, float[] SecondArray)
        {
            List<int> counts = new List<int>() { FirstArray.Count(), SecondArray.Count() };
            int n = counts.Max();

            FirstArray = FirstArray.FillZeroes(n); SecondArray = SecondArray.FillZeroes(n);

            float[] array = new float[n];
            for (int i = 0; i < n; i++)
            {
                array[i] = FirstArray[i] - SecondArray[i];
            }
            return array;
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
            var sdd = new SizingPeriodDesignDay()
            {
                name = "Summer including Extreme Summer days",
                strData = new List<string>() { "7,18,7,25,SummerDesignDay, Yes, Yes;" }
            };
            var wdd = new SizingPeriodDesignDay()
            {
                name = "Winter including Extreme Winter days",
                strData = new List<string>() { "1,25,2,1,WinterDesignDay, Yes, Yes;" }
            };
            return new List<SizingPeriodDesignDay>() { sdd, wdd };
        }
    }
}
