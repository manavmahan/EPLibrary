using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    public class BuildingZoneOccupant
    {
        public double AreaPerPerson;
        public string Name = "Building";
        public BuildingZoneOccupant() { }

        public BuildingZoneOccupant(double AreaPerPerson) { this.AreaPerPerson = AreaPerPerson; }
        public string ToString(string sep)
        {
            return string.Join(sep, AreaPerPerson);
        }
        public string Header(string sep)
        {
            return string.Format(
                "{0}Area Per Person",
                Name + ":", sep);
        }
    }
}
