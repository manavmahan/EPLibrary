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
        public float Height;
        public IEnumerable<XYZList> FloorPoints = new List<XYZList>();
        public IEnumerable<XYZList> OverhangPoints = new List<XYZList>();
        public IEnumerable<XYZList> CeilingPoints = new List<XYZList>();
        public IEnumerable<XYZList> RoofPoints = new List<XYZList>();
        public int Level;
        public IEnumerable<KeyValuePair<string, Line>> WallGeometryData = new List<KeyValuePair<string, Line>> ();
        public ZoneGeometryInformation() { }

        public void AddWallGeometryData(string key, Line value)
        {
            WallGeometryData = WallGeometryData.Append(new KeyValuePair<string, Line> (key, value));
        }
    }
}
