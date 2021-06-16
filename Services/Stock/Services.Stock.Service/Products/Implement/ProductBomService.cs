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
using VErp.Commons.GlobalObject.InternalDataInterface;
using static VErp.Commons.GlobalObject.InternalDataInterface.ProductModel;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Stock.Service.Products.Implement.ProductBomFacade;

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

        public ProductBomService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductBomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IUnitService unitService
            , IProductService productService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _unitService = unitService;
            _productService = productService;
        }

        public async Task<IList<ProductBomOutput>> GetBom(int productId)
        {
            if (!_stockDbContext.Product.Any(p => p.ProductId == productId)) throw new BadRequestException(ProductErrorCode.ProductNotFound);

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
                result.Add(bom);
            }
            return result;
        }

        public async Task<bool> Update(int productId, IList<ProductBomInput> productBoms, IList<ProductMaterialModel> productMaterials)
        {
            var product = _stockDbContext.Product.FirstOrDefault(p => p.ProductId == productId);
            if (product == null) throw new BadRequestException(ProductErrorCode.ProductNotFound);


            await UpdateProductBomDb(productId, productBoms, productMaterials);
            await _activityLogService.CreateLog(EnumObjectType.ProductBom, productId, $"Cập nhật chi tiết bom cho mặt hàng {product.ProductCode}, tên hàng {product.ProductName}", productBoms.JsonSerialize());
            return true;
        }

        public async Task<bool> UpdateProductBomDb(int productId, IList<ProductBomInput> productBoms, IList<ProductMaterialModel> productMaterials)
        {

            // Validate data
            // Validate child product id
            var childIds = productBoms.Select(b => b.ChildProductId).Distinct().ToList();
            if (_stockDbContext.Product.Count(p => childIds.Contains(p.ProductId)) != childIds.Count) throw new BadRequestException(ProductErrorCode.ProductNotFound, "Vật tư không tồn tại");

            if (productBoms.Any(b => b.ProductId == b.ChildProductId)) throw new BadRequestException(GeneralCode.InvalidParams, "Không được chọn vật tư là chính sản phẩm");

            if (productBoms.Any(p => p.ProductId != productId)) throw new BadRequestException(GeneralCode.InvalidParams, "Vật tư không thuộc sản phẩm");

            // Validate materials
            if (productMaterials.Any(m => m.RootProductId != productId)) throw new BadRequestException(GeneralCode.InvalidParams, "Nguyên vật liệu không thuộc sản phẩm");

            // Thiết lập sort order theo thứ tự tạo
            for (int indx = 0; indx < productBoms.Count; indx++)
            {
                productBoms[indx].SortOrder = indx;
            }

            // Remove duplicate
            productBoms = productBoms.GroupBy(b => new { b.ProductId, b.ChildProductId }).Select(g => g.First()).ToList();

            // Get old BOM info
            var oldBoms = _stockDbContext.ProductBom.Where(b => b.ProductId == productId).ToList();
            var newBoms = new List<ProductBomInput>(productBoms);
            var changeBoms = new List<(ProductBom OldValue, ProductBomInput NewValue)>();

            // Cập nhật BOM
            foreach (var newItem in productBoms)
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
            }

            // Cập nhật Material
            var oldMaterials = _stockDbContext.ProductMaterial.Where(m => m.RootProductId == productId).ToList();
            var createMaterials = productMaterials
                .Where(nm => !oldMaterials.Any(om => om.ProductId == nm.ProductId && om.PathProductIds == string.Join(",", nm.PathProductIds)))
                .Select(nm => new ProductMaterial
                {
                    ProductId = nm.ProductId,
                    ProductMaterialId = nm.ProductMaterialId,
                    RootProductId = nm.RootProductId,
                    PathProductIds = string.Join(",", nm.PathProductIds)
                })
                .ToList();
            var deleteMaterials = oldMaterials
                .Where(om => !productMaterials.Any(nm => nm.ProductId == om.ProductId && string.Join(",", nm.PathProductIds) == om.PathProductIds))
                .ToList();

            _stockDbContext.ProductMaterial.RemoveRange(deleteMaterials);
            _stockDbContext.ProductMaterial.AddRange(createMaterials);

            await _stockDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<(Stream stream, string fileName, string contentType)> ExportBom(IList<int> productIds)
        {
            var bomExport = new ProductBomExportFacade(_stockDbContext, productIds);
            return await bomExport.BomExport();
        }

        private bool HasChange(ProductBom oldValue, ProductBomInput newValue)
        {
            return oldValue.Quantity != newValue.Quantity
                || oldValue.Wastage != newValue.Wastage 
                || oldValue.InputStepId != newValue.InputStepId
                || oldValue.OutputStepId != newValue.OutputStepId
                || oldValue.Wastage != newValue.Wastage
                || oldValue.SortOrder != newValue.SortOrder;
        }

        public CategoryNameModel GetCustomerFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                CategoryId = 1,
                CategoryCode = "ProductBom",
                CategoryTitle = "Bill of Material",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = Utils.GetFieldNameModels<ProductBomImportModel>();
            result.Fields = fields;
            return result;
        }

        public Task<bool> ImportBomFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            return new ProductBomImportFacade()
                .SetService(_stockDbContext)
                .SetService(_productService)
                .SetService(_unitService)
                .SetService(_activityLogService)
                .SetService(this)
                .ProcessData(mapping, stream);
        }
    }
}
