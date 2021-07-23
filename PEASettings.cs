using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class PEASettings
    {
        public Location Location;
        public LevelOfDevelopment LevelOfDevelopment;
        public SimulationTool SimulationTool;
        public float OffsetDistance;
        public int NumSamplesDS, NumSamplesOp;
        public SamplingScheme SamplingDS, SamplingOp;
        public RandomGeometry RandomGeometry = new RandomGeometry();
        public ProbabilityDistributionFunction EnergyTarget = new ProbabilityDistributionFunction() { Mean = 60, VariationOrSD = 5, Distribution = PDF.unif };
        public PEASettings() { }
    }
}
