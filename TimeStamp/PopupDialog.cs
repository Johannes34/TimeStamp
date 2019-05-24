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
    public partial class PopupDialog : Form
    {
        public static void Initialize(Form1 owner)
        {
            DefaultOwner = owner;
            Manager = owner.Manager;
        }

        public static IWin32Window DefaultOwner { get; set; }
        public static TimeManager Manager { get; set; }


        public PopupDialog(string displayText, DateTime? showCountdownToDateTime, TimeSpan? showDuration, string suggestedActivity = null)
        {
            InitializeComponent();
            m_timer.Tick += m_timer_Tick;

            DisplayText = displayText;
            ShowDuration = showDuration;
            DisplayCountdown = showCountdownToDateTime;

            if (!String.IsNullOrEmpty(suggestedActivity))
            {
                btnChangeActivity.Text = $"Change to '{suggestedActivity}'";
                btnChangeActivity.Visible = true;
                btnChangeActivity.Click += (s, e) =>
                {
                    Manager.StartNewActivity(suggestedActivity, null);
                    this.Close();
                };
            }
        }

        public static PopupDialog Show8HrsIn5Minutes(DateTime endingTime)
        {
            string text;
            if (endingTime < Manager.Time.Now)
                text = "Today's total was 8hrs...";
            else
                text = "Today's total is 8hrs...";
            var diag = new PopupDialog(text, endingTime, TimeSpan.FromSeconds(8));
            diag.StartShowing(DefaultOwner);
            return diag;
        }

        public static PopupDialog ShowPlannedEndIn5Minutes(DateTime endingTime)
        {
            var diag = new PopupDialog("Remember today's end...", endingTime, TimeSpan.FromSeconds(8));
            diag.StartShowing(DefaultOwner);
            return diag;
        }

        public static PopupDialog ShowAfterPause(TimeSpan pauseDuration, string activity)
        {
            var diag = new PopupDialog(String.Format("Today's pause was {0} minutes. You are continuing with '{1}'...", pauseDuration.TotalMinutes, activity), null, TimeSpan.FromSeconds(8));
            diag.StartShowing(DefaultOwner);
            return diag;
        }

        public static PopupDialog ShowCurrentlyTrackingActivity(string activity)
        {
            var diag = new PopupDialog(String.Format("Your currently tracking activity is: {0}", activity), null, TimeSpan.FromSeconds(8));
            diag.StartShowing(DefaultOwner);
            return diag;
        }

        public static PopupDialog ShowCurrentlyTrackingActivityWithChangeToActivityButton(string activity, string suggestedActivity)
        {
            var diag = new PopupDialog(String.Format("Your currently tracking activity is: {0}", activity), null, TimeSpan.FromSeconds(8), suggestedActivity);
            diag.StartShowing(DefaultOwner);
            return diag;
        }

        private Timer m_timer = new Timer() { Interval = 100, Enabled = true };

        public TimeSpan? ShowDuration { get; set; }
        private DateTime m_openedTime { get; set; }

        private string m_displayText;
        public string DisplayText
        {
            get
            {
                return m_displayText;
            }
            set
            {
                m_displayText = value;
                lblText1.Text = m_displayText;
                AdjustLblCountdown();
                AdjustFormSize();
            }
        }

        private DateTime? m_displayCountdown;
        public DateTime? DisplayCountdown
        {
            get
            {
                return m_displayCountdown;
            }
            set
            {
                m_displayCountdown = value;
                lblCountdown.Text = DisplayCountdownText();
                AdjustLblCountdown();
                AdjustFormSize();
            }
        }

        private string DisplayCountdownText()
        {
            if (DisplayCountdown.HasValue)
            {
                var remaining = DisplayCountdown.Value - Manager.Time.Now;
                if (remaining > TimeSpan.Zero)
                    return String.Format(@"... in {0:hh\:mm\:ss} minutes", remaining);
                else if (remaining == TimeSpan.Zero)
                    return "... now!";
                else
                    return String.Format(@"... {0:hh\:mm\:ss} minutes ago", remaining);
            }
            return String.Empty;
        }
        private void AdjustLblCountdown()
        {
            if (m_displayCountdown.HasValue)
            {
                lblCountdown.Visible = true;
                lblCountdown.Top = lblText1.Top + lblText1.Height + lblText1.Margin.Bottom + lblCountdown.Margin.Top;
                lblCountdown.Left = Math.Max((lblText1.Left + lblText1.Width) - lblCountdown.Width, lblText1.Left);
            }
            else
                lblCountdown.Visible = false;
        }
        private void AdjustFormSize()
        {
            if (lblCountdown.Visible)
            {
                this.Width = Math.Max(lblText1.Left + lblText1.Width + lblText1.Margin.Left + lblText1.Left,
                    lblCountdown.Left + lblCountdown.Width + lblCountdown.Margin.Left + lblCountdown.Left);
                this.Height = lblCountdown.Top + lblCountdown.Height + lblCountdown.Margin.Top + lblCountdown.Top;
            }
            else if (btnChangeActivity.Visible)
            {
                this.Width = Math.Max(lblText1.Left + lblText1.Width + lblText1.Margin.Left + lblText1.Left,
                    btnChangeActivity.Left + btnChangeActivity.Width + btnChangeActivity.Margin.Left + btnChangeActivity.Left);
                this.Height = btnChangeActivity.Top + btnChangeActivity.Height + btnChangeActivity.Margin.Top + btnChangeActivity.Top;
            }
            else
            {
                this.Width = lblText1.Left + lblText1.Width + lblText1.Margin.Left + lblText1.Left;
                this.Height = lblText1.Top + lblText1.Height + lblText1.Margin.Top + lblText1.Top;
            }
        }

        public void StartShowing(IWin32Window owner)
        {
            if (Manager?.Settings != null && Manager.Settings.DisablePopupNotifications)
                return;

            this.Opacity = 0;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - (this.Width + 40), Screen.PrimaryScreen.WorkingArea.Height - (this.Height + 40));
            base.Show(owner);
            System.Media.SystemSounds.Hand.Play();

            m_openedTime = Manager.Time.Now;
            new Task(() =>
            {
                while ((bool)this.Invoke(new Func<bool>(() => this.Opacity < 1)))
                {
                    System.Threading.Thread.Sleep(10);
                    this.Invoke(new Action(() => { this.Opacity += 0.01; }));
                }
                if (ShowDuration.HasValue || DisplayCountdown.HasValue)
                    m_timer.Start();
            }).Start();
        }

        public void StartClosing()
        {
            ShowDuration = null;
            new Task(() =>
            {
                try
                {
                    while ((bool)this.Invoke(new Func<bool>(() => this.Opacity > 0)))
                    {
                        System.Threading.Thread.Sleep(10);
                        this.Invoke(new Action(() => { this.Opacity -= 0.01; }));
                    }
                    this.Invoke(new Action(() => { m_timer.Stop(); base.Close(); }));
                }
                catch { } //z.b. programm beenden während popup..
            }).Start();
        }

        private void lblClose_Click(object sender, EventArgs e)
        {
            ShowDuration = null;
            m_timer.Stop();
            base.Close();
        }

        private void lblClose_MouseEnter(object sender, EventArgs e)
        {
            lblClose.ForeColor = Color.Black;
        }

        private void lblClose_MouseLeave(object sender, EventArgs e)
        {
            lblClose.ForeColor = SystemColors.ControlDarkDark;
        }

        private void m_timer_Tick(object sender, EventArgs e)
        {
            if (ShowDuration.HasValue)
            {
                if (Manager.Time.Now > m_openedTime + ShowDuration.Value)
                {
                    this.StartClosing();
                }
            }

            if (DisplayCountdown.HasValue)
            {
                lblCountdown.Text = DisplayCountdownText();
            }
        }

        private void PopupDialog_Paint(object sender, PaintEventArgs e)
        {
        }

    }
}
