using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class DesignOption
    {
        public DesignOption() { }
        public ProbabilisticBuildingDesignParameters Parameters;// = new ProbabilisticBuildingDesignParameters();
        //public int NumSamples;
        //public SamplingScheme SamplingScheme;
        public Building Building;
        public ProbabilityDistributionFunction EnergyTarget = new ProbabilityDistributionFunction();
        public string MassId;
        public List<string> Zones = new List<string>();
        private List<Building> AllSamples = new List<Building>();
        public void SetSamples(List<Building> Samples) => AllSamples = Samples;
        public void SetSample(Building Sample, int n) => AllSamples[n] = Sample;
        public void AddSample(Building Sample) => AllSamples.Add(Sample);
        public List<Building> GetSamples() => AllSamples;
        public void OrderByEUI() => AllSamples = AllSamples.OrderBy(b => b.EP.EUI).ToList();
    }
}
