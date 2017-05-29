using System;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace DataCollector.Helpers
{
    public static class Helpers
    {
        public static string CleanData(string input)
        {
            var output = input.Replace(Environment.NewLine, " ")
                              .Replace("\n", " ");

            return output;
        }

        public static string ExtractTextFromPdf(string path)
        {
            ITextExtractionStrategy its = new LocationTextExtractionStrategy();

            using (PdfReader reader = new PdfReader(path))
            {
                StringBuilder text = new StringBuilder();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    string thePage = PdfTextExtractor.GetTextFromPage(reader, i, its);
                    string[] theLines = thePage.Split('\n');
                    foreach (var theLine in theLines)
                    {
                        text.AppendLine(theLine);
                    }
                }
                return text.ToString();
            }
        }
    }
}