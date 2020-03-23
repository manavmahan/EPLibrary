using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class WindowMaterial
    {
        public string name { get; set; }
        public double uValue { get; set; }
        public double gValue { get; set; }
        public double vTransmittance { get; set; }
        public WindowMaterial()
        {
        }
        public WindowMaterial(string n, double u, double g, double transmittance)
        {
            name = n; uValue = u; gValue = g; vTransmittance = transmittance;
        }
        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("WindowMaterial:SimpleGlazingSystem,");
            info.Add(name + ",  !- Name");
            info.Add(uValue + ",                 !- U-Factor {W/m2-K}");
            info.Add(gValue + ",                 !- Solar Heat Gain Coefficient");
            info.Add(vTransmittance + ";                     !- Visible Transmittance");
            return info;
        }
    }
}
