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
        public SiteGroundTemperature()
        { }
        public List<string> WriteInfo(Location location)
        {
            switch (location)
            {
                case Location.BERLIN_DEU:
                    return new List<string>() { "Site:GroundTemperature:BuildingSurface, 4.94, 2.03, 1.24, 1.93, 5.86, 10.25, 14.39, 17.39, 18.26, 16.86, 13.47, 9.19;" };
                case Location.MUNICH_DEU:
                default:
                    return new List<string>() { "Site:GroundTemperature:BuildingSurface,6.17,5.07,5.33,6.27,9.35,12.12,14.32,15.48,15.20,13.62,11.08,8.41;" };                   
            }
        }
    }
}
