﻿using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products.Implement.ProductBomFacade
{
    public class ProductBomExportFacade
    {
        private StockDBContext _stockDbContext;
        private ISheet sheet = null;
        private int currentRow = 0;

        const int START_PROP_COLUMN_INDEX = 16;

        private int maxColumnIndex;

        private IList<int> productIds;
        private readonly IList<StepSimpleInfo> steps;
        private readonly IList<PropertyModel> _productBomProperties;

        public ProductBomExportFacade(StockDBContext stockDbContext, IList<int> productIds, IList<StepSimpleInfo> steps, IList<PropertyModel> productBomProperties)
        {
            _stockDbContext = stockDbContext;
            this.productIds = productIds;
            this.steps = steps;
            _productBomProperties = productBomProperties;
            maxColumnIndex = 15 + productBomProperties.Count;
        }


        public async Task<(Stream stream, string fileName, string contentType)> BomExport()
        {

            var xssfwb = new XSSFWorkbook();
            sheet = xssfwb.CreateSheet();


            var firstProductCode = await WriteTable();

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
            var fileName = $"product-bom-{firstProductCode.NormalizeAsInternalName()}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            return (stream, fileName, contentType);
        }




        private async Task<string> WriteTable()
        {
            currentRow = 1;

            var fRow = currentRow;
            var sRow = currentRow;

            sheet.EnsureCell(fRow, 0).SetCellValue($"STT");

            sheet.EnsureCell(fRow, 1).SetCellValue($"Mã mặt hàng");

            sheet.EnsureCell(fRow, 2).SetCellValue($"Tên mặt hàng");

            sheet.EnsureCell(fRow, 3).SetCellValue($"ĐVT");

            sheet.EnsureCell(fRow, 4).SetCellValue($"Quy cách");

            sheet.EnsureCell(fRow, 5).SetCellValue($"Mã chi tiết");

            sheet.EnsureCell(fRow, 6).SetCellValue($"Tên chi tiết");

            sheet.EnsureCell(fRow, 7).SetCellValue($"ĐVT chi tiết");

            sheet.EnsureCell(fRow, 8).SetCellValue($"Quy cách chi tiết");

            sheet.EnsureCell(fRow, 9).SetCellValue($"Số lượng");

            sheet.EnsureCell(fRow, 10).SetCellValue($"Tỷ lệ hao hụt");

            sheet.EnsureCell(fRow, 11).SetCellValue($"Tổng SL");

            sheet.EnsureCell(fRow, 12).SetCellValue($"Mô tả");

            sheet.EnsureCell(fRow, 13).SetCellValue($"Là nguyên liệu");

            sheet.EnsureCell(fRow, 14).SetCellValue($"Cộng đoạn vào");

            sheet.EnsureCell(fRow, 15).SetCellValue($"Công đoạn ra");



            var col = START_PROP_COLUMN_INDEX;

            foreach (var p in _productBomProperties)
            {
                sheet.EnsureCell(fRow, col).SetCellValue(p.PropertyName);
                col++;
            }

            for (var i = fRow; i <= sRow; i++)
            {
                for (var j = 0; j <= maxColumnIndex; j++)
                {
                    sheet.SetHeaderCellStyle(i, j);
                }
            }

            currentRow = sRow + 1;

            return await WriteTableDetailData();
        }


        private async Task<string> WriteTableDetailData()
        {
            var processedProductIds = new HashSet<int>();
            var productBoms = new List<ProductBom>();

            var processingProductIds = productIds;
            while (processingProductIds.Count > 0)
            {
                var boms = await _stockDbContext.ProductBom.AsNoTracking().Where(b => processingProductIds.Contains(b.ProductId)).ToListAsync();
                productBoms.AddRange(boms);

                foreach (var productId in processingProductIds)
                {
                    processedProductIds.Add(productId);
                }

                processingProductIds = boms.Where(b => b.ChildProductId.HasValue).Select(b => b.ChildProductId.Value).Where(c => !processedProductIds.Contains(c)).ToList();
            }

            var productMaterial = (await _stockDbContext.ProductMaterial.Where(m => processedProductIds.Contains(m.RootProductId)).AsNoTracking().Select(m => m.ProductId).Distinct().ToListAsync()).ToHashSet();

            var productBomProperties = await _stockDbContext.ProductProperty.Where(m => processedProductIds.Contains(m.RootProductId)).AsNoTracking().ToListAsync();

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

            var firstProductCode = "";

            object currentProduct = null;
            var mapTotalQuantity = new Dictionary<int?, decimal?>();
            foreach (var item in productBoms)
            {
                sheet.EnsureCell(currentRow, 0).SetCellValue(stt);

                productInfos.TryGetValue(item.ProductId, out var productInfo);

                productInfos.TryGetValue(item.ChildProductId ?? 0, out var childProductInfo);

                var totalQuantity = item.Quantity * item.Wastage * (mapTotalQuantity.ContainsKey(item.ProductId) ? mapTotalQuantity[item.ProductId] : 1);

                if (!mapTotalQuantity.ContainsKey(item.ChildProductId))
                    mapTotalQuantity.Add(item.ChildProductId, totalQuantity);

                if (productInfo != null)
                {
                    if (string.IsNullOrWhiteSpace(firstProductCode))
                    {
                        firstProductCode = productInfo.ProductCode;
                    }

                    sheet.EnsureCell(currentRow, 1).SetCellValue(productInfo.ProductCode);
                    if (currentProduct != productInfo)
                    {
                        currentProduct = productInfo;
                        sheet.EnsureCell(currentRow, 2).SetCellValue(productInfo.ProductName);
                        sheet.EnsureCell(currentRow, 3).SetCellValue(productInfo.ProductUnitConversionName);
                        sheet.EnsureCell(currentRow, 4).SetCellValue(productInfo.Specification);
                    }
                }

                if (childProductInfo != null)
                {
                    sheet.EnsureCell(currentRow, 5).SetCellValue(childProductInfo.ProductCode);
                    sheet.EnsureCell(currentRow, 6).SetCellValue(childProductInfo.ProductName);
                    sheet.EnsureCell(currentRow, 7).SetCellValue(childProductInfo.ProductUnitConversionName);
                    sheet.EnsureCell(currentRow, 8).SetCellValue(childProductInfo.Specification);
                }

                sheet.EnsureCell(currentRow, 9).SetCellValue(Convert.ToDouble(item.Quantity));
                sheet.EnsureCell(currentRow, 10).SetCellValue(Convert.ToDouble(item.Wastage));
                sheet.EnsureCell(currentRow, 11).SetCellValue(Convert.ToDouble(totalQuantity));

                sheet.EnsureCell(currentRow, 12).SetCellValue(item.Description);


                if (productMaterial.Contains(item.ChildProductId ?? 0))
                {
                    sheet.EnsureCell(currentRow, 13).SetCellValue("Có");
                    sheet.EnsureCell(currentRow, 13).CellStyle.Alignment = HorizontalAlignment.Center;
                    //sheet.EnsureCell(currentRow, 10).CellStyle.VerticalAlignment = VerticalAlignment.Center;
                }

                sheet.EnsureCell(currentRow, 14).SetCellValue(GetStepName(item.InputStepId));
                sheet.EnsureCell(currentRow, 15).SetCellValue(GetStepName(item.OutputStepId));

                var col = START_PROP_COLUMN_INDEX;

                foreach (var p in _productBomProperties)
                {
                    if (productBomProperties.Any(prop => prop.ProductId == item.ChildProductId && prop.PropertyId == p.PropertyId))
                    {
                        sheet.EnsureCell(currentRow, col).SetCellValue("Có");
                        sheet.EnsureCell(currentRow, col).CellStyle.Alignment = HorizontalAlignment.Center;
                    }

                    col++;
                }

                currentRow++;
                stt++;
            }

            return firstProductCode;
        }

        private string GetStepName(int? stepId)
        {
            if (!stepId.HasValue) return string.Empty;

            var step = steps.FirstOrDefault(x => x.StepId == stepId.Value);
            if (step == null) return string.Empty;

            return step.StepName;
        }
    }
}
