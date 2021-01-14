using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        public ProbabilisticBuildingWWR pWWR ;
        public ProbabilisticBuildingService pService ;
        public List<ProbabilisticZoneConditions> zConditions =  new List<ProbabilisticZoneConditions>();       
        public ProbabilisticBuildingDesignParameters() { }
        public BuildingDesignParameters GetAverage()
        {
            return new BuildingDesignParameters()
            {
                Geometry = pGeometry==null?null: pGeometry.GetAverage(),
                Construction = pConstruction.GetAverage(),
                WWR = pWWR.GetAverage(),
                Service = pService.GetAverage(),
                ZConditions = zConditions.Select(z => z.GetAverage()).ToList()
            };
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
        public List<string> AllSamplesToString(List<BuildingDesignParameters> AllSamples)
        {
            List<string> vals = new List<string>() { Header(",") };
            foreach (BuildingDesignParameters pars in AllSamples)
            {
                vals.Add(pars.ToString(","));
            }
            return vals;
        }
        public void UpdateSenstivityResults(string sensData)
        {
            Dictionary<string, double[]> dataDict = Utility.ConvertToDataframe(File.ReadAllLines(sensData).Where(s => s.First() != '#'));

            List<string> parameters = dataDict.Keys.ToList();
            List<string> zoneListNames = parameters.Where(p => p.Contains(':'))
                .Select(p => p.Split(':')[0]).Distinct().ToList();

            typeof(ProbabilisticBuildingGeometry).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().ForEach(a =>
            {
                ProbabilityDistributionFunction pdf = a.GetValue(pGeometry) as ProbabilityDistributionFunction;
                pdf.Sensitivity = dataDict.GetSensitivityValueForParameter(pdf.Label);
            });

            typeof(ProbabilisticBuildingConstruction).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().ForEach(a =>
            {
                ProbabilityDistributionFunction pdf = a.GetValue(pConstruction) as ProbabilityDistributionFunction;
                pdf.Sensitivity = dataDict.GetSensitivityValueForParameter(pdf.Label);
            });

            typeof(ProbabilisticBuildingWWR).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().ForEach(a =>
            {
                ProbabilityDistributionFunction pdf = a.GetValue(pWWR) as ProbabilityDistributionFunction;
                pdf.Sensitivity = dataDict.GetSensitivityValueForParameter(pdf.Label);
            });

            typeof(ProbabilisticBuildingService).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().ForEach(a =>
            {
                ProbabilityDistributionFunction pdf = a.GetValue(pService) as ProbabilityDistributionFunction;
                pdf.Sensitivity = dataDict.GetSensitivityValueForParameter(pdf.Label);
            });

            foreach (string zlN in zoneListNames)
            {
                ProbabilisticZoneConditions z = zConditions.First(zo => zo.Name == zlN);

                typeof(ProbabilisticZoneConditions).GetFields().Where(a => a.FieldType == typeof(ProbabilityDistributionFunction)).ToList().ForEach(a =>
                {
                    ProbabilityDistributionFunction pdf = a.GetValue(z) as ProbabilityDistributionFunction;
                    pdf.Sensitivity = dataDict.GetSensitivityValueForParameter($"{z.Name}:{pdf.Label}");
                }); 
            }
            
        }
    }
}