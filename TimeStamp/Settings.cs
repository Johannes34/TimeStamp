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
    public partial class Settings : Form, INotifyPropertyChanged
    {

        public Settings(TimeManager manager)
        {
            InitializeComponent();

            m_manager = manager;

            var settings = manager.Settings;

            // Pause Recognition:

            cbEnablePauseRec.DataBindings.Add(new Binding(nameof(CheckBox.Checked), settings, nameof(settings.AutomaticPauseRecognition), false, DataSourceUpdateMode.OnPropertyChanged));

            txtMinimumPauseMinutes.DataBindings.Add(new Binding(nameof(TextBox.Text), settings, nameof(settings.AutomaticPauseRecognitionMinPauseTime), false, DataSourceUpdateMode.OnPropertyChanged));
            txtMinimumPauseMinutes.DataBindings.Add(new Binding(nameof(TextBox.Enabled), settings, nameof(settings.AutomaticPauseRecognition)));

            // General:

            txtDefaultHours.DataBindings.Add(new Binding(nameof(TextBox.Text), settings, nameof(settings.DefaultWorkingHours), false, DataSourceUpdateMode.OnPropertyChanged));
            txtDefaultHoursSa.DataBindings.Add(new Binding(nameof(TextBox.Text), settings, nameof(settings.DefaultWorkingHoursSaturday), false, DataSourceUpdateMode.OnPropertyChanged));
            txtDefaultHoursSo.DataBindings.Add(new Binding(nameof(TextBox.Text), settings, nameof(settings.DefaultWorkingHoursSunday), false, DataSourceUpdateMode.OnPropertyChanged));


            // Notifications:

            cbRemindVpn.DataBindings.Add(new Binding(nameof(CheckBox.Checked), settings, nameof(settings.RemindCurrentActivityWhenChangingVPN), false, DataSourceUpdateMode.OnPropertyChanged));
            txtVpnName.DataBindings.Add(new Binding(nameof(TextBox.Text), settings, nameof(settings.RemindCurrentActivityWhenChangingVPNWithName), false, DataSourceUpdateMode.OnPropertyChanged));
            txtVpnName.DataBindings.Add(new Binding(nameof(TextBox.Enabled), settings, nameof(settings.RemindCurrentActivityWhenChangingVPN)));

        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private TimeManager m_manager;

        private void btnManageActivities_Click(object sender, EventArgs e)
        {
            var diag = new ManageActivities(m_manager);

            diag.ShowDialog(this);
        }
    }
}
