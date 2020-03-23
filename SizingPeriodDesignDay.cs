﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class SizingPeriodDesignDay
    {
        public SizingPeriodDesignDay() { }
        public string name { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public string dayType { get; set; }

        public double maxDryBulbT { get; set; }
        public double dailyDryBulbTR { get; set; }
        public string dryBulbTRModifierType { get; set; }

        public string humidityConditionType { get; set; }
        public double wetbulbOrDawPointAtMaxDryBulb { get; set; }

        public double enthalpyAtMaxDryBulb { get; set; }

        public double baromPress { get; set; }
        public double windspeed { get; set; }
        public double windDir { get; set; }

        public string rainInd { get; set; }
        public string snowInd { get; set; }
        public string daylightSavTimeInd { get; set; }
        public string solarModelInd { get; set; }

        public double skyClearness { get; set; }

        public SizingPeriodDesignDay(string name, int month, int day, string dayType, double maxDryBulbT, double dailyDryBulbTR, double wetbulbOrDawPointAtMaxDryBulb, double enthalpyAtMaxDryBulb, double baromPress,
            double windspeed, double windDir, string rainInd, string snowInd, string daylightSavTimeInd, string solarModelInd, double skyClearness)
        {
            this.name = name;
            this.month = month;
            this.day = day;
            this.dayType = dayType;
            this.maxDryBulbT = maxDryBulbT;
            this.dailyDryBulbTR = dailyDryBulbTR;
            dryBulbTRModifierType = "DefaultMultipliers";
            humidityConditionType = "DewPoint";
            this.wetbulbOrDawPointAtMaxDryBulb = wetbulbOrDawPointAtMaxDryBulb;
            this.enthalpyAtMaxDryBulb = enthalpyAtMaxDryBulb;
            this.baromPress = baromPress;
            this.windspeed = windspeed;
            this.windDir = windDir;
            this.rainInd = rainInd;
            this.snowInd = snowInd;
            this.daylightSavTimeInd = daylightSavTimeInd;
            this.solarModelInd = solarModelInd;
            this.skyClearness = skyClearness;
        }


        public List<String> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nSizingPeriod:DesignDay,");
            info.Add("\t" + name + ",\t\t\t\t!-Name");
            info.Add("\t" + month + ", \t\t\t\t!-Month");
            info.Add("\t" + day + ",\t\t\t\t!-Day of Month");
            info.Add("\t" + dayType + ", \t\t\t\t!-Day Type");
            info.Add("\t" + maxDryBulbT + ", \t\t\t\t!-Maximum Dry-Bulb Temperature {C}");
            info.Add("\t" + dailyDryBulbTR + ", \t\t\t\t!-Daily Dry-Bulb Temperature Range {deltaC}");
            info.Add("\t" + dryBulbTRModifierType + ",\t\t\t\t!-Dry-Bulb Temperature Range Modifier Type");
            info.Add(",\t\t\t\t!-Dry-Bulb Temperature Range Modifier Day Schedule Name");
            info.Add("\t" + humidityConditionType + ",\t\t\t\t!-Humidity Condition Type");
            info.Add("\t" + wetbulbOrDawPointAtMaxDryBulb + ",\t\t\t\t!-Wetbulb or DewPoint at Maximum Dry-Bulb {C}");
            info.Add(",\t\t\t\t!-Humidity Condition Day Schedule Name");
            info.Add(",\t\t\t\t!-Humidity Ratio at Maximum Dry-Bulb {kgWater/kgDryAir}");
            info.Add(enthalpyAtMaxDryBulb + ",\t\t\t\t!-Enthalpy at Maximum Dry-Bulb {J/kg}");
            info.Add(",\t\t\t\t!-Daily Wet-Bulb Temperature Range {deltaC}");
            info.Add("\t" + baromPress + ",\t\t\t\t!-Barometric Pressure {Pa}");
            info.Add("\t" + windspeed + ",\t\t\t\t!-Wind Speed {m/s}");
            info.Add("\t" + windDir + ",\t\t\t\t!-Wind Direction {deg}");
            info.Add("\t" + rainInd + ",\t\t\t\t!-Rain Indicator");
            info.Add("\t" + snowInd + ",\t\t\t\t!-Snow Indicator");
            info.Add("\t" + daylightSavTimeInd + ",\t\t\t\t!-Daylight Saving Time Indicator");
            info.Add("\t" + solarModelInd + ",\t\t\t\t!-Solar Model Indicator");
            info.Add(",\t\t\t\t!-Beam Solar Day Schedule Name");
            info.Add(",\t\t\t\t!-Diffuse Solar Day Schedule Name");
            info.Add(",\t\t\t\t!-ASHRAE Clear Sky Optical Depth for Beam Irradiance (taub) {dimensionless}");
            info.Add(",\t\t\t\t!-ASHRAE Clear Sky Optical Depth for Diffuse Irradiance (taud) {dimensionless}");
            info.Add("\t" + skyClearness + ";\t\t\t\t!-Sky Clearness");

            return info;
        }

    }
}
