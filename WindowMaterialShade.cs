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
        public double sTransmittance = 0.3;
        public double sReflectance = 0.5;
        public double vTransmittance = 0.3;
        public double vReflectance = 0.5;
        public double infraEmissivity = 0.9;
        public double infraTransmittance = 0.05;
        public double thickness = 0.003;
        public double conductivity = 0.1;
        public double disShades = 0.05;
        public double tMultiplier = 0;
        public double bMultiplier = 0.5;
        public double lMultiplier = 0.5;
        public double rMultiplier = 0;
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
