using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    public class GridPointGeometry
    {
        public int NPoints, NFloor;
        public List<List<List<GridPoint>>> Floors = new List<List<List<GridPoint>>>(), Roofs = new List<List<List<GridPoint>>>(), 
            Ceilings = new List<List<List<GridPoint>>>(), OverhangFloors = new List<List<List<GridPoint>>>();

        public List<List<List<GridPoint>>> Walls = new List<List<List<GridPoint>>>();

        public GridPointGeometry() { }
        public GridPointGeometry(List<List<GridPoint>> gridPoints) {
            NFloor = gridPoints.Count();
            NPoints = 0;

            for (int f=0; f<NFloor; f++)
            {
                NPoints += gridPoints[f].Count;
                Walls.Add(DifferentiateLoops(gridPoints[f]));
                
                Floors.Add(new List<List<GridPoint>>());
                if (f == 0)
                {
                    Floors[f].Add(gridPoints[f]);
                }
                else
                {
                    List<GridPoint> floors = gridPoints[f].Where(p => gridPoints[f - 1].Contains(p)).ToList();
                    Floors[f].AddRange(DifferentiateLoops(floors));
                }                

                Ceilings.Add(new List<List<GridPoint>>());
                if (f < NFloor - 1)
                {
                    List<GridPoint> ceiling = gridPoints[f].Where(p => gridPoints[f + 1].Contains(p)).ToList();
                    Ceilings[f].AddRange(DifferentiateLoops(ceiling));
                }

                OverhangFloors.Add(new List<List<GridPoint>>());
                if (f > 0)
                {
                    List<GridPoint> overhangs = gridPoints[f].Where(p => !gridPoints[f - 1].Contains(p)).ToList();
                    OverhangFloors[f].AddRange(DifferentiateLoops(overhangs));
                }

                Roofs.Add(new List<List<GridPoint>>());
                if (f == NFloor-1)
                {
                    Roofs[f].Add(gridPoints[f]);
                }
                else
                {
                    List<GridPoint> roof = gridPoints[f].Where(p => !gridPoints[f + 1].Contains(p)).ToList();
                    Roofs[f].AddRange(DifferentiateLoops(roof));
                }                
            }
        }
        public List<List<GridPoint>> DifferentiateLoops(List<GridPoint> oPoints)
        {
            List<GridPoint> points = oPoints.Select(s=>s).ToList();

            List<List<GridPoint>> loops = new List<List<GridPoint>>();
           
            int cLoop = 0;
            while (points.Count > 0)
            {
                loops.Add(new List<GridPoint>() { points[0] });
                points.RemoveAt(0);

                HashSet<GridPoint> lrud = GetLRUD(loops[cLoop]);
                List<GridPoint> found = points.Where(p => lrud.Contains(p)).ToList();

                while (found.Count() > 0)
                {
                    loops[cLoop].AddRange(found);
                    found.ForEach(f => points.Remove(f));

                    lrud = GetLRUD(loops[cLoop]);
                    found = points.Where(p => lrud.Contains(p)).ToList();
                }
                cLoop++;
            }
            return loops;
        }
        public HashSet<GridPoint> GetLRUD(List<GridPoint> points)
        {
            HashSet<GridPoint> rPoints = new HashSet<GridPoint>();
            foreach(GridPoint p in points)
            {
                p.GetAllNeighbours();
                rPoints.Add(p.Down);
                rPoints.Add(p.Up);
                rPoints.Add(p.Left);
                rPoints.Add(p.Right);
            }
            return rPoints;
        }   
    }
}
