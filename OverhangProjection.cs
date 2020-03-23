using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class OverhangProjection
    {
        public Fenestration window;
        public double depthf;

        public OverhangProjection() { }
        public OverhangProjection(Fenestration win, double df)
        {
            window = win;
            depthf = df;

        }

        public List<string> OverhangInfo()
        {

            List<string> info = new List<string>();

            info.Add("Shading:Overhang:Projection,");
            info.Add("\t" + "Shading_On_" + window.SurfaceType + "_On_" + window.Face.Name + ",\t!- Name");
            info.Add("\t" + window.SurfaceType + "_On_" + window.Face.Name + ",\t!-Window or Door Name");
            info.Add("\t0,\t\t\t\t\t\t!-Height above Window or Door {m}");
            info.Add("\t90,\t\t\t\t\t\t!-Tilt Angle from Window/Door {deg}");
            info.Add("\t.2,\t\t\t\t\t\t!-Left extension from Window/Door Width {m}");
            info.Add("\t.2,\t\t\t\t\t\t!-Right extension from Window/Door Width {m}");
            info.Add("\t" + depthf + ";\t\t\t\t\t\t!-Depth as Fraction of Window/Door Height {m}");


            return info;
        }

    }
}
