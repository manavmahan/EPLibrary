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
        public float North,  East, West, South;
        public BuildingWWR() { }
        public BuildingWWR(float north, float east, float west, float south)
        {
            North = north;
            East = east;
            West = west;
            South = south;
        }
        public override string ToString()
        {
            return this.ToString(",");
        }
        public string ToString(string sep)
        {
            return string.Join(sep, North, East, West, South);
        }
        public string Header(string sep)
        {
            return string.Join(sep, "WWR (North)", "WWR (East)", "WWR (West)", "WWR (South)");
        }
    }
}
