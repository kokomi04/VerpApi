using Microsoft.EntityFrameworkCore;
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
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductBomExportFacade
    {
        private StockDBContext _stockDbContext;
        private ISheet sheet = null;
        private int currentRow = 0;
        private int maxColumnIndex = 10;

        private IList<int> productIds;

        public ProductBomExportFacade(StockDBContext stockDbContext, IList<int> productIds)
        {
            _stockDbContext = stockDbContext;
            this.productIds = productIds;
        }


        public async Task<(Stream stream, string fileName, string contentType)> BomExport()
        {

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

            currentRow = currentRowTmp;


            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"product-bom-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            return (stream, fileName, contentType);
        }

        private async Task WriteTable()
        {
            currentRow = 1;

            var fRow = currentRow;
            var sRow = currentRow;

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 0, 0));
            sheet.EnsureCell(fRow, 0).SetCellValue($"STT");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 1, 1));
            sheet.EnsureCell(fRow, 1).SetCellValue($"Mã mặt hàng");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 2, 2));
            sheet.EnsureCell(fRow, 2).SetCellValue($"Tên mặt hàng");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 3, 3));
            sheet.EnsureCell(fRow, 3).SetCellValue($"ĐVT");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, 4, 4));
            sheet.EnsureCell(fRow, 4).SetCellValue($"Quy cách");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 5, 5));
            sheet.EnsureCell(fRow, 5).SetCellValue($"Mã chi tiết");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 6, 6));
            sheet.EnsureCell(fRow, 6).SetCellValue($"Tên chi tiết");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 7, 7));
            sheet.EnsureCell(fRow, 7).SetCellValue($"ĐVT chi tiết");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, 8, 8));
            sheet.EnsureCell(fRow, 8).SetCellValue($"Quy cách chi tiết");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, 9, 9));
            sheet.EnsureCell(fRow, 9).SetCellValue($"Số lượng");

            sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, 10, 10));
            sheet.EnsureCell(fRow, 10).SetCellValue($"Là nguyên liệu");



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
            var processedProductIds = new HashSet<int>();
            var productBoms = new List<ProductBom>();

            var processingProductIds = productIds;
            while (processingProductIds.Count > 0)
            {
                var boms = await _stockDbContext.ProductBom.AsNoTracking().Where(b => processingProductIds.Contains(b.ProductId)).ToListAsync();
                productBoms.AddRange(boms);

                foreach (var productId in productIds)
                {
                    processedProductIds.Add(productId);
                }

                processingProductIds = boms.Where(b => b.ChildProductId.HasValue).Select(b => b.ChildProductId.Value).Where(c => !processedProductIds.Contains(c)).ToList();
            }

            var productMaterial = (await _stockDbContext.ProductMaterial.Where(m => processedProductIds.Contains(m.RootProductId)).AsNoTracking().Select(m => m.ProductId).Distinct().ToListAsync()).ToHashSet();

            var productInfos = (await (
                from p in _stockDbContext.Product
                join d in _stockDbContext.ProductExtraInfo on p.ProductId equals d.ProductId
                join u in _stockDbContext.ProductUnitConversion.Where(pu => pu.IsDefault) on p.ProductId equals u.ProductId
                select new
                {
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    u.ProductUnitConversionName,
                    d.Specification
                }).ToListAsync()
                ).GroupBy(p => p.ProductId)
                .ToDictionary(p => p.Key, p => p.FirstOrDefault());


            var stt = 1;
            foreach (var item in productBoms)
            {
                sheet.EnsureCell(currentRow, 0).SetCellValue(stt);

                productInfos.TryGetValue(item.ProductId, out var productInfo);

                productInfos.TryGetValue(item.ChildProductId ?? 0, out var childProductInfo);

                if (productInfo != null)
                {
                    sheet.EnsureCell(currentRow, 1).SetCellValue(productInfo.ProductCode);
                    sheet.EnsureCell(currentRow, 2).SetCellValue(productInfo.ProductName);
                    sheet.EnsureCell(currentRow, 3).SetCellValue(productInfo.ProductUnitConversionName);
                    sheet.EnsureCell(currentRow, 4).SetCellValue(productInfo.Specification);
                }

                if (childProductInfo != null)
                {
                    sheet.EnsureCell(currentRow, 5).SetCellValue(childProductInfo.ProductCode);
                    sheet.EnsureCell(currentRow, 6).SetCellValue(childProductInfo.ProductName);
                    sheet.EnsureCell(currentRow, 7).SetCellValue(childProductInfo.ProductUnitConversionName);
                    sheet.EnsureCell(currentRow, 8).SetCellValue(childProductInfo.Specification);
                }

                sheet.EnsureCell(currentRow, 10).SetCellValue(Convert.ToDouble(item.Quantity));

                if (productMaterial.Contains(item.ChildProductId ?? 0))
                {
                    sheet.EnsureCell(currentRow, 11).SetCellValue("Có");
                }

                currentRow++;
                stt++;
            }
        }


    }
}
