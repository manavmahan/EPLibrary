﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ElectricEquipment
    {
        public ElectricEquipment() { }
        public string Name, ZoneName, scheduleName;

        public string designLevelCalcMeth { get; set; }
        //public float lightingLevel { get; set; }
        public float wattsPerArea { get; set; }

        public float fractionLatent { get; set; }
        public float fractionRadiant { get; set; }
        public float fractionLost { get; set; }

        public ElectricEquipment(float wPA)
        {
            wattsPerArea = wPA;
            designLevelCalcMeth = "Watts/area";
            scheduleName = "Electric Equipment and Lighting Schedule";
            fractionRadiant = 0.1f;
        }
        public List<string> WriteInfo()
        {
            return new List<string>()
            {
                "ElectricEquipment,",
                Utility.IDFLineFormatter(Name, "Name"),
                Utility.IDFLineFormatter(ZoneName, "Zone or ZoneList Name"),
                Utility.IDFLineFormatter(scheduleName, "Schedule Name"),
                Utility.IDFLineFormatter(designLevelCalcMeth, "Design Level Calculation Method"),
                Utility.IDFLineFormatter("", "Design Level {W}"),
                Utility.IDFLineFormatter(wattsPerArea + "", "Watts per Zone Floor Area {W/m2}"),
                Utility.IDFLineFormatter("", "Watts per Person {W/person}"),
                Utility.IDFLineFormatter("", "Fraction Latent"),
                Utility.IDFLineFormatter(fractionRadiant, "Fraction Radiant"),
                Utility.IDFLastLineFormatter("", "Fraction Lost")
            };
        }

    }
}
