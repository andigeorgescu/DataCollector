using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;
using DataCollector.Data;
using DataCollector.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataCollector.Controllers
{
    [RoutePrefix("api/wiki")]
    [EnableCors(origins: "http://localhost:63119", headers: "*", methods: "*")]
    public class WikiController : ApiController
    {
        private readonly AppDbContext _db;

        public WikiController()
        {
            _db = new AppDbContext();    
        }

        [Route("ScrapePage")]
        [HttpPost]
        public HttpResponseMessage ScrapePage(WebPageModel model)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                //var url = "https://ro.wikipedia.org/wiki/Lista_ora%C8%99elor_din_Rom%C3%A2nia";
                var file = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "RawData\\text.txt");

                var htmlWeb = new HtmlWeb()
                {
                    AutoDetectEncoding = true,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36"
                };
                var htmlDocument = htmlWeb.Load(model.Url);

                var dataTable = GetWikiTable(htmlDocument);
                if (null == dataTable) return new HttpResponseMessage(HttpStatusCode.NotFound);

                var header = GetTableHeaders(dataTable);
                var contentList = GetTableContent(dataTable);
                file.Write(new JavaScriptSerializer().Serialize(ParseToJson(header, contentList)));
                file.Close();

                var rowGroupData = ParseToJson(header, contentList)
                    .GroupBy(g => g.Row, (key, value) => new {key, value});
                
                var list = new JavaScriptSerializer().Serialize(rowGroupData);
                var dataFormatted = JToken.Parse(list).ToString(Formatting.Indented);

                _db.Data.Add(new DataEntity()
                {
                    CreatedOn = DateTime.Now,
                    IdCollectionType = (int) CollectionTypeEnum.Wiki,
                    JsonObject = dataFormatted
                });
                _db.SaveChanges();

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                System.Diagnostics.Debug.WriteLine("Timp wiki scraper: " + elapsedMs);
                return Request.CreateErrorResponse(HttpStatusCode.OK, dataFormatted);
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

        public IList<DataItem> ParseToJson(IList<string> header, IList<KeyValuePair<int, string>> content)
        {
            var items = new List<DataItem>();

            for (var c = 0; c < content.Count; c++)
            {
                items.Add(new DataItem()
                {
                    Key = header[c % header.Count],
                    Value = content[c].Value,
                    Row = content[c].Key
                });
            }

            return items;
        }
    }
}