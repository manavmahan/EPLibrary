using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class DayLighting
    {
        public string Name;
        public Zone Zone { get; set; }
        public string DLMethod = "SplitFlux";
        public List<DayLightReferencePoint> ReferencePoints = new List<DayLightReferencePoint>();

        public ControlType CType = ControlType.Continuous;
        public double GlareCalcAngle = 180;
        public double DiscomGlare = 22;
        public double MinPower = 0.3;
        public double MinLight = 0.3;
        public int NStep = 3;
        public double ProbabilityManual = 1;
        public string AvailabilitySchedule;
        public double DELightGridResolution = 2;

        public DayLighting() { }
        public List<DayLightReferencePoint> CreateZoneDayLightReferencePoints(Zone zone, List<XYZ> points, double illuminance)
        {
            List<DayLightReferencePoint> dlRefPoints = new List<DayLightReferencePoint>();
            double totalPoints = points.Count();
            double pControlled = Math.Floor(1000 / totalPoints) / 1000;
            points.ForEach(p => dlRefPoints.Add(new DayLightReferencePoint()
            {
                Zone = zone,
                Point = p,
                Name = "Day Light Reference Point " + (points.IndexOf(p) + 1) + " for " + zone.Name,
                Illuminance = illuminance,
                PartControlled = p != points.Last() ? pControlled:1-(points.Count-1)*pControlled
            }));
            return dlRefPoints;
        }
        public DayLighting(Zone zone, string schedule, List<XYZ> points, double illuminance)
        {
            if (points.Count > 0)
            {
                Name = "DayLight Control For " + zone.Name;
                Zone = zone;
                AvailabilitySchedule = schedule;
                ReferencePoints = CreateZoneDayLightReferencePoints(zone, points, illuminance);
                zone.DayLightControl = this;
            }
            else
            {
                zone.DayLightControl = null;
            }
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>()
            {
                "Daylighting:Controls,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(Zone.Name, "Zone Name"),
                Utility.IDFLineFormatter(DLMethod, "Daylighting Method"),
                Utility.IDFLineFormatter(AvailabilitySchedule, "Availability Schedule Name"),
                Utility.IDFLineFormatter(CType, "Lighting control type {1=continuous,2=stepped,3=continuous/off}"),
                Utility.IDFLineFormatter(MinPower, "Minimum input power fraction for continuous dimming control"),
                Utility.IDFLineFormatter(MinLight, "Minimum light output fraction for continuous dimming control"),
                Utility.IDFLineFormatter(NStep, "Number of steps, excluding off, for stepped control"),
                Utility.IDFLineFormatter(ProbabilityManual, "Probability electric lighting will be reset when needed"),
                Utility.IDFLineFormatter(ReferencePoints.Last().Name, "Glare Calculation Reference Point Name"),
                Utility.IDFLineFormatter(GlareCalcAngle, "Azimuth angle of view direction for glare calculation {deg}"),
                Utility.IDFLineFormatter(DiscomGlare, "Maximum discomfort glare index for window shade control"),
                Utility.IDFLineFormatter(DELightGridResolution, "DE Light Gridding Resolution")
            };
            ReferencePoints.ForEach(p => info.AddRange(new List<string>() {
                Utility.IDFLineFormatter(p.Name, "Reference Point"),
                Utility.IDFLineFormatter(p.PartControlled, "Part Controlled"),
                Utility.IDFLineFormatter(p.Illuminance, "Illuminance Setpoint")
            }));
            return info.ReplaceLastComma();
        }
    }
}
