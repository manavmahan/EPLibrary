using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class SiteLocation
    {
        public string name { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double timeZone { get; set; }
        public double elevation { get; set; }

        public SiteLocation() { }
        public SiteLocation(string location)
        {
            switch (location)
            {
                case "MUNICH_DEU":
                    name = "MUNICH_DEU";
                    latitude = 48.13;
                    longitude = 11.7;
                    timeZone = 1.0;
                    elevation = 529.0;
                    break;
                default:
                    name = "MUNICH_DEU";
                    latitude = 48.13;
                    longitude = 11.7;
                    timeZone = 1.0;
                    elevation = 529.0;
                    break;
            }
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "Site:Location,",
                Utility.IDFLineFormatter(name, "Name"),
                Utility.IDFLineFormatter(latitude, "Latitude {deg}"),
                Utility.IDFLineFormatter(longitude, "Longitude {deg}"),
                Utility.IDFLineFormatter(timeZone, "Time Zone {hr}"),
                Utility.IDFLastLineFormatter(elevation, "Elevation {m}")
            };
        }
    }
}
