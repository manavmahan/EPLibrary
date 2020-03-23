using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ConvergenceLimits
    {
        public int minimumSystemTimestep = 0,
        maximumHVACIterations = 20,
            minimumPlantIterations = 2,
            maximumPlantIterations = 8;

        public ConvergenceLimits()
        {

        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "ConvergenceLimits,",
                Utility.IDFLineFormatter(minimumSystemTimestep, "Minimum System Timestep {minutes}"),
                Utility.IDFLineFormatter(maximumHVACIterations, "Maximum HVAC Iterations"),
                Utility.IDFLineFormatter(minimumPlantIterations, "Minimum Plant Iterations"),
                Utility.IDFLastLineFormatter(maximumPlantIterations, "Maximum Plant Iterations")
            };
        }
    }
}
