using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountant.Model.Category;
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErp.Services.Accountant.Service
{
    public abstract class AccoutantBaseService
    {
        protected readonly AppSetting _appSetting;
        protected readonly IMapper _mapper;
        protected readonly AccountingDBContext _accountingContext;
        private delegate Expression<Func<T, bool>> LogicOperator<T>(Expression<Func<T, bool>> expr);
        protected AccoutantBaseService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , IMapper mapper)
        {
            _accountingContext = accountingContext;
            _appSetting = appSetting.Value;
            _mapper = mapper;
        }

        protected int[] GetAllCategoryIds(int categoryId)
        {
            List<int> ids = new List<int> { categoryId };
            foreach (int id in _accountingContext.Category.Where(r => r.ParentId == categoryId).Select(r => r.CategoryId))
            {
                ids.AddRange(GetAllCategoryIds(id));
            }

            return ids.ToArray();
        }

        protected CategoryEntity GetReferenceCategory(CategoryField referenceCategoryField)
        {
            CategoryEntity category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == referenceCategoryField.CategoryId);

            while (category != null && !category.IsModule)
            {
                if (!category.ParentId.HasValue)
                {
                    break;
                }
                category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == category.ParentId);
            }
            return category;
        }

        protected void FillterProcess(ref IQueryable<CategoryRow> query, FilterModel[] filters)
        {
            Expression<Func<CategoryRow, bool>> predicate = PredicateBuilder.True<CategoryRow>();
            if (filters.Count() > 0)
            {
                predicate = PredicateBuilder.False<CategoryRow>();
                EnumLogicOperator? logicOperator = EnumLogicOperator.Or;
                foreach (FilterModel filter in filters)
                {
                    LogicOperator<CategoryRow> logic;
                    if (logicOperator == EnumLogicOperator.Or)
                    {
                        logic = predicate.Or<CategoryRow>;
                    }
                    else
                    {
                        logic = predicate.And<CategoryRow>;
                    }

                    logicOperator = filter.LogicOperator;

                    switch (filter.Operator)
                    {
                        case EnumOperator.Equal:
                            predicate = logic(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.ReferenceCategoryRowValue.Value == filter.Values[0] : rv.Value == filter.Values[0]));
                            break;
                        case EnumOperator.NotEqual:
                            predicate = logic(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.ReferenceCategoryRowValue.Value != filter.Values[0] : rv.Value != filter.Values[0]));
                            break;
                        case EnumOperator.Contains:
                            predicate = logic(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.ReferenceCategoryRowValue.Value.Contains(filter.Values[0]) : rv.Value.Contains(filter.Values[0])));
                            break;
                        case EnumOperator.InList:
                            string[] values = filter.Values[0].Split(',');
                            predicate = logic(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? values.Contains(rv.ReferenceCategoryRowValue.Value) : values.Contains(rv.Value)));
                            break;
                        case EnumOperator.IsLeafNode:
                            List<string> nodeValues = query
                                .Select(r => r.CategoryRowValue.FirstOrDefault(v => v.CategoryFieldId == filter.CategoryFieldId))
                                .Select(rv => (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.ReferenceCategoryRowValue.Value : rv.Value)
                                .ToList();

                            List<string> isLeafValues = nodeValues.Where(v => !nodeValues.Any(n => n != v && n.Contains(v))).ToList();
                            predicate = logic(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? isLeafValues.Contains(rv.ReferenceCategoryRowValue.Value) : isLeafValues.Contains(rv.Value)));
                            break;
                        case EnumOperator.StartsWith:
                            predicate = logic(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.ReferenceCategoryRowValue.Value.StartsWith(filter.Values[0]) : rv.Value.StartsWith(filter.Values[0])));
                            break;
                        case EnumOperator.EndsWith:
                            predicate = logic(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.ReferenceCategoryRowValue.Value.EndsWith(filter.Values[0]) : rv.Value.EndsWith(filter.Values[0])));
                            break;
                        default:
                            break;
                    }
                    if (!logicOperator.HasValue)
                    {
                        break;
                    }
                }
            }
            query = query.Where(predicate);
        }

        protected IQueryable<CategoryRow> GetOutSideCategoryRows(int categoryId)
        {
            List<CategoryRow> lst = new List<CategoryRow>();
            var config = _accountingContext.OutSideDataConfig.FirstOrDefault(cf => cf.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(config?.Url))
            {
                string url = $"{config.Url}?page=1&size=9999";
                (PageData<JObject>, HttpStatusCode) result = GetFromAPI<PageData<JObject>>(url, 100000);
                if (result.Item2 == HttpStatusCode.OK)
                {
                    int[] categoryIds = GetAllCategoryIds(categoryId);
                    List<CategoryField> fields = _accountingContext.CategoryField.Where(f => categoryIds.Contains(f.CategoryId)).ToList();

                    foreach (var item in result.Item1.List)
                    {
                        // Lấy thông tin row
                        Dictionary<string, string> properties = new Dictionary<string, string>();
                        foreach (var jprop in item.Properties())
                        {
                            var key = jprop.Name;
                            var value = jprop.Value.ToString();
                            properties.Add(key, value);
                        }

                        // Map row Id
                        int id = int.Parse(properties[config.Key]);
                        CategoryRow categoryRow = new CategoryRow
                        {
                            CategoryRowId = id,
                            CategoryId = categoryId,
                        };

                        // Map value cho các field
                        foreach (var field in fields)
                        {
                            var value = new CategoryRowValue
                            {
                                CategoryFieldId = field.CategoryFieldId,
                                Value = properties[field.CategoryFieldName],
                                CategoryField = field
                            };
                            categoryRow.CategoryRowValue.Add(value);
                        }
                        lst.Add(categoryRow);
                    }
                }
            }

            return lst.AsQueryable();
        }

        public (T, HttpStatusCode) GetFromAPI<T>(string url, int apiTimeOut)
        {
            HttpClient client = new HttpClient();
            T result = default;
            HttpStatusCode status = HttpStatusCode.OK;

            var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/{url}";
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri)
            };
            httpRequestMessage.Headers.TryAddWithoutValidation(Headers.CrossServiceKey, _appSetting?.Configuration?.InternalCrossServiceKey);
            CancellationTokenSource cts = new CancellationTokenSource(apiTimeOut);

            HttpResponseMessage response = client.SendAsync(httpRequestMessage, cts.Token).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                result = JsonConvert.DeserializeObject<T>(data);
            }
            else
            {
                status = response.StatusCode;
            }
            response.Dispose();
            cts.Dispose();
            httpRequestMessage.Dispose();
            client.Dispose();
            return (result, status);
        }

    }
}
