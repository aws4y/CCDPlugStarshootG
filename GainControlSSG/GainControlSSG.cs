namespace GainControlSSG
{
    public partial class SSGGainControlWindow : Form
    {
        public SSGGainControlWindow()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void tbGain_Scroll(object sender, EventArgs e)
        {
            numGain.Value=tbGain.Value;
        }

        private void numGain_ValueChanged(object sender, EventArgs e)
        {
            tbGain.Value=(int)numGain.Value;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSet_Click(object sender, EventArgs e)
        {

        }

        private void btnDefault_Click(object sender, EventArgs e)
        {

        }
    }
}