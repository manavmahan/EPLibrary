using System;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilityDistributionFunction
    {
        //'unif','triang','norm','lognorm'
        public float Mean, VariationOrSD, Sensitivity;
        public string Label, Unit;
        public PDF Distribution;
        public ProbabilityDistributionFunction() { }
        public ProbabilityDistributionFunction(string label, string unit) {
            Label = label;
            Unit = unit;
        }
        public ProbabilityDistributionFunction(string label, string unit, float mean, float variation)
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
        public float Mean, VariationOrSD; public PDF Distribution;

        public PDFValues()
        {

        }
        public PDFValues(float Mean, float VariationOrSD, PDF Distribution)
        {
            this.Mean = Mean;
            this.VariationOrSD = VariationOrSD;
            this.Distribution = Distribution;
        }
    }
}