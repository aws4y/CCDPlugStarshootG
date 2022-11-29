namespace GainControlSSG
{
    partial class SSGGainControlWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SSGGainControlWindow));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblLow = new System.Windows.Forms.Label();
            this.tbGain = new System.Windows.Forms.TrackBar();
            this.lblHi = new System.Windows.Forms.Label();
            this.numGain = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.tbGain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGain)).BeginInit();
            this.SuspendLayout();
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
            // lblLow
            // 
            resources.ApplyResources(this.lblLow, "lblLow");
            this.lblLow.Name = "lblLow";
            // 
            // tbGain
            // 
            resources.ApplyResources(this.tbGain, "tbGain");
            this.tbGain.Maximum = 15000;
            this.tbGain.Minimum = 100;
            this.tbGain.Name = "tbGain";
            this.tbGain.TickFrequency = 100;
            this.tbGain.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.tbGain.Value = 100;
            this.tbGain.Scroll += new System.EventHandler(this.tbGain_Scroll);
            // 
            // lblHi
            // 
            resources.ApplyResources(this.lblHi, "lblHi");
            this.lblHi.Name = "lblHi";
            // 
            // numGain
            // 
            resources.ApplyResources(this.numGain, "numGain");
            this.numGain.Maximum = new decimal(new int[] {
            15000,
            0,
            0,
            0});
            this.numGain.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numGain.Name = "numGain";
            this.numGain.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numGain.ValueChanged += new System.EventHandler(this.numGain_ValueChanged);
            // 
            // SSGGainControlWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numGain);
            this.Controls.Add(this.lblHi);
            this.Controls.Add(this.tbGain);
            this.Controls.Add(this.lblLow);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Name = "SSGGainControlWindow";
            ((System.ComponentModel.ISupportInitialize)(this.tbGain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGain)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnOK;
        private Button btnCancel;
        private Label lblLow;
        private TrackBar tbGain;
        private Label lblHi;
        private NumericUpDown numGain;
    }
}