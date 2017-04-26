using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataCollector.Helpers
{
    public class Helpers
    {
        public static string CleanData(string input)
        {
            var output = input.Replace(Environment.NewLine, " ")
                              .Replace("\n", " ");

            return output;
        }
    }
}