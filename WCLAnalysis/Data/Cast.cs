using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WCLAnalysis.Data
{
    public class Cast
    {
        public string Name { get; set; }
        public int SourceId { get; set; }
        public bool SourceIsFriendly { get; set; }
        public Cast(string json)
        {
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(json);
            SourceId = jsonObject["sourceID"];
            Name = jsonObject["ability"]["name"];
            SourceIsFriendly =jsonObject["sourceIsFriendly"];
        }

    }
}

