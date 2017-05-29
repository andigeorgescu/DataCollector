using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DataCollector.Models;

namespace DataCollector.Controllers
{
    [RoutePrefix("api/docs")]
    public class DocsController : ApiController
    {
        [Route("ScrapeDocument")]
        public async Task<HttpResponseMessage> ScrapeDocument(DocumentModel model)
        {
            try
            {
                model = new DocumentModel
                {
                    Url = "http://www.apanovabucuresti.ro/!res/fls/41(13).pdf",
                    Keywords =
                        new List<string>()
                        {
                            "Turbiditate",
                            "Clor",
                            "Amoniu",
                            "Nitriti",
                            "Nitrati",
                            "Fier",
                            "Aluminiu",
                            "Bacterii"
                        }
                };
                var modelToReturn = new ScrapeDocumentResult();
                var httpClient = new HttpClient();
                var data = await httpClient.GetStreamAsync(model.Url);
                var tempFile = Path.GetTempFileName();

                using (var fs = File.OpenWrite(tempFile))
                {
                    data.CopyTo(fs);
                }

                var text = Helpers.Helpers.ExtractTextFromPdf(tempFile);

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

                return Request.CreateResponse(HttpStatusCode.OK, modelToReturn);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        private string GetTextLine(string text, string keyword)
        {
            var indexKey = text.IndexOf(keyword, StringComparison.Ordinal);
            if (indexKey < 0) return null;

            var nextBreak = text.IndexOf(Environment.NewLine, indexKey, StringComparison.Ordinal);
            var extractedLine = text.Substring(indexKey, nextBreak - indexKey);

            if (extractedLine.Length > 20) return extractedLine;
            nextBreak = text.IndexOf(Environment.NewLine, nextBreak, StringComparison.Ordinal);

            return text.Substring(indexKey, nextBreak - indexKey);
        }
    }
}