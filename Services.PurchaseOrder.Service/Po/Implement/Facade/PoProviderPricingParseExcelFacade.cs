using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.PurchaseOrder.Model.PoProviderPricing;
using static Verp.Resources.PurchaseOrder.Po.PurchaseOrderParseExcelValidationMessage;

namespace VErp.Services.PurchaseOrder.Service.Po.Implement.Facade
{
    public class PoProviderPricingParseExcelFacade
    {
        private readonly IProductHelperService _productHelperService;
        public PoProviderPricingParseExcelFacade(IProductHelperService productHelperService)
        {
            _productHelperService = productHelperService;

        }

        public async IAsyncEnumerable<PoProviderPricingOutputDetail> ParseInvoiceDetails(ImportExcelMapping mapping, Stream stream)
        {
            var rowDatas = SingleInvoiceParseExcel(mapping, stream).ToList();

            var productCodes = rowDatas.Select(r => r.ProductCode).ToList();
            var productInternalNames = rowDatas.Select(r => r.ProductInternalName).ToList();

            var productInfos = await _productHelperService.GetListByCodeAndInternalNames(productCodes, productInternalNames);

            var productInfoByCode = productInfos.GroupBy(p => p.ProductCode)
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            var productInfoByInternalName = productInfos.GroupBy(p => p.ProductName.NormalizeAsInternalName())
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            foreach (var item in rowDatas)
            {
                IList<ProductModel> itemProducts = null;
                if (!string.IsNullOrWhiteSpace(item.ProductCode) && productInfoByCode.ContainsKey(item.ProductCode?.ToLower()))
                {
                    itemProducts = productInfoByCode[item.ProductCode?.ToLower()];
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(item.ProductInternalName) && productInfoByInternalName.ContainsKey(item.ProductInternalName))
                    {
                        itemProducts = productInfoByInternalName[item.ProductInternalName];
                    }
                }

                if (itemProducts == null || itemProducts.Count == 0)
                {
                    throw ProductInfoNotFound.BadRequestFormat($"{item.ProductCode} {item.ProductName}");
                }

                if (itemProducts.Count > 1)
                {
                    itemProducts = itemProducts.Where(p => p.ProductName == item.ProductName).ToList();

                    if (itemProducts.Count != 1)
                        throw FoundNumberOfProduct.BadRequestFormat(itemProducts.Count, $"{item.ProductCode} {item.ProductName}");
                }

                var productUnitConversionId = 0;
                if (!string.IsNullOrWhiteSpace(item.ProductUnitConversionName))
                {
                    var pus = itemProducts[0].StockInfo.UnitConversions
                            .Where(u => u.ProductUnitConversionName.NormalizeAsInternalName() == item.ProductUnitConversionName.NormalizeAsInternalName())
                            .ToList();

                    if (pus.Count != 1)
                    {
                        pus = itemProducts[0].StockInfo.UnitConversions
                           .Where(u => u.ProductUnitConversionName.Contains(item.ProductUnitConversionName) || item.ProductUnitConversionName.Contains(u.ProductUnitConversionName))
                           .ToList();

                        if (pus.Count > 1)
                        {
                            pus = itemProducts[0].StockInfo.UnitConversions
                             .Where(u => u.ProductUnitConversionName.Equals(item.ProductUnitConversionName, StringComparison.OrdinalIgnoreCase))
                             .ToList();
                        }
                    }

                    if (pus.Count == 0)
                    {
                        throw PuOfProductNotFound.BadRequestFormat(item.ProductUnitConversionName, $"{item.ProductCode} {item.ProductName}");
                    }
                    if (pus.Count > 1)
                    {
                        throw FoundNumberOfPuConversion.BadRequestFormat(pus.Count, item.ProductUnitConversionName, $"{item.ProductCode} {item.ProductName}");
                    }

                    productUnitConversionId = pus[0].ProductUnitConversionId;

                }
                else
                {
                    var puDefault = itemProducts[0].StockInfo.UnitConversions.FirstOrDefault(u => u.IsDefault);
                    if (puDefault == null)
                    {
                        throw PrimaryPuOfProductNotFound.BadRequestFormat($"{item.ProductCode} {item.ProductName}");

                    }
                    productUnitConversionId = puDefault.ProductUnitConversionId;
                }

                yield return new PoProviderPricingOutputDetail()
                {
                    OrderCode = item.OrderCode,
                    ProductionOrderCode = item.ProductionOrderCode,
                    Description = item.Description,
                    ProductId = itemProducts[0].ProductId.Value,
                    ProviderProductName = item.ProductProviderName,


                    PrimaryQuantity = item.PrimaryQuantity ?? 0,
                    PrimaryUnitPrice = item.PrimaryPrice ?? 0,

                    ProductUnitConversionId = productUnitConversionId,
                    ProductUnitConversionQuantity = item.ProductUnitConversionQuantity ?? 0,
                    ProductUnitConversionPrice = item.ProductUnitConversionPrice ?? 0,

                    IntoMoney = item.IntoMoney,

                    ExchangedMoney = item.ExchangedMoney,

                    SortOrder = item.SortOrder,
                    //TaxInMoney = item.TaxInMoney
                };

            }
        }

        private IEnumerable<PoPricingDetailRow> SingleInvoiceParseExcel(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var data = reader.ReadSheetEntity<PoPricingDetailRow>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;

                return false;
            });
            foreach (var item in data)
            {
                item.ProductInternalName = item.ProductName?.NormalizeAsInternalName();
            }
            return data.OrderBy(d => d.SortOrder).ToList();
        }

    }
}
