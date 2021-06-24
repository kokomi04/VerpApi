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
using VErp.Infrastructure.EF.EFExtensions;
using System.Linq;
using VErp.Infrastructure.ServiceCore.Model;

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

        public async Task<PageData<MaterialCalcListModel>> GetList(string keyword, ArrayClause filter, int page, int size)
        {
            var query = from c in _purchaseOrderDBContext.MaterialCalc
                        join d in _purchaseOrderDBContext.MaterialCalcProduct on c.MaterialCalcId equals d.MaterialCalcId
                        join p in _purchaseOrderDBContext.RefProduct on d.ProductId equals p.ProductId
                        join o in _purchaseOrderDBContext.MaterialCalcProductOrderGroup on d.MaterialCalcProductId equals o.MaterialCalcProductId into os
                        from o in os.DefaultIfEmpty()
                        join r in _purchaseOrderDBContext.PurchasingRequest on c.MaterialCalcId equals r.MaterialCalcId into rs
                        from r in rs.DefaultIfEmpty()
                        select new
                        {
                            c.MaterialCalcId,
                            c.MaterialCalcCode,
                            c.Title,
                            c.CreatedByUserId,
                            c.CreatedDatetimeUtc,
                            p.ProductId,
                            p.ProductCode,
                            p.ProductName,
                            TotalOrderProductQuantity = o == null ? null : o.TotalOrderProductQuantity,
                            OrderCodes = o == null ? null : o.OrderCodes,
                            PurchasingRequestId = r == null ? (long?)null : r.PurchasingRequestId,
                            PurchasingRequestCode = r == null ? null : r.PurchasingRequestCode
                        };
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(c => c.MaterialCalcCode.Contains(keyword)
                 || c.Title.Contains(keyword)
                 || c.ProductCode.Contains(keyword)
                 || c.ProductName.Contains(keyword)
                 || c.OrderCodes.Contains(keyword)
                );

            query = query.InternalFilter(filter);

            var total = await query.CountAsync();
            var paged = (await query.Skip((page - 1) * size).Take(size).ToListAsync())
                .Select(d => new MaterialCalcListModel()
                {

                    MaterialCalcId = d.MaterialCalcId,
                    MaterialCalcCode = d.MaterialCalcCode,
                    Title = d.Title,
                    CreatedByUserId = d.CreatedByUserId,
                    CreatedDatetimeUtc = d.CreatedDatetimeUtc.GetUnix(),
                    ProductId = d.ProductId,
                    ProductCode = d.ProductCode,
                    productName = d.ProductName,
                    OrderCodes = d.OrderCodes,
                    TotalOrderProductQuantity = d.TotalOrderProductQuantity,
                    PurchasingRequestId = d.PurchasingRequestId,
                    PurchasingRequestCode = d.PurchasingRequestCode
                }).ToList();
            return (paged, total);
        }

        public async Task<long> Create(MaterialCalcModel req)
        {
            var ctx = await GenerateCode(null, req);
            await Validate(null, req);

            var entity = _mapper.Map<MaterialCalc>(req);
            await _purchaseOrderDBContext.MaterialCalc.AddAsync(entity);
            await _purchaseOrderDBContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.MaterialCalc, entity.MaterialCalcId, $"Thêm mới tính nhu cầu VT {req.MaterialCalcCode}", req.JsonSerialize());

            await ctx.ConfirmCode();

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

            await Validate(materialCalcId, req);

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

        private async Task Validate(long? materialCalcId, MaterialCalcModel model)
        {
            if (materialCalcId > 0 && string.IsNullOrWhiteSpace(model.MaterialCalcCode))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng nhập mã số");
            }
            model.MaterialCalcCode = (model.MaterialCalcCode ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(model.MaterialCalcCode))
            {
                if (await _purchaseOrderDBContext.MaterialCalc.AnyAsync(s => s.MaterialCalcId != materialCalcId && s.MaterialCalcCode == model.MaterialCalcCode))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Mã số đã tồn tại");
                }
            }
        }

        private async Task<GenerateCodeContext> GenerateCode(long? materialCalcId, MaterialCalcModel model)
        {
            model.MaterialCalcCode = (model.MaterialCalcCode ?? "").Trim();

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.MaterialCalc)
                .SetConfigData(materialCalcId ?? 0, model.CreatedDatetimeUtc)
                .TryValidateAndGenerateCode(_purchaseOrderDBContext.MaterialCalc, model.MaterialCalcCode, (s, code) => s.MaterialCalcId != materialCalcId && s.MaterialCalcCode == code);

            model.MaterialCalcCode = code;

            return ctx;
        }
    }
}
