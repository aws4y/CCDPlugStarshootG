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
            clbGC = new System.Windows.Forms.CheckedListBox();
            this.boxGainControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // boxGainControl
            // 
            this.boxGainControl.Controls.Add(clbGC);
            resources.ApplyResources(this.boxGainControl, "boxGainControl");
            this.boxGainControl.Name = "boxGainControl";
            this.boxGainControl.TabStop = false;
            // 
            // clbGC
            // 
            clbGC.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(clbGC, "clbGC");
            clbGC.FormattingEnabled = true;
            clbGC.Items.AddRange(new object[] {
            resources.GetString("clbGC.Items"),
            resources.GetString("clbGC.Items1")});
            clbGC.Name = "clbGC";
            clbGC.SelectedIndexChanged += new System.EventHandler(this.clbGC_SelectedIndexChanged);
            // 
            // SettingsDialogSSG
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.boxGainControl);
            this.Name = "SettingsDialogSSG";
            this.boxGainControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox boxGainControl;
    }
}