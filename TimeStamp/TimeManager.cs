using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        /// <summary>
        /// The path to the stamp list xml file on the file system
        /// </summary>
        public string StampFilePath { get; } = ".\\StampFile.xml";

        /// <summary>
        /// The list of all available stamps
        /// </summary>
        public List<Stamp> StampList { get; set; }

        /// <summary>
        /// The currently displayed stamp in the UI
        /// </summary>
        public Stamp CurrentShown { get; set; }


        // Today Data:

        /// <summary>
        /// Todays current stamp
        /// </summary>
        public Stamp Today { get; set; }

        /// <summary>
        /// Todays currently running activity
        /// </summary>
        public ActivityRecord TodayCurrentActivity { get; set; }

        public event Action CurrentActivityUpdated = delegate { };


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

            ResumeStamping();

            if (Today == null)
                throw new InvalidDataException("Today Stamp is null");
        }

        public bool IsStamping { get; private set; } = false;

        public void ResumeStamping()
        {
            // assuming here that activity is null!
            //if (TodayCurrentActivity != null)
            //throw new NotSupportedException($"TodayCurrentActivity is not null after resume: {TodayCurrentActivity.Activity}, started: {TodayCurrentActivity.Begin}");

            // this can happen when:
            // starting the app (e.g. new day autostart, same day computer restart)
            // resuming from sleep (e.g. new day resume, same day resume)
            // resuming from lunch break (insert pause parameter)..

            // find existing stamp for today:

            IsStamping = true;

            var existing = StampList.SingleOrDefault(t => t.Day == Time.Today);

            if (existing != null)
            {
                Today = CurrentShown = existing;
                Today.End = TimeSpan.Zero;

                // TODO: (optionally) ask in a notification, whether have been working or not since last known stamp (default yes, if choosing no will automatically insert a pause)...

                bool hasSetPause = RestoreLastActivity(Today);

                // update event (for ui):
                CurrentActivityUpdated();

                // show notification:
                if (hasSetPause)
                    //new Task(() =>
                    //{
                    //    System.Threading.Thread.Sleep(10000);
                    //}).Start();
                    PopupDialog.ShowAfterPause(Today.Pause, TodayCurrentActivity.Activity);
                else
                    PopupDialog.ShowCurrentlyTrackingActivity(TodayCurrentActivity.Activity);

                return;
            }

            // new day, new stamp:

            TodayCurrentActivity = null;
            Today = CurrentShown = new Stamp(Time.Today, GetNowTime(), Settings.DefaultWorkingHours);
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

            var previous = StampList.Where(s => s.Day < Today.Day).OrderByDescending(s => s.Day).FirstOrDefault();
            PopupDialog.ShowCurrentlyTrackingActivityOnNewDay(TodayCurrentActivity.Activity, previous);
        }

        private bool RestoreLastActivity(Stamp today)
        {
            // assuming the last activity is still valid, and the downtime is not considered a break:
            // if there is no running activity, try to restore the last activity
            if (TodayCurrentActivity == null)
            {
                var openEnd = today.ActivityRecords.FirstOrDefault(r => !r.End.HasValue);
                if (openEnd != null) // this case should actually never happen?
                {
                    Log.Add("Warning: RestoreLastActivity has found an open end activity. " + new StackTrace().ToString());
                    TodayCurrentActivity = openEnd;
                }
                else
                {
                    // this branch should be the default case for this method:

                    // get lastest logged activity:
                    var last = today.GetLastActivity();

                    // the downtime is considered a break:
                    if (IsQualifiedPauseBreak)
                    {
                        // determine pause time from last qualified event:

                        TimeSpan pauseStartTime;

                        // last lock off time is 'master':
                        if (Settings.IsLockingComputerWhenLeaving)
                        {
                            pauseStartTime = last.End.Value;
                            TodayCurrentActivity = last;
                        }
                        // last mouse movement time is 'master':
                        else
                        {
                            pauseStartTime = GetTime(LastMouseMove.TimeOfDay); // should be on same day... ;-)
                            LastMouseMove = default(DateTime);

                            last.End = pauseStartTime;
                            TodayCurrentActivity = last;
                        }

                        // set pause:
                        Today.Pause = GetNowTime() - pauseStartTime;

                        // ... and resume with a new activity now:
                        TodayCurrentActivity = null;
                        StartNewActivity(last.Activity, last.Comment);
                        return true;
                    }
                    // the downtime is not considered a break:
                    // otherwise, if log off time since then was more than 7 minutes, create and start new activity record (better documented, as the end time of the 'relative longer' absence is not lost):
                    else if (GetNowTime() - last.End.Value > TimeSpan.FromMinutes(7))
                    {
                        today.ActivityRecords.Add(new ActivityRecord() { Activity = last.Activity, Begin = last.End.Value, End = GetNowTime() });
                        StartNewActivity(last.Activity, null);
                    }
                    // otherwise, assume this is a total unrelevant break, e.g. fetched a cup of coffee, went to toilet, so just resume that activity (end time is reset to null, ergo lost and not documented):
                    else
                    {
                        TodayCurrentActivity = last;
                        last.End = null;
                    }
                }
            }
            return false;
        }

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

            // has checked out already (e.g. by deleting last 'open end' activity) -> reopen day by removing end time
            // there shouldnt be any unrecoverable loss, as the end should be resembled by an according activity end time.
            if (Today.End != default(TimeSpan))
            {
                Today.End = default(TimeSpan);
                // it is also necessary to update the pause time:
                CalculatePauseFromActivities(Today);
            }

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
            {
                Log.Add($"There are {unfinishedActivities.Count()} simultaneously running activies: {String.Join(", ", unfinishedActivities.Select(a => a.Activity))}");
            }
        }

        public void SuspendStamping(TimeSpan? explicitEndTime = null, bool considerLastMouseMove = false)
        {
            if (considerLastMouseMove)
            {
                // To tackle the delayed firing of the suspend event:
                if (LastMouseMove != default(DateTime) && LastMouseMove.Date == Today.Day)
                {
                    // TimeManager.Time.Now might as well be the next day in the morning....!!! In this case, always apply last mouse move time...
                    // or if still same day: check if last mouse move longer ago than 5 minutes
                    if (Today.Day != TimeManager.Time.Today || LastMouseMove.TimeOfDay < TimeManager.Time.Now.TimeOfDay - TimeSpan.FromMinutes(5))
                    {
                        // if yes -> provide last mouse move time to "SuspendStamping" method
                        Log.Add($"No activity tracked since {FormatTimeSpan(LastMouseMove.TimeOfDay)}, setting days end to that time...");
                        explicitEndTime = LastMouseMove.TimeOfDay;
                    }
                }
            }

            // Set Todays End And Save Xml:

            IsStamping = false;

            // update end time:
            // if end time has been inserted and is smaller than current end time, set new end time to 'now' (TODO: is this always desired? usually end time is empty anyway, except when explicitly provided...)
            if (Today.Begin.TotalMinutes != 0 && (explicitEndTime.HasValue || Today.End.TotalMinutes < Time.Now.TimeOfDay.TotalMinutes))
                Today.End = explicitEndTime ?? GetNowTime();

            FinishActivity(Today.End);

            SaveStampListXml();
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
                stampXml.Add(new XElement("begin", ParseHHMM(stamp.Begin)));
                stampXml.Add(new XElement("pause", SerializeMM(stamp.Pause)));
                stampXml.Add(new XElement("end", ParseHHMM(stamp.End)));
                if (!String.IsNullOrEmpty(stamp.Comment))
                    stampXml.Add(new XElement("comment", stamp.Comment));
                // always export working hours, as it is configurable:
                stampXml.Add(new XElement("hours", stamp.WorkingHours));

                if (stamp.ActivityRecords.Count > 0)
                {
                    var activityRoot = new XElement("Activities");
                    stampXml.Add(activityRoot);
                    foreach (var activity in stamp.ActivityRecords)
                    {
                        activityRoot.Add(new XElement("Activity",
                            new XAttribute("Name", activity.Activity ?? String.Empty),
                            new XAttribute("Begin", ParseHHMM(activity.Begin)),
                            new XAttribute("End", ParseHHMM(activity.End)),
                            new XAttribute("Comment", activity.Comment ?? String.Empty)));
                    }
                }
                rootXml.Add(stampXml);
            }

            return rootXml;
        }

        public static string ParseHHMM(TimeSpan? time)
        {
            if (!time.HasValue)
                return String.Empty;
            return time.Value.Hours + ":" + time.Value.Minutes;
        }

        public static bool TryParseHHMM(string time, out TimeSpan result)
        {
            if (TimeSettings.HHMM.IsMatch(time) && Int32.TryParse(time.Substring(0, time.IndexOf(":")), out int hours) && Int32.TryParse(time.Substring(time.IndexOf(":") + 1), out int minutes))
            {
                result = new TimeSpan(hours, minutes, 0);
                return true;
            }
            result = TimeSpan.Zero;
            return false;
        }

        public static TimeSpan ParseHHMM(string time)
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
                var stamp = new Stamp(Settings.DefaultWorkingHours)
                {
                    Day = Convert.ToDateTime(stampXml.Element("day").Value),
                    Begin = ParseHHMM(stampXml.Element("begin").Value),
                    End = ParseHHMM(stampXml.Element("end").Value),
                    Pause = SerializeMM(stampXml.Element("pause").Value),
                    Comment = stampXml.Element("comment") != null ? stampXml.Element("comment").Value : String.Empty,
                    WorkingHours = stampXml.Element("hours") != null ? Convert.ToInt32(stampXml.Element("hours").Value) : 8 // legacy, when value was not configurable it was always 8
                };

                if (stampXml.Element("Activities") != null)
                {
                    stamp.ActivityRecords.Clear();
                    foreach (var actxml in stampXml.Element("Activities").Elements("Activity"))
                    {
                        stamp.ActivityRecords.Add(new ActivityRecord()
                        {
                            Activity = actxml.Attribute("Name").Value,
                            Begin = ParseHHMM(actxml.Attribute("Begin").Value),
                            End = ParseHHMM(actxml.Attribute("End").Value),
                            Comment = actxml.Attribute("Comment").Value
                        });
                    }
                }

                stamps.Add(stamp);
            }
            return stamps;
        }

        #endregion


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

        public static TimeProvider Time { get; set; } = new TimeProvider();

        public static TimeSpan GetNowTime()
        {
            return GetTime(Time.Now.TimeOfDay);
        }

        public static TimeSpan GetTime(TimeSpan accurate)
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

        public static TimeSpan Total(ActivityRecord activity)
        {
            if (!activity.Begin.HasValue)
                return TimeSpan.Zero;

            if (!activity.End.HasValue) // assuming this can only happen if it is the today's stamp and not yet checked out...
                return GetNowTime() - activity.Begin.Value;

            return activity.End.Value - activity.Begin.Value;
        }

        public bool HasMatchingActivityTimestamps(Stamp stamp, out string error)
        {
            var stampTime = DayTime(stamp);
            var activityTime = TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(r => Total(r).TotalMinutes));
            bool isMatching = stampTime == activityTime;
            error = isMatching ? null : $"The sum value of the day stamps ({stampTime}) does not match with the sum value of the activities ({activityTime}).";
            return isMatching;
        }

        // Stamping:

        public static void SetBegin(Stamp stamp, TimeSpan value)
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

        public static void SetEnd(Stamp stamp, TimeSpan value)
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

        public static void CalculatePauseFromActivities(Stamp stamp)
        {
            //if (stamp.End != TimeSpan.Zero)
            //{
            //var workDuration = stamp.End - stamp.Begin;
            //var activityDuration = TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => Total(a).TotalMinutes));
            //stamp.Pause = workDuration > activityDuration ? workDuration - activityDuration : TimeSpan.Zero;
            //}

            if (stamp.ActivityRecords.Any())
            {
                var ordered = stamp.ActivityRecords.OrderBy(r => r.Begin).ToList();

                TimeSpan pauses = TimeSpan.Zero;
                for (int i = 0; i < stamp.ActivityRecords.Count - 1; i++)
                {
                    // may be caused by 'SetPause':
                    if (i == 0 && ordered.ElementAt(i).Begin.Value > stamp.Begin)
                        pauses += ordered.ElementAt(i).Begin.Value - stamp.Begin;

                    if (ordered.ElementAt(i).End.HasValue && ordered.ElementAt(i + 1).Begin.HasValue)
                        pauses += (ordered.ElementAt(i + 1).Begin.Value - ordered.ElementAt(i).End.Value);
                }
                stamp.Pause = pauses;
            }
        }

        public static void SetActivityBegin(Stamp stamp, ActivityRecord activity, TimeSpan value)
        {
            // invalid action:
            if (value >= activity.End)
                return;

            // add new, pending activity entry if applicable:
            if (!stamp.ActivityRecords.Contains(activity))
                stamp.ActivityRecords.Add(activity);

            var ordered = stamp.ActivityRecords.OrderBy(r => r.Begin).ToList();

            // first activity start changed -> also change days start stamp
            if (activity.Begin == stamp.Begin)
            {
                stamp.Begin = value;
            }
            // in between start changed -> also change end of previous activity, if they previously matched
            else if (activity != ordered.FirstOrDefault())
            {
                int index = ordered.IndexOf(activity) - 1;

                bool isIterating;
                do
                {
                    isIterating = false;
                    var previousActivity = ordered.ElementAt(index); //grdActivities.Rows[index].Tag as ActivityRecord;
                    if (!previousActivity.End.HasValue || previousActivity.End.Value >= value)
                    {
                        previousActivity.End = value;
                        // activity is hidden / negative after change -> remove activity
                        if (Total(previousActivity) < TimeSpan.Zero)
                        {
                            stamp.ActivityRecords.Remove(previousActivity);
                            if (index == 0)
                            {
                                // removed first stamp -> also set stamp begin
                                stamp.Begin = value;
                            }
                            index--;
                            isIterating = true;
                        }
                    }
                } while (index >= 0 && isIterating);
            }

            activity.Begin = value;

            // pause interruption gap(s) is/are changed -> also change day pause stamp
            CalculatePauseFromActivities(stamp);
        }

        public static void SetActivityEnd(Stamp stamp, ActivityRecord activity, TimeSpan value)
        {
            // invalid action:
            if (value <= activity.Begin)
                return;

            var ordered = stamp.ActivityRecords.OrderBy(r => r.Begin).ToList();

            // last activity end changed -> also change days end stamp
            if (activity.End == stamp.End)
            {
                stamp.End = value;
            }
            // in between end changed -> also change start of next activity, if they previously matched
            else if (activity != ordered.LastOrDefault())
            {
                int index = ordered.IndexOf(activity) + 1;

                bool isIterating;
                do
                {
                    isIterating = false;
                    var nextActivity = ordered.ElementAt(index);
                    if (nextActivity.Begin <= value)
                    {
                        nextActivity.Begin = value;
                        // activity is hidden / negative after change -> remove activity
                        if (Total(nextActivity) < TimeSpan.Zero)
                        {
                            stamp.ActivityRecords.Remove(nextActivity);
                            if (index == ordered.Count - 1)
                            {
                                // removed last stamp -> also set stamp end
                                stamp.End = value;
                            }
                            index++;
                            isIterating = true;
                        }
                    }
                } while (index <= ordered.Count - 1 && isIterating);
            }
            activity.End = value;

            // pause interruption gap(s) is/are changed -> also change day pause stamp
            CalculatePauseFromActivities(stamp);
        }

        public bool CanDeleteActivity(Stamp stamp, ActivityRecord activity)
        {
            if (stamp.ActivityRecords.Count <= 1)
                return false;
            return true;
        }

        public void DeleteActivity(Stamp stamp, ActivityRecord activity)
        {
            if (activity == TodayCurrentActivity)
            {
                // todays end auf neuen letzten zeitpunkt setzen, current activity clearen:
                stamp.ActivityRecords.Remove(activity);
                TodayCurrentActivity = null;
                var newLastActivity = stamp.GetLastActivity();
                stamp.End = newLastActivity.End.Value;
                // TODO: need to recalculate pause?
                TimeManager.CalculatePauseFromActivities(stamp);
            }
            else if (activity == stamp.GetFirstActivity())
            {
                // update todays start time:
                stamp.ActivityRecords.Remove(activity);
                var newFirstActivity = stamp.GetFirstActivity();
                stamp.Begin = newFirstActivity.Begin.Value;
                // TODO: need to recalculate pause?
                TimeManager.CalculatePauseFromActivities(stamp);
            }
            else if (activity == stamp.GetLastActivity())
            {
                // update todays end time:
                stamp.ActivityRecords.Remove(activity);
                var newLastActivity = stamp.GetLastActivity();
                stamp.End = newLastActivity.End.Value;
                // TODO: need to recalculate pause?
                TimeManager.CalculatePauseFromActivities(stamp);
            }
            else
            {
                // in between activity; update todays pause time:
                stamp.ActivityRecords.Remove(activity);
                TimeManager.CalculatePauseFromActivities(stamp);
            }
        }


        // Pause Recognition:


        public DateTime LastMouseMove { get; set; }

        public bool IsInPauseTimeRecognitionMode
        {
            get
            {
                return Settings.AutomaticPauseRecognition
                    && (Time.Now.TimeOfDay >= Settings.AutomaticPauseRecognitionStartTime)
                    && (Time.Now.TimeOfDay <= Settings.AutomaticPauseRecognitionStopTime)
                    && Today.Pause == TimeSpan.Zero;
            }
        }

        public bool IsQualifiedPauseBreak
        {
            get
            {
                // is in time slot, doesnt already have pause set:
                if (!IsInPauseTimeRecognitionMode)
                    return false;

                // last lock off time is 'master':
                if (Settings.IsLockingComputerWhenLeaving)
                {
                    // last activitiy end is known and qualifies in time span:
                    if (Today.GetLastActivity().End.HasValue && Time.Now.TimeOfDay.Subtract(Today.GetLastActivity().End.Value).TotalMinutes >= Settings.AutomaticPauseRecognitionMinPauseTime)
                        return true;
                }
                // last mouse movement time is 'master':
                else
                {
                    // last mouse movement is known and qualifies in time span:
                    if (LastMouseMove != default(DateTime) && Time.Now.Day == LastMouseMove.Day && Time.Now.TimeOfDay.Subtract(LastMouseMove.TimeOfDay).TotalMinutes >= Settings.AutomaticPauseRecognitionMinPauseTime)
                        return true;
                }

                return false;
            }
        }

    }
}
