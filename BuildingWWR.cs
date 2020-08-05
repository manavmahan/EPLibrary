using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class BuildingWWR
    {
        public bool EachWallSeparately;
        public double North;
        public double East;
        public double West;
        public double South;
        public BuildingWWR() { }
        public BuildingWWR(double north, double east, double west, double south)
        {
            North = north;
            East = east;
            West = west;
            South = south;
        }
        public string ToString(string sep)
        {
            return string.Join(sep, North, East, West, South);
        }
        public string Header(string sep)
        {
            return string.Join(sep, "WWR_North", "WWR_East", "WWR_West", "WWR_South");
        }
    }
}
