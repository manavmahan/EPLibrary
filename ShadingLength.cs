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
        public float north;
        public float east;
        public float south;
        public float west;

        public ShadingLength() { }

        public ShadingLength(float north, float east, float south, float west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }


    }
}
