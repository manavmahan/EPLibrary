using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    public class BEnergyPerformance
    {
        public double TotalArea, TotalVolume, BoilerEnergy, ChillerEnergy, ThermalEnergy, OperationalEnergy,
            ZoneHeatingEnergy, ZoneCoolingEnergy, LightingEnergy, EUI;
        public BEnergyPerformance() { }
    }
    public class ProbablisticBEnergyPerformance
    {
        public double TotalArea, TotalVolume;
        public double[] BoilerEnergy, ChillerEnergy, ThermalEnergy, OperationalEnergy,
            ZoneHeatingEnergy, ZoneCoolingEnergy, LightingEnergy, EUI;
        public ProbablisticBEnergyPerformance() { }
    }
}
