using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
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

        protected CategoryEntity GetReferenceCategory(int categoryId)
        {

            CategoryEntity category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);

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

        public IQueryable<int> FilterClauseProcess(Clause clause, IQueryable<CategoryRowValue> query)
        {
            IQueryable<int> exp = query.Select(rv => rv.CategoryRowId);

            //Expression exp = Expression.Constant(true);
            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;
                    exp = BuildExpression(singleClause, query);
                }
                else if (clause is DoubleClause)
                {
                    var filterClause = clause as DoubleClause;
                    IQueryable<int> leftExp = FilterClauseProcess(filterClause.LeftClause, query);
                    IQueryable<int> rightExp = FilterClauseProcess(filterClause.RightClause, query);
                    exp = MergeExpression(leftExp, rightExp, filterClause.LogicOperator);
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;
                    List<IQueryable<int>> expressions = new List<IQueryable<int>>();
                    IQueryable<int> oldEx = null;
                    EnumLogicOperator? logicOperator = EnumLogicOperator.And;
                    foreach (var item in arrClause.Clauses)
                    {
                        if (!logicOperator.HasValue)
                        {
                            break;
                        }
                        if (logicOperator == EnumLogicOperator.Or)
                        {
                            expressions.Add(oldEx);
                            oldEx = FilterClauseProcess(item.Clause, query);
                        }
                        else if (logicOperator == EnumLogicOperator.And)
                        {
                            if (oldEx != null)
                            {
                                oldEx = oldEx.Join(FilterClauseProcess(item.Clause, query), l => l, r => r, (l, r) => l);
                            }
                            else
                            {
                                oldEx = FilterClauseProcess(item.Clause, query);
                            }
                        }
                        logicOperator = item.LogicOperator;
                    }
                    expressions.Add(oldEx);
                    exp = new List<int>().AsQueryable();
                    foreach (var ex in expressions)
                    {
                        exp = exp.Union(ex);
                    }
                    return exp;
                }
            }
            return exp;
        }

        private IQueryable<int> BuildExpression(SingleClause clause, IQueryable<CategoryRowValue> query)
        {
            IQueryable<int> expression = query.Select(rv => rv.CategoryRowId);
            if (clause != null)
            {
                switch (clause.Operator)
                {
                    case EnumOperator.Equal:
                        expression = query.Where(rv => rv.CategoryFieldId == clause.Key && rv.Value == clause.Values[0]).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.NotEqual:
                        expression = query.Where(rv => rv.CategoryFieldId == clause.Key && rv.Value != clause.Values[0]).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.Contains:
                        expression = query.Where(rv => rv.CategoryFieldId == clause.Key && rv.Value.Contains(clause.Values[0])).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.InList:
                        List<string> values = clause.Values[0].Split(',').ToList();
                        expression = query.Where(rv => rv.CategoryFieldId == clause.Key && values.Contains(rv.Value)).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.IsLeafNode:
                        List<string> nodeValues = query
                            .Where(rv => rv.CategoryFieldId == clause.Key)
                            .Select(rv => rv.Value)
                            .ToList();
                        List<string> isLeafValues = nodeValues.Where(v => !nodeValues.Any(n => n != v && n.Contains(v))).ToList();
                        expression = query.Where(rv => rv.CategoryFieldId == clause.Key && isLeafValues.Contains(rv.Value)).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.StartsWith:
                        expression = query.Where(rv => rv.CategoryFieldId == clause.Key && rv.Value.StartsWith(clause.Values[0])).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.EndsWith:
                        expression = query.Where(rv => rv.CategoryFieldId == clause.Key && rv.Value.EndsWith(clause.Values[0])).Select(rv => rv.CategoryRowId);
                        break;
                    default:
                        expression = new List<int>().AsQueryable();
                        break;
                }
            }
            return expression;

        }

        private IQueryable<int> MergeExpression(IQueryable<int> leftExp, IQueryable<int> rightExp, EnumLogicOperator? logicOperator)
        {
            IQueryable<int> expression = leftExp;
            if (logicOperator.HasValue)
            {
                switch (logicOperator.Value)
                {
                    case EnumLogicOperator.And:
                        expression = leftExp.Join(rightExp, l => l, r => r, (r, l) => r);
                        break;
                    case EnumLogicOperator.Or:
                        expression = leftExp.Union(rightExp);
                        break;
                    default:
                        break;
                }
            }
            return expression;
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
                                CategoryRowId = id,
                                CategoryFieldId = field.CategoryFieldId,
                                Value = properties[field.CategoryFieldName],
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
