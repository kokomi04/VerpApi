using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.Products;
using VErp.Services.Stock.Service.Stock;
using Xunit;
using static VErp.Commons.GlobalObject.InternalDataInterface.ProductModel;
using VErp.Infrastructure.EF.EFExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using System.Text;

namespace MasterTests
{
    public class SqlHelperTests : BaseDevelopmentUnitStartup
    {
        public SqlHelperTests()
        {
        }

        [Fact]
        public async Task TestAppendCondition()
        {

            try
            {
                var sql = @"
SELECT 
   bc.*, 
   a.AccountNameVi 
FROM
(
   SELECT 
      cuoi_ky.tk,
      SUM(CASE WHEN cuoi_ky.vnd_dau_ky > 0 THEN cuoi_ky.vnd_dau_ky ELSE 0 END) no_dau_ky,
      SUM(CASE WHEN cuoi_ky.vnd_dau_ky < 0 THEN -cuoi_ky.vnd_dau_ky ELSE 0 END) co_dau_ky,
      SUM(cuoi_ky.vnd_no_trong_ky) no_trong_ky,
      SUM(cuoi_ky.vnd_co_trong_ky) co_trong_ky,
      SUM(CASE WHEN cuoi_ky.vnd_cuoi_ky > 0 THEN cuoi_ky.vnd_cuoi_ky ELSE 0 END) no_cuoi_ky,
      SUM(CASE WHEN cuoi_ky.vnd_cuoi_ky < 0 THEN -cuoi_ky.vnd_cuoi_ky ELSE 0 END) co_cuoi_ky
   FROM
   (
      SELECT 
         ISNULL(no_ck_kh.tk, co_ck_kh.tk) tk,
         ISNULL(no_ck_kh.kh, co_ck_kh.kh) kh, 
         (ISNULL(no_ck_kh.vnd_dau_ky,0) - ISNULL(co_ck_kh.vnd_dau_ky,0)) vnd_dau_ky,
         ISNULL(no_ck_kh.vnd_trong_ky,0) vnd_no_trong_ky,
         ISNULL(co_ck_kh.vnd_trong_ky,0) vnd_co_trong_ky,
         (ISNULL(no_ck_kh.vnd_cuoi_ky,0) - ISNULL(co_ck_kh.vnd_cuoi_ky,0)) vnd_cuoi_ky
      FROM
      (
         SELECT 
            SUBSTRING(no_kh.Mask, 1, LEN(no_kh.Mask) - 1) tk,
            no_kh.kh_no kh,
            SUM(CASE WHEN no_kh.ngay_ct < @FromDate THEN no_kh.vnd ELSE 0 END) vnd_dau_ky,
            SUM(CASE WHEN no_kh.ngay_ct BETWEEN @FromDate AND @ToDate THEN no_kh.vnd ELSE 0 END) vnd_trong_ky,
            SUM(no_kh.vnd) vnd_cuoi_ky
         FROM 
         (     
            SELECT 
               a.Mask,
               CASE WHEN a.IsLiability = 1 THEN tk.kh_no ELSE NULL END kh_no,
               ISNULL(tk.vnd,0) vnd,
               tk.ngay_ct
            FROM
            (
               SELECT
                  CONCAT(a.AccountNumber, '%') Mask,
                  a.AccountNumber, 
                  a.IsLiability 
               FROM v_AccountingAccount a WHERE a.ParentId IS NULL 
            ) a
            INNER JOIN [dbo].[_rc] tk ON tk.tk_no LIKE a.Mask
            WHERE tk.SubsidiaryId = @SubId AND tk.tk_no is not null AND tk.tk_no NOT LIKE '0%' AND tk.ngay_ct <= @ToDate
         )  no_kh
         GROUP BY no_kh.Mask, no_kh.kh_no
      ) no_ck_kh
      FULL OUTER JOIN 
      (
         SELECT 
            SUBSTRING(co_kh.Mask, 1, LEN(co_kh.Mask) - 1) tk,
            co_kh.kh_co kh,
            SUM(CASE WHEN co_kh.ngay_ct < @FromDate THEN co_kh.vnd ELSE 0 END) vnd_dau_ky,
            SUM(CASE WHEN co_kh.ngay_ct BETWEEN @FromDate AND @ToDate THEN co_kh.vnd ELSE 0 END) vnd_trong_ky,
            SUM(co_kh.vnd) vnd_cuoi_ky
         FROM 
         (     
            SELECT 
               a.Mask,
               CASE WHEN a.IsLiability = 1 THEN tk.kh_co ELSE NULL END kh_co,
               ISNULL(tk.vnd,0) vnd,
               tk.ngay_ct
            FROM
            (
               SELECT
                  CONCAT(a.AccountNumber, '%') Mask,
                  a.AccountNumber, 
                  a.IsLiability 
               FROM v_AccountingAccount a WHERE a.ParentId IS NULL 
            ) a
            INNER JOIN [dbo].[_rc] tk ON tk.tk_co LIKE a.Mask
            WHERE tk.SubsidiaryId = @SubId AND tk.tk_co is not null AND tk.tk_co NOT LIKE '0%' AND tk.ngay_ct <= @ToDate
         )  co_kh
         GROUP BY co_kh.Mask, co_kh.kh_co
      ) co_ck_kh
      ON no_ck_kh.tk = co_ck_kh.tk AND ((no_ck_kh.kh IS NULL AND co_ck_kh.kh IS NULL) OR no_ck_kh.kh = co_ck_kh.kh)
   ) cuoi_ky
   GROUP BY/*ss*/cuoi_ky.tk

) bc--g
LEFT JOIN v_AccountingAccount a ON bc.tk = a.AccountNumber/*1*/
--aha
/*
sdd WHERE
*/
--d
";
                var newSlq2 = sql.TSqlAppendCondition("1=1");


                var data = await _accountancyDBContext.QueryDataTable(newSlq2, Array.Empty<SqlParameter>());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }


        }



