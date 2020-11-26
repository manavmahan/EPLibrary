using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Fenestration
    {
        public Fenestration() { }
        public double Area, SolarRadiation, Orientation;
        public double[] h_SolarRadiation;
        //public BuildingSurface Face;
        public string FaceName, ZoneName;
        public XYZList VerticesList;
        public string ConstructionName, SurfaceType, Name;
        public WindowShadingControl ShadingControl { get; set; }
        public OverhangProjection Overhang { get; set; }
        internal Fenestration(Surface wallFace)
        {
            SurfaceType = "Window";
            ConstructionName = "Glazing";
            Name = SurfaceType + "_On_" + wallFace.Name;
            FaceName = wallFace.Name;
            ZoneName = wallFace.ZoneName;
            Orientation = wallFace.Orientation;
            VerticesList = new XYZList(new List<XYZ>());
        }

        internal List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("FenestrationSurface:Detailed,");
            info.Add(Utility.IDFLineFormatter(Name, "Subsurface Name"));

            info.Add("\t" + SurfaceType + ",\t\t\t\t\t\t!- Surface Type");
            info.Add("\t" + ConstructionName + ",\t\t\t\t\t\t!- Construction Name");

            info.Add("\t" + FaceName + ",\t!-Building Surface Name)");
            info.Add("\t,\t\t\t\t\t\t!-Outside Boundary Condition Object");

            info.Add("\t,\t\t\t\t\t\t!-View Factor to Ground");
            info.Add("\t,\t\t\t\t\t\t!- Frame and Divider Name");
            info.Add("\t,\t\t\t\t\t\t!-Multiplier");

            info.AddRange(VerticesList.WriteInfo());
            return info;
        }
    }
}
