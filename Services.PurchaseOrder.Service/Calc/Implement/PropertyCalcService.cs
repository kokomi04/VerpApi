using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.PurchaseOrder.Calc.PropertyCalc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using static Verp.Resources.PurchaseOrder.Calc.PropertyCalc.PropertyCalcValidationMessage;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PropertyCalcService : IPropertyCalcService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IPropertyHelperService _propertyHelperService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _propertyCalcActivityLog;


        public PropertyCalcService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IActivityLogService activityLogService
           , ICustomGenCodeHelperService customGenCodeHelperService
            , IPropertyHelperService propertyHelperService
            , IMapper mapper
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _customGenCodeHelperService = customGenCodeHelperService;
            _propertyHelperService = propertyHelperService;
            _mapper = mapper;
            _propertyCalcActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PropertyCalc);
        }

        public async Task<PageData<PropertyCalcListModel>> GetList(string keyword, ArrayClause filter, int page, int size, string sortBy, bool? asc = true)
        {
            keyword = (keyword ?? "").Trim();

            var query = from c in _purchaseOrderDBContext.PropertyCalc
                        join d in _purchaseOrderDBContext.PropertyCalcProduct on c.PropertyCalcId equals d.PropertyCalcId
                        join p in _purchaseOrderDBContext.RefProduct on d.ProductId equals p.ProductId
                        join o in _purchaseOrderDBContext.PropertyCalcProductOrderGroup on d.PropertyCalcProductId equals o.PropertyCalcProductId into os
                        from o in os.DefaultIfEmpty()
                        join r in _purchaseOrderDBContext.PurchaseOrder on c.PropertyCalcId equals r.PropertyCalcId into rs
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
                            PurchaseOrderId = r == null ? (long?)null : r.PurchaseOrderId,
                            IsPurchaseOrderIdCreated = r != null
                            //  PurchasingRequestCode = r == null ? null : r.PurchasingRequestCode
                        };
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(c => c.PropertyCalcCode.Contains(keyword)
                 || c.Title.Contains(keyword)
                 || c.ProductCode.Contains(keyword)
                 || c.ProductName.Contains(keyword)
                 || c.OrderCodes.Contains(keyword)
                );

            query = query.InternalFilter(filter);
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = query.InternalOrderBy(sortBy, asc.HasValue ? asc.Value : true);
            }

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
                    IsPurchaseOrderIdCreated = d.IsPurchaseOrderIdCreated,
                    PurchaseOrderId = d.PurchaseOrderId,
                    //PurchasingRequestCode = d.PurchasingRequestCode
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
            var properties = await _propertyHelperService.GetByIds(req.Properties?.Select(p => p.PropertyId)?.ToList());
            var propertiesName = string.Join(", ", properties);

            var ctx = await GenerateCode(null, req);
            await Validate(null, req);

            var entity = _mapper.Map<PropertyCalc>(req);
            await _purchaseOrderDBContext.PropertyCalc.AddAsync(entity);
            await _purchaseOrderDBContext.SaveChangesAsync();

            await ctx.ConfirmCode();

            await _propertyCalcActivityLog.LogBuilder(() => PropertyCalcActivityLogMessage.Create)
               .MessageResourceFormatDatas(propertiesName, entity.PropertyCalcCode)
               .ObjectId(entity.PropertyCalcId)
               .JsonData(req.JsonSerialize())
               .CreateLog();

            return entity.PropertyCalcId;
        }

        public async Task<PropertyCalcModel> Info(long propertyCalcId)
        {
            var entity = await GetEntityIncludes(propertyCalcId);
            if (entity == null)
                throw PropertyCalcNotFound.BadRequest();

            var poInfo = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(r => r.PropertyCalcId == propertyCalcId);

            var info = _mapper.Map<PropertyCalcModel>(entity);

            foreach (var item in info.Summary)
            {
                // Nếu là chi tiết
                if (item.PropertyId > 0)
                {
                    item.CuttingQuantity = info.CuttingWorkSheet.SelectMany(s => s.CuttingWorkSheetDest).Where(d => d.ProductId == item.MaterialProductId).Sum(d => d.ProductQuantity);
                }
                else // Nếu là NVL
                {
                    item.CuttingQuantity = info.CuttingWorkSheet.Where(d => d.InputProductId == item.MaterialProductId).Sum(d => d.InputQuantity);
                }
            }

            info.PurchaseOrderId = poInfo?.PurchaseOrderId;
            return info;
        }

        public async Task<bool> Update(long propertyCalcId, PropertyCalcModel req)
        {
            var properties = await _propertyHelperService.GetByIds(req.Properties?.Select(p => p.PropertyId)?.ToList());
            var propertiesName = string.Join(", ", properties);

            var entity = await GetEntityIncludes(propertyCalcId);
            if (entity == null)
                throw PropertyCalcNotFound.BadRequest();
            if (req.UpdatedDatetimeUtc != entity.UpdatedDatetimeUtc.GetUnix())
            {
                throw GeneralCode.DataIsOld.BadRequest();
            }

            await Validate(propertyCalcId, req);
            _purchaseOrderDBContext.PropertyCalcProperty.RemoveRange(entity.PropertyCalcProperty);
            _purchaseOrderDBContext.PropertyCalcProductOrder.RemoveRange(entity.PropertyCalcProduct.SelectMany(p => p.PropertyCalcProductOrder));
            _purchaseOrderDBContext.PropertyCalcProductDetail.RemoveRange(entity.PropertyCalcProduct.SelectMany(p => p.PropertyCalcProductDetail));
            _purchaseOrderDBContext.PropertyCalcProduct.RemoveRange(entity.PropertyCalcProduct);
            _purchaseOrderDBContext.PropertyCalcSummary.RemoveRange(entity.PropertyCalcSummary);
            _purchaseOrderDBContext.CuttingWorkSheetDest.RemoveRange(entity.CuttingWorkSheet.SelectMany(p => p.CuttingWorkSheetDest));
            _purchaseOrderDBContext.CuttingWorkSheetFile.RemoveRange(entity.CuttingWorkSheet.SelectMany(p => p.CuttingWorkSheetFile));
            _purchaseOrderDBContext.CuttingExcessMaterial.RemoveRange(entity.CuttingWorkSheet.SelectMany(p => p.CuttingExcessMaterial));
            _purchaseOrderDBContext.CuttingWorkSheet.RemoveRange(entity.CuttingWorkSheet);
            _mapper.Map(req, entity);

            if (_purchaseOrderDBContext.HasChanges())
                entity.UpdatedDatetimeUtc = DateTime.UtcNow;

            await _purchaseOrderDBContext.SaveChangesAsync();

            await _propertyCalcActivityLog.LogBuilder(() => PropertyCalcActivityLogMessage.Update)
             .MessageResourceFormatDatas(propertiesName, entity.PropertyCalcCode)
             .ObjectId(entity.PropertyCalcId)
             .JsonData(req.JsonSerialize())
             .CreateLog();

            return true;
        }

        public async Task<bool> Delete(long propertyCalcId)
        {
            var entity = await GetEntityIncludes(propertyCalcId);
            if (entity == null)
                throw PropertyCalcNotFound.BadRequest();

            var properties = await _propertyHelperService.GetByIds(entity.PropertyCalcProperty?.Select(p => p.PropertyId)?.ToList());
            var propertiesName = string.Join(", ", properties);

            entity.IsDeleted = true;
            await _purchaseOrderDBContext.SaveChangesAsync();

            await _propertyCalcActivityLog.LogBuilder(() => PropertyCalcActivityLogMessage.Delete)
           .MessageResourceFormatDatas(propertiesName, entity.PropertyCalcCode)
           .ObjectId(entity.PropertyCalcId)
           .JsonData(entity.JsonSerialize())
           .CreateLog();
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
              .Include(s => s.CuttingWorkSheet)
              .ThenInclude(cs => cs.CuttingWorkSheetDest)
              .Include(s => s.CuttingWorkSheet)
              .ThenInclude(cs => cs.CuttingExcessMaterial)
              .Include(s => s.CuttingWorkSheet)
              .ThenInclude(cs => cs.CuttingWorkSheetFile)
              .FirstOrDefaultAsync(c => c.PropertyCalcId == propertyCalcId);
        }

        private async Task Validate(long? propertyCalcId, PropertyCalcModel model)
        {
            if (propertyCalcId > 0 && string.IsNullOrWhiteSpace(model.PropertyCalcCode))
            {
                throw PropertyCalcCodeEmpty.BadRequest();
            }
            model.PropertyCalcCode = (model.PropertyCalcCode ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(model.PropertyCalcCode))
            {
                if (await _purchaseOrderDBContext.PropertyCalc.AnyAsync(s => s.PropertyCalcId != propertyCalcId && s.PropertyCalcCode == model.PropertyCalcCode))
                {
                    throw PropertyCodeAlreadyExist.BadRequest();
                }
            }
            if (model.CuttingWorkSheet.Any(s => s.CuttingWorkSheetDest.GroupBy(d => d.ProductId).Any(g => g.Count() > 1)))
            {
                throw DuplicatedOutputCutting.BadRequest();
            }
            if (model.CuttingWorkSheet.Any(s => s.CuttingExcessMaterial.Any(m => !m.ProductId.HasValue && string.IsNullOrEmpty(m.ExcessMaterial))))
            {
                throw ExcessMaterialCuttingNameMustBeNotEmpty.BadRequest();
            }
            if (model.CuttingWorkSheet.Any(s => s.CuttingExcessMaterial.Where(m => !m.ProductId.HasValue).GroupBy(m => m.ExcessMaterial).Any(g => g.Count() > 1))
                || model.CuttingWorkSheet.Any(s => s.CuttingExcessMaterial.Where(m => m.ProductId.HasValue).GroupBy(m => m.ProductId.Value).Any(g => g.Count() > 1)))
            {
                throw DuplicatedExcessMaterialCuttingNameMustBeNotEmpty.BadRequest();
            }
        }

        private async Task<IGenerateCodeContext> GenerateCode(long? propertyCalcId, PropertyCalcModel model)
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
    }
}
