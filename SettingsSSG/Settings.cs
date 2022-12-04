﻿using System;
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
        int GC { get; set; }

        [JsonProperty("speed")]
        int Speed { get; set; }

        [JsonProperty ("low_noise")]
        int LowNoise { get; set; }

        [JsonProperty ("skip")]
        int Skip { get; set; }

        [JsonProperty ("blacklevel")]
        int BlackLevel { get; set; }

        [JsonProperty("dfc")]
        int DFC { get; set; }

        public Settings(int gC, int speed, int lowNoise, int skip, int blackLevel, int dFC)
        {
            GC = gC;
            Speed = speed;
            LowNoise = lowNoise;
            Skip = skip;
            BlackLevel = blackLevel;
            DFC = dFC;
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

                if(uSettings != null)
                {
                    this.GC = uSettings.GC;
                    this.Skip = uSettings.Skip;
                    this.BlackLevel= uSettings.BlackLevel;
                    this.DFC = uSettings.DFC;
                    this.LowNoise= uSettings.LowNoise;
                    this.Speed= uSettings.Speed;
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