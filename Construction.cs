using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDFObjects
{
    [Serializable]
    public class Construction
    {
        public Construction() { }
        public string name { get; set; }
        public List<Material> layers { get; set; }
        public List<WindowMaterial> wLayers { get; set; }
        public double heatCapacity { get; set; }
        public Construction(string n, List<Material> layers)
        {
            name = n; this.layers = layers; wLayers = new List<WindowMaterial>();
            heatCapacity = layers.Select(la => la.thickness * la.sHC * la.density).Sum();
        }
        public void AdjustInsulation(double requiredUValue, Material insulation)
        {
            if (requiredUValue != 0)
            {
                double sum = layers.Where(l=>l.name!=insulation.name).Select(m=> m.thickness / m.conductivity).Sum();
                insulation.thickness = Math.Round(insulation.conductivity * ((1 / requiredUValue) - sum),5);
                if (!(insulation.thickness > 0))
                {
                    MessageBox.Show(string.Format("U-value of {0} for construction {1} requires an insulation thickness of {2}\n" +
                        "Please check value properly.\n" +
                        "To proceed safely, material {3} has been removed from the construction!", requiredUValue, this.name,
                        insulation.thickness, insulation.name));
                    try { layers.Remove(insulation); } catch { } 
                }
            }
        }
        public Construction(string n, List<WindowMaterial> layers)
        {
            name = n; wLayers = layers; this.layers = new List<Material>();
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("Construction,");
            info.Add(name + ",   !- Name");

            if (wLayers.Count == 0)
            {
                for (int i = 0; i < layers.Count; i++)
                {
                    Material l = layers[i];
                    if (i != layers.Count - 1)
                    { info.Add(l.name + ",     !- Outside Layer"); }
                    else
                    { info.Add(l.name + ";     !- Outside Layer"); }
                }
            }
            else
            {
                for (int i = 0; i < wLayers.Count; i++)
                {
                    WindowMaterial l = wLayers[i];
                    if (i != wLayers.Count - 1)
                    { info.Add(l.name + ",     !- Outside Layer"); }
                    else
                    { info.Add(l.name + ";     !- Outside Layer"); }
                }
            }
            return info;
        }
    }
}
