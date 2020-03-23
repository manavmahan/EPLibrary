using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ShadingLength
    {
        public double north;
        public double east;
        public double south;
        public double west;

        public ShadingLength() { }

        public ShadingLength(double north, double east, double south, double west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }


    }
}
