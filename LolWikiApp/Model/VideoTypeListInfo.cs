using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolWikiApp
{
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
}
