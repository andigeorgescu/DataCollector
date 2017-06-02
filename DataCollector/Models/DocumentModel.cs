using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataCollector.Models
{
    public class DocumentModel
    {
        public string Url { get; set; }
        public IList<string> Keywords { get; set; }
    }

    public class ScrapeDocumentResult
    {
        public IList<DateTime> Dates { get; set; }
        public string Location { get; set; }
        public IList<KeyValuePair<string,string>> Results { get; set; }
        public IList<string> NoResultsKeys { get; set; }
    }
}