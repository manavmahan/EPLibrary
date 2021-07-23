using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Material
    {
        public string name { get; set; }
        public string roughness { get; set; }
        public float thickness, conductivity, density, sHC, tAbsorptance, sAbsorptance, vAbsorptance;
        public Material() { }
        public Material(string name, string rough, float th, float conduct, float dense, float sH, float tAbsorp, float sAbsorp, float vAbsorp)
        {
            this.name = name;
            roughness = rough;
            thickness = th;
            conductivity = conduct;
            density = dense;
            sHC = sH;
            tAbsorptance = tAbsorp;
            sAbsorptance = sAbsorp;
            vAbsorptance = vAbsorp;
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("Material,");
            info.Add(name + ",          !-Name");
            info.Add(roughness + ",            !-Roughness");
            info.Add(thickness + ",                    !-Thickness { m}");
            info.Add(conductivity + ",               !-Conductivity { W / m - K}");
            info.Add(density + ",                !-Density { kg / m3}");
            info.Add(sHC + ",                 !-Specific Heat { J / kg - K}");
            info.Add(tAbsorptance + ",                    !-Thermal Absorptance");
            info.Add(sAbsorptance + ",                    !-Solar Absorptance");
            info.Add(vAbsorptance + "; !-Visible Absorptance");
            return info;
        }
    }
}
