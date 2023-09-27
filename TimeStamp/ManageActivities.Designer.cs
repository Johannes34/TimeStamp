namespace TimeStamp
{
    partial class ManageActivities
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageActivities));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.grdActivities = new System.Windows.Forms.DataGridView();
            this.ActivityName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Comment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MorningDefault = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.DeleteRow = new System.Windows.Forms.DataGridViewButtonColumn();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.grdTags = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewButtonColumn1 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.grdAutoTrackApplications = new System.Windows.Forms.DataGridView();
            this.chkEnableAutoTrackApplications = new System.Windows.Forms.CheckBox();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewButtonColumn2 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.chkEnableAutoTagging = new System.Windows.Forms.CheckBox();
            this.grdAutoTag = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewButtonColumn3 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdActivities)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdTags)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdAutoTrackApplications)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdAutoTag)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnMoveDown);
            this.groupBox1.Controls.Add(this.btnMoveUp);
            this.groupBox1.Controls.Add(this.grdActivities);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(527, 188);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Manage tracked activities";
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveDown.Enabled = false;
            this.btnMoveDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveDown.Location = new System.Drawing.Point(487, 59);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(34, 34);
            this.btnMoveDown.TabIndex = 2;
            this.btnMoveDown.Text = "↓";
            this.toolTip1.SetToolTip(this.btnMoveDown, "Move selected activity down. The order of activities is preserved in the activity" +
        " list in the main view and in the context menu.");
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveUp.Enabled = false;
            this.btnMoveUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveUp.Location = new System.Drawing.Point(487, 19);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(34, 34);
            this.btnMoveUp.TabIndex = 1;
            this.btnMoveUp.Text = "↑";
            this.toolTip1.SetToolTip(this.btnMoveUp, "Move selected activity up. The order of activities is preserved in the activity l" +
        "ist in the main view and in the context menu.");
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // grdActivities
            // 
            this.grdActivities.AllowUserToDeleteRows = false;
            this.grdActivities.AllowUserToResizeRows = false;
            this.grdActivities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grdActivities.BackgroundColor = System.Drawing.SystemColors.Control;
            this.grdActivities.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdActivities.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ActivityName,
            this.Comment,
            this.MorningDefault,
            this.DeleteRow});
            this.grdActivities.Location = new System.Drawing.Point(6, 19);
            this.grdActivities.MultiSelect = false;
            this.grdActivities.Name = "grdActivities";
            this.grdActivities.RowHeadersVisible = false;
            this.grdActivities.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.grdActivities.Size = new System.Drawing.Size(475, 163);
            this.grdActivities.TabIndex = 0;
            this.grdActivities.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdActivities_CellContentClick);
            this.grdActivities.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdActivities_CellValueChanged);
            this.grdActivities.CurrentCellChanged += new System.EventHandler(this.grdActivities_CurrentCellChanged);
            // 
            // ActivityName
            // 
            this.ActivityName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ActivityName.HeaderText = "Name";
            this.ActivityName.Name = "ActivityName";
            this.ActivityName.Width = 60;
            // 
            // Comment
            // 
            this.Comment.HeaderText = "Comment";
            this.Comment.Name = "Comment";
            // 
            // MorningDefault
            // 
            this.MorningDefault.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.MorningDefault.FalseValue = "false";
            this.MorningDefault.HeaderText = "Default for new day *";
            this.MorningDefault.Name = "MorningDefault";
            this.MorningDefault.ToolTipText = "Determines whether a new day is always started with this activity. If none is set" +
    ", a new day starts with the most recent recorded activity.";
            this.MorningDefault.TrueValue = "true";
            this.MorningDefault.Width = 79;
            // 
            // DeleteRow
            // 
            this.DeleteRow.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.DeleteRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteRow.HeaderText = "Delete Activity";
            this.DeleteRow.Name = "DeleteRow";
            this.DeleteRow.Text = "✕";
            this.DeleteRow.UseColumnTextForButtonValue = true;
            this.DeleteRow.Width = 73;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.grdTags);
            this.groupBox2.Location = new System.Drawing.Point(3, 197);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(527, 188);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Manage activity tags";
            // 
            // grdTags
            // 
            this.grdTags.AllowUserToDeleteRows = false;
            this.grdTags.AllowUserToResizeRows = false;
            this.grdTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grdTags.BackgroundColor = System.Drawing.SystemColors.Control;
            this.grdTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewButtonColumn1});
            this.grdTags.Location = new System.Drawing.Point(6, 19);
            this.grdTags.MultiSelect = false;
            this.grdTags.Name = "grdTags";
            this.grdTags.RowHeadersVisible = false;
            this.grdTags.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.grdTags.Size = new System.Drawing.Size(515, 163);
            this.grdTags.TabIndex = 0;
            this.grdTags.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdTags_CellContentClick);
            this.grdTags.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdTags_CellValueChanged);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn1.HeaderText = "Category";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ToolTipText = "This is the category grouping of the tag, e.g. \'Customer\', \'Product\' or \'Location" +
    "\'";
            this.dataGridViewTextBoxColumn1.Width = 74;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.HeaderText = "Tag Name";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ToolTipText = "This is the tag value, e.g. \'VW\', \'BMW\', \'Toyota\' or \'Excel\', \'Word\', \'Powerpoint" +
    "\' or \'Munich\', \'Hamburg\', \'Berlin\'";
            // 
            // dataGridViewButtonColumn1
            // 
            this.dataGridViewButtonColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewButtonColumn1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dataGridViewButtonColumn1.HeaderText = "Delete Tag";
            this.dataGridViewButtonColumn1.Name = "dataGridViewButtonColumn1";
            this.dataGridViewButtonColumn1.Text = "✕";
            this.dataGridViewButtonColumn1.UseColumnTextForButtonValue = true;
            this.dataGridViewButtonColumn1.Width = 66;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(533, 555);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.grdAutoTag);
            this.groupBox3.Controls.Add(this.chkEnableAutoTagging);
            this.groupBox3.Controls.Add(this.chkEnableAutoTrackApplications);
            this.groupBox3.Controls.Add(this.grdAutoTrackApplications);
            this.groupBox3.Location = new System.Drawing.Point(3, 391);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(527, 161);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Auto-track activity applications used";
            // 
            // grdAutoTrackApplications
            // 
            this.grdAutoTrackApplications.AllowUserToDeleteRows = false;
            this.grdAutoTrackApplications.AllowUserToResizeRows = false;
            this.grdAutoTrackApplications.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grdAutoTrackApplications.BackgroundColor = System.Drawing.SystemColors.Control;
            this.grdAutoTrackApplications.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdAutoTrackApplications.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewButtonColumn2});
            this.grdAutoTrackApplications.Location = new System.Drawing.Point(6, 38);
            this.grdAutoTrackApplications.MultiSelect = false;
            this.grdAutoTrackApplications.Name = "grdAutoTrackApplications";
            this.grdAutoTrackApplications.RowHeadersVisible = false;
            this.grdAutoTrackApplications.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.grdAutoTrackApplications.Size = new System.Drawing.Size(297, 117);
            this.grdAutoTrackApplications.TabIndex = 0;
            this.grdAutoTrackApplications.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdAutoTrackApplications_CellContentClick);
            this.grdAutoTrackApplications.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdAutoTrackApplications_CellValueChanged);
            // 
            // chkEnableAutoTrackApplications
            // 
            this.chkEnableAutoTrackApplications.AutoSize = true;
            this.chkEnableAutoTrackApplications.Location = new System.Drawing.Point(6, 19);
            this.chkEnableAutoTrackApplications.Name = "chkEnableAutoTrackApplications";
            this.chkEnableAutoTrackApplications.Size = new System.Drawing.Size(124, 17);
            this.chkEnableAutoTrackApplications.TabIndex = 1;
            this.chkEnableAutoTrackApplications.Text = "Enable auto-tracking";
            this.chkEnableAutoTrackApplications.UseVisualStyleBackColor = true;
            this.chkEnableAutoTrackApplications.CheckedChanged += new System.EventHandler(this.chkEnableAutoTrackApplications_CheckedChanged);
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn3.HeaderText = "Window Title";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ToolTipText = "This is the partial text of the window title of the application, e.g. \"JIRA - Goo" +
    "gle Chrome\"";
            this.dataGridViewTextBoxColumn3.Width = 94;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn4.HeaderText = "Application Name";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ToolTipText = "This is the application name, e.g. \"JIRA\"";
            // 
            // dataGridViewButtonColumn2
            // 
            this.dataGridViewButtonColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewButtonColumn2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dataGridViewButtonColumn2.HeaderText = "Delete App";
            this.dataGridViewButtonColumn2.Name = "dataGridViewButtonColumn2";
            this.dataGridViewButtonColumn2.Text = "✕";
            this.dataGridViewButtonColumn2.UseColumnTextForButtonValue = true;
            this.dataGridViewButtonColumn2.Width = 66;
            // 
            // chkEnableAutoTagging
            // 
            this.chkEnableAutoTagging.AutoSize = true;
            this.chkEnableAutoTagging.Location = new System.Drawing.Point(309, 19);
            this.chkEnableAutoTagging.Name = "chkEnableAutoTagging";
            this.chkEnableAutoTagging.Size = new System.Drawing.Size(121, 17);
            this.chkEnableAutoTagging.TabIndex = 2;
            this.chkEnableAutoTagging.Text = "Enable auto-tagging";
            this.chkEnableAutoTagging.UseVisualStyleBackColor = true;
            this.chkEnableAutoTagging.CheckedChanged += new System.EventHandler(this.chkEnableAutoTagging_CheckedChanged);
            // 
            // grdAutoTag
            // 
            this.grdAutoTag.AllowUserToDeleteRows = false;
            this.grdAutoTag.AllowUserToResizeRows = false;
            this.grdAutoTag.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grdAutoTag.BackgroundColor = System.Drawing.SystemColors.Control;
            this.grdAutoTag.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdAutoTag.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewButtonColumn3});
            this.grdAutoTag.Location = new System.Drawing.Point(309, 38);
            this.grdAutoTag.MultiSelect = false;
            this.grdAutoTag.Name = "grdAutoTag";
            this.grdAutoTag.RowHeadersVisible = false;
            this.grdAutoTag.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.grdAutoTag.Size = new System.Drawing.Size(212, 117);
            this.grdAutoTag.TabIndex = 3;
            this.grdAutoTag.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdAutoTag_CellContentClick);
            this.grdAutoTag.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdAutoTag_CellValueChanged);
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn5.HeaderText = "Regex";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ToolTipText = "This is the regex value applied on the window title";
            this.dataGridViewTextBoxColumn5.Width = 63;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn6.HeaderText = "Category:Tag";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.ToolTipText = resources.GetString("dataGridViewTextBoxColumn6.ToolTipText");
            // 
            // dataGridViewButtonColumn3
            // 
            this.dataGridViewButtonColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewButtonColumn3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dataGridViewButtonColumn3.HeaderText = "Delete";
            this.dataGridViewButtonColumn3.Name = "dataGridViewButtonColumn3";
            this.dataGridViewButtonColumn3.Text = "✕";
            this.dataGridViewButtonColumn3.UseColumnTextForButtonValue = true;
            this.dataGridViewButtonColumn3.Width = 44;
            // 
            // ManageActivities
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 555);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ManageActivities";
            this.Text = "TimeStamp";
            this.Load += new System.EventHandler(this.ManageActivities_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdActivities)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdTags)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdAutoTrackApplications)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdAutoTag)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView grdActivities;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ActivityName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Comment;
        private System.Windows.Forms.DataGridViewCheckBoxColumn MorningDefault;
        private System.Windows.Forms.DataGridViewButtonColumn DeleteRow;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView grdTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewButtonColumn dataGridViewButtonColumn1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkEnableAutoTrackApplications;
        private System.Windows.Forms.DataGridView grdAutoTrackApplications;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewButtonColumn dataGridViewButtonColumn2;
        private System.Windows.Forms.DataGridView grdAutoTag;
        private System.Windows.Forms.CheckBox chkEnableAutoTagging;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewButtonColumn dataGridViewButtonColumn3;
    }
}