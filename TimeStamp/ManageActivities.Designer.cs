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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageActivities));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.grdActivities = new System.Windows.Forms.DataGridView();
            this.ActivityName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MorningDefault = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.DeleteRow = new System.Windows.Forms.DataGridViewButtonColumn();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdActivities)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.grdActivities);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(433, 216);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Manage tracked activities";
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
            this.MorningDefault,
            this.DeleteRow});
            this.grdActivities.Location = new System.Drawing.Point(6, 19);
            this.grdActivities.MultiSelect = false;
            this.grdActivities.Name = "grdActivities";
            this.grdActivities.RowHeadersVisible = false;
            this.grdActivities.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.grdActivities.Size = new System.Drawing.Size(421, 191);
            this.grdActivities.TabIndex = 0;
            this.grdActivities.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdActivities_CellContentClick);
            this.grdActivities.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdActivities_CellValueChanged);
            // 
            // ActivityName
            // 
            this.ActivityName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ActivityName.HeaderText = "Name";
            this.ActivityName.Name = "ActivityName";
            this.ActivityName.Width = 60;
            // 
            // MorningDefault
            // 
            this.MorningDefault.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.MorningDefault.FalseValue = "false";
            this.MorningDefault.HeaderText = "Default for new day";
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
            // ManageActivities
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 240);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ManageActivities";
            this.Text = "TimeStamp";
            this.Load += new System.EventHandler(this.ManageActivities_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdActivities)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView grdActivities;
        private System.Windows.Forms.DataGridViewTextBoxColumn ActivityName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn MorningDefault;
        private System.Windows.Forms.DataGridViewButtonColumn DeleteRow;
    }
}