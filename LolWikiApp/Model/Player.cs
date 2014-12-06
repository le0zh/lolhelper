using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolWikiApp
{
    public class CurrentGameInfo
    {
        #region 摇一摇相关实体
        public CurrentGameInfo()
        {
            Sort100PlayerInfos = new List<PlayerInfo>();
            Sort200PlayerInfos = new List<PlayerInfo>();
        }

        public List<string> Sort100 { get; set; }

        public List<PlayerInfo> Sort100PlayerInfos { get; private set; }

        public List<string> Sort200 { get; set; }

        public List<PlayerInfo> Sort200PlayerInfos { get; private set; }

        public string GameMode { get; set; }

        public string GameType { get; set; }

        public string PN { get; set; }

        public string QueueTypeCn { get; set; }

        public string QueueTypeName { get; set; }

        public string SN { get; set; }
    }

    public class PlayerInfo
    {
        public string Name { get; set; }

        public string TierDesc { get; set; }

        public string Total { get; set; }

        public string WinRate { get; set; }

        public string ZDL { get; set; }

        public string HeroName { get; set; }

        public string HeroImageUrl
        {
            get
            {
                return string.Format("/Data/Images/{0}/{1}.png", this.HeroName, this.HeroName);
            }
        }
    }
    #endregion


    /// <summary>
    /// 比赛信息
    /// </summary>
    public class GameInfo
    {
        /// <summary>
        /// 比赛详细信息网址
        /// </summary>
        public string GameDetailUrl { get; set; }

        /// <summary>
        /// 比赛中使用英雄头像地址
        /// </summary>
        public string HeroImageUrl { get; set; }

        /// <summary>
        /// 比赛类型
        /// </summary>
        public string GameMode { get; set; }

        /// <summary>
        /// 比赛结果
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 比赛日期
        /// </summary>
        public string Date { get; set; }
    }

    /// <summary>
    /// 匹配模式战绩信息
    /// </summary>
    public class MatchGameInfo
    {
        /// <summary>
        /// 模式
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// 胜率
        /// </summary>
        public string WinRate { get; set; }

        /// <summary>
        /// 胜场
        /// </summary>
        public string WinNumber { get; set; }

        /// <summary>
        /// 负场
        /// </summary>
        public string LoseNumber { get; set; }
    }

    /// <summary>
    /// 战斗力具体信息
    /// </summary>
    public class PowerDetailInfo
    {
        public string TotaScore { get; set; }

        public string BaseScore { get; set; }

        public string WinRateScore { get; set; }

        public string WinNumberScore { get; set; }
    }

    /// <summary>
    /// 排位模式战绩信息
    /// </summary>
    public class RankGameInfo
    {
        /// <summary>
        /// 比赛类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 胜率
        /// </summary>
        public string WinRate { get; set; }

        /// <summary>
        /// 胜场
        /// </summary>
        public string WinNumber { get; set; }

        /// <summary>
        /// 段位/胜点
        /// </summary>
        public string RangeAndWinPoint { get; set; }
    }

    public class Player
    {
        /// <summary>
        /// 表示是否将数据加载完毕，如果是从配置中读取的，这个值为False需要再次加载数据
        /// </summary>
        public bool IsDataLoaded { get; set; }

        public bool IsBinded
        {
            get
            {
                if (string.IsNullOrEmpty(Name) || ServerInfo == null || string.IsNullOrEmpty(ServerInfo.DisplayName))
                    return false;

                return App.ViewModel.BindedPlayerInfoWrappers.Any(
                                    p =>
                                        p.Name == this.Name &&
                                        p.ServerInfo.DisplayName == this.ServerInfo.DisplayName);
            }
        }

        public string Name { get; set; }

        public ServerInfo ServerInfo { get; set; }

        public string PhotoUrl { get; set; }

        /// <summary>
        /// 战斗力
        /// </summary>
        public int Power { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 被赞
        /// </summary>
        public int Good { get; set; }

        /// <summary>
        /// 被拉黑
        /// </summary>
        public int Bad { get; set; }

        private RankGameInfo rankGmeInfo;

        public RankGameInfo RankGmeInfo
        {
            get
            {
                return rankGmeInfo ?? (rankGmeInfo = new RankGameInfo()
                {
                    Type = "无",
                    RangeAndWinPoint = "无/无",
                    WinNumber = "无",
                    WinRate = "无"
                });
            }
            set { rankGmeInfo = value; }
        }

        public string Range
        {
            get
            {
                var range = RankGmeInfo.RangeAndWinPoint.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)[0];
                return range;
            }
        }

        public int TotalGamesNumber
        {
            get
            {
                var number = 0;
                foreach (var gameInfo in MatchGameInfos)
                {
                    number += ConvertoInt(gameInfo.WinNumber);
                    number += ConvertoInt(gameInfo.LoseNumber);
                }
                return number;
            }
        }

        private int ConvertoInt(string strInt)
        {
            var result = 0;
            try
            {
                result = int.Parse(strInt);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return result;
        }

        public PowerDetailInfo PowerDetailInfo { get; set; }

        public List<MatchGameInfo> MatchGameInfos { get; private set; }

        public List<string> RecentUsedHeroImageList { get; private set; }

        public List<GameInfo> RecentGameInfoList { get; private set; }

        public Player()
        {
            RecentGameInfoList = new List<GameInfo>();
            RecentUsedHeroImageList = new List<string>();
            MatchGameInfos = new List<MatchGameInfo>();
        }
    }

    public class PlayerSummary
    {
        public string PhotoUrl { get; set; }
        public string Level { get; set; }
        public string Name { get; set; }
        public string Range { get; set; }
        public string ServerName { get; set; }
        public string Power { get; set; }
        public string TotalGamesNumber { get; set; }
        public string RankGameWinNumber { get; set; }
    }

    public class PlayerInfoSettingWrapper
    {
        public bool IsDataLoaded { get; set; }
        public string Name { get; set; }

        public ServerInfo ServerInfo { get; set; }
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
