using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolWikiApp
{
    #region m3u8 style
    public enum VideoType
    {
        Series = 1,
        Game,
        Talker
    }

    public class VideoTypeListInfo
    {
        public int Count { get; set; }

        public string Id { get; set; }

        public string Img { get; set; }

        public string Name { get; set; }

        public int Time { get; set; }

        public DateTime Date
        {
            get
            {
                var dtBase = new DateTime(1970, 1, 1);
                var dt = dtBase + TimeSpan.FromSeconds(Time);
                return dt;
            }
        }
    }
    #endregion

    public class LetvVideoTypeListInfo
    {
        //"group": "gaoxiao",
        //"name": "搞笑视频",
        //"subCategory": [
        public string Group { get; set; }
        public string Name { get; set; }
        public List<LetvVideoSubcategory> SubCategory { get; set; }
    }

    public class LetvVideoSubcategory
    {
        //"tag": "missjs",
        //"name": "Miss解说",
        //"icon": "http://box.dwstatic.com/vicon/missjs.jpg",
        //"dailyUpdate": "0"

        public string Tag { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public int DailyUpdate { get; set; }
    }
}
