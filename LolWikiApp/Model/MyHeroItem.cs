using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LolWikiApp
{
    public class MyHeroItem
    {
        [JsonProperty("enName")]
        public string EnName { get; set; }

        [JsonProperty("cnName")]
        public string CnName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("presentTimes")]
        public string StrPresentTimes { get; set; }

        public int PresentTimes
        {
            get
            {
                int times;
                if (int.TryParse(StrPresentTimes, out times))
                {
                    return times;
                }

                return 0;
            }
        }

        public string ImageUrl
        {
            get { return string.Format("/Data/Images/{0}/{1}.png", this.EnName, this.EnName); }
        }
    }
}
