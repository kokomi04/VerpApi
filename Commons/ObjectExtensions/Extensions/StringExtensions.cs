using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace VErp.Commons.ObjectExtensions.Extensions
{
    public static class StringExtensions
    {
        public static string Format(this string str,params object[] args)
        {
            return string.Format(str, args);
        }

        public static string NormalizeAsInternalName(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            s = s.ConvertToUnSign2();
            s = s.ToLower().Trim();
            return Regex.Replace(s, "[^a-zA-Z0-9\\.\\-]", "");
        }

        public static string ConvertToUnSign2(this string s)
        {
            string stFormD = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            return (sb.ToString().Normalize(NormalizationForm.FormD));
        }
    }
}
