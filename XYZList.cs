using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IDFObjects
{
    [Serializable]
    public class XYZList
    {
        public List<XYZ> XYZs { get; set; }

        private List<Line> _loop;
        public List<Line> Loop
        {
            get
            {
                if (_loop == null)
                {
                    _loop = new List<Line>();
                    if (XYZs == null)
                        return _loop;

                    if (XYZs.Count < 2)
                        return _loop;

                    for (int i = 0; i < XYZs.Count; i++)
                    {
                        _loop.Add(new Line(XYZs[i], XYZs[(i + 1) % XYZs.Count]));
                    }
                }
                return _loop;
            }
        }      
        public XYZList() { }
        public XYZList(params XYZ[] args)
        {
            var ps = args.Distinct().ToList();
            foreach (var p in ps)
                Add(p);
        }
        public XYZList(string pointStr)
        {
            var points = pointStr.Split(';');
            foreach (var p in points)
            {
                var po = XYZ.Create(p);
                if (po != null)
                    Add(po);
            }
        }
        public XYZList(IEnumerable<XYZ> points)
        {
            foreach (var p in points)
                Add(p);
        }

        public bool Add(XYZ p)
        {
            if (XYZs == null)
                XYZs = new List<XYZ>();

            if (XYZs.Any(p1 => p1.Equals(p)))
                return false;

            var lastLine = Loop.LastOrDefault();
            if (lastLine != null)
            {
                var newLine = new Line(XYZs.Last(), p);
                if (newLine.Direction.Equals(lastLine.Direction))
                    XYZs.RemoveAt(XYZs.Count - 1);
            }

            XYZs.Add(p);
            _loop = null;
            return true;
        }

        public void AddRange(IEnumerable<XYZ> ps)
        {
            foreach (var p in ps)
                this.Add(p);
        }

        //public bool IsPointInside(XYZ xYZ) => this.IsPointInside(xYZ);
        public bool IsPointInside(XYZ point)
        {
            bool ans = false;
            if (XYZs == null)
                return false;

            int n = XYZs.Count;
            if (n < 2)
                return false;
            for (int i = 0; i < n; i++)
            {
                XYZ p1 = XYZs[i];
                XYZ p2 = XYZs[(i + 1) % n];

                if (p1.Equals(point))
                    return true;

                if (p2.Equals(point))
                    return true;
                
                if (new Line (p1, p2).IsOnLine(point))
                    return true;

                float x0 = p1.X, y0 = p1.Y;
                float x1 = p2.X, y1 = p2.Y;

                //min(y0, y1) < pt[1] <= max(y0, y1)
                if (!(Math.Min(y0, y1) < point.Y && point.Y <= Math.Max(y0, y1)))
                    continue;

                if (point.X < Math.Min(x0, x1))
                    continue;

                // cur_x = x0 if x0 == x1 else x0 + (pt[1] - y0) * (x1 - x0) / (y1 - y0)
                var cur_x = x0 == x1 ? x0 : x0 + (point.Y - y0) * (x1 - x0) / (y1 - y0);
                ans ^= point.X > cur_x;
            }
            return ans;


            //return intersections % 2 != 0;
        }
        public XYZList OffsetHeight(float height)
        {
            return new XYZList(XYZs.Select(p=>p.OffsetHeight(height)));
        }
        
        public XYZList Reverse(bool inPlace = false)
        {
            if (XYZs == null)
                return null;

            if (inPlace)
            {
                XYZs.Reverse();
                return this;
            }

            XYZList newList = new XYZList();
            newList.XYZs = new List<XYZ>();
            for (int n = XYZs.Count - 1; n > -1; n--)
                newList.XYZs.Add(XYZs[n]);

            return newList;            
        }

        public float CalculateArea()
        {
            float area = 0;
            for (int i = 0; i < XYZs.Count; i++)
            {
                XYZ point = XYZs[i], nextPoint = XYZs[(i+1) % XYZs.Count];
                area += (point.X * nextPoint.Y) - (point.Y * nextPoint.X);
            }
            area = Math.Abs(area / 2);
            return (float) Math.Round(area, 5);
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("\t" + ",\t\t\t\t\t\t!- Number of Vertices");
            XYZs.ForEach(xyz => info.Add("\t" + string.Join(",", xyz.X, xyz.Y, xyz.Z) + ", !- X Y Z of Point"));
            return info.ReplaceLastComma();
        }
        public override string ToString()
        {
            return string.Join(";", XYZs.Select(xyz => xyz.ToString()));
        }
        public string ToString(bool twoDimensions)
        {
            if (twoDimensions)
                return string.Join(";", XYZs.Select(p => p.ToString(true)));
            
            return ToString();
        }
        public List<Surface> CreateZoneWallExternal(Zone zone, float height)
        {
            List<Surface> walls = new List<Surface>();
            foreach (XYZ v1 in XYZs)
            {
                XYZ v2;
                if (!(v1 == XYZs.Last()))
                { v2 = XYZs.ElementAt((XYZs.IndexOf(v1) + 1)); }
                else { v2 = XYZs.First(); }

                XYZ v3 = v2.OffsetHeight(height);
                XYZ v4 = v1.OffsetHeight(height);

                XYZList vList = new XYZList( v4, v3, v2, v1 );
                float area = v1.DistanceTo(v2) * height;
                Surface wall = new Surface(zone, vList, SurfaceType.Wall, area);
                walls.Add(wall);
            }
            return walls;
        }
        public void CreateZoneWallExternal(Zone zone, float height, List<string> exposures, List<string> constructions)
        {
            for (int i = 0; i < XYZs.Count; i++)
            {
                XYZ v1 = XYZs[i], v2;
                try
                { v2 = XYZs.ElementAt(i + 1); }
                catch { v2 = XYZs.First(); }

                XYZ v3 = v2.OffsetHeight(height), v4 = v1.OffsetHeight(height);

                XYZList vList = new XYZList( v4, v3, v2, v1 );
                float area = v1.DistanceTo(v2) * height;
                Surface wall = new Surface(zone, vList, SurfaceType.Wall, area);
                
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
            foreach (XYZ v1 in XYZs)
            {
                XYZ v2 = new XYZ(0, 0, 0);
                if (!(v1 == XYZs.Last()))
                { v2 = XYZs.ElementAt((XYZs.IndexOf(v1) + 1)); }
                else { v2 = XYZs.First(); }

                XYZ v3 = v2.OffsetHeight(basementDepth);
                XYZ v4 = v1.OffsetHeight(basementDepth);

                XYZ v5 = v2.OffsetHeight(height);
                XYZ v6 = v1.OffsetHeight(height);

                XYZList vList1 = new XYZList( v4, v3, v2, v1 );
                float area = v1.DistanceTo(v2) * basementDepth;

                Surface wall1 = new Surface(z, vList1, SurfaceType.Wall, area);
                wall1.Fenestrations = new List<Fenestration>();
                wall1.OutsideCondition = "Ground";
                wall1.OutsideObject = "";
                wall1.SunExposed = "NoSun";
                wall1.WindExposed = "NoWind";

                area = v5.DistanceTo(v6) * (height-basementDepth);
                XYZList vList2 = new XYZList( v6, v5, v3, v4 );
                Surface wall2 = new Surface(z, vList2, SurfaceType.Wall, area);
                walls.Add(wall1);
                walls.Add(wall2);
            }

            return walls;
        }
        public XYZList Transform(float angle)
        {
            if (XYZs == null)
                return new XYZList();
            return new XYZList( XYZs.Select(v => v.Transform(angle)) );
        }
        public XYZList Transform(XYZ origin)
        {
            if (XYZs == null)
                return new XYZList();
            return new XYZList(XYZs.Select(v => v.Subtract(origin)));
        }
        public float GetWallOrientation(out Direction Direction)
        {
            XYZ v1 = XYZs[0]; XYZ v2 = XYZs[1]; XYZ v3 = XYZs[2];
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
        public XYZList ChangeZValue(float newZ, bool inPlace = false)
        {
            if (XYZs == null)
                return new XYZList();

            if (inPlace)
            {
                XYZs.ForEach(p => p.ChangeZValue(newZ, true));
                return this;
            }
            return new XYZList(XYZs.Select(p => p.ChangeZValue(newZ)));
        }
    }
}
