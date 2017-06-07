using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Linear;
using DataCollector.Models;

namespace DataCollector.Controllers
{
    [RoutePrefix("api/sheet")]
    public class SheetController : ApiController
    {
        [Route("ScrapeSheet")]
        [EnableCors(origins: "http://localhost:63119", headers: "*", methods: "*")]
        public HttpResponseMessage ScrapeSheet(SheetDocumentModel model)
        {
            try
            {
                var filePath = AppDomain.CurrentDomain.BaseDirectory + "//RawData//" + model.FileName;
                File.WriteAllBytes(filePath, Convert.FromBase64String(model.FileData));

                var connectionString =
                    $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties=Excel 12.0;";

                var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
                var ds = new DataSet();

                adapter.Fill(ds, "DataTable");

                var data = ds.Tables["DataTable"].AsEnumerable();

                var scrapedResult = data.Where(w => w.Field<DateTime?>("DateTime") != null).Select(x =>
                new SheetResult()
                {
                    DateTime = x.Field<DateTime?>("DateTime"),
                    Value = x.Field<double?>("Value"),
                    Unit = x.Field<string>("Unit"),
                }).ToList();

                var regressionInput =
                  scrapedResult.Where(w => w.Value != null).Select(s => Convert.ToDouble(s.DateTime?.Hour)).ToArray();
                var regressionOutput =
                    scrapedResult.Where(w => w.Value != null).Select(s => Convert.ToDouble(s.Value.Value)).ToArray();

                // Use Ordinary Least Squares to learn the regression
                OrdinaryLeastSquares ols = new OrdinaryLeastSquares();

                // Use OLS to learn the simple linear regression
                SimpleLinearRegression regression = ols.Learn(regressionInput, regressionOutput);

                // Compute the output for a given input:

                foreach (var v in scrapedResult)
                {
                    if (v.Value != null) continue;

                    v.Value = regression.Transform(Convert.ToDouble(v.DateTime?.Hour));
                    v.PredictedValue = true;

                    regressionInput =
                        scrapedResult.Where(w => w.Value != null).Select(s => Convert.ToDouble(s.DateTime?.Hour)).ToArray();
                    regressionOutput =
                        scrapedResult.Where(w => w.Value != null).Select(s => Convert.ToDouble(s.Value.Value)).ToArray();
                    regression = ols.Learn(regressionInput, regressionOutput);
                }

                return Request.CreateResponse(HttpStatusCode.OK, scrapedResult);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
