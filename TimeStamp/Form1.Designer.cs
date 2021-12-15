namespace TimeStamp
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.btnAddTimestamp = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblTotalBalance = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.middaySpan = new System.Windows.Forms.Timer(this.components);
            this.cmbStatisticRange = new System.Windows.Forms.ComboBox();
            this.cmbStatisticType = new System.Windows.Forms.ComboBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.btnDeleteStamp = new System.Windows.Forms.Button();
            this.btnTakeDayOff = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.flpChartFilter = new System.Windows.Forms.FlowLayoutPanel();
            this.StampCalendar = new System.Windows.Forms.MonthCalendar();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnExportExcelActivities = new System.Windows.Forms.Button();
            this.timelineToday = new TimeStamp.StampTimelineControl();
            this.pnlToday = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.pnlToday.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "TimeStamp";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_Click_1);
            // 
            // btnAddTimestamp
            // 
            this.btnAddTimestamp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddTimestamp.Location = new System.Drawing.Point(6, 3);
            this.btnAddTimestamp.Name = "btnAddTimestamp";
            this.btnAddTimestamp.Size = new System.Drawing.Size(753, 101);
            this.btnAddTimestamp.TabIndex = 39;
            this.btnAddTimestamp.Text = "No record for this day. Click here to add a new entry.";
            this.btnAddTimestamp.UseVisualStyleBackColor = true;
            this.btnAddTimestamp.Visible = false;
            this.btnAddTimestamp.Click += new System.EventHandler(this.btnAddTimestamp_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblTotalBalance);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 463);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(765, 56);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Total";
            // 
            // lblTotalBalance
            // 
            this.lblTotalBalance.AutoSize = true;
            this.lblTotalBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalBalance.Location = new System.Drawing.Point(3, 16);
            this.lblTotalBalance.Name = "lblTotalBalance";
            this.lblTotalBalance.Size = new System.Drawing.Size(209, 13);
            this.lblTotalBalance.TabIndex = 8;
            this.lblTotalBalance.Text = "Total Balance (w/o Today HH:MM):";
            this.lblTotalBalance.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // middaySpan
            // 
            this.middaySpan.Enabled = true;
            this.middaySpan.Interval = 10000;
            // 
            // cmbStatisticRange
            // 
            this.cmbStatisticRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbStatisticRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatisticRange.FormattingEnabled = true;
            this.cmbStatisticRange.Location = new System.Drawing.Point(118, 259);
            this.cmbStatisticRange.Name = "cmbStatisticRange";
            this.cmbStatisticRange.Size = new System.Drawing.Size(88, 21);
            this.cmbStatisticRange.TabIndex = 35;
            // 
            // cmbStatisticType
            // 
            this.cmbStatisticType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbStatisticType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatisticType.FormattingEnabled = true;
            this.cmbStatisticType.Location = new System.Drawing.Point(6, 259);
            this.cmbStatisticType.Name = "cmbStatisticType";
            this.cmbStatisticType.Size = new System.Drawing.Size(106, 21);
            this.cmbStatisticType.TabIndex = 34;
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Location = new System.Drawing.Point(224, 25);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.SeaGreen;
            series1.ChartArea = "ChartArea1";
            series1.CustomProperties = "EmptyPointValue=Zero";
            series1.IsVisibleInLegend = false;
            series1.IsXValueIndexed = true;
            series1.Name = "S";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(535, 231);
            this.chart1.TabIndex = 33;
            this.chart1.Text = "chart1";
            // 
            // btnDeleteStamp
            // 
            this.btnDeleteStamp.Location = new System.Drawing.Point(110, 27);
            this.btnDeleteStamp.Name = "btnDeleteStamp";
            this.btnDeleteStamp.Size = new System.Drawing.Size(95, 23);
            this.btnDeleteStamp.TabIndex = 30;
            this.btnDeleteStamp.Text = "Delete Stamp";
            this.btnDeleteStamp.UseVisualStyleBackColor = true;
            // 
            // btnTakeDayOff
            // 
            this.btnTakeDayOff.Location = new System.Drawing.Point(9, 27);
            this.btnTakeDayOff.Name = "btnTakeDayOff";
            this.btnTakeDayOff.Size = new System.Drawing.Size(95, 23);
            this.btnTakeDayOff.TabIndex = 31;
            this.btnTakeDayOff.Text = "Take Day Off";
            this.btnTakeDayOff.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.flpChartFilter);
            this.groupBox2.Controls.Add(this.StampCalendar);
            this.groupBox2.Controls.Add(this.cmbStatisticType);
            this.groupBox2.Controls.Add(this.chart1);
            this.groupBox2.Controls.Add(this.cmbStatisticRange);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 110);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(765, 291);
            this.groupBox2.TabIndex = 37;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "History";
            // 
            // flpChartFilter
            // 
            this.flpChartFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpChartFilter.Location = new System.Drawing.Point(224, 259);
            this.flpChartFilter.Name = "flpChartFilter";
            this.flpChartFilter.Size = new System.Drawing.Size(535, 21);
            this.flpChartFilter.TabIndex = 37;
            // 
            // StampCalendar
            // 
            this.StampCalendar.Location = new System.Drawing.Point(6, 25);
            this.StampCalendar.MaxSelectionCount = 1;
            this.StampCalendar.Name = "StampCalendar";
            this.StampCalendar.ShowWeekNumbers = true;
            this.StampCalendar.TabIndex = 29;
            this.StampCalendar.TodayDate = new System.DateTime(2011, 11, 27, 0, 0, 0, 0);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnSettings);
            this.groupBox3.Controls.Add(this.btnExportExcelActivities);
            this.groupBox3.Controls.Add(this.btnTakeDayOff);
            this.groupBox3.Controls.Add(this.btnDeleteStamp);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox3.Location = new System.Drawing.Point(0, 401);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(765, 62);
            this.groupBox3.TabIndex = 38;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Settings and Actions";
            // 
            // btnSettings
            // 
            this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSettings.Location = new System.Drawing.Point(649, 27);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(110, 23);
            this.btnSettings.TabIndex = 35;
            this.btnSettings.Text = "Settings...";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnExportExcelActivities
            // 
            this.btnExportExcelActivities.Location = new System.Drawing.Point(211, 27);
            this.btnExportExcelActivities.Name = "btnExportExcelActivities";
            this.btnExportExcelActivities.Size = new System.Drawing.Size(130, 23);
            this.btnExportExcelActivities.TabIndex = 33;
            this.btnExportExcelActivities.Text = "Activities to Excel     | ▼";
            this.btnExportExcelActivities.UseVisualStyleBackColor = true;
            this.btnExportExcelActivities.Click += new System.EventHandler(this.btnExportExcelActivities_Click);
            // 
            // timelineToday
            // 
            this.timelineToday.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timelineToday.Location = new System.Drawing.Point(6, 3);
            this.timelineToday.Manager = null;
            this.timelineToday.Name = "timelineToday";
            this.timelineToday.Owner = null;
            this.timelineToday.Size = new System.Drawing.Size(753, 101);
            this.timelineToday.Stamp = null;
            this.timelineToday.TabIndex = 37;
            // 
            // pnlToday
            // 
            this.pnlToday.Controls.Add(this.timelineToday);
            this.pnlToday.Controls.Add(this.btnAddTimestamp);
            this.pnlToday.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToday.Location = new System.Drawing.Point(0, 0);
            this.pnlToday.Name = "pnlToday";
            this.pnlToday.Size = new System.Drawing.Size(765, 110);
            this.pnlToday.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(765, 519);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.pnlToday);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(469, 480);
            this.Name = "Form1";
            this.Text = "TimeStamp";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.pnlToday.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblTotalBalance;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer middaySpan;
        public System.Windows.Forms.ComboBox cmbStatisticRange;
        public System.Windows.Forms.ComboBox cmbStatisticType;
        public System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        public System.Windows.Forms.Button btnDeleteStamp;
        public System.Windows.Forms.Button btnTakeDayOff;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnAddTimestamp;
        public System.Windows.Forms.Button btnExportExcelActivities;
        private System.Windows.Forms.Button btnSettings;
        public System.Windows.Forms.MonthCalendar StampCalendar;
        private StampTimelineControl timelineToday;
        private System.Windows.Forms.FlowLayoutPanel flpChartFilter;
        private System.Windows.Forms.Panel pnlToday;
    }
}

