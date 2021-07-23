using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class PhotovoltaicPerformanceSimple
    {
        public string Name = "Simple Flat PV";
        public string Type = "PhotovoltaicPerformance:Simple";
        public float FractionSurface = 0.7f;
        public string ConversionEff = "FIXED";
        public float CellEff = 0.12f;
        public PhotovoltaicPerformanceSimple() { }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "PhotovoltaicPerformance:Simple,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(FractionSurface, "Fraction of Surface Area with Active Solar Cells {dimensionless}"),
                Utility.IDFLineFormatter(ConversionEff, "Conversion Efficiency Input Mode"),
                Utility.IDFLastLineFormatter(CellEff, "Value for Cell Efficiency if Fixed")
            };
        }
    }
}
