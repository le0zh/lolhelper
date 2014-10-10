using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Tasks;

namespace LolWikiApp
{
    #region m3u8 style
    public class VideoListInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Youku_id { get; set; }

        public string Img { get; set; }

        public string Video_addr_super { get; set; }

        public string Video_addr_high { get; set; }

        public string Video_addr { get; set; }

        public string Length { get; set; }

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

    #region letv style
    public class LetvVideoListInfo
    {
        // "video_id": "141143769255511006",
        //"title": "Miss排位日记：教你做一个会飞的翔人！",
        //"cover_url": "http://s1.dwstatic.com/video/201409/23/7635630/4-120_90.jpg",
        //"introduction": "",
        //"notes": "",
        //"video_length": "2572",
        //"upload_time": "2014-09-23 10:01:32",
        //"channelId": "lolboxvideo",
        //"udb": "miss_game",
        //"editorId": "",
        //"amount_play": null,
        //"letv_video_id": "7635630",
        //"letv_video_unique": "413cf3d444",
        //"totalPage": 10

        public string Video_Id { get; set; }
        public string Title { get; set; }
        public string Cover_Url { get; set; }
        public string Introduction { get; set; }
        public string Notes { get; set; }
        public string Video_Length { get; set; }

        public string VideoLengthDisplay
        {
            get
            {
                var display = "";
                if (!string.IsNullOrEmpty(Video_Length))
                {
                    var length = int.Parse(Video_Length);
                    Debug.WriteLine("length:{0}",length);
                    display = TimeSpan.FromSeconds(length).ToString("g");
                }
                return display;
            }
        }

        public string Upload_Time { get; set; }
        public string ChannelId { get; set; }
        public string Udp { get; set; }
        public string EditorId { get; set; }
        public string Amount_Play { get; set; }
        public string Letv_Video_Id { get; set; }
        public string Letv_Video_Unique { get; set; }
        public int TotalPage { get; set; }
    }
    #endregion
}
