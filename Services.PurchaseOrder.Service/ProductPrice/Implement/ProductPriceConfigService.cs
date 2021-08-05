using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Config;
using VErp.Services.PurchaseOrder.Model.ProductPrice;
using VErp.Services.PurchaseOrder.Service.Resources;
using VErp.Commons.Library;

namespace VErp.Services.PurchaseOrder.Service.ProductPrice.Implement
{
    public class ProductPriceConfigService : IProductPriceConfigService
    {

        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IAsyncRunnerService _asyncRunner;
        private readonly ICurrentContextService _currentContext;
        private readonly IObjectGenCodeService _objectGenCodeService;
        private readonly IPurchasingSuggestService _purchasingSuggestService;
        private readonly IProductHelperService _productHelperService;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _objectActivityLog;

        public ProductPriceConfigService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<ProductPriceConfigService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           , ICurrentContextService currentContext
           , IObjectGenCodeService objectGenCodeService
           , IPurchasingSuggestService purchasingSuggestService
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
            _purchasingSuggestService = purchasingSuggestService;
            _productHelperService = productHelperService;
            _customGenCodeHelperService = customGenCodeHelperService;
            _mapper = mapper;
            _objectActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductPriceConfig);
        }

        public async Task<IList<ProductPriceConfigVersionModel>> GetList(bool? isActived)
        {
            var query = from c in _purchaseOrderDBContext.ProductPriceConfig
                        join v in _purchaseOrderDBContext.ProductPriceConfigVersion on c.LastestProductPriceConfigVersionId equals v.ProductPriceConfigVersionId
                        where isActived == null || c.IsActived == isActived
                        select new { Version = v, c.IsActived };
            var lst = await query.ToListAsync();
            var data = new List<ProductPriceConfigVersionModel>();
            foreach (var item in lst)
            {
                var dataItem = _mapper.Map<ProductPriceConfigVersionModel>(item.Version);
                dataItem.IsActived = item.IsActived;
                data.Add(dataItem);
            }

            return data;
        }

        public async Task<ProductPriceConfigVersionModel> LastestVersionInfo(int productPriceConfigId)
        {
            var info = await _purchaseOrderDBContext.ProductPriceConfig
                .FirstOrDefaultAsync(c => c.ProductPriceConfigId == productPriceConfigId);

            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            return await VersionInfo(info.LastestProductPriceConfigVersionId);
        }

        public async Task<ProductPriceConfigVersionModel> VersionInfo(int productPriceConfigVersionId)
        {

            var query = from c in _purchaseOrderDBContext.ProductPriceConfig
                        join v in _purchaseOrderDBContext.ProductPriceConfigVersion on c.ProductPriceConfigId equals v.ProductPriceConfigId
                        where c.LastestProductPriceConfigVersionId == productPriceConfigVersionId
                        select new { Version = v, c.IsActived };
            var data = await query.FirstOrDefaultAsync();
            if (data == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            var model = _mapper.Map<ProductPriceConfigVersionModel>(data.Version);
            model.IsActived = data.IsActived;

            var items = await _purchaseOrderDBContext.ProductPriceConfigItem
                .Where(c => c.ProductPriceConfigVersionId == productPriceConfigVersionId)
                .ProjectTo<ProductPriceConfigItemModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            model.Items = items;
            return model;
        }

        public async Task<int> Create(ProductPriceConfigVersionModel model)
        {
            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();

            var cfg = new ProductPriceConfig() { IsActived = model.IsActived };
            await _purchaseOrderDBContext.ProductPriceConfig.AddAsync(cfg);
            await _purchaseOrderDBContext.SaveChangesAsync();

            var versionId = await CreateVersion(cfg.ProductPriceConfigId, model);

            cfg.LastestProductPriceConfigVersionId = versionId;

            await _purchaseOrderDBContext.SaveChangesAsync();

            await trans.CommitAsync();

            await _objectActivityLog.LogBuilder(() => ProductPriceConfigActivityLogMessage.Created)
                .MessageResourceFormatDatas(model.Title)
                .ObjectId(cfg.ProductPriceConfigId)
                .JsonData(model.JsonSerialize())
                .CreateLog();

            return cfg.ProductPriceConfigId;
        }

        public async Task<int> Update(int productPriceConfigId, ProductPriceConfigVersionModel model)
        {
            var info = await _purchaseOrderDBContext.ProductPriceConfig
                 .FirstOrDefaultAsync(c => c.ProductPriceConfigId == productPriceConfigId);

            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            info.IsActived = model.IsActived;

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();

            var versionId = await CreateVersion(info.ProductPriceConfigId, model);

            info.LastestProductPriceConfigVersionId = versionId;

            await _purchaseOrderDBContext.SaveChangesAsync();

            await trans.CommitAsync();

            await _objectActivityLog.LogBuilder(() => ProductPriceConfigActivityLogMessage.Updated)
              .MessageResourceFormatDatas(model.Title)
              .ObjectId(productPriceConfigId)
              .JsonData(model.JsonSerialize())
              .CreateLog();

            return info.ProductPriceConfigId;
        }

        public async Task<bool> Delete(int productPriceConfigId)
        {
            var info = await _purchaseOrderDBContext.ProductPriceConfig
                .FirstOrDefaultAsync(c => c.ProductPriceConfigId == productPriceConfigId);

            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();

            info.IsDeleted = true;

            var versions = await _purchaseOrderDBContext.ProductPriceConfigVersion.Where(v => v.ProductPriceConfigId == productPriceConfigId).ToListAsync();

            var lastestVersionInfo = versions.FirstOrDefault(v => v.ProductPriceConfigVersionId == info.LastestProductPriceConfigVersionId);

            var versionIds = versions.Select(v => v.ProductPriceConfigVersionId).ToList();

            var items = await _purchaseOrderDBContext.ProductPriceConfigItem
                .Where(c => versionIds.Contains(c.ProductPriceConfigVersionId))
                .ToListAsync();

            foreach (var item in items)
            {
                item.IsDeleted = true;
            }

            await _purchaseOrderDBContext.SaveChangesAsync();

            await trans.CommitAsync();

            await _objectActivityLog.LogBuilder(() => ProductPriceConfigActivityLogMessage.Deleted)
             .MessageResourceFormatDatas(lastestVersionInfo.Title)
             .ObjectId(productPriceConfigId)
             .JsonData(lastestVersionInfo.JsonSerialize())
             .CreateLog();
            return true;
        }


        private async Task<int> CreateVersion(int productPriceConfigId, ProductPriceConfigVersionModel model)
        {
            var versionInfo = _mapper.Map<ProductPriceConfigVersion>(model);
            versionInfo.ProductPriceConfigVersionId = 0;
            versionInfo.ProductPriceConfigId = productPriceConfigId;

            await _purchaseOrderDBContext.ProductPriceConfigVersion.AddAsync(versionInfo);
            await _purchaseOrderDBContext.SaveChangesAsync();

            var items = _mapper.Map<IList<ProductPriceConfigItem>>(model.Items);
            foreach (var item in items)
            {
                item.ProductPriceConfigVersionId = versionInfo.ProductPriceConfigVersionId;
            }

            await _purchaseOrderDBContext.ProductPriceConfigItem.AddRangeAsync(items);
            await _purchaseOrderDBContext.SaveChangesAsync();

            return versionInfo.ProductPriceConfigVersionId;
        }

    }
}
