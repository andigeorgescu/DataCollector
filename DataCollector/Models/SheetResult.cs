using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataCollector.Models
{
    public class SheetResult
    {
        public DateTime? DateTime { get; set; }
        public double? Value { get; set; }
        public string Unit { get; set; }
        public bool PredictedValue { get; set; }
    }
}