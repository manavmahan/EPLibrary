using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ProbabilisticBuildingDesignParameters
    {
        public ProbabilisticBuildingGeometry pGeometry;
        public ProbabilisticBuildingConstruction pConstruction;
        public ProbabilisticBuildingWWR pWWR;
        public ProbabilisticBuildingService pService;
        public List<ProbabilisticZoneConditions> zConditions = new List<ProbabilisticZoneConditions>();
        
        public List<BuildingDesignParameters> AllSamples;
        public ProbabilisticBuildingDesignParameters() { }
        public List<ProbabilityDistributionFunction> GetValidPDFs()
        {
            List<ProbabilityDistributionFunction> v = new List<ProbabilityDistributionFunction>();
            v.AddRange(pGeometry.GetValidPDFs());
            v.AddRange(pConstruction.GetValidPDFs());
            v.AddRange(pWWR.GetValidPDFs());
            v.AddRange(pService.GetValidPDFs());
            v.AddRange(zConditions.SelectMany(z => z.GetValidPDFs()));
            return v;
        }
        public BuildingDesignParameters GetAverage()
        {
            return new BuildingDesignParameters()
            {
                Geometry = pGeometry.GetAverage(),
                Construction = pConstruction.GetAverage(),
                WWR = pWWR.GetAverage(),
                Service = pService.GetAverage(),
                ZConditions = zConditions.Select(z => z.GetAverage()).ToList()
            };
        }
        public void GetSamples(List<double[]> sequence)
        {
            AllSamples = new List<BuildingDesignParameters>();
            foreach (double[] sampleS in sequence)
            {
                int nG = pGeometry.GetValidPDFs().Count(),
                    nC = pConstruction.GetValidPDFs().Count(),
                    nW = pWWR.GetValidPDFs().Count(),
                    nS = pService.GetValidPDFs().Count();
                BuildingDesignParameters sample = new BuildingDesignParameters()
                {
                    Geometry = pGeometry.GetSample<ProbabilisticBuildingGeometry, BuildingGeometry>(sampleS.Skip(0).Take(nG).ToArray()),
                    Construction = pConstruction.GetSample<ProbabilisticBuildingConstruction, BuildingConstruction>(sampleS.Skip(nG).Take(nC).ToArray()),
                    WWR = pWWR.GetSample<ProbabilisticBuildingWWR, BuildingWWR>(sampleS.Skip(nG + nC).Take(nW).ToArray()),
                    Service = pService.GetSample<ProbabilisticBuildingService, BuildingService>(sampleS.Skip(nG + nC + nW).Take(nS).ToArray()),
                    ZConditions = new List<ZoneConditions>(),
                };
                int n = nG + nC + nW + nS;

                foreach (ProbabilisticZoneConditions op in zConditions)
                {
                    int n1 = op.GetValidPDFs().Count();
                    ZoneConditions o = op.GetSample<ProbabilisticZoneConditions, ZoneConditions>(sampleS.Skip(n).Take(n1).ToArray());
                    o.Name = op.Name;
                    sample.ZConditions.Add(o);
                    n += n1;
                }
                
                AllSamples.Add(sample);
            }
        }     
        public List<string> ToCSVString()
        {
            List<string[]> rValues = new List<string[]>();
            foreach (var line in pGeometry.Header("\n").Split('\n').Zip(pGeometry.ToString("\n").Split('\n'), (head, data) => new { Head = head, Data = data }))
            {
                rValues.Add(new string[] { line.Head, line.Data });
            }

            foreach (var line in pConstruction.Header("\n").Split('\n').Zip(pConstruction.ToString("\n").Split('\n'), (head, data) => new { Head = head, Data = data }))
            {
                rValues.Add(new string[] { line.Head, line.Data });
            }

            foreach (var line in pWWR.Header("\n").Split('\n').Zip(pWWR.ToString("\n").Split('\n'), (head, data) => new { Head = head, Data = data }))
            {
                rValues.Add(new string[] { line.Head, line.Data });
            }

            foreach (var line in pService.Header("\n").Split('\n').Zip(pService.ToString("\n").Split('\n'), (head, data) => new { Head = head, Data = data }))
            {
                rValues.Add(new string[] { line.Head, line.Data });
            }

            foreach (ProbabilisticZoneConditions zOperation in zConditions)
            {
                foreach (var line in zOperation.Header("\n").Split('\n').Zip(zOperation.ToString("\n").Split('\n'), (head, data) => new { Head = head, Data = data }))
                {
                    rValues.Add(new string[] { line.Head, line.Data });
                }
            }
            List<string> rValue = new List<string>();

            foreach (string[] strs in rValues)
            {
                rValue.Add(string.Join(",", strs));
            }
            return rValue;
        }

        public string Header(string sep)
        {
            return string.Join(sep,
                pGeometry.Header(sep),
                pConstruction.Header(sep),
                pWWR.Header(sep),
                pService.Header(sep),
                string.Join(sep, zConditions.Select(o => o.Header(sep))));
        }
        public List<string> AllSamplesToString()
        {
            List<string> vals = new List<string>() { Header(",") };
            foreach (BuildingDesignParameters pars in AllSamples)
            {
                vals.Add(pars.ToString(","));
            }
            return vals;
        }
    }
}