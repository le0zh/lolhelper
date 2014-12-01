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

        private const string CurrentMatchUrlForamt =
            "http://lolbox.duowan.com/phone/apiCurrentMatch.php?action=getCurrentMatch&serverName={0}&OSType=iOS7.1.1&target={1}";

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
                var p = new Player {Name = wrapper.Name, ServerInfo = wrapper.ServerInfo, IsDataLoaded = false};

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
                foreach (HtmlNode scoreNode in scoreNodes)
                {
                    MatchGameInfo gameInfo = new MatchGameInfo();

                    gameInfo.Mode = scoreNode.SelectSingleNode("span[1]").InnerText;
                    gameInfo.WinRate = scoreNode.SelectSingleNode("span[2]").InnerText;
                    gameInfo.WinNumber = scoreNode.SelectSingleNode("span[3]").InnerText;
                    gameInfo.LoseNumber = scoreNode.SelectSingleNode("span[4]").InnerText;

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

    }
}
