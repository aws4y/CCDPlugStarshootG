namespace SettingsSSG
{
    partial class SettingsDialogSSG
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.CheckedListBox clbGC;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialogSSG));
            this.boxGainControl = new System.Windows.Forms.GroupBox();
            this.grpBOptions = new System.Windows.Forms.GroupBox();
            this.cbDFC = new System.Windows.Forms.CheckBox();
            this.cbLowNoise = new System.Windows.Forms.CheckBox();
            this.cbSkip = new System.Windows.Forms.CheckBox();
            this.tbSpeed = new System.Windows.Forms.TrackBar();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.numSpeed = new System.Windows.Forms.NumericUpDown();
            this.lblBlackLevel = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.numBlackLevel = new System.Windows.Forms.NumericUpDown();
            clbGC = new System.Windows.Forms.CheckedListBox();
            this.boxGainControl.SuspendLayout();
            this.grpBOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlackLevel)).BeginInit();
            this.SuspendLayout();
            // 
            // clbGC
            // 
            clbGC.BackColor = System.Drawing.SystemColors.Control;
            clbGC.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(clbGC, "clbGC");
            clbGC.FormattingEnabled = true;
            clbGC.Items.AddRange(new object[] {
            resources.GetString("clbGC.Items"),
            resources.GetString("clbGC.Items1")});
            clbGC.Name = "clbGC";
            clbGC.SelectedIndexChanged += new System.EventHandler(this.clbGC_SelectedIndexChanged);
            // 
            // boxGainControl
            // 
            this.boxGainControl.Controls.Add(clbGC);
            resources.ApplyResources(this.boxGainControl, "boxGainControl");
            this.boxGainControl.Name = "boxGainControl";
            this.boxGainControl.TabStop = false;
            // 
            // grpBOptions
            // 
            this.grpBOptions.Controls.Add(this.cbSkip);
            this.grpBOptions.Controls.Add(this.cbDFC);
            this.grpBOptions.Controls.Add(this.cbLowNoise);
            resources.ApplyResources(this.grpBOptions, "grpBOptions");
            this.grpBOptions.Name = "grpBOptions";
            this.grpBOptions.TabStop = false;
            this.grpBOptions.Enter += new System.EventHandler(this.grpBOptions_Enter);
            // 
            // cbDFC
            // 
            resources.ApplyResources(this.cbDFC, "cbDFC");
            this.cbDFC.Name = "cbDFC";
            this.cbDFC.UseVisualStyleBackColor = true;
            // 
            // cbLowNoise
            // 
            resources.ApplyResources(this.cbLowNoise, "cbLowNoise");
            this.cbLowNoise.Name = "cbLowNoise";
            this.cbLowNoise.UseVisualStyleBackColor = true;
            // 
            // cbSkip
            // 
            resources.ApplyResources(this.cbSkip, "cbSkip");
            this.cbSkip.Name = "cbSkip";
            this.cbSkip.UseVisualStyleBackColor = true;
            // 
            // tbSpeed
            // 
            resources.ApplyResources(this.tbSpeed, "tbSpeed");
            this.tbSpeed.Maximum = 9;
            this.tbSpeed.Name = "tbSpeed";
            // 
            // lblSpeed
            // 
            resources.ApplyResources(this.lblSpeed, "lblSpeed");
            this.lblSpeed.Name = "lblSpeed";
            // 
            // numSpeed
            // 
            resources.ApplyResources(this.numSpeed, "numSpeed");
            this.numSpeed.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numSpeed.Name = "numSpeed";
            // 
            // lblBlackLevel
            // 
            resources.ApplyResources(this.lblBlackLevel, "lblBlackLevel");
            this.lblBlackLevel.Name = "lblBlackLevel";
            // 
            // trackBar1
            // 
            resources.ApplyResources(this.trackBar1, "trackBar1");
            this.trackBar1.Maximum = 1984;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // numBlackLevel
            // 
            resources.ApplyResources(this.numBlackLevel, "numBlackLevel");
            this.numBlackLevel.Maximum = new decimal(new int[] {
            1984,
            0,
            0,
            0});
            this.numBlackLevel.Name = "numBlackLevel";
            // 
            // SettingsDialogSSG
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numBlackLevel);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.lblBlackLevel);
            this.Controls.Add(this.numSpeed);
            this.Controls.Add(this.lblSpeed);
            this.Controls.Add(this.tbSpeed);
            this.Controls.Add(this.grpBOptions);
            this.Controls.Add(this.boxGainControl);
            this.Name = "SettingsDialogSSG";
            this.boxGainControl.ResumeLayout(false);
            this.grpBOptions.ResumeLayout(false);
            this.grpBOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlackLevel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GroupBox boxGainControl;
        private GroupBox grpBOptions;
        private CheckBox cbLowNoise;
        private CheckBox cbDFC;
        private CheckBox cbSkip;
        private TrackBar tbSpeed;
        private Label lblSpeed;
        private NumericUpDown numSpeed;
        private Label lblBlackLevel;
        private TrackBar trackBar1;
        private NumericUpDown numBlackLevel;
    }
}