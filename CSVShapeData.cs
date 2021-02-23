using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDFObjects
{
    public class CSVShapeData
    {
        public XYZList GroundPoints;
        public Dictionary<string, XYZList> ZonePoints = new Dictionary<string, XYZList>();
        public Dictionary<string, XYZList> WallPoints = new Dictionary<string, XYZList>();

        public string BuildingShape="", ZoneShape="", WallShape="";
        public List<string> BuildingShapeData, ZoneShapeData, WallShapeData;
        public CSVShapeData(Building building)
        {
            string File = building.name;
            GroundPoints = building.GroundPoints;
            building.zones.ForEach(z => ZonePoints.Add(z.Name,
                new XYZList(z.Surfaces.Where(s => s.surfaceType == SurfaceType.Floor).SelectMany(s => s.VerticesList.xyzs).ToList())));
            building.zones.SelectMany(z => z.Surfaces.Where(s => s.surfaceType == SurfaceType.Wall && s.OutsideCondition == "Outdoors")).ToList().
            ForEach(w=>WallPoints.Add(w.Name, w.VerticesList));
            
            BuildingShapeData = new List<string>() { string.Join(",", File, GroundPoints.To2DPointString()) };
            ZoneShapeData = new List<string>();
            foreach (KeyValuePair<string, XYZList> z in ZonePoints)
            {
                ZoneShapeData.Add(string.Join(",", File, z.Key, z.Value.To2DPointString()));
            }
            WallShapeData = new List<string>();
            foreach (KeyValuePair<string, XYZList> w in WallPoints)
            {
                WallShapeData.Add(string.Join(",", File, w.Key, w.Value.ToCSVString()));
            }
        }
    }
}
