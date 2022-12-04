
using System.IO;
namespace SettingsSSG
{
    public partial class SettingsDialogSSG : Form
    {
        private Settings settings;
        private string settingsFname;
        public SettingsDialogSSG()
        {
            InitializeComponent();
            
            settingsFname= Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\bin\\StarshootG\\Settings.json";
            settings = new Settings(settingsFname);

            tbBlackLevel.Value = settings.BlackLevel;
            numBlackLevel.Value = settings.BlackLevel;  
            tbSpeed.Value = settings.Speed;
            numSpeed.Value = settings.Speed;
            cbDFC.Checked = settings.DFC >= 1 ? true: false ;
            cbLowNoise.Checked = settings.LowNoise >= 1 ? true : false ; 
            cbSkip.Checked= settings.Skip >= 1 ? true : false ;
            clbGC.SetItemChecked(settings.GC, true);

        }

        private void clbGC_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedListBox clbGC=(CheckedListBox)sender;
            settings.GC = clbGC.SelectedIndex;

        }

        private void grpBOptions_Enter(object sender, EventArgs e)
        {
            //do nothing
        }

        private void tbSpeed_Scroll(object sender, EventArgs e)
        {
            numSpeed.Value= tbSpeed.Value;
            settings.Speed= tbSpeed.Value;
        }

        private void numSpeed_ValueChanged(object sender, EventArgs e)
        {
            tbSpeed.Value=(int)numSpeed.Value;
            settings.Speed = tbSpeed.Value;
        }

        private void SettingsDialogSSG_Load(object sender, EventArgs e)
        {

        }

        private void tbBlackLevel_Scroll(object sender, EventArgs e)
        {
            numBlackLevel.Value= tbBlackLevel.Value;
            settings.BlackLevel= (int)numBlackLevel.Value;
        }

        private void numBlackLevel_ValueChanged(object sender, EventArgs e)
        {
            tbBlackLevel.Value=(int)numBlackLevel.Value;
            settings.BlackLevel=(int)numBlackLevel.Value;
        }

        private void cbLowNoise_CheckedChanged(object sender, EventArgs e)
        {
            settings.LowNoise= cbLowNoise.Checked? 1: 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            settings.WriteSettings(settingsFname);
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if(File.Exists(settingsFname)) 
            { 
                Close();
            }
            else
            {
                settings.WriteSettings(settingsFname);
                Close(); 
            }
        }

        private void cbDFC_CheckedChanged(object sender, EventArgs e)
        {
            settings.DFC=cbDFC.Checked? 1: 0;
        }

        private void cbSkip_CheckedChanged(object sender, EventArgs e)
        {
            settings.Skip  =    cbSkip.Checked? 1: 0;
        }
    }
}