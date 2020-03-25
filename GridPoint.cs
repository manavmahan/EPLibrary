using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class GridPoint : IEquatable<GridPoint>
    {
        public double x, y;
        public GridPoint Left, Right, Up, Down;
        public GridPoint(double Coordx, double Coordy)
        {
            x = Coordx;
            y = Coordy;
        }
        public GridPoint()
        { }
        public List<GridPoint> GetAllNeighbours()
        {
            Left = new GridPoint(x - 1, y);
            Down = new GridPoint(x, y - 1);
            Right = new GridPoint(x + 1, y);
            Up = new GridPoint(x, y + 1);
            return new List<GridPoint> { Left, Down, Right, Up };
        }
        public bool Equals(GridPoint point1)
        {
            return (x == point1.x && y == point1.y);
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() * y.GetHashCode();
        }
    }

}
