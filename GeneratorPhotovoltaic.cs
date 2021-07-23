using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class GeneratorPhotovoltaic
    {
        public string Name;
        public string Type = "Generator:Photovoltaic";
        public string bSurfaceName;
        public string PhotovoltaicPerformanceObjectType;
        public PhotovoltaicPerformanceSimple pperformance;
        public string HeatTransferIntegrationMode = "Decoupled";

        public float GeneratorPowerOutput = 50000;
        public string Schedule;
        public float RatedThermalElectricalPowerRatio = 0;


        public GeneratorPhotovoltaic() { }
        public GeneratorPhotovoltaic(Surface Surface, PhotovoltaicPerformanceSimple Performance, string schedule)
        {
            Name = "PV on " + Surface.Name;
            bSurfaceName = Surface.Name;
            pperformance = Performance; 
            PhotovoltaicPerformanceObjectType = Performance.Type;
            Schedule = schedule;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "Generator:Photovoltaic,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(bSurfaceName, "Surface Name"),
                Utility.IDFLineFormatter(PhotovoltaicPerformanceObjectType, "Photovoltaic Performance Object Type"),
                Utility.IDFLineFormatter(pperformance.Name, "Module Performance Name"),
                Utility.IDFLastLineFormatter(HeatTransferIntegrationMode, "Heat Transfer Integration Mode")
            };
        }
    }
}
