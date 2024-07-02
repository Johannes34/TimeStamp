namespace TimeStamp
{
    partial class StampTimelineControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblToday = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtBegin = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtEnd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPause = new System.Windows.Forms.TextBox();
            this.btnStartActivity = new System.Windows.Forms.Button();
            this.timelineControl1 = new TimeStamp.TimelineControl();
            this.txtCurrentShownTotal = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtWorkingHours = new System.Windows.Forms.TextBox();
            this.lblTotal = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblToday.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblToday
            // 
            this.lblToday.Controls.Add(this.label3);
            this.lblToday.Controls.Add(this.txtBegin);
            this.lblToday.Controls.Add(this.label2);
            this.lblToday.Controls.Add(this.txtEnd);
            this.lblToday.Controls.Add(this.label1);
            this.lblToday.Controls.Add(this.txtPause);
            this.lblToday.Controls.Add(this.btnStartActivity);
            this.lblToday.Controls.Add(this.timelineControl1);
            this.lblToday.Controls.Add(this.txtCurrentShownTotal);
            this.lblToday.Controls.Add(this.label5);
            this.lblToday.Controls.Add(this.txtWorkingHours);
            this.lblToday.Controls.Add(this.lblTotal);
            this.lblToday.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblToday.Location = new System.Drawing.Point(0, 0);
            this.lblToday.Name = "lblToday";
            this.lblToday.Size = new System.Drawing.Size(693, 87);
            this.lblToday.TabIndex = 9;
            this.lblToday.TabStop = false;
            this.lblToday.Text = "Today:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(136, 65);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 50;
            this.label3.Text = "Begin:";
            // 
            // txtBegin
            // 
            this.txtBegin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtBegin.BackColor = System.Drawing.SystemColors.Menu;
            this.txtBegin.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtBegin.Location = new System.Drawing.Point(173, 65);
            this.txtBegin.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.txtBegin.Name = "txtBegin";
            this.txtBegin.ReadOnly = true;
            this.txtBegin.Size = new System.Drawing.Size(46, 13);
            this.txtBegin.TabIndex = 49;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(329, 65);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 48;
            this.label2.Text = "End:";
            // 
            // txtEnd
            // 
            this.txtEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtEnd.BackColor = System.Drawing.SystemColors.Menu;
            this.txtEnd.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtEnd.Location = new System.Drawing.Point(358, 65);
            this.txtEnd.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.txtEnd.Name = "txtEnd";
            this.txtEnd.ReadOnly = true;
            this.txtEnd.Size = new System.Drawing.Size(46, 13);
            this.txtEnd.TabIndex = 47;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(231, 65);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 46;
            this.label1.Text = "Pause:";
            // 
            // txtPause
            // 
            this.txtPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPause.BackColor = System.Drawing.SystemColors.Menu;
            this.txtPause.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPause.Location = new System.Drawing.Point(271, 65);
            this.txtPause.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.txtPause.Name = "txtPause";
            this.txtPause.ReadOnly = true;
            this.txtPause.Size = new System.Drawing.Size(46, 13);
            this.txtPause.TabIndex = 45;
            // 
            // btnStartActivity
            // 
            this.btnStartActivity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartActivity.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartActivity.Location = new System.Drawing.Point(662, 19);
            this.btnStartActivity.Name = "btnStartActivity";
            this.btnStartActivity.Size = new System.Drawing.Size(28, 28);
            this.btnStartActivity.TabIndex = 44;
            this.btnStartActivity.Text = "▶";
            this.toolTip1.SetToolTip(this.btnStartActivity, "Start a new activity");
            this.btnStartActivity.UseVisualStyleBackColor = true;
            this.btnStartActivity.Click += new System.EventHandler(this.btnStartActivity_Click);
            // 
            // timelineControl1
            // 
            this.timelineControl1.AddSectionMode = false;
            this.timelineControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timelineControl1.DrawDisplayTexts = true;
            this.timelineControl1.DrawSectionDisplayTexts = true;
            this.timelineControl1.EdgeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.timelineControl1.EmptyTimelineThickness = 1;
            this.timelineControl1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.timelineControl1.HighlightEdgeColor = System.Drawing.SystemColors.ActiveCaption;
            this.timelineControl1.HighlightForeColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.timelineControl1.HighlightInActiveEdgeColor = System.Drawing.SystemColors.InactiveCaption;
            this.timelineControl1.HighlightInActiveForeColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.timelineControl1.Location = new System.Drawing.Point(3, 16);
            this.timelineControl1.MajorTicklineSize = new System.Drawing.Size(6, 14);
            this.timelineControl1.MaximumValue = 0D;
            this.timelineControl1.MinimumValue = 0D;
            this.timelineControl1.MinorTicklineSize = new System.Drawing.Size(4, 10);
            this.timelineControl1.Name = "timelineControl1";
            this.timelineControl1.OnAddSection = null;
            this.timelineControl1.OnDragSection = null;
            this.timelineControl1.OnDragSeparator = null;
            this.timelineControl1.OnSectionClicked = null;
            this.timelineControl1.OnSeparatorClicked = null;
            this.timelineControl1.Size = new System.Drawing.Size(653, 39);
            this.timelineControl1.TabIndex = 43;
            this.timelineControl1.TimelineThickness = 8;
            // 
            // txtCurrentShownTotal
            // 
            this.txtCurrentShownTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCurrentShownTotal.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCurrentShownTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCurrentShownTotal.Location = new System.Drawing.Point(631, 64);
            this.txtCurrentShownTotal.MaxLength = 2;
            this.txtCurrentShownTotal.Name = "txtCurrentShownTotal";
            this.txtCurrentShownTotal.ReadOnly = true;
            this.txtCurrentShownTotal.Size = new System.Drawing.Size(57, 13);
            this.txtCurrentShownTotal.TabIndex = 40;
            this.txtCurrentShownTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 65);
            this.label5.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Working Hours:";
            // 
            // txtWorkingHours
            // 
            this.txtWorkingHours.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtWorkingHours.BackColor = System.Drawing.SystemColors.Menu;
            this.txtWorkingHours.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtWorkingHours.Location = new System.Drawing.Point(87, 65);
            this.txtWorkingHours.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.txtWorkingHours.MaxLength = 2;
            this.txtWorkingHours.Name = "txtWorkingHours";
            this.txtWorkingHours.Size = new System.Drawing.Size(37, 13);
            this.txtWorkingHours.TabIndex = 10;
            this.txtWorkingHours.TextChanged += new System.EventHandler(this.txtWorkingHours_TextChanged);
            // 
            // lblTotal
            // 
            this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(545, 64);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(82, 13);
            this.lblTotal.TabIndex = 7;
            this.lblTotal.Text = "Today Balance:";
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // StampTimelineControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblToday);
            this.Name = "StampTimelineControl";
            this.Size = new System.Drawing.Size(693, 87);
            this.lblToday.ResumeLayout(false);
            this.lblToday.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox lblToday;
        private System.Windows.Forms.Button btnStartActivity;
        private TimelineControl timelineControl1;
        private System.Windows.Forms.TextBox txtCurrentShownTotal;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtWorkingHours;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtBegin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPause;
    }
}
