using IDFObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace IDFObjects
{
    public class RandomBuilding
    {
        public GridPoint StartPoint;
        public List<GridPoint> BuildingGrid;

        public float EdgeSize;
        public float Area;
        public int RequiredGridPoints;

        public List<GridPoint> AllowedPoints = new List<GridPoint>();

        List<List<GridPoint>> Edges, Loop; public List<IDFObjects.XYZ> ScaledLoop;
        
        public RandomBuilding() { }
        public RandomBuilding(float area, float minimumEdgeLength, float maxEdgeLength, int maxBoxes, XYZList SitePoints, float Orientation, Random random)
        {
            int minPoints = (int) Math.Ceiling(area / (maxEdgeLength * maxEdgeLength));
            int maxPoints = (int) Math.Floor(area / (minimumEdgeLength * minimumEdgeLength));

            try { RequiredGridPoints = random.Next(minPoints, maxPoints); }
            catch { RequiredGridPoints = maxPoints; }

            RequiredGridPoints = Math.Min(maxBoxes, RequiredGridPoints);

            EdgeSize = (float) Math.Round(Math.Sqrt(area / RequiredGridPoints),2);
            
            XYZList sPoints = IDFObjects.Utility.DeepClone(SitePoints);
            sPoints.Transform(-Orientation); 
            
            List<XYZ> scSitePoints = Utility.GetOffset(sPoints.xyzs, 0.5f * EdgeSize * (float) Math.Cos(2));
            scSitePoints = sPoints.xyzs.Select(x => new XYZ(x.X / EdgeSize, x.Y / EdgeSize, x.Z)).ToList();
            

            float minX = scSitePoints.Select(x => x.X).Min();
            float maxX = scSitePoints.Select(x => x.X).Max();
            float minY = scSitePoints.Select(x => x.Y).Min();
            float maxY = scSitePoints.Select(x => x.Y).Max();
            List<Line> SiteEdges = IDFObjects.Utility.GetExternalEdges(scSitePoints);

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
                            if (IDFObjects.Utility.PointInsideLoopExceptZ(SiteEdges, new XYZ(x, y, 0)))
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
        public RandomBuilding(float area, int requiredGridPoints, float maxLength, float maxWidth, float minimumEdgeLength)
        {
            RequiredGridPoints = requiredGridPoints;
            EdgeSize = (float) Math.Max(Math.Round(Math.Sqrt(area / requiredGridPoints)), minimumEdgeLength);

            RequiredGridPoints = (int)(area / (EdgeSize * EdgeSize));
            Area = RequiredGridPoints * EdgeSize * EdgeSize;

            int minX = 0, minY = 0, maxX = (int)Math.Ceiling(maxLength / EdgeSize), maxY = (int)Math.Ceiling(maxWidth / EdgeSize);
            for (int x = minX; x<maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    AllowedPoints.Add(new GridPoint(x, y));
                }
            }
            BuildingGrid = new List<GridPoint> { };
            StartPoint = new GridPoint((float) Math.Ceiling((float)(maxX - minX) / 2), (float)Math.Ceiling((float)(maxX - minX) / 2));
            BuildingGrid.Add(StartPoint);
        }
        public bool GenerateBuilding(Random random)
        {
            if (AllowedPoints.Count() < RequiredGridPoints)
                return false;
            else
            {
                while (BuildingGrid.Count < RequiredGridPoints)
                {
                    List<GridPoint> Candidates = ExtractGridPointsLRUD();
                    Candidates = Candidates.FindAll(x => AllowedPoints.Contains(x));

                    List<GridPoint> ToRemoveGrid = GetCrossTranslation();
                    Candidates = Candidates.FindAll(x => !ToRemoveGrid.Contains(x));
                    int pnttoadd = random.Next(Candidates.Count());
                    BuildingGrid.Add(Candidates[pnttoadd]);
                }
                ReturnEdges();
                GetLoop();
                ScaleBuilding();
                return true;
            }
        }
        public List<GridPoint> ExtractGridPointsLRUD()
        {
            List<GridPoint> ReturnArr = new List<GridPoint> { };
            foreach (GridPoint Grid in BuildingGrid)
            {
                List<GridPoint> GridNeighbours = Grid.GetAllNeighbours();
                GridNeighbours.RemoveAll(x => (BuildingGrid.Contains(x)));
                if (GridNeighbours.Count() == 4)
                {
                    ReturnArr = ReturnArr.Concat(GridNeighbours).ToList();
                }
                else
                {
                    foreach (GridPoint Square in GridNeighbours)
                    {
                        List<GridPoint> Squares = Square.GetAllNeighbours();
                        Squares.RemoveAll(x => !(BuildingGrid.Contains(x)));
                        if (Squares.Count() == 1)
                        {

                            ReturnArr.Add(Square);
                        }
                        if (Squares.Count() == 2)
                        {
                            if ((Math.Abs(Squares[1].x - Squares[0].x) != 2) & (Math.Abs(Squares[1].x - Squares[0].x) != 0))
                            {
                                ReturnArr.Add(Square);
                            }
                            if ((Math.Abs(Squares[1].y - Squares[0].y) != 2) & (Math.Abs(Squares[1].y - Squares[0].y) != 0))
                            {
                                ReturnArr.Add(Square);
                            }
                        }
                        if (Squares.Count() == 3)
                        {
                            ReturnArr.Add(Square);
                        }
                    }
                }
            }
            return ReturnArr;
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
        public void ReturnEdges()
        {
            Edges = new List<List<GridPoint>> { };
            foreach (GridPoint item in BuildingGrid)
            {
                item.GetAllNeighbours();
                if (!(BuildingGrid.Contains(item.Left)))
                {
                    List<GridPoint> ToAdd = new List<GridPoint> { new GridPoint(item.x - 0.5f, item.y - 0.5f), new GridPoint(item.x - 0.5f, item.y + 0.5f) };
                    Edges.Add(ToAdd);
                }
                if (!(BuildingGrid.Contains(item.Right)))
                {
                    List<GridPoint> ToAdd = new List<GridPoint> { new GridPoint(item.x + 0.5f, item.y - 0.5f), new GridPoint(item.x + 0.5f, item.y + 0.5f) };
                    Edges.Add(ToAdd);
                }
                if (!(BuildingGrid.Contains(item.Up)))
                {
                    List<GridPoint> ToAdd = new List<GridPoint> { new GridPoint(item.x - 0.5f, item.y + 0.5f), new GridPoint(item.x + 0.5f, item.y + 0.5f) };
                    Edges.Add(ToAdd);
                }
                if (!(BuildingGrid.Contains(item.Down)))
                {
                    List<GridPoint> ToAdd = new List<GridPoint> { new GridPoint(item.x - 0.5f, item.y - 0.5f), new GridPoint(item.x + 0.5f, item.y - 0.5f) };
                    Edges.Add(ToAdd);
                }
            }
            Edges = Edges.Distinct().ToList();
        }
        public void GetLoop()
        {
            Loop = new List<List<GridPoint>> { Edges[0] };
            Edges.RemoveAt(0);
            while (Edges.Count() > 0)
            {
                List<GridPoint> lastLine = Loop.Last();
                List<GridPoint> ContinuousLineFound;
                try
                {
                    ContinuousLineFound = Edges.First(line => line[0].x == lastLine[1].x && line[0].y == lastLine[1].y);
                    Edges.Remove(ContinuousLineFound);
                }
                catch
                {
                    ContinuousLineFound = Edges.First(line => line[1].x == lastLine[1].x && line[1].y == lastLine[1].y);
                    Edges.Remove(ContinuousLineFound);
                    ContinuousLineFound.Reverse();
                }

                if (!IDFObjects.Utility.GetDirection(lastLine).Equals(IDFObjects.Utility.GetDirection(ContinuousLineFound)))
                {
                    Loop.Add(ContinuousLineFound);
                }
                else
                {
                    lastLine[1] = ContinuousLineFound[1];
                    Loop[Loop.Count - 1] = lastLine;
                }
            }
            if (IDFObjects.Utility.GetDirection(Loop.Last()).Equals(IDFObjects.Utility.GetDirection(Loop.First())))
            {
                Loop[0][0] = Loop.Last()[0];
                Loop.RemoveAt(Loop.Count - 1);
            }
            if (!IDFObjects.Utility.IsCounterClockWise(Loop.Select(l => l[0]).ToList()))
            {
                Loop.Reverse();
            }
        }
        public void ScaleBuilding()
        {
            ScaledLoop = new List<IDFObjects.XYZ>();
            Loop.ForEach(line => ScaledLoop.Add(new IDFObjects.XYZ(EdgeSize * line[0].x, EdgeSize * line[0].y, 0)));
        }
    }
}
