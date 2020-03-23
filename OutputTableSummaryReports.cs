using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class OutputTableSummaryReports
    {
        public string report1 = "ZoneComponentLoadSummary",
        report2 = "ComponentSizingSummary",
            report3 = "EquipmentSummary",
            report4 = "HVACSizingSummary",
            report5 = "ClimaticDataSummary",
            report6 = "OutdoorAirSummary",
            report7 = "EnvelopeSummary";

        public OutputTableSummaryReports()
        {

        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Table:SummaryReports,");
            info.Add("\t" + report1 + ",\t\t\t\t!-Report 1 Name");
            info.Add("\t" + report2 + ",\t\t\t\t!-Report 2 Name");
            info.Add("\t" + report3 + ",\t\t\t\t!-Report 3 Name");
            info.Add("\t" + report4 + ",\t\t\t\t!-Report 4 Name");
            info.Add("\t" + report5 + ",\t\t\t\t!-Report 5 Name");
            info.Add("\t" + report6 + ",\t\t\t\t!-Report 6 Name");
            info.Add("\t" + report7 + ";\t\t\t\t!-Report 7 Name");

            return info;
        }


    }
}
