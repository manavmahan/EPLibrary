﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticBuildingZoneOccupant
    {
        public ProbabilityDistributionFunction AreaPerPerson;
        public string Name = "Building";
        
        public ProbabilisticBuildingZoneOccupant() { }

        public ProbabilisticBuildingZoneOccupant(ProbabilityDistributionFunction AreaPerPerson)
        {
            this.AreaPerPerson = AreaPerPerson;           
        }

        public BuildingZoneOccupant GetAverage()
        {
            return new BuildingZoneOccupant(AreaPerPerson.Mean)
            {
                Name = Name
            };
        }
        public string ToString(string sep)
        {
            return string.Join(sep, AreaPerPerson);
        }
        public string Header(string sep)
        {
            return GetAverage().Header(sep);
        }
        
    }
}
