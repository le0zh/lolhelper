using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LolWikiApp.Repository
{
    public class Repository
    {
        protected async Task<String> GetJsonStringViaHTTPAsync(string url)
        {
            HttpClient client = new HttpClient();
            string json = await client.GetStringAsync(new Uri(url));
            return json;
        }
    }
}
