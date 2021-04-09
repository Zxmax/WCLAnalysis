using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using WCLAnalysis.Data;
using Newtonsoft.Json;

namespace WCLAnalysis.Service
{
    public class ReportService
    {
        private const string Url = "https://cn.warcraftlogs.com:443/v1/";
        private const string UrlParameters = "?api_key=" + Key;
        private const string Key = "2b1f831583857b7f53019586915226cf";
        private static readonly MongoClient Client = new MongoClient("mongodb://localhost:27017");
        private readonly IMongoDatabase _database = Client.GetDatabase("WOW");


        //api
        /// <summary>
        /// 根据战斗总id获取总的战斗json
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task<string> GetReportJsonStringByReportIdAsync(string reportId)
        {
            HttpClient client = new HttpClient();
            var getReportsUrl = Url + "report/fights/" + reportId;

            client.BaseAddress = new Uri(getReportsUrl);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            // List data response.
            var response = await client.GetAsync(UrlParameters);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            // Parse the response body.
            if (response.IsSuccessStatusCode)
            {
                var dataObjects =
                    await response.Content
                        .ReadAsStringAsync(); //Make sure to add a reference to System.Net.Http.Formatting.dll
                return dataObjects;
            }

            return null;
        }
        /// <summary>
        /// 根据角色和战斗id获取战斗日志summary
        /// </summary>
        /// <param name="reports"></param>
        /// <param name="reportId"></param>
        /// <param name="fightId"></param>
        /// <returns></returns>
        public async Task<string> GetCharacterFightJsonStringByReportAsync(List<Report> reports, string reportId, int fightId)
        {
            var startTime = reports.Find(p => p.Id == fightId)?.StartTimeUnix;
            var endTime = reports.Find(p => p.Id == fightId)?.EndTimeUnix;

            var client = new HttpClient();
            var getReportsUrl = Url + "report/events/summary/" + reportId;

            client.BaseAddress = new Uri(getReportsUrl);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var urlParameters = UrlParameters +
                             "&start=" + startTime + "&end=" + endTime;

            // List data response.
            var response = await client.GetAsync(urlParameters);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            // Parse the response body.
            if (response.IsSuccessStatusCode)
            {
                var dataObjects =
                    await response.Content
                        .ReadAsStringAsync(); //Make sure to add a reference to System.Net.Http.Formatting.dll
                return dataObjects;
            }
            return null;
        }
        /// <summary>
        /// 根据天赋得知专精
        /// </summary>
        /// <param name="talentList"></param>
        /// <returns></returns>
        public Tuple<string, List<string>> GetTalentByType(List<Talent> talentList)
        {
            var talentIntList = new List<string>();
            var spec = "";
            //use db to convert string to int

            var collection = _database.GetCollection<TalentClass>("wcl_talent");
            foreach (var doc in talentList.Select(talent => talent.Id).Select(talentId => collection.Find(p => p.TalentId == talentId).FirstOrDefault()))
            {
                if (spec == "")
                    spec = doc.SpecName;
                talentIntList.Add(doc.TalentNum.ToString());
            }
            var tuple =
                new Tuple<string, List<string>>(spec, talentIntList);

            return tuple;
        }
        /// <summary>
        /// 获取单人施法列表
        /// </summary>
        /// <param name="report"></param>
        /// <param name="reportId"></param>
        /// <param name="friendlyId"></param>
        /// <param name="isTrans"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, int>> GetCastAsync(Report report, string reportId, int friendlyId, bool isTrans)
        {
            var startTime = report.StartTimeUnix;
            var endTime = report.EndTimeUnix;

            HttpClient client = new HttpClient();
            var getReportsUrl = Url + "report/events/casts/" + reportId;

            client.BaseAddress = new Uri(getReportsUrl);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            string urlParameters;
            if (isTrans)
            {
                urlParameters = UrlParameters +
                                "&start=" + startTime + "&end=" + endTime + "&sourceid=" + friendlyId +
                                "&translate=true";
            }
            else
            {
                urlParameters = UrlParameters +
                                "&start=" + startTime + "&end=" + endTime + "&sourceid=" + friendlyId;
            }

            // List data response.
            var response = await client.GetAsync(urlParameters);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            var dpsHpsCast = new Dictionary<string, int>();
            var sortedCast = new Dictionary<string, int>();
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                var dataObjects = await response.Content.ReadAsStringAsync();//Make sure to add a reference to System.Net.Http.Formatting.dll

                //deserialize to your class
                var parsedObject = JObject.Parse(dataObjects);
                foreach (var cast in parsedObject["events"])
                {
                    var castType = new Cast(cast.ToString());
                    if (dpsHpsCast.ContainsKey(castType.Name))
                    {
                        dpsHpsCast[castType.Name] += 1;
                    }
                    else
                    {
                        dpsHpsCast.Add(castType.Name, 1);
                    }
                }
                sortedCast = dpsHpsCast.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            return sortedCast;
        }
        /// <summary>
        /// 获取相同天赋的模板人数
        /// </summary>
        /// <param name="bossId"></param>
        /// <param name="friendly"></param>
        /// <returns></returns>
        public async Task<Tuple<int, Dictionary<string, int>, double>> GetSameTalentCovenant(int bossId, Friendly friendly)
        {
            var collectionBoss = _database.GetCollection<Boss>("wclBoss");
            var bossName = collectionBoss.Find(p => p.EncounterId == bossId).FirstOrDefault().EncounterName;
            if (friendly.Spec == "Restoration" || friendly.Spec == "Mistweaver" || friendly.Spec == "Holy" || friendly.Spec == "Discipline")
                bossName = bossName.Replace(' ', '_') + "_HPS_talent";
            else
                bossName = bossName.Replace(' ', '_') + "_DPS_talent";
            var sameTalentCovenantCollection = _database.GetCollection<Rank100>(bossName);
            var filter2 = Builders<Rank100>.Filter.Eq("spec_name", friendly.Spec);
            var filter3 = Builders<Rank100>.Filter.Eq("covenant_id", friendly.CovenantId);
            var filter1 = Builders<Rank100>.Filter.Eq("class_name", friendly.Type);
            var results = await sameTalentCovenantCollection.Find(filter1 & filter2 & filter3).ToListAsync();
            for (var m = 0; m < results.Count; m++)
            {
                var tempRank100 = results[m];
                var isDelete = false;
                var talentTemp = tempRank100.Talent.Aggregate("", (current, p) => (string)(current + p));
                for (var i = 0; i < 7; i++)
                    if (talentTemp[i] != friendly.TalentNumber[i] && talentTemp[i] != 'X')
                        isDelete = true;
                if (isDelete)
                {
                    results.Remove(tempRank100);
                    m--;
                }
            }

            if (results.Count == 0)
                return new Tuple<int, Dictionary<string, int>, double>(0, new Dictionary<string, int>(), 0d);
            var model = results.First();
            var reportJson = await GetReportJsonStringByReportIdAsync(model.ReportId);
            var parsedObject = JObject.Parse(reportJson);
            var parsedJson = parsedObject["fights"]?.ToString();
            var reports = JsonConvert.DeserializeObject<List<Report>>(parsedJson ?? string.Empty);
            reports = (reports ?? throw new InvalidOperationException()).Where(p => p.Boss != 0).ToList();
            foreach (var report in reports)
            {
                // ReSharper disable once PossibleNullReferenceException
                report.StartTime = UnixToDateTime(parsedObject["start"].ToObject<long>() + report.StartTimeUnix);
                // ReSharper disable once PossibleNullReferenceException
                report.EndTime = UnixToDateTime(parsedObject["start"].ToObject<long>() + report.EndTimeUnix);
            }
            var friendlies = parsedObject["friendlies"]!.Select(jToken => new Friendly(jToken.ToString())).ToList();
            friendlies = friendlies.Where(p => p.Type != "NPC" && p.Type != "Boss").ToList();
            var friend = friendlies.Find(p => p.Name == model.CharacterName);
            var fight = reports.Find(p => p.StartTimeUnix == model.Start);

            var castsModel = await GetCastAsync(fight, model.ReportId, friend.Id, true);
            return new Tuple<int, Dictionary<string, int>, double>(results.Count, castsModel, model.Duration);
        }
        /// <summary>
        /// 获取团队爆发释放列表
        /// </summary>
        /// <param name="reports"></param>
        /// <param name="reportId"></param>
        /// <param name="fightId"></param>
        /// <param name="friendly"></param>
        /// <returns></returns>
        public async Task<List<Cast>> GetEruptList(List<Report> reports, string reportId, int fightId, Friendly friendly)
        {
            var startTime = reports.Find(p => p.Id == fightId)?.StartTimeUnix;
            var endTime = reports.Find(p => p.Id == fightId)?.EndTimeUnix;

            var client = new HttpClient();
            var getReportsUrl = Url + "report/events/casts/" + reportId;
            client.BaseAddress = new Uri(getReportsUrl);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var urlParameters = UrlParameters +
                                "&start=" + startTime + "&end=" + endTime + "&sourceid=" + friendly.Id;

            // List data response.
            var response = await client.GetAsync(urlParameters);
            if (!response.IsSuccessStatusCode) return null;
            {
                var dataObjects =
                    await response.Content
                        .ReadAsStringAsync(); //Make sure to add a reference to System.Net.Http.Formatting.dll

                var parsedObjectHeal = JObject.Parse(dataObjects);
                return (parsedObjectHeal["events"] ?? throw new InvalidOperationException()).Select(cast => new Cast(cast.ToString())).ToList();
            }
        }
        /// <summary>
        /// 获取爆发治疗列表
        /// </summary>
        /// <param name="allCastss"></param>
        /// <param name="startTimeUnix"></param>
        /// <returns></returns>
        public async Task<List<EruptTimeLine>> GetErupt(List<Cast> allCastss, long startTimeUnix)
        {
            var collection = _database.GetCollection<Erupt>("wow_erupt");
            var dicErupt = new List<EruptTimeLine>();
            string spec = null;
            var type = 0;
            foreach (var filter1 in allCastss.TakeWhile(_ => spec == null).Select(cast => Builders<Erupt>.Filter.Eq("CNName", cast.Name)))
            {
                var results = await collection.Find(filter1).ToListAsync();
                if (results.Count <= 0 || results.Count > 1) continue;
                spec = results.Find(p => p.Spec != null)?.Spec;
            }
            if (spec == "discipline")
                spec = "discipline";
            foreach (var cast in allCastss)
            {
                var filter1 = Builders<Erupt>.Filter.Eq("CNName", cast.Name);
                var filter2 = Builders<Erupt>.Filter.Gt("CoolDown", 59);
                try
                {
                    var results = await collection.Find(filter1 & filter2).ToListAsync();
                    switch (results.Count)
                    {
                        case <= 0:
                            continue;
                        case 1:
                            type = spec is "restoration" or "mistweaver" or "holy" or "discipline" ? 2 : 1;
                            break;
                        case > 1:
                            var temp = results.Find(p => p.Spec == spec);
                            if (temp != null) type = temp.Type;
                            break;
                    }

                    var values = dicErupt.FindAll(p => p.Name == cast.Name);
                    if (values.Count > 0)
                        if (values.Max(p => p.Time) > (cast.TimeUnix - startTimeUnix) / 1000.0 - 40)
                            continue;
                    dicErupt.Add(new EruptTimeLine(cast.Name, (cast.TimeUnix - startTimeUnix) / 1000.0, spec, type,
                        cast.SourceId));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return dicErupt;

        }



        #region inner class

        public class TalentClass
        {
            public ObjectId Id { get; set; }
            [BsonElement("class_name")]
            public string ClassName { get; set; }
            [BsonElement("spec_name")]
            public string SpecName { get; set; }
            [BsonElement("talent_id")]
            public string TalentId { get; set; }
            [BsonElement("talent_num")]
            public int TalentNum { get; set; }
            [BsonElement("remark")]
            public string Remark { get; set; }

        }
        [BsonIgnoreExtraElements]
        public class Boss
        {
            public ObjectId Id { get; set; }
            [BsonElement("encounterId")]
            public int EncounterId { get; set; }
            [BsonElement("encounter_name")]
            public string EncounterName { get; set; }
        }
        [BsonIgnoreExtraElements]
        public class Rank100
        {
            public ObjectId Id { get; set; }
            [BsonElement("character_name")]
            public string CharacterName { get; set; }
            [BsonElement("class_name")]
            public string ClassName { get; set; }
            [BsonElement("covenant_id")]
            public int CovenantId { get; set; }
            [BsonElement("duration")]
            public double Duration { get; set; }
            [BsonElement("start")]
            public long Start { get; set; }
            [BsonElement("end")]
            public long End { get; set; }
            [BsonElement("report_id")]
            public string ReportId { get; set; }
            [BsonElement("fight_id")]
            public int FightId { get; set; }
            [BsonElement("spec_name")]
            public string SpecName { get; set; }
            [BsonElement("talent")]
            public List<dynamic> Talent { get; set; }
            //[BsonElement("total")]
            //public string Total { get; set; }
        }
        [BsonIgnoreExtraElements]
        public class Erupt
        {
            public ObjectId Id { get; set; }
            [BsonElement("Type")]
            public int Type { get; set; }
            [BsonElement("Spec")]
            public string Spec { get; set; }

        }

        #endregion


        #region method

        /// <summary>
        /// 时间戳转换
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        private DateTime UnixToDateTime(long unixTimeStamp)
        {
#pragma warning disable 618
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
#pragma warning restore 618
            var dt = startTime.AddMilliseconds(unixTimeStamp);
            return dt;
        }

        #endregion

    }
}
