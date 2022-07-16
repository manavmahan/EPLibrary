using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    public class Line
    {
        public XYZ P0, P1;

        private float _distance;
        public float Distance
        {
            get
            {
                if (_distance == 0)
                    _distance = P0.DistanceTo(P1);
                return _distance;
            }
        }
        public Line() { }
        public Line(XYZ P0, XYZ P1)
        {
            this.P0 = P0; this.P1 = P1;
            if (Distance == 0)
                throw new Exception("Cannot create line!");
        }
        public Line(XYZ[] Points)
        {
            this.P0 = Points[0]; this.P1 = Points[1];
        }
        
        /// <summary>
        /// Collinear lines will not be treated as intersecting. Lines toucing will be treated as intersecting.
        /// </summary>
        /// <param name="intersectingLine"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public bool GetIntersection (Line intersectingLine, out XYZ intersection)
        {
            intersection = null;
            if (intersectingLine.Direction.Equals(Direction))
            {
                if (IsOnLine(intersectingLine.P0))
                {
                    intersection = intersectingLine.P0;
                    return true;

                }
                if (IsOnLine(intersectingLine.P1))
                {
                    intersection = intersectingLine.P1;
                    return true;
                }
                return false;
            } 

            if (intersectingLine.P0.Equals(P0) || intersectingLine.P1.Equals(P0))
            {
                intersection = P0;
                return true;
            }

            if (intersectingLine.P0.Equals(P1) || intersectingLine.P1.Equals(P1))
            {
                intersection = P1;
                return true;
            }

            if (IsOnLine(intersectingLine.P0))
            { 
                intersection = intersectingLine.P0;
                return true;
            }

            if (IsOnLine(intersectingLine.P1))
            {
                intersection = intersectingLine.P1;
                return true;
            }

            float x1 = P0.X, y1 = P0.Y;
            float x2 = P1.X, y2 = P1.Y;
            float x3 = intersectingLine.P0.X, y3 = intersectingLine.P0.Y;
            float x4 = intersectingLine.P1.X, y4 = intersectingLine.P1.Y;

            float d = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (d == 0)
                return false;
            float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / d,
                u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / d;

            if (t < 0 && t > 1 && u < 0 && u > 1)
                return false;
            else
            {
                intersection = new XYZ(x1 + t * (x2 - x1), y1 + t * (y2 - y1), P0.Z);
                if (IsOnLine(intersection) && intersectingLine.IsOnLine(intersection))
                    return true;
                intersection = null;
                return false;
            }
        }
        public XYZ[] GetArray ()=> new XYZ[] { P0, P1 };
        public XYZ GetCorner(int corner)
        {
            return corner == 0 ? P0 : P1;
        }

        public Line ChangeZValue(float baseZ)=> new Line(P0.ChangeZValue(baseZ), P1.ChangeZValue(baseZ));
        public XYZ Direction { 
            get 
            {
                XYZ d = P1.Subtract(P0);
                float mod = d.AbsoluteValue();
                return d.Multiply(1 / mod);
            } 
        }
        public Line Reverse()
        {
            return new Line(P1, P0);
        }

        public bool IsOnLine(XYZ point)
        {
            return Math.Round(P0.DistanceTo(point) + P1.DistanceTo(point) - Distance, 5) == 0;
        }
    }
}
