using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class BuildingGeometry
    {
        public double Length, Width, Height, rLenA, rWidA, BasementDepth,  FloorArea, ARatio, Orientation;
        public int Shape;
        public int NFloors;
        public BuildingGeometry() { }

        public string Header(string sep)
        {
            return string.Join(sep, "Length", "Width", "Height", "rLenA", "rWidA", "Basement Depth", 
                "Orientation", "Floor Area", "Aspect Ratio", "Shape", "Number of Floors");
        }
        public string ToString(string sep)
        {
            return string.Join(sep, Length, Width, Height, rLenA, rWidA, BasementDepth, Orientation, FloorArea, ARatio, Shape, NFloors);
        }
    }
}
