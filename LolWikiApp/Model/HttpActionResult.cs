using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolWikiApp
{
    public enum ActionResult
    {
        Success,
        NotFound,
        Exception404
    }

    public class HttpActionResult
    {
        public object Value { get; set; }

        public ActionResult Result { get; set; }
    }
}
