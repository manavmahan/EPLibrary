using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticBuildingOperation
    {
        public double[] operatingHours, startTime, endTime, areaPerPeople, ventilation, lightHeatGain, equipmentHeatGain, boilerEfficiency, chillerCOP;
        public ProbabilisticBuildingOperation() { }

        public BuildingOperation GetAverage()
        {
            BuildingOperation boP = new BuildingOperation();
            try
            {
                boP.startTime = startTime.Average();
                boP.endTime = endTime.Average();
                boP.areaPerPeople = areaPerPeople.Average();
                boP.ventillation = ventilation.Average();
                boP.operatingHours = boP.endTime - boP.startTime;
            }
            catch { }

            try { boP.operatingHours = operatingHours.Average(); } catch { }
            boP.lightHeatGain = lightHeatGain.Average();
            boP.equipmentHeatGain = equipmentHeatGain.Average();
            boP.boilerEfficiency = boilerEfficiency.Average();
            boP.chillerCOP = chillerCOP.Average();
            return boP;
        }
        public List<string> ToCSVString()
        {
            try
            {
                return new List<string>(){
                string.Join(",", operatingHours[0], lightHeatGain[0], equipmentHeatGain[0], boilerEfficiency[0], chillerCOP[0]),
                string.Join(",", operatingHours[1], lightHeatGain[1], equipmentHeatGain[1], boilerEfficiency[1], chillerCOP[1]) };
            }

            catch
            {
                return new List<string>(){
                string.Join(",", startTime[0], endTime[0], lightHeatGain[0], equipmentHeatGain[0], boilerEfficiency[0], chillerCOP[0]),
                string.Join(",", startTime[1], endTime[1], lightHeatGain[1], equipmentHeatGain[1], boilerEfficiency[1], chillerCOP[1]) };
            }
        }

    }
}
