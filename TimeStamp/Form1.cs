using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Win32;
using System.Xml.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Diagnostics;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using System.Globalization;

namespace TimeStamp
{
    public partial class Form1 : Form
    {

        #region Settings

        private bool automaticPauseRecognition = true;
        public static TimeSpan AutomaticPauseRecognitionStartTime = new TimeSpan(11, 30, 0);
        public static TimeSpan AutomaticPauseRecognitionStopTime = new TimeSpan(13, 30, 0);
        public readonly int AutomaticPauseRecognitionMinPauseTime = 12;

        private static readonly Regex HHMM = new Regex("[0-9]{2}[:]{1}[0-9]{2}");
        private readonly Regex DDMMYYYY = new Regex("[0-9]{2}[.]{1}[0-9]{2}[.]{1}[0-9]{4}");
        private readonly Regex Integer = new Regex("[^0-9]+");

        public static List<string> TrackedActivities { get; set; }

        public static string AlwaysStartNewDayWithActivity { get; set; } = "Product Development";

        private void LoadSettings()
        {
            automaticPauseRecognition = checkBox1.Checked = GetKey("AutomaticPauseRecognition", true);

            StatisticType = (StatisticTypes)GetKey("StatisticsTypeIndex", 0);
            StatisticRange = (StatisticRanges)GetKey("StatisticsTimeIndex", 0);

            this.Width = GetKey("WindowWidth", this.Width);
            this.Height = GetKey("WindowHeight", this.Height);

            TrackedActivities = GetKey("TrackedActivities", String.Empty).Split(new[] { ";;;" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!TrackedActivities.Any())
            {
                // default:
                TrackedActivities = new List<string>()
                {
                    "Paid Requirements",
                    "Product Development",
                    "Product Support",
                    "Meetings",
                    "Documentation"
                };
            }

            AlwaysStartNewDayWithActivity = GetKey("AlwaysStartNewDayWithActivity", (string)null);
            if (AlwaysStartNewDayWithActivity == String.Empty)
                AlwaysStartNewDayWithActivity = null;
        }

        private T GetKey<T>(string name, T defaultValue)
        {
            var key = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", name, defaultValue);
            if (key != null && key is T)
            {
                return (T)key;
            }
            return defaultValue;
        }

        private void SaveSettings()
        {
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "AutomaticPauseRecognition", checkBox1.Checked);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "StatisticsTypeIndex", cmbStatisticType.SelectedIndex);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "StatisticsTimeIndex", cmbStatisticRange.SelectedIndex);

            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "WindowWidth", this.Width);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "WindowHeight", this.Height);

            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "TrackedActivities", String.Join(";;;", TrackedActivities));
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "AlwaysStartNewDayWithActivity", AlwaysStartNewDayWithActivity ?? String.Empty);
        }

        #endregion

        // Data:

        public List<Stamp> StampList;
        public readonly string StampFilePath = ".\\StampFile.xml";
        public readonly string LogFilePath = ".\\StampLog.txt";
        public Stamp CurrentShown { get; set; }


        // Today Data:

        public Stamp Today { get; set; }

        public void SetToday()
        {
            // this can happen when:
            // starting the app (e.g. new day autostart, same day computer restart)
            // resuming from sleep (e.g. new day resume, same day resume)

            // find existing stamp for today:

            var existing = StampList.SingleOrDefault(t => t.Day == DateTime.Today);

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
                    HighlightCurrentActivity();
                }

                PopupDialog.ShowCurrentlyTrackingActivity(this, TodayCurrentActivity.Activity);
                CreateOrUpdateTrayIconContextMenu();
                return;
            }

            // new day, new stamp:

            TodayCurrentActivity = null;
            Today = CurrentShown = new Stamp(DateTime.Today, GetNowTime());
            StampList.Add(Today);

            if (String.IsNullOrEmpty(AlwaysStartNewDayWithActivity))
            {
                // not specified ? -> keep tracking for latest activity...
                foreach (var day in StampList.OrderByDescending(s => s.Day))
                {
                    if (!day.ActivityRecords.Any())
                        continue;
                    StartNewActivity(day.GetLastActivity().Activity, null);
                    break;
                }
            }

            if (TodayCurrentActivity == null) // immer noch null?
                StartNewActivity(AlwaysStartNewDayWithActivity ?? TrackedActivities.ElementAt(1), null);

            PopupDialog.ShowCurrentlyTrackingActivity(this, TodayCurrentActivity.Activity);
        }

        public ActivityRecord TodayCurrentActivity;

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

            HighlightCurrentActivity();
            CreateOrUpdateTrayIconContextMenu();
        }

        public void FinishActivity()
        {
            if (TodayCurrentActivity != null)
            {
                TodayCurrentActivity.End = GetNowTime();
                TodayCurrentActivity = null;
            }

            var unfinishedActivities = Today.ActivityRecords.Where(r => !r.End.HasValue);
            if (unfinishedActivities.Count() > 0)
                throw new ArgumentOutOfRangeException($"There are {unfinishedActivities.Count()} simultaneously running activies: {String.Join(", ", unfinishedActivities.Select(a => a.Activity))}");
        }

        // TODO:

        // LOW PRIO:

        // advanced features to ask whether current activity is still correct,
        //      e.g. after notebook hatch been closed and reopened -> Meeting or Activity before Meeting? https://stackoverflow.com/questions/3355606/detect-laptop-lid-closure-and-opening
        //           -> this is actually identical to lock/unlock, as the default action probably shouldnt be 'do nothing' anyway... (also, the event does not fire reliably...)
        //      e.g. upon certain app start / changes, 
        //      e.g. PKI card inserted / removed, https://cgeers.wordpress.com/2008/02/03/monitoring-a-smartcard-reader/ or http://forums.codeguru.com/showthread.php?510947-How-to-detect-smart-card-reader-insertion
        //      etc...?


        #region Loads

        public Form1()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine(e.ExceptionObject);
            };

            LoadSettings();
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
            catch (Exception e)
            {
                MessageBox.Show(getFullExceptionMessage(e));
            }

            try
            {
                foreach (var Stamp in StampList.ToArray())
                    if (Stamp == null)
                        StampList.Remove(Stamp);

                // set todays stamp:
                var todayEntries = StampList.Where(t => t.Day == DateTime.Today);

                if (todayEntries.Count() > 1)
                    throw new IndexOutOfRangeException("Several Todays in StampList found!");

                SetToday();

                if (Today == null)
                    throw new InvalidDataException("Today Stamp is null");

                // initialize controls:
                btnAddTimestamp.BringToFront();

                btnDeleteStamp.Click += new EventHandler(btnDeleteStamp_Click);
                btnTakeDayOff.Click += new EventHandler(btnTakeDayOff_Click);
                StampCalendar.DateChanged += StampCalendar_DateChanged;

                foreach (var statType in Enum.GetNames(typeof(StatisticTypes)))
                    cmbStatisticType.Items.Add(statType);

                foreach (var statRange in Enum.GetNames(typeof(StatisticRanges)))
                    cmbStatisticRange.Items.Add(statRange);

                cmbStatisticType.SelectedIndexChanged += new EventHandler(comboBox1_SelectedIndexChanged);
                cmbStatisticRange.SelectedIndexChanged += new EventHandler(comboBox2_SelectedIndexChanged);

                lblTotalBalance.MaximumSize = groupBox1.Size;
                refreshControls();

                pauseSpanRecognizer = new Timer() { Interval = 5000, Enabled = true };
                pauseSpanRecognizer.Tick += new EventHandler(middaySpan_Tick);
                checkBox1.Text = "Automatic Pause Recognition ( >" + AutomaticPauseRecognitionMinPauseTime + " mins AFK between "
                    + AutomaticPauseRecognitionStartTime.Hours + ":" + AutomaticPauseRecognitionStartTime.Minutes + "-"
                    + AutomaticPauseRecognitionStopTime.Hours + ":" + AutomaticPauseRecognitionStopTime.Minutes + ")";
            }
            catch (Exception e)
            {
                MessageBox.Show(getFullExceptionMessage(e));
                Application.Exit();
            }

            cmbStatisticType.SelectedIndex = (int)StatisticType;
            cmbStatisticRange.SelectedIndex = (int)StatisticRange;

            // enable events for system sleep/standby/resume and OS log on/off:
            //SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            // enable events for notebook lid opening/closing:
            //RegisterForPowerNotifications();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();

            SetTodaysEndAndSaveXml();

            this.notifyIcon1.Visible = false;
        }

        //private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        //{
        //    switch (e.Mode)
        //    {
        //        case PowerModes.Resume:

        //            Log("Resuming from sleep...");

        //            // might be:
        //            // - resuming in the morning of the next working day (Today set, but not correct any more)
        //            // - resuming on the same working day after sleep (Today set and correct)

        //            // does not always fire correctly, see:
        //            // https://stackoverflow.com/questions/51271460/c-sharp-wpf-powermodechanged-doesnt-work-on-surface
        //            // I am also working on such a problem. From what I've read, the Surface supports "Sleep state(Modern Standby)", 
        //            // or S0 low-power, and is not yet in actual sleep state (S1-3). Pressing the power button or clicking the "sleep"
        //            // option from the windows menu does not enter sleep directly but enters S0 low-power instead, thus not triggering PowerModeChanged.
        //            // https://docs.microsoft.com/en-us/windows/desktop/power/system-power-states#sleep-state-modern-standby

        //            ResumeStamping();

        //            break;

        //        case PowerModes.Suspend:

        //            Log("Sleeping...");

        //            SuspendStamping();

        //            break;
        //    }
        //}

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionUnlock:
                    // Back from lock/standby
                    Log("System Unlocked...");

                    ResumeStamping();

                    break;

                case SessionSwitchReason.SessionLock:
                    // Going into lock/standby screen
                    Log("System Locked...");

                    SuspendStamping();

                    break;
            }
        }

        private void ResumeStamping()
        {
            // assuming here that 
            if (TodayCurrentActivity != null)
                throw new NotSupportedException($"TodayCurrentActivity is not null after resume: {TodayCurrentActivity.Activity}, started: {TodayCurrentActivity.Begin}");

            SetToday();

            refreshControls();
        }

        private void SuspendStamping()
        {
            SetTodaysEndAndSaveXml();
        }

        private void SetTodaysEndAndSaveXml()
        {
            // update end time:
            if (Today.Begin.TotalMinutes != 0 && Today.End.TotalMinutes < DateTime.Now.TimeOfDay.TotalMinutes)
                Today.End = GetNowTime();

            FinishActivity();

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
                    //xml.Serialize(fs, StampList);
                    fs.Close();
                    success = true;
                }
                catch
                { }
            }
        }

        private void Log(string line)
        {
            File.AppendAllLines(LogFilePath, new[] { DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " " + line });
        }

        #endregion

        #region Controls
        private void refreshControls()
        {
            RefreshStampTextBoxes();

            txtComment.Text = CurrentShown.Comment;
            txtWorkingHours.Text = Convert.ToInt32(CurrentShown.WorkingHours).ToString();

            UpdateActivityList();

            RefreshDayBalanceLabel();
            TimeSpan tb = calculateTotalBalance();
            lblTotalBalance.Text = "Total Balance (w/o Today HH:MM): " + FormatTimeSpan(tb);

            StampCalendar.RemoveAllBoldedDates();
            foreach (var day in StampList)
                StampCalendar.AddBoldedDate(day.Day);
            StampCalendar.UpdateBoldedDates();

            StampCalendar.TodayDate = CurrentShown.Day;
            StampCalendar.DateChanged -= StampCalendar_DateChanged;
            StampCalendar.SetDate(CurrentShown.Day);
            StampCalendar.DateChanged += StampCalendar_DateChanged;
            UpdateStatistics();
        }

        private void RefreshStampTextBoxes()
        {
            txtStart.Text = FormatTimeSpan(CurrentShown.Begin);

            if (CurrentShown.End.TotalMinutes != 0)
                txtEnd.Text = FormatTimeSpan(CurrentShown.End);
            else
                txtEnd.Text = "";

            if (CurrentShown.Pause.TotalMinutes != 0)
                txtPause.Text = (int)CurrentShown.Pause.TotalMinutes + "";
            else
                txtPause.Text = "";
        }

        private void RefreshDayBalanceLabel()
        {
            string dayDesc = CurrentShown.Day == DateTime.Today ? "Today" : "Day";

            lblToday.Text = $"{dayDesc}: {CurrentShown.Day.ToShortDateString()}";
            lblTotal.Text = $"{dayDesc} Balance:";
            txtCurrentShownTotal.Text = FormatTimeSpan(CurrentShown.DayBalance);
        }

        private static bool TryParseHHMM(string text, out TimeSpan value)
        {
            if (HHMM.IsMatch(text))
            {
                int hours = Convert.ToInt32(text.Substring(0, text.IndexOf(":")));
                int minutes = Convert.ToInt32(text.Substring(text.IndexOf(":") + 1));

                value = new TimeSpan(hours, minutes, 0);
                return true;
            }

            value = TimeSpan.Zero;
            return false;
        }

        // Current Stamp Input Fields:

        private void txtStart_TextChanged(object sender, EventArgs e)
        {
            if (TryParseHHMM(txtStart.Text, out TimeSpan value))
            {
                CurrentShown.SetBegin(value);
                UpdateActivityList();
                RefreshDayBalanceLabel();
            }
        }
        private void txtEnd_TextChanged(object sender, EventArgs e)
        {
            if (TryParseHHMM(txtEnd.Text, out TimeSpan value))
            {
                if (value >= GetNowTime() + TimeSpan.FromMinutes(m_minuteThresholdToShowNotification))
                    m_endingPopupShownLastTime = default(DateTime);
                CurrentShown.SetEnd(value);
                UpdateActivityList();
            }
            else if (String.IsNullOrEmpty(txtEnd.Text))
            {
                // TODO: can this cause problems with the activities???
                CurrentShown.End = TimeSpan.Zero;
                m_endingPopupShownLastTime = default(DateTime);
            }
            RefreshDayBalanceLabel();
        }
        private void txtPause_TextChanged(object sender, EventArgs e)
        {
            if (Integer.IsMatch(txtPause.Text))
                return;
            if (!int.TryParse(txtPause.Text, out int pause))
                return;
            CurrentShown.SetPause(new TimeSpan(0, pause, 0));
            UpdateActivityList();
            RefreshDayBalanceLabel();
        }
        private void txtComment_TextChanged(object sender, EventArgs e)
        {
            CurrentShown.Comment = txtComment.Text;
        }
        private void txtWorkingHours_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtWorkingHours.Text))
                return;
            if (Integer.IsMatch(txtWorkingHours.Text))
                return;
            CurrentShown.WorkingHours = Convert.ToInt32(txtWorkingHours.Text);
            RefreshDayBalanceLabel();
        }

        // Current Stamp Activity Grid:

        private void UpdateActivityList()
        {
            grdActivities.Rows.Clear();
            grdActivities.Columns.Clear();

            if (cbActivityDetails.Checked)
            {
                grdActivities.AllowUserToAddRows = true;
                grdActivities.AllowUserToDeleteRows = false;

                grdActivities.Columns.Add(new DataGridViewComboBoxColumn()
                {
                    HeaderText = "Activity",
                    FlatStyle = FlatStyle.Flat,
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                    Width = 66,
                    DataSource = TrackedActivities.Concat(new[] { "[DELETE ENTRY]" }).ToArray()
                });
                grdActivities.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Start", Width = 60 });
                grdActivities.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "End", Width = 76 });
                var isPauseCol = new DataGridViewCheckBoxColumn() { HeaderText = "Pause", ReadOnly = true, Width = 60, FalseValue = false, TrueValue = true, ToolTipText = "Indicates, whether a pause time is between this activity and its predecessor." };
                isPauseCol.DefaultCellStyle.ForeColor = SystemColors.ControlDarkDark;
                isPauseCol.FlatStyle = FlatStyle.Flat;
                grdActivities.Columns.Add(isPauseCol);
                var hoursCol = new DataGridViewTextBoxColumn() { HeaderText = "Hours", ReadOnly = true, Width = 60 };
                hoursCol.DefaultCellStyle.ForeColor = SystemColors.ControlDarkDark;
                grdActivities.Columns.Add(hoursCol);
                grdActivities.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Comment", Width = 76 });

                ActivityRecord previous = null;
                foreach (var activity in CurrentShown.ActivityRecords.OrderBy(a => a.Begin.Value))
                {
                    int index = grdActivities.Rows.Add(
                        activity.Activity,
                        FormatTimeSpan(activity.Begin.Value),
                        activity.End.HasValue ? FormatTimeSpan(activity.End.Value) : String.Empty,
                        previous != null && previous.End.HasValue && previous.End.Value < activity.Begin.Value,
                        FormatTimeSpan(activity.Total),
                        activity.Comment);

                    grdActivities.Rows[index].Tag = activity;

                    previous = activity;
                }
            }
            else
            {
                grdActivities.AllowUserToAddRows = false;
                grdActivities.AllowUserToDeleteRows = false;

                grdActivities.Columns.Add(new DataGridViewButtonColumn() { HeaderText = "Start", FlatStyle = FlatStyle.Flat, Text = "▶", ToolTipText = "Start activity", UseColumnTextForButtonValue = true, Resizable = DataGridViewTriState.False, AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader, Width = 35 });
                grdActivities.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Activity", ReadOnly = true, Width = 66 });
                grdActivities.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Hours", ReadOnly = true, Width = 60 });
                grdActivities.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Comment", Width = 76 });

                grdActivities.Columns[0].Visible = CurrentShown.Day == DateTime.Today;
                foreach (var activity in TrackedActivities)
                {
                    if (CurrentShown.ActivityRecords.Any(r => r.Activity == activity))
                    {
                        var activityKind = CurrentShown.ActivityRecords.Where(r => r.Activity == activity);
                        var totalActivityTime = TimeSpan.FromMinutes(activityKind.Sum(a => a.Total.TotalMinutes));
                        int index = grdActivities.Rows.Add("", activity, FormatTimeSpan(totalActivityTime), activityKind.Last().Comment);
                        grdActivities.Rows[index].Tag = activity;
                    }
                    else
                    {
                        int index = grdActivities.Rows.Add("", activity, FormatTimeSpan(TimeSpan.Zero), String.Empty);
                        grdActivities.Rows[index].Tag = activity;
                        foreach (DataGridViewCell cell in grdActivities.Rows[index].Cells)
                            cell.Style.ForeColor = Color.Gray;
                    }
                }
            }

            HighlightCurrentActivity();

            var stampTime = CurrentShown.DayTime;
            var activityTime = TimeSpan.FromMinutes(CurrentShown.ActivityRecords.Sum(r => r.Total.TotalMinutes));
            bool isMatchingTimeStamps = stampTime == activityTime;

            if (isMatchingTimeStamps)
            {
                grdActivities.GridColor = SystemColors.ControlDark;
                lblActivityWarning.Visible = false;
            }
            else
            {
                grdActivities.GridColor = Color.Red;
                lblActivityWarning.Visible = true;

                string tooltipError = isMatchingTimeStamps ? null : $"The sum value of the day stamps ({stampTime}) does not match with the sum value of the activities ({activityTime}).";
                toolTip1.SetToolTip(lblActivityWarning, tooltipError);
            }

        }

        private void HighlightCurrentActivity()
        {
            // reset all highlighting:
            foreach (DataGridViewRow row in grdActivities.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = grdActivities.DefaultCellStyle.BackColor;
                }
            }

            if (CurrentShown != null && CurrentShown.Day == DateTime.Today)
            {
                // highlight current:
                DataGridViewRow currentRow;
                if (cbActivityDetails.Checked)
                    currentRow = grdActivities.Rows.OfType<DataGridViewRow>().FirstOrDefault(r => r.Tag as ActivityRecord == TodayCurrentActivity);
                else
                    currentRow = grdActivities.Rows.OfType<DataGridViewRow>().FirstOrDefault(r => r.Cells[1].Value as string == TodayCurrentActivity.Activity);

                if (currentRow != null)
                {
                    foreach (DataGridViewCell cell in currentRow.Cells)
                        cell.Style.BackColor = Color.LightGreen;
                }
            }
        }

        private void cbActivityDetails_CheckedChanged(object sender, EventArgs e)
        {
            UpdateActivityList();
        }

        private void grdActivities_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!cbActivityDetails.Checked)
            {
                var senderGrid = (DataGridView)sender;

                if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
                {
                    // start activity button clicked:
                    StartNewActivity(grdActivities.Rows[e.RowIndex].Cells[1].Value as string, null);
                    HighlightCurrentActivity();
                    foreach (DataGridViewCell cell in grdActivities.Rows[e.RowIndex].Cells)
                        cell.Style.ForeColor = grdActivities.DefaultCellStyle.ForeColor;
                }
            }
        }

        private void grdActivities_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            var currentRow = grdActivities.Rows[e.RowIndex];

            if (cbActivityDetails.Checked)
            {
                var currentActivity = grdActivities.Rows[e.RowIndex].Tag as ActivityRecord;
                if (currentActivity == null)
                {
                    // added new row:
                    currentActivity = new ActivityRecord();
                    grdActivities.Rows[e.RowIndex].Tag = currentActivity;
                    // do not yet add to the activity list -- instead, wait until there is a start time, and then add it.
                }
                var text = grdActivities.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;

                if (e.ColumnIndex == 0)
                {
                    //change name (currently only allowed if activity exists)
                    if (TrackedActivities.Any(a => a == text))
                        currentActivity.Activity = text;
                    else if (text == "[DELETE ENTRY]")
                    {
                        CurrentShown.ActivityRecords.Remove(currentActivity);
                        RefreshStampTextBoxes();
                        UpdateActivityList();
                        return;
                    }
                    else
                        grdActivities.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = String.Empty;

                }
                else if (e.ColumnIndex == 1)
                {
                    // change start
                    if (TryParseHHMM(text, out TimeSpan value))
                    {
                        // add new, pending activity entry if applicable:
                        if (!CurrentShown.ActivityRecords.Contains(currentActivity))
                            CurrentShown.ActivityRecords.Add(currentActivity);

                        // first activity start changed -> also change days start stamp
                        if (e.RowIndex == 0)
                        {
                            CurrentShown.Begin = value;
                        }
                        // in between start changed -> also change end of previous activity, if they previously matched
                        else
                        {
                            int index = e.RowIndex - 1;

                            bool isIterating;
                            do
                            {
                                isIterating = false;
                                var previousActivity = grdActivities.Rows[index].Tag as ActivityRecord;
                                if (!previousActivity.End.HasValue || previousActivity.End.Value >= value)
                                {
                                    previousActivity.End = value;
                                    // activity is hidden / negative after change -> remove activity
                                    if (previousActivity.Total < TimeSpan.Zero)
                                    {
                                        CurrentShown.ActivityRecords.Remove(previousActivity);
                                        if (index == 0)
                                        {
                                            // removed first stamp -> also set stamp begin
                                            CurrentShown.Begin = value;
                                        }
                                        index--;
                                        isIterating = true;
                                    }
                                }
                            } while (index >= 0 && isIterating);
                        }
                        currentActivity.Begin = value;

                        // pause interruption gap(s) is/are changed -> also change day pause stamp
                        CurrentShown.CalculatePauseFromActivities();

                        RefreshStampTextBoxes();
                        UpdateActivityList();
                    }
                }
                else if (e.ColumnIndex == 2)
                {
                    // change end
                    if (TryParseHHMM(text, out TimeSpan value))
                    {
                        // last activity end changed -> also change days end stamp
                        if (e.RowIndex == grdActivities.Rows.Count - 2 /*AllowUserToAddRows also results in an additional line*/)
                        {
                            CurrentShown.End = value;
                        }
                        // in between end changed -> also change start of next activity, if they previously matched
                        else
                        {
                            int index = e.RowIndex + 1;

                            bool isIterating;
                            do
                            {
                                isIterating = false;
                                var nextActivity = grdActivities.Rows[index].Tag as ActivityRecord;
                                if (nextActivity.Begin <= value)
                                {
                                    nextActivity.Begin = value;
                                    // activity is hidden / negative after change -> remove activity
                                    if (nextActivity.Total < TimeSpan.Zero)
                                    {
                                        CurrentShown.ActivityRecords.Remove(nextActivity);
                                        if (index == grdActivities.Rows.Count - 2)
                                        {
                                            // removed last stamp -> also set stamp end
                                            CurrentShown.End = value;
                                        }
                                        index++;
                                        isIterating = true;
                                    }
                                }
                            } while (index <= grdActivities.Rows.Count - 2 && isIterating);
                        }
                        currentActivity.End = value;

                        // pause interruption gap(s) is/are changed -> also change day pause stamp
                        CurrentShown.CalculatePauseFromActivities();

                        RefreshStampTextBoxes();
                        UpdateActivityList();
                    }
                }
                else if (e.ColumnIndex == 3)
                {
                    // follows pause value is readonly
                }
                else if (e.ColumnIndex == 4)
                {
                    // total value is readonly
                }
                else if (e.ColumnIndex == 5)
                {
                    // change comment
                    currentActivity.Comment = text;
                }
            }
            else
            {
                var currentActivity = grdActivities.Rows[e.RowIndex].Cells[1].Value as string;

                if (e.ColumnIndex == 1)
                {
                    // activity name is readonly
                }
                else if (e.ColumnIndex == 2)
                {
                    // activity time is readonly
                }
                else if (e.ColumnIndex == 3)
                {
                    // set comment of last activity:
                    var activity = CurrentShown.GetLastActivity(currentActivity);
                    if (activity != null)
                    {
                        activity.Comment = grdActivities.Rows[e.RowIndex].Cells[3].Value as string;
                    }
                    else
                    {
                        grdActivities.Rows[e.RowIndex].Cells[3].Value = String.Empty;
                    }
                }
            }
        }

        // Stamp Calendar:

        private void StampCalendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            //get newly selected stamp:
            var selectedStamp = StampList.SingleOrDefault(s => s.Day == StampCalendar.SelectionStart);
            if (selectedStamp == null)
            {
                // unoccupied date selected:
                // show 'Add' button:
                btnAddTimestamp.Visible = true;
            }
            else
            {
                btnAddTimestamp.Visible = false;
                CurrentShown = selectedStamp;
                refreshControls();
            }
        }

        private void btnAddTimestamp_Click(object sender, EventArgs e)
        {
            btnAddTimestamp.Visible = false;

            CurrentShown = new Stamp() { Day = StampCalendar.SelectionStart };
            StampList.Add(CurrentShown);

            refreshControls();
        }

        // Chart:

        private StatisticTypes StatisticType = StatisticTypes.TimeInLieu;
        private enum StatisticTypes
        {
            TimeInLieu,
            Activities,
            WeeklyActivities,
        };

        private StatisticRanges StatisticRange = StatisticRanges.Ever;
        private enum StatisticRanges
        {
            Ever,

            RecentYear,
            RecentTerm,
            RecentQuarter,
            RecentMonth,
            RecentWeek,

            SelectedYear,
            SelectedMonth,
            SelectedWeek,
            SelectedDay,
        };

        private void UpdateStatistics()
        {

            //Update Statistics Chart:
            chart1.ChartAreas.Clear();
            chart1.ChartAreas.Add(new ChartArea());
            chart1.Series.Clear();
            chart1.Legends.Clear();
            lblStatisticValues.Text = String.Empty;

            var Statistics = new Series("Timestamp Statistics");

            if (StatisticType == StatisticTypes.TimeInLieu)
            {
                Statistics.ChartType = SeriesChartType.Column;

                Statistics.IsXValueIndexed = true;
                Statistics.XValueType = ChartValueType.DateTime;
                Statistics.YValueType = ChartValueType.Double;

                var timeRangeStamps = GetTimeStampsInRange(false);

                if (timeRangeStamps.Count > 1)
                {
                    double min = calculateTotalBalance(timeRangeStamps.First().Day).TotalHours;
                    double max = min;
                    foreach (var stamp in timeRangeStamps)
                    {
                        var balance = calculateTotalBalance(stamp.Day).TotalHours;
                        Statistics.Points.AddXY(stamp.Day, balance);
                        if (balance < min)
                            min = balance;
                        if (balance > max)
                            max = balance;
                    }

                    string averageBegin = FormatTimeSpan(TimeSpan.FromHours(timeRangeStamps.Average(s => s.Begin.TotalHours)));
                    string averageEnd = FormatTimeSpan(TimeSpan.FromHours(timeRangeStamps.Average(s => s.End.TotalHours)));
                    string averagePause = timeRangeStamps.Average(s => s.Pause.TotalMinutes).ToString("0");
                    string averageTotal = FormatTimeSpan(TimeSpan.FromHours(timeRangeStamps.Select(s => s.DayBalance.TotalHours).Average()));

                    lblStatisticValues.Text = $"ø Begin: {averageBegin} | ø End: {averageEnd} | ø Pause: {averagePause} | ø Total: {averageTotal}";

                    chart1.ChartAreas.First().AxisY.Minimum = Math.Floor(min);
                    chart1.ChartAreas.First().AxisY.Maximum = Math.Ceiling(max);
                    chart1.ChartAreas.First().AxisY.LabelStyle.Format = "0";
                    chart1.ChartAreas.First().AxisY.RoundAxisValues();
                }
            }
            else if (StatisticType == StatisticTypes.Activities)
            {
                Statistics.ChartType = SeriesChartType.Pie;

                Statistics.XValueType = ChartValueType.String;
                Statistics.YValueType = ChartValueType.Double;


                var allActivities = GetTimeStampsInRange(true).SelectMany(s => s.ActivityRecords);

                var totalHoursPerActivity = allActivities.GroupBy(a => a.Activity).ToDictionary(a => a.Key, a => a.Sum(ar => ar.Total.TotalHours));

                var totalHours = totalHoursPerActivity.Values.Sum();
                var percentPerActivity = totalHoursPerActivity.ToDictionary(a => a.Key, a => (a.Value / totalHours) * 100);

                foreach (var act in percentPerActivity)
                {
                    var rounded = Math.Round(act.Value, 0);
                    int index = Statistics.Points.AddXY(act.Key, rounded);
                    Statistics.Points[index].Label = $"{act.Key}: {rounded} %";
                }
            }
            else if (StatisticType == StatisticTypes.WeeklyActivities)
            {
                var allStamps = GetTimeStampsInRange(true).Where(s => s.ActivityRecords.Any());
                var timeRangeStampsPerWeek = allStamps.GroupBy(s => GetWeekOfYearISO8601(s.Day)); // daily: (s => s.Day.Day + "." + s.Day.Month);

                var legend = chart1.Legends.Add("Legend");

                var allActivityNames = allStamps.SelectMany(s => s.ActivityRecords).Select(r => r.Activity).Distinct().ToArray();
                var series = allActivityNames.ToDictionary(a => a, a =>
                {
                    var statistics = new Series(a);
                    statistics.ChartType = SeriesChartType.StackedArea100;
                    statistics.IsXValueIndexed = true;
                    statistics.XValueType = ChartValueType.String;
                    statistics.YValueType = ChartValueType.Double;
                    statistics.LegendText = a.Substring(0, Math.Min(10, a.Length)) + (a.Length > 10 ? "..." : "");
                    statistics.LegendToolTip = a;
                    statistics.IsVisibleInLegend = true;
                    statistics.Legend = "Legend";

                    return statistics;
                });

                if (timeRangeStampsPerWeek.Count() > 1)
                {
                    foreach (var week in timeRangeStampsPerWeek)
                    {
                        var allActivities = week.SelectMany(s => s.ActivityRecords);

                        var totalHoursPerActivity = allActivities.GroupBy(a => a.Activity).ToDictionary(a => a.Key, a => a.Sum(ar => ar.Total.TotalHours));

                        var totalHours = totalHoursPerActivity.Values.Sum();
                        var percentPerActivity = totalHoursPerActivity.ToDictionary(a => a.Key, a => (a.Value / totalHours) * 100);

                        foreach (var act in allActivityNames)
                        {
                            series[act].Points.AddXY($"{week.Key}/{week.First().Day.Year % 100}", percentPerActivity.ContainsKey(act) ? percentPerActivity[act] : 0.0);
                        }
                    }

                    chart1.ChartAreas.First().AxisY.LabelStyle.Format = "0";
                    chart1.ChartAreas.First().AxisY.RoundAxisValues();
                }

                foreach (var stat in series.Values)
                    chart1.Series.Add(stat);
                return;
            }
            else
            {
                throw new NotImplementedException();
            }

            chart1.Series.Add(Statistics);
        }

        private List<Stamp> GetTimeStampsInRange(bool includeToday)
        {
            // Select time stamps according to user selection:
            TimeSpan sinceAgo;

            switch (StatisticRange)
            {
                //case StatisticRanges.CurrentMonth:
                //    timeRangeStamps.AddRange(StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == StampCalendar.SelectionStart.Year && s.Day.Month == StampCalendar.SelectionStart.Month && (includeToday || s.Day != DateTime.Today)));
                //    break;
                //case StatisticRanges.CurrentYear:
                //    timeRangeStamps.AddRange(StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == StampCalendar.SelectionStart.Year && (includeToday || s.Day != DateTime.Today)));
                //    break;
                case StatisticRanges.Ever:
                    return StampList.OrderBy(s => s.Day).Where(s => (includeToday || s.Day != DateTime.Today)).ToList();

                case StatisticRanges.RecentYear:
                    sinceAgo = TimeSpan.FromDays(365);
                    break;
                case StatisticRanges.RecentTerm:
                    sinceAgo = TimeSpan.FromDays(182);
                    break;
                case StatisticRanges.RecentQuarter:
                    sinceAgo = TimeSpan.FromDays(91);
                    break;
                case StatisticRanges.RecentMonth:
                    sinceAgo = TimeSpan.FromDays(30);
                    break;
                case StatisticRanges.RecentWeek:
                    sinceAgo = TimeSpan.FromDays(7);
                    break;

                case StatisticRanges.SelectedYear:
                    return StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == CurrentShown.Day.Year).ToList();
                case StatisticRanges.SelectedMonth:
                    return StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == CurrentShown.Day.Year && s.Day.Month == CurrentShown.Day.Month).ToList();
                case StatisticRanges.SelectedWeek:
                    var targetWeek = GetWeekOfYearISO8601(CurrentShown.Day);
                    return StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == CurrentShown.Day.Year && s.Day.Month == CurrentShown.Day.Month && GetWeekOfYearISO8601(s.Day) == targetWeek).ToList();
                case StatisticRanges.SelectedDay:
                    return new List<Stamp>() { CurrentShown };

                default:
                    throw new NotImplementedException();
            }

            return StampList.OrderBy(s => s.Day).Where(s => s.Day > DateTime.Now.Subtract(sinceAgo)).ToList();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            StatisticType = (StatisticTypes)cmbStatisticType.SelectedIndex;
            UpdateStatistics();
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            StatisticRange = (StatisticRanges)cmbStatisticRange.SelectedIndex;
            UpdateStatistics();
        }

        // Action Buttons:

        private void btnTakeDayOff_Click(object sender, EventArgs e)
        {
            if (CurrentShown.Begin.TotalMinutes != 0 && CurrentShown.End.TotalMinutes != 0)
            {
                var answer = MessageBox.Show("Are you sure? You already have a Stamp for that day! This will overwrite the whole stamp.", "Take Day Off?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (answer == System.Windows.Forms.DialogResult.No)
                    return;
            }
            CurrentShown.Begin = new TimeSpan(8, 0, 0);
            CurrentShown.End = new TimeSpan(8, 0, 0);
            CurrentShown.Pause = new TimeSpan(0);
            CurrentShown.ActivityRecords.Clear();
            refreshControls();
        }

        private void btnDeleteStamp_Click(object sender, EventArgs e)
        {
            if (CurrentShown != null)
            {
                int index = StampList.IndexOf(CurrentShown);
                StampList.Remove(CurrentShown);
                CurrentShown = StampList.Count > index ? StampList.ElementAt(index) : StampList.ElementAt(index - 1);
                refreshControls();
            }
        }

        private void btnExportExcelActivities_Click(object sender, EventArgs e)
        {
            var years = GetExportableYears();

            var menu = new ContextMenuStrip();

            foreach (var year in years)
            {
                menu.Items.Add(new ToolStripMenuItem(year.ToString(), null, (ss, ee) =>
                {
                    int exportYear = (int)(((ToolStripMenuItem)ss).Tag);
                    CreateExcel(exportYear);
                })
                { Tag = year });
            }

            var button = (Button)sender;

            menu.Show(button, new Point(0, button.Height));
        }

        private void btnManageActivities_Click(object sender, EventArgs e)
        {
            var diag = new ManageActivities();
            diag.ShowDialog(this);

            refreshControls();
        }

        // Create Excel Export:

        private int[] GetExportableYears()
        {
            return StampList.Select(s => s.Day.Year).Distinct().OrderByDescending(y => y).ToArray();
        }

        private void CreateExcel(int year)
        {
            ExcelPackage excel = new ExcelPackage();

            for (int i = 1; i <= 12; i++)
            {
                CreateExcelSheet(excel, year, i);

                if (year == DateTime.Today.Year && i == DateTime.Today.Month)
                    break;
            }

            var sfd = new SaveFileDialog();
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            sfd.FileName = $"Zeiterfassung {year}.xlsx";
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                excel.SaveAs(new FileInfo(sfd.FileName));
                Process.Start(sfd.FileName);
            }
        }

        private void CreateExcelSheet(ExcelPackage excel, int year, int month)
        {
            var sheet = excel.Workbook.Worksheets.Add($"{new DateTime(year, month, 1).ToString("MMM")} {year}");

            // write header texts:
            sheet.Cells[1, 1].Value = "Tag";
            sheet.Cells[1, 2].Value = "Projektst.";
            int column = 3;
            foreach (var activity in TrackedActivities)
                sheet.Cells[1, column++].Value = activity;

            int endColumn = column - 1;

            // gray background:
            sheet.Cells[1, 1, 1, endColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[1, 1, 1, endColumn].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            // properly sized:
            sheet.Row(1).Height = 30;
            for (int i = 1; i <= endColumn; i++)
            {
                sheet.Column(i).Width = 24;
                // border:
                sheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);
                // allow line breaks:
                sheet.Cells[1, i].Style.WrapText = true;
                // alignment:
                sheet.Cells[1, i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet.Cells[1, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // write table body:
            int row = 2;
            var inclusiveMin = new DateTime(year, month, 1);
            var exclusiveMax = new DateTime(month == 12 ? year + 1 : year, month == 12 ? 1 : month + 1, 1);
            foreach (var stamp in StampList.Where(s => s.Day >= inclusiveMin && s.Day < exclusiveMax).OrderBy(s => s.Day).ToArray())
            {
                sheet.Cells[row, 1].Value = stamp.Day.ToString("dd.MM.yyyy");
                sheet.Cells[row, 2].Formula = $"=SUM(C{row}:H{row})"; //GetTimeForExcelCell(stamp);

                column = 3;
                foreach (var activity in TrackedActivities)
                    sheet.Cells[row, column++].Value = GetTimeForExcelCell(stamp, activity);

                // formatting:
                sheet.Cells[row, 2, row, endColumn].Style.Numberformat.Format = "0.00";

                // border:
                sheet.Cells[row, 2].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                sheet.Cells[row, 2].Style.Border.Right.Color.SetColor(Color.Black);

                row++;
            }

            int summaryRow = 24;

            // keep drawing border down to summary row:
            for (int i = row; i < summaryRow; i++)
            {
                sheet.Cells[i, 2].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                sheet.Cells[i, 2].Style.Border.Right.Color.SetColor(Color.Black);
            }

            // write footer summary line formulas:
            row = summaryRow;
            sheet.Cells[row, 1].Formula = "=COUNTA(A2:A21)";
            for (int i = 2; i <= endColumn; i++)
                sheet.Cells[row, i].Formula = $"=SUM({ExcelAddress.GetAddressCol(i)}2:{ExcelAddress.GetAddressCol(i)}21)";

            // formatting:
            sheet.Cells[row, 2, row, endColumn].Style.Numberformat.Format = "0.00";

            // gray background:
            sheet.Cells[row, 1, row, endColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[row, 1, row, endColumn].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            // border:
            for (int i = 1; i <= endColumn; i++)
            {
                sheet.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);
            }
        }

        private double? GetTimeForExcelCell(Stamp stamp, string activity = null)
        {
            IEnumerable<ActivityRecord> activities = stamp.ActivityRecords;

            if (activity != null)
                activities = activities.Where(r => r.Activity == activity);

            var span = TimeSpan.FromMinutes(activities.Sum(r => r.Total.TotalMinutes));

            if (span == TimeSpan.Zero)
                return null; // String.Empty;

            var hours = Math.Floor(span.TotalHours);
            var fractional = span.TotalHours - hours;
            // Minuten runden auf 
            // 0.25 (viertelstunde): >= 7.5 && < 22.5
            // 0.50 (halbestunde): >= 22.5 && < 
            // 0.75 (dreiviertelstunde):
            // 0.00 (ganze stunde):
            if (fractional <= 0.125 || fractional > 0.875)
                fractional = 0;
            else if (fractional <= 0.375)
                fractional = 0.25;
            else if (fractional <= 0.625)
                fractional = 0.50;
            else if (fractional <= 0.875)
                fractional = 0.75;

            return hours + fractional;
            //return span.ToString("hh\\:mm");
        }

        // TrayIcon:

        private void CreateOrUpdateTrayIconContextMenu()
        {
            if (Today == null)
                return;

            var menu = notifyIcon1.ContextMenuStrip ?? new ContextMenuStrip();
            foreach (var activity in TrackedActivities)
            {
                string displayText;
                Color foreColor;
                if (Today.ActivityRecords.Any(r => r.Activity == activity))
                {
                    var activityKind = CurrentShown.ActivityRecords.Where(r => r.Activity == activity);
                    var totalActivityTime = TimeSpan.FromMinutes(activityKind.Sum(a => a.Total.TotalMinutes));

                    displayText = FormatTimeSpan(totalActivityTime) + " " + activity;
                    foreColor = Color.Black;
                }
                else
                {
                    displayText = FormatTimeSpan(TimeSpan.Zero) + " " + activity;
                    foreColor = Color.Gray;
                }

                bool isCurrentlyActive = TodayCurrentActivity?.Activity == activity;

                var bmp = new Bitmap(16, 16);
                var grc = Graphics.FromImage(bmp);
                grc.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                grc.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                grc.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                grc.DrawString("▶", new Font("Arial", 15, FontStyle.Bold), Brushes.Black, new PointF(0, -2));

                var existing = menu.Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => i.Tag as string == activity);

                if (existing == null)
                {
                    menu.Items.Add(new ToolStripMenuItem(displayText, isCurrentlyActive ? bmp : null, (ss, ee) =>
                    {
                        string newActivity = ((ToolStripMenuItem)ss).Tag as string;
                        StartNewActivity(newActivity, null);
                        PopupDialog.ShowCurrentlyTrackingActivity(this, newActivity);
                    })
                    { Tag = activity, ForeColor = foreColor });
                }
                else
                {
                    existing.Text = displayText;
                    existing.Image = isCurrentlyActive ? bmp : null;
                    existing.ForeColor = foreColor;
                }
            }

            if (notifyIcon1.ContextMenuStrip == null)
                notifyIcon1.ContextMenuStrip = menu;
        }

        private void notifyIcon1_Click_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.ShowInTaskbar = true;
                this.notifyIcon1.Visible = false;
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                //RefreshDayBalanceLabel();
                refreshControls();
            }
            if (e.Button == MouseButtons.Right)
            {
                CreateOrUpdateTrayIconContextMenu();
            }
        }

        private void Form1_Resize(object sender, System.EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
                this.notifyIcon1.Visible = true;
            }
            else
            {
                lblTotalBalance.MaximumSize = groupBox1.Size;
            }
        }

        #endregion


        #region Automatic Pause Recognition
        private MouseHookListener MouseHook;
        private TimeSpan lastMouseMove;
        private Timer pauseSpanRecognizer;
        //private bool isWaiting = false;

        private void MouseHook_MouseMoveExt(object sender, MouseEventExtArgs e)
        {
            if (lastMouseMove.TotalMinutes != 0 && DateTime.Now.TimeOfDay.Subtract(lastMouseMove).TotalMinutes >= AutomaticPauseRecognitionMinPauseTime)
            {
                Log("Mouse movement after pause: Today: " + Today + ", Activity: " + TodayCurrentActivity);
                Today.Pause = GetNowTime() - GetTime(lastMouseMove);
                var current = TodayCurrentActivity;
                TodayCurrentActivity.End = GetTime(lastMouseMove);
                TodayCurrentActivity = null;
                StartNewActivity(current.Activity, current.Comment + " Resuming after pause...");

                if (MouseHook != null)
                    MouseHook.Enabled = false;
                //isWaiting = false;
                refreshControls();
                new System.Threading.Tasks.Task(() =>
                {
                    System.Threading.Thread.Sleep(10000);
                    this.Invoke(new Action(() => { PopupDialog.ShowAfterPause(this, Today.Pause, current.Activity); }));
                }).Start();
                return;
            }
            lastMouseMove = DateTime.Now.TimeOfDay;
            //isWaiting = true;
        }

        private int m_minuteThresholdToShowNotification = 5;
        private DateTime m_endingPopupShownLastTime;
        private void middaySpan_Tick(object sender, EventArgs e)
        {
            if ((m_endingPopupShownLastTime == default(DateTime) || m_endingPopupShownLastTime.Date != Today.Day.Date) && Today.Day.Date == DateTime.Today)
            {
                if (Today.End == TimeSpan.Zero && Today.DayBalance >= TimeSpan.FromMinutes(-m_minuteThresholdToShowNotification))
                {
                    this.Invoke(new Action(() =>
                    {
                        m_endingPopupShownLastTime = DateTime.Now;
                        PopupDialog.Show8HrsIn5Minutes(this, DateTime.Today + Today.Begin + Today.Pause + TimeSpan.FromHours(Today.WorkingHours));
                    }));
                }
                else if (Today.End != TimeSpan.Zero && DateTime.Now + TimeSpan.FromMinutes(m_minuteThresholdToShowNotification) >= DateTime.Today + Today.End)
                {
                    this.Invoke(new Action(() =>
                    {
                        m_endingPopupShownLastTime = DateTime.Now;
                        PopupDialog.ShowPlannedEndIn5Minutes(this, DateTime.Today + Today.End);
                    }));
                }
            }


            if (!automaticPauseRecognition)
                return;
            if (Today.Pause != TimeSpan.Zero)
                return;

            if ((DateTime.Now.TimeOfDay >= AutomaticPauseRecognitionStartTime) && (DateTime.Now.TimeOfDay <= AutomaticPauseRecognitionStopTime))
            {
                if (MouseHook == null)
                {
                    MouseHook = new MouseHookListener(new GlobalHooker());
                    MouseHook.MouseMoveExt += MouseHook_MouseMoveExt;
                }

                if (MouseHook != null && !MouseHook.Enabled)
                {
                    MouseHook.Enabled = true;
                    //isWaiting = true;
                    //MouseHook.MouseMoveExt -= MouseHook_MouseMoveExt;
                    //MouseHook.Dispose();
                    //MouseHook = null;
                }
                //actively looking for idle
            }
            else
            {
                //if not waiting for user to come back from afk, stop looking for idle
                if (/*isWaiting == false && */MouseHook != null && MouseHook.Enabled)
                {
                    //MouseHook.MouseMoveExt -= MouseHook_MouseMoveExt;
                    MouseHook.Enabled = false;
                    //MouseHook.Dispose();
                    //MouseHook = null;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                automaticPauseRecognition = true;
                //pauseSpanRecognizer = new Timer() { Interval = 5000, Enabled = true };
                //pauseSpanRecognizer.Tick += new EventHandler(middaySpan_Tick);
            }
            else
            {
                automaticPauseRecognition = false;
                //if(pauseSpanRecognizer != null)
                //    pauseSpanRecognizer.Enabled = false;
            }
        }
        #endregion


        #region Calculations

        private string AtLeastTwoDigits(int time)
        {
            var abs = Math.Abs(time);
            return (abs < 10 ? "0" + abs : "" + abs);
        }
        private string FormatTimeSpan(TimeSpan span)
        {
            return (span < TimeSpan.Zero ? "-" : "")
                + AtLeastTwoDigits((int)Math.Floor(Math.Abs(span.TotalHours)))
                + ":" + AtLeastTwoDigits(span.Minutes);
        }
        private TimeSpan calculateTotalBalance()
        {
            TimeSpan totalBalance = new TimeSpan(0);
            foreach (var stamp in StampList)
            {
                if (stamp.Day == DateTime.Today)
                    continue;
                totalBalance = totalBalance.Add(stamp.DayBalance);
            }
            return totalBalance;
        }
        private TimeSpan calculateTotalBalance(DateTime CalculateEndDate)
        {
            TimeSpan totalBalance = new TimeSpan(0);
            var StampRange = StampList.Where(s => s.Day.Date <= CalculateEndDate);
            foreach (var stamp in StampRange)
            {
                if (stamp.Day == DateTime.Today)
                    continue;
                totalBalance = totalBalance.Add(stamp.DayBalance);
            }
            return totalBalance;
        }

        public static TimeSpan GetNowTime()
        {
            return GetTime(DateTime.Now.TimeOfDay);
        }

        public static TimeSpan GetTime(TimeSpan accurate)
        {
            return new TimeSpan(accurate.Hours, accurate.Minutes, 0);
        }

        #endregion

        #region Misc

        private string getFullExceptionMessage(Exception e)
        {
            Exception ie = e;
            string eMsg = ie.Message;
            while (ie.InnerException != null)
            {
                ie = ie.InnerException;
                eMsg += "\r\n" + ie.Message;
            }
            return eMsg;
        }

        private static int GetWeekOfYearISO8601(DateTime time)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        #endregion

        //#region Notebook Lid Open/Close Event

        //[DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        //private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);

        //internal struct POWERBROADCAST_SETTING
        //{
        //    public Guid PowerSetting;
        //    public uint DataLength;
        //    public byte Data;
        //}

        //Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
        //const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        //const int WM_POWERBROADCAST = 0x0218;
        //const int PBT_POWERSETTINGCHANGE = 0x8013;

        //private bool? _previousLidState = null;

        //protected override void WndProc(ref Message m)
        //{
        //    switch (m.Msg)
        //    {
        //        case WM_POWERBROADCAST:
        //            Log("OnPowerBroadcast-Message...");
        //            OnPowerBroadcast(m.WParam, m.LParam);
        //            break;
        //        default:
        //            break;
        //    }

        //    base.WndProc(ref m);
        //}

        //private void RegisterForPowerNotifications()
        //{
        //    IntPtr handle = this.Handle;
        //    IntPtr hLIDSWITCHSTATECHANGE = RegisterPowerSettingNotification(handle, ref GUID_LIDSWITCH_STATE_CHANGE, DEVICE_NOTIFY_WINDOW_HANDLE);
        //}

        //private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
        //{
        //    if ((int)wParam == PBT_POWERSETTINGCHANGE)
        //    {
        //        POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
        //        IntPtr pData = (IntPtr)((int)lParam + Marshal.SizeOf(ps));
        //        Int32 iData = (Int32)Marshal.PtrToStructure(pData, typeof(Int32));
        //        if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
        //        {
        //            bool isLidOpen = ps.Data != 0;

        //            if (!isLidOpen == _previousLidState)
        //            {
        //                LidStatusChanged(isLidOpen);
        //            }

        //            _previousLidState = isLidOpen;
        //        }
        //    }
        //}

        //private void LidStatusChanged(bool isLidOpen)
        //{
        //    if (isLidOpen)
        //    {
        //        Log("Notebook Lid opened...");

        //        if (TodayCurrentActivity.Activity == "Meetings")
        //        {
        //            // maybe returning from a meeting? notify that meeting activity is still running
        //            // TODO: show button in dialog with previous activity (before Meeting) to easily change back...
        //            var allPreviousActivities = Today.ActivityRecords.Where(r => r.Begin.Value < TodayCurrentActivity.Begin.Value).OrderBy(r => r.Begin.Value).Reverse().ToList();
        //            var previousActivity = allPreviousActivities.FirstOrDefault(a => a.Activity != "Meetings");
        //            if (previousActivity != null)
        //                PopupDialog.ShowCurrentlyTrackingActivityWithChangeToActivityButton(this, TodayCurrentActivity.Activity, previousActivity.Activity);
        //            else
        //                PopupDialog.ShowCurrentlyTrackingActivity(this, TodayCurrentActivity.Activity);
        //        }
        //        else
        //        {
        //            // maybe coming into a meeting? notify about current activity
        //            // TODO: show button in dialog with 'Meeting' to easily change...
        //            PopupDialog.ShowCurrentlyTrackingActivityWithChangeToActivityButton(this, TodayCurrentActivity.Activity, "Meetings");
        //        }
        //    }
        //    else
        //    {
        //        Log("Notebook Lid closed...");
        //    }
        //}

        //#endregion

        #region XML-IO

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

    }
}
