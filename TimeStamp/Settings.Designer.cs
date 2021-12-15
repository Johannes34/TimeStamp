namespace TimeStamp
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtMaximumPauseEnd = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtMinimumPauseStart = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMinimumPauseMinutes = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbComputerIsLockedWhenLeaving = new System.Windows.Forms.CheckBox();
            this.cbEnablePauseRec = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtDefaultHoursSo = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtDefaultHoursSa = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnManageActivities = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDefaultHours = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cbRemindVpn = new System.Windows.Forms.CheckBox();
            this.txtVpnName = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtMaximumPauseEnd);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtMinimumPauseStart);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtMinimumPauseMinutes);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbComputerIsLockedWhenLeaving);
            this.groupBox1.Controls.Add(this.cbEnablePauseRec);
            this.groupBox1.Location = new System.Drawing.Point(12, 99);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(638, 89);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Automatic Pause Recognition";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(610, 41);
            this.label5.MinimumSize = new System.Drawing.Size(0, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 20);
            this.label5.TabIndex = 42;
            this.label5.Text = ".";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMaximumPauseEnd
            // 
            this.txtMaximumPauseEnd.Location = new System.Drawing.Point(551, 41);
            this.txtMaximumPauseEnd.Name = "txtMaximumPauseEnd";
            this.txtMaximumPauseEnd.Size = new System.Drawing.Size(53, 20);
            this.txtMaximumPauseEnd.TabIndex = 41;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(520, 41);
            this.label4.MinimumSize = new System.Drawing.Size(0, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(25, 20);
            this.label4.TabIndex = 40;
            this.label4.Text = "and";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMinimumPauseStart
            // 
            this.txtMinimumPauseStart.Location = new System.Drawing.Point(462, 41);
            this.txtMinimumPauseStart.Name = "txtMinimumPauseStart";
            this.txtMinimumPauseStart.Size = new System.Drawing.Size(52, 20);
            this.txtMinimumPauseStart.TabIndex = 39;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(369, 40);
            this.label3.MinimumSize = new System.Drawing.Size(0, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 20);
            this.label3.TabIndex = 38;
            this.label3.Text = "minutes between";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMinimumPauseMinutes
            // 
            this.txtMinimumPauseMinutes.Location = new System.Drawing.Point(322, 40);
            this.txtMinimumPauseMinutes.Name = "txtMinimumPauseMinutes";
            this.txtMinimumPauseMinutes.Size = new System.Drawing.Size(41, 20);
            this.txtMinimumPauseMinutes.TabIndex = 36;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 39);
            this.label2.MinimumSize = new System.Drawing.Size(0, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(287, 20);
            this.label2.TabIndex = 35;
            this.label2.Text = "Set Pause time automatically, when being idle for more than";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbComputerIsLockedWhenLeaving
            // 
            this.cbComputerIsLockedWhenLeaving.Checked = true;
            this.cbComputerIsLockedWhenLeaving.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbComputerIsLockedWhenLeaving.Location = new System.Drawing.Point(32, 62);
            this.cbComputerIsLockedWhenLeaving.Name = "cbComputerIsLockedWhenLeaving";
            this.cbComputerIsLockedWhenLeaving.Size = new System.Drawing.Size(206, 17);
            this.cbComputerIsLockedWhenLeaving.TabIndex = 36;
            this.cbComputerIsLockedWhenLeaving.Text = "The computer is locked when leaving";
            this.cbComputerIsLockedWhenLeaving.UseVisualStyleBackColor = true;
            // 
            // cbEnablePauseRec
            // 
            this.cbEnablePauseRec.Checked = true;
            this.cbEnablePauseRec.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEnablePauseRec.Location = new System.Drawing.Point(12, 19);
            this.cbEnablePauseRec.Name = "cbEnablePauseRec";
            this.cbEnablePauseRec.Size = new System.Drawing.Size(333, 17);
            this.cbEnablePauseRec.TabIndex = 33;
            this.cbEnablePauseRec.Text = "Enable Automatic Lunch Break Recognition (once per day)";
            this.cbEnablePauseRec.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.txtDefaultHoursSo);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.txtDefaultHoursSa);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.btnManageActivities);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtDefaultHours);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(638, 81);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General";
            // 
            // txtDefaultHoursSo
            // 
            this.txtDefaultHoursSo.Location = new System.Drawing.Point(394, 24);
            this.txtDefaultHoursSo.Name = "txtDefaultHoursSo";
            this.txtDefaultHoursSo.Size = new System.Drawing.Size(35, 20);
            this.txtDefaultHoursSo.TabIndex = 39;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(342, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 13);
            this.label7.TabIndex = 38;
            this.label7.Text = "Sunday:";
            // 
            // txtDefaultHoursSa
            // 
            this.txtDefaultHoursSa.Location = new System.Drawing.Point(301, 24);
            this.txtDefaultHoursSa.Name = "txtDefaultHoursSa";
            this.txtDefaultHoursSa.Size = new System.Drawing.Size(35, 20);
            this.txtDefaultHoursSa.TabIndex = 37;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(243, 27);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 13);
            this.label6.TabIndex = 36;
            this.label6.Text = "Saturday:";
            // 
            // btnManageActivities
            // 
            this.btnManageActivities.Location = new System.Drawing.Point(12, 50);
            this.btnManageActivities.Name = "btnManageActivities";
            this.btnManageActivities.Size = new System.Drawing.Size(189, 23);
            this.btnManageActivities.TabIndex = 35;
            this.btnManageActivities.Text = "Manage Activities...";
            this.btnManageActivities.UseVisualStyleBackColor = true;
            this.btnManageActivities.Click += new System.EventHandler(this.btnManageActivities_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(187, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Default working hours per day (Mo-Fr):";
            // 
            // txtDefaultHours
            // 
            this.txtDefaultHours.Location = new System.Drawing.Point(202, 24);
            this.txtDefaultHours.Name = "txtDefaultHours";
            this.txtDefaultHours.Size = new System.Drawing.Size(35, 20);
            this.txtDefaultHours.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.cbRemindVpn);
            this.groupBox3.Controls.Add(this.txtVpnName);
            this.groupBox3.Location = new System.Drawing.Point(12, 194);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(638, 52);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Notifications";
            // 
            // cbRemindVpn
            // 
            this.cbRemindVpn.AutoSize = true;
            this.cbRemindVpn.Location = new System.Drawing.Point(12, 22);
            this.cbRemindVpn.Name = "cbRemindVpn";
            this.cbRemindVpn.Size = new System.Drawing.Size(461, 17);
            this.cbRemindVpn.TabIndex = 1;
            this.cbRemindVpn.Text = "Remind about current activity when entering / leaving VPN connection, optional wi" +
    "th name*:";
            this.cbRemindVpn.UseVisualStyleBackColor = true;
            // 
            // txtVpnName
            // 
            this.txtVpnName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtVpnName.Location = new System.Drawing.Point(479, 20);
            this.txtVpnName.Name = "txtVpnName";
            this.txtVpnName.Size = new System.Drawing.Size(153, 20);
            this.txtVpnName.TabIndex = 0;
            this.toolTip1.SetToolTip(this.txtVpnName, resources.GetString("txtVpnName.ToolTip"));
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 256);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Settings";
            this.Text = "TimeStamp - Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMinimumPauseMinutes;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMinimumPauseStart;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtMaximumPauseEnd;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.CheckBox cbComputerIsLockedWhenLeaving;
        public System.Windows.Forms.CheckBox cbEnablePauseRec;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDefaultHours;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox cbRemindVpn;
        private System.Windows.Forms.TextBox txtVpnName;
        private System.Windows.Forms.ToolTip toolTip1;
        public System.Windows.Forms.Button btnManageActivities;
        private System.Windows.Forms.TextBox txtDefaultHoursSo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtDefaultHoursSa;
        private System.Windows.Forms.Label label6;
    }
}