using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class EmbeddedEnergyParameters
    {
        public double th_ExtWall, th_IntWall, th_GFloor, th_IFloor, th_Roof, Reinforcement;
        public EmbeddedEnergyParameters() { }
        public EmbeddedEnergyParameters(double th_ExtWall, double th_IntWall, double th_GFloor, double th_IFloor, double th_Roof, double Reinforcement)
        {
            this.th_ExtWall = th_ExtWall;
            this.th_IntWall = th_IntWall;
            this.th_GFloor = th_GFloor;
            this.th_IFloor = th_IFloor;
            this.th_Roof = th_Roof;
            this.Reinforcement = Reinforcement;
        }
        public List<string> ToCSVString()
        {
            return new List<string>(){
                string.Join(",",  th_ExtWall,  th_IntWall,  th_GFloor,  th_IFloor,  th_Roof, Reinforcement)
            };
        }
        public string Header()
        {
            return "Thickness Ext Wall,Thickness Int Wall,Thickness Gfloor,Thickness Int Floor,Thickness Roof,Reinforcement";
        }
    }
}
