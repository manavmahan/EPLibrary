using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class WindowConstruction
    {
        public string name { get; set; }
        public List<WindowMaterial> layers { get; set; }
        public double uValue { get; set; }
        public double gValue { get; set; }
        public WindowConstruction() { }
        public WindowConstruction(string n, List<WindowMaterial> l)
        {
            name = n; layers = l;
        }
        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("Construction,");
            info.Add(name + ",   !- Name");
            foreach (WindowMaterial l in layers)
            {
                if (l != layers.Last())
                { info.Add(l.name + ",     !- Outside Layer"); }
                else
                { info.Add(l.name + ";     !- Outside Layer"); }
            }
            return info;
        }

    }
}
