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
