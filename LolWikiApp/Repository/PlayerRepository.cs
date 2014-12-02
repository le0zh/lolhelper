using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Windows.Storage;
using HtmlAgilityPack;
using Microsoft.Phone.Controls.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LolWikiApp.Repository
{
    public class PlayerRepository : Repository
    {
        private const string PlayerSettingsKey = "_playerSettings";

        private const string CurrentMatchUrlForamt = "http://lolbox.duowan.com/phone/apiCurrentMatch.php?action=getCurrentMatch&serverName={0}&OSType=iOS7.1.1&target={1}";

        public async Task<CurrentGameInfo> GetCurrentGameInfoAsync(string serverName, string userName)
        {
            var url = string.Format(CurrentMatchUrlForamt, serverName, userName);
            var json = await GetJsonStringViaHttpAsync(url);
            //string json = string.Empty;

            if (string.IsNullOrEmpty(json))
                return null;

            //var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/apiCurrentMatch.json"));
            //using (var stream = await file.OpenReadAsync())
            //using(StreamReader sr = new StreamReader(stream.AsStream()))
            //{
            //    json = await sr.ReadToEndAsync();
            //}

            var jObject = JObject.Parse(json);
            var gameInfo = JsonConvert.DeserializeObject<CurrentGameInfo>(jObject["gameInfo"].ToString());

            var sort100List = JsonConvert.DeserializeObject<List<string>>(jObject["gameInfo"]["100_sort"].ToString());
            var sort200List = JsonConvert.DeserializeObject<List<string>>(jObject["gameInfo"]["200_sort"].ToString());

            gameInfo.Sort100 = sort100List;
            gameInfo.Sort200 = sort200List;

            var user100HerosDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jObject["gameInfo"]["100"].ToString());
            var user200HerosDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jObject["gameInfo"]["200"].ToString());

            foreach (string s in sort100List)
            {
                var p = JsonConvert.DeserializeObject<PlayerInfo>(jObject["playerInfo"][s].ToString());
                p.Name = s;
                p.HeroName = user100HerosDict[s];
                gameInfo.Sort100PlayerInfos.Add(p);
            }

            foreach (string s in sort200List)
            {
                var p = JsonConvert.DeserializeObject<PlayerInfo>(jObject["playerInfo"][s].ToString());
                p.Name = s;
                p.HeroName = user200HerosDict[s];
                gameInfo.Sort200PlayerInfos.Add(p);
            }

            return gameInfo;
        }

        public void SavePlayerInfo(string userName, ServerInfo serverInfo)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            var wrapper = new PlayerInfoSettingWrapper()
            {
                Name = userName,
                ServerInfo = serverInfo
            };

            if (settings.Contains(PlayerSettingsKey))
            {
                settings[PlayerSettingsKey] = wrapper;
            }
            else
            {
                settings.Add(PlayerSettingsKey, wrapper);
            }

            settings.Save();
        }

        public Player ReadPlayerInfoSettings()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(PlayerSettingsKey))
            {
                return null;
            }

            var wrapper =
                IsolatedStorageSettings.ApplicationSettings[PlayerSettingsKey] as PlayerInfoSettingWrapper;
            if (wrapper != null)
            {
                var p = new Player { Name = wrapper.Name, ServerInfo = wrapper.ServerInfo, IsDataLoaded = false };

                return p;
            }
            else
            {
                return null;
            }
        }

        private Player PharsePlayerInfo(HtmlDocument doc)
        {
            var player = new Player();

            //解析返回的html内容
            var sb = new StringBuilder();

            var infoPathDictionary = new Dictionary<string, string>();

            HtmlNode headerSectionNode = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/header[1]/section[1]");

            infoPathDictionary.Add("Name", "div[1]/span[1]");
            infoPathDictionary.Add("Good", "div[1]/span[2]");
            infoPathDictionary.Add("Bad", "div[1]/span[3]");
            infoPathDictionary.Add("Power", "div[2]/em[1]");
            infoPathDictionary.Add("Level", "span[1]/em[1]");

            player.Name = GetPlayerInfo(infoPathDictionary["Name"], headerSectionNode);
            int tmp = 0;
            int.TryParse(GetPlayerInfo(infoPathDictionary["Good"], headerSectionNode), out tmp);
            player.Good = tmp;
            int.TryParse(GetPlayerInfo(infoPathDictionary["Bad"], headerSectionNode), out tmp);
            player.Bad = tmp;
            int.TryParse(GetPlayerInfo(infoPathDictionary["Power"], headerSectionNode), out tmp);
            player.Power = tmp;
            int.TryParse(GetPlayerInfo(infoPathDictionary["Level"], headerSectionNode), out tmp);
            player.Level = tmp;


            //头像
            HtmlNode userImgNode = headerSectionNode.SelectSingleNode("span[1]/a[1]/img[1]");
            if (userImgNode != null)
            {
                player.PhotoUrl = userImgNode.Attributes["src"].Value;
            }

            //从script中读取排位和战斗力信息
            var scriptNode = doc.DocumentNode.SelectSingleNode("/html[1]/head[1]/script[2]");
            if (scriptNode != null)
            {
                string[] functions = scriptNode.InnerText.Split(new string[] { "function" },
                    StringSplitOptions.RemoveEmptyEntries);
                string showDataFoo = functions.Last();
                string[] ifStatements = showDataFoo.Split(new string[] { "if" }, StringSplitOptions.RemoveEmptyEntries);
                var liRegex = new Regex("(?<=<li>)[\\s\\S]+?(?=</li>)");
                string rankInfo = ifStatements[ifStatements.Length - 2];
                string li1 = liRegex.Match(rankInfo).ToString();
                if (!string.IsNullOrEmpty(li1))
                {
                    var rankDoc = new HtmlDocument();
                    rankDoc.LoadHtml(li1);

                    var rankGameInfo = new RankGameInfo
                    {
                        Type = rankDoc.DocumentNode.SelectSingleNode("span[1]").InnerText,
                        WinRate = rankDoc.DocumentNode.SelectSingleNode("span[2]").InnerText,
                        WinNumber = rankDoc.DocumentNode.SelectSingleNode("span[3]").InnerText,
                        RangeAndWinPoint = rankDoc.DocumentNode.SelectSingleNode("span[4]").InnerText
                    };

                    player.RankGmeInfo = rankGameInfo;
                }

                string zdlInfo = ifStatements[ifStatements.Length - 1];
                string li2 = liRegex.Match(zdlInfo).ToString();
                if (!string.IsNullOrEmpty(li2))
                {
                    var zdlDoc = new HtmlDocument();
                    zdlDoc.LoadHtml(li2);

                    var powerDetailInfo = new PowerDetailInfo
                    {
                        TotaScore = zdlDoc.DocumentNode.SelectSingleNode("span[1]").InnerText,
                        BaseScore = zdlDoc.DocumentNode.SelectSingleNode("span[2]").InnerText,
                        WinRateScore = zdlDoc.DocumentNode.SelectSingleNode("span[3]").InnerText,
                        WinNumberScore = zdlDoc.DocumentNode.SelectSingleNode("span[4]").InnerText
                    };

                    player.PowerDetailInfo = powerDetailInfo;
                }
            }
            else
            {
                Debug.WriteLine("not found script");
            }

            HtmlNode mainContentSectionNode = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/section[1]");

            //战绩
            HtmlNodeCollection scoreNodes = mainContentSectionNode.SelectNodes("div[1]/ul[1]/li");
            if (scoreNodes != null)
            {
                foreach (var scoreNode in scoreNodes)
                {
                    var gameInfo = new MatchGameInfo
                    {
                        Mode = scoreNode.SelectSingleNode("span[1]").InnerText,
                        WinRate = scoreNode.SelectSingleNode("span[2]").InnerText,
                        WinNumber = scoreNode.SelectSingleNode("span[3]").InnerText,
                        LoseNumber = scoreNode.SelectSingleNode("span[4]").InnerText
                    };

                    player.MatchGameInfos.Add(gameInfo);
                }
            }

            //常用英雄
            HtmlNodeCollection frequentUsedHeroNodes = mainContentSectionNode.SelectNodes("div[2]/ul[1]/li");
            if (frequentUsedHeroNodes != null)
            {
                foreach (HtmlNode heroNode in frequentUsedHeroNodes)
                {
                    HtmlNode heroImgNode = heroNode.SelectSingleNode("a[1]/img[1]");
                    if (heroImgNode != null)
                    {
                        player.RecentUsedHeroImageList.Add(heroImgNode.Attributes["src"].Value);
                    }
                }
            }

            //最近比赛
            HtmlNodeCollection recentMatchNodes = mainContentSectionNode.SelectNodes("div[3]/ul[1]/li");
            if (recentMatchNodes != null)
            {
                foreach (HtmlNode matchNode in recentMatchNodes)
                {
                    HtmlNode matchHeroImgNode = matchNode.SelectSingleNode("span[1]/img[1]");
                    if (matchHeroImgNode == null)
                        continue;

                    var gameInfo = new GameInfo();

                    string moreInfoUrl = matchNode.Attributes["onclick"].Value;
                    //Regex urlRegex = new Regex("(?<=()[\\s\\S]+?(?=);)");
                    moreInfoUrl = moreInfoUrl.Trim().Substring(21);
                    moreInfoUrl = moreInfoUrl.Substring(0, moreInfoUrl.LastIndexOf('\''));

                    gameInfo.GameDetailUrl = moreInfoUrl;

                    gameInfo.HeroImageUrl = matchHeroImgNode.Attributes["src"].Value;
                    gameInfo.GameMode = matchNode.SelectSingleNode("span[2]").InnerText;
                    gameInfo.Result = matchNode.SelectSingleNode("span[3]").InnerText;
                    gameInfo.Date = matchNode.SelectSingleNode("span[4]/span[1]").InnerText;

                    player.RecentGameInfoList.Add(gameInfo);
                }
            }

            player.IsDataLoaded = true; //数据加载完毕

            return player;
        }

        public async Task<HttpActionResult> GameDetailTest()
        {
            const string notFoundTitle = "召唤师搜索";
            var httpActionResult = new HttpActionResult();

            var client = new HttpClient();
            const string url = @"http://lolbox.duowan.com/matchList.php?serverName=%E7%BD%91%E9%80%9A%E5%9B%9B&playerName=%E6%B5%AA%E6%BD%AE%E4%B9%8B%E5%B7%85#10249034537,NORMAL";

            string content;

            try
            {
                content = await client.GetStringAsync(new Uri(url));
            }
            catch (Exception exception404)
            {
                //HTTP请求有异常
                httpActionResult.Result = ActionResult.Exception404;
                return httpActionResult;
            }

            httpActionResult.Result = ActionResult.Success;
            httpActionResult.Value = content;
            return httpActionResult;
        }

        public async Task<HttpActionResult> PharsePlayerInfo(string sn, string pn)
        {
            const string notFoundTitle = "召唤师搜索";
            var httpActionResult = new HttpActionResult();

            var client = new HttpClient();
            const string urlFormat = @"http://lolbox.duowan.com/phone/playerDetail_ios.php?sn={0}&target={1}";
            string url = string.Format(urlFormat, sn, pn);

            string content;

            try
            {
                content = await client.GetStringAsync(new Uri(url));
            }
            catch (Exception exception404)
            {
                //HTTP请求有异常
                httpActionResult.Result = ActionResult.Exception404;
                return httpActionResult;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            var titleNode = doc.DocumentNode.SelectSingleNode("/html[1]/head[1]/title[1]");

            if (titleNode != null)
            {
                if (titleNode.InnerText == notFoundTitle)
                {
                    httpActionResult.Result = ActionResult.NotFound;
                    return httpActionResult;
                }
            }

            httpActionResult.Result = ActionResult.Success;
            httpActionResult.Value = PharsePlayerInfo(doc);
            return httpActionResult;
        }

        public void RemovePlayerInfoFromSettings()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(PlayerSettingsKey))
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(PlayerSettingsKey);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        private string GetPlayerInfo(string path, HtmlNode parentNode)
        {
            string value = string.Empty;
            HtmlNode node = parentNode.SelectSingleNode(path);
            if (node != null) value = node.InnerText;

            return value;
        }

        public GameDetailInfo ParseGameDetailTest(string htmlContent)
        {
            var gameDeatil = new GameDetailInfo();
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var gameTypeNode = doc.DocumentNode.SelectSingleNode("/html/body/section/div[1]/div[1]/div/span[1]");
            var gameDurationNode = doc.DocumentNode.SelectSingleNode("/html/body/section/div[1]/div[1]/div/span[2]");
            var uploadedTimeNode = doc.DocumentNode.SelectSingleNode("/html/body/section/div[1]/div[1]/div/span[3]");

            gameDeatil.GameType = gameTypeNode.InnerText.Trim();
            gameDeatil.GameDuration = gameDurationNode.InnerText.Trim();
            gameDeatil.UploadedTime = uploadedTimeNode.InnerText.Trim();

            var winTeamMemberNodes = doc.DocumentNode.SelectNodes("/html/body/section/div[1]/div[2]/ul/li");
            var loseTeamMemberNodes = doc.DocumentNode.SelectNodes("/html/body/section/div[2]/div[2]/ul/li");

            foreach (var winTeamMemberNode in winTeamMemberNodes)
            {
                gameDeatil.WonTeam.Add(ParsePlayerInfo(winTeamMemberNode));
            }

            foreach (var loseTeamMemberNode in loseTeamMemberNodes)
            {
                gameDeatil.LoseTeam.Add(ParsePlayerInfo(loseTeamMemberNode));
            }

            var scoreNode = doc.DocumentNode.SelectSingleNode("/html/body/section/div[2]/div[1]/div/span[1]");
            var moneyNode = doc.DocumentNode.SelectSingleNode("/html/body/section/div[2]/div[1]/div/span[2]");

            gameDeatil.ScoreInfo = scoreNode.InnerHtml.Trim();
            gameDeatil.MoneyInfo = moneyNode.InnerHtml.Trim();
            
            return gameDeatil;
        }

        private TeamMember ParsePlayerInfo(HtmlNode playerNode)
        {
            var member = new TeamMember();

            #region 基本信息
            var playerNameNode = playerNode.SelectSingleNode("div[1]/a/div/p/span[1]");
            var heroIconNode = playerNode.SelectSingleNode("div[1]/a/span/img");
            var scoreNode = playerNode.SelectSingleNode("div[1]/a/div/p/span[2]");

            var masterIconsNodes = playerNode.SelectNodes("div[1]/a/div/p/img");
            if (masterIconsNodes != null)
            {
                foreach (var masterIconNode in masterIconsNodes)
                {
                    member.MasterIconList.Add(masterIconNode.Attributes["src"].Value);                    
                }
            }

            var zbIconsNodes = playerNode.SelectNodes("div[1]/a/div/img");
            if (zbIconsNodes != null)
            {
                foreach (var zbIconNode in zbIconsNodes)
                {
                    member.ZbIconList.Add(zbIconNode.Attributes["src"].Value);                    
                }
            }

            //TODO: change the defualt value
            member.Name = playerNameNode.InnerHtml.Trim();
            member.HeroIcon = heroIconNode != null ? heroIconNode.Attributes["src"].Value : "notfound"; 
            
            var scoreAll = scoreNode != null ? scoreNode.InnerHtml.Trim() : "0/0/0";
            var scoreParts = scoreAll.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (scoreParts.Length == 3)
            {
                member.Kill = scoreParts[0];
                member.Dead = scoreParts[1];
                member.Assistant = scoreParts[2];
            }
            #endregion

            #region 详细信息
            var skillIconsNodes = playerNode.SelectNodes("div[2]/div/img");
            if (skillIconsNodes != null)
            {
                foreach (var skillIconNode in skillIconsNodes)
                {
                    member.SkillIconList.Add(skillIconNode.Attributes["src"].Value);                    
                }
            }

            //补兵
            var killUnitNode = playerNode.SelectSingleNode("div[2]/p[1]/em[1]/span[1]");
            member.KillUnitDesc = killUnitNode.InnerHtml.Trim();

            //推塔
            var killTowerNode = playerNode.SelectSingleNode("div[2]/p[1]/em[1]/span[2]");
            member.KillTowerDesc = killTowerNode.InnerHtml.Trim();

            //金钱
            var moneyNode = playerNode.SelectSingleNode("div[2]/p[1]/em[2]/span[1]");
            member.MoneyDesc = moneyNode.InnerHtml.Trim();

            //对英雄伤害
            var demageNode = playerNode.SelectSingleNode("div[2]/p[1]/em[2]/span[2]");
            member.DemageDesc = demageNode.InnerHtml.Trim();

            //连杀
            var serialKillNode = playerNode.SelectSingleNode("div[2]/p[2]/span[1]");
            member.SerialKillDesc = serialKillNode.InnerHtml.Trim();

            //多杀
            var multiKillNode = playerNode.SelectSingleNode("div[2]/p[2]/span[2]");
            member.MultiKillDesc = multiKillNode.InnerHtml.Trim();

            //暴击
            var criticalNode = playerNode.SelectSingleNode("div[2]/p[2]/span[3]");
            member.CriticalDesc = criticalNode.InnerHtml.Trim();

            //放眼数
            var putEyeNode = playerNode.SelectSingleNode("div[2]/p[3]/span[1]");
            member.PutEyeDesc = putEyeNode.InnerHtml.Trim();

            //排眼数
            var clearEyeNode = playerNode.SelectSingleNode("div[2]/p[3]/span[2]");
            member.ClearEyeDesc = clearEyeNode.InnerHtml.Trim();
            #endregion           

            return member;
        }
    }

    public class TeamMember
    {
        public TeamMember()
        {
            ZbIconList = new List<string>();
            SkillIconList = new List<string>();
            MasterIconList = new List<string>();
        }

        public string Name { get; set; }

        public string Kill { get; set; }

        public string Dead { get; set; }

        public string Assistant { get; set; }

        public string HeroIcon { get; set; }

        public List<string> ZbIconList { get; private set; }

        public List<string> SkillIconList { get; private set; }

        public List<string> MasterIconList { get; private set; }

        public string KillUnitDesc { get; set; }

        public string KillTowerDesc { get; set; }

        public string MoneyDesc { get; set; }

        public string DemageDesc { get; set; }

        public string SerialKillDesc { get; set; }

        public string MultiKillDesc { get; set; }

        public string CriticalDesc { get; set; }

        public string PutEyeDesc { get; set; }

        public string ClearEyeDesc { get; set; }
    }

    public class GameDetailInfo
    {
        public GameDetailInfo()
        {
            WonTeam = new List<TeamMember>();
            LoseTeam = new List<TeamMember>();
        }

        public string GameType { get; set; }

        public string GameDuration { get; set; }

        public string UploadedTime { get; set; }

        public string ScoreInfo { get; set; }

        public string MoneyInfo { get; set; }

        public List<TeamMember> WonTeam { get; private set; }

        public List<TeamMember> LoseTeam { get; private set; }
    }
}
