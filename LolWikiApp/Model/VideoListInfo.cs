using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Tasks;

namespace LolWikiApp
{
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
}
