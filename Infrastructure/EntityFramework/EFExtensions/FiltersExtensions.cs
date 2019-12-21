using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace VErp.Infrastructure.EF.EFExtensions
{

    public class FilterExpressionBuilder
    {
        private Type _type;
        private readonly List<Expression> _expressions;
        private readonly ParameterExpression _tableParam;
        public FilterExpressionBuilder(Type type)
        {
            _type = type;
            _tableParam = Expression.Parameter(type, "x");
            _expressions = new List<Expression>();
        }

        public FilterExpressionBuilder AddFilter(string field, Expression value)
        {
            var prop = Expression.Property(_tableParam, field);
            try
            {
                _expressions.Add(Expression.Equal(prop, value));
            }
            catch (Exception)
            {
                throw;
            }
            return this;
        }

        public FilterExpressionBuilder AddFilterListContains<T>(string field, Expression values)
        {
            var methodInfo = typeof(List<T>).GetMethod("Contains", new Type[] { typeof(T) });
            var prop = Expression.Property(_tableParam, field);
            _expressions.Add(Expression.Call(values, methodInfo, prop));
            return this;
        }

        public LambdaExpression Build()
        {
            if (_expressions.Count > 0)
            {
                var ex = _expressions[0];
                for (var i = 1; i < _expressions.Count; i++)
                {
                    ex = Expression.AndAlso(ex, _expressions[i]);
                }
                var delegateType = typeof(Func<,>).MakeGenericType(_type, typeof(bool));

                return Expression.Lambda(delegateType, ex, _tableParam);
            }
            return null;
        }
    }

}
