using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class CategoryRowService : ICategoryRowService
    {
        private readonly AccountingDBContext _accountingContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        public CategoryRowService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryRowService> logger
            , IActivityLogService activityLogService
             , IMapper mapper
            )
        {
            _accountingContext = accountingContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        
        public async Task<ServiceResult<int>> AddCategoryRow(int updatedUserId, int categoryId, CategoryRowInputModel data)
        {
            // Validate
            // TODO
      

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Thêm dòng
                    var categoryRow = new CategoryRow
                    {
                        CategoryId = categoryId
                    };
                    await _accountingContext.CategoryRow.AddAsync(categoryRow);
                    await _accountingContext.SaveChangesAsync();

                    // Duyệt danh sách field
                    foreach(var item in data.Values)
                    {
                        // Thêm value
                        var categoryValue = _mapper.Map<CategoryValue>(item);
                        await _accountingContext.CategoryRow.AddAsync(categoryRow);
                        await _accountingContext.SaveChangesAsync();
                        // Thêm mapping

                        var categoryRowValue = new CategoryRowValue
                        {
                            CategoryRowId = categoryRow.CategoryRowId,
                            CategoryValueId = categoryValue.CategoryValueId,
                        };
                        await _accountingContext.CategoryRowValue.AddAsync(categoryRowValue);
                        await _accountingContext.SaveChangesAsync();
                    }
                   
                    trans.Commit();
                    //await _activityLogService.CreateLog(EnumObjectType.Category, categoryField.CategoryFieldId, $"Thêm trường danh mục {categoryField.Title}", data.JsonSerialize());
                    return 1;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

    }
}
