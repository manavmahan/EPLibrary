using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticBuildingGeometry
    {
        public ProbabilityDistributionFunction 
            Length = new ProbabilityDistributionFunction("Length", "metres"),
            Width = new ProbabilityDistributionFunction("Width", "metres"), 
            Height = new ProbabilityDistributionFunction("Height", "metres"), 
            rLenA = new ProbabilityDistributionFunction("rLenA",""), 
            rWidA = new ProbabilityDistributionFunction("rWidA",""), 
            BasementDepth = new ProbabilityDistributionFunction("Basement Depth", "metres"), 
            Orientation = new ProbabilityDistributionFunction("Orientation","degrees"), 
            FloorArea = new ProbabilityDistributionFunction("Floor Area", "sq. metres"), 
            ARatio = new ProbabilityDistributionFunction("Aspect Ratio",""), 
            Shape = new ProbabilityDistributionFunction("Shape",""), 
            NFloors = new ProbabilityDistributionFunction("Number of Floors","");

        public ProbabilisticBuildingGeometry() { }
        public BuildingGeometry GetAverage()
        {
            return new BuildingGeometry()
            {
                Length = Length.Mean,
                Width = Width.Mean,
                Height = Height.Mean,
                rLenA = rLenA.Mean,
                rWidA = rWidA.Mean,
                BasementDepth = BasementDepth.Mean,
                Orientation = (int)Orientation.Mean,
                FloorArea = FloorArea.Mean,
                ARatio = ARatio.Mean,
                Shape = (int)Shape.Mean,
                NFloors = (int)NFloors.Mean
            };
        }
        public string Header(string sep)
        {
            return GetAverage().Header(sep);
        }
        public string ToString(string sep)
        {
            return string.Join(sep, Length, Width, Height, rLenA, rWidA, BasementDepth, 
                Orientation, FloorArea, ARatio, Shape, NFloors);
        }
        
    }
}
