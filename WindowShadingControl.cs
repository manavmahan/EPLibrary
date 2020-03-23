using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class WindowShadingControl
    {
        public string name = "CONTROL ON ZONE TEMP";
        public Zone zone;
        public string sequenceNumber = "";
        public string shadingType = "InteriorShade";
        public string construction = "";

        public string shadingControlType = "OnIfHighZoneAirTemperature";
        public string scehduleName = "";
        public double setPoint = 23;
        public string scheduled = "NO";
        public string glareControl = "NO";

        public string material = "ROLL SHADE";
        public string angleControl = "";
        public string slatSchedule = "";

        public string setPoint2 = "";
        public DayLighting daylightcontrolobjectname;
        public string multipleSurfaceControlType = "";
        public Fenestration fenestration;

        public WindowShadingControl() { }
        public WindowShadingControl(Fenestration fenestration)
        {
            this.fenestration = fenestration; zone = fenestration.Face.Zone;
            name = string.Format("CONTROL ON ZONE TEMP {0}", fenestration.Name);
            daylightcontrolobjectname = zone.DayLightControl;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "WindowShadingControl,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(zone.Name, "Zone Name"),
                Utility.IDFLineFormatter(sequenceNumber, "Sequence Number"),
                Utility.IDFLineFormatter(shadingType, "Shading Type"),
                Utility.IDFLineFormatter(construction, "Construction with Shading Name"),
                Utility.IDFLineFormatter(shadingControlType, "Shading Control Type"),
                Utility.IDFLineFormatter(scehduleName, "Schedule Name"),
                Utility.IDFLineFormatter(setPoint, "Setpoint {W/m2, W or deg C}"),
                Utility.IDFLineFormatter(scheduled, "Shading Control Is Scheduled"),
                Utility.IDFLineFormatter(glareControl, "Glare Control Is Active"),
                Utility.IDFLineFormatter(material, "Shading Device Material Name"),
                Utility.IDFLineFormatter(angleControl, "Type of Slat Angle Control for Blinds"),
                Utility.IDFLineFormatter(slatSchedule, "Slat Angle Schedule Name"),
                Utility.IDFLineFormatter(setPoint2, "Setpoint 2"),
                Utility.IDFLineFormatter(daylightcontrolobjectname==null? "": daylightcontrolobjectname.Name , "Daylight Control Object Name"),
                Utility.IDFLineFormatter(multipleSurfaceControlType, "Multiple Control Type"),
                Utility.IDFLastLineFormatter(fenestration.Name, "Fenestration")

            };
        }
    }
}
