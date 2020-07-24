﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDFObjects
{
    [Serializable]
    public class XYZList
    {
        public List<XYZ> xyzs;
        public XYZList() { }
        
        public void RemoveCollinearPoints()
        {
            List<IDFObjects.XYZ[]> Edges = Utility.GetExternalEdges(xyzs);
            List<IDFObjects.XYZ[]> Loop = new List<IDFObjects.XYZ[]> { Edges[0] };
            Edges.RemoveAt(0);
            while (Edges.Count() > 0)
            {
                IDFObjects.XYZ[] lastLine = Loop.Last();
                IDFObjects.XYZ[] currentLine = Edges[0];
                if (Utility.GetDirection(lastLine).Equals(Utility.GetDirection(currentLine)))
                {
                    lastLine[1] = currentLine[1];
                    Loop[Loop.Count - 1] = lastLine;
                }
                else
                {
                    Loop.Add(currentLine);
                }
                Edges.RemoveAt(0);
            }
            if (Utility.GetDirection(Loop.Last()).Equals(Utility.GetDirection(Loop.First())))
            {
                Loop[0][0] = Loop.Last()[0];
                Loop.RemoveAt(Loop.Count - 1);
            }
            xyzs = Loop.Select(l => l[0]).ToList();
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
        public XYZList Reverse()
        {
            XYZList newList = Utility.DeepClone(this);
            newList.xyzs.Reverse();
            return newList;
        }
        public double CalculateArea()
        {
            double Area = 0;
            for (int i = 0; i < xyzs.Count(); i++)
            {
                XYZ point = xyzs[i], nextPoint;
                try { nextPoint = xyzs[i + 1]; } catch { nextPoint = xyzs.First(); }
                Area += (point.X * nextPoint.Y) - (point.Y * nextPoint.X);
            }
            Area = Math.Abs(Area / 2);
            return Math.Round(Area,3);
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("\t" + ",\t\t\t\t\t\t!- Number of Vertices");
            xyzs.ForEach(xyz => info.Add(string.Join(",", xyz.X, xyz.Y, xyz.Z) + ", !- X Y Z of Point"));
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
        public List<Surface> CreateZoneWallExternal(Zone zone, double height)
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
                double area = v1.DistanceTo(v2) * height;
                Surface wall = new Surface(zone, vList, area, SurfaceType.Wall);
                walls.Add(wall);
            }
            return walls;
        }
        public void CreateZoneWallExternal(Zone zone, double height, List<string> exposures, List<string> constructions)
        {
            for (int i = 0; i < xyzs.Count; i++)
            {
                XYZ v1 = xyzs[i], v2;
                try
                { v2 = xyzs.ElementAt(i + 1); }
                catch { v2 = xyzs.First(); }

                XYZ v3 = v2.OffsetHeight(height), v4 = v1.OffsetHeight(height);

                XYZList vList = new XYZList(new List<XYZ>() { v4, v3, v2, v1 });
                double area = v1.DistanceTo(v2) * height;
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
        public List<Surface> CreateZoneWallExternal(Zone z, double height, double basementDepth)
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
                double area = v1.DistanceTo(v2) * basementDepth;

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
        public void Transform(double angle)
        {
            List<XYZ> newXYZ = new List<XYZ>();
            xyzs.ForEach(v => newXYZ.Add(v.Transform(angle)));
            xyzs = newXYZ;
        }
        public double GetWallOrientation(out Direction Direction)
        {
            XYZ v1 = xyzs[0]; XYZ v2 = xyzs[1]; XYZ v3 = xyzs[2];
            XYZ nVector1 = v2.Subtract(v1).CrossProduct(v3.Subtract(v1));
            
            double Orientation = nVector1.AngleOnPlaneTo(new XYZ(0, 1, 0), new XYZ(0, 0, 1));
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
        public XYZList ChangeZValue(double newZ)
        {
            List<XYZ> newVertices = new List<XYZ>();
            xyzs.ForEach(p => newVertices.Add(new XYZ(p.X, p.Y, newZ)));
            return new XYZList(newVertices);
        }
    }
}