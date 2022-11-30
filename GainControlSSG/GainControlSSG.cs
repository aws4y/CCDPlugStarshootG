using System.IO;

namespace GainControlSSG
{
    public partial class SSGGainControlWindow : Form
    {
        public Gain gain;
        public Gain defGain;
        string fNameGainSetting;

        public SSGGainControlWindow()
        {
            InitializeComponent();

            defGain = new Gain("Default.json");

            fNameGainSetting = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\MaxIm DL 6\\Settings\\CCDPlugStarshootG\\Gain.json";
            gain = new Gain(fNameGainSetting);
            numGain.Value=gain.Value;
            tbGain.Value= gain.Value;

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void tbGain_Scroll(object sender, EventArgs e)
        {
            numGain.Value=tbGain.Value;
            gain.Value = tbGain.Value;
        }

        private void numGain_ValueChanged(object sender, EventArgs e)
        {
            tbGain.Value=(int)numGain.Value;
            gain.Value=(int)numGain.Value;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            gain.Value = (int)numGain.Value;
            gain.WriteGainSetting(fNameGainSetting);
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            defGain.WriteGainSetting(fNameGainSetting);
        }
    }
}