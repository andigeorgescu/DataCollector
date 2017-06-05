using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataCollector.Models
{
    public class CrawlerResult
    {
        public IList<KeyValuePair<string,string>> Urls { get; set; }
        public bool IsMatch { get; set; }
    }   
}