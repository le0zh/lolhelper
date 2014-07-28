using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolWikiApp
{
    public  static class StringX
    {
        public static string HtmlDecode(this string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            content = content.Replace("&ldquo;", "“");
            content = content.Replace("&rdquo;", "”");

            content = content.Replace("&lsquo;", "‘");
            content = content.Replace("&rsquo;", "’");

            content = content.Replace("&bull;", "•");

            content = content.Replace("&hellip;", "…");

            content = content.Replace("&gt;", ">");
            content = content.Replace("&lt;", "<");

            content = content.Replace("&lsaquo;", "<");
            content = content.Replace("&rsaquo;", ">");

            content = content.Replace("&tilde;", "˜");

            content = content.Replace("&nbsp;", " ");
            content = content.Replace("&mdash;", "——");
            content = content.Replace("&ndash;", "-");

            content = content.Replace("&rarr;", "→");
            content = content.Replace("&larr;", "←");

            content = content.Replace("&quot;", "\"");

            return content;
        }
    }
}
