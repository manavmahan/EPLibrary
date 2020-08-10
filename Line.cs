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
        public Line() { }
        public Line(XYZ P1, XYZ P2)
        {
            this.P0 = P1; this.P1 = P2;
        }
        public Line(XYZ[] Points)
        {
            this.P0 = Points[0]; this.P1 = Points[1];
        }
        public XYZ GetIntersection (Line intersectingLine)
        {
            double x1 = P0.X, y1 = P0.Y,
                x2 = P1.X, y2 = P1.Y,
                x3 = intersectingLine.P0.X, y3 = intersectingLine.P0.X,
                x4 = intersectingLine.P1.X, y4 = intersectingLine.P1.X;
            double d = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (d == 0)
                return null;
            double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / d,
                u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / d;

            if (t < 0 && t > 1 && u < 0 && u > 1)
                return null;
            else
                return new XYZ(x1 + t * (x2 - x1), y1 + t*(y2 - y1), P0.Z);
        }
        public XYZ[] GetArray ()=> new XYZ[] { P0, P1 };
        public XYZ GetCorner(int corner)
        {
            return corner == 0 ? P0 : P1;
        }

        public Line ChangeZValue(double baseZ)=> new Line(P0.ChangeZValue(baseZ), P1.ChangeZValue(baseZ));
        public XYZ Direction()
        {
            XYZ d = P0.Subtract(P1);
            double mod = d.AbsoluteValue();
            return d.Multiply(1 / mod);
        }
    }
}
