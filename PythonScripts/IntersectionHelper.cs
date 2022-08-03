using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private string _buildingName;
        public IntersectionHelper(string buildingName)
        {
            _buildingName = buildingName;
        }
        
        private XYZList Intersect(params XYZList[] loops)
        {
            var loopStr = string.Join(":", loops.Select(p => p.ToString(true)));
            if (_intersections.TryGetValue(loopStr, out var result))
            {
                //Console.WriteLine(_intersections.Keys.ToList().IndexOf(loopStr));
                //Console.WriteLine(result);
                //Console.WriteLine(loopStr);
                var res = new XYZList(result);
                //Console.WriteLine(res);
                //Console.WriteLine();

                return res.XYZs != null && res.XYZs.Count > 2 ? res : null;
            }

            var args = string.Join(" ", new[] { loopStr });
            AddFileName(ref args, $"{_buildingName}_{_intersections.Count}");

            var output = PythonHelper.ExecuteCommand(nameof(Intersect).ToLower(), args).Item1;
            _intersections.Add(loopStr, output);
            return Intersect(loops);
        }

        [Conditional("DEBUG")]
        private void AddFileName(ref string args, string fileName)
        {
            args += " " + fileName;
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
