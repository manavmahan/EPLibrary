using System;
using System.Collections.Generic;
using System.Linq;

namespace IDFObjects
{
    public class RandomBuilding
    {
        GridPoint StartPoint;
        List<GridPoint> BuildingGrid;

        public float EdgeSize;
        List<GridPoint> AllowedPoints = new List<GridPoint>();

        public int NFloors = 0;
        public float Orientation = 0;
        int RequiredGridPoints;

        public List<XYZList>[] Floors, Ceilings, Roofs, OverhangFloors;
        public List<XYZList>[] Walls;
        public XYZList ScaledLoop;
        public float Area;

        public RandomBuilding() { }
        public RandomBuilding(float totalFloorArea, GridPointGeometry geometry, float orientation)
        {
            RequiredGridPoints = geometry.NPoints;
            Orientation = orientation;
            EdgeSize = (float)Math.Round(Math.Sqrt(totalFloorArea / RequiredGridPoints), 2);
            NFloors = geometry.NFloor;
            
            Floors = new List<XYZList>[NFloors];
            Roofs = new List<XYZList>[NFloors];
            Ceilings = new List<XYZList>[NFloors];
            OverhangFloors = new List<XYZList>[NFloors];
            Walls = new List<XYZList>[NFloors];

            for (int f=0; f<NFloors; f++)
            {
                Floors[f] = geometry.Floors[f].Select(p => LoopEdgesScaleRotate(p)).ToList();
                Roofs[f] = geometry.Roofs[f].Select(p => LoopEdgesScaleRotate(p)).ToList();
                Ceilings[f] = geometry.Ceilings[f].Select(p => LoopEdgesScaleRotate(p)).ToList();
                OverhangFloors[f] = geometry.OverhangFloors[f].Select(p => LoopEdgesScaleRotate(p)).ToList();
                Walls[f] = geometry.Walls[f].Select(p => LoopEdgesScaleRotate(p)).ToList();
            }
            float minX = Floors.Concat(OverhangFloors).SelectMany(
                x => x.SelectMany(p => p.XYZs.Select(p1 => p1.X))).Min();
            float minY = Floors.Concat(OverhangFloors).SelectMany(
                x => x.SelectMany(p => p.XYZs.Select(p1 => p1.Y))).Min();

            XYZ origin = new XYZ(minX, minY);
            for (int f=0; f<NFloors; f++)
            {
                Floors[f] = Floors[f].Select(l => l.Transform(origin)).ToList();
                Roofs[f] = Roofs[f].Select(l => l.Transform(origin)).ToList();
                Ceilings[f] = Ceilings[f].Select(l => l.Transform(origin)).ToList();
                OverhangFloors[f] = OverhangFloors[f].Select(l => l.Transform(origin)).ToList();
                Walls[f] = Walls[f].Select(l => l.Transform(origin)).ToList();
            }
        }
        public RandomBuilding(float area, float minimumEdgeLength, float maxEdgeLength, uint maxBoxes, XYZList site, float Orientation, Random random)
        {
            int minPoints = (int) Math.Ceiling(area / (maxEdgeLength * maxEdgeLength));
            int maxPoints = (int) Math.Floor(area / (minimumEdgeLength * minimumEdgeLength));

            try { RequiredGridPoints = random.Next(minPoints, maxPoints); }
            catch { RequiredGridPoints = maxPoints; }

            RequiredGridPoints = (int) Math.Min(maxBoxes, RequiredGridPoints);

            EdgeSize = (float) Math.Round(Math.Sqrt(area / RequiredGridPoints),2);
            
            XYZList sPoints = site.Transform(-Orientation); 
            
            var scSitePoints = Utility.GetOffset(sPoints, 0.5f * EdgeSize * (float) Math.Cos(2));
            
            float minX = scSitePoints.XYZs.Select(x => x.X).Min();
            float maxX = scSitePoints.XYZs.Select(x => x.X).Max();
            float minY = scSitePoints.XYZs.Select(x => x.Y).Min();
            float maxY = scSitePoints.XYZs.Select(x => x.Y).Max();
            List<Line> SiteEdges = scSitePoints.Loop;

            Area = RequiredGridPoints * EdgeSize * EdgeSize;
            float d = 0.1f;
            List<List<GridPoint>> aPoints = new List<List<GridPoint>>();

            for (float xd = 0; xd < 1; xd += d)
            {
                for (float yd = 0; yd < 1; yd += d)
                { 
                    List<GridPoint> a = new List<GridPoint>();
                    for (float x = (float) Math.Floor(minX-xd); x <= Math.Ceiling(maxX+xd); x++)
                    {
                        for (float y = (float) Math.Floor(minY-yd); y <= Math.Ceiling(maxY+yd); y++)
                        {
                            if (site.IsPointInside(new XYZ(x, y, 0)))
                            {
                                a.Add(new GridPoint(x, y));
                            }
                        }
                    }
                    aPoints.Add(a);
                }
            }
            AllowedPoints = aPoints.First(s=>s.Count() == aPoints.Select(se => se.Count()).Max());
            
            BuildingGrid = new List<GridPoint> { };          
            StartPoint = GetCenter(AllowedPoints);
            BuildingGrid.Add(StartPoint);
        }
        public GridPoint GetCenter(List<GridPoint> ps)
        {
            List<float> ds = new List<float>();
            foreach (GridPoint p in ps)
            {
                ds.Add( ps.Select(p1 => (p.x - p1.x) * (p.x - p1.x) + (p.y - p1.y) * (p.y - p1.y)).Sum());
            }
            return ps.ElementAt(ds.IndexOf(ds.Min()));
        }        
        public XYZList GenerateBuilding(Random random)
        {
            if (AllowedPoints.Count() < RequiredGridPoints)
                return null;
            else
            {
                while (BuildingGrid.Count < RequiredGridPoints)
                {
                    List<GridPoint> Candidates = ExtractGridPointsLRUD(BuildingGrid);
                    Candidates = Candidates.FindAll(x => AllowedPoints.Contains(x));

                    List<GridPoint> ToRemoveGrid = GetCrossTranslation();
                    Candidates = Candidates.FindAll(x => !ToRemoveGrid.Contains(x));
                    int pnttoadd = random.Next(Candidates.Count());
                    BuildingGrid.Add(Candidates[pnttoadd]);
                }
                return LoopEdgesScaleRotate(BuildingGrid);
            }
        }
        public List<GridPoint> ExtractGridPointsLRUD(List<GridPoint> grids)
        {
            List<GridPoint> returnArr = new List<GridPoint> { };
            foreach (GridPoint Grid in grids)
            {
                List<GridPoint> GridNeighbours = Grid.GetAllNeighbours();
                GridNeighbours.RemoveAll(x => (grids.Contains(x)));
                if (GridNeighbours.Count() == 4)
                {
                    returnArr = returnArr.Concat(GridNeighbours).ToList();
                }
                else
                {
                    foreach (GridPoint Square in GridNeighbours)
                    {
                        List<GridPoint> Squares = Square.GetAllNeighbours();
                        Squares.RemoveAll(x => !(grids.Contains(x)));
                        if (Squares.Count() == 1)
                        {
                            returnArr.Add(Square);
                        }
                        if (Squares.Count() == 2)
                        {
                            if ((Math.Abs(Squares[1].x - Squares[0].x) != 2) & (Math.Abs(Squares[1].x - Squares[0].x) != 0))
                            {
                                returnArr.Add(Square);
                            }
                            if ((Math.Abs(Squares[1].y - Squares[0].y) != 2) & (Math.Abs(Squares[1].y - Squares[0].y) != 0))
                            {
                                returnArr.Add(Square);
                            }
                        }
                        if (Squares.Count() == 3)
                        {
                            returnArr.Add(Square);
                        }
                    }
                }
            }
            return returnArr;
        }
        public List<GridPoint> GetCrossTranslation()
        {
            List<GridPoint> AllPointsToOutputAndRemove = new List<GridPoint> { };
            foreach (GridPoint Point in BuildingGrid)
            {
                List<GridPoint> AllNeighbours = Point.GetAllNeighbours();
                List<GridPoint> AllNeighboursWithinBuilding = BuildingGrid.FindAll(x => AllNeighbours.Contains(x));

                List<int> CalcNeighTrue = AllNeighbours.Select(x => AllNeighboursWithinBuilding.Contains(x) ? 0 : 1).ToList();
                List<int> CalcNeighFalse = CalcNeighTrue.Select(x => x == 0 ? 1 : 0).ToList();

                CalcNeighFalse = CalcNeighFalse.Skip(1).Concat(CalcNeighFalse.Take(1)).ToList();

                List<int> FinalCalc = CalcNeighTrue.Select((x, i) => x - CalcNeighFalse[i]).ToList().Select(x => x < 0 ? 0 : x).ToList();

                List<List<int>> ToAddToPoint = new List<List<int>> { new List<int> { 0, -1 }, new List<int> { 1, 0 }, new List<int> { 0, 1 }, new List<int> { -1, 0 } };

                ToAddToPoint = ToAddToPoint.Select((x, i) => x = x.Select(y => y *= FinalCalc[i]).ToList()).ToList();
                List<GridPoint> AllCrossPointsToRemove = IDFObjects.Utility.DeepClone(AllNeighbours);

                AllCrossPointsToRemove.ForEach(point => {
                    point.x = point.x + ToAddToPoint[AllCrossPointsToRemove.IndexOf(point)][0];
                    point.y = point.y + ToAddToPoint[AllCrossPointsToRemove.IndexOf(point)][1];
                });
                AllPointsToOutputAndRemove.AddRange(IDFObjects.Utility.DeepClone(AllCrossPointsToRemove.Except(AllNeighbours).ToList()));
            }
            return AllPointsToOutputAndRemove;
        }
        public List<List<GridPoint>> ReturnEdges(List<GridPoint> buildingGrid)
        {
            List<List<GridPoint>>  edges = new List<List<GridPoint>> { };
            foreach (GridPoint item in buildingGrid)
            {
                item.GetAllNeighbours();
                if (!(buildingGrid.Contains(item.Left)))
                {
                    List<GridPoint> ToAdd = new List<GridPoint> { new GridPoint(item.x - 0.5f, item.y - 0.5f), new GridPoint(item.x - 0.5f, item.y + 0.5f) };
                    edges.Add(ToAdd);
                }
                if (!(buildingGrid.Contains(item.Right)))
                {
                    List<GridPoint> ToAdd = new List<GridPoint> { new GridPoint(item.x + 0.5f, item.y - 0.5f), new GridPoint(item.x + 0.5f, item.y + 0.5f) };
                    edges.Add(ToAdd);
                }
                if (!(buildingGrid.Contains(item.Up)))
                {
                    List<GridPoint> ToAdd = new List<GridPoint> { new GridPoint(item.x - 0.5f, item.y + 0.5f), new GridPoint(item.x + 0.5f, item.y + 0.5f) };
                    edges.Add(ToAdd);
                }
                if (!(buildingGrid.Contains(item.Down)))
                {
                    List<GridPoint> ToAdd = new List<GridPoint> { new GridPoint(item.x - 0.5f, item.y - 0.5f), new GridPoint(item.x + 0.5f, item.y - 0.5f) };
                    edges.Add(ToAdd);
                }
            }
            edges = edges.Distinct().ToList();
            return edges;
        }
        public List<List<GridPoint>> GetLoop(List<List<GridPoint>> edges)
        {
            List<List<GridPoint>> loop = new List<List<GridPoint>> { edges[0] };
            edges.RemoveAt(0);
            while (edges.Count() > 0)
            {
                List<GridPoint> lastLine = loop.Last();
                List<GridPoint> ContinuousLineFound;
                try
                {
                    ContinuousLineFound = edges.First(line => line[0].x == lastLine[1].x && line[0].y == lastLine[1].y);
                    edges.Remove(ContinuousLineFound);
                }
                catch
                {
                    ContinuousLineFound = edges.First(line => line[1].x == lastLine[1].x && line[1].y == lastLine[1].y);
                    edges.Remove(ContinuousLineFound);
                    ContinuousLineFound.Reverse();
                }

                if (!IDFObjects.Utility.GetDirection(lastLine).Equals(IDFObjects.Utility.GetDirection(ContinuousLineFound)))
                {
                    loop.Add(ContinuousLineFound);
                }
                else
                {
                    lastLine[1] = ContinuousLineFound[1];
                    loop[loop.Count - 1] = lastLine;
                }
            }
            if (IDFObjects.Utility.GetDirection(loop.Last()).Equals(IDFObjects.Utility.GetDirection(loop.First())))
            {
                loop[0][0] = loop.Last()[0];
                loop.RemoveAt(loop.Count - 1);
            }
            if (!IDFObjects.Utility.IsCounterClockWise(loop.Select(l => l[0]).ToList()))
            {
                loop.Reverse();
            }
            return loop;
        }
        public XYZList ScaleBuilding(List<List<GridPoint>> Loop)
        {
            List<XYZ> scaledLoop = new List<XYZ>();
            Loop.ForEach(line => scaledLoop.Add(new XYZ(EdgeSize * line[0].x, EdgeSize * line[0].y, 0)));
            return new XYZList(scaledLoop);
        }

        public XYZList LoopEdgesScaleRotate(List<GridPoint> points)
        {
            if (points.Count > 0)
                return ScaleBuilding(GetLoop(ReturnEdges(points))).Transform(Orientation);
            else
                return (XYZList) null;
        }

        public XYZList ScaleBuilding(XYZList loop)
        {
            return new XYZList(loop.XYZs.Select(p => new XYZ(p.X * EdgeSize, p.Y * EdgeSize, 0)));
        }
        public List<XYZList> ScaleBuilding(List<XYZList> loops)
        {
            return loops.Select(l => ScaleBuilding(l)).ToList();
        }
        
    }

}
