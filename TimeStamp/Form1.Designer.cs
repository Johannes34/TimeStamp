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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.txtStart = new System.Windows.Forms.TextBox();
            this.txtEnd = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPause = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblToday = new System.Windows.Forms.GroupBox();
            this.lblActivityWarning = new System.Windows.Forms.Label();
            this.cbActivityDetails = new System.Windows.Forms.CheckBox();
            this.txtCurrentShownTotal = new System.Windows.Forms.TextBox();
            this.grdActivities = new System.Windows.Forms.DataGridView();
            this.StartActivity = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Activity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Hours = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Comment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label5 = new System.Windows.Forms.Label();
            this.txtWorkingHours = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.btnAddTimestamp = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblTotalBalance = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.middaySpan = new System.Windows.Forms.Timer(this.components);
            this.lblStatisticValues = new System.Windows.Forms.Label();
            this.cmbStatisticRange = new System.Windows.Forms.ComboBox();
            this.cmbStatisticType = new System.Windows.Forms.ComboBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.StampCalendar = new System.Windows.Forms.MonthCalendar();
            this.btnDeleteStamp = new System.Windows.Forms.Button();
            this.btnTakeDayOff = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnExportExcelActivities = new System.Windows.Forms.Button();
            this.lblToday.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdActivities)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "TimeStamp";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_Click_1);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Today\'s Start:";
            // 
            // txtStart
            // 
            this.txtStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtStart.Location = new System.Drawing.Point(117, 23);
            this.txtStart.MaxLength = 5;
            this.txtStart.Name = "txtStart";
            this.txtStart.Size = new System.Drawing.Size(57, 20);
            this.txtStart.TabIndex = 1;
            this.toolTip1.SetToolTip(this.txtStart, "HH:MM");
            this.txtStart.TextChanged += new System.EventHandler(this.txtStart_TextChanged);
            // 
            // txtEnd
            // 
            this.txtEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtEnd.Location = new System.Drawing.Point(117, 46);
            this.txtEnd.MaxLength = 5;
            this.txtEnd.Name = "txtEnd";
            this.txtEnd.Size = new System.Drawing.Size(57, 20);
            this.txtEnd.TabIndex = 3;
            this.toolTip1.SetToolTip(this.txtEnd, "HH:MM");
            this.txtEnd.TextChanged += new System.EventHandler(this.txtEnd_TextChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Today\'s End:";
            // 
            // txtPause
            // 
            this.txtPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPause.Location = new System.Drawing.Point(117, 69);
            this.txtPause.MaxLength = 3;
            this.txtPause.Name = "txtPause";
            this.txtPause.Size = new System.Drawing.Size(57, 20);
            this.txtPause.TabIndex = 5;
            this.toolTip1.SetToolTip(this.txtPause, "In Minutes");
            this.txtPause.TextChanged += new System.EventHandler(this.txtPause_TextChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Today\'s Pause [min]:";
            // 
            // lblTotal
            // 
            this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(6, 145);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(82, 13);
            this.lblTotal.TabIndex = 7;
            this.lblTotal.Text = "Today Balance:";
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblToday
            // 
            this.lblToday.Controls.Add(this.lblActivityWarning);
            this.lblToday.Controls.Add(this.cbActivityDetails);
            this.lblToday.Controls.Add(this.txtCurrentShownTotal);
            this.lblToday.Controls.Add(this.grdActivities);
            this.lblToday.Controls.Add(this.label5);
            this.lblToday.Controls.Add(this.txtWorkingHours);
            this.lblToday.Controls.Add(this.label4);
            this.lblToday.Controls.Add(this.txtComment);
            this.lblToday.Controls.Add(this.label1);
            this.lblToday.Controls.Add(this.lblTotal);
            this.lblToday.Controls.Add(this.txtStart);
            this.lblToday.Controls.Add(this.label2);
            this.lblToday.Controls.Add(this.txtPause);
            this.lblToday.Controls.Add(this.txtEnd);
            this.lblToday.Controls.Add(this.label3);
            this.lblToday.Controls.Add(this.btnAddTimestamp);
            this.lblToday.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblToday.Location = new System.Drawing.Point(0, 0);
            this.lblToday.Name = "lblToday";
            this.lblToday.Size = new System.Drawing.Size(549, 169);
            this.lblToday.TabIndex = 8;
            this.lblToday.TabStop = false;
            this.lblToday.Text = "Today:";
            // 
            // lblActivityWarning
            // 
            this.lblActivityWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblActivityWarning.BackColor = System.Drawing.Color.Red;
            this.lblActivityWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActivityWarning.Location = new System.Drawing.Point(512, 76);
            this.lblActivityWarning.Name = "lblActivityWarning";
            this.lblActivityWarning.Size = new System.Drawing.Size(25, 33);
            this.lblActivityWarning.TabIndex = 42;
            this.lblActivityWarning.Text = "❗";
            this.lblActivityWarning.Visible = false;
            // 
            // cbActivityDetails
            // 
            this.cbActivityDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbActivityDetails.AutoSize = true;
            this.cbActivityDetails.Location = new System.Drawing.Point(418, 144);
            this.cbActivityDetails.Name = "cbActivityDetails";
            this.cbActivityDetails.Size = new System.Drawing.Size(125, 17);
            this.cbActivityDetails.TabIndex = 41;
            this.cbActivityDetails.Text = "Show Activity Details";
            this.toolTip1.SetToolTip(this.cbActivityDetails, "Toggles summary and details+edit mode for activity grid");
            this.cbActivityDetails.UseVisualStyleBackColor = true;
            this.cbActivityDetails.CheckedChanged += new System.EventHandler(this.cbActivityDetails_CheckedChanged);
            // 
            // txtCurrentShownTotal
            // 
            this.txtCurrentShownTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtCurrentShownTotal.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCurrentShownTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCurrentShownTotal.Location = new System.Drawing.Point(117, 145);
            this.txtCurrentShownTotal.MaxLength = 2;
            this.txtCurrentShownTotal.Name = "txtCurrentShownTotal";
            this.txtCurrentShownTotal.ReadOnly = true;
            this.txtCurrentShownTotal.Size = new System.Drawing.Size(57, 13);
            this.txtCurrentShownTotal.TabIndex = 40;
            this.toolTip1.SetToolTip(this.txtCurrentShownTotal, "In full hours, e.g. 4");
            // 
            // grdActivities
            // 
            this.grdActivities.AllowUserToAddRows = false;
            this.grdActivities.AllowUserToDeleteRows = false;
            this.grdActivities.AllowUserToOrderColumns = true;
            this.grdActivities.AllowUserToResizeRows = false;
            this.grdActivities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grdActivities.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.grdActivities.BackgroundColor = System.Drawing.SystemColors.Control;
            this.grdActivities.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.grdActivities.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdActivities.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.StartActivity,
            this.Activity,
            this.Hours,
            this.Comment});
            this.grdActivities.Location = new System.Drawing.Point(180, 23);
            this.grdActivities.Name = "grdActivities";
            this.grdActivities.RowHeadersVisible = false;
            this.grdActivities.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.grdActivities.Size = new System.Drawing.Size(363, 90);
            this.grdActivities.TabIndex = 37;
            this.grdActivities.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdActivities_CellContentClick);
            this.grdActivities.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdActivities_CellValueChanged);
            // 
            // StartActivity
            // 
            this.StartActivity.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.StartActivity.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StartActivity.HeaderText = "Start";
            this.StartActivity.Name = "StartActivity";
            this.StartActivity.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.StartActivity.Text = "▶";
            this.StartActivity.ToolTipText = "Start activity";
            this.StartActivity.UseColumnTextForButtonValue = true;
            this.StartActivity.Width = 35;
            // 
            // Activity
            // 
            this.Activity.HeaderText = "Activity";
            this.Activity.Name = "Activity";
            this.Activity.ReadOnly = true;
            this.Activity.Width = 66;
            // 
            // Hours
            // 
            this.Hours.HeaderText = "Hours";
            this.Hours.Name = "Hours";
            this.Hours.Width = 60;
            // 
            // Comment
            // 
            this.Comment.HeaderText = "Comment";
            this.Comment.Name = "Comment";
            this.Comment.Width = 76;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 96);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Working Hours:";
            // 
            // txtWorkingHours
            // 
            this.txtWorkingHours.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtWorkingHours.Location = new System.Drawing.Point(117, 93);
            this.txtWorkingHours.MaxLength = 2;
            this.txtWorkingHours.Name = "txtWorkingHours";
            this.txtWorkingHours.Size = new System.Drawing.Size(57, 20);
            this.txtWorkingHours.TabIndex = 10;
            this.toolTip1.SetToolTip(this.txtWorkingHours, "In full hours, e.g. 4");
            this.txtWorkingHours.TextChanged += new System.EventHandler(this.txtWorkingHours_TextChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 122);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Comment:";
            // 
            // txtComment
            // 
            this.txtComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtComment.Location = new System.Drawing.Point(117, 119);
            this.txtComment.Name = "txtComment";
            this.txtComment.Size = new System.Drawing.Size(426, 20);
            this.txtComment.TabIndex = 8;
            this.toolTip1.SetToolTip(this.txtComment, "Text");
            this.txtComment.TextChanged += new System.EventHandler(this.txtComment_TextChanged);
            // 
            // btnAddTimestamp
            // 
            this.btnAddTimestamp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddTimestamp.Location = new System.Drawing.Point(3, 16);
            this.btnAddTimestamp.Name = "btnAddTimestamp";
            this.btnAddTimestamp.Size = new System.Drawing.Size(543, 150);
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
            this.groupBox1.Location = new System.Drawing.Point(0, 480);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(549, 56);
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
            // lblStatisticValues
            // 
            this.lblStatisticValues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStatisticValues.Location = new System.Drawing.Point(12, 190);
            this.lblStatisticValues.Name = "lblStatisticValues";
            this.lblStatisticValues.Size = new System.Drawing.Size(200, 48);
            this.lblStatisticValues.TabIndex = 36;
            this.lblStatisticValues.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbStatisticRange
            // 
            this.cmbStatisticRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbStatisticRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatisticRange.FormattingEnabled = true;
            this.cmbStatisticRange.Location = new System.Drawing.Point(336, 217);
            this.cmbStatisticRange.Name = "cmbStatisticRange";
            this.cmbStatisticRange.Size = new System.Drawing.Size(111, 21);
            this.cmbStatisticRange.TabIndex = 35;
            // 
            // cmbStatisticType
            // 
            this.cmbStatisticType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbStatisticType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatisticType.FormattingEnabled = true;
            this.cmbStatisticType.Location = new System.Drawing.Point(224, 217);
            this.cmbStatisticType.Name = "cmbStatisticType";
            this.cmbStatisticType.Size = new System.Drawing.Size(106, 21);
            this.cmbStatisticType.TabIndex = 34;
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea2.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea2);
            this.chart1.Location = new System.Drawing.Point(224, 25);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.SeaGreen;
            series2.ChartArea = "ChartArea1";
            series2.CustomProperties = "EmptyPointValue=Zero";
            series2.IsVisibleInLegend = false;
            series2.IsXValueIndexed = true;
            series2.Name = "S";
            this.chart1.Series.Add(series2);
            this.chart1.Size = new System.Drawing.Size(319, 186);
            this.chart1.TabIndex = 33;
            this.chart1.Text = "chart1";
            // 
            // StampCalendar
            // 
            this.StampCalendar.Location = new System.Drawing.Point(12, 25);
            this.StampCalendar.MaxSelectionCount = 1;
            this.StampCalendar.Name = "StampCalendar";
            this.StampCalendar.ShowWeekNumbers = true;
            this.StampCalendar.TabIndex = 29;
            this.StampCalendar.TodayDate = new System.DateTime(2011, 11, 27, 0, 0, 0, 0);
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
            this.groupBox2.Controls.Add(this.StampCalendar);
            this.groupBox2.Controls.Add(this.lblStatisticValues);
            this.groupBox2.Controls.Add(this.cmbStatisticType);
            this.groupBox2.Controls.Add(this.chart1);
            this.groupBox2.Controls.Add(this.cmbStatisticRange);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox2.Location = new System.Drawing.Point(0, 169);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(549, 249);
            this.groupBox2.TabIndex = 37;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "History";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnSettings);
            this.groupBox3.Controls.Add(this.btnExportExcelActivities);
            this.groupBox3.Controls.Add(this.btnTakeDayOff);
            this.groupBox3.Controls.Add(this.btnDeleteStamp);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox3.Location = new System.Drawing.Point(0, 418);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(549, 62);
            this.groupBox3.TabIndex = 38;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Settings and Actions";
            // 
            // btnSettings
            // 
            this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSettings.Location = new System.Drawing.Point(433, 27);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 536);
            this.Controls.Add(this.lblToday);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(469, 575);
            this.Name = "Form1";
            this.Text = "TimeStamp";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.lblToday.ResumeLayout(false);
            this.lblToday.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdActivities)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtStart;
        private System.Windows.Forms.TextBox txtEnd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPause;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.GroupBox lblToday;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblTotalBalance;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer middaySpan;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtWorkingHours;
        public System.Windows.Forms.Label lblStatisticValues;
        public System.Windows.Forms.ComboBox cmbStatisticRange;
        public System.Windows.Forms.ComboBox cmbStatisticType;
        public System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        public System.Windows.Forms.MonthCalendar StampCalendar;
        public System.Windows.Forms.Button btnDeleteStamp;
        public System.Windows.Forms.Button btnTakeDayOff;
        private System.Windows.Forms.DataGridView grdActivities;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnAddTimestamp;
        private System.Windows.Forms.DataGridViewButtonColumn StartActivity;
        private System.Windows.Forms.DataGridViewTextBoxColumn Activity;
        private System.Windows.Forms.DataGridViewTextBoxColumn Hours;
        private System.Windows.Forms.DataGridViewTextBoxColumn Comment;
        private System.Windows.Forms.TextBox txtCurrentShownTotal;
        public System.Windows.Forms.Button btnExportExcelActivities;
        private System.Windows.Forms.CheckBox cbActivityDetails;
        private System.Windows.Forms.Label lblActivityWarning;
        private System.Windows.Forms.Button btnSettings;
    }
}

