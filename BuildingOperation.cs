using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class BuildingOperation
    {
        public double operatingHours = 0, lightHeatGain = 0, equipmentHeatGain = 0, boilerEfficiency = 0, chillerCOP = 0;
        public double areaPerPeople = 10, ventillation = 0;
        public double startTime = 0, endTime = 0;
        public double[] heatingSetPoints, coolingSetPoints;
        public BuildingOperation() { }
        public string ToCSVString()
        {
            return string.Join(",", operatingHours, lightHeatGain, equipmentHeatGain, boilerEfficiency, chillerCOP);
        }
        public string Header()
        {
            return string.Join(",", "Operating Hours", "Light Heat Gain", "Equipment Heat Gain", "Boiler Efficiency", "Chiller COP");
        }
    }
}
