﻿using System;
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
        public double OffsetDistance;
        public int NumSamplesDS, NumSamplesOp;
        public SamplingScheme SamplingDS, SamplingOp;
        public ProbabilityDistributionFunction EnergyTarget = new ProbabilityDistributionFunction() { Mean = 60, VariationOrSD = 5, Distribution = PDF.unif };
        public PEASettings() { }
    }
}