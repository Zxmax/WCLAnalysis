using System;
using Newtonsoft.Json;

namespace WCLAnalysis.Data
{
    public class DamageDone
    {
        public string Name { get; set; }
        [JsonProperty(PropertyName = "timestamp")]
        public long TimeUnix { get; set; }
        public int SourceId { get; set; }
        public int TargetId { get; set; }
        public bool SourceIsFriendly { get; set; }
        public bool TargetIsFriendly { get; set; }
        //dot
        public bool Tick { get; set; } 
        public int Amount { get; set; }
        
        public DamageDone(string json)
        {
            try
            {
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(json);
                if (jsonObject == null) return;
                SourceId = jsonObject["sourceID"];
                TargetId = jsonObject["targetID"];
                Name = jsonObject["ability"]["name"];
                Amount = jsonObject["amount"];
                if (jsonObject["tick"] != null)
                    Tick = jsonObject["tick"];
                TimeUnix = jsonObject["timestamp"];
                SourceIsFriendly = jsonObject["sourceIsFriendly"];
                TargetIsFriendly = jsonObject["targetIsFriendly"];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}