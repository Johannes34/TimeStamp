using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TimeStamp
{
    public partial class StampTimelineControl : UserControl
    {
        public StampTimelineControl()
        {
            InitializeComponent();
        }

        public Stamp Stamp { get; set; }

        public Form1 Owner { get; set; }

        public TimeManager Manager { get; set; }

        public TimeSettings Settings => Manager.Settings;

        public event Action RequestRefresh = delegate { };




        public void UpdateTimeline()
        {
            timelineControl1.Sections.Clear();

            btnStartActivity.Top = (timelineControl1.Top + timelineControl1.TimelineY) - btnStartActivity.Height / 2;
            btnStartActivity.Visible = Stamp == Manager.Today;

            //txtComment.Text = Stamp.Comment;
            if (!txtWorkingHours.ContainsFocus)
            {
                txtWorkingHours.TextChanged -= txtWorkingHours_TextChanged;
                txtWorkingHours.Text = Convert.ToInt32(Stamp.WorkingHours).ToString();
                txtWorkingHours.TextChanged += txtWorkingHours_TextChanged;
            }

            if (!txtBegin.ContainsFocus)
            {
                txtBegin.TextChanged -= txtBegin_TextChanged;
                txtBegin.Text = Stamp.Begin != TimeSpan.Zero ? Manager.FormatTimeSpan(Stamp.Begin) : "";
                txtBegin.TextChanged += txtBegin_TextChanged;
            }

            if (!txtEnd.ContainsFocus)
            {
                txtEnd.TextChanged -= txtEnd_TextChanged;
                txtEnd.Text = Stamp.End != TimeSpan.Zero ? Manager.FormatTimeSpan(Stamp.End) : "";
                txtEnd.TextChanged += txtEnd_TextChanged;
            }

            if (!txtPause.ContainsFocus)
            {
                txtPause.TextChanged -= txtPause_TextChanged;
                txtPause.Text = ((int)Stamp.Pause.TotalMinutes).ToString();
                txtPause.TextChanged += txtPause_TextChanged;
            }

            string dayDesc = Stamp.Day == TimeManager.Time.Today ? "Today" : "Day";

            lblToday.Text = $"{dayDesc}: {Stamp.Day.ToShortDateString()}";
            lblTotal.Text = $"{dayDesc} Balance:";
            txtCurrentShownTotal.Text = Manager.FormatTimeSpan(Manager.DayBalance(Stamp));

            TimeSpan min = new TimeSpan(08, 00, 00);
            TimeSpan max = new TimeSpan(18, 00, 00);
            if (Stamp.ActivityRecords.Any())
            {
                min = Stamp.ActivityRecords.Min(r => r.Begin.Value);
                max = Stamp.ActivityRecords.Max(r => r.End ?? DateTime.Now.TimeOfDay);
                //timelineControl1.AddSectionMode = false;
            }
            //else
            //{
            timelineControl1.AddSectionMode = true;
            //}

            timelineControl1.MinimumValue = (min - TimeSpan.FromHours(1)).TotalMinutes;
            timelineControl1.MaximumValue = (max + TimeSpan.FromHours(1)).TotalMinutes;
            timelineControl1.Tag = Stamp;

            timelineControl1.Sections.AddRange(CreateTimelineSections(Stamp));
            timelineControl1.Refresh();

            // TODO: better preview of change while dragging, e.g. 'New Pause: xxx'

            timelineControl1.OnDragSeparator = TimeLine_OnDragSeparator;

            timelineControl1.OnDragSection = TimeLine_OnDragSection;

            timelineControl1.OnSectionClicked = TimeLine_SectionClicked;

            timelineControl1.OnSeparatorClicked = TimeLine_SeparatorClicked;

            timelineControl1.OnAddSection = TimeLine_AddSection;
        }

        private List<TimelineSection> TimeLine_OnDragSeparator(TimelineControl sender, TimelineLocation draggedEndSeparator, TimelineLocation draggedStartSeparator, double newPosition, bool commitPreview)
        {
            var stamp = sender.Tag as Stamp;

            var copy = commitPreview ? stamp : stamp.Clone();

            var draggingStart = draggedStartSeparator?.OfSection.Tag as ActivityRecord;
            var draggingEnd = draggedEndSeparator?.OfSection.Tag as ActivityRecord;

            var draggingStartIndex = draggingStart != null ? stamp.ActivityRecords.IndexOf(draggingStart) : -1;
            var draggingEndIndex = draggingEnd != null ? stamp.ActivityRecords.IndexOf(draggingEnd) : -1;

            var targetTime = TimeManager.GetTime(TimeSpan.FromMinutes(newPosition));

            if (draggingStart != null && draggingEnd == null)
            {
                Console.WriteLine($"Moving Start of activity '{draggingStart.Activity}' from {draggingStart.Begin.Value.ToString("hh\\:mm")} to {targetTime}...");
                var copyStart = copy.ActivityRecords.ElementAt(draggingStartIndex);
                if (copyStart.End.HasValue && targetTime >= copyStart.End.Value)
                {
                    copy.ActivityRecords.Remove(copyStart);

                    var delete = copy.ActivityRecords.Where(r => r.Begin.HasValue && r.End.HasValue && r.Begin.Value >= copyStart.End.Value && r.End.Value <= targetTime).ToList();
                    foreach (var del in delete)
                        copy.ActivityRecords.Remove(del);

                    var clip = copy.ActivityRecords.Where(r => r.Begin.HasValue && r.Begin.Value >= copyStart.End.Value && r.Begin.Value <= targetTime).ToList();
                    foreach (var cli in clip)
                        TimeManager.SetActivityBegin(copy, cli, targetTime);
                }
                else
                    TimeManager.SetActivityBegin(copy, copyStart, targetTime);
            }
            else if (draggingStart == null && draggingEnd != null)
            {
                Console.WriteLine($"Moving End of activity '{draggingEnd.Activity}' from {draggingEnd.End.Value.ToString("hh\\:mm")} to {targetTime}...");
                var copyEnd = copy.ActivityRecords.ElementAt(draggingEndIndex);
                if (copyEnd.Begin.HasValue && targetTime <= copyEnd.Begin.Value)
                {
                    copy.ActivityRecords.Remove(copyEnd);

                    var delete = copy.ActivityRecords.Where(r => r.End.HasValue && r.Begin.HasValue && r.End.Value <= copyEnd.Begin.Value && r.Begin.Value >= targetTime).ToList();
                    foreach (var del in delete)
                        copy.ActivityRecords.Remove(del);

                    var clip = copy.ActivityRecords.Where(r => r.End.HasValue && r.End.Value <= copyEnd.Begin.Value && r.End.Value >= targetTime).ToList();
                    foreach (var cli in clip)
                        TimeManager.SetActivityEnd(copy, cli, targetTime);
                }
                else
                    TimeManager.SetActivityEnd(copy, copyEnd, targetTime);
            }
            else if (draggingStart != null && draggingEnd != null)
            {
                Console.WriteLine($"Moving Start of activity '{draggingStart.Activity}' from {draggingStart.Begin.Value.ToString("hh\\:mm")} to {targetTime}...");
                Console.WriteLine($"Moving End of activity '{draggingEnd.Activity}' from {draggingEnd.End.Value.ToString("hh\\:mm")} to {targetTime}...");

                TimeManager.SetActivityBegin(copy, copy.ActivityRecords.ElementAt(draggingStartIndex), targetTime);
                TimeManager.SetActivityEnd(copy, copy.ActivityRecords.ElementAt(Math.Min(draggingEndIndex, copy.ActivityRecords.Count - 1)), targetTime);
            }

            if (commitPreview)
            {
                RequestRefresh.Invoke();
                //UpdateTimeline();
            }

            return CreateTimelineSections(copy, targetTime);
        }

        private List<TimelineSection> TimeLine_OnDragSection(TimelineControl sender, TimelineSection draggedSection, double offsetValue, bool commitPreview)
        {
            var stamp = sender.Tag as Stamp;

            var copy = commitPreview ? stamp : stamp.Clone();

            var draggedActivity = draggedSection.Tag as ActivityRecord;
            var index = stamp.ActivityRecords.IndexOf(draggedActivity);

            var copyActivity = copy.ActivityRecords.ElementAt(index);

            Console.WriteLine($"Shifting activity '{draggedActivity.Activity}' from {draggedActivity.Begin.Value.ToString("hh\\:mm")} to {TimeSpan.FromMinutes(offsetValue).ToString("hh\\:mm")}...");

            // make sure to shift begin only if end can also be shifted (not 'running'), or begin is not shifted right of 'running' end
            if (copyActivity.End.HasValue || TimeManager.GetTime(copyActivity.Begin.Value + TimeSpan.FromMinutes(offsetValue)) <= TimeManager.Time.Now.TimeOfDay)
                TimeManager.SetActivityBegin(copy, copyActivity, TimeManager.GetTime(copyActivity.Begin.Value + TimeSpan.FromMinutes(offsetValue)));
            if (copyActivity.End.HasValue)
                TimeManager.SetActivityEnd(copy, copyActivity, TimeManager.GetTime(copyActivity.End.Value + TimeSpan.FromMinutes(offsetValue)));

            if (commitPreview)
            {
                RequestRefresh.Invoke();
                //UpdateTimeline();
            }

            return CreateTimelineSections(copy, copyActivity.Begin, copyActivity.End);
        }

        private void TimeLine_SectionClicked(TimelineControl sender, TimelineSection sec, MouseEventArgs e, double pos)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var stamp = sender.Tag as Stamp;

            var clickedActivity = sec.Tag as ActivityRecord;

            var menu = new ContextMenuStrip();

            AddChangeActivityToMenuItem(menu, clickedActivity);

            AddSetTagsMenuItem(menu, clickedActivity);

            AddSetCommentMenuItem(menu, clickedActivity);


            menu.Items.Add(new ToolStripSeparator());

            var delete = new ToolStripMenuItem("Delete Activity", null, (ss, ee) =>
            {
                if (Manager.CanDeleteActivity(stamp, clickedActivity))
                {
                    Manager.DeleteActivity(stamp, clickedActivity);
                    RequestRefresh.Invoke();
                    //UpdateTimeline();
                }
            });
            delete.Enabled = Manager.CanDeleteActivity(stamp, clickedActivity);
            menu.Items.Add(delete);

            var split = new ToolStripMenuItem("Split Activity here", null, (ss, ee) =>
            {
                var splitTime = TimeManager.GetTime(TimeSpan.FromMinutes(pos));

                Manager.SplitActivity(stamp, clickedActivity, splitTime);

                RequestRefresh.Invoke();
                //UpdateTimeline();
            });
            menu.Items.Add(split);

            menu.Show(timelineControl1, e.Location);
        }

        private void AddChangeActivityToMenuItem(ContextMenuStrip menu, ActivityRecord currentActivity)
        {
            var changeTo = new ToolStripMenuItem("Change Activity to...");
            menu.Items.Add(changeTo);

            foreach (var activity in Settings.TrackedActivities)
            {
                string temp = activity;

                var menuItem = new ToolStripMenuItem(temp, null, (ss, ee) =>
                {
                    currentActivity.Activity = temp;
                    RequestRefresh.Invoke();
                });

                changeTo.DropDownItems.Add(menuItem);

                var mostFrequent = Manager.GetMostFrequentTags(activity, 10);
                if (mostFrequent.Any())
                {
                    foreach (var combination in mostFrequent)
                    {
                        var tempTags = combination;
                        var quickTagMenu = new ToolStripMenuItem(String.Join(", ", tempTags), null, (ss, ee) =>
                        {
                            currentActivity.Activity = temp;
                            currentActivity.Tags.Clear();
                            currentActivity.Tags = tempTags.ToList();
                            RequestRefresh.Invoke();
                        });
                        menuItem.DropDownItems.Add(quickTagMenu);
                    }
                }
            }
        }

        private void AddSetTagsMenuItem(ContextMenuStrip menu, ActivityRecord currentActivity)
        {
            var tagging = new ToolStripMenuItem("Set Tags...");
            menu.Items.Add(tagging);

            foreach (var category in Settings.Tags)
            {
                string temp = category.Key;

                var categoryMenu = new ToolStripMenuItem(temp);
                tagging.DropDownItems.Add(categoryMenu);

                foreach (var tag in category.Value)
                {
                    var tempTag = tag;
                    var tagMenu = new ToolStripMenuItem(tempTag, null, (ss, ee) =>
                    {
                        var item = ss as ToolStripMenuItem;
                        if (item.Checked)
                            currentActivity.Tags.Add(tempTag);
                        else
                            currentActivity.Tags.Remove(tempTag);
                        RequestRefresh.Invoke();
                        //UpdateTimeline();
                    });
                    tagMenu.CheckOnClick = true;
                    tagMenu.Checked = currentActivity.Tags.Contains(tag);
                    tagMenu.MouseEnter += (sss, eee) =>
                    {
                        menu.AutoClose = false;
                        tagging.DropDown.AutoClose = false;
                        categoryMenu.DropDown.AutoClose = false;
                    };
                    tagMenu.MouseLeave += (sss, eee) =>
                    {
                        menu.AutoClose = true;
                        tagging.DropDown.AutoClose = true;
                        categoryMenu.DropDown.AutoClose = true;
                    };
                    categoryMenu.DropDownItems.Add(tagMenu);
                }
            }

            var mostFrequent = Manager.GetMostFrequentTags(currentActivity.Activity, 10);
            if (mostFrequent.Any())
                tagging.DropDownItems.Add(new ToolStripSeparator());

            foreach (var combination in mostFrequent)
            {
                var tempTags = combination;
                var quickTagMenu = new ToolStripMenuItem(String.Join(", ", tempTags), null, (ss, ee) =>
                {
                    currentActivity.Tags.Clear();
                    foreach (var tag in tempTags)
                    {
                        if (!currentActivity.Tags.Contains(tag))
                            currentActivity.Tags.Add(tag);
                    }
                    RequestRefresh.Invoke();
                    //UpdateTimeline();
                });
                tagging.DropDownItems.Add(quickTagMenu);
            }
        }

        private void AddSetCommentMenuItem(ContextMenuStrip menu, ActivityRecord currentActivity)
        {
            var setComment = new ToolStripMenuItem("Set Comment...");
            menu.Items.Add(setComment);

            var commentText = new ToolStripTextBox() { Text = currentActivity.Comment };
            commentText.TextChanged += (ss, ee) =>
            {
                currentActivity.Comment = ((ToolStripTextBox)ss).Text;
                RequestRefresh.Invoke();
                //UpdateTimeline();
            };
            setComment.DropDownItems.Add(commentText);
        }

        private void TimeLine_SeparatorClicked(TimelineControl sender, TimelineSection clickedEndSeparator, TimelineSection clickedStartSeparator, MouseEventArgs e, double clickedPosition)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var startActivity = clickedStartSeparator?.Tag as ActivityRecord;
            var endActivity = clickedEndSeparator?.Tag as ActivityRecord;

            var menu = new ContextMenuStrip();

            var clickedActivity = startActivity ?? endActivity;
            if (startActivity != null && endActivity != null)
            {
                var endString = clickedActivity == endActivity ? "'Left'" : "'Right'";
                menu.Items.Add(new ToolStripMenuItem($"{clickedActivity.Activity} ({endString}):") { Enabled = false });
            }

            AddChangeActivityToMenuItem(menu, clickedActivity);

            AddSetTagsMenuItem(menu, clickedActivity);

            AddSetCommentMenuItem(menu, clickedActivity);

            menu.Items.Add(new ToolStripSeparator());

            var merge = new ToolStripMenuItem("Merge Activities", null, (ss, ee) =>
            {
                Manager.DeleteActivity(Stamp, endActivity);
                TimeManager.SetActivityBegin(Stamp, startActivity, endActivity.Begin.Value);
                RequestRefresh.Invoke();
                //UpdateTimeline();
            });
            merge.Enabled = startActivity != null && endActivity != null && startActivity.Activity == endActivity.Activity;
            menu.Items.Add(merge);

            List<ActivityRecord> adjoining = new List<ActivityRecord>();
            if (startActivity != null && endActivity != null && startActivity.Activity == endActivity.Activity)
            {
                var index = Stamp.ActivityRecords.IndexOf(startActivity);
                if (index != -1)
                {
                    adjoining.Add(startActivity);
                    adjoining.Add(endActivity);

                    // go downwards:
                    ActivityRecord current = endActivity;
                    while (true)
                    {
                        var previous = Stamp.ActivityRecords.ElementAtOrDefault(Stamp.ActivityRecords.IndexOf(current) - 1);
                        if (previous == null || previous.Activity != current.Activity || !previous.End.HasValue || previous.End.Value != current.Begin.Value)
                            break;
                        adjoining.Add(previous);
                        current = previous;
                    }

                    // go upwards:
                    current = startActivity;
                    while (true)
                    {
                        var next = Stamp.ActivityRecords.ElementAtOrDefault(Stamp.ActivityRecords.IndexOf(current) + 1);
                        if (next == null || next.Activity != current.Activity || !next.Begin.HasValue || next.Begin.Value != current.End.Value)
                            break;
                        adjoining.Add(next);
                        current = next;
                    }
                }
            }
            var mergeAll = new ToolStripMenuItem($"Merge all {adjoining.Count} adjoining Activities", null, (ss, ee) =>
            {
                var ordered = adjoining.OrderBy(a => a.Begin.Value).ToArray();
                foreach (var activity in ordered.Take(adjoining.Count - 1))
                    Manager.DeleteActivity(Stamp, activity);
                TimeManager.SetActivityBegin(Stamp, ordered.Last(), ordered.First().Begin.Value);
                RequestRefresh.Invoke();
                //UpdateTimeline();
            });
            mergeAll.Enabled = adjoining.Count > 2;
            menu.Items.Add(mergeAll);

            menu.Show(timelineControl1, e.Location);
        }

        private void TimeLine_AddSection(TimelineControl sender, double start, double end, MouseEventArgs e)
        {
            // zunächst noch ein context menu anzeigen zum activity auswählen:
            var selectActivityMenu = new ContextMenuStrip();

            foreach (var trackedAct in Settings.TrackedActivities)
            {
                string temp = trackedAct;
                var menuItem = new ToolStripMenuItem(temp, null,
                    (ss, ee) => { InsertActivity(temp, TimeSpan.FromMinutes(start), TimeSpan.FromMinutes(end)); });
                selectActivityMenu.Items.Add(menuItem);

                var mostFrequent = Manager.GetMostFrequentTags(trackedAct, 10);
                if (mostFrequent.Any())
                {
                    foreach (var combination in mostFrequent)
                    {
                        var tempTags = combination;
                        var quickTagMenu = new ToolStripMenuItem(String.Join(", ", tempTags), null, (ss, ee) =>
                        {
                            InsertActivity(temp, TimeSpan.FromMinutes(start), TimeSpan.FromMinutes(end), tempTags);
                        });
                        menuItem.DropDownItems.Add(quickTagMenu);
                    }
                }
            }

            selectActivityMenu.Show(sender, e.Location);
        }

        private void InsertActivity(string activityName, TimeSpan start, TimeSpan end, string[] tags = null)
        {
            // do not allow inserting activity after 'open end' activity (currently running)
            // TODO: this would not be such a bad feature; however, needs to be integrated properly, e.g. running activity should stop automatically when reaching the next already setup activity, and should show popup notification dialog etc.
            if (Stamp.ActivityRecords.FirstOrDefault(a => a.End == null) is var openEnd && openEnd != null && start > openEnd.Begin.Value)
                return;

            var activity = new ActivityRecord()
            {
                Activity = activityName,
                Begin = start,
                End = end,
            };

            if (tags != null)
                activity.Tags = tags.ToList();

            // add activity as-is:
            Stamp.ActivityRecords.Add(activity);

            // then call the proper methods to set begin/end, this will correct overlapping / shadowed activities etc:
            TimeManager.SetActivityBegin(Stamp, activity, activity.Begin.Value);
            TimeManager.SetActivityEnd(Stamp, activity, activity.End.Value);

            // then correct day stamp begin/end time:
            if (Stamp.GetFirstActivity() == activity)
                Stamp.Begin = activity.Begin.Value;
            if (Stamp.GetLastActivity() == activity)
                Stamp.End = activity.End.Value;

            RequestRefresh?.Invoke();
        }

        private List<TimelineSection> CreateTimelineSections(Stamp stamp, params TimeSpan?[] dragged)
        {
            var results = new List<TimelineSection>();
            var activities = stamp.ActivityRecords.OrderBy(r => r.Begin).ToArray();
            foreach (var activity in activities)
            {
                Color activityColor = Owner.GetColor(activity.Activity);
                var activityTags = String.Join(", ", activity.Tags);

                TimeSpan? overrideEnd = null;
                DrawingStyle style = DrawingStyle.Solid;
                if (!activity.End.HasValue)
                {
                    if (Width == 0)
                    {
                        // fallback -- control not yet ready -- instead of calculating to min pixel length, make the section at least '10 minutes' long...
                        if (DateTime.Now.TimeOfDay < activity.Begin.Value.Add(TimeSpan.FromMinutes(10)))
                        {
                            overrideEnd = activity.Begin.Value.Add(TimeSpan.FromMinutes(10));
                            style = DrawingStyle.Twisted;
                        }
                    }
                    else
                    {
                        var x = timelineControl1.CalculateX(activity.Begin.Value.TotalMinutes);
                        var x2 = timelineControl1.CalculateX(DateTime.Now.TimeOfDay.TotalMinutes);
                        int minSizePixel = 10;
                        if (Math.Abs(x2 - x) < minSizePixel)
                        {
                            try
                            {
                                overrideEnd = TimeSpan.FromMinutes(timelineControl1.CalculateValue(x + minSizePixel));
                                style = DrawingStyle.Twisted;
                            }
                            catch (Exception e)
                            {
                                Log.Add($"EXCEPTION: {e.Message} Activity Begin TotalMinutes: {activity.Begin.Value.TotalMinutes}, x: {x}, x2: {x2}, Calculated value: {timelineControl1.CalculateValue(x + minSizePixel)}, Control Width: {timelineControl1.Width}, Control Padd: {timelineControl1.Padding.Horizontal}, MinimumVal: {timelineControl1.MinimumValue}, MaximumVal: {timelineControl1.MaximumValue}");
                                return results;
                            }
                        }
                    }
                }

                var activitySection = new TimelineSection(activity, activity.Begin.Value, overrideEnd ?? activity.End ?? DateTime.Now.TimeOfDay, activity.End.HasValue)
                {
                    Style = style,
                    ForeColor = activityColor,
                    TooltipHeader = activity.Activity,
                    TooltipBody = String.Join(Environment.NewLine, new[] { (activity.End.HasValue ? "" : "This activity is currently running..."), activityTags, activity.Comment }.Where(s => !String.IsNullOrEmpty(s))),
                    TooltipDurationCustomText = activity.End != null && activity.Begin != null ? $"Duration: {Manager.FormatTimeSpan(activity.End.Value - activity.Begin.Value)}" : null,
                    DisplayText = activity.Activity,
                };

                // priorization of shown timestamps:
                // - dragged node = 7
                if (activity.Begin.HasValue && dragged.Any(d => d.HasValue && d.Value == activity.Begin.Value))
                    activitySection.Start.DisplayTextOrder = 7;
                if (activity.End.HasValue && dragged.Any(d => d.HasValue && d.Value == activity.End.Value))
                    activitySection.End.DisplayTextOrder = 7;
                // - ??? highlighted node = 6
                // - start node = 5
                if (activitySection.Start != null && activity == activities.First())
                    activitySection.Start.DisplayTextOrder = 5;
                // - end node = 4
                if (activity == activities.Last())
                {
                    if (activitySection.End != null && activitySection.End.IsVisible)
                        activitySection.End.DisplayTextOrder = 4;
                    else
                        activitySection.Start.DisplayTextOrder = 4;
                }
                // - node before pause = 3
                if (!activities.Any(a => activity.End.HasValue && a.Begin.Value == activity.End.Value))
                    activitySection.End.DisplayTextOrder = 3;
                // - node after pause = 2
                if (!activities.Any(a => a.End.HasValue && a.End.Value == activity.Begin.Value))
                    activitySection.Start.DisplayTextOrder = 2;
                // -> all others = 1 (default value)

                results.Add(activitySection);
            }
            return results;
        }


        private void btnStartActivity_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();

            Owner.CreateStartActivityContextMenuStrip(menu, false);

            menu.Show(btnStartActivity, new Point(btnStartActivity.Size.Width, btnStartActivity.Size.Height), ToolStripDropDownDirection.BelowLeft);
        }

        //private void txtComment_TextChanged(object sender, EventArgs e)
        //{
        //    //Stamp.Comment = txtComment.Text;

        //    RequestRefresh.Invoke();
        //    //UpdateTimeline();
        //}

        private void txtWorkingHours_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtWorkingHours.Text))
                return;
            if (TimeSettings.Integer.IsMatch(txtWorkingHours.Text))
                return;
            Stamp.WorkingHours = Convert.ToInt32(txtWorkingHours.Text);

            RequestRefresh.Invoke();
        }

        private void txtBegin_TextChanged(object sender, EventArgs e)
        {
            if (TimeManager.TryParseHHMM(txtBegin.Text, out TimeSpan newBegin))
            {
                TimeManager.SetBegin(Stamp, newBegin);
                RequestRefresh.Invoke();
            }
        }

        private void txtEnd_TextChanged(object sender, EventArgs e)
        {
            if (TimeManager.TryParseHHMM(txtEnd.Text, out TimeSpan newEnd))
            {
                TimeManager.SetEnd(Stamp, newEnd);
                RequestRefresh.Invoke();
            }
        }

        private void txtPause_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtPause.Text, out int pauseMins))
            {
                Manager.SetPause(Stamp, TimeSpan.FromMinutes(pauseMins), true);
                RequestRefresh.Invoke();
            }
        }
    }
}
