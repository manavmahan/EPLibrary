﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{

    [Serializable]
    public class OutputDiagnostics
    {
        public OutputDiagnostics()
        {

        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Diagnostics,");
            info.Add("DisplayAdvancedReportVariables, DisplayExtraWarnings;");
            return info;
        }
    }
}
