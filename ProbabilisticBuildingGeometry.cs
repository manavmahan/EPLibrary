using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDFObjects
{
    public class ProbabilisticBuildingGeometry
    {
        public ProbabilityDistributionFunction 
            Length = new ProbabilityDistributionFunction(),
            Width = new ProbabilityDistributionFunction(), 
            Height = new ProbabilityDistributionFunction(), 
            rLenA = new ProbabilityDistributionFunction(), 
            rWidA = new ProbabilityDistributionFunction(), 
            BasementDepth = new ProbabilityDistributionFunction(), 
            Orientation = new ProbabilityDistributionFunction(), 
            FloorArea = new ProbabilityDistributionFunction(), 
            ARatio = new ProbabilityDistributionFunction(), 
            Shape = new ProbabilityDistributionFunction(), 
            NFloors = new ProbabilityDistributionFunction();

        public ProbabilisticBuildingGeometry() { }
        public ProbabilisticBuildingGeometry(ProbabilityDistributionFunction Length,
            ProbabilityDistributionFunction Width, ProbabilityDistributionFunction Height,
            ProbabilityDistributionFunction rLenA, ProbabilityDistributionFunction rWidA,
            ProbabilityDistributionFunction BasementDepth, ProbabilityDistributionFunction Orientation,
            ProbabilityDistributionFunction FloorArea, ProbabilityDistributionFunction ARatio,
            ProbabilityDistributionFunction Shape, ProbabilityDistributionFunction NFloors)
        { 
            this.Length = Length; this.Width = Width; this.Height = Height; this.rLenA = rLenA; this.rWidA = rWidA;
            this.BasementDepth = BasementDepth; this.Orientation = Orientation; this.FloorArea = FloorArea; this.ARatio = ARatio;
            this.Shape = Shape; this.NFloors = NFloors;
        }
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
                Orientation = Orientation.Mean,
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
        public List<BuildingGeometry> GetSamples(Random random, int samples)
        {
            List<BuildingGeometry> vals = new List<BuildingGeometry>();

            new List<ProbabilityDistributionFunction>()
            {
                Length, Width, Height, rLenA, rWidA, BasementDepth,
                Orientation, FloorArea, ARatio, Shape, NFloors
            }
            .Select(p => p.GetLHSSamples(random, samples))
            .ZipAll(v => vals.Add(new BuildingGeometry() {
                Length = v.ElementAt(0),
                Width = v.ElementAt(1),
                Height = v.ElementAt(2),
                rLenA = v.ElementAt(3),
                rWidA = v.ElementAt(4),
                BasementDepth = v.ElementAt(5),
                Orientation = v.ElementAt(6),
                FloorArea = v.ElementAt(7),
                ARatio = v.ElementAt(8),
                Shape = (int) v.ElementAt(9),
                NFloors = (int) v.ElementAt(10)
            }));
            return vals;
        }
    }
}
