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
        public BuildingSurface bSurface;
        public string PhotovoltaicPerformanceObjectType;
        public PhotovoltaicPerformanceSimple pperformance;
        public string HeatTransferIntegrationMode = "Decoupled";

        public double GeneratorPowerOutput = 50000;
        public ScheduleCompact Schedule;
        public double RatedThermalElectricalPowerRatio = 0;


        public GeneratorPhotovoltaic() { }
        public GeneratorPhotovoltaic(BuildingSurface Surface, PhotovoltaicPerformanceSimple Performance, ScheduleCompact scheduleOn)
        {
            Name = "PV on " + Surface.Name;
            bSurface = Surface;
            pperformance = Performance;
            PhotovoltaicPerformanceObjectType = Performance.Type;
            Schedule = scheduleOn;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "Generator:Photovoltaic,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(bSurface.Name, "Surface Name"),
                Utility.IDFLineFormatter(PhotovoltaicPerformanceObjectType, "Photovoltaic Performance Object Type"),
                Utility.IDFLineFormatter(pperformance.Name, "Module Performance Name"),
                Utility.IDFLastLineFormatter(HeatTransferIntegrationMode, "Heat Transfer Integration Mode")
            };
        }
    }
}
