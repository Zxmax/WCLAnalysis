using System;
using Newtonsoft.Json;

namespace WCLAnalysis.Data
{
    public class Heal
    {
        public string Name { get; set; }
        [JsonProperty(PropertyName = "timestamp")]
        public long TimeUnix { get; set; }
        public int SourceId { get; set; }
        public int TargetId { get; set; }
        public bool SourceIsFriendly { get; set; }
        public bool TargetIsFriendly { get; set; }
        public int Amount { get; set; }
        public int OverHeal { get; set; }

        public Heal(string json)
        {
            try
            {
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(json);
                if (jsonObject != null)
                {
                    SourceId = jsonObject["sourceID"];
                    TargetId = jsonObject["targetID"];
                    Name = jsonObject["ability"]["name"];
                    Amount = jsonObject["amount"];
                    OverHeal = jsonObject["overheal"];
                    TimeUnix = jsonObject["timestamp"];
                    SourceIsFriendly = jsonObject["sourceIsFriendly"];
                    TargetIsFriendly = jsonObject["targetIsFriendly"];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


    }
}