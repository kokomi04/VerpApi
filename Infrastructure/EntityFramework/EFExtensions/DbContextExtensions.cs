﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public static class DbContextExtensions
    {
        public static DbContextOptions<T> ChangeOptionsType<T>(this DbContextOptions options, ILoggerFactory loggerFactory) where T : DbContext
        {
            var sqlExt = options.Extensions.FirstOrDefault(e => e is SqlServerOptionsExtension);

            if (sqlExt == null)
                throw (new Exception("Failed to retrieve SQL connection string for base Context"));

            return new DbContextOptionsBuilder<T>()
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                        .EnableSensitiveDataLogging(true)
                        .UseSqlServer(((SqlServerOptionsExtension)sqlExt).ConnectionString)
                        .UseLoggerFactory(loggerFactory)
                        .Options;
        }

        public static void TryRollbackTransaction(this IDbContextTransaction trans)
        {
            try
            {
                trans.Rollback();
            }
            catch (Exception)
            {

            }
        }
        public static async Task TryRollbackTransactionAsync(this IDbContextTransaction trans)
        {
            try
            {
                await trans.RollbackAsync();
            }
            catch (Exception)
            {

            }
        }

        public static void RollbackEntities(this DbContext context)
        {
            var changedEntries = context.ChangeTracker.Entries()
                .Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }

        public static bool HasChanges(this DbContext context)
        {
            return context.ChangeTracker.HasChanges();
        }

        public static void AddFilterBase(this ModelBuilder modelBuilder)
        {

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var filterBuilder = new FilterExpressionBuilder(entityType.ClrType);

                var isDeletedProp = entityType.FindProperty(GlobalFieldConstants.IsDeleted);
                if (isDeletedProp != null)
                {
                    var isDeleted = Expression.Constant(false);
                    filterBuilder.AddFilter(GlobalFieldConstants.IsDeleted, isDeleted);
                }

                entityType.SetQueryFilter(filterBuilder.Build());
            }
        }

        public static void AddFilterAuthorize(this ModelBuilder modelBuilder, DbContext dbContext)
        {
            bool filterSubId = true;
            bool filterStock = true;
            if (dbContext is IDbContextFilterTypeCache filterCache)
            {
                if (filterCache.IgnoreFilterSubsidiary)
                    filterSubId = false;

                if (filterCache.IgnoreFilterStock)
                    filterStock = false;
            }

            var ctxConstant = Expression.Constant(dbContext);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {

                var filterBuilder = new FilterExpressionBuilder(entityType.ClrType);

                var isDeletedProp = entityType.FindProperty(GlobalFieldConstants.IsDeleted);
                if (isDeletedProp != null)
                {
                    var isDeleted = Expression.Constant(false);
                    filterBuilder.AddFilter(GlobalFieldConstants.IsDeleted, isDeleted);
                }

                var isSubsidiaryIdProp = entityType.FindProperty(GlobalFieldConstants.SubsidiaryId);
                if (isSubsidiaryIdProp != null && dbContext is ISubsidiayRequestDbContext && filterSubId)
                {
                    var subsidiaryId = Expression.PropertyOrField(ctxConstant, nameof(ISubsidiayRequestDbContext.SubsidiaryId));
                    filterBuilder.AddFilter(GlobalFieldConstants.SubsidiaryId, subsidiaryId);

                }

                var isStockIdProp = entityType.FindProperty(GlobalFieldConstants.StockId);
                if (isStockIdProp != null && dbContext is IStockRequestDbContext db && filterStock && db.StockIds != null)
                {
                    var stockIds = Expression.PropertyOrField(ctxConstant, nameof(IStockRequestDbContext.StockIds));
                    filterBuilder.AddFilterListContains<int>(GlobalFieldConstants.StockId, stockIds);
                }

                entityType.SetQueryFilter(filterBuilder.Build());
            }
        }


        public static void SetHistoryBaseValue(this DbContext context, ICurrentContextService currentContext)
        {
            var entries = context.ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                var obj = entityEntry.Entity;

                var isDeleted = (bool?)obj.GetValue("IsDeleted") == true;

                if (!isDeleted)
                {
                    /**
                     * Validate if Code field contains special characters
                     * 
                     */
                    var type = obj.GetType();
                    var ps = type.GetProperties();

                    var exceptCodeFields = new[]
                    {
                        "jscode",
                        "lastcode",
                        "reftablecode",
                        "generatecode",
                        "categorycode",
                        "symbolcode",
                        "areacode",
                        "typecode",
                        "actionbuttoncode",
                        "tempcode",
                        "generatecode"
                    };

                    foreach (var prop in ps)
                    {
                        var propName = prop.Name.ToLower();
                        if (propName.EndsWith("code") && !exceptCodeFields.Any(c => propName.EndsWith(c)))
                        {
                            var code = prop.GetValue(obj) as string;
                            Utils.ValidateCodeSpecialCharactors(code);
                            prop.SetValue(obj, code?.Trim()?.ToUpper());
                        }
                    }
                }

                /**
                 * Set history base
                 */
                obj.SetValue("UpdatedByUserId", currentContext.UserId);

                if (entityEntry.State == EntityState.Added)
                {

                    obj.SetValue("CreatedDatetimeUtc", DateTime.UtcNow);
                    obj.SetValue("CreatedByUserId", currentContext.UserId);
                    obj.SetValue("IsDeleted", false);

                    obj.SetValue("UpdatedDatetimeUtc", DateTime.UtcNow);
                    obj.SetValue("UpdatedByUserId", currentContext.UserId);

                    obj.SetValue("DeletedDatetimeUtc", null);

                    if (!obj.GetType().Name.Contains("Subsidiary"))
                    {
                        var p = obj.GetType().GetProperty(GlobalFieldConstants.SubsidiaryId);
                        if (p != null)
                        {
                            p.SetValue(obj, currentContext.SubsidiaryId);
                        }
                    }

                }
                else
                {
                    if (isDeleted)
                    {
                        obj.SetValue("DeletedDatetimeUtc", DateTime.UtcNow);
                    }
                    else
                    {
                        obj.SetValue("UpdatedDatetimeUtc", DateTime.UtcNow);
                    }
                }
            }
        }


        public static IEnumerable<IDbContextTransaction> BeginTransaction(params DbContext[] contexts)
        {
            foreach (var ctx in contexts)
            {
                yield return ctx.Database.BeginTransactionAsync().GetAwaiter().GetResult();
            }
        }

        private static void SetValue(this object obj, string property, object value)
        {
            var type = obj.GetType();
            var p = type.GetProperty(property);
            if (p != null)
            {
                p.SetValue(obj, value);
            }
        }

        private static object GetValue(this object obj, string property)
        {
            var type = obj.GetType();
            var p = type.GetProperty(property);
            if (p != null)
            {
                return p.GetValue(obj);
            }

            return null;
        }


        /// <summary>
        /// DynamicSelectGenerator
        /// Ref docs: https://stackoverflow.com/questions/42820866/dynamically-build-iqueryable-select-clause-with-fieldnames-in-net-core
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <param name="fieldFroms"></param>
        /// <returns></returns>
        public static IQueryable<TResult> DynamicSelectGenerator<T, TResult>(this IQueryable<T> query, Dictionary<string, string> fieldFroms)
        {
            Dictionary<string, string> EntityFields;
            if (fieldFroms.Count == 0)
                // get Properties of the T
                EntityFields = typeof(T).GetProperties().Select(propertyInfo => propertyInfo.Name).ToArray().ToDictionary(f => f, f => f);
            else
                EntityFields = fieldFroms;

            // input parameter "o"
            var xParameter = Expression.Parameter(typeof(T), "o");

            // new statement "new Data()"
            var xNew = Expression.New(typeof(TResult));

            // create initializers
            var bindings = EntityFields
                .Select(o =>
                {
                    // property "Field1"
                    var toField = typeof(TResult).GetProperty(o.Key);

                    // original value "o.Field1"
                    Expression xOriginal = null;

                    if (!o.Value.StartsWith('['))
                    {
                        // property "Field1"
                        //var fromField = typeof(T).GetProperty(o.Value);

                        //xOriginal = Expression.Property(xParameter, fromField);

                        // property "Field1"
                        var fields = o.Value.Split('.');

                        xOriginal = (Expression)Expression.Property(xParameter, fields[0]);

                        var obj = xOriginal;

                        for (var i = 1; i < fields.Length; i++)
                        {
                            xOriginal = Expression.Property(obj, fields[i]);
                        }
                    }
                    else
                    {
                        xOriginal = Expression.Constant(o.Value.Trim('[').Trim(']'));
                    }

                    // set value "Field1 = o.Field1"
                    return Expression.Bind(toField, xOriginal);
                }
            );

            // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var xInit = Expression.MemberInit(xNew, bindings);

            // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var lambda = Expression.Lambda<Func<T, TResult>>(xInit, xParameter);

            // compile to Func<Data, Data>
            //return lambda.Compile();

            return query.Select(lambda);
        }

        //public static IQueryable<T> InternalFilter<T>(this IQueryable<T> query, Dictionary<string, List<string>> filters = null)
        //{
        //    if (filters != null && filters.Count > 0)
        //    {
        //        foreach (var filter in filters)
        //        {
        //            var sParam = Expression.Parameter(typeof(T), "s");
        //            var prop = Expression.Property(sParam, filter.Key);
        //            TypeConverter typeConverter = TypeDescriptor.GetConverter(prop.Type);
        //            Type listType = typeof(List<>);
        //            Type constructedListType = listType.MakeGenericType(prop.Type);
        //            var instance = Activator.CreateInstance(constructedListType);
        //            foreach (var value in filter.Value)
        //            {
        //                MethodInfo method = constructedListType.GetMethod("Add");
        //                method.Invoke(instance, new object[] { typeConverter.ConvertFromString(value) });
        //            }
        //            var methodInfo = constructedListType.GetMethod("Contains");
        //            var expression = Expression.Call(Expression.Constant(instance), methodInfo, prop);
        //            query = query.Where(Expression.Lambda<Func<T, bool>>(expression, sParam));
        //        }
        //    }
        //    return query;
        //}

        public static IQueryable<T> InternalFilter<T>(this IQueryable<T> query, Clause filters = null, int? timeZoneOffset = null)
        {
            if (filters != null)
            {
                var param = Expression.Parameter(typeof(T), "s");
                Expression filterExp = FilterClauseProcess<T>(param, filters, query, false, timeZoneOffset);
                if (filterExp != null)
                    query = query.Where(Expression.Lambda<Func<T, bool>>(filterExp, param));
            }
            return query;
        }

        public static IQueryable<T> InternalOrderBy<T>(this IQueryable<T> query, string orderByFieldName, bool asc)
        {
            if (!string.IsNullOrWhiteSpace(orderByFieldName))
            {
                string command = asc ? "OrderBy" : "OrderByDescending";
                var type = typeof(T);
                var propertyNames = orderByFieldName.Split(".");

                var parameter = Expression.Parameter(type, "s");
                Expression body = parameter;
                foreach (var propertyName in propertyNames)
                {
                    body = Expression.PropertyOrField(body, propertyName);
                }
                var orderByExpression = Expression.Lambda(body, parameter);
                var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, body.Type },
                    query.Expression, Expression.Quote(orderByExpression));
                query = query.Provider.CreateQuery<T>(resultExpression);
            }
            return query;
        }


        public static Expression FilterClauseProcess<T>(ParameterExpression param, Clause clause, IQueryable<T> query, bool not = false, int? timeZoneOffset = null)
        {
            Expression exp = null;
            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;
                    exp = BuildExpression<T>(param, singleClause, timeZoneOffset);
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;
                    //bool isNot = not ^ arrClause.Not;
                    //bool isOr = (!isNot && arrClause.Condition == EnumLogicOperator.Or) || (isNot && arrClause.Condition == EnumLogicOperator.And);

                    var isNot = arrClause.Not;
                    bool isOr = arrClause.Condition == EnumLogicOperator.Or;
                    foreach (var item in arrClause.Rules)
                    {
                        if (exp == null)
                        {
                            exp = FilterClauseProcess<T>(param, item, query, isNot, timeZoneOffset);
                        }
                        else
                        {
                            if (isOr)
                            {
                                exp = Expression.OrElse(exp, FilterClauseProcess<T>(param, item, query, isNot, timeZoneOffset));
                            }
                            else
                            {
                                exp = Expression.AndAlso(exp, FilterClauseProcess<T>(param, item, query, isNot, timeZoneOffset));
                            }
                        }
                    }

                    if (isNot)
                    {
                        exp = Expression.Not(exp);
                    }

                }
            }
            return exp;
        }

        private static Expression BuildExpression<T>(ParameterExpression param, SingleClause clause, int? timeZoneOffset = null)
        {
            Expression expression = null;
            if (clause != null)
            {
                var propertyNames = clause.FieldName.Split(".");
                Expression prop = param;
                //var nullable = false;
                foreach (var propertyName in propertyNames)
                {
                    prop = Expression.PropertyOrField(prop, propertyName);


                    if (Nullable.GetUnderlyingType(prop.Type) != null)
                    {
                        //var getValueMethod = prop.Type.GetMethod("GetValueOrDefault", Type.EmptyTypes);
                        //prop = Expression.Call(prop, getValueMethod);
                        //nullable = true;
                    }
                }


                if (clause.DataType == EnumDataType.Date && prop.Type == typeof(long))
                    clause.DataType = EnumDataType.BigInt;

                //var prop = Expression.Property(param, clause.FieldName);
                // Check value
                Expression value = null;
                MethodInfo method;

                var toStringMethod = prop.Type.GetMethod("ToString", Type.EmptyTypes);
                var propExpression = Expression.Call(prop, toStringMethod);


                object dbValue = null;

                Expression fromDate = null;
                Expression toDate = null;
                if (clause.DataType == EnumDataType.Date)
                {
                    if (clause.Value.IsNullOrEmptyObject())
                    {
                        fromDate = Expression.PropertyOrField(Expression.Constant(new { p = (DateTime?)null }), "p");
                        toDate = Expression.PropertyOrField(Expression.Constant(new { p = (DateTime?)null }), "p");
                    }
                    else
                    {
                        var date = ((long)clause.Value).UnixToDateTime(timeZoneOffset);
                        var fDate = new DateTime(date.Year, date.Month, date.Day).ToUtc(timeZoneOffset);
                        var tDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 990).ToUtc(timeZoneOffset);
                        if (Nullable.GetUnderlyingType(prop.Type) != null)
                        {
                            fromDate = Expression.PropertyOrField(Expression.Constant(new { p = (DateTime?)fDate }), "p");
                            toDate = Expression.PropertyOrField(Expression.Constant(new { p = (DateTime?)tDate }), "p");
                        }
                        else
                        {
                            fromDate = Expression.PropertyOrField(Expression.Constant(new { p = fDate }), "p");
                            toDate = Expression.PropertyOrField(Expression.Constant(new { p = tDate }), "p");
                        }

                    }

                }
                if (clause.Operator != EnumOperator.InList)
                {
                    dbValue = clause.Value.IsNullOrEmptyObject() ? clause.Value : clause.DataType.GetSqlValue(clause.Value);

                    //value = Expression.Constant(dbValue, prop.Type);
                    //if (nullable)
                    //{
                    //    value = Expression.Convert(value, prop.Type);
                    //}
                    value = Expression.PropertyOrField(Expression.Constant(new { p = dbValue }), "p");

                    value = Expression.Convert(value, prop.Type);


                }
                switch (clause.Operator)
                {
                    case EnumOperator.Equal:
                        expression = Expression.Equal(prop, value);
                        if (clause.DataType == EnumDataType.Date)
                        {
                            expression = Expression.And(Expression.GreaterThanOrEqual(prop, fromDate), Expression.LessThanOrEqual(prop, toDate));
                        }
                        break;
                    case EnumOperator.NotEqual:
                        expression = Expression.NotEqual(prop, value);
                        if (clause.DataType == EnumDataType.Date)
                        {
                            expression = Expression.Or(Expression.LessThan(prop, fromDate), Expression.GreaterThan(prop, toDate));
                        }
                        break;
                    case EnumOperator.Contains:

                        method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
                        if (prop.Type == typeof(string))
                        {
                            expression = Expression.Call(prop, method, value);
                        }
                        else
                        {
                            expression = Expression.Call(propExpression, method, value);
                        }

                        break;
                    case EnumOperator.NotContains:

                        method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
                        if (prop.Type == typeof(string))
                        {
                            expression = Expression.Not(Expression.Call(prop, method, value));
                        }
                        else
                        {
                            expression = Expression.Not(Expression.Call(propExpression, method, value));
                        }

                        break;
                    case EnumOperator.InList:

                        // int inSuffix = 0;
                        // var paramNames = new StringBuilder();
                        IList<object> values = new List<object>();
                        //if (clause.Value is IList<string> lst)
                        //{
                        //    values = lst;
                        //}
                        var type = clause.Value.GetType();
                        if (type != typeof(string) && (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type)))
                        {
                            foreach (object v in (dynamic)clause.Value)
                            {
                                if (!values.Contains(v))
                                    values.Add(v);
                            }
                        }

                        if (clause.Value is string str)
                        {
                            values = (str ?? "").Split(",").Distinct().Select(v => (object)v.Trim()).ToList();
                        }


                        Type listType = typeof(List<>);
                        Type constructedListType = listType.MakeGenericType(prop.Type);
                        var instance = Activator.CreateInstance(constructedListType);
                        foreach (var item in values)//((string)clause.Value).Split(','))
                        {
                            //var v = clause.DataType.GetSqlValue(item);
                            if (!item.IsNullOrEmptyObject())
                            {
                                MethodInfo addMethod = constructedListType.GetMethod("Add");
                                addMethod.Invoke(instance, new object[] { clause.DataType.GetSqlValue(item) });
                            }
                        }

                        method = constructedListType.GetMethod("Contains");
                        expression = Expression.Call(Expression.Constant(instance, constructedListType), method, prop);
                        break;
                    case EnumOperator.StartsWith:

                        method = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
                        if (prop.Type == typeof(string))
                        {
                            expression = Expression.Call(prop, method, value);
                        }
                        else
                        {
                            expression = Expression.Call(propExpression, method, value);
                        }
                        break;
                    case EnumOperator.NotStartsWith:

                        method = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
                        if (prop.Type == typeof(string))
                        {
                            expression = Expression.Not(Expression.Call(prop, method, value));
                        }
                        else
                        {
                            expression = Expression.Not(Expression.Call(propExpression, method, value));
                        }
                        break;
                    case EnumOperator.EndsWith:
                        method = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
                        if (prop.Type == typeof(string))
                        {
                            expression = Expression.Call(prop, method, value);
                        }
                        else
                        {
                            expression = Expression.Call(propExpression, method, value);
                        }
                        break;
                    case EnumOperator.NotEndsWith:
                        method = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
                        if (prop.Type == typeof(string))
                        {
                            expression = Expression.Not(Expression.Call(prop, method, value));
                        }
                        else
                        {
                            expression = Expression.Not(Expression.Call(propExpression, method, value));
                        }
                        break;
                    case EnumOperator.GreaterOrEqual:
                        expression = Expression.GreaterThanOrEqual(prop, value);
                        if (clause.DataType == EnumDataType.Date)
                        {
                            expression = Expression.GreaterThanOrEqual(prop, fromDate);
                        }
                        break;
                    case EnumOperator.LessThanOrEqual:
                        expression = Expression.LessThanOrEqual(prop, value);
                        if (clause.DataType == EnumDataType.Date)
                        {
                            expression = Expression.LessThanOrEqual(prop, toDate);
                        }
                        break;
                    case EnumOperator.Greater:
                        expression = Expression.GreaterThan(prop, value);
                        if (clause.DataType == EnumDataType.Date)
                        {
                            expression = Expression.GreaterThan(prop, toDate);
                        }
                        break;
                    case EnumOperator.LessThan:
                        expression = Expression.LessThan(prop, value);
                        if (clause.DataType == EnumDataType.Date)
                        {
                            expression = Expression.LessThan(prop, fromDate);
                        }
                        break;
                    default:
                        expression = Expression.Constant(true);
                        break;
                }
            }
            return expression;
        }

        //public static IOrderedQueryable<TSource> InternalOrderBy<TSource>(this IQueryable<TSource> query, string propertyName, bool asc) {
        //    return asc ? BuildOrderBy(query, "OrderBy", propertyName) : BuildOrderBy(query, "OrderByDescending", propertyName);
        //}

        ///// <summary>
        ///// 
        ///// Ref links: https://entityframework.net/knowledge-base/31955025/generate-ef-orderby-expression-by-string 
        ///// </summary>
        ///// <typeparam name="TSource"></typeparam>
        ///// <param name="query"></param>
        ///// <param name="linqMethod"></param>
        ///// <param name="propertyName"></param>
        ///// <returns></returns>
        //private static IOrderedQueryable<TSource> BuildOrderBy<TSource>(IQueryable<TSource> query, string linqMethod, string propertyName) {
        //    var entityType = typeof(TSource);

        //    //Create x=>x.PropName
        //    ParameterExpression arg = Expression.Parameter(entityType, "s");
        //    MemberExpression property = Expression.Property(arg, propertyName);
        //    var selector = Expression.Lambda(property, new ParameterExpression[] { arg });

        //    //Get System.Linq.Queryable.OrderBy() method.
        //    var enumarableType = typeof(System.Linq.Queryable);
        //    var method = enumarableType.GetMethods()
        //         .Where(m => m.Name == linqMethod && m.IsGenericMethodDefinition)
        //         .Where(m => {
        //             var parameters = m.GetParameters().ToList();
        //             //Put more restriction here to ensure selecting the right overload                
        //             return parameters.Count == 2;//overload that has 2 parameters
        //         }).Single();
        //    //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
        //    MethodInfo genericMethod = method
        //         .MakeGenericMethod(entityType, property.Type);

        //    /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
        //      Note that we pass the selector as Expression to the method and we don't compile it.
        //      By doing so EF can extract "order by" columns and generate SQL for it.*/
        //    var newQuery = (IOrderedQueryable<TSource>)genericMethod
        //         .Invoke(genericMethod, new object[] { query, selector });
        //    return newQuery;
        //}
    }
}
