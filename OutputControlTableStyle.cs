using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class OutputcontrolTableStyle
    {
        public string columnSeparator = "XMLandHTML";
        public OutputcontrolTableStyle()
        {

        }
        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutputControl:Table:Style,");
            info.Add("\t" + columnSeparator + ";\t\t\t\t!-Column Separator");

            return info;
        }
    }
}
