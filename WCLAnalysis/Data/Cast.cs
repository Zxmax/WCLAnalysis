using Newtonsoft.Json;

namespace WCLAnalysis.Data
{
    public class Cast
    {
        public string Name { get; set; }
        public int SourceId { get; set; }
        public bool SourceIsFriendly { get; set; }
        public long TimeUnix { get; set; }
        public Cast(string json)
        {
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(json);
            if (jsonObject == null) return;
            if(jsonObject["type"]!= "cast") return;
            SourceId = jsonObject["sourceID"];
            Name = jsonObject["ability"]["name"];
            SourceIsFriendly = jsonObject["sourceIsFriendly"];
            TimeUnix = jsonObject["timestamp"];
        }

    }
}

