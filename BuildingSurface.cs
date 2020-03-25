using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class BuildingSurface
    {
        public string Name, ConstructionName, OutsideCondition, OutsideObject, SunExposed, WindExposed;
        public double Orientation, GrossArea, Area, WWR = 0, ShadingLength = 0;

        public XYZList VerticesList;
        public Zone Zone;
        public SurfaceType surfaceType;
        public Direction Direction;
        public List<Fenestration> Fenestrations;
        public List<ShadingOverhang> Shading;
        public double SolarRadiation, HeatFlow;
        public double[] p_SolarRadiation, p_HeatFlow;

        public BuildingSurface() { }
        private void AddName()
        {
            Name = Zone.Name + ":" + ConstructionName + ":" + (Zone.Surfaces.Count + 1);
            if (surfaceType == SurfaceType.Wall)
            {
                Name = Zone.Name + ":" + Direction + ":" + ConstructionName + ":" + (Zone.Surfaces.Count + 1);
            }
        }
        internal void CreateWindowsShadingControlShadingOverhang()
        {
            Orientation = VerticesList.GetWallDirection();
            if (Orientation < 45 || Orientation >= 315)
            {
                WWR = Zone.building.WWR.north;
                ShadingLength = Zone.building.shadingLength.north;
                Direction = Direction.North;
            }
            if (Orientation >= 45 && Orientation < 135)
            {
                WWR = Zone.building.WWR.east;
                ShadingLength = Zone.building.shadingLength.east;
                Direction = Direction.East;
            }
            if (Orientation >= 135 && Orientation < 225)
            {
                WWR = Zone.building.WWR.south;
                ShadingLength = Zone.building.shadingLength.south;
                Direction = Direction.South;
            }
            if (Orientation >= 225 && Orientation < 315)
            {
                WWR = Zone.building.WWR.west;
                ShadingLength = Zone.building.shadingLength.west;
                Direction = Direction.West;
            }
            CreateFenestration(1);
            List<WindowShadingControl> shadingControls = new List<WindowShadingControl>();
            
            switch (Direction)
            {
                case Direction.North:
                    Fenestrations.ForEach(f => f.ShadingControl = null);
                    break;
                case Direction.East:
                case Direction.South:
                case Direction.West:
                    Fenestrations.ForEach(f => f.ShadingControl = new WindowShadingControl(f));
                    shadingControls.AddRange(Fenestrations.Select(f => f.ShadingControl));
                    break;
            }           
        }
        public BuildingSurface(Zone zone, XYZList verticesList, double grossArea, SurfaceType surfaceType)
        {
            Zone = zone;
            Area = grossArea;
            GrossArea = grossArea;
            VerticesList = verticesList;
            this.surfaceType = surfaceType;

            switch (this.surfaceType)
            {
                case (SurfaceType.Floor):
                    ConstructionName = "Slab_Floor";
                    OutsideCondition = "Ground";
                    OutsideObject = "";
                    SunExposed = "NoSun";
                    WindExposed = "NoWind";
                    break;
                case (SurfaceType.Wall):
                    OutsideObject = "";
                    OutsideCondition = "Outdoors";
                    SunExposed = "SunExposed";
                    WindExposed = "WindExposed";
                    ConstructionName = "Wall ConcreteBlock";
                    break;
                case (SurfaceType.Ceiling):
                    ConstructionName = "General_Floor_Ceiling";
                    OutsideCondition = "Zone";
                    SunExposed = "NoSun";
                    WindExposed = "NoWind";
                    break;
                case (SurfaceType.Roof):
                    ConstructionName = "Up Roof Concrete";
                    OutsideObject = "";
                    OutsideCondition = "Outdoors";
                    SunExposed = "SunExposed";
                    WindExposed = "WindExposed";
                    break;
            }
            AddName();
            zone.Surfaces.Add(this);
        }

        public List<string> SurfaceInfo()
        {
            List<string> info = new List<string>();
            info.Add("BuildingSurface:Detailed,");
            info.Add("\t" + Name + ",\t\t!- Name");
            info.Add("\t" + surfaceType + ",\t\t\t\t\t!-Surface Type");
            info.Add("\t" + ConstructionName + ",\t\t\t\t!-Construction Name");
            info.Add("\t" + Zone.Name + ",\t\t\t\t\t\t!-Zone Name");
            info.Add("\t" + OutsideCondition + ",\t\t\t\t\t!-Outside Boundary Condition");
            info.Add("\t" + OutsideObject + ",\t\t\t\t\t\t!-Outside Boundary Condition Object");
            info.Add("\t" + SunExposed + ",\t\t\t\t\t\t!-Sun Exposure");
            info.Add("\t" + WindExposed + ",\t\t\t\t\t\t!-Wind Exposure");
            info.Add("\t" + ",\t\t\t\t\t\t!-View Factor to Ground");
            info.AddRange(VerticesList.WriteInfo());
            return info;
        }

        internal void CreateFenestration(int count)
        {
            List<Fenestration> fenestrationList = new List<Fenestration>();
            double fenArea = GrossArea * WWR / count;
            if (fenArea > 0.5)
            {                
                for (int i = 0; i < count; i++)
                {
                    Fenestration fen = new Fenestration(this);
                    XYZ P1 = VerticesList.xyzs.ElementAt(0);
                    XYZ P2 = VerticesList.xyzs.ElementAt(1);
                    XYZ P3 = VerticesList.xyzs.ElementAt(2);
                    XYZ P4 = VerticesList.xyzs.ElementAt(3);
                    double openingFactor = Math.Sqrt(WWR / count);

                    XYZ pMid = new XYZ((P1.X + P3.X) / (count - i + 1), (P1.Y + P3.Y) / (count - i + 1), (P1.Z + P3.Z) / 2);

                    fen.VerticesList = new XYZList(VerticesList.xyzs.Select(v => new XYZ(pMid.X + (v.X - pMid.X) * openingFactor,
                                                                pMid.Y + (v.Y - pMid.Y) * openingFactor,
                                                                pMid.Z + (v.Z - pMid.Z) * openingFactor)).ToList());
                    fen.Area = fenArea;
                    fenestrationList.Add(fen);
                }
            }
            else
            {
                WWR = 0;
            }           
            Fenestrations = fenestrationList;
            Area = GrossArea * (1 - WWR);
        }
    }
}
