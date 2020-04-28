using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
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
        public List<ProbabilisticBuildingZoneOperation> zOperations = new List<ProbabilisticBuildingZoneOperation>();
        public List<ProbabilisticBuildingZoneOccupant> zOccupants = new List<ProbabilisticBuildingZoneOccupant>();
        public List<ProbabilisticBuildingZoneEnvironment> zEnvironments = new List<ProbabilisticBuildingZoneEnvironment>();

        public List<BuildingDesignParameters> AllSamples;
        public ProbabilisticBuildingDesignParameters() { }
        public void GetSamples(Random random, int samples)
        {
            AllSamples = new List<BuildingDesignParameters>();
            List<BuildingGeometry> geometryL = pGeometry.GetSamples(random, samples);
            List<BuildingConstruction> constructionL = pConstruction.GetSamples(random, samples);
            List<BuildingWWR> wwrL = pWWR.GetSamples(random, samples);
            List<BuildingService> serviceL = pService.GetSamples(random, samples);
            List<List<BuildingZoneOperation>> operationsL =
                zOperations.Select(z => z.GetSamples(random, samples)).ToList();
            List<List<BuildingZoneOccupant>> occupantsL =
                zOccupants.Select(z => z.GetSamples(random, samples)).ToList();
            List<List<BuildingZoneEnvironment>> environmentsL =
                zEnvironments.Select(z => z.GetSamples(random, samples)).ToList();

            for (int i = 0; i < samples; i++)
            {
                BuildingDesignParameters sample = new BuildingDesignParameters();
                sample.Geometry = geometryL[i];
                sample.Construction = constructionL[i];
                sample.WWR = wwrL[i];
                sample.Service = serviceL[i];

                operationsL.ForEach(os => sample.Operations.Add(os[i]));
                occupantsL.ForEach(os => sample.Occupants.Add(os[i]));
                environmentsL.ForEach(os => sample.Environments.Add(os[i]));

                AllSamples.Add(sample);
            }
        } 
        public List<string> ToString()
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

            foreach (ProbabilisticBuildingZoneOperation zOperation in zOperations)
            {
                foreach (var line in zOperation.Header("\n").Split('\n').Zip(zOperation.ToString("\n").Split('\n'), (head, data) => new { Head = head, Data = data }))
                {
                    rValues.Add(new string[] { line.Head, line.Data });
                }
            }

            foreach (ProbabilisticBuildingZoneOccupant zOccupant in zOccupants)
            {
                foreach (var line in zOccupant.Header("\n").Split('\n')
                    .Zip(zOccupant.ToString("\n").Split('\n'), (head, data) => 
                    new { Head = head, Data = data }))
                {
                    rValues.Add(new string[] { line.Head, line.Data });
                }
            }

            foreach (ProbabilisticBuildingZoneEnvironment zEnvironment in zEnvironments)
            {
                foreach (var line in zEnvironment.Header("\n").Split('\n')
                    .Zip(zEnvironment.ToString("\n").Split('\n'), (head, data) => 
                    new { Head = head, Data = data }))
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
                string.Join(sep, zOperations.Select(o => o.Header(sep))),
                string.Join(sep, zOccupants.Select(o => o.Header(sep))),
                string.Join(sep, zEnvironments.Select(o => o.Header(sep))));
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