        private static char[] SpaceChars = new[] { ';', '\n', '\r', '\t', '\v', ' ' };
        private static int GetSqlKeyWordIndex(string sql, string keyword)
        {
            var index = sql.LastIndexOf(keyword, StringComparison.OrdinalIgnoreCase);

            while (index >= 0
                && (
                    (index > 0 && !SpaceChars.Contains(sql[index - 1])) || !SpaceChars.Contains(sql[index + keyword.Length])
                )
            )
            {
                index = sql.LastIndexOf(keyword, index, StringComparison.OrdinalIgnoreCase);
            }


            return index;
        }

        public static string TSqlAppendCondition_Bak(string sql, string filterCondition)
        {
            sql = sql.TrimEnd(SpaceChars);

            sql = TSqlRemoveComments(sql);

            if (!string.IsNullOrEmpty(filterCondition))
            {
                var idxSelect = GetSqlKeyWordIndex(sql, "select");

                int idxWhere = GetSqlKeyWordIndex(sql, "where");

                if (idxWhere < idxSelect)
                {
                    idxWhere = -1;
                }

                if (idxWhere > 0)
                {
                    var stack = new Stack<char>();
                    for (var i = idxWhere + "where".Length; i < sql.Length; i++)
                    {
                        var c = sql[i];
                        if (c == '(')
                        {
                            stack.Push(c);
                        }
                        else if (c == ')')
                        {
                            if (stack.Count > 0)
                            {
                                stack.Pop();
                            }
                            else
                            {
                                stack.Push('F');
                                break;
                            }
                        }
                    }

                    if (stack.Count == 0)
                    {
                        var conditionStart = idxWhere + "where".Length;
                        sql = sql.Insert(conditionStart, " (");
                        sql += ")";

                        sql += $" AND ({filterCondition})";
                    }
                    else
                    {
                        sql += $" WHERE {filterCondition}";
                    }
                }
                else
                {
                    sql += $" WHERE {filterCondition}";
                }

            }
            return sql;
        }

        private static string TSqlRemoveComments(string sql)
        {
            ParseOptions parseOptions = new ParseOptions();
            Scanner scanner = new Scanner(parseOptions);

            int state = 0,
                start,
                end,
                //lastTokenEnd = -1,
                token;

            bool isPairMatch, isExecAutoParamHelp;

            scanner.SetSource(sql, 0);

            var stringBuilder = new StringBuilder();
            var commentTokens = new[] { (int)Tokens.LEX_END_OF_LINE_COMMENT, (int)Tokens.LEX_MULTILINE_COMMENT };



            while ((token = scanner.GetNext(ref state, out start, out end, out isPairMatch, out isExecAutoParamHelp)) != (int)Tokens.EOF)
            {
                var endIndex = end;
                for (var i = end + 1; i < sql.Length; i++)
                {
                    if (SpaceChars.Contains(sql[i]))
                    {
                        endIndex = i;
                    }
                    else
                    {
                        break;
                    }
                }

                var startIndex = start;
                if (commentTokens.Contains(token))
                {
                    startIndex = end + 1;

                    if (endIndex - startIndex + 1 == 0)
                    {
                        stringBuilder.Append(" ");
                    }
                };

                stringBuilder.Append(sql.Substring(startIndex, endIndex - startIndex + 1));
            }

            return stringBuilder.ToString();
        }


    }
}
