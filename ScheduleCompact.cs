using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDFObjects
{
    [Serializable]
    public class ScheduleCompact
    {
        public ScheduleCompact() { }
        public string name { get; set; }
        public ScheduleLimits scheduleLimits { get; set; }
        public double value { get; set; }
        public Dictionary<string, Dictionary<string, double>> daysTimeValue;

        public ScheduleCompact(string name, List<string> fileData)
        {
            this.name = name;
            List<string> days = fileData.Where(x => x.ToLower().Contains("day") || x.ToLower().Contains("weekends")).ToList();
            daysTimeValue = new Dictionary<string, Dictionary<string, double>>();
            for (int i = 0; i < days.Count; i++)
            {
                int start = fileData.IndexOf(days[i]) + 1;
                int end = 0;
                if (i < days.Count - 1)
                {
                    end = fileData.IndexOf(days[i + 1]);
                }
                else
                {
                    end = fileData.Count;
                }
                Dictionary<string, double> dayValues = new Dictionary<string, double>();
                for (int x = start; x < end; x++)
                {
                    List<string> lineValue = fileData[x].Split(',').ToList();
                    dayValues.Add(lineValue[0], double.Parse(lineValue[1]));
                }
                daysTimeValue.Add(days[i], dayValues);
            }
        }
        public List<string> WriteInfo()
        {
            string sLimitName = scheduleLimits == null ? "" : scheduleLimits.name;

            List<string> info = new List<string>();
            info.Add("Schedule:compact,");
            info.Add(Utility.IDFLineFormatter(name, "Name"));
            info.Add(Utility.IDFLineFormatter(sLimitName, "Schedule Type Limits Name"));
            info.Add(Utility.IDFLineFormatter("Through: 12/31", "Field1"));

            foreach (KeyValuePair<string, Dictionary<string, double>> kV in daysTimeValue)
            {
                info.Add("For: " + kV.Key + ",");
                foreach (KeyValuePair<string, double> tValue in kV.Value)
                {
                    info.Add("Until: " + tValue.Key + "," + tValue.Value + ",");
                }
            }
            info[info.Count - 1] = info.Last().Remove(info.Last().Count() - 1) + ';';
            return info;
        }
    }
}
