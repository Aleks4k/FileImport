using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceFirst(this string text, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(oldValue))
                return text;
            int pos = text.IndexOf(oldValue);
            if (pos < 0)
                return text;
            return text.Substring(0, pos) + newValue + text.Substring(pos + oldValue.Length);
        }
    }
}
