using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class OutputPreProcessorMessage
    {
        public string preprocessorName;
        public string errorSeverity;
        public List<string> messageLines;

        public OutputPreProcessorMessage() { }
        public OutputPreProcessorMessage(List<string> messageLines)
        {
            preprocessorName = "ExpandObjects";
            errorSeverity = "Warning";
            this.messageLines = messageLines;
        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:PreprocessorMessage,");
            info.Add("\t" + preprocessorName + ",\t\t\t\t!-Preprocessor Name");
            info.Add("\t" + errorSeverity + ",\t\t\t\t!-Error Severity");
            for (int i = 1; i < messageLines.Count; i++)
            {
                info.Add("\t" + messageLines[i - 1] + ",\t\t\t\t!-Message Line " + i);
            }
            info.Add("\t" + messageLines[messageLines.Count - 1] + ";\t\t\t\t!-Message Line " + messageLines.Count);


            return info;
        }

    }
}

