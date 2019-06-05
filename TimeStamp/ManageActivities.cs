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
                int index = grdActivities.Rows.Add(activity, activity == m_settings.AlwaysStartNewDayWithActivity, "");
                grdActivities.Rows[index].Tag = activity;

                var button = grdActivities.Rows[index].Cells[2] as DataGridViewButtonCell;

                var hasAffectedRecords = m_manager.StampList.SelectMany(s => s.ActivityRecords).Any(r => r.Activity == activity);
                if (hasAffectedRecords)
                    button.Style.ForeColor = SystemColors.ControlDark;
            }
        }

        private void grdActivities_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            var currentRow = grdActivities.Rows[e.RowIndex];

            var currentActivity = grdActivities.Rows[e.RowIndex].Cells[0].Value as string;

            if (e.ColumnIndex == 0)
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
                        currentRow.Cells[1].Value = true;
                }
                else
                {
                    grdActivities.Rows[e.RowIndex].Cells[0].Value = oldName;
                }
            }
            else if (e.ColumnIndex == 1)
            {
                // set default activity for new days:

                // reset all other values:
                foreach (DataGridViewRow row in grdActivities.Rows)
                {
                    row.Cells[1].Value = false;
                }

                // set current value:
                m_settings.AlwaysStartNewDayWithActivity = (bool)currentRow.Cells[1].Value ? (string)currentRow.Cells[0].Value : null;
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
    }
}
