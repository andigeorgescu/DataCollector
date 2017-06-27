using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;
using DataCollector.Data;
using DataCollector.Helpers;
using DataCollector.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataCollector.Controllers
{
    [RoutePrefix("api/docs")]
    public class DocsController : ApiController
    {
        private readonly AppDbContext _db;

        public DocsController()
        {
            _db = new AppDbContext();
        }

        [Route("ScrapeDocument")]
        [EnableCors(origins: "http://localhost:63119", headers: "*", methods: "*")]
        public async Task<HttpResponseMessage> ScrapeDocument(DocumentModel model)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                
                var modelToReturn = new ScrapeDocumentResult()
                {
                    NoResultsKeys = new List<string>(),
                    Results = new List<KeyValuePair<string, string>>()
                };

                var httpClient = new HttpClient();
                var data = await httpClient.GetStreamAsync(model.Url);
                var tempFile = Path.GetTempFileName();

                using (var fs = File.OpenWrite(tempFile))
                {
                    data.CopyTo(fs);
                }

                var text = Helpers.Helpers.ExtractTextFromPdf(tempFile);

                GetInfo(text, modelToReturn);

                foreach (var k in model.Keywords)
                {
                    var line = GetTextLine(text, k);
                    if (null == line)
                    {
                        modelToReturn.NoResultsKeys.Add(k);
                        continue;
                    }

                    modelToReturn.Results.Add(new KeyValuePair<string, string>(k, line));
                }

                var list = new JavaScriptSerializer().Serialize(modelToReturn);
                var dataFormatted = JToken.Parse(list).ToString(Formatting.Indented);

                _db.Data.Add(new DataEntity()
                {
                    CreatedOn = DateTime.Now,
                    IdCollectionType = (int)CollectionTypeEnum.Pdf,
                    JsonObject = dataFormatted
                });
                _db.SaveChanges();

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                System.Diagnostics.Debug.WriteLine("Timp document scraper: " + elapsedMs);
                return Request.CreateResponse(HttpStatusCode.OK, modelToReturn);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        [Route("Crawl")]
        [EnableCors(origins: "http://localhost:63119", headers: "*", methods: "*")]
        public HttpResponseMessage Crawl(WebPageModel model)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();


                var result = new CrawlerResult()
                {
                    IsMatch = false,
                    Urls = new List<KeyValuePair<string,string>>()
                };

                var htmlWeb = new HtmlWeb()
                {
                    AutoDetectEncoding = true,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36"
                };
                var htmlDocument = htmlWeb.Load(model.Url);
                var dataLinks =
                    htmlDocument.DocumentNode.SelectNodes("//a[@href]")
                        .Where(w => LevenshteinDistance(w.InnerText, model.Location) < 2).ToList();

                if (!dataLinks.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }

                var aElement = dataLinks.FirstOrDefault(w => w.InnerText == model.Location) ?? dataLinks.First();

                var hrefValue = aElement.Attributes.Single(w => w.Name == "href").Value;
                var parsedUrl = new Uri(model.Url);

                var link = BuildUrl(parsedUrl, hrefValue);
                var results = SeekPath(link);
                if (!results.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }

                var matchUrl = MatchKey(results, model.Key);
                if (null == matchUrl)
                {
                    foreach (var r in results)
                    {
                        result.Urls.Add(new KeyValuePair<string, string>(r,GetLatestDataUrl(BuildUrl(parsedUrl, r))));
                    }
                }
                else
                {
                    result.IsMatch = true;
                    result.Urls.Add(new KeyValuePair<string, string>(null, GetLatestDataUrl(BuildUrl(parsedUrl, matchUrl))));
                }

                var list = new JavaScriptSerializer().Serialize(result);
                var dataFormatted = JToken.Parse(list).ToString(Formatting.Indented);

                _db.Data.Add(new DataEntity()
                {
                    CreatedOn = DateTime.Now,
                    IdCollectionType = (int)CollectionTypeEnum.Nav,
                    JsonObject = dataFormatted
                });
                _db.SaveChanges();

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                System.Diagnostics.Debug.WriteLine("Timp crawler: " + elapsedMs);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public static string BuildUrl(Uri host, string path)
        {
            return host.GetLeftPart(UriPartial.Authority) + '/' + path;
        }

        public static IList<string> SeekPath(string url)
        {
            var urlsToReturn = new List<string>();
            var parsedLink = new Uri(url);
            var htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = true,
                UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36"
            };
            var htmlDocument = htmlWeb.Load(url);

            var dataLinks =
                    htmlDocument.DocumentNode.SelectNodes("//a[@href]")
                        .Where(w => w.Attributes.Single(s => s.Name == "href").Value.Contains(parsedLink.AbsolutePath.TrimStart('/'))).ToList();

            //Data cleaning 
            foreach (var l in dataLinks)
            {
                var hrefValue = l.Attributes.Single(w => w.Name == "href").Value;
                if (urlsToReturn.Any(a => a.Equals(hrefValue)) || hrefValue.Equals(parsedLink.AbsolutePath.TrimStart('/'))) continue;

                urlsToReturn.Add(hrefValue);
            }

            return urlsToReturn;
        }

        public static string MatchKey(IList<string> results, string key)
        {
            if (key == null) return null; 

            foreach (var r in results)
            {
                if (r.ToLower().Contains(key.ToLower())) return r;
            }

            return null;
        }

        private static void GetInfo(string text, ScrapeDocumentResult model)
        {
            var regex = new Regex(@"[01]?\d[/-][0123]?\d[/-]\d{2}");
            var dates = regex.Matches(text);
            var dateList = new List<DateTime>();

            foreach (var d in dates)
            {
                var parsedDate = DateTime.ParseExact(d.ToString(), "dd-MM-yy", CultureInfo.InvariantCulture);
                if (!dateList.Any(a => a.Equals(parsedDate)))
                    dateList.Add(parsedDate);
            }

            model.Dates = dateList.OrderBy(o => o.Date).ToList();
        }

        private static string GetLatestDataUrl(string url)
        {
            var parsedUrl = new Uri(url);
            var regex = new Regex(@"[0-9]{2}.[0-9]{2}.[0-9]{4}");
            string urlToReturn = null;

            var htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = true,
                UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36"
            };
            var htmlDocument = htmlWeb.Load(url);
            var dataLinks =
                    htmlDocument.DocumentNode.SelectNodes("//a[@href]")
                        .Where(w => regex.IsMatch(w.InnerText)).ToList();

            var currentDate = DateTime.Now.Ticks;
            var latestDate = DateTime.ParseExact(dataLinks[0].InnerText, "dd.MM.yyyy", CultureInfo.InvariantCulture).Ticks;
            urlToReturn = dataLinks[0].Attributes.Single(w => w.Name == "href").Value;

            foreach (var d in dataLinks)
            {
                var datetimeFormat = DateTime.ParseExact(d.InnerText, "dd.MM.yyyy", CultureInfo.InvariantCulture).Ticks;

                if (currentDate - datetimeFormat < (currentDate - latestDate))
                {
                    urlToReturn = d.Attributes.Single(w => w.Name == "href").Value;
                    latestDate = datetimeFormat;
                }
            }

            return BuildUrl(parsedUrl, urlToReturn);
        }

        private static string GetTextLine(string text, string keyword)
        {
            var words = text.GetWords();
            var target = words.FirstOrDefault(w => LevenshteinDistance(keyword, w) < 2); // eroare admisa

            if (null == target) return null;

            var indexKey = text.IndexOf(target, StringComparison.OrdinalIgnoreCase);
            if (indexKey < 0) return null;

            var nextBreak = text.IndexOf(Environment.NewLine, indexKey, StringComparison.OrdinalIgnoreCase);
            var extractedLine = text.Substring(indexKey, nextBreak - indexKey);

            if (extractedLine.Length > 20) return extractedLine;
            nextBreak = text.IndexOf(Environment.NewLine, nextBreak, StringComparison.OrdinalIgnoreCase);

            return text.Substring(indexKey, nextBreak - indexKey);
        }

        private static int LevenshteinDistance(string key, string target)
        {
            if (key.ToLower() == target.ToLower()) return 0;
            if (key.Length == 0) return target.Length;
            if (target.Length == 0) return key.Length;

            var v0 = new int[target.Length + 1];
            var v1 = new int[target.Length + 1];

            for (int i = 0; i < v0.Length; i++)
            {
                v0[i] = i;
            }

            for (int i = 0; i < key.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < target.Length; j++)
                {
                    var cost = (char.ToLower(key[i]) == char.ToLower(target[j])) ? 0 : 1;
                    v1[j + 1] = Math.Min(v1[j] + 1, Math.Min(v0[j + 1] + 1, v0[j] + cost));
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[target.Length];
        }
    }
}