using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace SettingsSSG
{
    internal class Settings
    {
        [JsonProperty("gc")]
        public int GC { get; set; }

        [JsonProperty("speed")]
        public int Speed { get; set; }

        [JsonProperty ("low_noise")]
        public int LowNoise { get; set; }

        [JsonProperty ("skip")]
        public int Skip { get; set; }

        [JsonProperty ("blacklevel")]
        public int BlackLevel { get; set; }

        [JsonProperty("dfc")]
        public int DFC { get; set; }

        [JsonProperty("heat")]
        public int Heat { get; set; }

        public Settings(int gC, int speed, int lowNoise, int skip, int blackLevel, int dFC, int heat)
        {
            GC = gC;
            Speed = speed;
            LowNoise = lowNoise;
            Skip = skip;
            BlackLevel = blackLevel;
            DFC = dFC;
            Heat = heat;
        }
        public Settings() { }

        public Settings(string fname)
        {
            Settings? uSettings= new Settings();
            if(File.Exists(fname))
            {
                string jsonString=File.ReadAllText(fname);
                try
                {
                    uSettings=JsonConvert.DeserializeObject<Settings>(jsonString);
                }
                catch 
                { 
                    uSettings = null; 
                }

                if (uSettings != null)
                {
                    this.GC = uSettings.GC;
                    this.Skip = uSettings.Skip;
                    this.BlackLevel = uSettings.BlackLevel;
                    this.DFC = uSettings.DFC;
                    this.LowNoise = uSettings.LowNoise;
                    this.Speed = uSettings.Speed;
                    this.Heat= uSettings.Heat;
                }
                else
                {
                    GC = 0;
                    Skip= 0;    
                    BlackLevel= 0;
                    DFC = 0;
                    LowNoise= 0;
                    Speed = 9;
                    Heat= 0;
                }
            }
        }

        public void WriteSettings(string fname)
        {
            string jsonStr = string.Empty;
            jsonStr = JsonConvert.SerializeObject(this);
            File.WriteAllText(fname, jsonStr);
        }
    }
}
