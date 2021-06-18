using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Config;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Commons.Library;
using VErp.Commons.Enums.StandardEnum;
using Microsoft.EntityFrameworkCore;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class MaterialCalcService : IMaterialCalcService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IAsyncRunnerService _asyncRunner;
        private readonly ICurrentContextService _currentContext;
        private readonly IObjectGenCodeService _objectGenCodeService;
        private readonly IProductHelperService _productHelperService;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IMapper _mapper;
        public MaterialCalcService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PurchasingSuggestService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           , ICurrentContextService currentContext
           , IObjectGenCodeService objectGenCodeService
           , IProductHelperService productHelperService
           , ICustomGenCodeHelperService customGenCodeHelperService
            , IMapper mapper
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _asyncRunner = asyncRunner;
            _currentContext = currentContext;
            _objectGenCodeService = objectGenCodeService;
            _productHelperService = productHelperService;
            _customGenCodeHelperService = customGenCodeHelperService;
            _mapper = mapper;
        }

        public async Task<long> Create(MaterialCalcModel req)
        {
            var entity = _mapper.Map<MaterialCalc>(req);
            await _purchaseOrderDBContext.MaterialCalc.AddAsync(entity);
            await _purchaseOrderDBContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.MaterialCalc, entity.MaterialCalcId, $"Thêm mới tính nhu cầu VT {req.MaterialCalcCode}", req.JsonSerialize());
            return entity.MaterialCalcId;
        }


        public async Task<MaterialCalcModel> Info(long materialCalcId)
        {
            var entity = await GetEntityIncludes(materialCalcId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng tính");

            return _mapper.Map<MaterialCalcModel>(entity);
        }

        public async Task<bool> Update(long materialCalcId, MaterialCalcModel req)
        {
            var entity = await GetEntityIncludes(materialCalcId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng tính");

            _mapper.Map(req, entity);
            await _purchaseOrderDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.MaterialCalc, entity.MaterialCalcId, $"Cập nhật tính nhu cầu VT {req.MaterialCalcCode}", req.JsonSerialize());

            return true;
        }

        public async Task<bool> Delete(long materialCalcId)
        {
            var entity = await GetEntityIncludes(materialCalcId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng tính");

            entity.IsDeleted = true;
            await _purchaseOrderDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.MaterialCalc, entity.MaterialCalcId, $"Xóa tính nhu cầu VT {entity.MaterialCalcCode}", new { materialCalcId }.JsonSerialize());

            return true;
        }

        private Task<MaterialCalc> GetEntityIncludes(long materialCalcId)
        {
            return _purchaseOrderDBContext.MaterialCalc
              .Include(c => c.MaterialCalcProduct)
              .ThenInclude(s => s.MaterialCalcProductDetail)
              .Include(s => s.MaterialCalcProduct)
              .ThenInclude(s => s.MaterialCalcProductOrder)
              .Include(s => s.MaterialCalcSummary)
              .FirstOrDefaultAsync(c => c.MaterialCalcId == materialCalcId);
        }
    }
}
