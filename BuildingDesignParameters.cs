using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    public class BuildingDesignParameters
    {
        public double Length, Width, Height, rLenA, rWidA, BasementDepth, Orientation, FloorArea, ARatio;
        public WWR wwr;
        public BuildingConstruction construction;
        public BuildingOperation operation;
        public string Shape;
        public int NFloors;
        public BuildingDesignParameters() { }        
    }
}
