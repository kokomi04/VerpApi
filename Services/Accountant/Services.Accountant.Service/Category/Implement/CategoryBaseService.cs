using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public CategoryBaseService(AccountingDBContext accountingContext)
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

        protected IQueryable<int> FillterProcess(IQueryable<CategoryRowValueModel> query, FilterModel[] filters)
        {
            IQueryable<int> rowIds = null;

            foreach (var filter in filters)
            {
                IQueryable<int> filterIds = null;
                switch (filter.Operator)
                {
                    case EnumOperator.Equal:
                        filterIds = query
                            .Where(v => v.CategoryFieldId == filter.CategoryFieldId && v.Value == filter.Values[0])
                            .GroupBy(v => v.CategoryRowId)
                            .Select(g => g.Key);
                        break;
                    case EnumOperator.NotEqual:
                        filterIds = query
                            .Where(v => v.CategoryFieldId == filter.CategoryFieldId && v.Value != filter.Values[0])
                            .GroupBy(v => v.CategoryRowId)
                            .Select(g => g.Key);
                        break;
                    case EnumOperator.Contains:
                        filterIds = query
                            .Where(v => v.CategoryFieldId == filter.CategoryFieldId && v.Value.Contains(filter.Values[0]))
                            .GroupBy(v => v.CategoryRowId)
                            .Select(g => g.Key);
                        break;
                    case EnumOperator.InList:
                        string[] values = filter.Values[0].Split(',');
                        filterIds = query
                            .Where(v => v.CategoryFieldId == filter.CategoryFieldId && values.Contains(v.Value))
                            .GroupBy(v => v.CategoryRowId)
                            .Select(g => g.Key);
                        break;
                    case EnumOperator.IsLeafNode:
                        List<string> nodeValues = query
                            .Where(v => v.CategoryFieldId == filter.CategoryFieldId)
                            .Select(v => v.Value).ToList();
                        List<string> isLeafValues = nodeValues.Where(v => !nodeValues.Any(n => n != v && n.Contains(v))).ToList();
                        filterIds = query
                            .Where(v => v.CategoryFieldId == filter.CategoryFieldId && isLeafValues.Contains(v.Value))
                            .GroupBy(v => v.CategoryRowId)
                            .Select(g => g.Key).AsQueryable();
                        break;
                    default:
                        break;
                }

                if (rowIds == null)
                {
                    rowIds = filterIds;
                }
                else if (filter != null)
                {
                    rowIds = rowIds.Join(filterIds, r => r, f => f, (r, f) => r);
                }
            }

            return rowIds;
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
