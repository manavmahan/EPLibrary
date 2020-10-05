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
        public double Mean, VariationOrSD, Sensitivity;
        public string Label, Unit;
        public PDF Distribution;
        public ProbabilityDistributionFunction() { }
        public ProbabilityDistributionFunction(string label, string unit) {
            Label = label;
            Unit = unit;
        }
        public ProbabilityDistributionFunction(string label, string unit, double mean, double variation)
        {
            Mean = mean; VariationOrSD = variation;
            Distribution = PDF.unif;
        }
        public void UpdateProbabilityDistributionFunction(PDFValues p)
        {
            Mean = p.Mean;
            VariationOrSD = p.VariationOrSD;
            Distribution = p.Distribution;
        }
        public override string ToString()
        {
            string val;
            if (Mean != 0 || VariationOrSD != 0)
            {
                val = string.Join(",", Mean, VariationOrSD, Distribution);
            }
            else
            {
                val = "";
            }
            return val;
        }
    }

    public class PDFValues{
        public double Mean, VariationOrSD; public PDF Distribution;

        public PDFValues()
        {

        }
        public PDFValues(double Mean, double VariationOrSD, PDF Distribution)
        {
            this.Mean = Mean;
            this.VariationOrSD = VariationOrSD;
            this.Distribution = Distribution;
        }
    }
}