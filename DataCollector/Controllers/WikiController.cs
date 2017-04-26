using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using DataCollector.Helpers;
using DataCollector.Models;
using HtmlAgilityPack;

namespace DataCollector.Controllers
{
    [RoutePrefix("api/wiki")]
    public class WikiController : ApiController
    {
        [Route("ScrapePage")]
        public HttpResponseMessage ScrapePage()
        {
            try
            {
                var url = "https://ro.wikipedia.org/wiki/Lista_ora%C8%99elor_din_Rom%C3%A2nia";
                var file = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "RawData\\text.txt");
                var location = "Bucuresti";
                var parameter = "Locatie";

                var htmlWeb = new HtmlWeb()
                {
                    AutoDetectEncoding = true,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36"
                };
                var htmlDocument = htmlWeb.Load(url);

                var dataTable = GetWikiTable(htmlDocument);
                if (null == dataTable) return new HttpResponseMessage(HttpStatusCode.NotFound);

                var header = GetTableHeaders(dataTable);
                /*       foreach (var h in header)
                       {
                           file.Write(h);
                       }
       */

                var contentList = GetTableContent(dataTable);

                file.Write(ParseToJson(header, contentList));
                /*
                                var rows = from c in contentList
                                           group c by c.Key
                                    into grp
                                           select string.Join(" ", grp.Select(s => s.Value));

                                foreach (var r in rows)
                                {
                                    file.WriteLine(r);
                                }*/

                file.Close();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public HtmlNode GetWikiTable(HtmlDocument doc)
        {
            var tables = doc.DocumentNode.Descendants("table").Where(x => x.Attributes.Contains("class"));
            foreach (var table in tables)
            {
                if (table.Attributes["class"].Value.Contains("wikitable"))
                    return table;
            }

            return null;
        }


        public IList<string> GetTableHeaders(HtmlNode node)
        {
            var headers = node.Descendants("th").Select(s => Helpers.Helpers.CleanData(s.InnerText)).ToList();

            return headers;
        }

        public IList<KeyValuePair<int, string>> GetTableContent(HtmlNode node)
        {
            var content = new List<KeyValuePair<int, string>>();
            var rows = node.Descendants("tr").Where(w => !w.Descendants("th").Any()).ToList();
            for (var i = 0; i < rows.Count; i++)
            {
                var cells = rows[i].Descendants("td").ToList();
                foreach (var cell in cells)
                {
                    content.Add(new KeyValuePair<int, string>(i, cell.ChildNodes.Any() ? cell.LastChild.InnerText ?? "" : ""));
                }
            }

            return content;
        }

        public string ParseToJson(IList<string> header, IList<KeyValuePair<int, string>> content)
        {
            var items = new List<DataItem>();

            for (var c = 0; c < content.Count; c++)
            {
                items.Add(new DataItem()
                {
                    Key = header[c%header.Count],
                    Value = content[c].Value,
                    Row = content[c].Key
                });
            }
            
            return new JavaScriptSerializer().Serialize(items);
        }
    }
}