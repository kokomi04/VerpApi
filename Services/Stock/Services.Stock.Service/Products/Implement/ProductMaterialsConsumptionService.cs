using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Products.Implement.ProductMaterialsConsumptionFacade;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductMaterialsConsumptionService : IProductMaterialsConsumptionService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        private readonly IProductBomService _productBomService;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IManufacturingHelperService _manufacturingHelperService;

        private readonly IProductService _productService;
        private readonly IUnitService _unitService;

        public ProductMaterialsConsumptionService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductBomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IProductBomService productBomService
            , IProductService productService
            , IUnitService unitService
            , IOrganizationHelperService organizationHelperService
            , IManufacturingHelperService manufacturingHelperService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _productBomService = productBomService;
            _organizationHelperService = organizationHelperService;
            _manufacturingHelperService = manufacturingHelperService;
            _productService = productService;
            _unitService = unitService;
        }


        public async Task<IDictionary<int, IEnumerable<ProductMaterialsConsumptionOutput>>> GetProductMaterialsConsumptionByProductIds(IList<int> productIds)
        {
            var dic = new Dictionary<int, IEnumerable<ProductMaterialsConsumptionOutput>>();
            foreach (var productId in productIds.Distinct())
            {
                dic.Add(productId, await GetProductMaterialsConsumption(productId));
            }

            return dic;
        }

        public async Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumption(int productId)
        {
            var productBom = (await _productBomService.GetBom(productId)).Where(x => !x.IsMaterial);
            var productMap = CalcProductBomTotalQuantity(productBom);

            var materialsConsumptionInheri = (await _stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                .Where(x => productBom.Select(y => y.ChildProductId).Contains(x.ProductId))
                .Include(x => x.MaterialsConsumption)
                .ProjectTo<ProductMaterialsConsumptionOutput>(_mapper.ConfigurationProvider)
                .ToListAsync());

            int minLevel = 1;
            var materialsInheri = LoopGetMaterialConsumInheri(materialsConsumptionInheri, productBom, productBom.Where(x => x.Level == minLevel), productMap);

            var materialsConsumption = await _stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                            .Where(x => x.ProductId == productId)
                            .ProjectTo<ProductMaterialsConsumptionOutput>(_mapper.ConfigurationProvider)
                            .ToListAsync();

            var exceptMaterialsConsumption = materialsInheri.Except(materialsConsumption, new ProductMaterialsConsumptionBaseComparer())
                .Select(x => new ProductMaterialsConsumptionOutput
                {
                    ProductMaterialsConsumptionGroupId = x.ProductMaterialsConsumptionGroupId,
                    MaterialsConsumptionId = x.MaterialsConsumptionId,
                    ProductId = productId,
                    DepartmentId = x.DepartmentId,
                    StepId = x.StepId,
                    UnitId = x.UnitId
                });

            materialsConsumption.AddRange(exceptMaterialsConsumption);
            foreach (var m in materialsConsumption)
            {
                m.MaterialsConsumptionInheri = materialsInheri.Where(x => x.ProductMaterialsConsumptionGroupId == m.ProductMaterialsConsumptionGroupId
                                && x.MaterialsConsumptionId == m.MaterialsConsumptionId).ToList();
                m.BomQuantity = 1;
                m.TotalQuantityInheritance = m.MaterialsConsumptionInheri.Select(x => (x.Quantity * x.BomQuantity) + x.TotalQuantityInheritance).Sum();
            }

            return materialsConsumption;
        }

        private IList<ProductMaterialsConsumptionOutput> LoopGetMaterialConsumInheri(List<ProductMaterialsConsumptionOutput> materialsConsumptionInheri
            , IEnumerable<ProductBomOutput> productBom
            , IEnumerable<ProductBomOutput> boms
            , Dictionary<int?, decimal> productMap)
        {
            var result = new List<ProductMaterialsConsumptionOutput>();
            foreach (var bom in boms)
            {
                var materials = materialsConsumptionInheri.Where(x => x.ProductId == bom.ChildProductId).ToList();
                var childBom = productBom.Where(x => x.ProductId == bom.ChildProductId).ToList();
                var materialsInheri = LoopGetMaterialConsumInheri(materialsConsumptionInheri, productBom, childBom, productMap);

                var exceptMaterials = materialsInheri.Except(materials, new ProductMaterialsConsumptionBaseComparer())
                    .Select(x => new ProductMaterialsConsumptionOutput
                    {
                        ProductMaterialsConsumptionGroupId = x.ProductMaterialsConsumptionGroupId,
                        MaterialsConsumptionId = x.MaterialsConsumptionId,
                        ProductId = bom.ChildProductId.GetValueOrDefault(),
                        DepartmentId = x.DepartmentId,
                        StepId = x.StepId,
                        UnitId = x.UnitId,
                    });

                materials.AddRange(exceptMaterials);
                foreach (var m in materials)
                {
                    m.MaterialsConsumptionInheri = materialsInheri.Where(x => x.ProductMaterialsConsumptionGroupId == m.ProductMaterialsConsumptionGroupId
                                    && x.MaterialsConsumptionId == m.MaterialsConsumptionId).ToList();
                    m.BomQuantity = productMap.ContainsKey(m.ProductId) ? productMap[m.ProductId] : 1;
                    m.TotalQuantityInheritance = m.MaterialsConsumptionInheri.Select(x => (x.Quantity * x.BomQuantity) + x.TotalQuantityInheritance).Sum();
                }

                result.AddRange(materials);
            }

            return result;
        }

        private async Task<bool> FindMaterialsConsumpMissing(int productId, List<ProductMaterialsConsumptionOutput> materialsConsumptionInheri)
        {
            var materialsConsumption = _stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                            .Where(x => x.ProductId == productId)
                            .ProjectTo<ProductMaterialsConsumptionOutput>(_mapper.ConfigurationProvider)
                            .ToList();

            var newTemp = (materialsConsumptionInheri.Where(x => IsCheckNotExistsMaterialsConsumpOrGroup(materialsConsumption, x))
                .Select(x => new ProductMaterialsConsumption
                {
                    ProductId = productId,
                    MaterialsConsumptionId = x.MaterialsConsumptionId,
                    Quantity = 0,
                    ProductMaterialsConsumptionGroupId = x.ProductMaterialsConsumptionGroupId
                })
                .ToArray()).GroupBy(x => new { x.ProductMaterialsConsumptionGroupId, x.MaterialsConsumptionId })
                .Select(x => x.First());

            await _stockDbContext.ProductMaterialsConsumption.AddRangeAsync(newTemp);
            await _stockDbContext.SaveChangesAsync();

            return true;
        }

        private bool IsCheckNotExistsMaterialsConsumpOrGroup(IEnumerable<ProductMaterialsConsumptionOutput> materials, ProductMaterialsConsumptionOutput item)
        {
            if (materials.Count() == 0) return true;

            var groups = materials.Where(x => x.ProductMaterialsConsumptionGroupId == item.ProductMaterialsConsumptionGroupId);
            if (groups.Count() == 0) return true;

            return !groups.Any(x => x.MaterialsConsumptionId == item.MaterialsConsumptionId);
        }

        private IList<ProductMaterialsConsumptionOutput> LoopCalcMaterialsConsump(IEnumerable<ProductBomOutput> productBom
            , IEnumerable<ProductBomOutput> level
            , IEnumerable<ProductMaterialsConsumptionOutput> materialsConsumptions
            , Dictionary<int?, decimal> productMap
            , int consumpId
            , int groupId)
        {
            var result = new List<ProductMaterialsConsumptionOutput>();
            foreach (var bom in level)
            {
                var consump = materialsConsumptions.FirstOrDefault(x => x.ProductId == bom.ChildProductId
                        && x.MaterialsConsumptionId == consumpId
                        && x.ProductMaterialsConsumptionGroupId == groupId);
                if (consump != null)
                {
                    var child = productBom.Where(x => x.ProductId == bom.ChildProductId);
                    consump.BomQuantity = productMap[bom.ChildProductId];
                    consump.MaterialsConsumptionInheri = LoopCalcMaterialsConsump(productBom, child, materialsConsumptions, productMap, consumpId, groupId);

                    result.Add(consump);
                }

            }

            return result;
        }

        private Dictionary<int?, decimal> CalcProductBomTotalQuantity(IEnumerable<ProductBomOutput> productBom)
        {
            var level1 = productBom.Where(x => x.Level == 1).ToArray();
            var productMap = new Dictionary<int?, decimal>();
            foreach (var bom in level1)
            {
                var totalQuantity = bom.Quantity; // không có tiêu hao
                if (productMap.ContainsKey(bom.ChildProductId))
                    productMap[bom.ChildProductId] += totalQuantity;
                else
                    productMap.Add(bom.ChildProductId, totalQuantity);

                var childs = productBom.Where(x => x.ProductId == bom.ChildProductId);

                LoopCalcProductBom(productBom, productMap, childs, totalQuantity);
            }

            return productMap;
        }

        private void LoopCalcProductBom(IEnumerable<ProductBomOutput> productBom, Dictionary<int?, decimal> productMap, IEnumerable<ProductBomOutput> level, decimal scale)
        {
            foreach (var bom in level)
            {
                var totalQuantity = bom.Quantity * bom.Wastage * scale;
                if (productMap.ContainsKey(bom.ChildProductId))
                    productMap[bom.ChildProductId] += totalQuantity;
                else
                    productMap.Add(bom.ChildProductId, totalQuantity);
                var childs = productBom.Where(x => x.ProductId == bom.ChildProductId);
                LoopCalcProductBom(productBom, productMap, childs, totalQuantity);
            }
        }

        public async Task<bool> UpdateProductMaterialsConsumption(int productId, ICollection<ProductMaterialsConsumptionInput> model)
        {
            var product = _stockDbContext.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
                throw new BadRequestException(ProductErrorCode.ProductNotFound);

            var materials = await _stockDbContext.ProductMaterialsConsumption
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            var newMaterials = model.Where(x => x.ProductMaterialsConsumptionId == 0).AsQueryable()
                .ProjectTo<ProductMaterialsConsumption>(_mapper.ConfigurationProvider).ToArray();

            foreach (var m in materials)
            {
                var s = model.FirstOrDefault(x => x.ProductMaterialsConsumptionId == m.ProductMaterialsConsumptionId);
                if (s != null)
                {
                    _mapper.Map(s, m);
                }
                else m.IsDeleted = true;
            }

            _stockDbContext.ProductMaterialsConsumption.AddRange(newMaterials);

            await _stockDbContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.ProductMaterialsConsumption, productId, $"Cập nhật vật tư tiêu hao cho sản phẩm '{product.ProductCode}/ {product.ProductName}'", model.JsonSerialize());
            return true;
        }

        public async Task<bool> UpdateProductMaterialsConsumption(int productId, long productMaterialsConsumptionId, ProductMaterialsConsumptionInput model)
        {
            var product = _stockDbContext.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
                throw new BadRequestException(ProductErrorCode.ProductNotFound);

            var material = _stockDbContext.ProductMaterialsConsumption.FirstOrDefault(p => p.ProductMaterialsConsumptionId == productMaterialsConsumptionId);
            if (material == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            model.ProductId = productId;
            _mapper.Map(model, material);

            await _stockDbContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.ProductMaterialsConsumption, productMaterialsConsumptionId, $"Cập nhật vật tư tiêu hao {productMaterialsConsumptionId}", model.JsonSerialize());
            return true;
        }

        public CategoryNameModel GetCustomerFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                CategoryId = 1,
                CategoryCode = "ProductMaterialsConsumption",
                CategoryTitle = "Định mức vật tư tiêu hao",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = Utils.GetFieldNameModels<ImportProductMaterialsConsumptionExcelMapping>();
            result.Fields = fields;
            return result;
        }

        public async Task<bool> ImportMaterialsConsumptionFromMapping(int? productId, ImportExcelMapping mapping, Stream stream)
        {
            var facade = new ProductMaterialsConsumptionImportFacade(_stockDbContext
                , _organizationHelperService
                , _manufacturingHelperService
                , _activityLogService)
                .SetService(_productService)
                .SetService(_productBomService)
                .SetService(_unitService);

            return await facade.ProcessData(mapping, stream, productId);
        }

        public async Task<long> AddProductMaterialsConsumption(int productId, ProductMaterialsConsumptionInput model)
        {
            var product = _stockDbContext.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
                throw new BadRequestException(ProductErrorCode.ProductNotFound);

            var entity = _mapper.Map<ProductMaterialsConsumption>(model);
            entity.ProductId = productId;

            await _stockDbContext.ProductMaterialsConsumption.AddAsync(entity);
            await _stockDbContext.SaveChangesAsync();
            return entity.ProductMaterialsConsumptionId;
        }

        public async Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumption(int[] productIds)
        {
            var data = new List<ProductMaterialsConsumptionOutput>();
            for (int i = 0; i < productIds.Length; i++)
            {
                var task = GetProductMaterialsConsumption(productIds[i]);
                data.AddRange(await task);
            }

            return data;
        }

        public async Task<(Stream stream, string fileName, string contentType)> ExportProductMaterialsConsumption(int productId)
        {
            var product = _stockDbContext.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
                throw new BadRequestException(ProductErrorCode.ProductNotFound);

            var materialsConsum = await GetProductMaterialsConsumption(productId);
            var exportFacade = new ProductMaterialsConsumptionExportFacade(_stockDbContext, materialsConsum, _organizationHelperService, _manufacturingHelperService);

            return await exportFacade.Export(product.ProductCode);
        }
    }
}
