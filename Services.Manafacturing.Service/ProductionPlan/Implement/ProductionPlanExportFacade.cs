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
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionPlan;

namespace VErp.Services.Manafacturing.Service.ProductionPlan.Implement
{
    public class ProductionPlanExportFacade
    {
        private IProductionPlanService _productionPlanService;
        private IProductHelperService _productHelperService;
        private IProductBomHelperService _productBomHelperService;
        private IProductCateHelperService _productCateHelperService;
        private IList<ProductionOrderListModel> productionPlanInfo = null;
        private IList<InternalProductCateOutput> productCates = null;
        private IDictionary<int, Dictionary<int, decimal>> productCateQuantity = new Dictionary<int, Dictionary<int, decimal>>();
        private ISheet sheet = null;
        private int currentRow = 0;
        private int maxColumnIndex = 0;

        public ProductionPlanExportFacade SetProductionPlanService(IProductionPlanService productionPlanService)
        {
            _productionPlanService = productionPlanService;
            return this;
        }
        public ProductionPlanExportFacade SetProductHelperService(IProductHelperService productHelperService)
        {
            _productHelperService = productHelperService;
            return this;
        }
        public ProductionPlanExportFacade SetProductBomHelperService(IProductBomHelperService productBomHelperService)
        {
            _productBomHelperService = productBomHelperService;
            return this;
        }
        public ProductionPlanExportFacade SetProductCateHelperService(IProductCateHelperService productCateHelperService)
        {
            _productCateHelperService = productCateHelperService;
            return this;
        }
        public async Task<(Stream stream, string fileName, string contentType)> Export(long startDate, long endDate, ProductionPlanExportModel data, IList<string> mappingFunctionKeys = null)
        {
            maxColumnIndex = 11 + data.ProductCateIds.Length;
            productionPlanInfo = await _productionPlanService.GetProductionOrders(startDate, endDate);
            productCates = (await _productCateHelperService.Search(null, string.Empty, -1, -1, string.Empty, true)).List.Where(pc => data.ProductCateIds.Contains(pc.ProductCateId)).ToList();
            var productIds = productionPlanInfo.Select(p => p.ProductId.Value).Distinct().ToList();

            var products = await _productHelperService.GetByIds(productIds);

            var productElements = await _productBomHelperService.GetElements(productIds.ToArray());

            // map decimal place
            foreach (var plan in productionPlanInfo)
            {
                var product = products.FirstOrDefault(p => p.ProductId == plan.ProductId);
                plan.DecimalPlace = product != null ? product.DecimalPlace : 5;

            }

            // Xử lý tính toán số lượng chi tiết
            foreach (var productId in productIds)
            {
                var product = products.FirstOrDefault(p => p.ProductId == productId);
                productCateQuantity.Add(productId, new Dictionary<int, decimal>());

                foreach (var productCate in productCates)
                {
                    productCateQuantity[productId].Add(productCate.ProductCateId, 0);
                    // Nếu sản phẩm thuộc danh mục => thêm số lượng vào
                    if (product != null && product.ProductCateId == productCate.ProductCateId)
                    {
                        this.productCateQuantity[productId][productCate.ProductCateId] = 1;
                    }
                    else if (product != null && !productCates.Any(pc => pc.ProductCateId == product.ProductCateId))
                    {
                        var elementQuantity = productElements.Where(pe => pe.ParentProductId == product.ProductId && pe.ProductCateId == productCate.ProductCateId)
                        .Sum(pe => pe.Quantity.GetValueOrDefault() * pe.Wastage.GetValueOrDefault());

                        productCateQuantity[product.ProductId][productCate.ProductCateId] = elementQuantity;
                    }
                }
            }

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

            WriteGeneralInfo(data.MonthPlanName);

            currentRow = currentRowTmp;
            WriteFooter(data.Note);

            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"KHSX_{startDate}_{endDate}.xlsx";
            return (stream, fileName, contentType);
        }


