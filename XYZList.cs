using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDFObjects
{
    [Serializable]
    public class XYZList : IEquatable<XYZList>
    {
        public List<XYZ> xyzs;
        public XYZList() { }
        public bool Equals(XYZList loop)
        {
            RemoveCollinearPoints();
            loop.RemoveCollinearPoints();
            if (xyzs.Count != loop.xyzs.Count)
                return false;
            else
            {
                int overLaps = 0;
                foreach (XYZ p in loop.xyzs)
                {
                    if (xyzs.Any(p1=>p1==p))
                        overLaps++;
                }
                return overLaps == xyzs.Count;
            }
        }
        public override int GetHashCode()
        {
            return xyzs.Select(p=>p.GetHashCode()).Sum();
        }
        public void OrientLoop(int n)
        {
            List<XYZ> xYZs = new List<XYZ>();         
            for (int i=n; i<n+xyzs.Count;i++)
            {
                try
                {
                    xYZs.Add(xyzs[n]);
                }
                catch
                {
                    xYZs.Add(xyzs[n-xyzs.Count]);
                }
            }
            xyzs = xYZs;
        }
        public void RemoveCollinearPoints()
        {
            List<Line> Edges = Utility.GetExternalEdges(xyzs);
            List<Line> Loop = new List<Line> { Edges[0] };
            Edges.RemoveAt(0);
            while (Edges.Count() > 0)
            {
                Line lastLine = Loop.Last();
                Line currentLine = Edges[0];
                if (lastLine.Direction() == currentLine.Direction())
                {
                    lastLine.P1 = currentLine.P1;
                    Loop[Loop.Count - 1] = lastLine;
                }
                else
                {
                    Loop.Add(currentLine);
                }
                Edges.RemoveAt(0);
            }
            if (Loop.Last().Direction() == Loop.First().Direction())
            {
                Loop[0].P0 = Loop.Last().P0;
                Loop.RemoveAt(Loop.Count - 1);
            }
            xyzs = Loop.Select(l => l.P0).ToList();
        }
        public XYZList OffsetHeight(float height)
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
        public XYZList Reverse()
        {
            XYZList newList = Utility.DeepClone(this);
            newList.xyzs.Reverse();
            return newList;
        }
        public float CalculateArea()
        {
            float Area = 0;
            for (int i = 0; i < xyzs.Count(); i++)
            {
                XYZ point = xyzs[i], nextPoint;
                try { nextPoint = xyzs[i + 1]; } catch { nextPoint = xyzs.First(); }
                Area += (point.X * nextPoint.Y) - (point.Y * nextPoint.X);
            }
            Area = Math.Abs(Area / 2);
            return (float) Math.Round(Area,2);
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("\t" + ",\t\t\t\t\t\t!- Number of Vertices");
            xyzs.ForEach(xyz => info.Add("\t" + string.Join(",", xyz.X, xyz.Y, xyz.Z) + ", !- X Y Z of Point"));
            return info.ReplaceLastComma();
        }
        public string ToCSVString()
        {
            return string.Join(",", xyzs.Select(xyz => xyz.ToString()));
        }
        public string To2DPointString()
        {
            return string.Join(",", xyzs.Select(p=>p.To2DPointString()));
        }
        public List<Surface> CreateZoneWallExternal(Zone zone, float height)
        {
            List<Surface> walls = new List<Surface>();
            foreach (XYZ v1 in xyzs)
            {
                XYZ v2;
                if (!(v1 == xyzs.Last()))
                { v2 = xyzs.ElementAt((xyzs.IndexOf(v1) + 1)); }
                else { v2 = xyzs.First(); }

                XYZ v3 = v2.OffsetHeight(height);
                XYZ v4 = v1.OffsetHeight(height);

                XYZList vList = new XYZList(new List<XYZ>() { v4, v3, v2, v1 });
                float area = v1.DistanceTo(v2) * height;
                Surface wall = new Surface(zone, vList, area, SurfaceType.Wall);
                walls.Add(wall);
            }
            return walls;
        }
        public void CreateZoneWallExternal(Zone zone, float height, List<string> exposures, List<string> constructions)
        {
            for (int i = 0; i < xyzs.Count; i++)
            {
                XYZ v1 = xyzs[i], v2;
                try
                { v2 = xyzs.ElementAt(i + 1); }
                catch { v2 = xyzs.First(); }

                XYZ v3 = v2.OffsetHeight(height), v4 = v1.OffsetHeight(height);

                XYZList vList = new XYZList(new List<XYZ>() { v4, v3, v2, v1 });
                float area = v1.DistanceTo(v2) * height;
                Surface wall = new Surface(zone, vList, area, SurfaceType.Wall);
                
                if (exposures[i] == "Adiabatic")
                {
                    wall.OutsideCondition = "Adiabatic";
                    wall.SunExposed = "NoSun"; wall.WindExposed = "NoWind";
                }
                if (constructions[i] == "")
                    wall.ConstructionName = "ExWall";
                else
                    wall.ConstructionName = constructions[i];


            }
        }
        public List<Surface> CreateZoneWallExternal(Zone z, float height, float basementDepth)
        {
            List<Surface> walls = new List<Surface>();
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

                XYZList vList1 = new XYZList(new List<XYZ>() { v4, v3, v2, v1 });
                float area = v1.DistanceTo(v2) * basementDepth;

                Surface wall1 = new Surface(z, vList1, area, SurfaceType.Wall);
                wall1.Fenestrations = new List<Fenestration>();
                wall1.OutsideCondition = "Ground";
                wall1.OutsideObject = "";
                wall1.SunExposed = "NoSun";
                wall1.WindExposed = "NoWind";

                area = v5.DistanceTo(v6) * (height-basementDepth);
                XYZList vList2 = new XYZList(new List<XYZ>() { v6, v5, v3, v4 });
                Surface wall2 = new Surface(z, vList2,area, SurfaceType.Wall);
                walls.Add(wall1);
                walls.Add(wall2);
            }

            return walls;
        }
        public void Transform(float angle)
        {
            List<XYZ> newXYZ = new List<XYZ>();
            xyzs.ForEach(v => newXYZ.Add(v.Transform(angle)));
            xyzs = newXYZ;
        }
        public void Transform(XYZ origin)
        {
            List<XYZ> newXYZ = new List<XYZ>();
            xyzs.ForEach(v => newXYZ.Add(v.Subtract(origin)));
            xyzs = newXYZ;
        }
        public float GetWallOrientation(out Direction Direction)
        {
            XYZ v1 = xyzs[0]; XYZ v2 = xyzs[1]; XYZ v3 = xyzs[2];
            XYZ nVector1 = v2.Subtract(v1).CrossProduct(v3.Subtract(v1));
            
            float Orientation = nVector1.AngleOnPlaneTo(new XYZ(0, 1, 0), new XYZ(0, 0, 1));
            Direction = Direction.North;
            if (Orientation < 45 || Orientation >= 315)
            {
                Direction = Direction.North;
            }
            if (Orientation >= 45 && Orientation < 135)
            {
                Direction = Direction.East;
            }
            if (Orientation >= 135 && Orientation < 225)
            {
                Direction = Direction.South;
            }
            if (Orientation >= 225 && Orientation < 315)
            {
                Direction = Direction.West;
            }
            return Orientation;
        }
        public XYZList ChangeZValue(float newZ)
        {
            List<XYZ> newVertices = new List<XYZ>();
            xyzs.ForEach(p => newVertices.Add(new XYZ(p.X, p.Y, newZ)));
            return new XYZList(newVertices);
        }
    }
}
