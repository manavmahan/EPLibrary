using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Report
    {
        public string reportType = "dxf";
        public Report()
        {

        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:Surfaces:Drawing,");
            info.Add("\t" + reportType + ";\t\t\t\t!-Report Type");

            return info;
        }
    }
}
