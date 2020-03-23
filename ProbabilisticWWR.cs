using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticWWR
    {
        public double[] north;
        public double[] east;
        public double[] south;
        public double[] west;

        public ProbabilisticWWR() { }

        public ProbabilisticWWR(double[] north, double[] east, double[] south, double[] west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }
        public WWR GetAverage()
        {
            return new WWR(north.Average(), east.Average(), west.Average(), south.Average());
        }
        public List<string> ToCSVString()
        {
            return new List<string>(){
                string.Join(",", north[0], east[0], west[0], south[0]),
                string.Join(",", north[1], east[1], west[1], south[1])
            };
        }
    }
}
