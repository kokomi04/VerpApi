using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Stock.Model.Product;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class BillOfMaterialService : IBillOfMaterialService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityService _activityService;

        public BillOfMaterialService(StockDBContext stockContext
           , IOptions<AppSetting> appSetting
           , ILogger<BillOfMaterialService> logger
           , IActivityService activityService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityService = activityService;
        }

        public async Task<ServiceResult<BillOfMaterialOutput>> Get(long billOfMaterialId)
        {
            var entity = _stockDbContext.BillOfMaterial.Include(q => q.Product).Include(q => q.ParentProduct).AsNoTracking().FirstOrDefault(q => q.BillOfMaterialId == billOfMaterialId);
            if (entity != null)
            {
                var productExtraObj = _stockDbContext.ProductExtraInfo.AsNoTracking().FirstOrDefault(q => q.ProductId == entity.ProductId);
                var productCateObj = _stockDbContext.ProductCate.AsNoTracking().FirstOrDefault(q => q.ProductCateId == entity.Product.ProductCateId);
                var billOfMaterialOutputModel = new BillOfMaterialOutput
                {
                    BillOfMaterialId = entity.BillOfMaterialId,
                    Level = entity.Level,
                    ProductId = entity.ProductId,
                    ParentProductId = entity.ParentProductId,
                    ProductCode = entity.Product.ProductCode,
                    ProductName = entity.Product.ProductName,
                    ProductCateName = productCateObj.ProductCateName,
                    ProductSpecification = productExtraObj.Specification,
                    Quantity = entity.Quantity,
                    Wastage = entity.Wastage,
                    Description = entity.Description,
                    CreatedDatetimeUtc = entity.CreatedDatetimeUtc,
                    UpdatedDatetimeUtc = entity.UpdatedDatetimeUtc
                };
                return billOfMaterialOutputModel;
            }
            return null;
        }

        public async Task<PageData<BillOfMaterialOutput>> GetAll(int productId)
        {
            
            var BomData = new List<BillOfMaterial>();
            GetAllBom(productId, BomData);
            var resultList = new List<BillOfMaterialOutput>(BomData.Count);
            foreach (var item in BomData)
            {
                var entity = _stockDbContext.BillOfMaterial.Include(q => q.Product).Include(q => q.ParentProduct).AsNoTracking().FirstOrDefault(q => q.BillOfMaterialId == item.BillOfMaterialId);
                var productExtraObj = _stockDbContext.ProductExtraInfo.AsNoTracking().FirstOrDefault(q => q.ProductId == entity.ProductId);
                var productCateObj = _stockDbContext.ProductCate.AsNoTracking().FirstOrDefault(q => q.ProductCateId == entity.Product.ProductCateId);
                var billOfMaterialOutputModel = new BillOfMaterialOutput
                {
                    BillOfMaterialId = entity.BillOfMaterialId,
                    Level = entity.Level,
                    ProductId = entity.ProductId,
                    ParentProductId = entity.ParentProductId,
                    ProductCode = entity.Product.ProductCode,
                    ProductName = entity.Product.ProductName,
                    ProductCateName = productCateObj.ProductCateName,
                    ProductSpecification = productExtraObj.Specification,
                    Quantity = entity.Quantity,
                    Wastage = entity.Wastage,
                    Description = entity.Description,
                    CreatedDatetimeUtc = entity.CreatedDatetimeUtc,
                    UpdatedDatetimeUtc = entity.UpdatedDatetimeUtc
                };
                resultList.Add(billOfMaterialOutputModel);
            }
            return (resultList, resultList.Count);
        }
        public async Task<PageData<BillOfMaterialOutput>> GetList(int productId, int page = 1, int size = 20)
        {
            var bomQuery = _stockDbContext.BillOfMaterial.Where(q => q.RootProductId == productId);

            var totalRecord = bomQuery.Count();
            var bomDataList = bomQuery.Skip((page - 1) * size).Take(size).AsNoTracking().ToList();

            var resultList = new List<BillOfMaterialOutput>(totalRecord);
            foreach (var item in bomDataList)
            {
                var entity = _stockDbContext.BillOfMaterial.Include(q => q.Product).Include(q => q.ParentProduct).AsNoTracking().FirstOrDefault(q => q.BillOfMaterialId == item.BillOfMaterialId);
                var productExtraObj = _stockDbContext.ProductExtraInfo.AsNoTracking().FirstOrDefault(q => q.ProductId == entity.ProductId);
                var productCateObj = _stockDbContext.ProductCate.AsNoTracking().FirstOrDefault(q => q.ProductCateId == entity.Product.ProductCateId);
                var billOfMaterialOutputModel = new BillOfMaterialOutput
                {
                    BillOfMaterialId = entity.BillOfMaterialId,
                    Level = entity.Level,
                    ProductId = entity.ProductId,
                    ParentProductId = entity.ParentProductId,
                    ProductCode = entity.Product.ProductCode,
                    ProductName = entity.Product.ProductName,
                    ProductCateName = productCateObj.ProductCateName,
                    ProductSpecification = productExtraObj.Specification,
                    Quantity = entity.Quantity,
                    Wastage = entity.Wastage,
                    Description = entity.Description,
                    CreatedDatetimeUtc = entity.CreatedDatetimeUtc,
                    UpdatedDatetimeUtc = entity.UpdatedDatetimeUtc
                };
                resultList.Add(billOfMaterialOutputModel);
            }
            return (resultList, totalRecord);
        }

        public async Task<ServiceResult<long>> Add(BillOfMaterialInput req)
        {
            try
            {
                var checkExists = _stockDbContext.BillOfMaterial.Any(q =>q.RootProductId == req.RootProductId && q.ProductId == req.ProductId && q.ParentProductId == req.ParentProductId);
                if (checkExists)
                    return GeneralCode.InvalidParams;
                var entity = new BillOfMaterial
                {
                    Level = 0,
                    RootProductId = req.RootProductId,
                    ProductId = req.ProductId,
                    ParentProductId = req.ParentProductId,
                    Quantity = req.Quantity,
                    Wastage = req.Wastage,
                    Description = req.Description,
                    IsDeleted = false,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                };
                await _stockDbContext.BillOfMaterial.AddAsync(entity);
                await _stockDbContext.SaveChangesAsync();

                _activityService.CreateActivityAsync(EnumObjectType.BillOfMaterial, entity.BillOfMaterialId, $"Thêm mới 1 chi tiết bom {entity.ProductId}", null, entity);

                return entity.BillOfMaterialId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Add");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> Update(long billOfMaterialId, BillOfMaterialInput req)
        {
            try
            {
                var entity = _stockDbContext.BillOfMaterial.FirstOrDefault(q => q.BillOfMaterialId == billOfMaterialId);
                if (entity == null)
                    return GeneralCode.InvalidParams;
                var beforeJson = entity.JsonSerialize();

                entity.ProductId = req.ProductId;
                entity.ParentProductId = req.ParentProductId;
                entity.Quantity = req.Quantity;
                entity.Wastage = req.Wastage;
                entity.Description = req.Description;
                entity.UpdatedDatetimeUtc = DateTime.UtcNow;
                await _stockDbContext.SaveChangesAsync();
                _activityService.CreateActivityAsync(EnumObjectType.BillOfMaterial, entity.BillOfMaterialId, $"Cập nhật chi tiết bom {entity.ProductId} {entity.ParentProductId}", beforeJson, entity);
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }


        public async Task<Enum> Delete(long billOfMaterialId,int rootProductId)
        {
            try
            {
                var entity = _stockDbContext.BillOfMaterial.FirstOrDefault(q =>q.RootProductId == rootProductId && q.BillOfMaterialId == billOfMaterialId);
                if (entity == null)
                    return GeneralCode.InvalidParams;
                entity.IsDeleted = true;
                entity.UpdatedDatetimeUtc = DateTime.UtcNow;
                var childList = new List<BillOfMaterial>();
                GetAllBom(entity.ProductId, childList);                
                foreach (var item in childList)
                {
                    item.IsDeleted = true;
                    item.UpdatedDatetimeUtc = DateTime.UtcNow;
                }
                await _stockDbContext.SaveChangesAsync();
                _activityService.CreateActivityAsync(EnumObjectType.BillOfMaterial, entity.ProductId, $"Xóa thông tin bom {entity.ProductId}", entity.JsonSerialize(), null);

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete");
                return GeneralCode.InternalError;
            }
        }

        #region Private methods
        protected void GetAllBom(int productId, List<BillOfMaterial> bomList)
        {
            var BomDataList = _stockDbContext.BillOfMaterial.Where(q => q.ParentProductId == productId).AsNoTracking().ToList();
            foreach (var item in BomDataList)
            {
                if (item.ProductId > 0 && item.IsDeleted == false)
                {
                    bomList.Add(item);
                    GetAllBom((int)item.ProductId, bomList);
                }
            }
        }
        #endregion

    }
}
