using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WCLAnalysis.Data
{
    public class Report
    {
        public int Id { get; set; }
        public int Boss { get; set; }
        [JsonProperty(PropertyName = "start_time")]
        public long StartTimeUnix { get; set; }
        public DateTime StartTime { get; set; }
        [JsonProperty(PropertyName = "end_time")]
        public long EndTimeUnix { get; set; }
        public DateTime EndTime { get; set; }
        public string Name { get; set; }
        public DifficultyType Difficulty { get; set; }
        public bool Kill { get; set; }

        public enum DifficultyType
        {
            Lfr = 1,//随机难度
            Temp = 2,//占位符
            Normal = 3,//普通
            Hero = 4,//英雄
            Mythic = 5//史诗
        }
    }
}

