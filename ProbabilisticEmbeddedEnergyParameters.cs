using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticEmbeddedEnergyParameters
    {
        public float[] th_ExtWall, th_IntWall, th_GFloor, th_IFloor, th_Roof, Reinforcement;

        public ProbabilisticEmbeddedEnergyParameters() { }

        public ProbabilisticEmbeddedEnergyParameters(float[] th_ExtWall, float[] th_IntWall, float[] th_GFloor, float[] th_IFloor, float[] th_Roof, float[] Reinforcement)
        {
            this.th_ExtWall = th_ExtWall;
            this.th_IntWall = th_IntWall;
            this.th_GFloor = th_GFloor;
            this.th_IFloor = th_IFloor;
            this.th_Roof = th_Roof;
            this.Reinforcement = Reinforcement;
        }
        public EmbeddedEnergyParameters GetAverage()
        {
            return new EmbeddedEnergyParameters(th_ExtWall.Average(), th_IntWall.Average(), th_GFloor.Average(), th_IFloor.Average(), th_Roof.Average(), Reinforcement.Average());
        }
        public List<string> ToCSVString()
        {
            return new List<string>(){
                string.Join(",", th_ExtWall[0], th_IntWall[0], th_GFloor[0], th_IFloor[0], th_Roof[0], Reinforcement[0]),
                string.Join(",", th_ExtWall[1], th_IntWall[1], th_GFloor[1], th_IFloor[1], th_Roof[0], Reinforcement[1])
            };
        }
    }
}
