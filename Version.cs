using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Version
    {
        public string VersionIdentifier = "9.2.0";
        public Version()
        {

        }
        public List<string> WriteInfo()
        {
            return new List<string>() { "Version,", Utility.IDFLastLineFormatter(VersionIdentifier, "Version Identifier") };
        }
    }
}
