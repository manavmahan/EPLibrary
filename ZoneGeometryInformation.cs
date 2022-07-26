using System;
using System.Collections.Generic;

namespace IDFObjects
{
    [Serializable]
    public class ZoneGeometryInformation
    {
        public string Name;
        public float Height;
        public IList<XYZList> FloorPoints = new List<XYZList>();
        public IList<XYZList> OverhangPoints = new List<XYZList>();
        public IList<XYZList> CeilingPoints = new List<XYZList>();
        public IList<XYZList> RoofPoints = new List<XYZList>();
        public int Level;
        public IList<KeyValuePair<string, Line>> WallGeometryData = new List<KeyValuePair<string, Line>> ();
        public ZoneGeometryInformation() { }

        public void AddWallGeometryData(string key, Line value)
        {
            WallGeometryData.Add(new KeyValuePair<string, Line> (key, value));
        }
    }
}
