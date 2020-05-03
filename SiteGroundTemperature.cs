using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class SiteGroundTemperature
    {
        public double jan, feb, mar, apr, may, jun, jul, aug, sep, oct, nov, dec;
        public SiteGroundTemperature()
        { }
        public SiteGroundTemperature(Location location)
        {
            switch (location)
            {
                case Location.MUNICH_DEU:
                default:
                    jan = 6.17; feb = 5.07; mar = 5.33; apr = 6.27; may = 9.35; jun = 12.12;
                    jul = 14.32; aug = 15.48; sep = 15.20; oct = 13.62; nov = 11.08; dec = 8.41;
                    break;               
            }
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>() { "Site:GroundTemperature:BuildingSurface," };
            info.AddRange(new List<string>() { string.Join(",", jan, feb, mar, apr, may, jun, jul, aug, sep, oct, nov, dec) + "; ! - Site Ground Temperatures" });
            return info;
        }
    }
}
