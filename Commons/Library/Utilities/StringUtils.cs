using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VErp.Commons.Constants;
using VErp.Commons.ObjectExtensions.Extensions;


namespace VErp.Commons.Library
{
    public static class StringUtils
    {
        public static string SubStringMaxLength(this string str, int maxLength, bool byword = false, bool elipsis = false)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            if (str.Length < maxLength) return str;
            str = str.Substring(0, maxLength);
            if (byword)
            {
                var idxOfSpace = str.LastIndexOfAny(new[] { ' ', '\n', '\r', '\t' });
                if (idxOfSpace > 0)
                {
                    str = str.Substring(0, idxOfSpace);
                }
            }
            if (elipsis)
            {
                str += "...";
            }
            return str;
        }


        public static bool StringContains(this object value1, object value2)
        {
            if (value1 == null || value2 == null) return false;
            return value1.ToString().Contains(value2.ToString());
        }

        public static bool StringStartsWith(this object value1, object value2)
        {
            if (value1 == null || value2 == null) return false;
            return value1.ToString().StartsWith(value2.ToString());
        }

        public static bool StringEndsWith(this object value1, object value2)
        {
            if (value1 == null || value2 == null) return false;
            return value1.ToString().EndsWith(value2.ToString());
        }


        public static string NormalizeAsInternalName(this string s)
        {
            return StringExtensions.NormalizeAsInternalName(s);
        }


        public static string RemoveDiacritics(this string str)
        {
            if (str == null) return null;
            var chars =
                from c in str.Normalize(NormalizationForm.FormD).ToCharArray()
                let uc = CharUnicodeInfo.GetUnicodeCategory(c)
                where uc != UnicodeCategory.NonSpacingMark
                select c;

            var cleanStr = new string(chars.ToArray()).Normalize(NormalizationForm.FormC);

            return cleanStr.Replace("đ", "d").Replace("Đ", "D");
        }

        public static string FormatStyle(string template, string code, long? fId, DateTime? dateTime, string number)
        {
            if (string.IsNullOrWhiteSpace(template)) return template;
            var values = new Dictionary<string, object>{
                { StringTemplateConstants.CODE, code },
                { StringTemplateConstants.FID, fId },
            };


            var dateReg = new Regex("\\%DATE\\((?<format>[^\\)]*)\\)\\%");
            foreach (Match m in dateReg.Matches(template))
            {
                if (dateTime.HasValue)
                {
                    values.Add(m.Value, dateTime.Value.ToString(m.Groups["format"].Value));
                }
                else
                {
                    values.Add(m.Value, m.Groups["format"].Value);
                }
            }



            if (!string.IsNullOrWhiteSpace(number))
            {
                values.Add(StringTemplateConstants.SNUMBER, number);
            }
            return FormatStyle(template, values)?.Replace("%", "");
        }

        public static string FormatStyle(string template, IDictionary<string, object> data)
        {
            foreach (var item in data)
            {
                template = template?.Replace(item.Key, item.Value?.ToString());
            }
            return template;
        }



    }
}
