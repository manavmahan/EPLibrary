using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Boiler
    {
        public string name;
        public double boilerEfficiency;
        public string fuelType;

        public Boiler() { }
        public Boiler(double efficiency, string fuelType)
        {
            name = "Main Boiler";
            boilerEfficiency = efficiency;
            this.fuelType = fuelType;
        }

        public List<String> writeInfo()
        {
            List<string> info = new List<string>() {
                "HVACTemplate:Plant:Boiler,",
                name + ",             !-Name",
                "HotWaterBoiler,          !-Boiler Type",
                "autosize,                !-Capacity { W}",
                boilerEfficiency + ",                     !-Efficiency",
                Utility.IDFLineFormatter(fuelType,  "Fuel Type"),
                "1,                       !-Priority",
                "1.2,                     !-Sizing Factor",
                "0.1,                     !-Minimum Part Load Ratio",
                "1.1,                     !-Maximum Part Load Ratio",
                "0.9,                     !-Optimum Part Load Ratio",
                "99.9; !-Water Outlet Upper Temperature Limit { C}"
            };

            return info;
        }
    }
}
