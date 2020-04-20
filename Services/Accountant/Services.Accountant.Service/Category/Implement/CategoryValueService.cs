using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    public class CategoryValueService : CategoryBaseService, ICategoryValueService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        public CategoryValueService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryValueService> logger
            , IActivityLogService activityLogService
             , IMapper mapper
            ) : base(accountingContext)
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }


        public async Task<PageData<CategoryReferenceValueModel>> GetReferenceValues(int categoryId, int categoryFieldId, string keyword, FilterModel[] filters, int page, int size)
        {
            var field = await _accountingContext.CategoryField.FirstOrDefaultAsync(f => f.CategoryFieldId == categoryFieldId);
            IQueryable<CategoryReferenceValueModel> query;
            List<CategoryReferenceValueModel> lst = new List<CategoryReferenceValueModel>();
            int total = 0;
            if (field.ReferenceCategoryFieldId.HasValue)
            {
                CategoryField referField = _accountingContext.CategoryField.First(f => f.CategoryFieldId == field.ReferenceCategoryFieldId.Value);

                IQueryable<CategoryRow> tempQuery = _accountingContext.CategoryRow
                    .Where(r => r.CategoryId == referField.CategoryId)
                    .Include(r => r.CategoryRowValues)
                    .ThenInclude(rv => rv.SourceCategoryRowValue)
                    .Include(r => r.CategoryRowValues)
                    .ThenInclude(rv => rv.CategoryField);

                if (!string.IsNullOrEmpty(field.Filters))
                {
                    FillterProcess(ref tempQuery, filters);
                }

                query = tempQuery
                  .Select(r => new CategoryReferenceValueModel
                  {
                      CategoryFieldId = field.ReferenceCategoryFieldId.Value,
                      CategoryValueId = r.CategoryRowValues.FirstOrDefault(rv => rv.CategoryFieldId == field.ReferenceCategoryFieldId.Value).CategoryRowValueId,
                      Value = r.CategoryRowValues.FirstOrDefault(rv => rv.CategoryFieldId == field.ReferenceCategoryFieldId.Value).Value,
                      Title = field.ReferenceCategoryTitleFieldId.HasValue
                      ? r.CategoryRowValues.FirstOrDefault(rv => rv.CategoryFieldId == field.ReferenceCategoryTitleFieldId.Value).Value
                      : r.CategoryRowValues.FirstOrDefault(rv => rv.CategoryFieldId == field.ReferenceCategoryFieldId.Value).Value
                  });

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(v => v.Value.Contains(keyword));
                }
                total = await query.CountAsync();
                if (size > 0)
                {
                    query = query.Skip((page - 1) * size).Take(size);
                }
                lst = query.ToList();
            }
            return (lst, total);
        }
    }
}
