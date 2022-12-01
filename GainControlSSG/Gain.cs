using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace GainControlSSG
{
    public class Gain
    {
        [JsonProperty ("max_gain")]
        public int Max { get; set; }
        [JsonProperty ("min_gain")]
        public int Min { get; set; }
        [JsonProperty ("gain")]
        public int Value { get; set; }

        public Gain() { }
        public Gain(string fname)
        {
            Gain? newGain = null;
            //read the Gain from a file if it exists
            if (File.Exists(fname))
            {
                string gainJson = File.ReadAllText(fname);
                //if the input has  data
                if (!string.IsNullOrEmpty(gainJson))
                {
                    //Try to covert the data into a new gain value
                    try
                    {
                        newGain = JsonConvert.DeserializeObject<Gain>(gainJson);
                    }
                    catch
                    {
                        newGain = null;
                    }
                }
            }
            //if we fail to get gain data
            if (newGain is null)
            {
                newGain = new Gain();
                newGain.Value = 100;
                newGain.Min = 100;
                newGain.Max = 15000;
            }
            this.Min = newGain.Min;
            this.Max = newGain.Max;
            this.Value = newGain.Value;
        }
        public void WriteGainSetting(string fname)
        {
            string gainJson;

            gainJson = JsonConvert.SerializeObject(this);

            if(!string.IsNullOrEmpty(gainJson)) 
            { 
                //if(File.Exists(fname))
                //{ 
                //    File.Delete(fname); 
                //}

                File.WriteAllText(fname, gainJson);

            }
        }
    }
}
