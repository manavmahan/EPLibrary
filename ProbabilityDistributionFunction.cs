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
                    this.Distribution = PDF.tria;
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
        public override string ToString()
        {
            string val;
            if (Mean != 0)
            {
                switch (Distribution)
                {
                    case PDF.unif:
                    case PDF.norm:
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