using IDFObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    public class ShadingBuildingDetailed
    {
        public Building Building;
        public string Name, ShadowingTransmittanceSchedule;
        public XYZList XYZList;
        public ShadingBuildingDetailed(Building Building, string Name, string ShadowingTransmittanceSchedule, XYZList XYZList)
        {
            this.Building = Building;           
            this.Name = Name; this.ShadowingTransmittanceSchedule = ShadowingTransmittanceSchedule; this.XYZList = XYZList;
            Building.DetachedShading.Add(this);
        }
        public List<string> WriteInfo() => new List<string>()
        {
            "Shading:Building:Detailed ,",
            Utility.IDFLineFormatter(Name, "Detached Shading"),
            Utility.IDFLineFormatter(ShadowingTransmittanceSchedule, "Shadowing Transmittance & Schedule"),
            Utility.IDFLineFormatter(XYZList.xyzs.Count, "No. of Vertices"),
            string.Join("\n", XYZList.WriteInfo())
        };
    }
}
