using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilityDistributionFunction
    {
        //'unif','triang','norm','lognorm'
        public double Mean, VariationOrSD, Range, Max, Min;
        public PDF Distribution;

        public ProbabilityDistributionFunction() { }
        public ProbabilityDistributionFunction(double mean, double variation)
        {
            Mean = mean; VariationOrSD = variation;
            Max = mean + variation;
            Min = mean - variation;
            Range = Max - Min;
            Distribution = PDF.unif;
        }
        
        
        public ProbabilityDistributionFunction(double mean, double VariationOrSD, string Distribution)
        {
            switch (Distribution)
            {
                case "norm":
                default:
                    Mean = mean;
                    this.VariationOrSD = VariationOrSD;
                    this.Distribution = PDF.norm;
                    break;
                case "triang":
                    Mean = mean;
                    this.VariationOrSD = VariationOrSD;
                    Max = Mean + this.VariationOrSD;
                    Min = Mean - this.VariationOrSD;
                    Range = Max - Min;
                    this.Distribution = PDF.triang;
                    break;
                case "unif":
                    Mean = mean;
                    this.VariationOrSD = VariationOrSD;
                    Max = Mean + this.VariationOrSD;
                    Min = Mean - this.VariationOrSD;
                    Range = Max - Min;
                    this.Distribution = PDF.unif;
                    break;
            }
        }
        internal double[] GetLHSSamples(Random random, int samples)
        {
            double[] vals = new double[samples];
            switch (Distribution) 
            {
                case PDF.unif:
                    List<int> parts = Enumerable.Range(0, samples)
                                    .Select(x => new { Number = random.Next(), Item = x })
                                    .OrderBy(x => x.Number)
                                    .Select(x => x.Item).ToList();
                    double r = Range / samples;
                    for (int n = 0; n < samples; n++)
                    {
                        vals[n] = Min + (parts[n] + random.NextDouble()) * r;
                    } 
                    break;
            }
            return vals;
        }

        public double[] GetMCSamples(Random random, int count)
        {
            double[] val = new double[count];          
            switch (Distribution)
            {
                case PDF.unif:
                default:
                    for (int n = 0; n < count; n++)
                    {
                        val[n] = Min + random.NextDouble() * VariationOrSD;
                    }
                    break;
            }
            return val;
        }
        public override string ToString()
        {
            string val;
            if (Mean != 0)
            {
                switch (Distribution)
                {
                    case PDF.unif:
                    default:
                        val = string.Join(",", Mean, VariationOrSD, Distribution);
                        break;
                }
            }
            else
            {
                val = "";
            }
            return val;          
        }
    }
}
