using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Services.PurchaseOrder.Model.Request;

namespace VErp.Services.PurchaseOrder.Service.Po.Implement.Facade
{
    public class PurchaseOrderParseExcelFacade
    {
        private readonly IProductHelperService _productHelperService;
        public PurchaseOrderParseExcelFacade(IProductHelperService productHelperService)
        {
            _productHelperService = productHelperService;

        }

        public async IAsyncEnumerable<PurchaseOrderInputDetail> ParseInvoiceDetails(ImportExcelMapping mapping, SingleInvoiceStaticContent extra, Stream stream)
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

                if (productInfo == null || productInfo.Count == 0)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy mặt hàng {item.ProductCode} {item.ProductName}");
                }

                if (productInfo.Count > 1)
                {
                    productInfo = productInfo.Where(p => p.ProductName == item.ProductName).ToList();

                    if (productInfo.Count != 1)
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tìm thấy {productInfo.Count} mặt hàng {item.ProductCode} {item.ProductName}");
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
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy đơn vị chuyển đổi {item.ProductUnitConversionName} mặt hàng {item.ProductCode} {item.ProductName}");
                    }
                    if (pus.Count > 1)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tìm thấy {pus.Count} đơn vị chuyển đổi {item.ProductUnitConversionName} mặt hàng {item.ProductCode} {item.ProductName}");
                    }

                    productUnitConversionId = pus[0].ProductUnitConversionId;

                }
                else
                {
                    var puDefault = productInfo[0].StockInfo.UnitConversions.FirstOrDefault(u => u.IsDefault);
                    if (puDefault == null)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Dữ liệu đơn vị tính default lỗi, mặt hàng {item.ProductCode} {item.ProductName}");

                    }
                    productUnitConversionId = puDefault.ProductUnitConversionId;
                }

                yield return new PurchaseOrderInputDetail()
                {
                    OrderCode = item.OrderCode,
                    ProductionOrderCode = item.ProductionOrderCode,
                    Description = item.Description,
                    ProductId = productInfo[0].ProductId.Value,
                    ProviderProductName = item.ProductProviderName,


                    PrimaryQuantity = item.PrimaryQuantity ?? 0,
                    PrimaryUnitPrice = item.PrimaryPrice ?? 0,

                    ProductUnitConversionId = productUnitConversionId,
                    ProductUnitConversionQuantity = item.ProductUnitConversionQuantity ?? 0,
                    ProductUnitConversionPrice = item.ProductUnitConversionPrice ?? 0,

                    IntoMoney = item.IntoMoney,

                    ExchangedMoney = item.ExchangedMoney,

                    SortOrder = item.SortOrder

                    //TaxInMoney = item.TaxInMoney
                };

            }
        }

        private IEnumerable<PoDetailRowValue> SingleInvoiceParseExcel(ImportExcelMapping mapping, SingleInvoiceStaticContent extra, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var data = reader.ReadSheetEntity<PoDetailRowValue>(mapping, (entity, propertyName, value) =>
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
            return data.OrderBy(d=>d.SortOrder).ToList();
        }

    }
}
