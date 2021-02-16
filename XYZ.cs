using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class XYZ : IEquatable<XYZ>
    {
        public double X = 0, Y = 0, Z = 0;
        public XYZ() { }
        public XYZ(double x, double y, double z) { X = Math.Round(x,5); Y = Math.Round(y,5); Z = Math.Round(z,5); }
        public XYZ(double x, double y) { X = Math.Round(x, 5); Y = Math.Round(y, 5); Z = 0; }
        public XYZ(double[] point) { new XYZ(point[0], point[1], point[2]); }
        public XYZ Subtract(XYZ newXYZ) { return new XYZ(X - newXYZ.X, Y - newXYZ.Y, Z - newXYZ.Z); }
        public XYZ Transform(double angle)
        {
            double angleRad = angle * Math.PI/180;
            double x1 = X * Math.Cos(angleRad) - Y * Math.Sin(angleRad);
            double y1 = X * Math.Sin(angleRad) + Y * Math.Cos(angleRad);
            return new XYZ(x1, y1, Z);
        }
        public bool Equals(XYZ point1)
        {
            return Math.Round(X - point1.X, 1) == 0 && Math.Round(Y - point1.Y, 1) == 0 && Math.Round(Z - point1.Z, 1) == 0;
        }
        public bool EqualsExceptZ(XYZ point1)
        {
            return X == point1.X && Y == point1.Y;
        }
        public bool IsAlmostEqual(XYZ point1)
        {
            return Math.Round(X-point1.X,1) ==0 && Math.Round(Y-point1.Y,1)==0 && Math.Round(Z-point1.Z,1)==0;
        }
        public override int GetHashCode()
        {
            return (X+Y+Z).GetHashCode();
        }

        public XYZ OffsetHeight(double height)
        {
            return new XYZ(X, Y, Z + height);
        }
        public override string ToString()
        {
            return string.Join(",", X, Y, Z);
        }
        public double DotProduct(XYZ newXYZ)
        {
            return X * newXYZ.X + Y * newXYZ.Y + Z * newXYZ.Z;
        }
        public XYZ CrossProduct(XYZ newXYZ)
        {
            return new XYZ(Y * newXYZ.Z - Z * newXYZ.Y, Z * newXYZ.X - X * newXYZ.Z, X * newXYZ.Y - Y * newXYZ.X);
        }
        public double AngleOnPlaneTo(XYZ right, XYZ normalPlane)
        {
            double nDouble = DotProduct(right);
            double anglePI = Math.Atan2(CrossProduct(right).DotProduct(normalPlane), nDouble - (right.DotProduct(normalPlane)) * DotProduct(normalPlane));
            if (anglePI < 0) { anglePI = Math.PI * 2 + anglePI; }
            return Math.Round(180 * anglePI / Math.PI);
        }
        public double AngleBetweenVectors(XYZ newXYZ)
        {
            return (Math.Round(Math.Acos((X * newXYZ.X + Y * newXYZ.Y + Z * newXYZ.Z) / (AbsoluteValue() * newXYZ.AbsoluteValue())), 2));
        }
        public double DistanceTo(XYZ newXYZ)
        {
            return Math.Sqrt(Math.Pow(X - newXYZ.X, 2) + Math.Pow(Y - newXYZ.Y, 2) + Math.Pow(Z - newXYZ.Z, 2));
        }
        public double AbsoluteValue()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }
        public XYZ MovePoint(double d, XYZ tp1, XYZ tp2)
        {
            double d1 = 2 * d / DistanceTo(tp1);
            double d2 = 2 * d / DistanceTo(tp2);

            XYZ dir1 = tp1.Subtract(this);
            XYZ p1 = new XYZ(X + d1 * dir1.X, Y + d1 * dir1.Y, Z + d1 * dir1.Z);

            XYZ dir2 = tp2.Subtract(this);
            XYZ p2 = new XYZ(X + d2 * dir2.X, Y + d2 * dir2.Y, Z + d2 * dir2.Z);

            return new XYZ((p1.X + p2.X) * .5, (p1.Y + p2.Y) * .5, (p1.Z + p2.Z) * .5);
        }

        public string To2DPointString()
        {
            return string.Join(",", X, Y);
        }
        public XYZ ChangeZValue(double z)
        {
            return new XYZ(X, Y, z);
        }
        public XYZ Add(XYZ xYZ)
        {
            return new XYZ(X + xYZ.X, Y + xYZ.Y, Z + xYZ.Z);
        }
        public XYZ Multiply(double d)
        {
            return new XYZ(d * X, d * Y, d * Z);
        }
        public XYZ MovePoint(XYZ towardsPoint, double distance)
        {
            double distBetPoints = DistanceTo(towardsPoint);
            return Math.Round(distBetPoints, 3) > 0 ? Add(Subtract(towardsPoint).Multiply(distance / distBetPoints)) : this;
        }
    }
}
