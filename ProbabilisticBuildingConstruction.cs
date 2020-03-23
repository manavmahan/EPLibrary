using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticBuildingConstruction
    {
        //To store the values from samples
        public double[] uWall, uGFloor, uRoof, uIFloor, uIWall, uWindow, gWindow, hcSlab, infiltration;

        public ProbabilisticBuildingConstruction() { }
        public ProbabilisticBuildingConstruction(double[] uWall, double[] uGFloor, double[] uRoof, double[] uIFloor, double[] uIWall, double[] uWindow, double[] gWindow, double[] HCSlab)
        {
            this.uWall = uWall;
            this.uGFloor = uGFloor;
            this.uRoof = uRoof;
            this.uIFloor = uIFloor;
            this.uIWall = uIWall;
            this.uWindow = uWindow;
            this.gWindow = gWindow;
            this.hcSlab = HCSlab;
        }
        public BuildingConstruction GetAverage()
        {
            return new BuildingConstruction()
            {
                uWall = uWall.Average(),
                uGFloor = uGFloor.Average(),
                uRoof = uRoof.Average(),
                uIFloor = uIFloor.Average(),
                uIWall = uIWall.Average(),
                uWindow = uWindow.Average(),
                gWindow = gWindow.Average(),
                hcSlab = hcSlab.Average(),
                infiltration = infiltration.Average()

            };
        }
        public List<string> ToCSVString()
        {
            return new List<string>(){
                string.Join(",", uWall[0], uGFloor[0], uRoof[0], uIFloor[0], uIWall[0], uWindow[0], gWindow[0], hcSlab[0], infiltration[0]),
                string.Join(",", uWall[1], uGFloor[1], uRoof[1], uIFloor[1], uIWall[1], uWindow[1], gWindow[1], hcSlab[1], infiltration[1])
            };
        }
    }
}
