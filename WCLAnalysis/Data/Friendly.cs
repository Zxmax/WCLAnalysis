using System.Collections.Generic;
using Newtonsoft.Json;

namespace WCLAnalysis.Data
{
    public class Friendly
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //职业
        public string Type { get; set; }
        //专精
        public string Spec { get; set; }
        //盟约
        public string Covenant { get; set; }
        public int CovenantId { get; set; }
        public double ItemLevel { get; set; }
        public List<Gear> Gears { get; set; } = new();
        public List<Talent> Talents { get; set; } = new();
        public string TalentNumber { get; set; }
        public List<int> Fights { get; set; } = new();
        public Friendly(string json)
        {
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(json);
            Id = jsonObject["id"];
            Name = (string)jsonObject["name"];
            Type = (string) jsonObject["type"] switch
            {
                "DeathKnight" => "Death Knight",
                "DemonHunter" => "Demon Hunter",
                _ => (string) jsonObject["type"]
            };
            foreach (var fight in jsonObject["fights"])
            {
                int id = fight["id"];
                Fights.Add(id);
            }
        }

    }

    public class Gear
    {
        public int Id { get; set; }
        public int ItemLevel { get; set; }
        public bool HasGems { get; set; }
    }

    public class Talent
    {
        public string Id { get; set; }
    }

}

