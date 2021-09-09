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
using VErp.Commons.GlobalObject;
using Microsoft.Data.SqlClient;
using VErp.Infrastructure.EF.EFExtensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Data;
using System.IO;
using VErp.Commons.Library.Model;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Service.Products.Implement.ProductBomFacade;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Stock.Model.Product.Bom;
using VErp.Infrastructure.ServiceCore.Facade;
using Verp.Resources.Stock.Product;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductBomService : IProductBomService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        private readonly IUnitService _unitService;
        private readonly IProductService _productService;
        private readonly IManufacturingHelperService _manufacturingHelperService;
        private readonly IPropertyService _propertyService;
        private readonly ObjectActivityLogFacade _productActivityLog;

        public ProductBomService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductBomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IUnitService unitService
            , IProductService productService
            , IManufacturingHelperService manufacturingHelperService
            , IPropertyService propertyService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _unitService = unitService;
            _productService = productService;
            _manufacturingHelperService = manufacturingHelperService;
            _propertyService = propertyService;
            _productActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Product);
        }

        public async Task<IList<ProductElementModel>> GetProductElements(IList<int> productIds)
        {
            if (!_stockDbContext.Product.Any(p => productIds.Contains(p.ProductId))) throw new BadRequestException(ProductErrorCode.ProductNotFound);

            var parammeters = new SqlParameter[]
            {
                productIds.ToSqlParameter("@ProductIds")
            };

            var resultData = await _stockDbContext.ExecuteDataProcedure("asp_GetProductElements", parammeters);
            var result = resultData.ConvertData<ProductElementModel>();
            return result;
        }


        public async Task<IDictionary<int, IList<ProductBomOutput>>> GetBoms(IList<int> productIds)
        {
            var dic = new Dictionary<int, IList<ProductBomOutput>>();
            foreach (var productId in productIds.Distinct())
            {
                dic.Add(productId, await GetBom(productId));
            }

            return dic;
        }

        public async Task<IList<ProductBomOutput>> GetBom(int productId)
        {
            if (!_stockDbContext.Product.Any(p => p.ProductId == productId)) throw new BadRequestException(ProductErrorCode.ProductNotFound);
            var productProperties = await _stockDbContext.ProductProperty
               .Where(p => p.RootProductId == productId)
               .ToListAsync();
            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@ProductId", productId)
            };

            var resultData = await _stockDbContext.ExecuteDataProcedure("asp_GetProductBom", parammeters);
            var result = new List<ProductBomOutput>();
            foreach (var item in resultData.ConvertData<ProductBomEntity>())
            {
                var bom = _mapper.Map<ProductBomOutput>(item);
                bom.PathProductIds = Array.ConvertAll(item.PathProductIds.Split(','), s => int.Parse(s));
                bom.Properties = productProperties.Where(p => p.ProductId == item.ChildProductId && p.PathProductIds == item.PathProductIds).Select(p => p.PropertyId).Distinct().ToList();
                result.Add(bom);
            }
            return result;
        }

        public async Task<bool> Update(int productId, ProductBomUpdateInfoModel bomInfo)
        {
            var product = _stockDbContext.Product.FirstOrDefault(p => p.ProductId == productId);
            if (product == null) throw new BadRequestException(ProductErrorCode.ProductNotFound);
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                await UpdateProductBomDb(productId, bomInfo);
                await trans.CommitAsync();
            }

            await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.UpdateBom)
             .MessageResourceFormatDatas(product.ProductCode)
             .ObjectId(productId)
             .JsonData(bomInfo.JsonSerialize())
             .CreateLog();

            return true;
        }

        public async Task<bool> UpdateProductBomDb(int productId, ProductBomUpdateInfoModel bomInfo)
        {
            await UpdateBoms(productId, bomInfo.BomInfo);
            await UpdateBomMaterials(productId, bomInfo.MaterialsInfo);
            await UpdateBomProperties(productId, bomInfo.PropertiesInfo);
            return true;
        }


        private async Task UpdateBoms(int rootProductId, ProductBomUpdateInfo bomInfo)
        {
            // Validate data
            // Validate child product id
            var childIds = bomInfo.Bom.Select(b => b.ChildProductId).Distinct().ToList();
            if (_stockDbContext.Product.Count(p => childIds.Contains(p.ProductId)) != childIds.Count) throw ProductErrorCode.ProductNotFound.BadRequest();

            if (bomInfo.Bom.Any(b => b.ProductId == b.ChildProductId)) throw new BadRequestException(GeneralCode.InvalidParams, "Không được chọn vật tư là chính sản phẩm");

            if (bomInfo.Bom.Any(p => p.ProductId != rootProductId)) throw new BadRequestException(GeneralCode.InvalidParams, "Vật tư không thuộc sản phẩm");


            // Thiết lập sort order theo thứ tự tạo nếu không truyền từ client lên
            for (int indx = 0; indx < bomInfo.Bom.Count; indx++)
            {
                if (!bomInfo.Bom[indx].SortOrder.HasValue)
                    bomInfo.Bom[indx].SortOrder = indx;
            }

            // Remove duplicate
            bomInfo.Bom = bomInfo.Bom.GroupBy(b => new { b.ProductId, b.ChildProductId }).Select(g => g.First()).ToList();

            // Get old BOM info
            var oldBoms = _stockDbContext.ProductBom.Where(b => b.ProductId == rootProductId).ToList();
            var newBoms = new List<ProductBomInput>(bomInfo.Bom);
            var changeBoms = new List<(ProductBom OldValue, ProductBomInput NewValue)>();

            // Cập nhật BOM
            foreach (var newItem in bomInfo.Bom)
            {
                var oldBom = oldBoms.FirstOrDefault(b => b.ChildProductId == newItem.ChildProductId);
                // Nếu là thay đổi
                if (oldBom != null)
                {
                    // Kiểm tra thay đổi thông tin
                    if (HasChange(oldBom, newItem))
                    {
                        changeBoms.Add((oldBom, newItem));
                    }
                    newBoms.Remove(newItem);
                    oldBoms.Remove(oldBom);
                }
            }

            // Xóa BOM
            foreach (var entity in oldBoms)
            {
                entity.IsDeleted = true;
            }

            // Tạo mới bom
            foreach (var newBom in newBoms)
            {
                var entity = _mapper.Map<ProductBom>(newBom);
                entity.ProductBomId = 0;
                _stockDbContext.ProductBom.Add(entity);
            }

            // Cập nhật BOM
            foreach (var updateBom in changeBoms)
            {
                updateBom.OldValue.Quantity = updateBom.NewValue.Quantity;
                updateBom.OldValue.Wastage = updateBom.NewValue.Wastage;
                updateBom.OldValue.InputStepId = updateBom.NewValue.InputStepId;
                updateBom.OldValue.OutputStepId = updateBom.NewValue.OutputStepId;
                updateBom.OldValue.SortOrder = updateBom.NewValue.SortOrder;
                updateBom.OldValue.Description = updateBom.NewValue.Description;
            }

            await _stockDbContext.SaveChangesAsync();
        }

        private async Task UpdateBomMaterials(int rootProductId, ProductBomMaterialUpdateInfo materialsInfo)
        {
            // Validate materials
            if (materialsInfo.BomMaterials.Any(m => m.RootProductId != rootProductId)) throw new BadRequestException(GeneralCode.InvalidParams, "Nguyên vật liệu không thuộc sản phẩm");


            // Cập nhật Material
            var oldMaterials = _stockDbContext.ProductMaterial.Where(m => m.RootProductId == rootProductId).ToList();
            var createMaterials = materialsInfo.BomMaterials
                .Where(nm => !oldMaterials.Any(om => om.ProductId == nm.ProductId && om.PathProductIds == string.Join(",", nm.PathProductIds)))
                .Select(nm => new ProductMaterial
                {
                    ProductId = nm.ProductId,
                    ProductMaterialId = nm.ProductMaterialId,
                    RootProductId = nm.RootProductId,
                    PathProductIds = string.Join(",", nm.PathProductIds)
                })
                .ToList();
            if (materialsInfo.CleanOldData)
            {
                var deleteMaterials = oldMaterials
                    .Where(om => !materialsInfo.BomMaterials.Any(nm => nm.ProductId == om.ProductId && string.Join(",", nm.PathProductIds) == om.PathProductIds))
                    .ToList();
                _stockDbContext.ProductMaterial.RemoveRange(deleteMaterials);
            }
            _stockDbContext.ProductMaterial.AddRange(createMaterials);

            await _stockDbContext.SaveChangesAsync();
        }


        private async Task UpdateBomProperties(int rootProductId, ProductBomPropertyUpdateInfo propertiesInfo)
        {
            // Cập nhật Properties
            var propertyIds = propertiesInfo.BomProperties.Select(p => p.PropertyId).Distinct().ToList();
            var properties = _stockDbContext.Property.Where(p => propertyIds.Contains(p.PropertyId)).ToList();
            if (propertyIds.Count != properties.Count) throw new BadRequestException(GeneralCode.InvalidParams, "BOM có chứa thuộc tính sản phẩm không tồn tại");
            var oldProperties = _stockDbContext.ProductProperty.Where(m => m.RootProductId == rootProductId).ToList();
            var createProperties = propertiesInfo.BomProperties
                .Where(np => !oldProperties.Any(op => op.ProductId == np.ProductId && op.PropertyId == np.PropertyId && op.PathProductIds == string.Join(",", np.PathProductIds)))
                .Select(np => new ProductProperty
                {
                    ProductId = np.ProductId,
                    ProductPropertyId = np.ProductPropertyId,
                    RootProductId = np.RootProductId,
                    PathProductIds = string.Join(",", np.PathProductIds),
                    PropertyId = np.PropertyId
                })
                .ToList();
            if (propertiesInfo.CleanOldData)
            {
                var deleteProperties = oldProperties
                    .Where(op => !propertiesInfo.BomProperties.Any(np => np.ProductId == op.ProductId && np.PropertyId == op.PropertyId && string.Join(",", np.PathProductIds) == op.PathProductIds))
                    .ToList();
                _stockDbContext.ProductProperty.RemoveRange(deleteProperties);
            }

            _stockDbContext.ProductProperty.AddRange(createProperties);
            await _stockDbContext.SaveChangesAsync();
        }


        public async Task<(Stream stream, string fileName, string contentType)> ExportBom(IList<int> productIds)
        {
            var steps = await _manufacturingHelperService.GetSteps();
            var properties = await _propertyService.GetProperties();
            var bomExport = new ProductBomExportFacade(_stockDbContext, productIds, steps, properties);
            return await bomExport.BomExport();
        }

        private bool HasChange(ProductBom oldValue, ProductBomInput newValue)
        {
            return oldValue.Quantity != newValue.Quantity
                || oldValue.Wastage != newValue.Wastage
                || oldValue.InputStepId != newValue.InputStepId
                || oldValue.OutputStepId != newValue.OutputStepId
                || oldValue.Wastage != newValue.Wastage
                || oldValue.SortOrder != newValue.SortOrder
                || oldValue.Description != newValue.Description;
        }

        public async Task<CategoryNameModel> GetBomFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "ProductBom",
                CategoryTitle = "Bill of Material",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = Utils.GetFieldNameModels<ProductBomImportModel>();
            result.Fields = fields;
            var properties = await _propertyService.GetProperties();
            foreach (var p in properties)
            {
                result.Fields.Add(new CategoryFieldNameModel()
                {
                    GroupName = "Thuộc tính",
                    FieldName = nameof(ProductBomImportModel.Properties) + p.PropertyId,
                    FieldTitle = p.PropertyName + " (Có, Không)"
                });
            }
            return result;
        }

        public Task<bool> ImportBomFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            return InitImportBomFacade(false)
                .ProcessData(mapping, stream);
        }

        public async Task<IList<ProductBomByProduct>> PreviewBomFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var bomProcess = InitImportBomFacade(true);
            var r = await bomProcess.ProcessData(mapping, stream);
            if (!r) return null;
            return bomProcess.PreviewData;
        }


        private ProductBomImportFacade InitImportBomFacade(bool isPreview)
        {
            return new ProductBomImportFacade(isPreview)
               .SetService(_stockDbContext)
               .SetService(_productService)
               .SetService(_unitService)
               .SetService(_activityLogService)
               .SetService(_manufacturingHelperService)
               .SetService(this);
        }
    }
}
