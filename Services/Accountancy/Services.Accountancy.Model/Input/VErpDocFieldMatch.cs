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
    public class VErpDocMatch
    {
        public string fullMatch { get; set; }
        public Paragraph paragraph { get; set; }
        public NonCamelCaseDictionary<VErpDocFieldMatch> fieldMatchs { get; set; }

        public VErpDocMatch(Paragraph paragraph, string pattern)
        {
            this.paragraph = paragraph;
            this.fieldMatchs = new NonCamelCaseDictionary<VErpDocFieldMatch>();
            foreach (Match match in Regex.Matches(paragraph.InnerText.Replace("’","'"), pattern))
            {
                fieldMatchs.Add(match.Groups[0].Value,
                    new VErpDocFieldMatch { textExpression = match.Groups[1].Value });
            }
        }
    }
    public class VErpDocFieldMatch
    {
        public string textExpression { get; set; }

        public async Task<object> GetFieldValue(List<NonCamelCaseDictionary> datas, DbContext dBContext)
        {
            if (datas.Count <= 0) return null;

            if (textExpression.StartsWith("="))
            {
                var sqlParam = new SqlParameter("@data", datas.JsonSerialize());
                var tbl = await dBContext.QueryDataTable($"select dbo.{textExpression.Substring(1)}", new[]{ sqlParam });
                return tbl.Rows[0][0];
            }

            return datas[0][textExpression];
        }
    }
}
