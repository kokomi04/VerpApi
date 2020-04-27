﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
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

namespace VErp.Services.Accountant.Service.Category.Implement
{
    public abstract class CategoryBaseService
    {
        protected readonly AccountingDBContext _accountingContext;
        private delegate Expression<Func<T, bool>> LogicOperator<T>(Expression<Func<T, bool>> expr);
        protected CategoryBaseService(AccountingDBContext accountingContext)
        {
            _accountingContext = accountingContext;
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
                            predicate = logic(r => r.CategoryRowValues.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.SourceCategoryRowValue.Value == filter.Values[0] : rv.Value == filter.Values[0]));
                            break;
                        case EnumOperator.NotEqual:
                            predicate = logic(r => r.CategoryRowValues.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.SourceCategoryRowValue.Value != filter.Values[0] : rv.Value != filter.Values[0]));
                            break;
                        case EnumOperator.Contains:
                            predicate = logic(r => r.CategoryRowValues.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.SourceCategoryRowValue.Value.Contains(filter.Values[0]) : rv.Value.Contains(filter.Values[0])));
                            break;
                        case EnumOperator.InList:
                            string[] values = filter.Values[0].Split(',');
                            predicate = logic(r => r.CategoryRowValues.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? values.Contains(rv.SourceCategoryRowValue.Value) : values.Contains(rv.Value)));
                            break;
                        case EnumOperator.IsLeafNode:
                            List<string> nodeValues = query
                                .Select(r => r.CategoryRowValues.FirstOrDefault(v => v.CategoryFieldId == filter.CategoryFieldId))
                                .Select(rv => (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.SourceCategoryRowValue.Value : rv.Value)
                                .ToList();

                            List<string> isLeafValues = nodeValues.Where(v => !nodeValues.Any(n => n != v && n.Contains(v))).ToList();
                            predicate = logic(r => r.CategoryRowValues.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? isLeafValues.Contains(rv.SourceCategoryRowValue.Value) : isLeafValues.Contains(rv.Value)));
                            break;
                        case EnumOperator.StartsWith:
                            predicate = logic(r => r.CategoryRowValues.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.SourceCategoryRowValue.Value.StartsWith(filter.Values[0]) : rv.Value.StartsWith(filter.Values[0])));
                            break;
                        case EnumOperator.EndsWith:
                            predicate = logic(r => r.CategoryRowValues.Any(rv => rv.CategoryFieldId == filter.CategoryFieldId && (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) ? rv.SourceCategoryRowValue.Value.EndsWith(filter.Values[0]) : rv.Value.EndsWith(filter.Values[0])));
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

        protected private Enum CheckValue(CategoryValueModel valueItem, CategoryField field)
        {
            if (field.DataSize > 0 && valueItem.Value.Length > field.DataSize)
            {
                return CategoryErrorCode.CategoryValueInValid;
            }

            if (!string.IsNullOrEmpty(field.DataType.RegularExpression) && !Regex.IsMatch(valueItem.Value, field.DataType.RegularExpression))
            {
                return CategoryErrorCode.CategoryValueInValid;
            }

            if (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(valueItem.Value, field.RegularExpression))
            {
                return CategoryErrorCode.CategoryValueInValid;
            }

            return GeneralCode.Success;
        }
    }
}
