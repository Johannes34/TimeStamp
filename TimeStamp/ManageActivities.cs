using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeStamp
{
    public partial class ManageActivities : Form
    {
        public ManageActivities(TimeManager manager)
        {
            m_manager = manager;
            m_settings = manager.Settings;
            InitializeComponent();
        }

        private TimeSettings m_settings;
        private TimeManager m_manager;

        private void ManageActivities_Load(object sender, EventArgs e)
        {
            FillGrid();
        }

        private void FillGrid()
        {
            grdActivities.Rows.Clear();

            foreach (var activity in m_settings.TrackedActivities)
            {
                int index = grdActivities.Rows.Add(activity, "" /*TODO*/, activity == m_settings.AlwaysStartNewDayWithActivity, "");
                grdActivities.Rows[index].Tag = activity;

                var button = grdActivities.Rows[index].Cells[m_deleteColumnIndex] as DataGridViewButtonCell;

                var hasAffectedRecords = m_manager.StampList.SelectMany(s => s.ActivityRecords).Any(r => r.Activity == activity);
                if (hasAffectedRecords)
                    button.Style.ForeColor = SystemColors.ControlDark;
            }

            grdTags.Rows.Clear();
            foreach (var category in m_settings.Tags)
            {
                foreach (var tag in category.Value)
                {
                    int index = grdTags.Rows.Add(category.Key, tag, "");
                    grdTags.Rows[index].Tag = tag;

                    var button = grdTags.Rows[index].Cells[m_tagDeleteColumnIndex] as DataGridViewButtonCell;

                    var hasAffectedRecords = m_manager.StampList.SelectMany(s => s.ActivityRecords).Any(r => r.Tags.Contains(tag));
                    if (hasAffectedRecords)
                        button.Style.ForeColor = SystemColors.ControlDark;
                }
            }

            chkEnableAutoTrackApplications.Checked = m_settings.EnableAutoTrackingApplications;
            grdAutoTrackApplications.Rows.Clear();
            foreach (var app in m_settings.AutoTrackingApplications)
            {
                int index = grdAutoTrackApplications.Rows.Add(app.Key, app.Value, "");
                grdAutoTrackApplications.Rows[index].Tag = app.Value;

                var button = grdAutoTrackApplications.Rows[index].Cells[m_appDeleteColumnIndex] as DataGridViewButtonCell;

                var hasAffectedRecords = m_manager.StampList.SelectMany(s => s.ActivityRecords).Any(r => r.Tags.Contains(app.Value));
                if (hasAffectedRecords)
                    button.Style.ForeColor = SystemColors.ControlDark;
            }

            chkEnableAutoTagging.Checked = m_settings.EnableAutoTagging;
            grdAutoTag.Rows.Clear();
            foreach (var tag in m_settings.AutoTagging)
            {
                int index = grdAutoTag.Rows.Add(tag.Key, tag.Value, "");
            }

            UpdateEnabled();
        }



        // Activities Grid:

        private int m_nameColumnIndex = 0;
        private int m_commentColumnIndex = 1;
        private int m_isDefaultColumnIndex = 2;
        private int m_deleteColumnIndex = 3;

        private void grdActivities_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            var currentRow = grdActivities.Rows[e.RowIndex];

            var currentActivity = grdActivities.Rows[e.RowIndex].Cells[m_nameColumnIndex].Value as string;

            if (e.ColumnIndex == m_nameColumnIndex)
            {
                // rename activity:

                string oldName = currentRow.Tag as string;
                string newName = currentActivity;

                var affectedDays = m_manager.StampList.Where(s => s.ActivityRecords.Any(r => r.Activity == oldName)).ToList();
                var affectedRecords = m_manager.StampList.SelectMany(s => s.ActivityRecords).Where(r => r.Activity == oldName).ToList();

                DialogResult result;

                // new activity -> just rename without asking
                // nothing affected -> just rename without asking
                if (oldName == null || affectedRecords.Count == 0)
                    result = DialogResult.Yes;
                // otherwise -> ask whether to really rename?
                else
                    result = MessageBox.Show(this, $"Really rename activity '{oldName}' to '{newName}' for all stamps? ({affectedDays.Count} days / {affectedRecords.Count} records affected)", "Rename Activity?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    currentRow.Tag = newName;
                    foreach (var affect in affectedRecords)
                    {
                        affect.Activity = newName;
                    }

                    int index = m_settings.TrackedActivities.IndexOf(oldName);
                    if (index != -1)
                        m_settings.TrackedActivities.Remove(oldName); // remove existing
                    else
                        index = m_settings.TrackedActivities.Count; // add as new at the end of the list

                    if (!m_settings.TrackedActivities.Contains(newName))
                        m_settings.TrackedActivities.Insert(index, newName);

                    if (m_settings.AlwaysStartNewDayWithActivity == oldName)
                        m_settings.AlwaysStartNewDayWithActivity = newName;

                    // if new name already exists, those two activities have been merged -> show only one entry in grid

                    foreach (var row in grdActivities.Rows.OfType<DataGridViewRow>().ToArray())
                    {
                        if (row.Tag as string == newName && row != currentRow)
                        {
                            grdActivities.Rows.Remove(row);
                        }
                    }

                    if (m_settings.AlwaysStartNewDayWithActivity == newName)
                        currentRow.Cells[m_isDefaultColumnIndex].Value = true;
                }
                else
                {
                    grdActivities.Rows[e.RowIndex].Cells[m_nameColumnIndex].Value = oldName;
                }
            }
            else if (e.ColumnIndex == m_commentColumnIndex)
            {

            }
            else if (e.ColumnIndex == m_isDefaultColumnIndex)
            {
                // set default activity for new days:

                // reset all other values:
                foreach (DataGridViewRow row in grdActivities.Rows)
                {
                    row.Cells[m_isDefaultColumnIndex].Value = false;
                }

                // set current value:
                m_settings.AlwaysStartNewDayWithActivity = (bool)currentRow.Cells[m_isDefaultColumnIndex].Value ? (string)currentRow.Cells[m_nameColumnIndex].Value : null;
            }
        }

        private void grdActivities_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                // delete activity button clicked:

                var currentActivity = grdActivities.Rows[e.RowIndex].Tag as string;

                var affectedDays = m_manager.StampList.Where(s => s.ActivityRecords.Any(r => r.Activity == currentActivity)).ToList();
                var affectedRecords = m_manager.StampList.SelectMany(s => s.ActivityRecords).Where(r => r.Activity == currentActivity).ToList();

                if (affectedRecords.Count == 0)
                {
                    // no worries, do it:
                    m_settings.TrackedActivities.Remove(currentActivity);

                    if (m_settings.AlwaysStartNewDayWithActivity == currentActivity)
                        m_settings.AlwaysStartNewDayWithActivity = null;
                }
                else
                {
                    // can not delete:
                    var result = MessageBox.Show(this, $"Can not delete activity '{currentActivity}' because it is tracked in {affectedDays.Count} days / {affectedRecords.Count} records. Please instead rename it to a name of another existing activity to merge them.", "Can not delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                FillGrid();
            }
        }

        private void grdActivities_CurrentCellChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }


        private void UpdateEnabled()
        {
            // Activities:
            var selectedCell = grdActivities.SelectedCells.OfType<DataGridViewCell>().FirstOrDefault();

            var activity = m_settings.TrackedActivities.FirstOrDefault(a => selectedCell?.OwningRow.Tag as string == a);
            var index = m_settings.TrackedActivities.IndexOf(activity);

            bool hasSelection = selectedCell != null && selectedCell.OwningRow.Tag != null;

            btnMoveUp.Enabled = hasSelection && index > 0;
            btnMoveDown.Enabled = hasSelection && index < m_settings.TrackedActivities.Count - 1;
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            Move(true);
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            Move(false);
        }

        private void Move(bool up)
        {
            var selectedCell = grdActivities.SelectedCells.OfType<DataGridViewCell>().FirstOrDefault();
            int column = selectedCell.ColumnIndex;
            var activity = m_settings.TrackedActivities.FirstOrDefault(a => selectedCell?.OwningRow.Tag as string == a);
            if (activity != null)
            {
                var index = m_settings.TrackedActivities.IndexOf(activity);
                m_settings.TrackedActivities.Remove(activity);
                m_settings.TrackedActivities.Insert(index + (up ? -1 : +1), activity);

                grdActivities.CurrentCellChanged -= grdActivities_CurrentCellChanged;
                FillGrid();
                var row = grdActivities.Rows.OfType<DataGridViewRow>().FirstOrDefault(r => r.Tag as string == activity);
                if (row != null)
                    row.Cells[column].Selected = true;
                grdActivities.CurrentCellChanged += grdActivities_CurrentCellChanged;

                UpdateEnabled();
            }
        }



        // Tags Grid:

        private int m_tagCategoryColumnIndex = 0;
        private int m_tagNameColumnIndex = 1;
        private int m_tagDeleteColumnIndex = 2;

        private void grdTags_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            var currentRow = grdTags.Rows[e.RowIndex];

            var currentCategory = grdTags.Rows[e.RowIndex].Cells[m_tagCategoryColumnIndex].Value as string;
            var currentTag = grdTags.Rows[e.RowIndex].Cells[m_tagNameColumnIndex].Value as string;

            if (e.ColumnIndex == m_tagCategoryColumnIndex)
            {
                // rename category:
                // no prob, since category is not saved to xml / data objects...
                DeleteTagFromSettings(currentTag);
                AddTagToSettings(currentTag, currentCategory);
            }
            else if (e.ColumnIndex == m_tagNameColumnIndex)
            {
                // rename tag:

                string oldName = currentRow.Tag as string;
                string newName = currentTag;

                var affectedDays = m_manager.StampList.Where(s => s.ActivityRecords.Any(r => r.Tags.Contains(oldName))).ToList();
                var affectedRecords = m_manager.StampList.SelectMany(s => s.ActivityRecords).Where(r => r.Tags.Contains(oldName)).ToList();

                DialogResult result;
                bool alreadyContainsTag = m_settings.Tags.Any(t => t.Value.Contains(newName));

                // new activity -> just rename without asking
                // nothing affected -> just rename without asking
                if (oldName == null || affectedRecords.Count == 0)
                    result = DialogResult.Yes;
                // otherwise -> ask whether to really rename?
                else
                    result = MessageBox.Show(this, $"Really rename tag '{oldName}' to '{newName}' for all stamps? ({affectedDays.Count} days / {affectedRecords.Count} activities affected).{(alreadyContainsTag ? (Environment.NewLine + "Please note: This tag already exists in another category. Renaming it will merge this tag into the existing category.") : String.Empty)}", "Rename Tag?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    currentRow.Tag = newName;
                    foreach (var affect in affectedRecords)
                    {
                        affect.Tags.Remove(oldName);
                        affect.Tags.Add(newName);
                    }

                    DeleteTagFromSettings(oldName);
                    AddTagToSettings(newName, currentCategory); // on tag merging, this method will return 'false', as the tag already exists...

                    // refresh grid:
                    FillGrid();
                }
                else
                {
                    grdTags.Rows[e.RowIndex].Cells[m_tagNameColumnIndex].Value = oldName;
                }
            }
        }

        private void grdTags_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                // delete activity button clicked:

                var currentTag = grdTags.Rows[e.RowIndex].Tag as string;

                var affectedDays = m_manager.StampList.Where(s => s.ActivityRecords.Any(r => r.Tags.Contains(currentTag))).ToList();
                var affectedRecords = m_manager.StampList.SelectMany(s => s.ActivityRecords).Where(r => r.Tags.Contains(currentTag)).ToList();

                if (affectedRecords.Any())
                {
                    var result = MessageBox.Show(this, $"Really delete tag '{currentTag}'? It is set on {affectedDays.Count} days / {affectedRecords.Count} activities. This will remove this tag from all these days/activities.", "Delete Tag?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                        return;
                }

                // delete from activities:
                foreach (var record in affectedRecords)
                    record.Tags.Remove(currentTag);

                // delete from settings:
                DeleteTagFromSettings(currentTag);

                // delete from grid:
                FillGrid();
            }
        }

        private void DeleteTagFromSettings(string currentTag)
        {
            foreach (var category in m_settings.Tags.ToList())
            {
                foreach (var tag in category.Value.ToList())
                {
                    if (tag == currentTag)
                        m_settings.Tags[category.Key].Remove(tag);
                }
                if (m_settings.Tags[category.Key].Count == 0)
                    m_settings.Tags.Remove(category.Key);
            }
        }

        private bool AddTagToSettings(string tag, string category)
        {
            if (m_settings.Tags.Any(k => k.Value.Contains(tag)))
                return false;

            if (!m_settings.Tags.ContainsKey(category))
                m_settings.Tags.Add(category, new List<string>());

            m_settings.Tags[category].Add(tag);
            return true;
        }



        // Auto-Track Apps Grid:

        private int m_appDeleteColumnIndex = 2;

        private void chkEnableAutoTrackApplications_CheckedChanged(object sender, EventArgs e)
        {
            m_settings.EnableAutoTrackingApplications = chkEnableAutoTrackApplications.Checked;
        }

        private void grdAutoTrackApplications_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            var currentRow = grdAutoTrackApplications.Rows[e.RowIndex];

            var appWindowTitle = grdAutoTrackApplications.Rows[e.RowIndex].Cells[0].Value as string;
            var appName = grdAutoTrackApplications.Rows[e.RowIndex].Cells[1].Value as string;

            if (e.ColumnIndex == 0)
            {
                // update app window title:
                // no prob, since title key is not saved to xml / data objects...
                RemoveAppFromSettings(appWindowTitle);
                AddAppToSettings(appWindowTitle, appName);
            }
            else if (e.ColumnIndex == 1)
            {
                // update app name:

                string oldName = currentRow.Tag as string;
                string newName = appName;

                var affectedDays = m_manager.StampList.Where(s => s.ActivityRecords.Any(r => r.Tags.Contains(oldName))).ToList();
                var affectedRecords = m_manager.StampList.SelectMany(s => s.ActivityRecords).Where(r => r.Tags.Contains(oldName)).ToList();

                DialogResult result;

                // new activity -> just rename without asking
                // nothing affected -> just rename without asking
                if (oldName == null || affectedRecords.Count == 0)
                    result = DialogResult.Yes;
                // otherwise -> ask whether to really rename?
                else
                    result = MessageBox.Show(this, $"Really rename app '{oldName}' to '{newName}' for all stamps? ({affectedDays.Count} days / {affectedRecords.Count} activities affected).", "Rename App?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    currentRow.Tag = newName;
                    foreach (var affect in affectedRecords)
                    {
                        affect.Tags.Remove(oldName);
                        affect.Tags.Add(newName);
                    }

                    RemoveAppFromSettings(appWindowTitle);
                    AddAppToSettings(appWindowTitle, newName); // on tag merging, this method will return 'false', as the tag already exists...

                    // refresh grid:
                    FillGrid();
                }
                else
                {
                    grdAutoTrackApplications.Rows[e.RowIndex].Cells[1].Value = oldName;
                }
            }
        }

        private void grdAutoTrackApplications_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                // delete activity button clicked:

                var app = senderGrid.Rows[e.RowIndex].Tag as string;

                var affectedDays = m_manager.StampList.Where(s => s.ActivityRecords.Any(r => r.Tags.Contains(app))).ToList();
                var affectedRecords = m_manager.StampList.SelectMany(s => s.ActivityRecords).Where(r => r.Tags.Contains(app)).ToList();

                if (affectedRecords.Any())
                {
                    var result = MessageBox.Show(this, $"Really delete app '{app}'? It is set on {affectedDays.Count} days / {affectedRecords.Count} activities. This will remove this app from all these days/activities.", "Delete App?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                        return;
                }

                // delete from activities:
                foreach (var record in affectedRecords)
                    record.Tags.Remove(app);

                // delete from settings:
                var appWindowTitle = senderGrid.Rows[e.RowIndex].Cells[0].Value as string;
                RemoveAppFromSettings(appWindowTitle);

                // delete from grid:
                FillGrid();
            }
        }

        private void RemoveAppFromSettings(string appWindowTitle)
        {
            if (m_settings.AutoTrackingApplications.ContainsKey(appWindowTitle))
                m_settings.AutoTrackingApplications.Remove(appWindowTitle);
        }

        private bool AddAppToSettings(string appWindowTitle, string appName)
        {
            if (m_settings.AutoTrackingApplications.ContainsKey(appWindowTitle))
                return false;

            m_settings.AutoTrackingApplications.Add(appWindowTitle, appName);
            return true;
        }


        // Auto-Tag Grid:

        private void chkEnableAutoTagging_CheckedChanged(object sender, EventArgs e)
        {
            m_settings.EnableAutoTagging = chkEnableAutoTagging.Checked;
        }

        private void grdAutoTag_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            var currentRow = grdAutoTag.Rows[e.RowIndex];

            var regex = grdAutoTag.Rows[e.RowIndex].Cells[0].Value as string;
            var tag = grdAutoTag.Rows[e.RowIndex].Cells[1].Value as string;

            if (e.ColumnIndex == 0)
            {
                // update regex:
                // no prob, since title key is not saved to xml / data objects...
                RemoveAutoTagFromSettings(regex);
                AddAutoTagToSettings(regex, tag);
            }
            else if (e.ColumnIndex == 1)
            {
                // update category:tag:

                RemoveAutoTagFromSettings(regex);
                AddAutoTagToSettings(regex, tag);

                // refresh grid:
                FillGrid();
            }
        }

        private void grdAutoTag_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                // delete button clicked:

                // delete from settings:
                var regex = senderGrid.Rows[e.RowIndex].Cells[0].Value as string;
                RemoveAutoTagFromSettings(regex);

                // delete from grid:
                FillGrid();
            }
        }

        private void RemoveAutoTagFromSettings(string regex)
        {
            if (m_settings.AutoTagging.ContainsKey(regex))
                m_settings.AutoTagging.Remove(regex);
        }

        private bool AddAutoTagToSettings(string regex, string tag)
        {
            if (m_settings.AutoTagging.ContainsKey(regex))
                return false;

            m_settings.AutoTagging.Add(regex, tag);
            return true;
        }

    }
}
