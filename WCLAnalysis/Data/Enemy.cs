using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WCLAnalysis.Data
{
    public class Enemy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> Fights { get; set; } = new();
        public Enemy(string json)
        {
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(json);
            Id = jsonObject["id"];
            Name = (string)jsonObject["name"];
            foreach (var fight in jsonObject["fights"])
            {
                int id = fight["id"];
                Fights.Add(id);
            }
        }

    }
}
