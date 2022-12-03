namespace SettingsSSG
{
    public partial class SettingsDialogSSG : Form
    {
        public SettingsDialogSSG()
        {
            InitializeComponent();
        }

        private void clbGC_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedListBox clbGC=(CheckedListBox)sender;
            if(clbGC.SelectedIndex ==0)
            {

            }

        }
    }
}