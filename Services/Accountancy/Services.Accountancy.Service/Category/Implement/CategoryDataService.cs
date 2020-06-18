using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Accountancy.Service.Category
{
    public class CategoryDataService : ICategoryDataService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyContext;

        public CategoryDataService(AccountancyDBContext accountancyContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _accountancyContext = accountancyContext;
            _appSetting = appSetting.Value;
            _mapper = mapper;
        }

        public Task<ServiceResult<int>> AddCategoryRow(int categoryId, NonCamelCaseDictionary data)
        {
            throw new NotImplementedException();
            //var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            //if (category == null)
            //{
            //    throw BadRequestException(CategoryErrorCode.CategoryNotFound);
            //}
            //var tableName = $"v{category.CategoryCode}";
            //var fields = (from f in _accountancyContext.CategoryField
            //              join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
            //              where c.CategoryId == categoryId && f.FormTypeId != (int)EnumFormType.ViewOnly
            //              select new SelectField
            //              {
            //                  CategoryFieldName = f.CategoryFieldName,
            //                  RefTableCode = f.RefTableCode,
            //                  RefTableField = f.RefTableField,
            //                  RefTableTitle = f.RefTableTitle,
            //                  DataTypeId = f.DataTypeId
            //              }).ToList();



        }

        public async Task<ServiceResult<NonCamelCaseDictionary>> GetCategoryRow(int categoryId, int fId)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            var tableName = $"v{category.CategoryCode}";
            var fields = (from f in _accountancyContext.CategoryField
                          join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                          where c.CategoryId == categoryId && f.FormTypeId != (int)EnumFormType.ViewOnly
                          select new SelectField
                          {
                              CategoryFieldName = f.CategoryFieldName,
                              RefTableCode = f.RefTableCode,
                              RefTableField = f.RefTableField,
                              RefTableTitle = f.RefTableTitle,
                              DataTypeId = f.DataTypeId
                          }).ToList();

            var dataSql = new StringBuilder();
            dataSql.Append(GetSelect(tableName, fields));
            dataSql.Append($" FROM {tableName} WHERE [{tableName}].F_Id = {fId}");

            var data = await _accountancyContext.QueryDataTable(dataSql.ToString(), Array.Empty<SqlParameter>());
            var lst = ConvertData(data);
            NonCamelCaseDictionary row;
            if (lst.Count > 0)
            {
                row = lst[0];
            }
            else
            {
                throw new BadRequestException(CategoryErrorCode.CategoryRowNotFound);
            }
            return row;
        }

        private string GetSelect(string tableName, List<SelectField> fields)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append($"SELECT ");
            foreach (var field in fields)
            {
                if (string.IsNullOrEmpty(field.RefTableCode))
                {
                    sql.Append($"[{tableName}].{field.CategoryFieldName},");
                }
                else
                {
                    sql.Append($"[{tableName}].{field.CategoryFieldName},");
                    foreach (var item in field.RefTableTitle.Split(","))
                    {
                        var title = item.Trim();
                        sql.Append($"[{tableName}].{field.RefTableCode}_{title}");
                    }
                }
            }
            if (fields.Count > 0)
            {
                sql.Remove(sql.Length - 1, 0);
            }
            return sql.ToString();
        }
        private List<NonCamelCaseDictionary> ConvertData(DataTable data)
        {
            var lst = new List<NonCamelCaseDictionary>();
            for (var i = 0; i < data.Rows.Count; i++)
            {
                var row = data.Rows[i];
                var dic = new NonCamelCaseDictionary();
                foreach (DataColumn c in data.Columns)
                {
                    var v = row[c];
                    if (v != null && v.GetType() == typeof(DateTime) || v.GetType() == typeof(DateTime?))
                    {
                        var vInDateTime = (v as DateTime?).GetUnix();
                        dic.Add(c.ColumnName, vInDateTime);
                    }
                    else
                    {
                        dic.Add(c.ColumnName, row[c]);
                    }
                }
                lst.Add(dic);
            }
            return lst;
        }

        public async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(int categoryId, string keyword, string filters, int page, int size)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            var tableName = $"v{category.CategoryCode}";
            var fields = (from f in _accountancyContext.CategoryField
                          join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                          where c.CategoryId == categoryId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.IsShowList == true
                          select new SelectField
                          {
                              CategoryFieldName = f.CategoryFieldName,
                              RefTableCode = f.RefTableCode,
                              RefTableField = f.RefTableField,
                              RefTableTitle = f.RefTableTitle,
                              DataTypeId = f.DataTypeId
                          }).ToList();

            var dataSql = new StringBuilder();
            dataSql.Append(GetSelect(tableName, fields));
            dataSql.Append($" FROM {tableName}");
            var serchCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                foreach (var field in fields)
                {
                    if (serchCondition.Length > 0)
                    {
                        serchCondition.Append(" OR ");
                    }

                    if (string.IsNullOrEmpty(field.RefTableCode))
                    {
                        serchCondition.Append($"[{tableName}].{field.CategoryFieldName} LIKE %{keyword}%");
                    }
                    else
                    {
                        foreach (var item in field.RefTableTitle.Split(","))
                        {
                            var title = item.Trim();
                            serchCondition.Append($"[{tableName}].{field.RefTableCode}_{title} LIKE %{keyword}%");
                        }
                    }
                }
            }
            var totalSql = new StringBuilder($"SELECT COUNT(F_Id) as Total FROM {tableName}");
            if (serchCondition.Length > 0)
            {
                dataSql.Append($" WHERE {serchCondition.ToString()}");
                totalSql.Append($" WHERE {serchCondition.ToString()}");
            }

            var countTable = await _accountancyContext.QueryDataTable(totalSql.ToString(), Array.Empty<SqlParameter>());
            var total = 0;
            if (countTable != null && countTable.Rows.Count > 0)
            {
                total = (countTable.Rows[0]["Total"] as int?).GetValueOrDefault();
            }
            dataSql.Append($" ORDER BY [{tableName}].F_Id");
            if (size > 0)
            {
                dataSql.Append($" OFFSET {(page - 1) * size} ROWS FETCH NEXT {size} ROWS ONLY;");
            }

            var data = await _accountancyContext.QueryDataTable(dataSql.ToString(), Array.Empty<SqlParameter>());
            var lst = ConvertData(data);

            return (lst, total);
        }


        private class SelectField
        {
            public string CategoryFieldName { get; set; }
            public string RefTableCode { get; set; }
            public string RefTableField { get; set; }
            public string RefTableTitle { get; set; }
            public int DataTypeId { get; set; }
        }
    }
}
