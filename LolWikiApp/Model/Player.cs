using System;
using System.Collections.Generic;
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
                    RangeAndWinPoint = "无",
                    WinNumber = "无",
                    WinRate = "无"
                });
            }
            set { rankGmeInfo = value; }
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

    public class PlayerInfoSettingWrapper
    {
        public string Name { get; set; }

        public ServerInfo ServerInfo { get; set; }
    }
}
