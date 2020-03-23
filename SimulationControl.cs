using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class SimulationControl
    {
        public string doZoneSizingCalculation = "Yes",
        doSystemSizingCalculation = "Yes",
            doPlantSizingCalculation = "Yes",
            runSimulationForSizingPeriods = "No",
            runSimulationForWeatherFileRunPeriods = "Yes";

        public SimulationControl()
        {

        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "SimulationControl,",
                Utility.IDFLineFormatter(doZoneSizingCalculation, "Do Zone Sizing Calculation"),
                Utility.IDFLineFormatter(doSystemSizingCalculation, "Do System Sizing Calculation"),
                Utility.IDFLineFormatter(doPlantSizingCalculation, "Do Plant Sizing Calculation"),
                Utility.IDFLineFormatter(runSimulationForSizingPeriods, "Run Simulation for Sizing Periods"),
                Utility.IDFLastLineFormatter(runSimulationForWeatherFileRunPeriods, "Run Simulation for Sizing Periods"),
            };
        }
    }
}
