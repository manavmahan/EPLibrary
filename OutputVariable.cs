using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class OutputVariable
    {
        public string keyValue { get; set; }
        public string variableName { get; set; }
        public string reportingFrequency { get; set; }

        public OutputVariable() { }
        public OutputVariable(string varName, string reportfreq)
        {
            keyValue = "*";
            variableName = varName;
            reportingFrequency = reportfreq;
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Variable,");
            info.Add("\t" + keyValue + ",\t\t\t\t!-Key Value");
            info.Add("\t" + variableName + ",\t\t\t\t!-Variable Name");
            info.Add("\t" + reportingFrequency + ";\t\t\t\t!-Reporting Frequency");

            return info;
        }
    }
}
