using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

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

        public static void AddFilterBase(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {

                var filterBuilder = new FilterExpressionBuilder(entityType.ClrType);

                var isDeletedProp = entityType.FindProperty("IsDeleted");
                if (isDeletedProp != null)
                {
                    var isDeleted = Expression.Constant(false);
                    filterBuilder.AddFilter("IsDeleted", isDeleted);
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
                    if (obj.GetValue("IsDeleted") == (object)true)
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

        public static IQueryable<T> InternalFilter<T>(this IQueryable<T> query, Dictionary<string, List<string>> filters = null)
        {
            if (filters != null && filters.Count > 0)
            {
                foreach (var filter in filters)
                {
                    var sParam = Expression.Parameter(typeof(T), "s");
                    var prop = Expression.Property(sParam, filter.Key);
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(prop.Type);
                    Type listType = typeof(List<>);
                    Type constructedListType = listType.MakeGenericType(prop.Type);
                    var instance = Activator.CreateInstance(constructedListType);
                    foreach (var value in filter.Value)
                    {
                        MethodInfo method = constructedListType.GetMethod("Add");
                        method.Invoke(instance, new object[] { typeConverter.ConvertFromString(value) });
                    }
                    var methodInfo = constructedListType.GetMethod("Contains");
                    var expression = Expression.Call(Expression.Constant(instance), methodInfo, prop);
                    query = query.Where(Expression.Lambda<Func<T, bool>>(expression, sParam));
                }
            }
            return query;
        }

        public static IQueryable<T> InternalFilter<T>(this IQueryable<T> query, Clause filters = null)
        {
            if (filters != null)
            {
                var param = Expression.Parameter(typeof(T), "s");
                Expression filterExp = FilterClauseProcess<T>(param, filters, query);
                query = query.Where(Expression.Lambda<Func<T, bool>>(filterExp, param));
            }
            return query;
        }

        public static Expression FilterClauseProcess<T>(ParameterExpression param, Clause clause, IQueryable<T> query, bool not = false)
        {
            Expression exp = Expression.Constant(false);
            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;
                    exp = BuildExpression<T>(param, singleClause);
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
                            exp = FilterClauseProcess<T>(param, item, query, isNot);
                        }
                        else
                        {
                            if (isOr)
                            {
                                exp = Expression.OrElse(exp, FilterClauseProcess<T>(param, item, query, isNot));
                            }
                            else
                            {
                                exp = Expression.AndAlso(exp, FilterClauseProcess<T>(param, item, query, isNot));
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private static Expression BuildExpression<T>(ParameterExpression param, SingleClause clause)
        {
            Expression expression = null;
            if (clause != null)
            {
                var prop = Expression.Property(param, clause.FieldName);
                TypeConverter typeConverter = TypeDescriptor.GetConverter(prop.Type);
                // Check value
                ConstantExpression value;
                MethodInfo method;
                switch (clause.Operator)
                {
                    case EnumOperator.Equal:
                        value = Expression.Constant(typeConverter.ConvertFromString((string)clause.Value));
                        expression = Expression.Equal(prop, value);
                        break;
                    case EnumOperator.NotEqual:
                        value = Expression.Constant(typeConverter.ConvertFromString((string)clause.Value));
                        expression = Expression.NotEqual(prop, value);
                        break;
                    case EnumOperator.Contains:
                        value = Expression.Constant(typeConverter.ConvertFromString((string)clause.Value));
                        var toStringMethod = prop.Type.GetMethod("ToString");
                        var propExpression = Expression.Call(prop, toStringMethod);
                        method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
                        expression = Expression.Call(propExpression, method, value);
                        break;
                    case EnumOperator.InList:
                        Type listType = typeof(List<>);
                        Type constructedListType = listType.MakeGenericType(prop.Type);
                        var instance = Activator.CreateInstance(constructedListType);
                        foreach (var item in ((string)clause.Value).Split(','))
                        {
                            MethodInfo addMethod = constructedListType.GetMethod("Add");
                            addMethod.Invoke(instance, new object[] { typeConverter.ConvertFromString(item) });
                        }
                        method = constructedListType.GetMethod("Contains");
                        expression = Expression.Call(Expression.Constant(instance), method, prop);
                        break;
                    case EnumOperator.StartsWith:
                        value = Expression.Constant(typeConverter.ConvertFromString((string)clause.Value));
                        toStringMethod = prop.Type.GetMethod("ToString");
                        propExpression = Expression.Call(prop, toStringMethod);
                        method = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
                        expression = Expression.Call(propExpression, method, value);
                        break;
                    case EnumOperator.EndsWith:
                        value = Expression.Constant(typeConverter.ConvertFromString((string)clause.Value));
                        toStringMethod = prop.Type.GetMethod("ToString");
                        propExpression = Expression.Call(prop, toStringMethod);
                        method = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
                        expression = Expression.Call(propExpression, method, value);
                        break;
                    default:
                        expression = Expression.Constant(true);
                        break;
                }
            }
            return expression;
        }



    }
}
