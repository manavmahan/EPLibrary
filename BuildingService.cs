using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    public class BuildingService
    {      
        public double BoilerEfficiency, ChillerCOP;
        public HVACSystem HVACSystem;
        public BuildingService() { }
        public BuildingService(double BoilerEfficiency, double ChillerCOP) 
        {
            this.BoilerEfficiency = BoilerEfficiency; this.ChillerCOP = ChillerCOP;
        }
        public string ToString(string sep)
        {
            return string.Join(sep, BoilerEfficiency, ChillerCOP);
        }
        public string Header(string sep)
        {
            return string.Join(sep, "Boiler Efficiency", "Chiller COP");
        }
    }
}
