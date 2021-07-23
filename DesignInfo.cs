using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    
    [Serializable]
    public class DesignInfo
    {
        public string Name;
        public DateTime DateTime;

        public string Document;
        public Location Location;
        public Dictionary<string, Building> AllCreatedOptions;
        public Dictionary<string, string> OptionsMassId;
        public Dictionary<string, ProbabilisticBuildingDesignParameters> PParametersDictionary;
        public ProbabilisticBuildingDesignParameters PParameters;

        public int NumSamples, NumSamplesOption; 
        
        public float OffsetDistance;
        
        public RandomGeometry RandomGeometry;
        public List<Building> AllPossibleOptions;
        public LevelOfDevelopment LevelOfDevelopment;
        public Building ProbabisticBuilding;
        public DesignInfo() { }
    }
}
