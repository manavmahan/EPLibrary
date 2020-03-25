using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ElectricLoadCenterDistribution
    {
        public string Name = "Electric Load Center";
        public ElectricLoadCenterGenerators GeneratorList;
        public string GeneratorOperationSchemeType = "DemandLimit";
        public double DemandLimitSchemePurchasedElectricDemandLimit = 100000;
        public string TrackScheduleNameSchemeScheduleName = " ";
        public string TrackMeterSchemeMeterName = " ";
        public string ElectricalBussType = "AlternatingCurrent";
        public string InverterObjectName = " ";
        public string ElectricalStorageObjectName = " ";

        public ElectricLoadCenterDistribution() { }
        public ElectricLoadCenterDistribution(ElectricLoadCenterGenerators GeneratorList1)
        {
            GeneratorList = GeneratorList1;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "ElectricLoadCenter:Distribution,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(GeneratorList.Name,  "Generator List Name"),
                Utility.IDFLineFormatter(GeneratorOperationSchemeType, "Generator Operation Scheme Type"),
                Utility.IDFLineFormatter(DemandLimitSchemePurchasedElectricDemandLimit, "Demand Limit Scheme Purchased Electric Demand Limit {W}"),
                Utility.IDFLineFormatter(TrackScheduleNameSchemeScheduleName, "Track Schedule Name Scheme Schedule Name"),
                Utility.IDFLineFormatter(TrackMeterSchemeMeterName, "Track Meter Scheme Meter Name"),
                Utility.IDFLineFormatter(ElectricalBussType, "Electrical Buss Type"),
                Utility.IDFLineFormatter(InverterObjectName, "Inverter Object Name"),
                Utility.IDFLastLineFormatter(ElectricalStorageObjectName, "Electrical Storage Object Name")
            };
        }
    }
}
