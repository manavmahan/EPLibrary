using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
