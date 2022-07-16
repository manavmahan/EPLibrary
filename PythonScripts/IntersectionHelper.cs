using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects.PythonScripts
{
    /// <summary>
    /// Must be unique for each building.
    /// </summary>
    public class IntersectionHelper
    {
        private Dictionary<string, string> _intersections = new Dictionary<string, string>();
        
        public IntersectionHelper()
        {
            
        }
        
        private XYZList Intersect(params XYZList[] loops)
        {
            var loopStr = string.Join(":", loops.Select(p => p.ToString(true)));
            if (_intersections.TryGetValue(loopStr, out var result))
            {
                var res = new XYZList(result);
                return res.XYZs != null && res.XYZs.Count > 2 ? res : null;
            }

            string python = "python3";
            string arg = $"{nameof(PythonScripts)}/{nameof(Intersect).ToLower()}.py" + " " + loopStr;
            var output = PythonHelper.ExecuteCommand(string.Join(" ", new[] { python, arg }));
            _intersections.Add(loopStr, output);
            return Intersect(loops);
        }

        public List<XYZList> Intersect(IEnumerable<XYZList> loops, XYZList loop)
        {
            var surfaces = new List<XYZList>();
            foreach (var l in loops)
            {
                var intersect = Intersect(l, loop);
                if (intersect != null)
                    surfaces.Add( intersect );
            }
            return surfaces;
        }
    }
}
