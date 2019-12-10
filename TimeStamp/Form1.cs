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
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace TimeStamp
{
    public partial class Form1 : Form
    {
        // TODO:

        // LOW PRIO:

        // advanced features to ask whether current activity is still correct,
        //      e.g. after notebook hatch been closed and reopened -> Meeting or Activity before Meeting? https://stackoverflow.com/questions/3355606/detect-laptop-lid-closure-and-opening
        //           -> this is actually identical to lock/unlock, as the default action probably shouldnt be 'do nothing' anyway... (also, the event does not fire reliably...)
        //      e.g. upon certain app start / changes, 
        //      e.g. PKI card inserted / removed, https://cgeers.wordpress.com/2008/02/03/monitoring-a-smartcard-reader/ or http://forums.codeguru.com/showthread.php?510947-How-to-detect-smart-card-reader-insertion
        //      e.g. wifi network changed,
        //      etc...?

        public TimeSettings Settings { get; private set; }
        public TimeManager Manager { get; private set; }


        public List<Stamp> StampList => Manager.StampList;
        public Stamp CurrentShown => Manager.CurrentShown;
        public Stamp Today => Manager.Today;
        public ActivityRecord TodayCurrentActivity => Manager.TodayCurrentActivity;
        public string FormatTimeSpan(TimeSpan tb) => Manager.FormatTimeSpan(tb);

        // High Level Events:

        public Form1()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Log.Add("Unhandled Exception: " + (e.ExceptionObject is Exception ? (e.ExceptionObject as Exception)?.GetFullExceptionMessage() : e.ExceptionObject.ToString()));
            };

            Settings = new TimeSettings();
            Settings.LoadSettings();

            Manager = new TimeManager(Settings);

            // needs to be called after 'Manager' has been set, and before Manager.Initialize():
            PopupDialog.Initialize(this);

            try
            {
                Manager.Initialize();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.GetFullExceptionMessage());
                Application.Exit();
            }

            Manager.CurrentActivityUpdated += () =>
            {
                // refresh grid to highlight the current activity with green background:
                HighlightCurrentActivity();
                // refresh context menu on tray icon:
                CreateOrUpdateTrayIconContextMenu();
            };


            // initialize controls:


            btnAddTimestamp.BringToFront();

            btnDeleteStamp.Click += new EventHandler(btnDeleteStamp_Click);
            btnTakeDayOff.Click += new EventHandler(btnTakeDayOff_Click);
            StampCalendar.DateChanged += StampCalendar_DateChanged;

            foreach (var statType in Enum.GetNames(typeof(TimeSettings.StatisticTypes)))
                cmbStatisticType.Items.Add(statType);

            foreach (var statRange in Enum.GetNames(typeof(TimeSettings.StatisticRanges)))
                cmbStatisticRange.Items.Add(statRange);

            cmbStatisticType.SelectedIndexChanged += new EventHandler(comboBox1_SelectedIndexChanged);
            cmbStatisticRange.SelectedIndexChanged += new EventHandler(comboBox2_SelectedIndexChanged);

            lblTotalBalance.MaximumSize = groupBox1.Size;
            RefreshControls();

            m_timer = new Timer() { Interval = 5000, Enabled = true };
            m_timer.Tick += new EventHandler(Timer_Tick);

            // data bind control values to settings:

            cmbStatisticType.DataBindings.Add(new Binding(nameof(ComboBox.SelectedIndex), Settings, nameof(Settings.StatisticType)));
            cmbStatisticRange.DataBindings.Add(new Binding(nameof(ComboBox.SelectedIndex), Settings, nameof(Settings.StatisticRange)));

            if (Settings.WindowWidth > this.MinimumSize.Width)
                this.Width = Settings.WindowWidth;
            if (Settings.WindowHeight > this.MinimumSize.Height)
                this.Height = Settings.WindowHeight;

            this.SizeChanged += Form1_SizeChanged;
            chart1.SizeChanged += Chart1_SizeChanged;


            // enable events for system sleep/standby/resume and OS log on/off:
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            SystemEvents.SessionEnded += SystemEvents_SessionEnded;
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            SystemEvents.EventsThreadShutdown += SystemEvents_EventsThreadShutdown;



            // enable events for notebook lid opening/closing:
            //RegisterForPowerNotifications();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.WindowWidth = this.Width;
                Settings.WindowHeight = this.Height;
            }
        }

        private void SystemEvents_EventsThreadShutdown(object sender, EventArgs e)
        {
            Log.Add($"(EventsThreadShutdown)");

            Manager.SuspendStamping();
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            Log.Add($"(SessionEnding: {e.Reason.ToString()})");

            Manager.SuspendStamping();
        }

        private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            Log.Add($"(SessionEnded: {e.Reason.ToString()})");

            Manager.SuspendStamping();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.SaveSettings();

            Manager.SuspendStamping();

            this.notifyIcon1.Visible = false;
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            Log.Add($"{e.Mode.ToString()}...");

            switch (e.Mode)
            {
                //case PowerModes.Resume:

                //    Log("Resuming from sleep...");

                //    // might be:
                //    // - resuming in the morning of the next working day (Today set, but not correct any more)
                //    // - resuming on the same working day after sleep (Today set and correct)

                //    // does not always fire correctly, see:
                //    // https://stackoverflow.com/questions/51271460/c-sharp-wpf-powermodechanged-doesnt-work-on-surface
                //    // I am also working on such a problem. From what I've read, the Surface supports "Sleep state(Modern Standby)", 
                //    // or S0 low-power, and is not yet in actual sleep state (S1-3). Pressing the power button or clicking the "sleep"
                //    // option from the windows menu does not enter sleep directly but enters S0 low-power instead, thus not triggering PowerModeChanged.
                //    // https://docs.microsoft.com/en-us/windows/desktop/power/system-power-states#sleep-state-modern-standby

                //    ResumeStamping();

                //    refreshControls();

                //    break;

                case PowerModes.Suspend:

                    // when activating 'sleep', there might be a delay between actual trigger 'sleep' and 'lock'-event (whatever causes this... looking at you, DELL!).
                    // this may lead to erroneous, shorter pause times as well as erroneous, later day end stamps (in fact, quite enourmous differences, as in somewhere between 20 minutes and 2 hours).
                    // e.g.:
                    /*
                        06.08.2019 11:09 SessionUnlock...
                        (actually pressed 'sleep' button on keyboard at 11:30)
                        Last Mouse Move: 11:30
                        06.08.2019 11:52 SessionLock...
                        06.08.2019 12:07 SessionUnlock...
                        (too short pause has been tracked!)
                    */
                    /*
                        05.08.2019 15:34 System Unlocked...
                        (actually pressed 'sleep' button before leaving work at around 16:20)
                        05.08.2019 19:52 System Locked...
                        06.08.2019 09:12 System Unlocked...
                        (WAY too long work day balance has been tracked!)
                    */

                    // this shall be handled here:

                    Manager.SuspendStamping();

                    break;
            }
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Log.Add($"{e.Reason.ToString()}...");

            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    // Back from lock/standby
                    Manager.ResumeStamping();

                    RefreshControls();

                    break;


                case SessionSwitchReason.SessionLogoff:
                case SessionSwitchReason.SessionLock:
                    // Going into lock/standby screen

                    Manager.SuspendStamping(considerLastMouseMove: true);
                    break;
            }
        }

        private bool? m_lastVpnStatus = null;
        private Task m_checkVpn;
        private void StartDetectingVpnConnectionChangeAndNotify()
        {
            if (Settings.RemindCurrentActivityWhenChangingVPN && (m_checkVpn == null || m_checkVpn.IsCompleted))
            {
                m_checkVpn = Task.Run(() =>
                {
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                        bool isVpn = interfaces
                            .Where(i => i.OperationalStatus == OperationalStatus.Up)
                            .Any(i => /*default detection: PPP */(i.NetworkInterfaceType == NetworkInterfaceType.Ppp && i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                            || /*custom detection: by description (e.g. "Cisco AnyConnect")*/ (!String.IsNullOrEmpty(Settings.RemindCurrentActivityWhenChangingVPNWithName) ? i.Description.Contains(Settings.RemindCurrentActivityWhenChangingVPNWithName) : true));

                        if (m_lastVpnStatus.HasValue && m_lastVpnStatus.Value != isVpn)
                        {
                            this.Invoke(new Action(() =>
                            {
                                PopupDialog.ShowCurrentlyTrackingActivity(TodayCurrentActivity.Activity);
                            }));
                        }

                        m_lastVpnStatus = isVpn;
                    }
                });
            }
        }

        // Control Handling:

        private void RefreshControlsAfterTextboxChange()
        {
            // do not update textboxes to avoid cursor reset,
            // calendar does not need to be refreshed
            RefreshControls(false, true, true, false, true, true);
        }

        private void RefreshControls(bool updateTextBoxes = true, bool updateActivities = true, bool updateBalances = true, bool updateCalendar = true, bool updateStatistics = true, bool updateTimeline = true)
        {
            if (updateTextBoxes)
            {
                RefreshStampTextBoxes();

                txtComment.Text = CurrentShown.Comment;
                txtWorkingHours.Text = Convert.ToInt32(CurrentShown.WorkingHours).ToString();
            }

            if (updateActivities)
            {
                UpdateActivityList();
            }

            if (updateBalances)
            {
                RefreshDayBalanceLabel();
                TimeSpan tb = Manager.CalculateTotalBalance();
                lblTotalBalance.Text = "Total Balance (w/o Today HH:MM): " + FormatTimeSpan(tb);
            }

            if (updateCalendar)
            {
                StampCalendar.RemoveAllBoldedDates();
                foreach (var day in StampList)
                    StampCalendar.AddBoldedDate(day.Day);
                StampCalendar.UpdateBoldedDates();

                StampCalendar.TodayDate = CurrentShown.Day;
                StampCalendar.DateChanged -= StampCalendar_DateChanged;
                StampCalendar.SetDate(CurrentShown.Day);
                StampCalendar.DateChanged += StampCalendar_DateChanged;
            }

            if (updateStatistics)
            {
                UpdateStatistics();
            }

            if (updateTimeline)
            {
                UpdateTimeline();
            }
        }

        private void UpdateTimeline()
        {
            timelineControl1.Sections.Clear();

            if (!CurrentShown.ActivityRecords.Any())
                return;

            var min = CurrentShown.ActivityRecords.Min(r => r.Begin);
            var max = CurrentShown.ActivityRecords.Max(r => r.End ?? DateTime.Now.TimeOfDay);
            timelineControl1.MinimumValue = (min - TimeSpan.FromHours(1)).Value.TotalMinutes;
            timelineControl1.MaximumValue = (max + TimeSpan.FromHours(1)).TotalMinutes;

            timelineControl1.Sections.AddRange(CreateTimelineSections(CurrentShown, chart1));
            timelineControl1.Refresh();

            // TODO: better preview of change while dragging, e.g. 'New Pause: xxx'

            timelineControl1.OnDragSeparator = (sep, newPos, isStartSep, commit) =>
            {
                var copy = commit ? CurrentShown : CurrentShown.Clone();

                var draggedActivityStart = sep.OfSection.Tag as ActivityRecord;
                var index = CurrentShown.ActivityRecords.IndexOf(draggedActivityStart);

                Console.WriteLine($"Moving Start of activity '{draggedActivityStart.Activity}' from {draggedActivityStart.Begin.Value.ToString("hh\\:mm")} to {TimeSpan.FromMinutes(newPos).ToString("hh\\:mm")}...");

                if (isStartSep)
                    TimeManager.SetActivityBegin(copy, copy.ActivityRecords.ElementAt(index), TimeManager.GetTime(TimeSpan.FromMinutes(newPos)));
                else
                    TimeManager.SetActivityEnd(copy, copy.ActivityRecords.ElementAt(index), TimeManager.GetTime(TimeSpan.FromMinutes(newPos)));

                if (commit)
                    RefreshControls();

                return CreateTimelineSections(copy, chart1);
            };

            timelineControl1.OnDragSection = (sec, offset, commit) =>
            {
                var copy = commit ? CurrentShown : CurrentShown.Clone();

                var draggedActivity = sec.Tag as ActivityRecord;
                var index = CurrentShown.ActivityRecords.IndexOf(draggedActivity);

                var copyActivity = copy.ActivityRecords.ElementAt(index);

                Console.WriteLine($"Shifting activity '{draggedActivity.Activity}' from {draggedActivity.Begin.Value.ToString("hh\\:mm")} to {TimeSpan.FromMinutes(offset).ToString("hh\\:mm")}...");

                TimeManager.SetActivityBegin(copy, copyActivity, TimeManager.GetTime(copyActivity.Begin.Value + TimeSpan.FromMinutes(offset)));
                if (copyActivity.End.HasValue)
                    TimeManager.SetActivityEnd(copy, copyActivity, TimeManager.GetTime(copyActivity.End.Value + TimeSpan.FromMinutes(offset)));

                if (commit)
                    RefreshControls();

                return CreateTimelineSections(copy, chart1);
            };

            timelineControl1.OnSectionClicked = (sec, e, pos) =>
            {
                if (e.Button != MouseButtons.Right)
                    return;

                var clickedActivity = sec.Tag as ActivityRecord;

                var menu = new ContextMenuStrip();

                var changeTo = new ToolStripMenuItem("Change Activity to...");
                menu.Items.Add(changeTo);

                foreach (var activity in Settings.TrackedActivities)
                {
                    string temp = activity;
                    changeTo.DropDownItems.Add(new ToolStripMenuItem(temp, null, (ss, ee) =>
                    {
                        clickedActivity.Activity = temp;
                        RefreshControls();
                    }));
                }

                var setComment = new ToolStripMenuItem("Set Comment...");
                menu.Items.Add(setComment);

                var commentText = new ToolStripTextBox() { Text = clickedActivity.Comment };
                commentText.TextChanged += (ss, ee) =>
                {
                    clickedActivity.Comment = ((ToolStripTextBox)ss).Text;
                    RefreshControls();
                };
                setComment.DropDownItems.Add(commentText);

                var delete = new ToolStripMenuItem("Delete Activity", null, (ss, ee) =>
                {
                    if (Manager.CanDeleteActivity(CurrentShown, clickedActivity))
                    {
                        Manager.DeleteActivity(CurrentShown, clickedActivity);
                        RefreshControls();
                    }
                });
                delete.Enabled = Manager.CanDeleteActivity(CurrentShown, clickedActivity);
                menu.Items.Add(delete);

                var split = new ToolStripMenuItem("Split Activity here", null, (ss, ee) =>
                {
                    var splitTime = TimeManager.GetTime(TimeSpan.FromMinutes(pos));

                    CurrentShown.ActivityRecords.Insert(CurrentShown.ActivityRecords.IndexOf(clickedActivity), new ActivityRecord()
                    {
                        Activity = clickedActivity.Activity,
                        Comment = clickedActivity.Comment,
                        Begin = clickedActivity.Begin,
                        End = splitTime,
                    });

                    var newBegin = splitTime;

                    clickedActivity.Begin = newBegin;

                    RefreshControls();
                });
                menu.Items.Add(split);

                menu.Show(timelineControl1, e.Location);
            };
        }

        private List<TimelineSection> CreateTimelineSections(Stamp stamp, Chart withColorsFromChart)
        {
            var seaGreenPalette = new[]
            {
                ColorTranslator.FromHtml("#2E8B57"),
                ColorTranslator.FromHtml("#66CDAA"),
                ColorTranslator.FromHtml("#4682B4"),
                ColorTranslator.FromHtml("#008B8B"),
                ColorTranslator.FromHtml("#5F9EA0"),
                ColorTranslator.FromHtml("#38B16E"),
                ColorTranslator.FromHtml("#48D1CC"),
                ColorTranslator.FromHtml("#B0C4DE"),
                ColorTranslator.FromHtml("#8FBC8B"),
                ColorTranslator.FromHtml("#87CEEB")
            };

            var results = new List<TimelineSection>();
            foreach (var activity in stamp.ActivityRecords.OrderBy(r => r.Begin))
            {
                var point = withColorsFromChart.Series.FirstOrDefault()?.Points.FirstOrDefault(p => p.AxisLabel == activity.Activity);
                Color activityColorAccordingToChart = Color.Empty;
                if (point != null)
                    activityColorAccordingToChart = seaGreenPalette.ElementAtOrDefault(withColorsFromChart.Series.FirstOrDefault().Points.IndexOf(point));
                else
                    activityColorAccordingToChart = seaGreenPalette.ElementAtOrDefault(stamp.ActivityRecords.Select(r => r.Activity).Distinct().FirstIndexOf(a => a == activity.Activity));

                results.Add(new TimelineSection(activity, activity.Begin.Value, activity.End ?? DateTime.Now.TimeOfDay, activity.End.HasValue)
                {
                    ForeColor = activityColorAccordingToChart,
                    TooltipHeader = activity.Activity,
                    TooltipBody = activity.Comment,
                    TooltipDurationCustomText = activity.End != null && activity.Begin != null ? $"Duration: {Manager.FormatTimeSpan(activity.End.Value - activity.Begin.Value)}" : null
                });
            }
            return results;
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
            string dayDesc = CurrentShown.Day == TimeManager.Time.Today ? "Today" : "Day";

            lblToday.Text = $"{dayDesc}: {CurrentShown.Day.ToShortDateString()}";
            lblTotal.Text = $"{dayDesc} Balance:";
            txtCurrentShownTotal.Text = FormatTimeSpan(Manager.DayBalance(CurrentShown));
        }

        #region Current Day - Time Input Fields

        private void txtStart_TextChanged(object sender, EventArgs e)
        {
            string error = "";
            if (TimeManager.TryParseHHMM(txtStart.Text, out TimeSpan value))
            {
                TimeManager.SetBegin(CurrentShown, value);

                RefreshControlsAfterTextboxChange();
            }
            else if (String.IsNullOrEmpty(txtStart.Text))
            {
                error = "Start Stamp must not be empty!";
            }

            Validate(txtStart, error);
        }
        private void txtEnd_TextChanged(object sender, EventArgs e)
        {
            string error = string.Empty;

            if (TimeManager.TryParseHHMM(txtEnd.Text, out TimeSpan value))
            {
                if (CurrentShown.Day == TimeManager.Time.Today && value >= TimeManager.GetNowTime() + TimeSpan.FromMinutes(m_minuteThresholdToShowNotification))
                    m_endingPopupShownLastTime = default(DateTime);
                TimeManager.SetEnd(CurrentShown, value);
            }
            else if (String.IsNullOrEmpty(txtEnd.Text))
            {
                if (CurrentShown.Day == TimeManager.Time.Today)
                {
                    if (Manager.TodayCurrentActivity != null)
                        Manager.TodayCurrentActivity.End = null;
                    CurrentShown.End = TimeSpan.Zero;

                    m_endingPopupShownLastTime = default(DateTime);
                }
                else
                {
                    // not allowed if not 'today', highlight textbox in red:
                    error = "End Stamp must not be empty for days other than today!";
                }
            }

            Validate(txtEnd, error);

            RefreshControlsAfterTextboxChange();
        }
        private void txtPause_TextChanged(object sender, EventArgs e)
        {
            if (TimeSettings.Integer.IsMatch(txtPause.Text))
                return;
            if (!int.TryParse(txtPause.Text, out int pause))
                return;

            Manager.SetPause(CurrentShown, TimeSpan.FromMinutes(pause));

            RefreshControlsAfterTextboxChange();
        }
        private void txtComment_TextChanged(object sender, EventArgs e)
        {
            CurrentShown.Comment = txtComment.Text;

            RefreshControlsAfterTextboxChange();
        }
        private void txtWorkingHours_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtWorkingHours.Text))
                return;
            if (TimeSettings.Integer.IsMatch(txtWorkingHours.Text))
                return;
            CurrentShown.WorkingHours = Convert.ToInt32(txtWorkingHours.Text);

            RefreshControlsAfterTextboxChange();
        }

        private void Validate(TextBox txt, string error)
        {
            if (!String.IsNullOrEmpty(error))
            {
                txt.BackColor = Color.LightSalmon;
                toolTip1.SetToolTip(txt, error);
            }
            else
            {
                txt.BackColor = SystemColors.Window;
                toolTip1.SetToolTip(txt, null);
            }
        }

        #endregion

        #region Current Day - Activity Grid

        private void UpdateActivityList()
        {
            grdActivities.Rows.Clear();
            grdActivities.Columns.Clear();

            grdActivities.AllowUserToAddRows = false;
            grdActivities.AllowUserToDeleteRows = false;

            grdActivities.Columns.Add(new DataGridViewButtonColumn() { HeaderText = "Start", FlatStyle = FlatStyle.Flat, Text = "▶", ToolTipText = "Start activity", UseColumnTextForButtonValue = true, Resizable = DataGridViewTriState.False, AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader, Width = 35 });
            grdActivities.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Activity", ReadOnly = true, Width = 66 });
            grdActivities.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Hours", ReadOnly = true, Width = 60 });
            grdActivities.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Comment", Width = 76 });

            grdActivities.Columns[0].Visible = CurrentShown.Day == TimeManager.Time.Today;
            foreach (var activity in Settings.TrackedActivities)
            {
                if (CurrentShown.ActivityRecords.Any(r => r.Activity == activity))
                {
                    var activityKind = CurrentShown.ActivityRecords.Where(r => r.Activity == activity);
                    var totalActivityTime = TimeSpan.FromMinutes(activityKind.Sum(a => TimeManager.Total(a).TotalMinutes));
                    int index = grdActivities.Rows.Add("", activity, FormatTimeSpan(totalActivityTime), activityKind.Last().Comment);
                    grdActivities.Rows[index].Tag = activity;
                    foreach (DataGridViewCell cell in grdActivities.Rows[index].Cells)
                        cell.Style.ForeColor = grdActivities.DefaultCellStyle.ForeColor;
                }
                else
                {
                    int index = grdActivities.Rows.Add("", activity, FormatTimeSpan(TimeSpan.Zero), String.Empty);
                    grdActivities.Rows[index].Tag = activity;
                    foreach (DataGridViewCell cell in grdActivities.Rows[index].Cells)
                        cell.Style.ForeColor = Color.Gray;
                }
            }

            HighlightCurrentActivity();

            m_hasActivityIssues = !Manager.HasMatchingActivityTimestamps(CurrentShown, out var tooltipError);

            if (!m_hasActivityIssues)
            {
                grdActivities.GridColor = SystemColors.ControlDark;
                lblActivityWarning.Visible = false;
            }
            else
            {
                grdActivities.GridColor = Color.Red;
                lblActivityWarning.Visible = true;

                toolTip1.SetToolTip(lblActivityWarning, tooltipError);
            }

            grdActivities.Columns[2].ReadOnly = !m_hasActivityIssues;

        }

        private bool m_hasActivityIssues = false;

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

            if (CurrentShown != null && CurrentShown.Day == TimeManager.Time.Today)
            {
                // highlight current:
                DataGridViewRow currentRow = grdActivities.Rows.OfType<DataGridViewRow>().FirstOrDefault(r => r.Cells[1]?.Value as string == TodayCurrentActivity?.Activity);

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
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                // start activity button clicked:
                Manager.StartNewActivity(grdActivities.Rows[e.RowIndex].Cells[1].Value as string, null);
                //HighlightCurrentActivity();
                //foreach (DataGridViewCell cell in grdActivities.Rows[e.RowIndex].Cells)
                //    cell.Style.ForeColor = grdActivities.DefaultCellStyle.ForeColor;

                RefreshControls();
            }
        }

        private void grdActivities_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            var currentRow = grdActivities.Rows[e.RowIndex];
            var currentActivity = grdActivities.Rows[e.RowIndex].Cells[1].Value as string;

            if (e.ColumnIndex == 1)
            {
                // activity name is readonly
            }
            else if (e.ColumnIndex == 2)
            {
                // activity time is readonly
                CurrentShown.ActivityRecords.RemoveAll(r => r.Activity == currentActivity);
                if (TimeManager.TryParseHHMM(grdActivities.Rows[e.RowIndex].Cells[2].Value as string, out TimeSpan duration))
                {
                    CurrentShown.ActivityRecords.Add(new ActivityRecord() { Activity = currentActivity, Begin = CurrentShown.Begin, End = CurrentShown.Begin + duration });
                    RefreshControls(true, true, true, true, true, true);
                }
            }
            else if (e.ColumnIndex == 3)
            {
                // set comment of last activity:
                var activity = CurrentShown.GetLastActivity(currentActivity);
                if (activity != null)
                {
                    activity.Comment = grdActivities.Rows[e.RowIndex].Cells[3].Value as string;
                    RefreshControls(false, true, true, false, true, true);
                }
                else
                {
                    grdActivities.Rows[e.RowIndex].Cells[3].Value = String.Empty;
                }
            }
        }

        #endregion

        #region Calendar

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
                Manager.CurrentShown = selectedStamp;
                RefreshControls();
            }
        }

        private void btnAddTimestamp_Click(object sender, EventArgs e)
        {
            btnAddTimestamp.Visible = false;

            Manager.CurrentShown = new Stamp(Settings.DefaultWorkingHours) { Day = StampCalendar.SelectionStart };
            StampList.Add(CurrentShown);

            RefreshControls();
        }

        #endregion

        #region Chart

        private void UpdateStatistics()
        {

            //Update Statistics Chart:
            chart1.ChartAreas.Clear();
            chart1.ChartAreas.Add(new ChartArea());
            chart1.Series.Clear();
            chart1.Legends.Clear();
            lblStatisticValues.Text = String.Empty;

            var Statistics = new Series("Timestamp Statistics");

            if (Settings.StatisticType == TimeSettings.StatisticTypes.TimeInLieu)
            {
                Statistics.ChartType = SeriesChartType.Column;

                Statistics.IsXValueIndexed = true;
                Statistics.XValueType = ChartValueType.DateTime;
                Statistics.YValueType = ChartValueType.Double;

                var timeRangeStamps = GetTimeStampsInRange(false);

                if (timeRangeStamps.Count > 1)
                {
                    double min = Manager.CalculateTotalBalance(timeRangeStamps.First().Day).TotalHours;
                    double max = min;
                    foreach (var stamp in timeRangeStamps)
                    {
                        var balance = Manager.CalculateTotalBalance(stamp.Day).TotalHours;
                        Statistics.Points.AddXY(stamp.Day, balance);
                        if (balance < min)
                            min = balance;
                        if (balance > max)
                            max = balance;
                    }

                    string averageBegin = FormatTimeSpan(TimeSpan.FromHours(timeRangeStamps.Average(s => s.Begin.TotalHours)));
                    string averageEnd = FormatTimeSpan(TimeSpan.FromHours(timeRangeStamps.Average(s => s.End.TotalHours)));
                    string averagePause = timeRangeStamps.Average(s => s.Pause.TotalMinutes).ToString("0");

                    var hours = timeRangeStamps.Select(s => Manager.DayBalance(s).TotalHours);
                    string averageTotal = FormatTimeSpan(TimeSpan.FromHours(hours.Average()));

                    lblStatisticValues.Text = $"ø Begin: {averageBegin} | ø End: {averageEnd} | ø Pause: {averagePause} | ø Total: {averageTotal}";

                    chart1.ChartAreas.First().AxisY.Minimum = Math.Floor(min);
                    chart1.ChartAreas.First().AxisY.Maximum = Math.Ceiling(max);
                    chart1.ChartAreas.First().AxisY.LabelStyle.Format = "0";
                    chart1.ChartAreas.First().AxisY.RoundAxisValues();
                }
            }
            else if (Settings.StatisticType == TimeSettings.StatisticTypes.Activities)
            {
                Statistics.ChartType = SeriesChartType.Pie;

                Statistics.XValueType = ChartValueType.String;
                Statistics.YValueType = ChartValueType.Double;


                var allActivities = GetTimeStampsInRange(true).SelectMany(s => s.ActivityRecords);

                var totalHoursPerActivity = allActivities.GroupBy(a => a.Activity).ToDictionary(a => a.Key, a => a.Sum(ar => TimeManager.Total(ar).TotalHours));

                var totalHours = totalHoursPerActivity.Values.Sum();
                var percentPerActivity = totalHoursPerActivity.ToDictionary(a => a.Key, a => (a.Value / totalHours) * 100);

                foreach (var act in percentPerActivity)
                {
                    var rounded = Math.Round(act.Value, 0);
                    int index = Statistics.Points.AddXY(act.Key, rounded);
                    Statistics.Points[index].Label = $"{act.Key}: {rounded} %";
                }
            }
            else if (Settings.StatisticType == TimeSettings.StatisticTypes.WeeklyActivities)
            {
                var allStamps = GetTimeStampsInRange(true).Where(s => s.ActivityRecords.Any());
                var timeRangeStampsPerWeek = allStamps.GroupBy(s => s.Day.GetWeekOfYearISO8601()); // daily: (s => s.Day.Day + "." + s.Day.Month);

                var legend = chart1.Legends.Add("Legend");

                var allActivityNames = allStamps.SelectMany(s => s.ActivityRecords).Select(r => r.Activity).Distinct().ToArray();

                int legendLength = allActivityNames.Max(a => a.Length);
                if (chart1.Width < 600)
                    legendLength = 8 + (chart1.Width / 100) - 2;

                var legendTexts = allActivityNames.GetUniqueAbbreviations(legendLength);

                var series = allActivityNames.ToDictionary(a => a, a =>
                {
                    var statistics = new Series(a);
                    statistics.ChartType = SeriesChartType.StackedArea100;
                    statistics.IsXValueIndexed = true;
                    statistics.XValueType = ChartValueType.String;
                    statistics.YValueType = ChartValueType.Double;
                    statistics.LegendText = legendTexts[allActivityNames.FirstIndexOf(n => n == a)];
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

                        var totalHoursPerActivity = allActivities.GroupBy(a => a.Activity).ToDictionary(a => a.Key, a => a.Sum(ar => TimeManager.Total(ar).TotalHours));

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
            else if (Settings.StatisticType == TimeSettings.StatisticTypes.ActivityComments)
            {
                var allActivities = GetTimeStampsInRange(true).SelectMany(s => s.ActivityRecords).Where(a => !String.IsNullOrEmpty(a.Comment));

                var allCommentsWithActivities = allActivities.GroupBy(a => a.Comment.Trim());

                var allActs = allActivities.Select(a => a.Activity).Distinct().ToList();

                // TODO: rather show as grid!

                var legend = chart1.Legends.Add("Legend");

                int legendLength = allCommentsWithActivities.Max(a => a.Key.Length);
                if (chart1.Width < 600)
                    legendLength = 8 + (chart1.Width / 100) - 2;

                var legendTexts = allCommentsWithActivities.Select(c => c.Key).GetUniqueAbbreviations(legendLength);

                for (int i = 0; i < allCommentsWithActivities.Count(); i++)
                {
                    var comment = allCommentsWithActivities.ElementAt(i);

                    var totalHoursPerActivity = comment.GroupBy(a => a.Activity).ToDictionary(a => a.Key, a => a.Sum(ar => TimeManager.Total(ar).TotalHours));

                    var statistics = new Series(comment.Key);
                    statistics.ChartType = SeriesChartType.StackedBar;
                    //statistics.IsXValueIndexed = true;
                    statistics.XValueType = ChartValueType.String;
                    statistics.YValueType = ChartValueType.Double;
                    statistics.LegendText = legendTexts[i];
                    statistics.LegendToolTip = comment.Key;
                    statistics.IsVisibleInLegend = true;
                    statistics.Legend = "Legend";

                    foreach (var a in allActs)
                    {
                        if (totalHoursPerActivity.ContainsKey(a))
                            statistics.Points.AddXY(a, totalHoursPerActivity[a]);
                        else
                            statistics.Points.AddXY(a, 0);

                    }

                    chart1.Series.Add(statistics);
                }

                chart1.ChartAreas.First().AxisY.LabelStyle.Format = "0";
                chart1.ChartAreas.First().AxisY.RoundAxisValues();

                return;
            }
            else
            {
                throw new NotImplementedException();
            }

            chart1.Series.Add(Statistics);
        }

        private void Chart1_SizeChanged(object sender, EventArgs e)
        {
            if (Settings.StatisticType == TimeSettings.StatisticTypes.WeeklyActivities)
            {
                var legend = chart1.Legends.FirstOrDefault();

                var allActivityNames = chart1.Series.Select(s => s.LegendToolTip).Where(s => s != null).ToArray();

                int legendLength = allActivityNames.Max(a => a.Length);
                if (chart1.Width < 800)
                    legendLength = 6 + (chart1.Width / 50) - 4;

                Debug.WriteLine($"Chart Width: {chart1.Width}, Legend Length: {legendLength}");

                var legendTexts = allActivityNames.GetUniqueAbbreviations(legendLength);

                foreach (var series in chart1.Series)
                {
                    var activity = series.LegendToolTip;

                    if (activity != null)
                    {
                        var newProposedLegendText = legendTexts.ElementAt(allActivityNames.FirstIndexOf(n => n == activity));
                        if (series.LegendText != newProposedLegendText)
                        {
                            series.LegendText = newProposedLegendText;
                        }
                    }
                }

            }
        }

        private List<Stamp> GetTimeStampsInRange(bool includeToday)
        {
            // Select time stamps according to user selection:
            TimeSpan sinceAgo;

            switch (Settings.StatisticRange)
            {
                //case StatisticRanges.CurrentMonth:
                //    timeRangeStamps.AddRange(StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == StampCalendar.SelectionStart.Year && s.Day.Month == StampCalendar.SelectionStart.Month && (includeToday || s.Day != Manager.Time.Today)));
                //    break;
                //case StatisticRanges.CurrentYear:
                //    timeRangeStamps.AddRange(StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == StampCalendar.SelectionStart.Year && (includeToday || s.Day != Manager.Time.Today)));
                //    break;
                case TimeSettings.StatisticRanges.Ever:
                    return StampList.OrderBy(s => s.Day).Where(s => (includeToday || s.Day != TimeManager.Time.Today)).ToList();

                case TimeSettings.StatisticRanges.RecentYear:
                    sinceAgo = TimeSpan.FromDays(365);
                    break;
                case TimeSettings.StatisticRanges.RecentTerm:
                    sinceAgo = TimeSpan.FromDays(182);
                    break;
                case TimeSettings.StatisticRanges.RecentQuarter:
                    sinceAgo = TimeSpan.FromDays(91);
                    break;
                case TimeSettings.StatisticRanges.RecentMonth:
                    sinceAgo = TimeSpan.FromDays(30);
                    break;
                case TimeSettings.StatisticRanges.RecentFortnight:
                    sinceAgo = TimeSpan.FromDays(14);
                    break;
                case TimeSettings.StatisticRanges.RecentWeek:
                    sinceAgo = TimeSpan.FromDays(7);
                    break;

                case TimeSettings.StatisticRanges.SelectedYear:
                    return StampList.OrderBy(s => s.Day).Where(s => (includeToday || s.Day != TimeManager.Time.Today)).Where(s => s.Day.Year == CurrentShown.Day.Year).ToList();
                case TimeSettings.StatisticRanges.SelectedMonth:
                    return StampList.OrderBy(s => s.Day).Where(s => (includeToday || s.Day != TimeManager.Time.Today)).Where(s => s.Day.Year == CurrentShown.Day.Year && s.Day.Month == CurrentShown.Day.Month).ToList();
                case TimeSettings.StatisticRanges.SelectedWeek:
                    var targetWeek = CurrentShown.Day.GetWeekOfYearISO8601();
                    return StampList.OrderBy(s => s.Day).Where(s => (includeToday || s.Day != TimeManager.Time.Today)).Where(s => s.Day.Year == CurrentShown.Day.Year && s.Day.Month == CurrentShown.Day.Month && s.Day.GetWeekOfYearISO8601() == targetWeek).ToList();
                case TimeSettings.StatisticRanges.SelectedDay:
                    return new List<Stamp>() { CurrentShown };

                default:
                    throw new NotImplementedException();
            }

            return StampList.OrderBy(s => s.Day).Where(s => (includeToday || s.Day != TimeManager.Time.Today)).Where(s => s.Day > TimeManager.Time.Now.Subtract(sinceAgo)).ToList();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.StatisticType = (TimeSettings.StatisticTypes)cmbStatisticType.SelectedIndex;
            UpdateStatistics();
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.StatisticRange = (TimeSettings.StatisticRanges)cmbStatisticRange.SelectedIndex;
            UpdateStatistics();
        }

        #endregion

        #region Action Buttons

        private void btnTakeDayOff_Click(object sender, EventArgs e)
        {
            if (CurrentShown.Begin.TotalMinutes != 0 && CurrentShown.End.TotalMinutes != 0)
            {
                var answer = MessageBox.Show("Are you sure? You already have a Stamp for that day! This will overwrite the whole stamp.", "Take Day Off?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (answer == System.Windows.Forms.DialogResult.No)
                    return;
            }

            Manager.TakeDayOff(CurrentShown);

            RefreshControls();
        }

        private void btnDeleteStamp_Click(object sender, EventArgs e)
        {
            if (Manager.DeleteStamp(CurrentShown))
            {
                RefreshControls();
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            var diag = new Settings(Manager);

            diag.ShowDialog(this);

            RefreshControls();
        }

        private void btnExportExcelActivities_Click(object sender, EventArgs e)
        {
            var exporter = new ExcelExport(this);

            var years = exporter.GetExportableYears();

            var menu = new ContextMenuStrip();

            foreach (var year in years)
            {
                menu.Items.Add(new ToolStripMenuItem(year.ToString(), null, (ss, ee) =>
                {
                    int exportYear = (int)(((ToolStripMenuItem)ss).Tag);
                    exporter.CreateExcel(exportYear);
                })
                { Tag = year });
            }

            var button = (Button)sender;

            menu.Show(button, new Point(0, button.Height));
        }

        #endregion

        #region Tray Icon

        private void CreateOrUpdateTrayIconContextMenu()
        {
            if (Today == null)
                return;

            var menu = notifyIcon1.ContextMenuStrip ?? new ContextMenuStrip();
            foreach (var activity in Settings.TrackedActivities)
            {
                string displayText;
                Color foreColor;
                if (Today.ActivityRecords.Any(r => r.Activity == activity))
                {
                    var activityKind = CurrentShown.ActivityRecords.Where(r => r.Activity == activity);
                    var totalActivityTime = TimeSpan.FromMinutes(activityKind.Sum(a => TimeManager.Total(a).TotalMinutes));

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
                        Manager.StartNewActivity(newActivity, null);
                        PopupDialog.ShowCurrentlyTrackingActivity(newActivity);
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
                RefreshControls();
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

        #region Tick-Timer: End-Notification, Automatic Pause Recognition

        private Timer m_timer;
        private int m_minuteThresholdToShowNotification = 5;
        private DateTime m_endingPopupShownLastTime;

        private void Timer_Tick(object sender, EventArgs e)
        {
            // handle 'todays end' notifications:

            if ((m_endingPopupShownLastTime == default(DateTime) || m_endingPopupShownLastTime.Date != Today.Day.Date) && Today.Day.Date == TimeManager.Time.Today)
            {
                if (Today.End == TimeSpan.Zero && Manager.DayBalance(Today) >= TimeSpan.FromMinutes(-m_minuteThresholdToShowNotification))
                {
                    this.Invoke(new Action(() =>
                    {
                        m_endingPopupShownLastTime = TimeManager.Time.Now;
                        PopupDialog.Show8HrsIn5Minutes(TimeManager.Time.Today + Today.Begin + Today.Pause + TimeSpan.FromHours(Today.WorkingHours));
                    }));
                }
                else if (Today.End != TimeSpan.Zero && TimeManager.Time.Now + TimeSpan.FromMinutes(m_minuteThresholdToShowNotification) >= TimeManager.Time.Today + Today.End)
                {
                    this.Invoke(new Action(() =>
                    {
                        m_endingPopupShownLastTime = TimeManager.Time.Now;
                        PopupDialog.ShowPlannedEndIn5Minutes(TimeManager.Time.Today + Today.End);
                    }));
                }
            }


            // activate / deactivate mouse movement tracking for automatic pause time recognition:

            //if (Manager.IsInPauseTimeRecognitionMode)
            {
                if (MouseHook == null)
                {
                    MouseHook = new MouseHookListener(new GlobalHooker());
                    MouseHook.MouseMoveExt += MouseHook_MouseMoveExt;
                }

                if (MouseHook != null && !MouseHook.Enabled)
                {
                    MouseHook.Enabled = true;
                    Manager.LastMouseMove = default(DateTime);
                }

                // TODO: should not be here? moved to there -^
                //Manager.LastMouseMove = TimeSpan.Zero;
            }
            //else
            //{
            //    // if not waiting for user to come back from afk, stop looking for idle
            //    if (MouseHook != null && MouseHook.Enabled)
            //    {
            //        MouseHook.Enabled = false;
            //    }
            //}


            // check whether there is an active vpn connection, and if applicable show current activity reminder:

            StartDetectingVpnConnectionChangeAndNotify();


        }


        private MouseHookListener MouseHook;

        private void MouseHook_MouseMoveExt(object sender, MouseEventExtArgs e)
        {
            // test comment out to show the correct notification (upon log in):

            //if (Manager.IsQualifiedPauseBreak)
            //{
            //    Log.Add("Mouse moved + qualified pause break, resuming stamping (not logged in yet?) -> notification is being shown...");

            //    Manager.ResumeStamping();

            //    // refresh UI:
            //    RefreshControls();
            //}


            // if m


            Manager.LastMouseMove = TimeManager.Time.Now;
        }

        #endregion

    }
}
