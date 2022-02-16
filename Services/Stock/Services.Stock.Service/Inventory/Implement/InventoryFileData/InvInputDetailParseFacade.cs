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
using static VErp.Services.Stock.Model.Inventory.InvOutDetailRowValue;
using static Verp.Resources.Stock.Inventory.InventoryFileData.InventoryDetailParseFacadeMessage;
using VErp.Services.Stock.Model.Package;

namespace VErp.Services.Stock.Service.Stock.Implement.InventoryFileData
{
    public class InvInputDetailParseFacade
    {
        private IProductService productService;
        private StockDBContext stockDbContext;
        public InvInputDetailParseFacade SetProductService(IProductService productService)
        {
            this.productService = productService;
            return this;
        }

        public InvInputDetailParseFacade SetStockDbContext(StockDBContext stockDbContext)
        {
            this.stockDbContext = stockDbContext;
            return this;
        }

        public async IAsyncEnumerable<InvInputDetailRowValue> ParseExcel(ImportExcelMapping mapping, Stream stream, int stockId)
        {
            var reader = new ExcelReader(stream);

            var customProps = await stockDbContext.PackageCustomProperty.ToListAsync();

            var rowDatas = reader.ReadSheetEntity<InventoryInputExcelParseModel>(mapping, (entity, propertyName, value, refObj, refProperty) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;
                switch (propertyName)
                {
                    case nameof(InventoryInputExcelParseModel.PackageOptionId):
                        if (value.IsRangeOfAllowValueForBooleanTrueValue())
                        {
                            entity.PackageOptionId = EnumPackageOption.Create;
                        }
                        else
                        {
                            entity.PackageOptionId = EnumPackageOption.NoPackageManager;
                        }
                        return true;

                    case nameof(InventoryInputExcelParseModel.ToPackgeInfo):

                        if (refProperty?.StartsWith(nameof(PackageInputModel.CustomPropertyValue)) == true)
                        {
                            var packageInfo = (PackageInputModel)refObj;
                            var customPropertyId = Convert.ToInt32(refProperty.Substring(nameof(PackageInputModel.CustomPropertyValue).Length));

                            var propertyInfo = customProps.FirstOrDefault(p => p.PackageCustomPropertyId == customPropertyId);

                            if (propertyInfo == null) throw GeneralCode.ItemNotFound.BadRequest("Property " + customPropertyId + " was not found!");

                            if (packageInfo.CustomPropertyValue == null)
                            {
                                packageInfo.CustomPropertyValue = new Dictionary<int, object>();
                            }

                            if (!packageInfo.CustomPropertyValue.ContainsKey(customPropertyId))
                            {
                                var customValue = value.ConvertValueByType((EnumDataType)propertyInfo.DataTypeId);

                                packageInfo.CustomPropertyValue.Add(customPropertyId, customValue);
                                return true;
                            }
                        }

                        break;
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

            var packageCodes = rowDatas.SelectMany(r => new[] { r.ToPackgeInfo.PackageCode }).Distinct().ToList();

            var productPackages = (await stockDbContext.Package.Where(p => p.StockId == stockId && (packageCodes.Contains(p.PackageCode) || p.PackageTypeId == (int)EnumPackageType.Default) && productIds.Contains(p.ProductId)).ToListAsync())
                .GroupBy(p => p.ProductId)
                .ToDictionary(p => p.Key, p => p.ToList());

            var unitIds = productInfos.Select(p => p.UnitId).ToList();

            foreach (var item in rowDatas)
            {
                if (item.PackageOptionId == EnumPackageOption.NoPackageManager && !string.IsNullOrWhiteSpace(item.ToPackgeInfo.PackageCode))
                {
                    item.PackageOptionId = EnumPackageOption.Append;
                }

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



                productPackages.TryGetValue(productInfo[0].ProductId.Value, out var packages);
                if (packages == null)
                {
                    packages = new List<Package>();
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

                    productUnitConversionId = pus[0].ProductUnitConversionId;
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

                Package packageInfo = null;

                if (!string.IsNullOrWhiteSpace(item.ToPackgeInfo.PackageCode) && item.PackageOptionId == EnumPackageOption.Append)
                {
                    var packageByCodes = packages?.Where(p => p.PackageCode.Equals(item.ToPackgeInfo.PackageCode, StringComparison.OrdinalIgnoreCase))?.ToList();

                    if (packageByCodes?.Count == 1)
                    {
                        if (string.IsNullOrWhiteSpace(item.ProductUnitConversionName))
                        {
                            packageInfo = packageByCodes[0];
                            productUnitConversionId = packageByCodes[0].ProductUnitConversionId;
                        }
                        else
                        {
                            if (item.PackageOptionId == EnumPackageOption.Append && packageByCodes[0].ProductUnitConversionId != productUnitConversionId)
                            {
                                throw PuConversionDiffToPackage.BadRequestFormat(item.ProductUnitConversionName, packageInfo.PackageCode, $"{item.ProductCode} {item.ProductName}");
                            }
                            else
                            {
                                packageInfo = packageByCodes[0];
                            }
                        }

                    }
                    else if (packageByCodes?.Count > 1 && !string.IsNullOrWhiteSpace(item.ProductUnitConversionName))
                    {
                        packageInfo = packageByCodes.FirstOrDefault(p => p.ProductUnitConversionId == productUnitConversionId);
                    }

                    if (packageInfo == null && item.PackageOptionId == EnumPackageOption.Append)
                    {
                        throw PackageCodeOfProductNotFound.BadRequestFormat(item.ToPackgeInfo.PackageCode, $"{item.ProductCode} {item.ProductName}");

                    }
                }


                var puInfo = productInfo[0].StockInfo.UnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);


                yield return new InvInputDetailRowValue()
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
                    //AccountancyAccountNumberDu = item.AccountancyAccountNumberDu,
                    PackageOptionId = item.PackageOptionId,

                    ToPackageId = packageInfo?.PackageId,
                    ToPackageCode = packageInfo?.PackageCode,
                    ToPackageInfo = packageInfo != null ? null : item.ToPackgeInfo
                };

            }

        }

    }
}