        private void WriteGeneralInfo(string monthPlanName)
        {
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, maxColumnIndex));
            sheet.EnsureCell(0, 0).SetCellValue($"LỊCH SẢN XUẤT XƯỞNG THÁNG {monthPlanName}");
            sheet.GetRow(0).Height = 1500;
            sheet.SetCellStyle(0, 0, 20, true, false, VerticalAlignment.Center, HorizontalAlignment.Center, false);
        }


        private async Task WriteTable()
        {
            currentRow = 2;

            var fRow = currentRow;

            sheet.EnsureCell(fRow, 0).SetCellValue($"Khách hàng");
            sheet.EnsureCell(fRow, 1).SetCellValue($"Đơn hàng");
            sheet.EnsureCell(fRow, 2).SetCellValue($"PO của khách");
            sheet.EnsureCell(fRow, 3).SetCellValue($"Mã hàng");
            sheet.EnsureCell(fRow, 4).SetCellValue($"Tên hàng");
            sheet.EnsureCell(fRow, 5).SetCellValue($"Số cont");
            sheet.EnsureCell(fRow, 6).SetCellValue($"Số lượng");
            sheet.EnsureCell(fRow, 7).SetCellValue($"Đơn vị");
            sheet.EnsureCell(fRow, 8).SetCellValue($"Ngày bắt đầu");
            sheet.EnsureCell(fRow, 9).SetCellValue($"Hoàn thành hàng trắng");
            sheet.EnsureCell(fRow, 10).SetCellValue($"Ngày hoàn thành");
            sheet.EnsureCell(fRow, 11).SetCellValue($"Ghi chú");
            int colIndx = 12;
            foreach (var productCate in productCates)
            {
                sheet.EnsureCell(fRow, colIndx).SetCellValue($"{productCate.ProductCateName}");
                colIndx++;
            }


            for (var j = 0; j <= maxColumnIndex; j++)
            {
                sheet.SetHeaderCellStyle(fRow, j);
            }

            currentRow = fRow + 1;

            await WriteTableDetailData();
        }


        private async Task WriteTableDetailData()
        {

            // var centerCell = sheet.GetCellStyle(hAlign: HorizontalAlignment.Center, isBorder: true);

            var normalCell = sheet.GetCellStyle(isBorder: true);

            var numberCell = sheet.GetCellStyle(isBorder: true, dataFormat: "#,##0");
            var dateCell = sheet.GetCellStyle(isBorder: true, dataFormat: "dd/MM/yyyy");

            foreach (var item in productionPlanInfo)
            {
                for (var i = 0; i <= maxColumnIndex; i++)
                {
                    var style = normalCell;
                    if (i == 6)
                    {
                        style = numberCell;
                    }
                    else if(i == 8 || i == 9 || i == 10)
                    {
                        style = dateCell;
                    }
                    sheet.EnsureCell(currentRow, i).CellStyle = style;
                }

                sheet.EnsureCell(currentRow, 0).SetCellValue(string.IsNullOrEmpty(item.PartnerName) ? item.PartnerCode : $"{item.PartnerCode} ({item.PartnerName})");
                sheet.EnsureCell(currentRow, 1).SetCellValue(item.OrderCode);
                sheet.EnsureCell(currentRow, 2).SetCellValue(item.CustomerPO);
                sheet.EnsureCell(currentRow, 3).SetCellValue(item.ProductCode);
                sheet.EnsureCell(currentRow, 4).SetCellValue(item.ProductName);
                sheet.EnsureCell(currentRow, 5).SetCellValue(item.ContainerNumber);
                sheet.EnsureCell(currentRow, 6).SetCellValue((double)(item.Quantity.GetValueOrDefault() + item.ReserveQuantity.GetValueOrDefault()));
                sheet.EnsureCell(currentRow, 7).SetCellValue(item.UnitName);
                sheet.EnsureCell(currentRow, 8).SetCellValue(item.StartDate.UnixToDateTime().Value);
                sheet.EnsureCell(currentRow, 9).SetCellValue(item.PlanEndDate.UnixToDateTime().Value);
                sheet.EnsureCell(currentRow, 10).SetCellValue(item.EndDate.UnixToDateTime().Value);
                sheet.EnsureCell(currentRow, 11).SetCellValue(item.Note);

                int colIndx = 12;
                foreach (var productCate in productCates)
                {
                    sheet.EnsureCell(currentRow, colIndx).SetCellValue((double)((item.Quantity.GetValueOrDefault() + item.ReserveQuantity.GetValueOrDefault()) * productCateQuantity[item.ProductId.Value][productCate.ProductCateId]));
                    colIndx++;
                }

                currentRow++;
            }
        }

        private void WriteFooter(string note)
        {
            var fRow = currentRow + 2;
            var sRow = currentRow + 2;

            sheet.EnsureCell(fRow, 0).SetCellValue($"Ghi chú");
            sheet.SetCellStyle(fRow, 0, 12, false, false, VerticalAlignment.Center, HorizontalAlignment.Center, true);

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 1, maxColumnIndex));
            sheet.EnsureCell(fRow, 1).SetCellValue(note);

            var normalCell = sheet.GetCellStyle(isBorder: true);
            for (var j = 1; j <= maxColumnIndex; j++)
            {
                sheet.EnsureCell(fRow, j).CellStyle = normalCell;
            }

            sheet.SetCellStyle(fRow, 1, 12, false, false, VerticalAlignment.Center, HorizontalAlignment.Left, true, true);

            sheet.GetRow(fRow).Height = 1700;

            currentRow++;
        }
    }
}
