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
using AutoMapper.QueryableExtensions;
using Verp.Cache.RedisCache;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PropertyCalcService : IPropertyCalcService
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
        public PropertyCalcService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PropertyCalcService> logger
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

        public async Task<PageData<PropertyCalcListModel>> GetList(string keyword, ArrayClause filter, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            
            var query = from c in _purchaseOrderDBContext.PropertyCalc
                        join d in _purchaseOrderDBContext.PropertyCalcProduct on c.PropertyCalcId equals d.PropertyCalcId
                        join p in _purchaseOrderDBContext.RefProduct on d.ProductId equals p.ProductId
                        join o in _purchaseOrderDBContext.PropertyCalcProductOrderGroup on d.PropertyCalcProductId equals o.PropertyCalcProductId into os
                        from o in os.DefaultIfEmpty()
                        join r in _purchaseOrderDBContext.PurchasingRequest on c.PropertyCalcId equals r.PropertyCalcId into rs
                        from r in rs.DefaultIfEmpty()
                        select new
                        {
                            c.PropertyCalcId,
                            c.PropertyCalcCode,
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
                query = query.Where(c => c.PropertyCalcCode.Contains(keyword)
                 || c.Title.Contains(keyword)
                 || c.ProductCode.Contains(keyword)
                 || c.ProductName.Contains(keyword)
                 || c.OrderCodes.Contains(keyword)
                );

            query = query.InternalFilter(filter);

            var total = await query.CountAsync();
            var paged = (await query.Skip((page - 1) * size).Take(size).ToListAsync())
                .Select(d => new PropertyCalcListModel()
                {
                    PropertyCalcId = d.PropertyCalcId,
                    PropertyCalcCode = d.PropertyCalcCode,
                    Title = d.Title,
                    CreatedByUserId = d.CreatedByUserId,
                    CreatedDatetimeUtc = d.CreatedDatetimeUtc.GetUnix(),
                    ProductId = d.ProductId,
                    ProductCode = d.ProductCode,
                    ProductName = d.ProductName,
                    OrderCodes = d.OrderCodes,
                    TotalOrderProductQuantity = d.TotalOrderProductQuantity,
                    IsPurchasingRequestCreated = d.PurchasingRequestId > 0,
                    PurchasingRequestId = d.PurchasingRequestId,
                    PurchasingRequestCode = d.PurchasingRequestCode
                }).ToList();
            return (paged, total);
        }

        public async IAsyncEnumerable<PropertyOrderProductHistory> GetHistoryProductOrderList(IList<int> productIds, IList<string> orderCodes)
        {
            var lst = await (
                from c in _purchaseOrderDBContext.PropertyCalc
                join d in _purchaseOrderDBContext.PropertyCalcProduct on c.PropertyCalcId equals d.PropertyCalcId
                join o in _purchaseOrderDBContext.PropertyCalcProductOrder on d.PropertyCalcProductId equals o.PropertyCalcProductId
                where productIds.Contains(d.ProductId) && orderCodes.Contains(o.OrderCode)
                select new
                {
                    c.PropertyCalcId,
                    c.PropertyCalcCode,
                    c.Title,
                    d.ProductId,
                    o.OrderCode,
                    o.OrderProductQuantity
                }).ToListAsync();

            var propertyCalcIds = lst.Select(c => c.PropertyCalcId).ToList();

            var groups = await _purchaseOrderDBContext.PropertyCalcProperty.Where(g => propertyCalcIds.Contains(g.PropertyCalcId)).ToListAsync();

            foreach (var item in lst)
            {
                yield return new PropertyOrderProductHistory()
                {
                    PropertyCalcId = item.PropertyCalcId,
                    PropertyCalcCode = item.PropertyCalcCode,
                    Title = item.Title,
                    Properties = _mapper.Map<List<PropertyCalcPropertyModel>>(groups.Where(g => g.PropertyCalcId == item.PropertyCalcId)),

                    OrderCode = item.OrderCode,
                    ProductId = item.ProductId,
                    OrderProductQuantity = item.OrderProductQuantity,
                };
            }

        }

        public async Task<long> Create(PropertyCalcModel req)
        {
            var ctx = await GenerateCode(null, req);
            await Validate(null, req);

            var entity = _mapper.Map<PropertyCalc>(req);
            await _purchaseOrderDBContext.PropertyCalc.AddAsync(entity);
            await _purchaseOrderDBContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.PropertyCalc, entity.PropertyCalcId, $"Thêm mới tính nhu cầu VT có thuộc tính đặc biệt {req.PropertyCalcCode}", req.JsonSerialize());

            await ctx.ConfirmCode();

            return entity.PropertyCalcId;
        }

        public async Task<PropertyCalcModel> Info(long propertyCalcId)
        {
            var entity = await GetEntityIncludes(propertyCalcId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng tính");

            var requestInfo = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(r => r.PropertyCalcId == propertyCalcId);

            var info = _mapper.Map<PropertyCalcModel>(entity);
            info.PurchasingRequestId = requestInfo?.PurchasingRequestId;
            info.PurchasingRequestCode = requestInfo?.PurchasingRequestCode;
            return info;
        }

        public async Task<bool> Update(long propertyCalcId, PropertyCalcModel req)
        {
            var entity = await GetEntityIncludes(propertyCalcId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng tính");

            await Validate(propertyCalcId, req);
            _purchaseOrderDBContext.PropertyCalcProperty.RemoveRange(entity.PropertyCalcProperty);

            _purchaseOrderDBContext.PropertyCalcProductOrder.RemoveRange(entity.PropertyCalcProduct.SelectMany(p => p.PropertyCalcProductOrder));

            _purchaseOrderDBContext.PropertyCalcProductDetail.RemoveRange(entity.PropertyCalcProduct.SelectMany(p => p.PropertyCalcProductDetail));

            _purchaseOrderDBContext.PropertyCalcProduct.RemoveRange(entity.PropertyCalcProduct);

            _purchaseOrderDBContext.PropertyCalcSummary.RemoveRange(entity.PropertyCalcSummary);

            _mapper.Map(req, entity);

            await _purchaseOrderDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.PropertyCalc, entity.PropertyCalcId, $"Cập nhật tính nhu cầu VT có thuộc tính đặc biệt {req.PropertyCalcCode}", req.JsonSerialize());

            return true;
        }

        public async Task<bool> Delete(long propertyCalcId)
        {
            var entity = await GetEntityIncludes(propertyCalcId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng tính");

            entity.IsDeleted = true;
            // Xóa bảng tính cắt ván
            DeleteCuttingWorkSheet(propertyCalcId);
            await _purchaseOrderDBContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.PropertyCalc, entity.PropertyCalcId, $"Xóa tính nhu cầu VT có thuộc tính đặc biệt {entity.PropertyCalcCode}", new { propertyCalcId }.JsonSerialize());

            return true;
        }

        private Task<PropertyCalc> GetEntityIncludes(long propertyCalcId)
        {
            return _purchaseOrderDBContext.PropertyCalc
              .Include(c => c.PropertyCalcProperty)
              .Include(c => c.PropertyCalcProduct)
              .ThenInclude(s => s.PropertyCalcProductDetail)
              .Include(s => s.PropertyCalcProduct)
              .ThenInclude(s => s.PropertyCalcProductOrder)
              .Include(s => s.PropertyCalcSummary)
              .FirstOrDefaultAsync(c => c.PropertyCalcId == propertyCalcId);
        }

        private async Task Validate(long? propertyCalcId, PropertyCalcModel model)
        {
            if (propertyCalcId > 0 && string.IsNullOrWhiteSpace(model.PropertyCalcCode))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng nhập mã số");
            }
            model.PropertyCalcCode = (model.PropertyCalcCode ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(model.PropertyCalcCode))
            {
                if (await _purchaseOrderDBContext.PropertyCalc.AnyAsync(s => s.PropertyCalcId != propertyCalcId && s.PropertyCalcCode == model.PropertyCalcCode))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Mã số đã tồn tại");
                }
            }
        }

        private async Task<GenerateCodeContext> GenerateCode(long? propertyCalcId, PropertyCalcModel model)
        {
            model.PropertyCalcCode = (model.PropertyCalcCode ?? "").Trim();

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.PropertyCalc)
                .SetConfigData(propertyCalcId ?? 0, DateTime.Now.GetUnix())
                .TryValidateAndGenerateCode(_purchaseOrderDBContext.PropertyCalc, model.PropertyCalcCode, (s, code) => s.PropertyCalcId != propertyCalcId && s.PropertyCalcCode == code);

            model.PropertyCalcCode = code;

            return ctx;
        }

        public async Task<IList<CuttingWorkSheetModel>> GetCuttingWorkSheet(long propertyCalcId)
        {
            var result = await _purchaseOrderDBContext.CuttingWorkSheet
                .Include(s => s.CuttingWorkSheetDest)
                .Include(s => s.CuttingWorkSheetFile)
                .Where(s => s.PropertyCalcId == propertyCalcId)
                .ProjectTo<CuttingWorkSheetModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return result;
        }

        public async Task<IList<CuttingWorkSheetModel>> UpdateCuttingWorkSheet(long propertyCalcId, IList<CuttingWorkSheetModel> data)
        {
            var propertyCalc = _purchaseOrderDBContext.PropertyCalc.FirstOrDefault(p => p.PropertyCalcId == propertyCalcId);
            if (propertyCalc == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng tính");

            var currentWorkSheets = _purchaseOrderDBContext.CuttingWorkSheet.Where(s => s.PropertyCalcId == propertyCalcId).ToList();
            var currentWorkSheetIds = currentWorkSheets.Select(s => s.CuttingWorkSheetId).ToList();
            var currentFiles = _purchaseOrderDBContext.CuttingWorkSheetFile.Where(f => currentWorkSheetIds.Contains(f.CuttingWorkSheetId)).ToList();
            var currentDests = _purchaseOrderDBContext.CuttingWorkSheetDest.Where(d => currentWorkSheetIds.Contains(d.CuttingWorkSheetId)).ToList();
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCuttingWorkSheet(propertyCalcId));
            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                // Xoá 
                var deleteWorkSheets = currentWorkSheets.Where(s => !data.Any(d => d.CuttingWorkSheetId == s.CuttingWorkSheetId)).ToList();
                foreach (var deleteWorkSheet in deleteWorkSheets)
                {
                    deleteWorkSheet.IsDeleted = true;
                }
                var tupples = new List<Tuple<CuttingWorkSheetModel, CuttingWorkSheet>>();

                // Tạo mới/thay đổi
                foreach (var item in data)
                {
                    var currentWorkSheet = currentWorkSheets.FirstOrDefault(s => s.CuttingWorkSheetId == item.CuttingWorkSheetId);
                    if (currentWorkSheet == null)
                    {
                        currentWorkSheet = _mapper.Map<CuttingWorkSheet>(item);
                        _purchaseOrderDBContext.CuttingWorkSheet.Add(currentWorkSheet);
                    }
                    else
                    {
                        currentWorkSheet.InputProductId = item.InputProductId;
                        currentWorkSheet.InputQuantity = item.InputQuantity;
                    }

                    tupples.Add(new Tuple<CuttingWorkSheetModel, CuttingWorkSheet>(item, currentWorkSheet));
                }

                _purchaseOrderDBContext.SaveChanges();

                // Cập nhật file và thông tin đích
                foreach (var tuple in tupples)
                {
                    // Xóa dữ liệu cũ
                    var deleteFiles = currentFiles.Where(f => f.CuttingWorkSheetId == tuple.Item2.CuttingWorkSheetId).ToList();
                    _purchaseOrderDBContext.CuttingWorkSheetFile.RemoveRange(deleteFiles);
                    var deleteDests = currentDests.Where(d => d.CuttingWorkSheetId == tuple.Item2.CuttingWorkSheetId).ToList();
                    _purchaseOrderDBContext.CuttingWorkSheetDest.RemoveRange(deleteDests);

                    // Tạo dữ liệu mới
                    foreach (var file in tuple.Item1.CuttingWorkSheetFile)
                    {
                        var newFile = _mapper.Map<CuttingWorkSheetFile>(file);
                        newFile.CuttingWorkSheetId = tuple.Item2.CuttingWorkSheetId;
                        _purchaseOrderDBContext.CuttingWorkSheetFile.Add(newFile);
                    }
                    foreach (var dest in tuple.Item1.CuttingWorkSheetDest)
                    {
                        var newDest = _mapper.Map<CuttingWorkSheetDest>(dest);
                        newDest.CuttingWorkSheetId = tuple.Item2.CuttingWorkSheetId;
                        _purchaseOrderDBContext.CuttingWorkSheetDest.Add(newDest);
                    }
                }

                _purchaseOrderDBContext.SaveChanges();

                data = await _purchaseOrderDBContext.CuttingWorkSheet
                .Include(s => s.CuttingWorkSheetDest)
                .Include(s => s.CuttingWorkSheetFile)
                .Where(s => s.PropertyCalcId == propertyCalcId)
                .ProjectTo<CuttingWorkSheetModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.PropertyCalc, propertyCalcId, $"Cập nhật bảng cắt ván {propertyCalc.PropertyCalcCode}", data.JsonSerialize());
                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateCuttingWorkSheet");
                throw;
            }
        }

        private bool DeleteCuttingWorkSheet(long propertyCalcId)
        {
            var deleteWorkSheets = _purchaseOrderDBContext.CuttingWorkSheet.Where(s => s.PropertyCalcId == propertyCalcId).ToList();
            // Xoá 
            foreach (var deleteWorkSheet in deleteWorkSheets)
            {
                deleteWorkSheet.IsDeleted = true;
            }
            _purchaseOrderDBContext.SaveChanges();
            return true;
        }
    }
}
