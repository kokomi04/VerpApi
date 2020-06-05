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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.EF.EFExtensions;
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

        public IQueryable<int> FilterClauseProcess(Clause clause, IQueryable<CategoryRowValue> query, bool not = false)
        {
            IQueryable<int> exp = query.Select(i => i.CategoryRowId);

            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;
                    exp = BuildExpression(singleClause, query, not);
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;
                    bool isNot = not ^ arrClause.Not;
                    bool isOr = (!isNot && arrClause.Condition == EnumLogicOperator.Or) || (isNot && arrClause.Condition == EnumLogicOperator.And);
                    foreach (var item in arrClause.Rules)
                    {
                        if (exp == null)
                        {
                            exp = FilterClauseProcess(item, query, isNot);
                        }
                        else
                        {

                            if (isOr)
                            {
                                exp = exp.Union(FilterClauseProcess(item, query, isNot));
                            }
                            else
                            {
                                exp = exp.Join(FilterClauseProcess(item, query, isNot), l => l, r => r, (l, r) => l);
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private IQueryable<int> BuildExpression(SingleClause clause, IQueryable<CategoryRowValue> query, bool not)
        {
            IQueryable<int> expression = query.Select(rv => rv.CategoryRowId);
            if (clause != null)
            {
                switch (clause.Operator)
                {
                    case EnumOperator.Equal:
                        expression = query.Where(rv => rv.CategoryFieldId == clause.FieldId && (not ? rv.Value != (string)clause.Value : rv.Value == (string)clause.Value)).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.NotEqual:
                        expression = query.Where(rv => rv.CategoryFieldId == clause.FieldId && (not ? rv.Value == (string)clause.Value : rv.Value != (string)clause.Value)).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.Contains:
                        expression = query.Where(rv => rv.CategoryFieldId == clause.FieldId && (not ? !rv.Value.Contains((string)clause.Value) : rv.Value.Contains((string)clause.Value))).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.InList:
                        List<string> values = ((string)clause.Value).Split(',').ToList();
                        expression = query.Where(rv => rv.CategoryFieldId == clause.FieldId && (not ? !values.Contains(rv.Value) : values.Contains(rv.Value))).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.IsLeafNode:
                        List<string> nodeValues = query
                            .Where(rv => rv.CategoryFieldId == clause.FieldId)
                            .Select(rv => rv.Value)
                            .ToList();
                        List<string> isLeafValues = nodeValues.Where(v => !nodeValues.Any(n => n != v && n.Contains(v))).ToList();
                        expression = query.Where(rv => rv.CategoryFieldId == clause.FieldId && (not ? !isLeafValues.Contains(rv.Value) : isLeafValues.Contains(rv.Value))).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.StartsWith:
                        expression = query.Where(rv => rv.CategoryFieldId == clause.FieldId && (not ? !rv.Value.StartsWith((string)clause.Value) : rv.Value.StartsWith((string)clause.Value))).Select(rv => rv.CategoryRowId);
                        break;
                    case EnumOperator.EndsWith:
                        expression = query.Where(rv => rv.CategoryFieldId == clause.FieldId && (not ? !rv.Value.EndsWith((string)clause.Value) : rv.Value.EndsWith((string)clause.Value))).Select(rv => rv.CategoryRowId);
                        break;
                    default:
                        break;
                }
            }

            return expression;

        }

        protected IQueryable<CategoryRow> GetOutSideCategoryRows(int categoryId, object body = null)
        {
            List<CategoryRow> lst = new List<CategoryRow>();
            var config = _accountingContext.OutSideDataConfig.FirstOrDefault(cf => cf.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(config?.Url))
            {
                string url = $"{config.Url}?page=1&size=9999";
                (PageData<JObject>, HttpStatusCode) result = GetFromAPI<PageData<JObject>>(url, 100000, HttpMethod.Post, body);
                if (result.Item2 == HttpStatusCode.OK)
                {
                    List<CategoryField> fields = _accountingContext.CategoryField.Where(f => categoryId == f.CategoryId).ToList();

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
                            string value = string.Empty;
                            if (field.CategoryFieldName == AccountantConstants.F_IDENTITY)
                            {
                                value = id.ToString();
                            }
                            else
                            {
                                bool bValue = properties.TryGetValue(field.CategoryFieldName, out value);
                                if (value == true.ToString() || value == false.ToString())
                                {
                                    value = value.ToLower();
                                }
                            }

                            CategoryRowValue rowValue = new CategoryRowValue
                            {
                                CategoryRowId = id,
                                CategoryFieldId = field.CategoryFieldId,
                                Value = value,
                                CategoryField = field
                            };

                            categoryRow.CategoryRowValue.Add(rowValue);
                        }
                        lst.Add(categoryRow);
                    }
                }
            }

            return lst.AsQueryable();
        }

        public (T, HttpStatusCode) GetFromAPI<T>(string url, int apiTimeOut, HttpMethod method, object body = null)
        {
            HttpClient client = new HttpClient();
            T result = default;
            HttpStatusCode status = HttpStatusCode.OK;

            var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/{url}";

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(uri),
            };
            httpRequestMessage.Content = new StringContent("{}", Encoding.UTF8, "application/json");
            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body);
                httpRequestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

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

        public Clause AddFieldName(Clause filterClause, List<CategoryField> fields)
        {
            Clause result = null;
            if (filterClause is SingleClause)
            {
                var field = fields.FirstOrDefault(f => f.CategoryFieldId == (filterClause as SingleClause).FieldId);
                if(field == null)
                {
                    throw new BadRequestException(CategoryErrorCode.CategoryFieldNotFound);
                }
                result = new SingleClause
                {
                    FieldId = (filterClause as SingleClause).FieldId,
                    FieldName = field.CategoryFieldName,
                    Operator = (filterClause as SingleClause).Operator,
                    Value = (filterClause as SingleClause).Value,
                };
            }
            else if (filterClause is ArrayClause)
            {
                result = new ArrayClause
                {
                    Condition = (filterClause as ArrayClause).Condition,
                    Not = (filterClause as ArrayClause).Not
                };
                foreach (var item in (filterClause as ArrayClause).Rules)
                {
                    Clause clause = AddFieldName(item, fields);
                    (result as ArrayClause).Rules.Add(clause);
                }
            }
            return result;
        }

    }
}
