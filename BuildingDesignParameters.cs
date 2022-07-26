using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class BuildingDesignParameters
    {
        public string Name;
        public BuildingGeometry Geometry;
        public BuildingConstruction Construction;
        public BuildingWWR WWR;
        public BuildingService Service;

        public List<ZoneConditions> ZConditions = new List<ZoneConditions>();
        public BuildingDesignParameters() { }
        public string Header(string sep) =>
            string.Join(sep,
                Geometry.Header(sep),
                Construction.Header(sep),
                WWR.Header(sep),
                Service.Header(sep),
                string.Join(sep, ZConditions.Select(o => o.Header(sep))));

        public string ToString(string sep) =>
            string.Join(sep,
                Geometry.ToString(sep),
                Construction.ToString(sep),
                WWR.ToString(sep),
                Service.ToString(sep),
                string.Join(sep, ZConditions.Select(o => o.ToString(sep))));

        private Random _random;
        public Random Random {
            get
            {
                if (_random == null)
                {
                    int seed = (int)Math.Ceiling(typeof(BuildingGeometry).GetFields().
                            Where(f => f.FieldType == typeof(float)).Select(f => 1000*(float)f.GetValue(this.Geometry)).Sum());
                    _random = new Random(seed);
                }
                return _random;
            }
        }
    }
}
