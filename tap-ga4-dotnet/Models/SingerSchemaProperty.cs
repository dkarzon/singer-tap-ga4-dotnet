using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SingerTapGA4.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingerTapGA4.Models
{
    public class SingerSchemaProperty
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string> Type { get; set; }

        [JsonProperty("format", NullValueHandling=NullValueHandling.Ignore)]
        public string Format { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Children { get; set; }
    }
}
