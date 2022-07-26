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
        public float X, Y, Z;
        public XYZ() { X = 0; Y = 0; Z = 0; }
        public XYZ(float x, float y, float z) { 
            X = (float) Math.Round(x, 5); Y = (float) Math.Round(y, 5); Z = (float) Math.Round(z, 5); }
        public XYZ(float x, float y) { X = (float) Math.Round(x, 5); Y = (float) Math.Round(y, 5); Z = 0; }
        public XYZ(float[] point) { new XYZ(point[0], point[1], point[2]); }
        public static XYZ Create(string p)
        {
            p.Replace("\r", "");
            p.Replace("\n", "");
            if (!string.IsNullOrWhiteSpace(p))
            {
                XYZ n = new XYZ();
                var s = p.Split(',');
                n.X = (float) Math.Round(float.Parse(s[0]), 5);
                n.Y = (float) Math.Round(float.Parse(s[1]), 5);

                if (s.Length > 2)
                    n.Z = (float) Math.Round(float.Parse(s[2]), 5);
                return n;
            }
            return null;
        }
        public XYZ Subtract(XYZ newXYZ) { return new XYZ(X - newXYZ.X, Y - newXYZ.Y, Z - newXYZ.Z); }
        
        public XYZ Transform(float angle)
        {
            float angleRad = angle * (float) Math.PI/180;
            float x1 = X * (float) Math.Cos(angleRad) - Y * (float) Math.Sin(angleRad);
            float y1 = X * (float) Math.Sin(angleRad) + Y * (float) Math.Cos(angleRad);
            return new XYZ(x1, y1, Z);
        }

        public XYZ OffsetHeight(float height)
        {
            return new XYZ(X, Y, Z + height);
        }

        public override string ToString()
        {
            return $"{X:0.000},{Y:0.000},{Z:.0.000}";
        }

        public float DotProduct(XYZ newXYZ)
        {
            return X * newXYZ.X + Y * newXYZ.Y + Z * newXYZ.Z;
        }

        public XYZ CrossProduct(XYZ newXYZ)
        {
            return new XYZ(Y * newXYZ.Z - Z * newXYZ.Y, Z * newXYZ.X - X * newXYZ.Z, X * newXYZ.Y - Y * newXYZ.X);
        }

        public float AngleOnPlaneTo(XYZ right, XYZ normalPlane)
        {
            float nfloat = DotProduct(right);
            float anglePI = (float) Math.Atan2(CrossProduct(right).DotProduct(normalPlane), nfloat - (right.DotProduct(normalPlane)) * DotProduct(normalPlane));
            if (anglePI < 0) { anglePI = (float)Math.PI * 2 + anglePI; }
            return (float) Math.Round(180 * anglePI / Math.PI);
        }

        public float AngleBetweenVectors(XYZ newXYZ)
        {
            return (float) Math.Round(Math.Acos((X * newXYZ.X + Y * newXYZ.Y + Z * newXYZ.Z) / (AbsoluteValue() * newXYZ.AbsoluteValue())), 4);
        }

        public float DistanceTo(XYZ newXYZ)
        {
            return (float) Math.Round(Math.Sqrt(Math.Pow(X - newXYZ.X, 2) + Math.Pow(Y - newXYZ.Y, 2) + Math.Pow(Z - newXYZ.Z, 2)), 4);
        }
        public float AbsoluteValue()
        {
            return (float) Math.Sqrt(X * X + Y * Y + Z * Z);
        }
        public XYZ MovePoint(float d, XYZ tp1, XYZ tp2)
        {
            float d1 = 2 * d / DistanceTo(tp1);
            float d2 = 2 * d / DistanceTo(tp2);

            XYZ dir1 = tp1.Subtract(this);
            XYZ p1 = new XYZ(X + d1 * dir1.X, Y + d1 * dir1.Y, Z + d1 * dir1.Z);

            XYZ dir2 = tp2.Subtract(this);
            XYZ p2 = new XYZ(X + d2 * dir2.X, Y + d2 * dir2.Y, Z + d2 * dir2.Z);

            return new XYZ((p1.X + p2.X) * .5f, (p1.Y + p2.Y) * .5f, (p1.Z + p2.Z) * .5f);
        }

        public string ToString(bool twoDimensions)
        {
            if (twoDimensions)
                return $"{X:0.000},{Y:0.000}";
            
            return ToString();
        }
        public XYZ ChangeZValue(float z, bool inPlace = false)
        {
            z = (float) Math.Round(z, 5);
            if (!inPlace)
                return new XYZ(X, Y, z);
            Z = z;
            return this;
        }
        public XYZ Add(XYZ xYZ)
        {
            return new XYZ(X + xYZ.X, Y + xYZ.Y, Z + xYZ.Z);
        }
        public XYZ Multiply(float d)
        {
            return new XYZ(d * X, d * Y, d * Z);
        }
        public XYZ MovePoint(XYZ towardsPoint, float distance)
        {
            float distBetPoints = DistanceTo(towardsPoint);
            return Math.Round(distBetPoints, 3) > 0 ? Add(Subtract(towardsPoint).Multiply(distance / distBetPoints)) : this;
        }
        public bool EqualsExceptZ(XYZ point1)
        {
            return ToString(true).Equals(point1.ToString(true));
        }

        public bool Equals(XYZ pt)
        {
            return ToString().Equals(pt.ToString());
        }  

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
