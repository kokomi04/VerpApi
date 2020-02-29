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
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductBomService : IProductBomService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public ProductBomService(StockDBContext stockContext
           , IOptions<AppSetting> appSetting
           , ILogger<ProductBomService> logger
           , IActivityLogService activityLogService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<ServiceResult<ProductBomOutput>> Get(long productBomId)
        {
            var entity = _stockDbContext.ProductBom.Include(q => q.Product).Include(q => q.ParentProduct).AsNoTracking().FirstOrDefault(q => q.ProductBomId == productBomId);
            if (entity != null)
            {
                var productExtraObj = _stockDbContext.ProductExtraInfo.AsNoTracking().FirstOrDefault(q => q.ProductId == entity.ProductId);
                var productCateObj = _stockDbContext.ProductCate.AsNoTracking().FirstOrDefault(q => q.ProductCateId == entity.Product.ProductCateId);
                var billOfMaterialOutputModel = new ProductBomOutput
                {
                    ProductBomId = entity.ProductBomId,
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
                    CreatedDatetimeUtc = entity.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = entity.UpdatedDatetimeUtc.GetUnix()
                };
                return billOfMaterialOutputModel;
            }
            return null;
        }

        public async Task<PageData<ProductBomOutput>> GetAll(int productId)
        {
            
            var BomData = new List<ProductBom>();
            GetAllBom(productId, BomData);
            var resultList = new List<ProductBomOutput>(BomData.Count);
            foreach (var item in BomData)
            {
                var entity = _stockDbContext.ProductBom.Include(q => q.Product).Include(q => q.ParentProduct).AsNoTracking().FirstOrDefault(q => q.ProductBomId == item.ProductBomId);
                var productExtraObj = _stockDbContext.ProductExtraInfo.AsNoTracking().FirstOrDefault(q => q.ProductId == entity.ProductId);
                var productCateObj = _stockDbContext.ProductCate.AsNoTracking().FirstOrDefault(q => q.ProductCateId == entity.Product.ProductCateId);
                var billOfMaterialOutputModel = new ProductBomOutput
                {
                    ProductBomId = entity.ProductBomId,
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
                    CreatedDatetimeUtc = entity.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = entity.UpdatedDatetimeUtc.GetUnix()
                };
                resultList.Add(billOfMaterialOutputModel);
            }
            return (resultList, resultList.Count);
        }
        public async Task<PageData<ProductBomOutput>> GetList(int productId, int page = 1, int size = 20)
        {
            var bomQuery = _stockDbContext.ProductBom.Where(q => q.RootProductId == productId);

            var totalRecord = bomQuery.Count();
            var bomDataList = bomQuery.Skip((page - 1) * size).Take(size).AsNoTracking().ToList();

            var resultList = new List<ProductBomOutput>(totalRecord);
            foreach (var item in bomDataList)
            {
                var entity = _stockDbContext.ProductBom.Include(q => q.Product).Include(q => q.ParentProduct).AsNoTracking().FirstOrDefault(q => q.ProductBomId == item.ProductBomId);
                var productExtraObj = _stockDbContext.ProductExtraInfo.AsNoTracking().FirstOrDefault(q => q.ProductId == entity.ProductId);
                var productCateObj = _stockDbContext.ProductCate.AsNoTracking().FirstOrDefault(q => q.ProductCateId == entity.Product.ProductCateId);
                var billOfMaterialOutputModel = new ProductBomOutput
                {
                    ProductBomId = entity.ProductBomId,
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
                    CreatedDatetimeUtc = entity.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = entity.UpdatedDatetimeUtc.GetUnix()
                };
                resultList.Add(billOfMaterialOutputModel);
            }
            return (resultList, totalRecord);
        }

        public async Task<ServiceResult<long>> Add(ProductBomInput req)
        {
            try
            {
                var checkExists = _stockDbContext.ProductBom.Any(q =>q.RootProductId == req.RootProductId && q.ProductId == req.ProductId && q.ParentProductId == req.ParentProductId);
                if (checkExists)
                    return GeneralCode.InvalidParams;
                var entity = new ProductBom
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
                await _stockDbContext.ProductBom.AddAsync(entity);
                await _stockDbContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.ProductBom, entity.ProductBomId, $"Thêm mới 1 chi tiết bom {entity.ProductId}", req.JsonSerialize());

                return entity.ProductBomId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Add");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> Update(long productBomId, ProductBomInput req)
        {
            try
            {
                if(productBomId <= 0)
                    return GeneralCode.InvalidParams;
                var entity = _stockDbContext.ProductBom.FirstOrDefault(q => q.ProductBomId == productBomId);
                if (entity == null)
                    return GeneralCode.InvalidParams;

                //entity.ProductId = req.ProductId;
                //entity.ParentProductId = req.ParentProductId;
                entity.Quantity = req.Quantity;
                entity.Wastage = req.Wastage;
                entity.Description = req.Description;
                entity.UpdatedDatetimeUtc = DateTime.UtcNow;
                await _stockDbContext.SaveChangesAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductBom, entity.ProductBomId, $"Cập nhật chi tiết bom {entity.ProductId} {entity.ParentProductId}", req.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }


        public async Task<Enum> Delete(long productBomId,int rootProductId)
        {
            try
            {
                var entity = _stockDbContext.ProductBom.FirstOrDefault(q =>q.RootProductId == rootProductId && q.ProductBomId == productBomId);
                if (entity == null)
                    return GeneralCode.InvalidParams;
                entity.IsDeleted = true;
                entity.UpdatedDatetimeUtc = DateTime.UtcNow;
                var childList = new List<ProductBom>();
                GetAllBom(entity.ProductId, childList);                
                foreach (var item in childList)
                {
                    item.IsDeleted = true;
                    item.UpdatedDatetimeUtc = DateTime.UtcNow;
                }
                await _stockDbContext.SaveChangesAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductBom, entity.ProductId, $"Xóa thông tin bom {entity.ProductId}", entity.JsonSerialize());

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete");
                return GeneralCode.InternalError;
            }
        }

        #region Private methods
        protected void GetAllBom(int productId, List<ProductBom> bomList)
        {
            var BomDataList = _stockDbContext.ProductBom.Where(q => q.ParentProductId == productId).AsNoTracking().ToList();
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
