using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class Surface
    {
        public string Name, ConstructionName, OutsideCondition, OutsideObject, SunExposed, WindExposed;
        public double Orientation, GrossArea, Area, WWR = 0, ShadingLength = 0;

        public XYZList VerticesList;

        public SurfaceType surfaceType;
        public Direction Direction;
        public List<Fenestration> Fenestrations;
        public List<ShadingOverhang> Shading;
        public double SolarRadiation, HeatFlow;
        public double[] h_SolarRadiation, h_HeatFlow;

        //[NonSerialized]
        //public Zone Zone;
        public string ZoneName;
        public Surface() { }
        private void AddName(string zoneName, int sCount)
        {
            ZoneName = zoneName;
            Name = surfaceType == SurfaceType.Wall  ? ZoneName + ":" + Direction + ":" + surfaceType + ":" + (sCount + 1)
                                                        : ZoneName + ":" + surfaceType + ":" + (sCount + 1);          
        }
        public void CreateWindowsShadingControlShadingOverhang(Zone zone, BuildingWWR wWR, ShadingLength shadingLength)
        {
            switch (Direction)
            {
                case Direction.North:
                    WWR = wWR.North;
                    ShadingLength = shadingLength.north;
                    break;
                case Direction.East:           
                WWR = wWR.East;
                ShadingLength = shadingLength.east;
                    break;
                case Direction.South:
                WWR = wWR.South;
                ShadingLength = shadingLength.south;
                    break;
                case Direction.West:
                    WWR = wWR.West;
                    ShadingLength = shadingLength.west;
                    break;
            }
            CreateFenestration(1);

            if (Fenestrations != null)
            {
                switch (Direction)
                {
                    case Direction.North:
                        Fenestrations.ForEach(f => f.ShadingControl = null);
                        break;
                    case Direction.East:
                    case Direction.South:
                    case Direction.West:
                        string dayLightControlObjectName = zone.DayLightControl == null ? "" : zone.DayLightControl.Name;
                        Fenestrations.ForEach(f => f.ShadingControl = new WindowShadingControl(f, dayLightControlObjectName));
                        break;
                }
            }        
        }
        public void CreateWindows(Zone zone)
        {
            CreateFenestration(1);
            if (Fenestrations != null)
            {
                switch (Direction)
                {
                    case Direction.North:
                        Fenestrations.ForEach(f => f.ShadingControl = null);
                        break;
                    case Direction.East:
                    case Direction.South:
                    case Direction.West:
                        string dayLightControlObjectName = zone.DayLightControl == null ? "" : zone.DayLightControl.Name;
                        Fenestrations.ForEach(f => f.ShadingControl = new WindowShadingControl(f, dayLightControlObjectName));
                        break;
                }
            }
        }
        public Surface(Zone zone, XYZList verticesList, double area, SurfaceType surfaceType)
        {
            Area = area;
            GrossArea = area;
            VerticesList = verticesList;
            this.surfaceType = surfaceType;
            switch (this.surfaceType)
            {
                case (SurfaceType.Floor):
                    ConstructionName = "GroundFloor";
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
                    ConstructionName = "ExternalWall";
                    Orientation = verticesList.GetWallOrientation(out Direction);                    
                    break;
                case (SurfaceType.Ceiling):
                    ConstructionName = "Floor_Ceiling";
                    OutsideCondition = "Adiabatic";
                    SunExposed = "NoSun";
                    WindExposed = "NoWind";
                    break;
                case (SurfaceType.Roof):
                    ConstructionName = "Roof";
                    OutsideObject = "";
                    OutsideCondition = "Outdoors";
                    SunExposed = "SunExposed";
                    WindExposed = "WindExposed";
                    break;
            }
            AddName(zone.Name, zone.Surfaces.Count());
            zone.Surfaces.Add(this);
        }

        public List<string> SurfaceInfo()
        {
            List<string> info = new List<string>();
            info.Add("BuildingSurface:Detailed,");
            info.Add("\t" + Name + ",\t\t!- Name");
            info.Add("\t" + surfaceType + ",\t\t\t\t\t!-Surface Type");
            info.Add("\t" + ConstructionName + ",\t\t\t\t!-Construction Name");
            info.Add("\t" + ZoneName + ",\t\t\t\t\t\t!-Zone Name");
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
            if (fenArea > 0.1)
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
                Fenestrations = fenestrationList;
                Area = GrossArea * (1 - WWR);
            }
            else
            {
                Fenestrations = null;
                WWR = 0;
                Area = GrossArea;
            }           
            
        }
    }
}
