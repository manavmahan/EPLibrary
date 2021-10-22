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
        public Dictionary<string, List<XYZList>> ZonePoints = new Dictionary<string, List<XYZList>>();
        public Dictionary<string, XYZList> WallPoints = new Dictionary<string, XYZList>();

        public string BuildingShape="", ZoneShape="", WallShape="";
        public List<string> BuildingShapeData, ZoneShapeData, WallShapeData;
        public CSVShapeData(Building building)
        {
            string File = building.name;
            if(building.GroundPoints!=null)
                GroundPoints = building.GroundPoints;
            else
                GroundPoints = building.zones.First().Surfaces.First(s => s.SurfaceType == SurfaceType.Floor).XYZList;
            
            building.zones.ForEach(z => ZonePoints.Add(z.Name,
                z.Surfaces.Where(s => s.SurfaceType == SurfaceType.Floor).Select(s => 
                s.XYZList).ToList()));
            building.zones.SelectMany(z => z.Surfaces.Where(s => s.SurfaceType == SurfaceType.Wall && s.OutsideCondition == "Outdoors")).ToList().
            ForEach(w=>WallPoints.Add(w.Name, w.XYZList));
            
            BuildingShapeData = new List<string>() { string.Join(",", File, GroundPoints.To2DPointString()) };
            ZoneShapeData = new List<string>();
            foreach (KeyValuePair<string, List<XYZList>> z in ZonePoints)
            {
                ZoneShapeData.Add(string.Join(",", File, z.Key, string.Join(";", z.Value.Select(
                    v=>v.ToCSVString()))));
            }
            WallShapeData = new List<string>();
            foreach (KeyValuePair<string, XYZList> w in WallPoints)
            {
                WallShapeData.Add(string.Join(",", File, w.Key, w.Value.ToCSVString()));
            }
        }
    }
}
