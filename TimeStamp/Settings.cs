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

            cbComputerIsLockedWhenLeaving.DataBindings.Add(new Binding(nameof(CheckBox.Checked), settings, nameof(settings.IsLockingComputerWhenLeaving), false, DataSourceUpdateMode.OnPropertyChanged));
            cbComputerIsLockedWhenLeaving.DataBindings.Add(new Binding(nameof(CheckBox.Enabled), settings, nameof(settings.AutomaticPauseRecognition)));

            txtMinimumPauseMinutes.DataBindings.Add(new Binding(nameof(TextBox.Text), settings, nameof(settings.AutomaticPauseRecognitionMinPauseTime), false, DataSourceUpdateMode.OnPropertyChanged));
            txtMinimumPauseMinutes.DataBindings.Add(new Binding(nameof(TextBox.Enabled), settings, nameof(settings.AutomaticPauseRecognition)));

            txtMinimumPauseStart.DataBindings.Add(new Binding(nameof(TextBox.Text), this, nameof(PauseStartTime), false, DataSourceUpdateMode.OnPropertyChanged));
            txtMinimumPauseStart.DataBindings.Add(new Binding(nameof(TextBox.Enabled), settings, nameof(settings.AutomaticPauseRecognition)));

            txtMaximumPauseEnd.DataBindings.Add(new Binding(nameof(TextBox.Text), this, nameof(PauseEndTime), false, DataSourceUpdateMode.OnPropertyChanged));
            txtMaximumPauseEnd.DataBindings.Add(new Binding(nameof(TextBox.Enabled), settings, nameof(settings.AutomaticPauseRecognition)));


            // General:

            txtDefaultHours.DataBindings.Add(new Binding(nameof(TextBox.Text), settings, nameof(settings.DefaultWorkingHours), false, DataSourceUpdateMode.OnPropertyChanged));


            // Notifications:

            cbRemindVpn.DataBindings.Add(new Binding(nameof(CheckBox.Checked), settings, nameof(settings.RemindCurrentActivityWhenChangingVPN), false, DataSourceUpdateMode.OnPropertyChanged));
            txtVpnName.DataBindings.Add(new Binding(nameof(TextBox.Text), settings, nameof(settings.RemindCurrentActivityWhenChangingVPNWithName), false, DataSourceUpdateMode.OnPropertyChanged));
            txtVpnName.DataBindings.Add(new Binding(nameof(TextBox.Enabled), settings, nameof(settings.RemindCurrentActivityWhenChangingVPN)));

        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private TimeManager m_manager;

        public string PauseStartTime
        {
            get
            {
                return TimeManager.ParseHHMM(m_manager.Settings.AutomaticPauseRecognitionStartTime);
            }
            set
            {
                if (TimeManager.TryParseHHMM(value, out TimeSpan res))
                    m_manager.Settings.AutomaticPauseRecognitionStartTime = res;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PauseStartTime)));
            }
        }

        public string PauseEndTime
        {
            get
            {
                return TimeManager.ParseHHMM(m_manager.Settings.AutomaticPauseRecognitionStopTime);
            }
            set
            {
                if (TimeManager.TryParseHHMM(value, out TimeSpan res))
                    m_manager.Settings.AutomaticPauseRecognitionStopTime = res;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PauseEndTime)));
            }
        }

        private void btnManageActivities_Click(object sender, EventArgs e)
        {
            var diag = new ManageActivities(m_manager);

            diag.ShowDialog(this);
        }
    }
}
