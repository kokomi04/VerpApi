using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Service.Products;
using static VErp.Services.Stock.Model.Inventory.InventoryDetailRowValue;
using static VErp.Services.Stock.Service.Resources.Inventory.InventoryFileData.InventoryDetailParseFacadeMessage;

namespace VErp.Services.Stock.Service.Stock.Implement.InventoryFileData
{
    public class InventoryDetailParseFacade
    {
        private IProductService productService;
        private StockDBContext stockDbContext;
        public InventoryDetailParseFacade SetProductService(IProductService productService)
        {
            this.productService = productService;
            return this;
        }

        public InventoryDetailParseFacade SetStockDbContext(StockDBContext stockDbContext)
        {
            this.stockDbContext = stockDbContext;
            return this;
        }

        public async IAsyncEnumerable<InventoryDetailRowValue> ParseExcel(ImportExcelMapping mapping, Stream stream, EnumInventoryType inventoryTypeId)
        {
            var reader = new ExcelReader(stream);

            var rowDatas = reader.ReadSheetEntity<InventoryExcelParseModel>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;
                switch (propertyName)
                {
                    case nameof(InventoryExcelParseModel.PackageOptionId):                        
                        if (value.IsRangeOfAllowValueForBooleanTrueValue())
                        {
                            entity.PackageOptionId = EnumPackageOption.Create;
                        }
                        else
                        {
                            entity.PackageOptionId = EnumPackageOption.NoPackageManager;
                        }
                        return true;
                }

                return false;
            });


            var productCodes = rowDatas.Select(r => r.ProductCode).ToList();
            var productInternalNames = rowDatas.Select(r => r.ProductName.NormalizeAsInternalName()).ToList();

            var productInfos = await productService.GetListByCodeAndInternalNames(new Model.Product.ProductQueryByProductCodeOrInternalNameRequest() { ProductCodes = productCodes, ProductInternalNames = productInternalNames });

