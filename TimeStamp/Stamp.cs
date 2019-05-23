using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TimeStamp
{

    [Serializable]
    public class Stamp
    {
        public static int DefaultWorkingHours = 8;

        public Stamp()
        {
            WorkingHours = DefaultWorkingHours;
            ActivityRecords = new List<ActivityRecord>();
        }


        public Stamp(DateTime day, TimeSpan begin)
            : this()
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


        public void SetBegin(TimeSpan value)
        {
            if (value < Begin)
            {
                // also move first activity to the correct earlier begin time
                var activity = GetFirstActivity();
                if (activity != null)
                    activity.Begin = value;
            }
            else if (value > Begin)
            {
                // cut off earlier activities and/or set the earliest activity to the correct later begin time
                var activities = ActivityRecords.OrderBy(r => r.Begin.Value).ToArray();
                foreach (var act in activities)
                {
                    // starts before 'new check-in' and ends before 'new check-in': remove activity completely:
                    if (act.Begin.Value < value && act.End.HasValue && act.End.Value < value)
                        ActivityRecords.Remove(act);
                    // either starts after, ends after 'new check-in', or has open end: set new begin time for activity and stop iteration:
                    else
                    {
                        act.Begin = value;
                        break;
                    }
                }
            }
            Begin = value;
        }

        public void SetEnd(TimeSpan value)
        {
            if (End != default(TimeSpan) && value > End)
            {
                // also move latest activity to the correct later end time
                var activity = GetLastActivity();
                if (activity != null)
                    activity.End = value;
            }
            else if (End == default(TimeSpan) || value < End)
            {
                // cutt off later activities and/or set the latest activity to the correct earlier end time
                var activities = ActivityRecords.OrderByDescending(r => r.Begin.Value).ToArray();
                foreach (var act in activities)
                {
                    // starts after 'new check-out': remove activity completely:
                    if (act.Begin.Value >= value)
                        ActivityRecords.Remove(act);
                    // starts before: set new end time for activity and stop iteration:
                    else
                    {
                        act.End = value;
                        break;
                    }
                }
            }
            End = value;
        }

        public void SetPause(TimeSpan value)
        {
            if (ActivityRecords.Any())
            {
                // pause is only a number in minutes, there is not necessarily a 'start' / 'end' time available.
                // if pause has been recognized automatically, the function will cut off the activity before the pause and continues it after the pause, resulting in an activity 'gap', which indeed would be the pause start/end times.

                // find activity gap, created by automatic pause recognition function:
                // get the latest activity before pause, that is, the activity with an end time and no other activity with a matching start time (leaving the gap afterwards):
                // this may also be the last activity of the day, if the day has been finished, and no other pause gap has been found
                var activityBeforePause = ActivityRecords.FirstOrDefault(a => a.End.HasValue && !ActivityRecords.Any(aa => Math.Round(aa.Begin.Value.TotalMinutes) == Math.Round(a.End.Value.TotalMinutes)));

                TimeSpan timeDiff = (value - Pause);

                if (activityBeforePause != null)
                {
                    bool canDistribute = true;
                    do
                    {
                        canDistribute = true;
                        activityBeforePause.End = activityBeforePause.End.Value - timeDiff;
                        // should not result in negative activity time -> remove completely and apply change to previous activity
                        if (activityBeforePause.Total < TimeSpan.Zero)
                        {
                            timeDiff = TimeSpan.Zero - activityBeforePause.Total;
                            var previousActivity = ActivityRecords.FirstOrDefault(a => a.End.HasValue && a.End.Value == activityBeforePause.Begin.Value);
                            if (previousActivity != null) // can this ever be null? (except for having an unreasonable long break of 4 hours...)
                            {
                                ActivityRecords.Remove(activityBeforePause);
                                activityBeforePause = previousActivity;
                                canDistribute = false;
                            }
                        }
                    } while (!canDistribute);
                }
                else
                {
                    // however, this may also be null, if the day is not yet finished (last is open-end) and the pause has not been recognized. in this case, just modify the first.
                    var starting = ActivityRecords.FirstOrDefault();

                    bool canDistribute = true;
                    do
                    {
                        canDistribute = true;
                        starting.Begin = starting.Begin.Value + timeDiff; // should expand in the other direction

                        // should not result in negative activity time -> remove completely and apply change to next activity
                        if (starting.End.HasValue && starting.Total < TimeSpan.Zero)
                        {
                            timeDiff = TimeSpan.Zero - starting.Total;
                            var nextActivity = ActivityRecords.FirstOrDefault(a => a.Begin.Value == starting.End.Value);
                            if (nextActivity != null) // can this ever be null?
                            {
                                ActivityRecords.Remove(starting);
                                starting = nextActivity;
                                canDistribute = false;
                            }
                        }
                    } while (!canDistribute);
                }
            }
            Pause = value;
        }

        public void CalculatePauseFromActivities()
        {
            if (End != TimeSpan.Zero)
            {
                var workDuration = End - Begin;
                var activityDuration = TimeSpan.FromMinutes(ActivityRecords.Sum(a => a.Total.TotalMinutes));
                Pause = workDuration > activityDuration ? workDuration - activityDuration : TimeSpan.Zero;
            }
        }


        public TimeSpan DayBalance => DayTime.Subtract(TimeSpan.FromHours(WorkingHours));

        public TimeSpan DayTime
        {
            get
            {
                if (Day == DateTime.Today && End == TimeSpan.Zero)
                    return Form1.GetNowTime().Subtract(Begin).Subtract(Pause);
                else
                    return End.Subtract(Begin).Subtract(Pause);
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

    public class ActivityRecord
    {
        public string Activity { get; set; }

        public TimeSpan? Begin { get; set; }

        public TimeSpan? End { get; set; }

        public TimeSpan Total
        {
            get
            {
                if (!Begin.HasValue)
                    return TimeSpan.Zero;

                if (!End.HasValue) // assuming this can only happen if it is the today's stamp and not yet checked out...
                    return Form1.GetNowTime() - Begin.Value;

                return End.Value - Begin.Value;
            }
        }

        public string Comment { get; set; }
    }
}
