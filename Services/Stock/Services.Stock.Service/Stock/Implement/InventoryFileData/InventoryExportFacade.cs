using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;

namespace VErp.Services.Stock.Service.Stock.Implement.InventoryFileData
{
    public class InventoryExportFacade
    {
        private ICurrentContextService _currentContextService;
        private IInventoryService _inventoryService;
        private IOrganizationHelperService _organizationHelperService;
        private IStockHelperService _stockHelperService;
        private IProductHelperService _productHelperService;

        private BusinessInfoModel bussinessInfo = null;
        private BaseCustomerModel customerInfo = null;
        private SimpleStockInfo stockInfo = null;
        private InventoryOutput inventoryInfo = null;
        private EnumInventoryType inventoryTypeId = EnumInventoryType.Input;
        private ISheet sheet = null;
        private int currentRow = 0;
        private int maxColumnIndex = 15;

        public InventoryExportFacade SetCurrentContext(ICurrentContextService currentContextService)
        {
            _currentContextService = currentContextService;
            return this;
        }

        public InventoryExportFacade SetInventoryService(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            return this;
        }

        public InventoryExportFacade SetOrganizationHelperService(IOrganizationHelperService organizationHelperService)
        {
            _organizationHelperService = organizationHelperService;
            return this;
        }

        public InventoryExportFacade SetStockHelperService(IStockHelperService stockHelperService)
        {
            _stockHelperService = stockHelperService;
            return this;
        }

        public InventoryExportFacade SetProductHelperService(IProductHelperService productHelperService)
        {
            _productHelperService = productHelperService;
            return this;
        }

        public async Task<(Stream stream, string fileName, string contentType)> InventoryInfoExport(long inventoryId, IList<string> mappingFunctionKeys = null)
        {
            inventoryInfo = await _inventoryService.InventoryInfo(inventoryId, mappingFunctionKeys);

            inventoryTypeId = (EnumInventoryType)inventoryInfo.InventoryTypeId;

            if (inventoryInfo.CustomerId > 0)
            {
                customerInfo = await _organizationHelperService.CustomerInfo(inventoryInfo.CustomerId.Value);
            }

            bussinessInfo = await _organizationHelperService.BusinessInfo();

            stockInfo = await _stockHelperService.StockInfo(inventoryInfo.StockId);


            var xssfwb = new XSSFWorkbook();
            sheet = xssfwb.CreateSheet();


            await WriteTable();

            var currentRowTmp = currentRow;

            for (var i = 1; i <= maxColumnIndex; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            for (var i = 1; i <= maxColumnIndex; i++)
            {
                var c = sheet.GetColumnWidth(i);
                if (c < 2000)
                {
                    sheet.SetColumnWidth(i, 2000);
                }
            }

            WriteGeneralInfo();

            currentRow = currentRowTmp;
            WriteFooter();

            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"{inventoryInfo?.InventoryCode}.xlsx";
            return (stream, fileName, contentType);
        }


        private void WriteGeneralInfo()
        {
            sheet.SetCellStyle(1, 0, isBold: true, isItalic: true).SetCellValue($"Đơn vị:");
            sheet.SetCellStyle(1, 1, isBold: true, isItalic: true).SetCellValue($"{bussinessInfo?.CompanyName}");

            sheet.SetCellStyle(2, 0, isBold: true, isItalic: true).SetCellValue($"Địa chỉ:");
            sheet.SetCellStyle(2, 1, isBold: true, isItalic: true).SetCellValue($"{bussinessInfo?.Address}");

            sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, maxColumnIndex));

            sheet.SetCellStyle(5, 0, 16, true, false, VerticalAlignment.Center, HorizontalAlignment.Center)
                .SetCellValue(inventoryTypeId == EnumInventoryType.Input ? "PHIẾU NHẬP KHO" : "PHIẾU XUẤT KHO");

            sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, maxColumnIndex));
            var date = inventoryInfo.Date.UnixToDateTime(_currentContextService.TimeZoneOffset);
            sheet.SetCellStyle(6, 0, hAlign: HorizontalAlignment.Center, isItalic: true).SetCellValue($"Ngày {date.Day} tháng {date.Month} năm {date.Year}");

            sheet.AddMergedRegion(new CellRangeAddress(7, 7, 0, maxColumnIndex));
            sheet.SetCellStyle(7, 0, hAlign: HorizontalAlignment.Center).SetCellValue($"Mã phiếu: {inventoryInfo.InventoryCode}");


            var orderStartColumn = 8;


            sheet.AddMergedRegion(new CellRangeAddress(9, 9, 0, 1));
            sheet.SetCellStyle(9, 0).SetCellValue($"Họ tên người bàn giao:");
            sheet.SetCellStyle(9, 2).SetCellValue($"{inventoryInfo.Shipper}");

            sheet.AddMergedRegion(new CellRangeAddress(9, 9, orderStartColumn, orderStartColumn + 1));
            sheet.SetCellStyle(9, orderStartColumn).SetCellValue($"Khách hàng:");
            sheet.SetCellStyle(9, orderStartColumn + 2).SetCellValue($"{customerInfo?.CustomerName}");


            sheet.AddMergedRegion(new CellRangeAddress(10, 10, 0, 1));
            if (inventoryTypeId == EnumInventoryType.Input)
            {
                sheet.SetCellStyle(10, 0).SetCellValue($"Lý do nhập kho:");
            }
            else
            {
                sheet.SetCellStyle(10, 0).SetCellValue($"Lý do xuất kho:");
            }
            sheet.SetCellStyle(10, 2).SetCellValue($"{inventoryInfo.Content}");


            sheet.AddMergedRegion(new CellRangeAddress(10, 10, orderStartColumn, orderStartColumn + 1));
            sheet.SetCellStyle(10, orderStartColumn).SetCellValue($"Địa chỉ:");
            sheet.SetCellStyle(10, orderStartColumn + 2).SetCellValue($"{customerInfo?.Address}");


            sheet.AddMergedRegion(new CellRangeAddress(11, 11, 0, 1));
            if (inventoryTypeId == EnumInventoryType.Input)
            {
                sheet.SetCellStyle(11, 0).SetCellValue($"Nhập vào kho:");
            }
            else
            {
                sheet.SetCellStyle(11, 0).SetCellValue($"Xuất từ kho:");
            }
            sheet.SetCellStyle(11, 2).SetCellValue($"{stockInfo.StockName}");


            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 12, 13));
            sheet.SetCellStyle(1, 12, isItalic: true).SetCellValue($"Mẫu hóa đơn:");

            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 14, 15));
            sheet.SetCellStyle(1, 14, hAlign: HorizontalAlignment.Right, isItalic: true).SetCellValue($"{inventoryInfo?.BillForm}");


            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 12, 13));
            sheet.SetCellStyle(2, 12, isItalic: true).SetCellValue($"Serial hóa đơn:");

            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 14, 15));
            sheet.SetCellStyle(2, 14, hAlign: HorizontalAlignment.Right, isItalic: true).SetCellValue($"{inventoryInfo?.BillSerial}");


            sheet.AddMergedRegion(new CellRangeAddress(3, 3, 12, 13));
            sheet.SetCellStyle(3, 12, isItalic: true).SetCellValue($"Mã hóa đơn:");

            sheet.AddMergedRegion(new CellRangeAddress(3, 3, 14, 15));
            sheet.SetCellStyle(3, 14, hAlign: HorizontalAlignment.Right, isItalic: true).SetCellValue($"{inventoryInfo?.BillCode}");


            sheet.AddMergedRegion(new CellRangeAddress(4, 4, 12, 13));
            sheet.SetCellStyle(4, 12, isItalic: true).SetCellValue($"Ngày hóa đơn:");

            sheet.AddMergedRegion(new CellRangeAddress(4, 4, 14, 15));
            sheet.SetCellStyle(4, 14, hAlign: HorizontalAlignment.Right, isItalic: true).SetCellValue($"{inventoryInfo.BillDate.UnixToDateTime(_currentContextService.TimeZoneOffset)?.ToString("dd/MM/yyyy")}");

            currentRow = 12;
        }


        private async Task WriteTable()
        {
            currentRow = 12;

            var fRow = currentRow;
            var sRow = currentRow + 1;

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 0, 0));
            sheet.EnsureCell(fRow, 0).SetCellValue($"STT");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 1, 1));
            sheet.EnsureCell(fRow, 1).SetCellValue($"Mã mặt hàng");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 2, 2));
            sheet.EnsureCell(fRow, 2).SetCellValue($"Mặt hàng, quy cách");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 3, 3));
            sheet.EnsureCell(fRow, 3).SetCellValue($"ĐVT");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, 4, 5));
            sheet.EnsureCell(fRow, 4).SetCellValue($"Số lượng");

            sheet.EnsureCell(sRow, 4).SetCellValue($"Yêu cầu");
            sheet.EnsureCell(sRow, 5).SetCellValue($"Thực nhập");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 6, 6));
            sheet.EnsureCell(fRow, 6).SetCellValue($"Đơn giá");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 7, 7));
            sheet.EnsureCell(fRow, 7).SetCellValue($"ĐVCĐ");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, 8, 9));
            sheet.EnsureCell(fRow, 8).SetCellValue($"Số lượng");

            sheet.EnsureCell(sRow, 8).SetCellValue($"Yêu cầu");
            sheet.EnsureCell(sRow, 9).SetCellValue($"Thực nhập");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 10, 10));
            sheet.EnsureCell(fRow, 10).SetCellValue($"Đơn giá");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 11, 11));
            sheet.EnsureCell(fRow, 11).SetCellValue($"Thành tiền");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 12, 12));
            sheet.EnsureCell(fRow, 12).SetCellValue($"Mã PO");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 13, 13));
            sheet.EnsureCell(fRow, 13).SetCellValue($"Mã ĐH");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 14, 14));
            sheet.EnsureCell(fRow, 14).SetCellValue($"Mã LSX");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 15, 15));
            sheet.EnsureCell(fRow, 15).SetCellValue($"Ghi chú");

            for (var i = fRow; i <= sRow; i++)
            {
                for (var j = 0; j <= maxColumnIndex; j++)
                {
                    sheet.SetHeaderCellStyle(i, j);
                }
            }

            currentRow = sRow + 1;

            await WriteTableDetailData();
        }


        private async Task WriteTableDetailData()
        {
            var productIds = inventoryInfo.InventoryDetailOutputList.Select(p => p.ProductId).ToList();
            var productInfos = (await _productHelperService.GetListProducts(productIds)).ToDictionary(p => p.ProductId, p => p);

            var stt = 1;
            foreach (var item in inventoryInfo.InventoryDetailOutputList)
            {
                sheet.EnsureCell(currentRow, 0).SetCellValue(stt);

                productInfos.TryGetValue(item.ProductId, out var productInfo);

                var defaultPu = productInfo?.StockInfo?.UnitConversions?.FirstOrDefault(u => u.IsDefault);
                var pu = productInfo?.StockInfo?.UnitConversions?.FirstOrDefault(u => u.ProductUnitConversionId == item.ProductUnitConversionId);
                var isUsePu = defaultPu != pu;

                sheet.EnsureCell(currentRow, 1).SetCellValue(productInfo?.ProductCode);
                sheet.EnsureCell(currentRow, 2).SetCellValue(productInfo?.ProductName);
                sheet.EnsureCell(currentRow, 3).SetCellValue(defaultPu?.ProductUnitConversionName);


                if (item.RequestPrimaryQuantity.HasValue)
                {
                    sheet.EnsureCell(currentRow, 4).SetCellValue(Convert.ToDouble(item.RequestPrimaryQuantity));
                }
                else
                {
                    sheet.EnsureCell(currentRow, 4);
                }

                sheet.EnsureCell(currentRow, 5).SetCellValue(Convert.ToDouble(item.PrimaryQuantity));
                sheet.EnsureCell(currentRow, 6).SetCellValue(Convert.ToDouble(item.UnitPrice));

                if (isUsePu)
                {
                    sheet.EnsureCell(currentRow, 7).SetCellValue(pu?.ProductUnitConversionName);
                }
                else
                {
                    sheet.EnsureCell(currentRow, 7);
                }

                if (isUsePu && item.RequestProductUnitConversionQuantity.HasValue)
                {
                    sheet.EnsureCell(currentRow, 8).SetCellValue(Convert.ToDouble(item.RequestProductUnitConversionQuantity));
                }
                else
                {
                    sheet.EnsureCell(currentRow, 8);
                }

                if (isUsePu && item.ProductUnitConversionQuantity.HasValue)
                {
                    sheet.EnsureCell(currentRow, 9).SetCellValue(Convert.ToDouble(item.ProductUnitConversionQuantity));
                }
                else
                {
                    sheet.EnsureCell(currentRow, 9);
                }


                if (isUsePu && item.ProductUnitConversionPrice.HasValue)
                {
                    sheet.EnsureCell(currentRow, 10).SetCellValue(Convert.ToDouble(item.ProductUnitConversionPrice));
                }
                else
                {
                    sheet.EnsureCell(currentRow, 10);
                }

                sheet.EnsureCell(currentRow, 11).SetCellValue(Convert.ToDouble(item.PrimaryQuantity * item.UnitPrice));

                sheet.EnsureCell(currentRow, 12).SetCellValue(item.POCode);

                sheet.EnsureCell(currentRow, 13).SetCellValue(item.OrderCode);

                sheet.EnsureCell(currentRow, 14).SetCellValue(item.ProductionOrderCode);

                sheet.EnsureCell(currentRow, 15).SetCellValue(item.Description);

                for (var i = 0; i <= maxColumnIndex; i++)
                {
                    var cell = sheet.GetRow(currentRow).GetCell(i);
                    if (cell != null)
                        cell.CellStyle = sheet.GetCellStyle(hAlign: (i == 0 || i == 3 || i == 6) ? (HorizontalAlignment?)HorizontalAlignment.Center : null, isBorder: true);
                }

                currentRow++;
                stt++;
            }
        }

        private void WriteFooter()
        {
            var fRow = currentRow + 2;
            var sRow = currentRow + 2;

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 0, 1));
            sheet.EnsureCell(fRow, 0).SetCellValue($"Người lập phiếu\r\n(Ký, họ tên)");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 2, 3));
            sheet.EnsureCell(fRow, 2).SetCellValue($"Người bàn giao\r\n(Ký, họ tên)");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 4, 7));
            sheet.EnsureCell(fRow, 4).SetCellValue($"Thủ kho\r\n(Ký, họ tên");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 8, 11));
            sheet.EnsureCell(fRow, 8).SetCellValue($"Kế toán trưởng\r\n(Ký, họ tên)");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, 12, 15));
            sheet.EnsureCell(fRow, 12).SetCellValue($"Giám đốc\r\n(Ký, họ tên)");

            for (var i = fRow; i <= sRow; i++)
            {
                for (var j = 0; j <= maxColumnIndex; j++)
                {
                    sheet.SetSignatureCellStyle(i, j);
                }
            }

            sheet.GetRow(fRow).Height = 1700;

            currentRow++;
        }
    }
}
