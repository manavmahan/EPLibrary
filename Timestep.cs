using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Timestep
    {
        public Timestep() { }
        public int NumberOfTimestepsPerHour { get; set; }

        public Timestep(int numberOfTimestepsPerHour)
        {
            this.NumberOfTimestepsPerHour = numberOfTimestepsPerHour;
        }
        public List<string> WriteInfo()
        {
            return new List<string>() { "Timestep,", Utility.IDFLastLineFormatter(NumberOfTimestepsPerHour, "Number of Timesteps per Hour") };
        }
    }
}
