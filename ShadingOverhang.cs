using System;
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
        public BuildingSurface face { get; set; }
        public XYZList listVertice { get; set; }

        public ShadingOverhang(BuildingSurface face1)
        {
            face = face1;

            switch (face.Direction)
            {
                case Direction.North:
                    listVertice = createShadingY(face.VerticesList, face.ShadingLength).reverse();
                    break;
                case Direction.South:
                    listVertice = createShadingY(face.VerticesList, -face.ShadingLength).reverse();
                    break;
                case Direction.East:
                    listVertice = createShadingX(face.VerticesList, face.ShadingLength).reverse();
                    break;
                case Direction.West:
                    listVertice = createShadingX(face.VerticesList, -face.ShadingLength).reverse();
                    break;
            }

            XYZList createShadingY(XYZList listVertices, double sl)
            {
                XYZ P1 = listVertices.xyzs.ElementAt(0);
                XYZ P2 = listVertices.xyzs.ElementAt(1);
                XYZ P3 = listVertices.xyzs.ElementAt(2);
                XYZ P4 = listVertices.xyzs.ElementAt(3);

                double shadingLength = sl;

                XYZ pmid = new XYZ((P1.X + P3.X) / 2, P1.Y, (P1.Z + P3.Z) / 2);
                double Y = pmid.Y;
                double Z = P1.Z;

                XYZ Vertice1 = new XYZ(P2.X, Y, Z);
                XYZ Vertice2 = new XYZ(P2.X + shadingLength, Y + shadingLength, Z);
                XYZ Vertice3 = new XYZ(P1.X - shadingLength, Y + shadingLength, Z);
                XYZ Vertice4 = new XYZ(P1.X, Y, Z);

                return new XYZList(new List<XYZ>() { Vertice1, Vertice2, Vertice3, Vertice4 });
            }

            XYZList createShadingX(XYZList listVertices, double sl)
            {
                XYZ P1 = listVertices.xyzs.ElementAt(0);
                XYZ P2 = listVertices.xyzs.ElementAt(1);
                XYZ P3 = listVertices.xyzs.ElementAt(2);
                XYZ P4 = listVertices.xyzs.ElementAt(3);

                double shadingLength = sl;

                XYZ pmid = new XYZ((P1.X + P3.X) / 2, P1.Y, (P1.Z + P3.Z) / 2);
                double X = pmid.X;
                double Z = P1.Z;

                XYZ Vertice1 = new XYZ(X, P2.Y, Z);
                XYZ Vertice2 = new XYZ(X + shadingLength, P2.Y - shadingLength, Z);
                XYZ Vertice3 = new XYZ(X + shadingLength, P1.Y + shadingLength, Z);
                XYZ Vertice4 = new XYZ(X, P1.Y, Z);

                return new XYZList(new List<XYZ>() { Vertice1, Vertice2, Vertice3, Vertice4 });
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
