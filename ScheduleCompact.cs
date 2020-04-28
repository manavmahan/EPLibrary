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
        public string scheduleLimitName { get; set; }
        public double value { get; set; }
        public Dictionary<string, Dictionary<string, double>> daysTimeValue;

        public List<string> allData = new List<string>();

        public ScheduleCompact(string name, List<string> FileData)
        {
            this.name = name;
            allData = FileData.Where(str => str[0] != '#').ToList();                       
        }
        public List<string> WriteInfo()
        {
            List<string> info = new List<string>();
            info.Add("\nSchedule:compact,");
            info.Add(Utility.IDFLineFormatter(name, "Name"));
            info.Add(Utility.IDFLineFormatter(scheduleLimitName, "Schedule Type Limits Name"));
            if (daysTimeValue!=null)
            {
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
            }
            else
            {
                info.AddRange(allData);
            }
            return info;
        }
    }
}
