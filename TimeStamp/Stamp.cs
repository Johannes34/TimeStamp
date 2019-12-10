using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace TimeStamp
{

    [Serializable]
    public class Stamp
    {
        public Stamp(int workingHours)
        {
            WorkingHours = workingHours;
            ActivityRecords = new List<ActivityRecord>();
        }

        public Stamp(DateTime day, TimeSpan begin, int workingHours)
            : this(workingHours)
        {
            this.Day = day;
            this.Begin = begin;
        }


        public DateTime Day { get; set; }
        public TimeSpan Begin { get; set; }
        public TimeSpan End { get; set; }
        public TimeSpan Pause { get; set; }
        public string Comment { get; set; }
        public int WorkingHours { get; set; }


        public Stamp Clone()
        {
            using (MemoryStream memory_stream = new MemoryStream())
            {
                // Serialize the object into the memory stream.
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory_stream, this);

                // Rewind the stream and use it to create a new object.
                memory_stream.Position = 0;
                return (Stamp)formatter.Deserialize(memory_stream);
            }
        }

        public List<ActivityRecord> ActivityRecords { get; set; }

        public ActivityRecord GetFirstActivity(string activity = null)
        {
            // from all / from certain activity field:
            IEnumerable<ActivityRecord> records = (activity == null ? ActivityRecords : ActivityRecords.Where(r => r.Activity == activity));

            if (!records.Any())
                return null;

            // with the first start:
            var firstStart = records.OrderBy(r => r.Begin.Value.TotalMinutes).FirstOrDefault();
            return firstStart;
        }

        //public ActivityRecord[] GetActivitiesDuring(TimeSpan from, TimeSpan to, string activity = null)
        //{
        //    // from all / from certain activity field:
        //    IEnumerable<ActivityRecord> records = (activity == null ? ActivityRecords : ActivityRecords.Where(r => r.Activity == activity));

        //    if (!records.Any())
        //        return null;

        //    // with intersecting spans:
        //    var intersecting = records.Where(r => r.begin.Value < to && (!r.end.HasValue || r.end > from)).ToArray();
        //    return intersecting;
        //}

        public ActivityRecord GetLastActivity(string activity = null)
        {
            // from all / from certain activity field:
            IEnumerable<ActivityRecord> records = (activity == null ? ActivityRecords : ActivityRecords.Where(r => r.Activity == activity));

            if (!records.Any())
                return null;

            // preferrably when having open end (currently running):
            var openEnd = records.FirstOrDefault(r => !r.End.HasValue);
            if (openEnd != null)
                return openEnd;

            // otherwise with the latest end:
            var latestEnd = records.OrderByDescending(r => r.End.Value.TotalMinutes).FirstOrDefault();
            return latestEnd;
        }

    }

    [Serializable]
    public class ActivityRecord
    {
        public string Activity { get; set; }

        public TimeSpan? Begin { get; set; }

        public TimeSpan? End { get; set; }

        public string Comment { get; set; }
    }
}
