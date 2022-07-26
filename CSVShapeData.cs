using System.Collections.Generic;
using System.Linq;

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
            if(building.GroundPoints!=null)
                GroundPoints = building.GroundPoints;
            else
                GroundPoints = building.zones.First().Surfaces.First(s => s.SurfaceType == SurfaceType.Floor).XYZList;

            foreach (var zone in building.zones)
            {
                foreach (var floor in zone.Surfaces.Where(s => s.SurfaceType == SurfaceType.Floor))
                { 
                    ZonePoints.Add(floor.Name, floor.XYZList);
                } 
            }

            building.zones.SelectMany(z => z.Surfaces.Where(s => s.SurfaceType == SurfaceType.Wall && s.OutsideCondition == "Outdoors")).ToList().
            ForEach(w=>WallPoints.Add(w.Name, w.XYZList));
            
            BuildingShapeData = new List<string>() { string.Join("|", File, GroundPoints.ToString(true)) };
            ZoneShapeData = new List<string>();
            foreach (var z in ZonePoints)
            {
                ZoneShapeData.Add(string.Join("|", File, z.Key, z.Value.ToString()));
            }
            WallShapeData = new List<string>();
            foreach (KeyValuePair<string, XYZList> w in WallPoints)
            {
                WallShapeData.Add(string.Join("|", File, w.Key, w.Value.ToString()));
            }
        }
    }
}
