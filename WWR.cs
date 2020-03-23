using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class WWR
    {
        public double north;
        public double east;
        public double south;
        public double west;

        public WWR() { }

        public WWR(double north, double east, double south, double west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }
        public string ToCSVString()
        {
            return string.Join(",", north, east, west, south);
        }
        public string Header()
        {
            return string.Join(",", "WWR_North", "WWR_East", "WWR_West", "WWR_South");
        }
    }
}
