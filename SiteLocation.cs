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
        public SiteLocation(Location location)
        {
            name = location.ToString();
            switch (location)
            {         
                case Location.MUNICH_DEU:
                default:
                    latitude = 48.13;
                    longitude = 11.7;
                    timeZone = 1.0;
                    elevation = 529.0;
                    break;
                case Location.BRUSSELS_BEL:
                    latitude = 50.9;
                    longitude = 4.53;
                    timeZone = 1.0;
                    elevation = 58.0;
                    break;
                case Location.BERLIN_DEU:
                    latitude = 50.9;
                    longitude = 13.40;
                    timeZone = 1.0;
                    elevation = 49.0;
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
