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

        public Action RequestUpdateUI { get; set; } = delegate { };

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

            var allActs = StampList.SelectMany(s => s.ActivityRecords).ToArray();
            //foreach (var act in allActs.Where(a => a.Activity == "Product Development (CA2)"))
            //{
            //    act.Activity = "Product Development";
            //    act.Tags.Add("CA2");
            //}
            //foreach (var act in allActs.Where(a => a.Activity == "Product Development (VES)"))
            //{
            //    act.Activity = "Product Development";
            //    act.Tags.Add("VES");
            //}
            //foreach (var act in allActs.Where(a => a.Activity == "Product Development (LDD-Cap)"))
            //{
            //    act.Activity = "Product Development";
            //    act.Tags.Add("Premigrate");
            //}
            //foreach (var act in allActs.Where(a => a.Activity == "Mentor Product Requirements" || a.Activity == "Mentor / Siemens Integration"))
            //{
            //    act.Activity = "Compliance Requirements";
            //}
            //var allTags = Settings.Tags.SelectMany(t => t.Value).ToArray();
            //foreach (var act in allActs.Where(a => !String.IsNullOrEmpty(a.Comment)))
            //{
            //    if (act.Comment == " Resuming after pause..." || act.Comment == "logged off time")
            //        act.Comment = string.Empty;

            //    if (act.Comment == "Vestigo" && !act.Tags.Contains("VES"))
            //    {
            //        act.Tags.Add("VES");
            //        act.Comment = string.Empty;
            //    }

            //    var matches = allTags.Where(t => act.Comment.Contains(t));
            //    Console.WriteLine(act.Comment + " -> " + String.Join(", ", matches));
            //    act.Tags.AddRange(matches);
            //    if (matches.Any())
            //        act.Comment = String.Empty;
            //}

            //SaveStampListXml();

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

            var existing = StampList.SingleOrDefault(t => t.Day == Time.Today);

            if (existing != null)
            {
                Today = CurrentShown = existing;
                Today.End = TimeSpan.Zero;

                // TODO: (optionally) ask in a notification, whether have been working or not since last known stamp (default yes, if choosing no will automatically insert a pause)...

                // restore last activity:
                // assuming the last activity is still valid, if there is no running activity, try to restore the last activity:
                if (TodayCurrentActivity == null)
                {
                    // get latest logged activity:
                    var last = Today.GetLastActivity();

                    // the downtime is not considered a break, however a new activity record is started to provide better documentation:
                    // finish last now:
                    last.End = GetNowTime();
                    // and immediately start new:
                    StartNewActivity(last.Activity, last);
                }

                // update event (for ui):
                CurrentActivityUpdated();

                // show notification:
                PopupDialog.ShowCurrentlyTrackingActivity(TodayCurrentActivity?.Activity);

                return;
            }

            // new day, new stamp:

            TodayCurrentActivity = null;
            Today = CurrentShown = new Stamp(Time.Today, GetNowTime(), Settings.GetDefaultWorkingHours(Time.Today));
            StampList.Add(Today);

            string startingActivity = GetDefaultStartingActivity(Time.Today, out var mostRecent);

            StartNewActivity(startingActivity, mostRecent);

            var previous = StampList.Where(s => s.Day < Today.Day).OrderByDescending(s => s.Day).FirstOrDefault();
            PopupDialog.ShowCurrentlyTrackingActivityOnNewDay(TodayCurrentActivity.Activity, previous);
        }

        public string GetDefaultStartingActivity(DateTime date, out ActivityRecord lastRecord)
        {
            lastRecord = null;

            // default starting activity is specified in settings?
            if (!String.IsNullOrEmpty(Settings.AlwaysStartNewDayWithActivity))
                return Settings.AlwaysStartNewDayWithActivity;

            // find most recent activity:
            foreach (var day in StampList.Where(d => d.Day <= date).OrderByDescending(s => s.Day))
            {
                if (!day.ActivityRecords.Any())
                    continue;
                lastRecord = day.GetLastActivity();
                return lastRecord.Activity;
            }

            // fallback, first tracked activity:
            return Settings.TrackedActivities.ElementAtOrDefault(0) ?? String.Empty;
        }

        public void StartNewActivity(string name, ActivityRecord previous)
        {
            // finish current activity:
            if (TodayCurrentActivity != null)
            {
                TodayCurrentActivity.End = GetNowTime();
            }

            var newActivity = new ActivityRecord() { Activity = name, Begin = GetNowTime() };

            // if same activity, bring previous comment and tags:
            if (previous != null && name == previous.Activity)
            {
                newActivity.Comment = previous.Comment;
                newActivity.Tags = previous.Tags.ToList();
            }

            // start new activity:
            TodayCurrentActivity = newActivity;
            Today.ActivityRecords.Add(TodayCurrentActivity);

            // has checked out already (e.g. by deleting last 'open end' activity) -> reopen day by removing end time
            // there shouldnt be any unrecoverable loss, as the end should be resembled by an according activity end time.
            if (Today.End != default(TimeSpan))
            {
                Today.End = default(TimeSpan);
                // it is also necessary to update the pause time:
                CalculatePauseFromActivities(Today);
            }

            ValidateStamp(Today);

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
                if (LastUserAction != default(DateTime) && LastUserAction.Date == Today.Day)
                {
                    // TimeManager.Time.Now might as well be the next day in the morning....!!! In this case, always apply last mouse move time...
                    // or if still same day: check if last mouse move longer ago than 5 minutes
                    if (Today.Day != TimeManager.Time.Today || LastUserAction.TimeOfDay < TimeManager.Time.Now.TimeOfDay - TimeSpan.FromMinutes(5))
                    {
                        // if yes -> provide last mouse move time to "SuspendStamping" method
                        Log.Add($"No activity tracked since {FormatTimeSpan(LastUserAction.TimeOfDay)}, setting days end to that time...");
                        explicitEndTime = LastUserAction.TimeOfDay;
                    }
                }
            }

            // update end time:
            // if end time has been inserted and is smaller than current end time, set new end time to 'now' (TODO: is this always desired? usually end time is empty anyway, except when explicitly provided...)
            if (Today.Begin.TotalMinutes != 0 && (explicitEndTime.HasValue || Today.End.TotalMinutes < Time.Now.TimeOfDay.TotalMinutes))
                Today.End = explicitEndTime ?? GetNowTime();

            FinishActivity(Today.End);

            // save xml:
            SaveStampListXml();
        }

        public void ValidateStamp(Stamp stamp)
        {
            // remove activities with duration '0':
            if (stamp.ActivityRecords != null)
            {
                foreach (var activity in stamp.ActivityRecords.ToArray())
                {
                    if (activity.Begin.HasValue && activity.End.HasValue && activity.End.Value == activity.Begin.Value)
                        DeleteActivity(stamp, activity);
                }

                // auto-fix unmatching activity times with day start/end times:
                if (stamp.ActivityRecords.Any())
                {
                    var stampTime = DayTime(stamp);
                    var activityTime = TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(r => Total(r).TotalMinutes));
                    if (Math.Abs(stampTime.TotalMinutes - activityTime.TotalMinutes) > 0.01)
                    {
                        Log.Add($"The sum value of the day stamps ({stampTime}) does not match with the sum value of the activities ({activityTime}). Stamp: {stamp.Day.ToShortDateString()}");
                    }
                }
            }

            // TODO: check if start > end...
            // TODO: check if overlapping...
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
                            new XAttribute("Comment", activity.Comment ?? String.Empty),
                            activity.Tags.Select((t, i) => new XAttribute("Tag_" + i, t))));
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
                        var record = new ActivityRecord()
                        {
                            Activity = actxml.Attribute("Name").Value,
                            Begin = ParseHHMM(actxml.Attribute("Begin").Value),
                            End = ParseHHMM(actxml.Attribute("End").Value),
                            Comment = actxml.Attribute("Comment").Value
                        };
                        foreach (var tagAttrib in actxml.Attributes().Where(a => a.Name.LocalName.StartsWith("Tag_")))
                            record.Tags.Add(tagAttrib.Value);

                        // skip '0' duration activities...
                        if (record.Begin.HasValue && record.End.HasValue && record.Begin.Value == record.End.Value)
                            continue;
                        stamp.ActivityRecords.Add(record);
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

        public void SetPause(Stamp stamp, TimeSpan value, bool autoSplitActivity = false)
        {
            if (stamp.ActivityRecords.Any())
            {
                // pause is only a number in minutes, there is not necessarily a 'start' / 'end' time available.
                // if pause has been recognized automatically, the function will cut off the activity before the pause and continues it after the pause, resulting in an activity 'gap', which indeed would be the pause start/end times.

                // find activity gap, created by automatic pause recognition function:
                // get the latest activity before pause, that is, the activity with an end time and no other activity with a matching start time (leaving the gap afterwards):
                // this may also be the last activity of the day, if the day has been finished, and no other pause gap has been found
                var activityBeforePause = stamp.ActivityRecords.FirstOrDefault(a => a.End.HasValue && !stamp.ActivityRecords.Any(aa => Math.Round(aa.Begin.Value.TotalMinutes) == Math.Round(a.End.Value.TotalMinutes)) && a != stamp.GetLastActivity());

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
                else if (autoSplitActivity)
                {
                    var pauseMatch = stamp.ActivityRecords.FirstOrDefault(a => a.Begin.Value < Settings.AutomaticPauseRecognitionStopTime && (!a.End.HasValue || a.End.Value > Settings.AutomaticPauseRecognitionStartTime));
                    if (pauseMatch == null)
                        pauseMatch = stamp.ActivityRecords.Last();

                    var halfDuration = GetTime(TimeSpan.FromMinutes(((pauseMatch.End ?? Time.Now.TimeOfDay) - pauseMatch.Begin.Value).TotalMinutes / 2));
                    var splitTime = pauseMatch.Begin.Value + halfDuration;

                    var newAct = SplitActivity(stamp, pauseMatch, splitTime);

                    var halfPause = GetTime(TimeSpan.FromMinutes(timeDiff.TotalMinutes / 2));
                    // TODO: known bug: this may fail and created chaos, when new end < begin -> should be handled as if dragged in timeline (killing previous activities?)
                    SetActivityEnd(stamp, newAct, newAct.End.Value - halfPause);
                    SetActivityBegin(stamp, pauseMatch, pauseMatch.Begin.Value + halfPause);
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
                    var current = ordered.ElementAt(i);
                    var next = ordered.ElementAt(i + 1);

                    // may be caused by 'SetPause':
                    if (i == 0 && current.Begin.HasValue && current.Begin.Value > stamp.Begin)
                        pauses += current.Begin.Value - stamp.Begin;

                    if (current.End.HasValue && next.Begin.HasValue)
                        pauses += (next.Begin.Value - current.End.Value);
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

        public ActivityRecord SplitActivity(Stamp stamp, ActivityRecord activity, TimeSpan splitTime)
        {
            var newActivity = new ActivityRecord()
            {
                Activity = activity.Activity,
                Comment = activity.Comment,
                Tags = activity.Tags.ToList(),
                Begin = activity.Begin,
                End = splitTime,
            };

            stamp.ActivityRecords.Insert(stamp.ActivityRecords.IndexOf(activity), newActivity);

            activity.Begin = splitTime;

            return newActivity;
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


        // Tagging:

        public List<string[]> GetMostFrequentTags(string activity = null, int count = 5, TimeSpan? recentActivities = null)
        {
            var allTags = StampList.Where(s => recentActivities == null || DateTime.Today - s.Day <= recentActivities.Value).SelectMany(s => s.ActivityRecords).Where(a => a.Tags.Any() && (activity == null || a.Activity == activity)).Select(a => a.Tags).ToList();

            var groupedTags = allTags.GroupBy(a => String.Join("", a.OrderBy(t => t))).OrderByDescending(g => g.Count());

            return groupedTags.Take(count).Select(g => g.First().ToArray()).ToList();
        }

        // Pause Recognition:

        private DateTime m_lastUserAction;

        /// <summary>
        /// Time of last user action. This could be mouse movement, touch input, log on/off event, keyboard key press etc...
        /// </summary>
        public DateTime LastUserAction
        {
            get => m_lastUserAction;
            set
            {
                if (m_lastUserAction != value)
                {
                    ValidateLastUserAction(m_lastUserAction, value);
                    m_lastUserAction = value;
                }
            }
        }

        private void ValidateLastUserAction(DateTime lastUserAction, DateTime currentUserAction)
        {
            if (lastUserAction != default(DateTime))
            {
                // mouse move on a new day...
                if (lastUserAction.Date < currentUserAction.Date)
                {
                    Log.Add($"New working day detected (by mouse move) (last mouse move: {lastUserAction}, mouse move just now: {currentUserAction})");

                    // set correct end time on the previous stamp:
                    var lastMouseMoveStamp = StampList.FirstOrDefault(s => s.Day == lastUserAction.Date);
                    if (lastMouseMoveStamp != null)
                    {
                        var lastMoveTime = GetTime(lastUserAction.TimeOfDay);
                        Log.Add($"End time of stamp {lastMouseMoveStamp.Day} set from {lastMouseMoveStamp.End} to {lastMoveTime}");
                        SetEnd(lastMouseMoveStamp, lastMoveTime);
                    }

                    // start new stamp for today:
                    if (Today.Day != currentUserAction.Date)
                    {
                        Log.Add($"Creating new day stamp for {currentUserAction.Date}");
                        ResumeStamping();
                    }
                    // or, if already existing (starting at weird time in the middle of night like 3:10am, from an windows update or something):
                    else
                    {
                        var nowTime = GetTime(currentUserAction.TimeOfDay);
                        Log.Add($"Start time of stamp {Today.Day} set from {Today.Begin} to {nowTime}");
                        SetBegin(Today, nowTime);

                        // show notification:
                        PopupDialog.ShowCurrentlyTrackingActivity(TodayCurrentActivity?.Activity);
                    }

                    // update UI:
                    RequestUpdateUI();
                }
                // pause recognition -> check if it is a qualified pause break:
                else if (IsInPauseTimeRecognitionMode && currentUserAction.Day == lastUserAction.Day && currentUserAction.TimeOfDay.Subtract(lastUserAction.TimeOfDay).TotalMinutes >= Settings.AutomaticPauseRecognitionMinPauseTime)
                {
                    // get latest logged activity:
                    var last = Today.GetLastActivity();

                    if (last != null)
                    {
                        // clip end of last activity to 'pause start time':
                        last.End = GetTime(lastUserAction.TimeOfDay);

                        // ... and resume with a new activity now: // TODO: this does not honor currentUserAction parameter; otherwise, it currently is always 'NOW'...
                        TodayCurrentActivity = null;
                        StartNewActivity(last.Activity, last);

                        // update the pause time:
                        CalculatePauseFromActivities(Today);

                        // update UI:
                        RequestUpdateUI();

                        // show notification:
                        PopupDialog.ShowAfterPause(Today.Pause, TodayCurrentActivity?.Activity);
                    }
                }
            }
        }

        /// <summary>
        /// Is pause recognition enabled and currently is in specified pause time slot
        /// </summary>
        public bool IsInPauseTimeRecognitionMode => Settings.AutomaticPauseRecognition
                                                    && Time.Now.TimeOfDay >= Settings.AutomaticPauseRecognitionStartTime
                                                    && Time.Now.TimeOfDay <= Settings.AutomaticPauseRecognitionStopTime;
    }
}
