﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ShadingOverhang
    {
        public ShadingOverhang() { }
        public Surface face { get; set; }
        public XYZList listVertice { get; set; }

        public ShadingOverhang(Surface face1)
        {
            face = face1;

            switch (face.Direction)
            {
                case Direction.North:
                    listVertice = createShadingY(face.XYZList, face.ShadingLength).Reverse();
                    break;
                case Direction.South:
                    listVertice = createShadingY(face.XYZList, -face.ShadingLength).Reverse();
                    break;
                case Direction.East:
                    listVertice = createShadingX(face.XYZList, face.ShadingLength).Reverse();
                    break;
                case Direction.West:
                    listVertice = createShadingX(face.XYZList, -face.ShadingLength).Reverse();
                    break;
            }

            XYZList createShadingY(XYZList listVertices, float sl)
            {
                XYZ P1 = listVertices.XYZs.ElementAt(0);
                XYZ P2 = listVertices.XYZs.ElementAt(1);
                XYZ P3 = listVertices.XYZs.ElementAt(2);
                XYZ P4 = listVertices.XYZs.ElementAt(3);

                float shadingLength = sl;

                XYZ pmid = new XYZ((P1.X + P3.X) / 2, P1.Y, (P1.Z + P3.Z) / 2);
                float Y = pmid.Y;
                float Z = P1.Z;

                XYZ Vertice1 = new XYZ(P2.X, Y, Z);
                XYZ Vertice2 = new XYZ(P2.X + shadingLength, Y + shadingLength, Z);
                XYZ Vertice3 = new XYZ(P1.X - shadingLength, Y + shadingLength, Z);
                XYZ Vertice4 = new XYZ(P1.X, Y, Z);

                return new XYZList(Vertice1, Vertice2, Vertice3, Vertice4);
            }

            XYZList createShadingX(XYZList listVertices, float sl)
            {
                XYZ P1 = listVertices.XYZs.ElementAt(0);
                XYZ P2 = listVertices.XYZs.ElementAt(1);
                XYZ P3 = listVertices.XYZs.ElementAt(2);
                XYZ P4 = listVertices.XYZs.ElementAt(3);

                float shadingLength = sl;

                XYZ pmid = new XYZ((P1.X + P3.X) / 2, P1.Y, (P1.Z + P3.Z) / 2);
                float X = pmid.X;
                float Z = P1.Z;

                XYZ Vertice1 = new XYZ(X, P2.Y, Z);
                XYZ Vertice2 = new XYZ(X + shadingLength, P2.Y - shadingLength, Z);
                XYZ Vertice3 = new XYZ(X + shadingLength, P1.Y + shadingLength, Z);
                XYZ Vertice4 = new XYZ(X, P1.Y, Z);

                return new XYZList(  Vertice1, Vertice2, Vertice3, Vertice4 );
            }
        }
        public List<string> shadingInfo()
        {
            List<string> info = new List<string>();
            info.Add("Shading:Zone:Detailed,");
            info.Add("\t" + "Shading_On_" + face.Name + ",\t!- Name");
            info.Add("\t" + face.Name + ",\t!-Base Surface Name)");
            info.Add("\t,\t\t\t\t\t\t!-Transmittance Schedule Name");
            info.AddRange(listVertice.WriteInfo());
            return info;
        }
    }
}
