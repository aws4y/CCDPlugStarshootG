using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
    }
}
