using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataCollector.Helpers
{
    public static class Extensions
    {
        public static void SetProperty(this object obj, string propertyName, object value)
        {
            var propertyInfo = obj.GetType().GetProperty(propertyName);
            if (propertyInfo == null) return;
            propertyInfo.SetValue(obj, value);
        }
    }
}