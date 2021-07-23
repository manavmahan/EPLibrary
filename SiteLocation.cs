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
        public float latitude { get; set; }
        public float longitude { get; set; }
        public float timeZone { get; set; }
        public float elevation { get; set; }

        public SiteLocation() { }
        public SiteLocation(Location location)
        {
            name = location.ToString();
            switch (location)
            {         
                case Location.MUNICH_DEU:
                default:
                    latitude = 48.13f;
                    longitude = 11.7f;
                    timeZone = 1.0f;
                    elevation = 529.0f;
                    break;
                case Location.BRUSSELS_BEL:
                    latitude = 50.9f;
                    longitude = 4.53f;
                    timeZone = 1.0f;
                    elevation = 58.0f;
                    break;
                case Location.BERLIN_DEU:
                    latitude = 50.9f;
                    longitude = 13.40f;
                    timeZone = 1.0f;
                    elevation = 49.0f;
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
