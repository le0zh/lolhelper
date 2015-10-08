using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;

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
        //"vid": "118308",
        //"udb": "18604093877yy",
        //"letv_video_id": "12092776",
        //"cover_url": "http:\/\/vimg.dwstatic.com\/1520\/118281\/4-220x124.jpg",
        //"title": "\u6bcf\u65e5\u7cbe\u5f69\u96c6\u9526\uff1a\u8f6e\u5b50\u5988\u5854\u4e0b\u667a\u6597\u75af\u72d7\u5200\u59b9",
        //"channelId": "lolboxvideo",
        //"video_length": 337,
        //"letv_video_unique": "c3c82bfd45",
        //"upload_time": "2015-05-13 11:49:38",
        //"totalPage": 56

        [JsonProperty("vid")]
        public string Video_Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("cover_url")]
        public string Cover_Url { get; set; }

        public string Introduction { get; set; }

        public string Notes { get; set; }

        [JsonProperty("video_length")]
        public string Video_Length { get; set; }

        public string VideoLengthDisplay
        {
            get
            {
                var display = "";
                if (!string.IsNullOrEmpty(Video_Length))
                {
                    var length = int.Parse(Video_Length);
                    //Debug.WriteLine("length:{0}",length);
                    display = TimeSpan.FromSeconds(length).ToString("g");
                }
                return display;
            }
        }

        [JsonProperty("upload_time")]
        public string Upload_Time { get; set; }


        public string ChannelId { get; set; }

        [JsonProperty("udb")]
        public string Udp { get; set; }

        public string EditorId { get; set; }

        public string Amount_Play { get; set; }

        public string Letv_Video_Id { get; set; }

        [JsonProperty("letv_video_unique")]
        public string Letv_Video_Unique { get; set; }

        [JsonProperty("totalPage")]
        public int TotalPage { get; set; }
    }
    #endregion

    public class CachedVideoInfo
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Length { get; set; }
        public string Src { get; set; }
        public long TotalSize { get; set; }
    }
}
