using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataCollector.Models
{
    public class CrawlerModel
    {
        public IList<DocumentModel> Documents { get; set; }
        public string Location { get; set; }
    }
}