using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Services.Accountancy.Model.Input
{

    public class VErpDocTableMatch
    {
        private readonly string pattern = @"#table{([\w|,|$|\[\]|\{|\}]+)}";
        public string fullMatch { get; set; }
        public Paragraph paragraph { get; set; }
        public NonCamelCaseDictionary<VErpDocFieldMatch> fieldMatchs { get; set; }

        public VErpDocTableMatch(Paragraph paragraph)
        {
            this.paragraph = paragraph;
            this.fieldMatchs = new NonCamelCaseDictionary<VErpDocFieldMatch>();
            foreach (Match match in Regex.Matches(paragraph.InnerText, pattern))
            {
                fieldMatchs.Add(match.Groups[0].Value,
                    new VErpDocFieldMatch { textExpression = match.Groups[1].Value });
            }
        }
    }
    public class VErpDocMatch
    {
        private readonly string pattern = @"#{([\w|,|$|\[\]|\{|\}]+)}";
        public string fullMatch { get; set; }
        public Paragraph paragraph { get; set; }
        public NonCamelCaseDictionary<VErpDocFieldMatch> fieldMatchs { get; set; }

        public VErpDocMatch(Paragraph paragraph)
        {
            this.paragraph = paragraph;
            this.fieldMatchs = new NonCamelCaseDictionary<VErpDocFieldMatch>();
            foreach (Match match in Regex.Matches(paragraph.InnerText, pattern))
            {
                fieldMatchs.Add(match.Groups[0].Value,
                    new VErpDocFieldMatch { textExpression = match.Groups[1].Value });
            }
        }
    }
    public class VErpDocFieldMatch
    {
        private readonly string pattern = @"(\w+)\[(\d*)\]";
        public string textExpression { get; set; }

        public async Task<object> GetFieldValue(List<NonCamelCaseDictionary> datas, DbContext dBContext)
        {
            var regex = new Regex(pattern);
            if (textExpression.StartsWith("[") && textExpression.EndsWith("]"))
            {
                var funcMatch = new VErpDocFuncMatch { textExpression = textExpression };
                return await funcMatch.GetFuncValue(datas, dBContext);
            }
            else
            if (regex.IsMatch(textExpression))
            {
                var m = regex.Match(textExpression);
                object result;
                string fieldName = m.Groups[1].Value;
                if (string.IsNullOrEmpty(m.Groups[2].Value))
                {
                    return datas.Select(x => x[fieldName]).ToArray();
                }
                else
                {
                    int index;
                    int.TryParse(m.Groups[2].Value, out index);
                    datas[index].TryGetValue(fieldName, out result);
                }


                return result;
            }

            return null;
        }
    }

    public class VErpDocFuncMatch
    {
        private readonly string pattern = @"\[(\w+){([\w|\[|\]|\{|\}|$|,]+)}\]";
        public string textExpression { get; set; }

        public async Task<object> GetFuncValue(List<NonCamelCaseDictionary> datas, DbContext dBContext)
        {
            var regex = new Regex(pattern);
            if (regex.IsMatch(textExpression))
            {
                var m = regex.Match(textExpression);
                NonCamelCaseDictionary paramsData = new NonCamelCaseDictionary();

                string functionName = m.Groups[1].Value;

                var @params = ParameterDecomposition(m.Groups[2].Value);
                foreach (var p in @params)
                {
                    int i = p.IndexOf("$");

                    var varName = p.Substring(0, i);
                    var field = new VErpDocFieldMatch { textExpression = p.Substring(i + 1) };

                    paramsData.Add(varName, await field.GetFieldValue(datas, dBContext));
                }

                var sqlParams = paramsData.Select(p => new SqlParameter("@" + p.Key, $"{{\"{p.Key}\":{p.Value.JsonSerialize()}}}")).ToArray();
                var tbl = await dBContext.QueryFunction(functionName, sqlParams);
                
                return tbl.Rows[0][0];
            }
            return null;
        }

        private List<string> ParameterDecomposition(string str)
        {
            List<string> ls = new List<string>();

            while (!string.IsNullOrEmpty(str))
            {
                int lent = str.Length;
                var a = str.IndexOf("}]");
                var b = str.IndexOf(",");
                var c = str.IndexOf("$[");
                if (a < 0)
                {
                    ls.AddRange(str.Split(","));
                    str = "";
                }
                else if (b < 0)
                {
                    ls.Add(str);
                    str = "";
                }
                else if (a > b && b < c)
                {
                    ls.Add(str.Substring(0, b));
                    ls.Add(str.Substring(b + 1, a - b + 1));
                    if (a + 3 > lent) str = "";
                    else
                        str = str.Substring(a + 3);
                }
                else
                {
                    ls.Add(str.Substring(0, a + 2));
                    if (a + 3 > lent) str = "";
                    else
                        str = str.Substring(a + 3);
                }
            }
            return ls;
        }
    }
}
