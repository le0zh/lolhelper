using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolWikiApp
{
    public class FreeHeroCache
    {
        

        public DateTime LastUpdated { get; set; }

        public List<Hero> Cache { get; set; } 
    }

    public class Hero
    {
        public Hero()
        {
            Tags = new List<string>();
        }

        public string Id { get; set; }

        public int Key { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public List<string> Tags
        {
            get;
            private set;
        }

        public string ImageUrl
        {
            get { return string.Format("/Data/Images/{0}/{1}.png", this.Id, this.Id); }
        }
    }
}
