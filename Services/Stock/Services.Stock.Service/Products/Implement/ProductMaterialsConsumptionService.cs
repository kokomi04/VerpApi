﻿using AutoMapper;
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
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductMaterialsConsumptionService: IProductMaterialsConsumptionService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        private readonly IProductBomService _productBomService;

        public ProductMaterialsConsumptionService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductBomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IProductBomService productBomService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _productBomService = productBomService;
        }

        public async Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumptionService(int productId)
        {
            var productBom = (await _productBomService.GetBom(productId)).Where(x => !x.IsMaterial);
            var productMap = CalcProductBomTotalQuantity(productBom);

            var materialsConsumptionInheri = (_stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                .Where(x => productBom.Select(x => x.ChildProductId).Contains(x.ProductId))
                .Include(x => x.MaterialsConsumption)
                .ProjectTo<ProductMaterialsConsumptionOutput>(_mapper.ConfigurationProvider)
                .ToList());

            await FindMaterialsConsumpMissing(productId, materialsConsumptionInheri);

            var materialsConsumption = _stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                            .Where(x => x.ProductId == productId)
                            .ProjectTo<ProductMaterialsConsumptionOutput>(_mapper.ConfigurationProvider)
                            .ToList();

            foreach (var group in materialsConsumption.GroupBy(x => x.ProductMaterialsConsumptionGroupId))
            {
                foreach (var m in group)
                {
                    var boms = productBom.Where(x => x.Level == 1);
                    m.MaterialsConsumptionInheri = LoopCalcMaterialsConsump(productBom
                        , boms
                        , materialsConsumptionInheri
                        , productMap
                        , m.MaterialsConsumptionId
                        , m.ProductMaterialsConsumptionGroupId);
                }
            }

            return materialsConsumption.Select(x=>x);
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

            return !groups.Any(x=> x.MaterialsConsumptionId == item.MaterialsConsumptionId);
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
                if(consump != null)
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

        public async Task<bool> UpdateProductMaterialsConsumptionService(int productId, ICollection<ProductMaterialsConsumptionInput> model)
        {
            var product = _stockDbContext.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            if (product == null) 
                throw new BadRequestException(ProductErrorCode.ProductNotFound);

            var materials = await _stockDbContext.ProductMaterialsConsumption
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            var newMaterials = model.Where(x => x.ProductMaterialsConsumptionId == 0).AsQueryable()
                .ProjectTo<ProductMaterialsConsumption>(_mapper.ConfigurationProvider).ToArray();

            foreach(var m in materials)
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

        public async Task<bool> UpdateProductMaterialsConsumptionService(int productId, long productMaterialsConsumptionId, ProductMaterialsConsumptionInput model)
        {
            var product = _stockDbContext.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
                throw new BadRequestException(ProductErrorCode.ProductNotFound);

            var material = _stockDbContext.ProductMaterialsConsumption.AsNoTracking().FirstOrDefault(p => p.ProductMaterialsConsumptionId == productMaterialsConsumptionId);
            if (material == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

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

        public async Task<bool> ImportMaterialsConsumptionFromMapping(int productId, ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);
            var data = reader.ReadSheetEntity<ImportProductMaterialsConsumptionExcelMapping>(mapping, null);

            var products = (await _stockDbContext.Product.AsNoTracking().ToListAsync()).ToDictionary(k => k.ProductCode, v => v.ProductId);
            var oldMaterialConsumption = (await _stockDbContext.ProductMaterialsConsumption.AsNoTracking().ToListAsync()).Select(k => k.MaterialsConsumptionId);

            foreach (var row in data)
            {
                if (!products.ContainsKey(row.ProductCode) || oldMaterialConsumption.Contains(products[row.ProductCode])) continue;

                var item = new ProductMaterialsConsumption
                {
                    MaterialsConsumptionId = products[row.ProductCode],
                    ProductId = productId,
                    Quantity = row.Quantity
                };

                _stockDbContext.ProductMaterialsConsumption.Add(item);
            }

            await _stockDbContext.SaveChangesAsync();
            return true;
        }

    }
}
