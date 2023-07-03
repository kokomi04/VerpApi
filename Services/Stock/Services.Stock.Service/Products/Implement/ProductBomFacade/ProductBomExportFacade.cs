using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.Organization;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
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
        private readonly IProductBomService _productBomService;

        public ProductBomExportFacade(StockDBContext stockDbContext, IList<int> productIds, IList<StepSimpleInfo> steps, IList<PropertyModel> productBomProperties, IProductBomService productBomService)
        {
            _stockDbContext = stockDbContext;
            this.productIds = productIds;
            this.steps = steps;
            _productBomProperties = productBomProperties;
            maxColumnIndex = 15 + productBomProperties.Count;
            _productBomService = productBomService;
        }


        public async Task<(Stream stream, string fileName, string contentType)> BomExport(bool isFindTopBOM = false)
        {

            var xssfwb = new XSSFWorkbook();
            sheet = xssfwb.CreateSheet();


            var firstProductCode = await WriteTable(isFindTopBOM);

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
            var fileName = StringUtils.RemoveDiacritics($"{firstProductCode.NormalizeAsInternalName()} product bom.xlsx").Replace(" ", "#");
            return (stream, fileName, contentType);
        }




        private async Task<string> WriteTable(bool isFindTopBOM)
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

            return await WriteTableDetailData(isFindTopBOM);
        }

        private async Task<string> WriteTableDetailData(bool isFindTopBOM)
        {
            IList<int> topMostProductIds = new List<int>();

            if (!isFindTopBOM)
            {
                topMostProductIds = productIds;
            }
            else
            {
                var checkParams = new[]
                {
                     productIds.ToSqlParameter("@InputProductIds")
                };
                var productParentIds = (await _stockDbContext.ExecuteDataProcedure("asp_GetTopMostBomProductIds", checkParams)).ConvertData();
                foreach (var p in productParentIds)
                {
                    topMostProductIds.Add(Convert.ToInt32(p["ProductId"]));
                }
            }
            
            var productBomsLevels = await _productBomService.GetBoms(topMostProductIds);
            var productMaterial = (await _stockDbContext.ProductMaterial.Where(m => topMostProductIds.Contains(m.RootProductId)).AsNoTracking().Select(m => m.ProductId).Distinct().ToListAsync()).ToHashSet();

            var productBomProperties = await _stockDbContext.ProductProperty.Where(m => topMostProductIds.Contains(m.RootProductId)).AsNoTracking().ToListAsync();

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

            var mapTotalQuantity = new Dictionary<int?, decimal?>();
            foreach (var productBoms in productBomsLevels)
            {
                foreach (var item in productBoms.Value)
                {

                    sheet.EnsureCell(currentRow, 0).SetCellValue(stt);
                    productInfos.TryGetValue(item.ProductId, out var productInfo);
                    productInfos.TryGetValue(item.ChildProductId ?? 0, out var childProductInfo);

                    if (productInfo != null )
                    {
                        if (string.IsNullOrWhiteSpace(firstProductCode))
                        {
                            firstProductCode = productInfo.ProductCode;
                        }
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

                    sheet.EnsureCell(currentRow, 9).SetCellValue(Convert.ToDouble(item.Quantity));
                    sheet.EnsureCell(currentRow, 10).SetCellValue(Convert.ToDouble(item.Wastage));
                    sheet.EnsureCell(currentRow, 11).SetCellValue(Convert.ToDouble(item.TotalQuantity));

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
