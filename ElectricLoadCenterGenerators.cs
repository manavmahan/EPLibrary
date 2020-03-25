using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ElectricLoadCenterGenerators
    {
        public string Name = "Supplementary Generator";
        public List<GeneratorPhotovoltaic> Generator = new List<GeneratorPhotovoltaic>();

        public ElectricLoadCenterGenerators() { }

        public ElectricLoadCenterGenerators(List<GeneratorPhotovoltaic> Generators)
        {
            Generator = Generators;
        }
        public List<string> WriteInfo()
        {
            List<string> GeneratorInfo = new List<string>
            {
                "ElectricLoadCenter:Generators,",
                Utility.IDFLineFormatter(Name, "Name")
            };
            Generator.ForEach
            (
                g => GeneratorInfo.AddRange(new List<string>()
                {
                    Utility.IDFLineFormatter(g.Name, "Generator Name"),
                    Utility.IDFLineFormatter(g.Type, "Generator Type"),
                    Utility.IDFLineFormatter(g.GeneratorPowerOutput, "Generator Power Output"),
                    Utility.IDFLineFormatter(g.Schedule.name, "Generator Schedule"),
                    Utility.IDFLineFormatter(g.RatedThermalElectricalPowerRatio, "Generator Rated Thermal to Electrical Power Ratio")
                })
            );
            Utility.ReplaceLastComma(GeneratorInfo);
            return GeneratorInfo;
        }
    }
}
