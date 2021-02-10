using Microsoft.JScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ZoneGeometryInformation
    {
        public string Name;
        public double Height;
        public XYZList FloorPoints;
        public List<XYZList> CeilingPoints;
        public int Level;
        public List<string> WallCreationDataKey = new List<string>();
        public List<Line> WallCreationDataValue = new List<Line>();
        public ZoneGeometryInformation() { }
    }
}
