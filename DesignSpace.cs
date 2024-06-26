﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class DesignSpace
    {
        public DesignSpace() { }    
        public Building Building;

        public List<XYZList> SamplesFloorPlans = new List<XYZList>();

        public ProbabilisticBuildingDesignParameters Parameters;
        public List<BuildingDesignParameters> SampleParameters = new List<BuildingDesignParameters>();
        public List<EPBuilding> EPP = new List<EPBuilding>();
        public EPBuilding EP;

        private List<Building> AllSamples = new List<Building>();
        public void SetSamples(List<Building> Samples) => AllSamples = Samples;
        public void SetSample(Building Sample, int n) => AllSamples[n] = Sample;
        public void AddSample(Building Sample) => AllSamples.Add(Sample);
        public void AddSamples(List<Building> Samples) => AllSamples.AddRange(Samples);
        public void RemoveAllSamples() => AllSamples = new List<Building>();
        public List<Building> GetSamples() => AllSamples;
        public void OrderByEUI() => AllSamples = AllSamples.OrderBy(b => b.EP.EUI).ToList();

        public List<float[]> EnergyIntervals = new List<float[]>();
        public void CreateEnergyIntervals(ProbabilityDistributionFunction EnergyTarget, int num, int count)
        {
            EnergyIntervals = new List<float[]>();
            float var = EnergyTarget.Distribution == PDF.norm ? 3 * EnergyTarget.VariationOrSD : EnergyTarget.VariationOrSD;
            switch (EnergyTarget.Distribution)
            {
                default:
                case PDF.unif:
                    float interval = 2 * var / count;
                    for (int i = 0; i < count; i++)
                    {
                        float low = EnergyTarget.Mean + (i-count/2) * interval;
                        for (int m = 0; m < num/count; m++)
                            EnergyIntervals.Add(new float[] { low, low + interval });
                    }
                    break;
            }
        }
        public void AddSamplesForEnergyTarget(List<Building> samples)
        {
            try
            {
                samples = samples.Where(s => s.EP.EUI >= EnergyIntervals.First()[0] && s.EP.EUI <= EnergyIntervals.Last()[1]).ToList();
                samples.ForEach(s =>
                {
                    try
                    {
                        float[] ei = EnergyIntervals.First(e => s.EP.EUI > e[0] && s.EP.EUI < e[1]);
                        EnergyIntervals.Remove(ei);
                        AddSample(s);
                    }
                    catch { }
                });
            }
            catch { }
        }
        public void AdjustSamplesToMultiples()
        {
            AllSamples = AllSamples.Take(5 * (AllSamples.Count / 5)).ToList();
        }
        public void AdjustParameters()
        {
            Parameters.GetProbabilisticParametersBasedOnSamples(AllSamples.Select(s => s.Parameters).ToList());
        }
    }
}
 