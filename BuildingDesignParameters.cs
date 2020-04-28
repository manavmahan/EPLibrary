using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    public class BuildingDesignParameters
    {
        public BuildingGeometry Geometry;
        public BuildingConstruction Construction;
        public BuildingWWR WWR;          
        public BuildingService Service;

        public List<BuildingZoneOperation> Operations = new List<BuildingZoneOperation>();
        public List<BuildingZoneOccupant> Occupants = new List<BuildingZoneOccupant>();
        public List<BuildingZoneEnvironment> Environments = new List<BuildingZoneEnvironment>();       
        public BuildingDesignParameters() { }
        public string Header(string sep)
        {
            return string.Join(sep,
                Geometry.Header(sep),
                Construction.Header(sep),
                WWR.Header(sep),
                Service.Header(sep),
                string.Join(sep, Operations.Select(o => o.Header(sep))),
                string.Join(sep, Occupants.Select(o => o.Header(sep))),
                string.Join(sep, Environments.Select(o => o.Header(sep))));
        }
        public string ToString(string sep)
        {
            return string.Join(sep,
                Geometry.ToString(sep),
                Construction.ToString(sep),
                WWR.ToString(sep),
                Service.ToString(sep),
                string.Join(sep, Operations.Select(o => o.ToString(sep))),
                string.Join(sep, Occupants.Select(o => o.ToString(sep))),
                string.Join(sep, Environments.Select(o => o.ToString(sep))));
        }
    }
}
