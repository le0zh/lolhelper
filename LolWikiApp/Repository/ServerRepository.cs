using System.Collections.Generic;
using System.Linq;

namespace LolWikiApp.Repository
{
    public class ServerRepository
    {
        private ServerRepository() { }

        private List<ServerInfo> serverInfos;
        public static readonly ServerRepository instance = new ServerRepository();

        public string GetServerDisplayName(string value)
        {
            GetServerInfos();            
            foreach (ServerInfo serverInfo in serverInfos)
            {
                if (serverInfo.Value == value)
                    return serverInfo.DisplayName;
            }
            return value;
        }

        public List<ServerInfo> GetServerInfos()
        {
            if (serverInfos == null || serverInfos.Count == 0)
            {
                serverInfos = new List<ServerInfo>();

                serverInfos.Add(new ServerInfo() {Value = "电信一", DisplayName = "艾欧尼亚"});
                serverInfos.Add(new ServerInfo() {Value = "电信二", DisplayName = "祖安"});
                serverInfos.Add(new ServerInfo() {Value = "电信三", DisplayName = "诺克萨斯"});
                serverInfos.Add(new ServerInfo() {Value = "电信四", DisplayName = "班德尔城"});
                serverInfos.Add(new ServerInfo() {Value = "电信五", DisplayName = "皮尔特沃夫"});
                serverInfos.Add(new ServerInfo() {Value = "电信六", DisplayName = "战争学院"});
                serverInfos.Add(new ServerInfo() {Value = "电信七", DisplayName = "巨神峰"});
                serverInfos.Add(new ServerInfo() {Value = "电信八", DisplayName = "雷瑟守备"});
                serverInfos.Add(new ServerInfo() {Value = "电信九", DisplayName = "裁决之地"});
                serverInfos.Add(new ServerInfo() {Value = "电信十", DisplayName = "黑色玫瑰"});
                serverInfos.Add(new ServerInfo() {Value = "电信十一", DisplayName = "暗影岛"});
                serverInfos.Add(new ServerInfo() {Value = "电信十二", DisplayName = "钢铁烈阳"});
                serverInfos.Add(new ServerInfo() {Value = "电信十三", DisplayName = "均衡教派"});
                serverInfos.Add(new ServerInfo() {Value = "电信十四", DisplayName = "水晶之痕"});
                serverInfos.Add(new ServerInfo() {Value = "电信十五", DisplayName = "影流"});
                serverInfos.Add(new ServerInfo() {Value = "电信十六", DisplayName = "守望之海"});
                serverInfos.Add(new ServerInfo() {Value = "电信十七", DisplayName = "征服之海"});
                serverInfos.Add(new ServerInfo() {Value = "电信十八", DisplayName = "卡拉曼达"});
                serverInfos.Add(new ServerInfo() {Value = "电信十九", DisplayName = "皮城警备"});
                serverInfos.Add(new ServerInfo() {Value = "网通一", DisplayName = "比尔吉沃特"});
                serverInfos.Add(new ServerInfo() {Value = "网通二", DisplayName = "德玛西亚"});
                serverInfos.Add(new ServerInfo() {Value = "网通三", DisplayName = "弗雷尔卓德"});
                serverInfos.Add(new ServerInfo() {Value = "网通四", DisplayName = "无畏先锋"});
                serverInfos.Add(new ServerInfo() { Value = "网通五", DisplayName = "恕瑞玛" });
                serverInfos.Add(new ServerInfo() { Value = "网通六", DisplayName = "扭曲丛林" });
                serverInfos.Add(new ServerInfo() {Value = "教育一", DisplayName = "教育网专区"});
            }

            return serverInfos;
        }
    }
}