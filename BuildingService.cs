using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class BuildingService
    {      
        public float BoilerEfficiency, HeatingCOP, CoolingCOP;
        public HVACSystem HVACSystem;
        public BuildingService() { }
        public BuildingService(float BoilerEfficiency, float HeatingCOP, float CoolingCOP) 
        {
            this.BoilerEfficiency = BoilerEfficiency; this.HeatingCOP = HeatingCOP;  this.CoolingCOP = CoolingCOP;
        }
        public string ToString(string sep)
        {
            return string.Join(sep, BoilerEfficiency, HeatingCOP, CoolingCOP);
        }
        public string Header(string sep)
        {
            return string.Join(sep, "Boiler Efficiency", "Heating COP", "Cooling COP");
        }
    }
}
