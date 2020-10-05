using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDFObjects
{
    public class ShapeData
    {
        public string File;
        public XYZList GroundPoints;
        public Dictionary<string, XYZList> ZonePoints = new Dictionary<string, XYZList>();
        public Dictionary<string, XYZList> WallPoints = new Dictionary<string, XYZList>();
        public ShapeData(Building building)
        {
            File = building.name;
            GroundPoints = building.GroundPoints;
            building.zones.ForEach(z => ZonePoints.Add(z.Name,
                new XYZList(z.Surfaces.Where(s => s.surfaceType == SurfaceType.Floor).SelectMany(s => s.VerticesList.xyzs).ToList())));
            building.zones.SelectMany(z => z.Surfaces.Where(s => s.surfaceType == SurfaceType.Wall && s.OutsideCondition == "Outdoors")).ToList().
            ForEach(w=>WallPoints.Add(w.Name, w.VerticesList));
        }
        public List<string> BuildingString()
        {
            return new List<string>() { string.Join(",", File, GroundPoints.To2DPointString()) };
        }
        public List<string> ZoneString()
        {
            List<string> v = new List<string>();
            foreach (KeyValuePair<string, XYZList> z in ZonePoints)
            {
                v.Add(string.Join(",", File, z.Key, z.Value.To2DPointString()));
            }
            return v;
        }
        public List<string> WallString() 
        {
            List<string> v = new List<string>(); 
            foreach (KeyValuePair<string, XYZList> w in WallPoints)
            {
                v.Add(string.Join(",", File, w.Key, w.Value.ToCSVString())); 
            } 
            return v;
        }
    }
}
