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

        private async Task<bool> UpdateProductBomDb(int productId, IList<ProductBomInput> productBoms, IList<ProductMaterialModel> productMaterials)
        {

            // Validate data
            // Validate child product id
            var childIds = productBoms.Select(b => b.ChildProductId).Distinct().ToList();
            if (_stockDbContext.Product.Count(p => childIds.Contains(p.ProductId)) != childIds.Count) throw new BadRequestException(ProductErrorCode.ProductNotFound, "Vật tư không tồn tại");

            if (productBoms.Any(b => b.ProductId == b.ChildProductId)) throw new BadRequestException(GeneralCode.InvalidParams, "Không được chọn vật tư là chính sản phẩm");

            if (productBoms.Any(p => p.ProductId != productId)) throw new BadRequestException(GeneralCode.InvalidParams, "Vật tư không thuộc sản phẩm");

            // Validate materials
            if (productMaterials.Any(m => m.RootProductId != productId)) throw new BadRequestException(GeneralCode.InvalidParams, "Nguyên vật liệu không thuộc sản phẩm");

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
                || oldValue.Wastage != newValue.Wastage;
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

        public async Task<bool> ImportBomFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var lstData = reader.ReadSheetEntity<ProductBomImportModel>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;
                switch (propertyName)
                {
                    case nameof(ProductBomImportModel.IsMaterial):
                        if (value.NormalizeAsInternalName().Equals("Có".NormalizeAsInternalName()))
                        {
                            entity.IsMaterial = true;
                        }
                        return true;
                    case nameof(ProductBomImportModel.Wastage):
                        decimal.TryParse(value, out var v);
                        if (v > 0)
                        {
                            entity.Wastage = v;
                        }
                        else
                        {
                            entity.Wastage = 1;
                        }
                        return true;

                }

                return false;
            });

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                //product type
                var productTypes = (await _stockDbContext.ProductType.AsNoTracking().ToListAsync()).GroupBy(t => t.IdentityCode.NormalizeAsInternalName()).ToDictionary(t => t.Key, t => t.FirstOrDefault());

                var newProductTypes = lstData.SelectMany(p => new[] { p.ProductTypeCode, p.ChildProductTypeCode }).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                    .GroupBy(t => t.NormalizeAsInternalName())
                    .ToDictionary(t => t.Key, t => t.FirstOrDefault());

                var newTypes = newProductTypes.Where(t => !productTypes.ContainsKey(t.Key))
                    .Select(t => new ProductType()
                    {
                        ProductTypeName = t.Value,
                        IdentityCode = t.Value
                    }).ToList();
                await _stockDbContext.ProductType.AddRangeAsync(newTypes);
                await _stockDbContext.SaveChangesAsync();

                foreach (var t in newTypes)
                {
                    productTypes.Add(t.IdentityCode.NormalizeAsInternalName(), t);
                }


                //product cate
                var productCates = (await _stockDbContext.ProductCate.AsNoTracking().ToListAsync()).GroupBy(t => t.ProductCateName.NormalizeAsInternalName()).ToDictionary(t => t.Key, t => t.FirstOrDefault());

                var newProductCates = lstData.SelectMany(p => new[] { p.ProductCateName, p.ChildProductCateName }).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                    .GroupBy(t => t.NormalizeAsInternalName())
                    .ToDictionary(t => t.Key, t => t.FirstOrDefault());

                var newCates = newProductCates.Where(t => !productCates.ContainsKey(t.Key))
                    .Select(t => new ProductCate()
                    {
                        ProductCateName = t.Value
                    }).ToList();

                await _stockDbContext.ProductCate.AddRangeAsync(newCates);
                await _stockDbContext.SaveChangesAsync();

                foreach (var t in newCates)
                {
                    productCates.Add(t.ProductCateName.NormalizeAsInternalName(), t);
                }

                var units = (await _unitService.GetList(string.Empty, null, 1, -1, null)).List.GroupBy(u => u.UnitName.NormalizeAsInternalName())
                    .ToDictionary(u => u.Key, u => u.FirstOrDefault());


                var importedUnits = lstData.SelectMany(p => new[] { p.UnitName, p.ChildUnitName }).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                    .GroupBy(t => t.NormalizeAsInternalName())
                    .ToDictionary(t => t.Key, t => t.FirstOrDefault());


                var newUnits = importedUnits.Where(t => !units.ContainsKey(t.Key))
                    .Select(t => new UnitInput()
                    {
                        UnitName = t.Value,
                        UnitStatusId = EnumUnitStatus.Using
                    }).ToList();
                foreach (var uni in newUnits)
                {
                    var unitId = await _unitService.AddUnit(uni);
                    units.Add(uni.UnitName.NormalizeAsInternalName(), new UnitOutput() { UnitId = unitId, UnitName = uni.UnitName, UnitStatusId = uni.UnitStatusId });
                }

                var importProducts = lstData.SelectMany(p => new[]
                {
                new { p.ProductCode, p.ProductName, p.ProductTypeCode, p.ProductCateName, p.UnitName, p.Specification },
                new { ProductCode = p.ChildProductCode, ProductName = p.ChildProductName, ProductTypeCode= p.ChildProductTypeCode,ProductCateName=p.ChildProductCateName, UnitName = p.ChildUnitName, Specification=p.ChildSpecification } }
                ).Where(p => string.IsNullOrWhiteSpace(p.ProductCode))
                .Distinct()
                .ToList()
                .GroupBy(p => p.ProductCode.NormalizeAsInternalName())
                .ToDictionary(p => p.Key, p => p.FirstOrDefault());

                var existedProducts = (await _stockDbContext.Product.AsNoTracking().Select(p => new { p.ProductId, p.ProductCode, p.ProductName }).ToListAsync()).GroupBy(p => p.ProductCode.NormalizeAsInternalName())
                    .ToDictionary(p => p.Key, p => p.FirstOrDefault());

                var newProducts = importProducts.Where(p => !existedProducts.ContainsKey(p.Key))
                    .Select(p =>
                    {

                        productTypes.TryGetValue(p.Value.ProductCode, out var type);
                        productCates.TryGetValue(p.Value.ProductName, out var cate);
                        if (cate == null)
                        {
                            cate = productCates.FirstOrDefault(c => c.Value.IsDefault).Value;
                        }
                        if (cate == null)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy danh mục mặt hàng hoặc danh mục mặc định cho mặt hàng {p.Value.ProductCode} {p.Value.ProductName}");
                        }

                        units.TryGetValue(p.Value.UnitName, out var unit);
                        if (unit == null)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy đơn vị tính \"{p.Value.UnitName}\" cho mặt hàng {p.Value.ProductCode} {p.Value.ProductName}");
                        }

                        return new ProductModel
                        {

                            ProductCode = p.Value.ProductCode,
                            ProductName = p.Value.ProductName,

                            ProductTypeId = type?.ProductTypeId,
                            ProductCateId = cate.ProductCateId,
                            UnitId = unit.UnitId,


                            Extra = new ProductModelExtra()
                            {
                                Specification = p.Value.Specification
                            },
                            StockInfo = new ProductModelStock()
                            {
                                UnitConversions = new List<ProductModelUnitConversion>()
                            }

                        };
                    });
                foreach (var product in newProducts)
                {
                    var productId = await _productService.AddProductToDb(product);
                    existedProducts.Add(product.ProductCode.NormalizeAsInternalName(), new { ProductId = productId, ProductCode = product.ProductCode, ProductName = product.ProductName });
                }

                
                var importBoms = lstData.GroupBy(b => b.ProductCode.NormalizeAsInternalName()).ToDictionary(b => b.Key, b => b.GroupBy(c => c.ChildProductCode.NormalizeAsInternalName()).Select(g => g.First()).ToList());

                var childImports = lstData.GroupBy(c => c.ChildProductCode.NormalizeAsInternalName()).ToDictionary(c => c.Key, c => c.Max(m => m.IsMaterial));


                using (var logBath = _activityLogService.BeginBatchLog())
                {
                    foreach (var bom in importBoms)
                    {
                        existedProducts.TryGetValue(bom.Key, out var productInfo);

                        var productMaterials = new List<ProductMaterialModel>();

                        var st = new Stack<string>();
                        st.Push(bom.Key);

                        var path = new Stack<string>();

                        var productMaterialPath = new Dictionary<int, IList<string>>();

                        while (st.Count > 0)
                        {
                            var child = st.Pop();
                            existedProducts.TryGetValue(child, out var childInfo);

                            if (childImports.TryGetValue(child, out var isMaterial) && isMaterial)
                            {
                                productMaterials.Add(new ProductMaterialModel()
                                {
                                    RootProductId = productInfo.ProductId,
                                    ProductId = childInfo.ProductId,
                                    PathProductIds = string.Join(",", path)
                                });
                                path.Pop();
                            }
                            else
                            {
                                path.Push(child);
                                foreach (var b in importBoms[child])
                                {
                                    st.Push(b.ChildProductCode);
                                }
                            }
                        }

                        var productBoms = bom.Value.Select(b =>
                        {
                            existedProducts.TryGetValue(bom.Key, out var childProduct);

                            return new ProductBomInput()
                            {
                                ProductBomId = null,
                                ProductId = productInfo.ProductId,
                                ChildProductId = childProduct.ProductId,
                                Quantity = b.Quantity,
                                Wastage = b.Wastage ?? 1
                            };
                        }).ToList();

                        await UpdateProductBomDb(productInfo.ProductId, productBoms, productMaterials);

                        await _activityLogService.CreateLog(EnumObjectType.ProductBom, productInfo.ProductId, $"Cập nhật chi tiết bom cho mặt hàng {productInfo.ProductCode}, tên hàng {productInfo.ProductName} (import)", new { productBoms, productMaterials }.JsonSerialize());
                    }
                }
                return true;
            }
        }
    }
}
