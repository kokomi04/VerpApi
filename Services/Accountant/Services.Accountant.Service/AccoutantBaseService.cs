using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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

        protected void FillterProcess(ref IQueryable<CategoryRow> query, Clause filters)
        {
            var rvParam = Expression.Parameter(typeof(CategoryRowValue), "rv");
            Expression filterExp = FilterClauseProcess(rvParam, filters, query);
            query = query.Where(r => r.CategoryRowValue.AsQueryable().Any(Expression.Lambda<Func<CategoryRowValue, bool>>(filterExp, rvParam)));
        }

        private Expression BuildExpression(ParameterExpression rvParam, SingleClause clause, IQueryable<CategoryRow> query)
        {
            Expression expression = null;
            if (clause != null)
            {
                var fieldIdProp = Expression.Property(rvParam, nameof(CategoryRowValue.CategoryFieldId));

                // Check categoryFieldId
                Expression fieldExp = Expression.Equal(fieldIdProp, Expression.Constant(clause.Key));
                // Check reference
                var field = typeof(CategoryRowValue).GetProperty(nameof(CategoryRowValue.CategoryField));
                var formType = field.PropertyType.GetProperty(nameof(CategoryField.FormTypeId));
                var fieldProp = Expression.Property(rvParam, field);
                var formTypeProp = Expression.Property(fieldProp, formType);

                Expression refExp = Expression.AndAlso(Expression.Equal(formTypeProp, Expression.Constant((int)EnumFormType.SearchTable)),
                    Expression.Equal(formTypeProp, Expression.Constant((int)EnumFormType.Select)));

                // Check value
                Expression valueExp;
                Expression referValueExp;
                var value = Expression.Constant(clause.Values[0]);

                var valueProp = Expression.Property(rvParam, nameof(CategoryRowValue.Value));
                var refer = typeof(CategoryRowValue).GetProperty(nameof(CategoryRowValue.ReferenceCategoryRowValue));
                var valueRefer = refer.PropertyType.GetProperty(nameof(CategoryRowValue.Value));
                var referProp = Expression.Property(rvParam, refer);
                var valueReferProp = Expression.Property(referProp, valueRefer);
                MethodInfo method;
                switch (clause.Operator)
                {
                    case EnumOperator.Equal:
                        valueExp = Expression.Equal(valueProp, value);
                        referValueExp = Expression.Equal(valueReferProp, value);
                        break;
                    case EnumOperator.NotEqual:
                        valueExp = Expression.NotEqual(valueProp, value);
                        referValueExp = Expression.NotEqual(valueReferProp, value);
                        break;
                    case EnumOperator.Contains:
                        method = typeof(string).GetMethod(nameof(string.Contains));
                        valueExp = Expression.Call(valueProp, method, value);
                        referValueExp = Expression.Call(valueReferProp, method, value);
                        break;
                    case EnumOperator.InList:
                        List<string> values = clause.Values[0].Split(',').ToList();
                        method = typeof(string[]).GetMethod(nameof(List<string>.Contains));
                        valueExp = Expression.Call(Expression.Constant(values), method, valueProp);
                        referValueExp = Expression.Call(Expression.Constant(values), method, valueReferProp);
                        break;
                    case EnumOperator.IsLeafNode:
                        List<string> nodeValues = query
                            .Select(r => r.CategoryRowValue.FirstOrDefault(v => v.CategoryFieldId == clause.Key))
                            .Select(rv => ((EnumFormType)rv.CategoryField.FormTypeId == EnumFormType.SearchTable 
                            || (EnumFormType)rv.CategoryField.FormTypeId == EnumFormType.Select) 
                            ? rv.ReferenceCategoryRowValue.Value : rv.Value)
                            .ToList();
                        List<string> isLeafValues = nodeValues.Where(v => !nodeValues.Any(n => n != v && n.Contains(v))).ToList();
                        method = typeof(string[]).GetMethod(nameof(List<string>.Contains));
                        valueExp = Expression.Call(Expression.Constant(isLeafValues), method, valueProp);
                        referValueExp = Expression.Call(Expression.Constant(isLeafValues), method, valueReferProp);
                        break;
                    case EnumOperator.StartsWith:
                        method = typeof(string).GetMethod(nameof(string.StartsWith));
                        valueExp = Expression.Call(valueProp, method, value);
                        referValueExp = Expression.Call(valueReferProp, method, value);
                        break;
                    case EnumOperator.EndsWith:
                        method = typeof(string).GetMethod(nameof(string.EndsWith));
                        valueExp = Expression.Call(valueProp, method, value);
                        referValueExp = Expression.Call(valueReferProp, method, value);
                        break;
                    default:
                        valueExp = Expression.Constant(true);
                        referValueExp = Expression.Constant(true);
                        break;
                }

                Expression isReferExp = Expression.AndAlso(refExp, referValueExp);
                Expression isNotReferExp = Expression.AndAlso(Expression.Not(refExp), valueExp);
                expression = Expression.AndAlso(fieldExp, Expression.OrElse(isReferExp, isNotReferExp));
            }
            return expression;
        }
        
        private Expression MergeExpression(Expression leftExp, Expression rightExp, EnumLogicOperator? logicOperator)
        {
            Expression expression = leftExp;
            if (logicOperator.HasValue)
            {
                switch (logicOperator.Value)
                {
                    case EnumLogicOperator.And:
                        expression = Expression.AndAlso(leftExp, rightExp);
                        break;
                    case EnumLogicOperator.Or:
                        expression = Expression.OrElse(leftExp, rightExp);
                        break;
                    default:
                        break;
                }
            }
            return expression;
        }

        public Expression FilterClauseProcess(ParameterExpression rvParam, Clause clause, IQueryable<CategoryRow> query)
        {
            Expression exp = Expression.Constant(false);
            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;
                    exp = BuildExpression(rvParam, singleClause, query);
                }
                else if (clause is FilterClause)
                {
                    var filterClause = clause as FilterClause;
                    var leftExp = FilterClauseProcess(rvParam, filterClause.LeftClause, query);
                    var rightExp = FilterClauseProcess(rvParam, filterClause.RightClause, query);
                    exp = MergeExpression(leftExp, rightExp, filterClause.LogicOperator);
                }
            }
            return exp;
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
