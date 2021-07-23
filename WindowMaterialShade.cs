using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class WindowMaterialShade
    {
        public string name = "ROLL SHADE";
        public float sTransmittance = 0.3f;
        public float sReflectance = 0.5f;
        public float vTransmittance = 0.3f;
        public float vReflectance = 0.5f;
        public float infraEmissivity = 0.9f;
        public float infraTransmittance = 0.05f;
        public float thickness = 0.003f;
        public float conductivity = 0.1f;
        public float disShades = 0.05f;
        public float tMultiplier = 0;
        public float bMultiplier = 0.5f;
        public float lMultiplier = 0.5f;
        public float rMultiplier = 0;
        public string airPermeability = "";

        public WindowMaterialShade() { }
        public List<string> writeInfo()
        {
            return new List<string>()
            {
                "WindowMaterial:Shade,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(sTransmittance, "Solar Transmittance { dimensionless }"),
                Utility.IDFLineFormatter(sReflectance, "Solar Reflectance { dimensionless }"),
                Utility.IDFLineFormatter(vTransmittance, "Visible Transmittance { dimensionless }"),
                Utility.IDFLineFormatter(vReflectance, "Visible Reflectance { dimensionless }"),
                Utility.IDFLineFormatter(infraEmissivity, "Infrared Hemispherical Emissivity { dimensionless }"),
                Utility.IDFLineFormatter(infraTransmittance, "Infrared Transmittance { dimensionless }"),
                Utility.IDFLineFormatter(thickness, "Thickness { m }"),
                Utility.IDFLineFormatter(conductivity, "Conductivity { W / m - K }"),
                Utility.IDFLineFormatter(disShades, "Shade to Glass Distance { m }"),
                Utility.IDFLineFormatter(tMultiplier, "Top Opening Multiplier"),
                Utility.IDFLineFormatter(bMultiplier, "Bottom Opening Multiplier"),
                Utility.IDFLineFormatter(lMultiplier, "Left - Side Opening Multiplier"),
                Utility.IDFLineFormatter(rMultiplier, "Right - Side Opening Multiplier"),
                Utility.IDFLastLineFormatter(airPermeability, "Airflow Permeability { dimensionless}")
            };
        }
    }
}
