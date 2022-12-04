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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialogSSG));
            this.clbGC = new System.Windows.Forms.CheckedListBox();
            this.boxGainControl = new System.Windows.Forms.GroupBox();
            this.grpBOptions = new System.Windows.Forms.GroupBox();
            this.cbSkip = new System.Windows.Forms.CheckBox();
            this.cbDFC = new System.Windows.Forms.CheckBox();
            this.cbLowNoise = new System.Windows.Forms.CheckBox();
            this.tbSpeed = new System.Windows.Forms.TrackBar();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.numSpeed = new System.Windows.Forms.NumericUpDown();
            this.lblBlackLevel = new System.Windows.Forms.Label();
            this.tbBlackLevel = new System.Windows.Forms.TrackBar();
            this.numBlackLevel = new System.Windows.Forms.NumericUpDown();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.boxGainControl.SuspendLayout();
            this.grpBOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBlackLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlackLevel)).BeginInit();
            this.SuspendLayout();
            // 
            // clbGC
            // 
            this.clbGC.BackColor = System.Drawing.SystemColors.Control;
            this.clbGC.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.clbGC.FormattingEnabled = true;
            this.clbGC.Items.AddRange(new object[] {
            resources.GetString("clbGC.Items"),
            resources.GetString("clbGC.Items1")});
            resources.ApplyResources(this.clbGC, "clbGC");
            this.clbGC.Name = "clbGC";
            this.clbGC.SelectedIndexChanged += new System.EventHandler(this.clbGC_SelectedIndexChanged);
            // 
            // boxGainControl
            // 
            this.boxGainControl.Controls.Add(this.clbGC);
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
            // cbSkip
            // 
            resources.ApplyResources(this.cbSkip, "cbSkip");
            this.cbSkip.Name = "cbSkip";
            this.cbSkip.UseVisualStyleBackColor = true;
            this.cbSkip.CheckedChanged += new System.EventHandler(this.cbSkip_CheckedChanged);
            // 
            // cbDFC
            // 
            resources.ApplyResources(this.cbDFC, "cbDFC");
            this.cbDFC.Name = "cbDFC";
            this.cbDFC.UseVisualStyleBackColor = true;
            this.cbDFC.CheckedChanged += new System.EventHandler(this.cbDFC_CheckedChanged);
            // 
            // cbLowNoise
            // 
            resources.ApplyResources(this.cbLowNoise, "cbLowNoise");
            this.cbLowNoise.Name = "cbLowNoise";
            this.cbLowNoise.UseVisualStyleBackColor = true;
            this.cbLowNoise.CheckedChanged += new System.EventHandler(this.cbLowNoise_CheckedChanged);
            // 
            // tbSpeed
            // 
            resources.ApplyResources(this.tbSpeed, "tbSpeed");
            this.tbSpeed.Maximum = 9;
            this.tbSpeed.Name = "tbSpeed";
            this.tbSpeed.Scroll += new System.EventHandler(this.tbSpeed_Scroll);
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
            this.numSpeed.ValueChanged += new System.EventHandler(this.numSpeed_ValueChanged);
            // 
            // lblBlackLevel
            // 
            resources.ApplyResources(this.lblBlackLevel, "lblBlackLevel");
            this.lblBlackLevel.Name = "lblBlackLevel";
            // 
            // tbBlackLevel
            // 
            resources.ApplyResources(this.tbBlackLevel, "tbBlackLevel");
            this.tbBlackLevel.Maximum = 1984;
            this.tbBlackLevel.Name = "tbBlackLevel";
            this.tbBlackLevel.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbBlackLevel.Scroll += new System.EventHandler(this.tbBlackLevel_Scroll);
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
            this.numBlackLevel.ValueChanged += new System.EventHandler(this.numBlackLevel_ValueChanged);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SettingsDialogSSG
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.numBlackLevel);
            this.Controls.Add(this.tbBlackLevel);
            this.Controls.Add(this.lblBlackLevel);
            this.Controls.Add(this.numSpeed);
            this.Controls.Add(this.lblSpeed);
            this.Controls.Add(this.tbSpeed);
            this.Controls.Add(this.grpBOptions);
            this.Controls.Add(this.boxGainControl);
            this.Name = "SettingsDialogSSG";
            this.Load += new System.EventHandler(this.SettingsDialogSSG_Load);
            this.boxGainControl.ResumeLayout(false);
            this.grpBOptions.ResumeLayout(false);
            this.grpBOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBlackLevel)).EndInit();
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
        private TrackBar tbBlackLevel;
        private NumericUpDown numBlackLevel;
        private CheckedListBox clbGC;
        private Button btnOK;
        private Button btnCancel;
    }
}