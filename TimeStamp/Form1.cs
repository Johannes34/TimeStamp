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

        // TODO:

        // - bug bei tageswechsel (funktioniert nicht mehr) -- neuer stamp wird nicht angelegt

        // replace 'day comment' with 'day tags' (same principle as activity tags, but in a separate definition table)
        // e.g. Category Location -- Home, Office, Customer
        // e.g. Lunch -- Subway, Bakery, Dillinger
        // e.g. Status -- Sick, Holiday, Special Holiday



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
                if (e.ExceptionObject is Exception ex)
                    Log.Add("Unhandled Exception: " + ex.GetFullExceptionMessage());
                else
                    Log.Add("Unhandled Exception: " + e.ExceptionObject.ToString());
            };

            Settings = new TimeSettings();
            Settings.LoadSettings();

            Manager = new TimeManager(Settings);
            Manager.RequestUpdateUI = RefreshControls;

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

            timelineToday.Owner = this;
            timelineToday.Manager = Manager;
            timelineToday.RequestRefresh += RefreshControls;

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
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.WindowWidth = this.Width;
                Settings.WindowHeight = this.Height;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.SaveSettings();

            Manager.SuspendStamping();

            this.notifyIcon1.Visible = false;
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

        private void RefreshControls()
        {
            if (this.WindowState == FormWindowState.Minimized)
                return;

            // validate / repair stamp:
            Manager.ValidateStamp(CurrentShown);

            // updateBalances:
            TimeSpan tb = Manager.CalculateTotalBalance();
            lblTotalBalance.Text = "Total Balance (w/o Today HH:MM): " + FormatTimeSpan(tb);

            // updateCalendar:
            StampCalendar.RemoveAllBoldedDates();
            foreach (var day in StampList)
                StampCalendar.AddBoldedDate(day.Day);
            StampCalendar.UpdateBoldedDates();

            StampCalendar.TodayDate = CurrentShown.Day;
            StampCalendar.DateChanged -= StampCalendar_DateChanged;
            StampCalendar.SetDate(CurrentShown.Day);
            StampCalendar.DateChanged += StampCalendar_DateChanged;

            // updateStatistics:
            UpdateStatistics();

            // updateTimeline:
            timelineToday.Stamp = CurrentShown;
            timelineToday.UpdateTimeline();
        }

        public void CreateStartActivityContextMenuStrip(ContextMenuStrip menu, bool isFromNotifyTray)
        {
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

                //var existing = menu.Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => i.Tag as string == activity);

                //if (existing == null)
                //{

                var menuItem = new ToolStripMenuItem(displayText, isCurrentlyActive ? bmp : null, (ss, ee) =>
                {
                    string newActivity = ((ToolStripMenuItem)ss).Tag as string;
                    Manager.StartNewActivity(newActivity, Today.GetLastActivity());
                    if (isFromNotifyTray)
                        PopupDialog.ShowCurrentlyTrackingActivity(newActivity);
                    else
                        RefreshControls();
                })
                {
                    Tag = activity,
                    ForeColor = foreColor
                };

                menu.Items.Add(menuItem);

                var mostFrequent = Manager.GetMostFrequentTags(activity, 10);
                if (mostFrequent.Any())
                {
                    var newActivity = activity;
                    foreach (var combination in mostFrequent)
                    {
                        var tempTags = combination;
                        var quickTagMenu = new ToolStripMenuItem(String.Join(", ", tempTags), null, (ss, ee) =>
                        {
                            Manager.StartNewActivity(newActivity, Today.GetLastActivity());
                            Manager.TodayCurrentActivity.Tags.Clear();
                            Manager.TodayCurrentActivity.Tags = tempTags.ToList();
                            if (isFromNotifyTray)
                                PopupDialog.ShowCurrentlyTrackingActivity(newActivity);
                            else
                                RefreshControls();
                        });
                        menuItem.DropDownItems.Add(quickTagMenu);
                    }
                }

                //}
                //else
                //{
                //    existing.Text = displayText;
                //    existing.Image = isCurrentlyActive ? bmp : null;
                //    existing.ForeColor = foreColor;
                //}
            }
        }

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
            AddTimestampForSelectedDay();
        }

        private void AddTimestampForSelectedDay()
        {
            btnAddTimestamp.Visible = false;

            Manager.CurrentShown = new Stamp(Settings.GetDefaultWorkingHours(StampCalendar.SelectionStart)) { Day = StampCalendar.SelectionStart };
            StampList.Add(CurrentShown);

            RefreshControls();
        }

        #endregion

        #region Chart

        private static Color[] s_seaGreenPalette = new[]
        {
            ColorTranslator.FromHtml("#2E8B57"),
            ColorTranslator.FromHtml("#8B0000"),
            ColorTranslator.FromHtml("#000080"),
            ColorTranslator.FromHtml("#4682B4"),
            ColorTranslator.FromHtml("#B0C4DE"),
            ColorTranslator.FromHtml("#48D1CC"),
            ColorTranslator.FromHtml("#32CD32"),
            ColorTranslator.FromHtml("#FF7F50"),
            ColorTranslator.FromHtml("#DEB887"),
            ColorTranslator.FromHtml("#A9A9A9"),
            ColorTranslator.FromHtml("#FFE4E1"),
            ColorTranslator.FromHtml("#FF69B4"),
            ColorTranslator.FromHtml("#8A2BE2"),
            ColorTranslator.FromHtml("#FFFF00"),
            ColorTranslator.FromHtml("#000000"),
        };

        public Color GetColor(string activity)
        {
            if (Settings.TrackedActivities.Contains(activity))
                return s_seaGreenPalette.ElementAt(Settings.TrackedActivities.IndexOf(activity));

            //if (Settings.Tags.SelectMany(t => t.Value).Contains(activity))
            //    return s_seaGreenPalette.ElementAt(Settings.TrackedActivities.IndexOf(activity));

            return s_seaGreenPalette[0];
        }

        private void CreateFilterControls(bool force = false)
        {
            if (flpChartFilter.Controls.Count == 0 || force)
            {
                flpChartFilter.Controls.Clear();

                var actCmb = new ComboBox();
                actCmb.Margin = new Padding(3, 0, 3, 0);
                actCmb.DropDownStyle = ComboBoxStyle.DropDownList;
                actCmb.Items.Add(FilterControl_SelectActivityText());
                actCmb.Items.Add(FilterControl_AllActivitiesText());
                foreach (var activity in Settings.TrackedActivities)
                    actCmb.Items.Add(activity);
                actCmb.SelectedItem = String.IsNullOrEmpty(Settings.StatisticActivityFilter) ? FilterControl_SelectActivityText() : Settings.StatisticActivityFilter;
                actCmb.SelectedIndexChanged += ActivityFilter_SelectedIndexChanged;
                actCmb.Tag = "Activity";
                actCmb.ContextMenu = new ContextMenu(new[]
                {
                    new MenuItem("Move Left", FilterControl_MoveLeft),
                    new MenuItem("Move Right", FilterControl_MoveRight),
                    new MenuItem("Move First", FilterControl_MoveFirst),
                    new MenuItem("Move Last", FilterControl_MoveLast),
                });
                flpChartFilter.Controls.Add(actCmb);

                foreach (var tagCategory in Settings.Tags)
                {
                    var cmb = new ComboBox();
                    cmb.Margin = new Padding(3, 0, 3, 0);
                    cmb.DropDownStyle = ComboBoxStyle.DropDownList;
                    cmb.Tag = tagCategory.Key;
                    cmb.Items.Add(FilterControl_SelectCategoryText(tagCategory.Key));
                    cmb.Items.Add(FilterControl_AllCategoriesText(tagCategory.Key));
                    foreach (var tag in tagCategory.Value)
                        cmb.Items.Add(tag);
                    cmb.SelectedItem = !Settings.StatisticTagCategoryFilter.ContainsKey(tagCategory.Key) ? FilterControl_SelectCategoryText(tagCategory.Key) : Settings.StatisticTagCategoryFilter[tagCategory.Key];
                    cmb.SelectedIndexChanged += CategoryFilter_SelectedIndexChanged;
                    cmb.Tag = tagCategory.Key;
                    cmb.ContextMenu = new ContextMenu(new[]
                    {
                        new MenuItem("Move Left", FilterControl_MoveLeft),
                        new MenuItem("Move Right", FilterControl_MoveRight),
                        new MenuItem("Move First", FilterControl_MoveFirst),
                        new MenuItem("Move Last", FilterControl_MoveLast),
                    });
                    flpChartFilter.Controls.Add(cmb);
                }
            }
        }

        private string FilterControl_SelectActivityText() => "Select Activity...";
        private string FilterControl_AllActivitiesText() => "All Activities";
        private string FilterControl_SelectCategoryText(string tagCategory) => $"Select {tagCategory}...";
        private string FilterControl_AllCategoriesText(string tagCategory) => "All " + tagCategory + (tagCategory.EndsWith("s") ? "" : "s");

        private void FilterControl_MoveFirst(object sender, EventArgs e)
        {
            FilterControl_MoveSet(sender as MenuItem, 0);
        }

        private void FilterControl_MoveLast(object sender, EventArgs e)
        {
            FilterControl_MoveSet(sender as MenuItem, flpChartFilter.Controls.Count);
        }

        private void FilterControl_MoveLeft(object sender, EventArgs e)
        {
            FilterControl_Move(sender as MenuItem, -1);
        }

        private void FilterControl_MoveRight(object sender, EventArgs e)
        {
            FilterControl_Move(sender as MenuItem, +1);
        }

        private void FilterControl_Move(MenuItem sender, int indexOffset)
        {
            var item = sender as MenuItem;
            var menu = item?.Parent as ContextMenu;
            var control = menu?.SourceControl as ComboBox;
            if (control == null)
                return;

            var index = flpChartFilter.Controls.GetChildIndex(control);
            index += indexOffset;

            if (index < 0 || index > flpChartFilter.Controls.Count)
                return;

            flpChartFilter.Controls.SetChildIndex(control, index);
            flpChartFilter.Invalidate();
            UpdateStatistics();
        }

        private void FilterControl_MoveSet(MenuItem sender, int index)
        {
            var item = sender as MenuItem;
            var menu = item?.Parent as ContextMenu;
            var control = menu?.SourceControl as ComboBox;
            if (control == null)
                return;

            flpChartFilter.Controls.SetChildIndex(control, index);
            flpChartFilter.Invalidate();
            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            //Update Statistics Chart:
            chart1.Titles.Clear();
            chart1.ChartAreas.Clear();
            chart1.ChartAreas.Add(new ChartArea());
            chart1.Series.Clear();
            chart1.Legends.Clear();
            flpChartFilter.Visible = false;

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

                    chart1.Titles.Add($"ø Begin: {averageBegin} | ø End: {averageEnd} | ø Pause: {averagePause} | ø Total: {averageTotal}");

                    chart1.ChartAreas.First().AxisY.Minimum = Math.Floor(min);
                    chart1.ChartAreas.First().AxisY.Maximum = Math.Ceiling(max);
                    chart1.ChartAreas.First().AxisY.LabelStyle.Format = "0";
                    chart1.ChartAreas.First().AxisY.RoundAxisValues();
                }
            }
            else if (Settings.StatisticType == TimeSettings.StatisticTypes.Activities)
            {
                CreateFilterControls();
                flpChartFilter.Visible = true;

                Statistics.ChartType = SeriesChartType.Pie;

                Statistics.XValueType = ChartValueType.String;
                Statistics.YValueType = ChartValueType.Double;


                var allActivities = GetFilteredActivities(GetTimeStampsInRange(true).SelectMany(s => s.ActivityRecords));

                var totalHoursPerActivity = allActivities.GroupBy(a => GetActivityGrouping(a)/*.Activity*/).ToDictionary(a => a.Key, a => a.Sum(ar => TimeManager.Total(ar).TotalHours));

                var totalHours = totalHoursPerActivity.Values.Sum();
                var percentPerActivity = totalHoursPerActivity.ToDictionary(a => a.Key, a => (a.Value / totalHours) * 100);

                foreach (var act in percentPerActivity)
                {
                    var rounded = Math.Round(act.Value, 0);
                    int index = Statistics.Points.AddXY(act.Key, rounded);
                    Statistics.Points[index].Label = $"{act.Key}: {Math.Round(totalHoursPerActivity[act.Key], 1)} h ({rounded} %)";
                }
            }
            //else if (Settings.StatisticType == TimeSettings.StatisticTypes.WeeklyActivities)
            //{
            //    var allStamps = GetTimeStampsInRange(true).Where(s => s.ActivityRecords.Any());
            //    var timeRangeStampsPerWeek = allStamps.GroupBy(s => s.Day.GetWeekOfYearISO8601()); // daily: (s => s.Day.Day + "." + s.Day.Month);

            //    var legend = chart1.Legends.Add("Legend");

            //    var allActivityNames = allStamps.SelectMany(s => s.ActivityRecords).Select(r => r.Activity).Distinct().ToArray();

            //    int legendLength = allActivityNames.Max(a => a.Length);
            //    if (chart1.Width < 600)
            //        legendLength = 8 + (chart1.Width / 100) - 2;

            //    var legendTexts = allActivityNames.GetUniqueAbbreviations(legendLength);

            //    var series = allActivityNames.ToDictionary(a => a, a =>
            //    {
            //        var statistics = new Series(a);
            //        statistics.ChartType = SeriesChartType.StackedArea100;
            //        statistics.IsXValueIndexed = true;
            //        statistics.XValueType = ChartValueType.String;
            //        statistics.YValueType = ChartValueType.Double;
            //        statistics.LegendText = legendTexts[allActivityNames.FirstIndexOf(n => n == a)];
            //        statistics.LegendToolTip = a;
            //        statistics.IsVisibleInLegend = true;
            //        statistics.Legend = "Legend";

            //        return statistics;
            //    });

            //    if (timeRangeStampsPerWeek.Count() > 1)
            //    {
            //        foreach (var week in timeRangeStampsPerWeek)
            //        {
            //            var allActivities = week.SelectMany(s => s.ActivityRecords);

            //            var totalHoursPerActivity = allActivities.GroupBy(a => a.Activity).ToDictionary(a => a.Key, a => a.Sum(ar => TimeManager.Total(ar).TotalHours));

            //            var totalHours = totalHoursPerActivity.Values.Sum();
            //            var percentPerActivity = totalHoursPerActivity.ToDictionary(a => a.Key, a => (a.Value / totalHours) * 100);

            //            foreach (var act in allActivityNames)
            //            {
            //                series[act].Points.AddXY($"{week.Key}/{week.First().Day.Year % 100}", percentPerActivity.ContainsKey(act) ? percentPerActivity[act] : 0.0);
            //            }
            //        }

            //        chart1.ChartAreas.First().AxisY.LabelStyle.Format = "0";
            //        chart1.ChartAreas.First().AxisY.RoundAxisValues();
            //    }

            //    foreach (var stat in series.Values)
            //        chart1.Series.Add(stat);
            //    return;
            //}
            //else if (Settings.StatisticType == TimeSettings.StatisticTypes.ActivityComments)
            //{
            //    var allActivities = GetTimeStampsInRange(true).SelectMany(s => s.ActivityRecords).Where(a => !String.IsNullOrEmpty(a.Comment));

            //    var allCommentsWithActivities = allActivities.GroupBy(a => a.Comment.Trim());

            //    var allActs = allActivities.Select(a => a.Activity).Distinct().ToList();

            //    // TODO: rather show as grid!

            //    var legend = chart1.Legends.Add("Legend");

            //    int legendLength = allCommentsWithActivities.Max(a => a.Key.Length);
            //    if (chart1.Width < 600)
            //        legendLength = 8 + (chart1.Width / 100) - 2;

            //    var legendTexts = allCommentsWithActivities.Select(c => c.Key).GetUniqueAbbreviations(legendLength);

            //    for (int i = 0; i < allCommentsWithActivities.Count(); i++)
            //    {
            //        var comment = allCommentsWithActivities.ElementAt(i);

            //        var totalHoursPerActivity = comment.GroupBy(a => a.Activity).ToDictionary(a => a.Key, a => a.Sum(ar => TimeManager.Total(ar).TotalHours));

            //        var statistics = new Series(comment.Key);
            //        statistics.ChartType = SeriesChartType.StackedBar;
            //        //statistics.IsXValueIndexed = true;
            //        statistics.XValueType = ChartValueType.String;
            //        statistics.YValueType = ChartValueType.Double;
            //        statistics.LegendText = legendTexts[i];
            //        statistics.LegendToolTip = comment.Key;
            //        statistics.IsVisibleInLegend = true;
            //        statistics.Legend = "Legend";

            //        foreach (var a in allActs)
            //        {
            //            if (totalHoursPerActivity.ContainsKey(a))
            //                statistics.Points.AddXY(a, totalHoursPerActivity[a]);
            //            else
            //                statistics.Points.AddXY(a, 0);

            //        }

            //        chart1.Series.Add(statistics);
            //    }

            //    chart1.ChartAreas.First().AxisY.LabelStyle.Format = "0";
            //    chart1.ChartAreas.First().AxisY.RoundAxisValues();

            //    return;
            //}
            else
            {
                throw new NotImplementedException();
            }

            chart1.Series.Add(Statistics);

            // set color palette to always show same colors for same activities / labels:
            var labels = chart1.Series.FirstOrDefault().Points.Select(p => p.AxisLabel);
            var colorDict = new Dictionary<string, Color>();
            foreach (var label in labels)
            {
                if (!colorDict.ContainsKey(label))
                    colorDict.Add(label, GetColor(label));
            }
            chart1.Palette = ChartColorPalette.None;
            chart1.PaletteCustomColors = colorDict.Values.ToArray();
        }

        private void Chart1_SizeChanged(object sender, EventArgs e)
        {
            //if (Settings.StatisticType == TimeSettings.StatisticTypes.WeeklyActivities)
            //{
            //    var legend = chart1.Legends.FirstOrDefault();

            //    var allActivityNames = chart1.Series.Select(s => s.LegendToolTip).Where(s => s != null).ToArray();

            //    int legendLength = allActivityNames.Max(a => a.Length);
            //    if (chart1.Width < 800)
            //        legendLength = 6 + (chart1.Width / 50) - 4;

            //    Debug.WriteLine($"Chart Width: {chart1.Width}, Legend Length: {legendLength}");

            //    var legendTexts = allActivityNames.GetUniqueAbbreviations(legendLength);

            //    foreach (var series in chart1.Series)
            //    {
            //        var activity = series.LegendToolTip;

            //        if (activity != null)
            //        {
            //            var newProposedLegendText = legendTexts.ElementAt(allActivityNames.FirstIndexOf(n => n == activity));
            //            if (series.LegendText != newProposedLegendText)
            //            {
            //                series.LegendText = newProposedLegendText;
            //            }
            //        }
            //    }
            //}
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

        private IEnumerable<ActivityRecord> GetFilteredActivities(IEnumerable<ActivityRecord> activities)
        {
            if (!String.IsNullOrEmpty(Settings.StatisticActivityFilter))
                activities = activities.Where(a => a.Activity == Settings.StatisticActivityFilter);

            if (Settings.StatisticTagCategoryFilter.Any())
                activities = activities.Where(a => Settings.StatisticTagCategoryFilter.Values.All(f => a.Tags.Contains(f)));

            return activities.ToArray();
        }

        private bool IsExpandActivitiesFilterEnabled()
        {
            bool expandActivities = (string)flpChartFilter.Controls.OfType<ComboBox>().FirstOrDefault(c => (string)c.Tag == "Activity")?.SelectedItem == FilterControl_AllActivitiesText();
            return expandActivities;
        }

        private bool IsExpandCategoryFilterEnabled(string category)
        {
            bool expandCategory = (string)flpChartFilter.Controls.OfType<ComboBox>().FirstOrDefault(c => (string)c.Tag == category)?.SelectedItem == FilterControl_AllCategoriesText(category);
            return expandCategory;
        }

        private string GetActivityGrouping(ActivityRecord activity)
        {
            bool hasActivityFilter = !String.IsNullOrEmpty(Settings.StatisticActivityFilter);
            bool hasCategoryFilter = Settings.StatisticTagCategoryFilter.Any();

            //if (!hasActivityFilter && !hasCategoryFilter)
            //{
            //    // group by first filter box:
            //    if ((string)flpChartFilter.Controls[0].Tag == "Activity")
            //        return activity.Activity;
            //    else
            //        return activity.Tags.FirstOrDefault(t => Settings.Tags[(string)flpChartFilter.Controls[0].Tag].Contains(t)) ?? "Other";
            //}

            //string[] groupByTags = Settings.Tags.Where(t => !Settings.StatisticTagCategoryFilter.ContainsKey(t.Key)).SelectMany(k => k.Value).ToArray();

            var groupings = new List<string>();

            bool firstIteration = true;
            foreach (var filter in flpChartFilter.Controls.OfType<ComboBox>().ToList())
            {
                if ((string)filter.Tag == "Activity")
                {
                    string activityName = (string)filter.Tag;
                    if (IsExpandActivitiesFilterEnabled())
                    {
                        groupings.Add(activity.Activity);
                    }
                    else if (!hasActivityFilter && !IsExpandActivitiesFilterEnabled())
                    {
                        if (firstIteration)
                            groupings.Add(activity.Activity);
                    }
                    else if (Settings.StatisticActivityFilter == activity.Activity)
                    {
                        groupings.Add(activity.Activity);
                    }
                }
                else
                {
                    string tagCategory = (string)filter.Tag;
                    if (IsExpandCategoryFilterEnabled(tagCategory) || (!hasCategoryFilter && firstIteration))
                    {
                        if (Settings.Tags.ContainsKey(tagCategory))
                        {
                            string[] groupByTags = Settings.Tags[tagCategory].ToArray();
                            var groupedTags = String.Join(" ", activity.Tags.Where(t => groupByTags.Contains(t)));
                            if (!String.IsNullOrEmpty(groupedTags))
                                groupings.Add(groupedTags);
                        }
                    }
                    else if (Settings.StatisticTagCategoryFilter.ContainsKey(tagCategory))
                    {
                        string groupByTag = Settings.StatisticTagCategoryFilter[tagCategory];
                        if (activity.Tags.Any(t => t == groupByTag))
                            groupings.Add(groupByTag);
                    }
                    //var groupedTags = String.Join(" ", activity.Tags.Where(t => groupByTags.Contains(t)));
                    //if (!String.IsNullOrEmpty(groupedTags))
                    //    groupings.Add(groupedTags);
                }
                firstIteration = false;
            }

            var grouping = String.Join(" ", groupings);

            if (String.IsNullOrWhiteSpace(grouping))
                return "Other";
            return grouping;
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
        private void ActivityFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            Settings.StatisticActivityFilter = Settings.TrackedActivities.Contains((string)cmb.SelectedItem) ? (string)cmb.SelectedItem : null;
            UpdateStatistics();
        }
        private void CategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            string category = cmb.Tag as string;
            string tag = (string)cmb.SelectedItem;

            if (Settings.Tags[category].Contains(tag))
                Settings.StatisticTagCategoryFilter[category] = tag;
            else
                Settings.StatisticTagCategoryFilter.Remove(category);

            UpdateStatistics();
        }

        #endregion

        #region Action Buttons

        private void btnTakeDayOff_Click(object sender, EventArgs e)
        {
            // Take day off should create timestamp if not existing yet (when button "no record for this day, click here to add a new entry" is shown), instead of overwriting current day:
            if (CurrentShown.Day != StampCalendar.SelectionStart)
            {
                AddTimestampForSelectedDay();
            }
            else if (CurrentShown.Begin.TotalMinutes != 0 || CurrentShown.End.TotalMinutes != 0)
            {
                var answer = MessageBox.Show($"Are you sure? You already have a Stamp for {CurrentShown.Day.ToShortDateString()}! This will overwrite the whole stamp.", "Take Day Off?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
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
            menu.Items.Clear();

            CreateStartActivityContextMenuStrip(menu, true);

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

            // check for user mouse movement to update last user action (e.g. pause recognition):
            CheckHasMouseMoved();

            // check whether there is an active vpn connection, and if applicable show current activity reminder:
            StartDetectingVpnConnectionChangeAndNotify();

            // update ui once per minute (only if app is currently shown):
            if (DateTime.Now.Second < 9)
            {
                RefreshControls();
            }
        }

        private Point m_lastLocation;

        private void CheckHasMouseMoved()
        {
            // only considered user-invoked mouse move if moved at least 2 px to any direction (to prevent 'idle', 'ghost' ticks):
            if (!m_lastLocation.IsEmpty && (Math.Abs(m_lastLocation.X - MousePosition.X) > 1 || Math.Abs(m_lastLocation.Y - MousePosition.Y) > 1))
            {
                Manager.LastUserAction = TimeManager.Time.Now;
            }

            m_lastLocation = MousePosition;
        }

        #endregion
    }
}