            var productInfoByCode = productInfos.GroupBy(p => p.ProductCode)
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            var productInfoByInternalName = productInfos.GroupBy(p => p.ProductName.NormalizeAsInternalName())
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());


            var productIds = productInfos.Select(p => p.ProductId).ToList();

            var packageCodes = rowDatas.SelectMany(r => new[] { r.FromPackageCode, r.ToPackageCode }).Distinct().ToList();

            var productPackages = (await stockDbContext.Package.Where(p => (packageCodes.Contains(p.PackageCode) || p.PackageTypeId == (int)EnumPackageType.Default) && productIds.Contains(p.ProductId)).ToListAsync())
                .GroupBy(p => p.ProductId)
                .ToDictionary(p => p.Key, p => p.ToList());

            var unitIds = productInfos.Select(p => p.UnitId).ToList();

            foreach (var item in rowDatas)
            {
                IList<ProductModel> productInfo = null;
                if (!string.IsNullOrWhiteSpace(item.ProductCode) && productInfoByCode.ContainsKey(item.ProductCode?.ToLower()))
                {
                    productInfo = productInfoByCode[item.ProductCode?.ToLower()];
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(item.ProductName) && productInfoByInternalName.ContainsKey(item.ProductName.NormalizeAsInternalName()))
                    {
                        productInfo = productInfoByInternalName[item.ProductName.NormalizeAsInternalName()];
                    }
                }

                if (productInfo == null || productInfo.Count == 0)
                {
                    throw GeneralCode.ItemNotFound.BadRequestFormat(ProductNotFound, $"{item.ProductCode} {item.ProductName}");
                }

                if (productInfo.Count > 1)
                {
                    productInfo = productInfo.Where(p => p.ProductName == item.ProductName).ToList();

                    if (productInfo.Count != 1)
                        throw ProductFoundMoreThanOne.BadRequestFormat(productInfo.Count, $"{item.ProductCode} {item.ProductName}");
                }

                long? toPackageId = null;
                long? fromPackageId = null;
                productPackages.TryGetValue(productInfo[0].ProductId.Value, out var packages);
                if (packages == null)
                {
                    packages = new List<Package>();
                }

                Package packageInfo = null;
                if (!string.IsNullOrWhiteSpace(item.FromPackageCode))
                {
                    packageInfo = packages?.FirstOrDefault(p => p.PackageCode.Equals(item.FromPackageCode, StringComparison.OrdinalIgnoreCase));
                    if (packageInfo == null)
                    {
                        throw PackageCodeOfProductNotFound.BadRequestFormat(item.FromPackageCode, $"{item.ProductCode} {item.ProductName}");
                    }
                    fromPackageId = packageInfo.PackageId;
                }


                if (!string.IsNullOrWhiteSpace(item.ToPackageCode))
                {
                    packageInfo = packages?.FirstOrDefault(p => p.PackageCode.Equals(item.ToPackageCode, StringComparison.OrdinalIgnoreCase));
                    if (packageInfo == null)
                    {
                        throw PackageCodeOfProductNotFound.BadRequestFormat(item.ToPackageCode, $"{item.ProductCode} {item.ProductName}");

                    }
                    toPackageId = packageInfo.PackageId;
                }

                var productUnitConversionId = 0;
                if (!string.IsNullOrWhiteSpace(item.ProductUnitConversionName))
                {
                    var pus = productInfo[0].StockInfo.UnitConversions
                            .Where(u => u.ProductUnitConversionName.NormalizeAsInternalName() == item.ProductUnitConversionName.NormalizeAsInternalName())
                            .ToList();

                    if (pus.Count != 1)
                    {
                        pus = productInfo[0].StockInfo.UnitConversions
                           .Where(u => u.ProductUnitConversionName.Contains(item.ProductUnitConversionName) || item.ProductUnitConversionName.Contains(u.ProductUnitConversionName))
                           .ToList();

                        if (pus.Count > 1)
                        {
                            pus = productInfo[0].StockInfo.UnitConversions
                             .Where(u => u.ProductUnitConversionName.Equals(item.ProductUnitConversionName, StringComparison.OrdinalIgnoreCase))
                             .ToList();
                        }
                    }

                    if (pus.Count == 0)
                    {
                        throw PuConversionOfProductNotFound.BadRequestFormat(item.ProductUnitConversionName, $"{item.ProductCode} {item.ProductName}");                        
                    }
                    if (pus.Count > 1)
                    {
                        throw PuConversionOfProductFoundMoreThanOne.BadRequestFormat(pus.Count, item.ProductUnitConversionName, $"{item.ProductCode} {item.ProductName}");
                    }

                    if (packageInfo != null && packageInfo.ProductUnitConversionId != pus[0].ProductUnitConversionId)
                    {
                        throw PuConversionDiffToPackage.BadRequestFormat(item.ProductUnitConversionName, packageInfo.PackageCode, $"{item.ProductCode} {item.ProductName}");
                    }
                    productUnitConversionId = pus[0].ProductUnitConversionId;
                }
                else
                {

                    if (packageInfo != null)
                    {
                        productUnitConversionId = packageInfo.ProductUnitConversionId;
                    }
                    else
                    {
                        var puDefault = productInfo[0].StockInfo.UnitConversions.FirstOrDefault(u => u.IsDefault);
                        if (puDefault == null)
                        {
                            throw PuConversionDefaultError.BadRequestFormat($"{item.ProductCode} {item.ProductName}");

                        }

                        productUnitConversionId = puDefault.ProductUnitConversionId;

                    }

                }



                if (inventoryTypeId == EnumInventoryType.Output && packageInfo == null)
                {

                    packageInfo = packages?.FirstOrDefault(p => p.PackageTypeId == (int)EnumPackageType.Default && p.ProductUnitConversionId == productUnitConversionId);
                    if (packageInfo == null)
                    {
                        throw NoPackageForInventoryOutput.BadRequestFormat($"{item.ProductCode} {item.ProductName}");
                    }
                    fromPackageId = packageInfo.PackageId;

                }

                var puInfo = productInfo[0].StockInfo.UnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);

                InventoryDetailRowPackage packageOutput = null;
                if (packageInfo != null)
                {
                    packageOutput = new InventoryDetailRowPackage()
                    {
                        PackageId = packageInfo.PackageId,
                        PackageCode = packageInfo.PackageCode,
                        PrimaryQuantityRemaining = packageInfo.PrimaryQuantityRemaining,
                        ProductUnitConversionRemaining = packageInfo.ProductUnitConversionRemaining
                    };
                }

                yield return new InventoryDetailRowValue()
                {
                    ProductId = productInfo[0].ProductId.Value,
                    ProductCode = productInfo[0].ProductCode,
                    ProductName = productInfo[0].ProductName,

                    PrimaryUnitId = productInfo[0].UnitId,
                    PrimaryUnitName = productInfo[0].StockInfo.UnitConversions.FirstOrDefault(c => c.IsDefault)?.ProductUnitConversionName,


                    PrimaryQuantity = item.PrimaryQuantity,
                    PrimaryPrice = item.UnitPrice,

                    ProductUnitConversionId = productUnitConversionId,
                    ProductUnitConversionName = puInfo?.ProductUnitConversionName,
                    ProductUnitConversionExpression = puInfo?.FactorExpression,

                    ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
                    ProductUnitConversionPrice = item.ProductUnitConversionPrice,

                    PoCode = item.PoCode,
                    OrderCode = item.OrderCode,
                    ProductionOrderCode = item.ProductionOrderCode,
                    Description = item.Description,
                    AccountancyAccountNumberDu = item.AccountancyAccountNumberDu,
                    PackageOptionId = item.PackageOptionId,
                    FromPackageId = fromPackageId,
                    ToPackageId = toPackageId,
                    FromPackageCode = item.FromPackageCode,
                    ToPackageCode = item.ToPackageCode,
                    PackageInfo = packageOutput
                };

            }

        }

    }
}
