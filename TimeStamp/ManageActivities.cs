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

        private int m_nameColumnIndex = 0;
        private int m_commentColumnIndex = 1;
        private int m_isDefaultColumnIndex = 2;
        private int m_deleteColumnIndex = 3;

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

            UpdateEnabled();
        }

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

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            Move(true);
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            Move(false);
        }

        private void UpdateEnabled()
        {
            var selectedCell = grdActivities.SelectedCells.OfType<DataGridViewCell>().FirstOrDefault();

            var activity = m_settings.TrackedActivities.FirstOrDefault(a => selectedCell?.OwningRow.Tag as string == a);
            var index = m_settings.TrackedActivities.IndexOf(activity);

            bool hasSelection = selectedCell != null && selectedCell.OwningRow.Tag != null;

            btnMoveUp.Enabled = hasSelection && index > 0;
            btnMoveDown.Enabled = hasSelection && index < m_settings.TrackedActivities.Count - 1;
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
    }
}
