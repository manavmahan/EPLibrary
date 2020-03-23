using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class OutputVariableDictionary
    {
        public string keyField = "idf";
        public OutputVariableDictionary()
        {

        }

        public List<string> writeInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nOutput:VariableDictionary,");
            info.Add("\t" + keyField + ";\t\t\t\t!-Key Field");

            return info;
        }
    }
}
