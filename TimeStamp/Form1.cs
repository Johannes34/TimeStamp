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
                Log.Add("Unhandled Exception: " + (e.ExceptionObject is Exception ? getFullExceptionMessage(e.ExceptionObject as Exception) : e.ExceptionObject.ToString()));
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
                MessageBox.Show(getFullExceptionMessage(e));
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
            refreshControls();

            m_timer = new Timer() { Interval = 5000, Enabled = true };
            m_timer.Tick += new EventHandler(Timer_Tick);

            // data bind control values to settings:

            cmbStatisticType.DataBindings.Add(new Binding(nameof(ComboBox.SelectedIndex), Settings, nameof(Settings.StatisticType)));
            cmbStatisticRange.DataBindings.Add(new Binding(nameof(ComboBox.SelectedIndex), Settings, nameof(Settings.StatisticRange)));
            this.DataBindings.Add(new Binding(nameof(Form.Width), Settings, nameof(Settings.WindowWidth)));
            this.DataBindings.Add(new Binding(nameof(Form.Height), Settings, nameof(Settings.WindowHeight)));



            // enable events for system sleep/standby/resume and OS log on/off:
            //SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            // enable events for notebook lid opening/closing:
            //RegisterForPowerNotifications();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.SaveSettings();

            Manager.SuspendStamping();

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

        //            refreshControls();

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
                    Log.Add("System Unlocked...");

                    Manager.ResumeStamping();

                    refreshControls();

                    break;

                case SessionSwitchReason.SessionLock:
                    // Going into lock/standby screen
                    Log.Add("System Locked...");

                    Manager.SuspendStamping();

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

        private void refreshControls()
        {
            RefreshStampTextBoxes();

            txtComment.Text = CurrentShown.Comment;
            txtWorkingHours.Text = Convert.ToInt32(CurrentShown.WorkingHours).ToString();

            UpdateActivityList();

            RefreshDayBalanceLabel();
            TimeSpan tb = Manager.CalculateTotalBalance();
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
            string dayDesc = CurrentShown.Day == Manager.Time.Today ? "Today" : "Day";

            lblToday.Text = $"{dayDesc}: {CurrentShown.Day.ToShortDateString()}";
            lblTotal.Text = $"{dayDesc} Balance:";
            txtCurrentShownTotal.Text = FormatTimeSpan(Manager.DayBalance(CurrentShown));
        }

        #region Current Day - Time Input Fields

        private void txtStart_TextChanged(object sender, EventArgs e)
        {
            if (TimeManager.TryParseHHMM(txtStart.Text, out TimeSpan value))
            {
                Manager.SetBegin(CurrentShown, value);
                UpdateActivityList();
                RefreshDayBalanceLabel();
            }
        }
        private void txtEnd_TextChanged(object sender, EventArgs e)
        {
            if (TimeManager.TryParseHHMM(txtEnd.Text, out TimeSpan value))
            {
                if (value >= Manager.GetNowTime() + TimeSpan.FromMinutes(m_minuteThresholdToShowNotification))
                    m_endingPopupShownLastTime = default(DateTime);
                Manager.SetEnd(CurrentShown, value);
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
            if (TimeSettings.Integer.IsMatch(txtPause.Text))
                return;
            if (!int.TryParse(txtPause.Text, out int pause))
                return;
            Manager.SetPause(CurrentShown, TimeSpan.FromMinutes(pause));
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
            if (TimeSettings.Integer.IsMatch(txtWorkingHours.Text))
                return;
            CurrentShown.WorkingHours = Convert.ToInt32(txtWorkingHours.Text);
            RefreshDayBalanceLabel();
        }

        #endregion

        #region Current Day - Activity Grid

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
                    DataSource = Settings.TrackedActivities.Concat(new[] { "[DELETE ENTRY]" }).ToArray()
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
                        FormatTimeSpan(Manager.Total(activity)),
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

                grdActivities.Columns[0].Visible = CurrentShown.Day == Manager.Time.Today;
                foreach (var activity in Settings.TrackedActivities)
                {
                    if (CurrentShown.ActivityRecords.Any(r => r.Activity == activity))
                    {
                        var activityKind = CurrentShown.ActivityRecords.Where(r => r.Activity == activity);
                        var totalActivityTime = TimeSpan.FromMinutes(activityKind.Sum(a => Manager.Total(a).TotalMinutes));
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

            var stampTime = Manager.DayTime(CurrentShown);
            var activityTime = TimeSpan.FromMinutes(CurrentShown.ActivityRecords.Sum(r => Manager.Total(r).TotalMinutes));
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

            if (CurrentShown != null && CurrentShown.Day == Manager.Time.Today)
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
                    Manager.StartNewActivity(grdActivities.Rows[e.RowIndex].Cells[1].Value as string, null);
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
                    if (Settings.TrackedActivities.Any(a => a == text))
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
                    if (TimeManager.TryParseHHMM(text, out TimeSpan value))
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
                                    if (Manager.Total(previousActivity) < TimeSpan.Zero)
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
                        Manager.CalculatePauseFromActivities(CurrentShown);

                        RefreshStampTextBoxes();
                        UpdateActivityList();
                    }
                }
                else if (e.ColumnIndex == 2)
                {
                    // change end
                    if (TimeManager.TryParseHHMM(text, out TimeSpan value))
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
                                    if (Manager.Total(nextActivity) < TimeSpan.Zero)
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
                        Manager.CalculatePauseFromActivities(CurrentShown);

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
                refreshControls();
            }
        }

        private void btnAddTimestamp_Click(object sender, EventArgs e)
        {
            btnAddTimestamp.Visible = false;

            Manager.CurrentShown = new Stamp(Settings.DefaultWorkingHours) { Day = StampCalendar.SelectionStart };
            StampList.Add(CurrentShown);

            refreshControls();
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
                    string averageTotal = FormatTimeSpan(TimeSpan.FromHours(timeRangeStamps.Select(s => Manager.DayBalance(s).TotalHours).Average()));

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

                var totalHoursPerActivity = allActivities.GroupBy(a => a.Activity).ToDictionary(a => a.Key, a => a.Sum(ar => Manager.Total(ar).TotalHours));

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

                        var totalHoursPerActivity = allActivities.GroupBy(a => a.Activity).ToDictionary(a => a.Key, a => a.Sum(ar => Manager.Total(ar).TotalHours));

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

            switch (Settings.StatisticRange)
            {
                //case StatisticRanges.CurrentMonth:
                //    timeRangeStamps.AddRange(StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == StampCalendar.SelectionStart.Year && s.Day.Month == StampCalendar.SelectionStart.Month && (includeToday || s.Day != Manager.Time.Today)));
                //    break;
                //case StatisticRanges.CurrentYear:
                //    timeRangeStamps.AddRange(StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == StampCalendar.SelectionStart.Year && (includeToday || s.Day != Manager.Time.Today)));
                //    break;
                case TimeSettings.StatisticRanges.Ever:
                    return StampList.OrderBy(s => s.Day).Where(s => (includeToday || s.Day != Manager.Time.Today)).ToList();

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
                case TimeSettings.StatisticRanges.RecentWeek:
                    sinceAgo = TimeSpan.FromDays(7);
                    break;

                case TimeSettings.StatisticRanges.SelectedYear:
                    return StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == CurrentShown.Day.Year).ToList();
                case TimeSettings.StatisticRanges.SelectedMonth:
                    return StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == CurrentShown.Day.Year && s.Day.Month == CurrentShown.Day.Month).ToList();
                case TimeSettings.StatisticRanges.SelectedWeek:
                    var targetWeek = GetWeekOfYearISO8601(CurrentShown.Day);
                    return StampList.OrderBy(s => s.Day).Where(s => s.Day.Year == CurrentShown.Day.Year && s.Day.Month == CurrentShown.Day.Month && GetWeekOfYearISO8601(s.Day) == targetWeek).ToList();
                case TimeSettings.StatisticRanges.SelectedDay:
                    return new List<Stamp>() { CurrentShown };

                default:
                    throw new NotImplementedException();
            }

            return StampList.OrderBy(s => s.Day).Where(s => s.Day > Manager.Time.Now.Subtract(sinceAgo)).ToList();
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

            refreshControls();
        }

        private void btnDeleteStamp_Click(object sender, EventArgs e)
        {
            if (Manager.DeleteStamp(CurrentShown))
            {
                refreshControls();
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            var diag = new Settings(Manager);

            diag.ShowDialog(this);

            refreshControls();
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
                    var totalActivityTime = TimeSpan.FromMinutes(activityKind.Sum(a => Manager.Total(a).TotalMinutes));

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

        #region Tick-Timer: End-Notification, Automatic Pause Recognition

        private Timer m_timer;
        private int m_minuteThresholdToShowNotification = 5;
        private DateTime m_endingPopupShownLastTime;

        private void Timer_Tick(object sender, EventArgs e)
        {
            // handle 'todays end' notifications:

            if ((m_endingPopupShownLastTime == default(DateTime) || m_endingPopupShownLastTime.Date != Today.Day.Date) && Today.Day.Date == Manager.Time.Today)
            {
                if (Today.End == TimeSpan.Zero && Manager.DayBalance(Today) >= TimeSpan.FromMinutes(-m_minuteThresholdToShowNotification))
                {
                    this.Invoke(new Action(() =>
                    {
                        m_endingPopupShownLastTime = Manager.Time.Now;
                        PopupDialog.Show8HrsIn5Minutes(Manager.Time.Today + Today.Begin + Today.Pause + TimeSpan.FromHours(Today.WorkingHours));
                    }));
                }
                else if (Today.End != TimeSpan.Zero && Manager.Time.Now + TimeSpan.FromMinutes(m_minuteThresholdToShowNotification) >= Manager.Time.Today + Today.End)
                {
                    this.Invoke(new Action(() =>
                    {
                        m_endingPopupShownLastTime = Manager.Time.Now;
                        PopupDialog.ShowPlannedEndIn5Minutes(Manager.Time.Today + Today.End);
                    }));
                }
            }


            // activate / deactivate mouse movement tracking for automatic pause time recognition:

            if (Manager.IsInPauseTimeRecognitionMode)
            {
                if (MouseHook == null)
                {
                    MouseHook = new MouseHookListener(new GlobalHooker());
                    MouseHook.MouseMoveExt += MouseHook_MouseMoveExt;
                }

                if (MouseHook != null && !MouseHook.Enabled)
                {
                    MouseHook.Enabled = true;
                }

                Manager.LastMouseMove = TimeSpan.Zero;
            }
            else
            {
                // if not waiting for user to come back from afk, stop looking for idle
                if (MouseHook != null && MouseHook.Enabled)
                {
                    MouseHook.Enabled = false;
                }
            }



            // check whether there is an active vpn connection, and if applicable show current activity reminder:

            StartDetectingVpnConnectionChangeAndNotify();
        }


        private MouseHookListener MouseHook;

        private void MouseHook_MouseMoveExt(object sender, MouseEventExtArgs e)
        {
            if (Manager.IsQualifiedPauseBreak)
            {
                Manager.ResumeStamping();

                // refresh UI:
                refreshControls();
            }

            Manager.LastMouseMove = Manager.Time.Now.TimeOfDay;
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

    }
}
