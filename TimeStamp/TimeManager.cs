using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TimeStamp
{
    public class TimeManager
    {
        public TimeManager(TimeSettings settings)
        {
            Settings = settings;
        }

        public TimeSettings Settings { get; private set; }

        // Data:

        public string StampFilePath { get; } = ".\\StampFile.xml";

        public List<Stamp> StampList { get; set; }
        public Stamp CurrentShown { get; set; }


        // Today Data:

        public Stamp Today { get; set; }

        public void SetToday()
        {
            // this can happen when:
            // starting the app (e.g. new day autostart, same day computer restart)
            // resuming from sleep (e.g. new day resume, same day resume)

            // find existing stamp for today:

            var existing = StampList.SingleOrDefault(t => t.Day == Time.Today);

            if (existing != null)
            {
                // TODO: (optionally) ask in a notification, whether have been working or not since last known stamp (default yes, if choosing no will automatically insert a pause)...

                Today = CurrentShown = existing;
                Today.End = TimeSpan.Zero;

                // assuming the last activity is still valid, and the downtime is not considered a break:
                // if there is no running activity, try to restore the last activity
                if (TodayCurrentActivity == null)
                {
                    var openEnd = existing.ActivityRecords.FirstOrDefault(r => !r.End.HasValue);
                    if (openEnd != null)
                        TodayCurrentActivity = openEnd;
                    else
                    {
                        var last = existing.GetLastActivity();
                        existing.ActivityRecords.Add(new ActivityRecord() { Activity = last.Activity, Begin = last.End.Value, End = GetNowTime(), Comment = "logged off time" });
                        StartNewActivity(last.Activity, null);
                    }
                }

                CurrentActivityUpdated();
                PopupDialog.ShowCurrentlyTrackingActivity(TodayCurrentActivity.Activity);
                return;
            }

            // new day, new stamp:

            TodayCurrentActivity = null;
            Today = CurrentShown = new Stamp(Time.Today, GetNowTime());
            StampList.Add(Today);

            // not specified ? -> keep tracking for latest activity...
            if (String.IsNullOrEmpty(Settings.AlwaysStartNewDayWithActivity))
            {
                foreach (var day in StampList.OrderByDescending(s => s.Day))
                {
                    if (!day.ActivityRecords.Any())
                        continue;
                    StartNewActivity(day.GetLastActivity().Activity, null);
                    break;
                }
            }

            // if not yet set -> start tracking either default activity or just the first activity...
            if (TodayCurrentActivity == null)
                StartNewActivity(Settings.AlwaysStartNewDayWithActivity ?? Settings.TrackedActivities.ElementAt(0), null);

            PopupDialog.ShowCurrentlyTrackingActivity(TodayCurrentActivity.Activity);
        }

        public ActivityRecord TodayCurrentActivity { get; set; }

        public event Action CurrentActivityUpdated = delegate { };

        public void StartNewActivity(string name, string comment, bool autoMatchLastComment = false)
        {
            // finish current activity:
            if (TodayCurrentActivity != null)
            {
                TodayCurrentActivity.End = GetNowTime();
            }

            // if no comment provided, automatically apply last comment:
            if (String.IsNullOrEmpty(comment) && autoMatchLastComment)
                comment = Today.ActivityRecords.Where(r => r.Activity == name && !String.IsNullOrEmpty(r.Comment)).LastOrDefault()?.Comment;

            // start new activity:
            TodayCurrentActivity = new ActivityRecord() { Activity = name, Begin = GetNowTime(), Comment = comment };
            Today.ActivityRecords.Add(TodayCurrentActivity);

            CurrentActivityUpdated();
        }

        public void FinishActivity(TimeSpan? explicitEndTime)
        {
            if (TodayCurrentActivity != null)
            {
                TodayCurrentActivity.End = explicitEndTime ?? GetNowTime();
                TodayCurrentActivity = null;
            }

            var unfinishedActivities = Today.ActivityRecords.Where(r => !r.End.HasValue);
            if (unfinishedActivities.Count() > 0)
                throw new ArgumentOutOfRangeException($"There are {unfinishedActivities.Count()} simultaneously running activies: {String.Join(", ", unfinishedActivities.Select(a => a.Activity))}");
        }


        // Methods:

        public void Initialize()
        {
            try
            {
                FileStream fs = new FileStream(StampFilePath, FileMode.Open);
                StampList = LoadStampListXml(XElement.Load(fs));
                fs.Close();
            }
            catch (FileNotFoundException)
            {
                StampList = new List<Stamp>();
            }

            foreach (var Stamp in StampList.ToArray())
                if (Stamp == null)
                    StampList.Remove(Stamp);

            // set todays stamp:
            var todayEntries = StampList.Where(t => t.Day == Time.Today);

            if (todayEntries.Count() > 1)
                throw new IndexOutOfRangeException("Several Todays in StampList found!");

            SetToday();

            if (Today == null)
                throw new InvalidDataException("Today Stamp is null");
        }


        #region XML-IO

        private void SaveStampListXml()
        {
            if (Settings.IgnoreStampFile)
                return;

            FileStream fs;
            if (!File.Exists(StampFilePath))
                fs = new FileStream(StampFilePath, FileMode.CreateNew);
            else
                fs = new FileStream(StampFilePath, FileMode.Truncate);

            bool success = false;
            while (!success)
            {
                try
                {
                    GetStampListXml().Save(fs);
                    fs.Close();
                    success = true;
                }
                catch
                { }
            }
        }

        private XElement GetStampListXml()
        {
            var rootXml = new XElement("ArrayOfStamp");

            foreach (var stamp in StampList)
            {
                var stampXml = new XElement("Stamp");

                stampXml.Add(new XElement("day", stamp.Day));
                stampXml.Add(new XElement("begin", SerializeHHMM(stamp.Begin)));
                stampXml.Add(new XElement("pause", SerializeMM(stamp.Pause)));
                stampXml.Add(new XElement("end", SerializeHHMM(stamp.End)));
                if (!String.IsNullOrEmpty(stamp.Comment))
                    stampXml.Add(new XElement("comment", stamp.Comment));
                if (stamp.WorkingHours != Stamp.DefaultWorkingHours)
                    stampXml.Add(new XElement("hours", stamp.WorkingHours));

                if (stamp.ActivityRecords.Count > 0)
                {
                    var activityRoot = new XElement("Activities");
                    stampXml.Add(activityRoot);
                    foreach (var activity in stamp.ActivityRecords)
                    {
                        activityRoot.Add(new XElement("Activity",
                            new XAttribute("Name", activity.Activity ?? String.Empty),
                            new XAttribute("Begin", SerializeHHMM(activity.Begin)),
                            new XAttribute("End", SerializeHHMM(activity.End)),
                            new XAttribute("Comment", activity.Comment ?? String.Empty)));
                    }
                }
                rootXml.Add(stampXml);
            }

            return rootXml;
        }

        private string SerializeHHMM(TimeSpan? time)
        {
            if (!time.HasValue)
                return String.Empty;
            return time.Value.Hours + ":" + time.Value.Minutes;
        }

        private TimeSpan SerializeHHMM(string time)
        {
            return new TimeSpan(Convert.ToInt32(time.Substring(0, time.IndexOf(":"))), Convert.ToInt32(time.Substring(time.IndexOf(":") + 1)), 0);
        }

        private string SerializeMM(TimeSpan? time)
        {
            if (!time.HasValue)
                return String.Empty;
            return ((int)time.Value.TotalMinutes).ToString();
        }

        private TimeSpan SerializeMM(string time)
        {
            return new TimeSpan(0, Convert.ToInt32(time), 0);
        }

        private List<Stamp> LoadStampListXml(XElement xml)
        {
            if (Settings.IgnoreStampFile)
                return new List<Stamp>();

            List<Stamp> stamps = new List<Stamp>();
            foreach (var stampXml in xml.Elements("Stamp"))
            {
                var stamp = new Stamp()
                {
                    Day = Convert.ToDateTime(stampXml.Element("day").Value),
                    Begin = SerializeHHMM(stampXml.Element("begin").Value),
                    End = SerializeHHMM(stampXml.Element("end").Value),
                    Pause = SerializeMM(stampXml.Element("pause").Value),
                    Comment = stampXml.Element("comment") != null ? stampXml.Element("comment").Value : String.Empty,
                    WorkingHours = stampXml.Element("hours") != null ? Convert.ToInt32(stampXml.Element("hours").Value) : Stamp.DefaultWorkingHours
                };

                if (stampXml.Element("Activities") != null)
                {
                    stamp.ActivityRecords.Clear();
                    foreach (var actxml in stampXml.Element("Activities").Elements("Activity"))
                    {
                        stamp.ActivityRecords.Add(new ActivityRecord()
                        {
                            Activity = actxml.Attribute("Name").Value,
                            Begin = SerializeHHMM(actxml.Attribute("Begin").Value),
                            End = SerializeHHMM(actxml.Attribute("End").Value),
                            Comment = actxml.Attribute("Comment").Value
                        });
                    }
                }

                stamps.Add(stamp);
            }
            return stamps;
        }

        #endregion


        public void ResumeStamping()
        {
            // assuming here that 
            if (TodayCurrentActivity != null)
                throw new NotSupportedException($"TodayCurrentActivity is not null after resume: {TodayCurrentActivity.Activity}, started: {TodayCurrentActivity.Begin}");

            SetToday();
        }

        public void SuspendStamping()
        {
            SetTodaysEndAndSaveXml();
        }

        public void SetTodaysEndAndSaveXml()
        {
            // update end time:
            // if end time has been inserted and is smaller than current end time, set new end time to 'now' (TODO: is this always desired? usually end time is empty anyway, except when explicitly provided...)
            if (Today.Begin.TotalMinutes != 0 && Today.End.TotalMinutes < Time.Now.TimeOfDay.TotalMinutes)
                Today.End = GetNowTime();

            FinishActivity(Today.End);

            SaveStampListXml();
        }


        // Actions:

        public void TakeDayOff(Stamp stamp)
        {
            stamp.Begin = new TimeSpan(8, 0, 0);
            stamp.End = new TimeSpan(8, 0, 0);
            stamp.Pause = new TimeSpan(0);
            stamp.ActivityRecords.Clear();
        }

        public bool DeleteStamp(Stamp stamp)
        {
            if (stamp != null)
            {
                int index = StampList.IndexOf(stamp);
                if (index != -1)
                {
                    StampList.Remove(stamp);
                    CurrentShown = StampList.Count > index ? StampList.ElementAt(index) : StampList.ElementAt(index - 1);
                    return true;
                }
            }
            return false;
        }


        // Formatting:

        public string FormatTimeSpan(TimeSpan span)
        {
            return (span < TimeSpan.Zero ? "-" : "")
                + AtLeastTwoDigits((int)Math.Floor(Math.Abs(span.TotalHours)))
                + ":" + AtLeastTwoDigits(span.Minutes);
        }

        private string AtLeastTwoDigits(int time)
        {
            var abs = Math.Abs(time);
            return (abs < 10 ? "0" + abs : "" + abs);
        }


        // Timing:

        public TimeProvider Time { get; set; } = new TimeProvider();

        public TimeSpan GetNowTime()
        {
            return GetTime(Time.Now.TimeOfDay);
        }

        public TimeSpan GetTime(TimeSpan accurate)
        {
            return new TimeSpan(accurate.Hours, accurate.Minutes, 0);
        }


        // Calculations:

        public TimeSpan CalculateTotalBalance(DateTime? calculateEndDate = null)
        {
            TimeSpan totalBalance = new TimeSpan(0);
            var StampRange = calculateEndDate.HasValue ? StampList.Where(s => s.Day.Date <= calculateEndDate) : StampList;
            foreach (var stamp in StampRange)
            {
                if (stamp.Day == Time.Today)
                    continue;
                totalBalance = totalBalance.Add(DayBalance(stamp));
            }
            return totalBalance;
        }

        public TimeSpan DayBalance(Stamp stamp) => DayTime(stamp) - TimeSpan.FromHours(stamp.WorkingHours);

        public TimeSpan DayTime(Stamp stamp)
        {
            if (stamp.Day == Time.Today && stamp.End == TimeSpan.Zero)
                return GetNowTime().Subtract(stamp.Begin).Subtract(stamp.Pause);
            else
                return stamp.End.Subtract(stamp.Begin).Subtract(stamp.Pause);
        }

        public TimeSpan Total(ActivityRecord activity)
        {
            if (!activity.Begin.HasValue)
                return TimeSpan.Zero;

            if (!activity.End.HasValue) // assuming this can only happen if it is the today's stamp and not yet checked out...
                return GetNowTime() - activity.Begin.Value;

            return activity.End.Value - activity.Begin.Value;
        }


        // Stamping:

        public void SetBegin(Stamp stamp, TimeSpan value)
        {
            if (value < stamp.Begin)
            {
                // also move first activity to the correct earlier begin time
                var activity = stamp.GetFirstActivity();
                if (activity != null)
                    activity.Begin = value;
            }
            else if (value > stamp.Begin)
            {
                // cut off earlier activities and/or set the earliest activity to the correct later begin time
                var activities = stamp.ActivityRecords.OrderBy(r => r.Begin.Value).ToArray();
                foreach (var act in activities)
                {
                    // starts before 'new check-in' and ends before 'new check-in': remove activity completely:
                    if (act.Begin.Value < value && act.End.HasValue && act.End.Value < value)
                        stamp.ActivityRecords.Remove(act);
                    // either starts after, ends after 'new check-in', or has open end: set new begin time for activity and stop iteration:
                    else
                    {
                        act.Begin = value;
                        break;
                    }
                }
            }
            stamp.Begin = value;
        }

        public void SetEnd(Stamp stamp, TimeSpan value)
        {
            if (stamp.End != default(TimeSpan) && value > stamp.End)
            {
                // also move latest activity to the correct later end time
                var activity = stamp.GetLastActivity();
                if (activity != null)
                    activity.End = value;
            }
            else if (stamp.End == default(TimeSpan) || value < stamp.End)
            {
                // cutt off later activities and/or set the latest activity to the correct earlier end time
                var activities = stamp.ActivityRecords.OrderByDescending(r => r.Begin.Value).ToArray();
                foreach (var act in activities)
                {
                    // starts after 'new check-out': remove activity completely:
                    if (act.Begin.Value >= value)
                        stamp.ActivityRecords.Remove(act);
                    // starts before: set new end time for activity and stop iteration:
                    else
                    {
                        act.End = value;
                        break;
                    }
                }
            }
            stamp.End = value;
        }

        public void SetPause(Stamp stamp, TimeSpan value)
        {
            if (stamp.ActivityRecords.Any())
            {
                // pause is only a number in minutes, there is not necessarily a 'start' / 'end' time available.
                // if pause has been recognized automatically, the function will cut off the activity before the pause and continues it after the pause, resulting in an activity 'gap', which indeed would be the pause start/end times.

                // find activity gap, created by automatic pause recognition function:
                // get the latest activity before pause, that is, the activity with an end time and no other activity with a matching start time (leaving the gap afterwards):
                // this may also be the last activity of the day, if the day has been finished, and no other pause gap has been found
                var activityBeforePause = stamp.ActivityRecords.FirstOrDefault(a => a.End.HasValue && !stamp.ActivityRecords.Any(aa => Math.Round(aa.Begin.Value.TotalMinutes) == Math.Round(a.End.Value.TotalMinutes)));

                TimeSpan timeDiff = (value - stamp.Pause);

                if (activityBeforePause != null)
                {
                    bool canDistribute = true;
                    do
                    {
                        canDistribute = true;
                        activityBeforePause.End = activityBeforePause.End.Value - timeDiff;
                        // should not result in negative activity time -> remove completely and apply change to previous activity
                        if (Total(activityBeforePause) < TimeSpan.Zero)
                        {
                            timeDiff = TimeSpan.Zero - Total(activityBeforePause);
                            var previousActivity = stamp.ActivityRecords.FirstOrDefault(a => a.End.HasValue && a.End.Value == activityBeforePause.Begin.Value);
                            if (previousActivity != null) // can this ever be null? (except for having an unreasonable long break of 4 hours...)
                            {
                                stamp.ActivityRecords.Remove(activityBeforePause);
                                activityBeforePause = previousActivity;
                                canDistribute = false;
                            }
                        }
                    } while (!canDistribute);
                }
                else
                {
                    // however, this may also be null, if the day is not yet finished (last is open-end) and the pause has not been recognized. in this case, just modify the first.
                    var starting = stamp.ActivityRecords.FirstOrDefault();

                    bool canDistribute = true;
                    do
                    {
                        canDistribute = true;
                        starting.Begin = starting.Begin.Value + timeDiff; // should expand in the other direction

                        // should not result in negative activity time -> remove completely and apply change to next activity
                        if (starting.End.HasValue && Total(starting) < TimeSpan.Zero)
                        {
                            timeDiff = TimeSpan.Zero - Total(starting);
                            var nextActivity = stamp.ActivityRecords.FirstOrDefault(a => a.Begin.Value == starting.End.Value);
                            if (nextActivity != null) // can this ever be null?
                            {
                                stamp.ActivityRecords.Remove(starting);
                                starting = nextActivity;
                                canDistribute = false;
                            }
                        }
                    } while (!canDistribute);
                }
            }
            stamp.Pause = value;
        }

        public void CalculatePauseFromActivities(Stamp stamp)
        {
            if (stamp.End != TimeSpan.Zero)
            {
                var workDuration = stamp.End - stamp.Begin;
                var activityDuration = TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => Total(a).TotalMinutes));
                stamp.Pause = workDuration > activityDuration ? workDuration - activityDuration : TimeSpan.Zero;
            }
        }

    }
}
