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
        public ProbabilisticBuildingWWR pWWR;
        public ProbabilisticBuildingService pService;
        public List<ProbabilisticZoneConditions> zConditions = new List<ProbabilisticZoneConditions>();
        
        public ProbabilisticBuildingDesignParameters() { }
        //public Dictionary<string, ProbabilityDistributionFunction> GetValidPDFs()
        //{
        //    Dictionary<string, ProbabilityDistributionFunction> v;
        //    List<Dictionary<string, ProbabilityDistributionFunction>> allPDFs =
        //        new List<Dictionary<string, ProbabilityDistributionFunction>>()
        //        { pGeometry.GetValidPDFs(), pConstruction.GetValidPDFs(), pWWR.GetValidPDFs(),
        //            pService.GetValidPDFs()};
        //    allPDFs.AddRange(zConditions.Select(z => z.GetValidPDFs()));
        //    return allPDFs.SelectMany(dict => dict)
        //                 .ToDictionary(pair => pair.Key, pair => pair.Value);
        //}
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
        //public void GetSamples(Dictionary<string, double[]> sequence)
        //{
        //    AllSamples = new List<BuildingDesignParameters>();
        //    int samples = sequence.Values.First().Count();
        //    for (int i=0; i<samples; i++)
        //    {
        //        BuildingDesignParameters sample = new BuildingDesignParameters()
        //        {
        //            Geometry = pGeometry.GetSample<ProbabilisticBuildingGeometry, BuildingGeometry>(sequence, i),
        //            Construction = pConstruction.GetSample<ProbabilisticBuildingConstruction, BuildingConstruction>(sequence, i),
        //            WWR = pWWR.GetSample<ProbabilisticBuildingWWR, BuildingWWR>(sequence, i),
        //            Service = pService.GetSample<ProbabilisticBuildingService, BuildingService>(sequence, i),
        //            ZConditions = new List<ZoneConditions>(),
        //        };              
        //        foreach (ProbabilisticZoneConditions op in zConditions)
        //        {
        //            ZoneConditions o = op.GetSample<ProbabilisticZoneConditions, ZoneConditions>(sequence, i);
        //            o.Name = op.Name;
        //            sample.ZConditions.Add(o);
        //        }                
        //        AllSamples.Add(sample);
        //    }
        //}     
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


            pConstruction.InternalMass.Sensitivity = dataDict.GetSensitivityValueForParameter("Internal Mass");
            pConstruction.UWall.Sensitivity = dataDict.GetSensitivityValueForParameter("u_Wall");
            pConstruction.UGFloor.Sensitivity = dataDict.GetSensitivityValueForParameter("u_GFloor");
            pConstruction.URoof.Sensitivity = dataDict.GetSensitivityValueForParameter("u_Roof");
            pConstruction.UWindow.Sensitivity = dataDict.GetSensitivityValueForParameter("u_Window");
            pConstruction.GWindow.Sensitivity = dataDict.GetSensitivityValueForParameter("g_Window");
            pConstruction.Infiltration.Sensitivity = dataDict.GetSensitivityValueForParameter("Infiltration");
            pConstruction.Permeability.Sensitivity = dataDict.GetSensitivityValueForParameter("Air Permeability");
            pConstruction.UIFloor.Sensitivity = dataDict.GetSensitivityValueForParameter("u_IFloor");
            pConstruction.UIWall.Sensitivity = dataDict.GetSensitivityValueForParameter("u_IWall");
            pConstruction.HCSlab.Sensitivity = dataDict.GetSensitivityValueForParameter("hc_Slab");

            pWWR.North.Sensitivity = dataDict.GetSensitivityValueForParameter("WWR_North");
            pWWR.East.Sensitivity = dataDict.GetSensitivityValueForParameter("WWR_East");
            pWWR.West.Sensitivity = dataDict.GetSensitivityValueForParameter("WWR_West");
            pWWR.South.Sensitivity = dataDict.GetSensitivityValueForParameter("WWR_South");

            pService.BoilerEfficiency.Sensitivity = dataDict.GetSensitivityValueForParameter("Boiler Efficiency");
            pService.HeatingCOP.Sensitivity = dataDict.GetSensitivityValueForParameter("Heating COP");
            pService.CoolingCOP.Sensitivity = dataDict.GetSensitivityValueForParameter("Cooling COP");

            foreach (string zlN in zoneListNames)
            {
                ProbabilisticZoneConditions z = zConditions.First(zo => zo.Name == zlN);
                z.LHG.Sensitivity = dataDict.GetSensitivityValueForParameter(zlN + ":Light Heat Gain");
                z.EHG.Sensitivity = dataDict.GetSensitivityValueForParameter(zlN + ":Equipment Heat Gain");
                z.StartTime.Sensitivity = dataDict.GetSensitivityValueForParameter(zlN + ":Start Time");
                z.OperatingHours.Sensitivity = dataDict.GetSensitivityValueForParameter(zlN + ":Operating Hours");
                z.AreaPerPerson.Sensitivity = dataDict.GetSensitivityValueForParameter(zlN + ":Area Per Person");
                z.HeatingSetpoint.Sensitivity = dataDict.GetSensitivityValueForParameter(zlN + ":Heating Setpoint");
                z.CoolingSetpoint.Sensitivity = dataDict.GetSensitivityValueForParameter(zlN + ":Cooling Setpoint");
            }
            
        }
    }
}