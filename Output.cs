using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Output
    {
        public Output() { }
        public OutputVariableDictionary varDict;
        public Report report;
        public OutputTableSummaryReports tableSumReports;
        public OutputcontrolTableStyle tableStyle;
        public List<OutputVariable> vars;
        public OutputDiagnostics diagn;
        public List<OutputPreProcessorMessage> preprocessormess;

        public Output(Dictionary<string, string> variables)
        {
            varDict = new OutputVariableDictionary();
            report = new Report();
            tableSumReports = new OutputTableSummaryReports();
            tableStyle = new OutputcontrolTableStyle();
            diagn = new OutputDiagnostics();

            preprocessormess = new List<OutputPreProcessorMessage>();
            preprocessormess.Add(new OutputPreProcessorMessage(new List<String>(new string[] { "Cannot find Energy +.idd as specified in Energy +.ini." })));
            preprocessormess.Add(new OutputPreProcessorMessage(new List<String>(new string[] { "Since the Energy+.IDD file cannot be read no range or choice checking was", "performed." })));

            vars = new List<OutputVariable>();
            foreach (string key in variables.Keys)
            { vars.Add(new OutputVariable(key, variables[key])); }
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:VARIABLEDICTIONARY ===========");
            info.AddRange(varDict.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:SURFACES:DRAWING ===========");

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: REPORT ===========");
            info.AddRange(report.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:TABLE:SUMMARYREPORTS ===========");
            info.AddRange(tableSumReports.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUTCONTROL:TABLE:STYLE ===========");
            info.AddRange(tableStyle.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:VARIABLE ===========");
            foreach (OutputVariable var in vars)
            { info.AddRange(var.writeInfo()); }

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:DIAGNOSTICS ===========");
            info.AddRange(diagn.writeInfo());

            info.Add("\n\n!-   ===========  ALL OBJECTS IN CLASS: OUTPUT:PREPROCESSORMESSAGE ===========");
            foreach (OutputPreProcessorMessage mes in preprocessormess)
            { info.AddRange(mes.writeInfo()); }

            return info;
        }

    }
}
