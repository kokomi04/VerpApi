
using System;
using System.Linq;
using System.Collections.Generic;
using VErp.Commons.Library;
using VErp.Commons.GlobalObject;
using VErp.Services.PurchaseOrder.Model;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using System.IO;
using VErp.Services.PurchaseOrder.Model.Request;
using VErp.Commons.GlobalObject.InternalDataInterface;
using Verp.Resources.PurchaseOrder.PurchasingRequest;
using VErp.Commons.Library.Model;
namespace VErp.Services.PurchaseOrder.Service.Po.Implement.Facade {
    public class PurchasingRequestParseExcelFacade
    {
        private readonly IProductHelperService _productHelperService;
        public PurchasingRequestParseExcelFacade(IProductHelperService productHelperService)
        {
            _productHelperService = productHelperService;

        }
        public async IAsyncEnumerable<PurchasingRequestInputDetail> ParseInvoiceDetails(ImportExcelMapping mapping, SingleInvoiceStaticContent extra, Stream stream)
        {
            var rowDatas = SingleInvoiceParseExcel(mapping, extra, stream).ToList();

            var productCodes = rowDatas.Select(r => r.ProductCode).ToList();
            var productInternalNames = rowDatas.Select(r => r.ProductInternalName).ToList();

            var productInfos = await _productHelperService.GetListByCodeAndInternalNames(productCodes, productInternalNames);

            var productInfoByCode = productInfos.GroupBy(p => p.ProductCode)
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            var productInfoByInternalName = productInfos.GroupBy(p => p.ProductName.NormalizeAsInternalName())
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            foreach (var item in rowDatas)
            {
                IList<ProductModel> productInfo = null;
                if (!string.IsNullOrWhiteSpace(item.ProductCode) && productInfoByCode.ContainsKey(item.ProductCode?.ToLower()))
                {
                    productInfo = productInfoByCode[item.ProductCode?.ToLower()];
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(item.ProductInternalName) && productInfoByInternalName.ContainsKey(item.ProductInternalName))
                    {
                        productInfo = productInfoByInternalName[item.ProductInternalName];
                    }
                }

                var itemMessage = $"{item.ProductCode} {item.ProductName}";
                if (productInfo == null || productInfo.Count == 0)
                {
                    throw PurchasingRequestMessage.NoProductFound.BadRequestFormat(itemMessage);
                }

                if (productInfo.Count > 1)
                {
                    productInfo = productInfo.Where(p => p.ProductName == item.ProductName).ToList();

                    if (productInfo.Count != 1)
                    {
                        throw PurchasingRequestMessage.FoundNumberProduct.BadFormat()
                            .Add(productInfo.Count)
                            .Add(itemMessage)
                            .Build();
                    }
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
                        throw PurchasingRequestMessage.NoPuOfProductFound.BadFormat()
                            .Add(item.ProductUnitConversionName)
                            .Add($"{item.ProductCode} {item.ProductName}")
                            .Build();
                    }
                    if (pus.Count > 1)
                    {
                        throw PurchasingRequestMessage.FoundNumberPuOfProduct.BadFormat()
                            .Add(pus.Count)
                            .Add(item.ProductUnitConversionName)
                            .Add($"{item.ProductCode} {item.ProductName}")
                            .Build();
                    }

                    productUnitConversionId = pus[0].ProductUnitConversionId;

                }
                else
                {
                    var puDefault = productInfo[0].StockInfo.UnitConversions.FirstOrDefault(u => u.IsDefault);
                    if (puDefault == null)
                    {
                        throw PurchasingRequestMessage.PuDefaultError
                            .BadRequestFormat($"{item.ProductCode} {item.ProductName}");

                    }
                    productUnitConversionId = puDefault.ProductUnitConversionId;
                }

                yield return new PurchasingRequestInputDetail()
                {
                    OrderCode = item.OrderCode,
                    ProductionOrderCode = item.ProductionOrderCode,
                    Description = item.Description,
                    ProductId = productInfo[0].ProductId.Value,
                    PrimaryQuantity = item.PrimaryQuantity,
                    ProductUnitConversionId = productUnitConversionId,
                    ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
                };

            }
        }

        private IEnumerable<PurchasingRequestDetailRowValue> SingleInvoiceParseExcel(ImportExcelMapping mapping, SingleInvoiceStaticContent extra, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var data = reader.ReadSheetEntity<PurchasingRequestDetailRowValue>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;

                return false;
            });
            foreach (var item in data)
            {
                if (string.IsNullOrWhiteSpace(item.OrderCode))
                {
                    item.OrderCode = extra.OrderCode;
                }
                if (string.IsNullOrWhiteSpace(item.ProductionOrderCode))
                {
                    item.ProductionOrderCode = extra.ProductionOrderCode;
                }

                if (string.IsNullOrWhiteSpace(item.ProductUnitConversionName))
                {
                    item.ProductUnitConversionName = extra.ProductUnitConversionName;
                }

                item.ProductInternalName = item.ProductName?.NormalizeAsInternalName();
            }
            return data;
        }

    }
}
