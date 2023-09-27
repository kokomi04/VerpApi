﻿using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Hr;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Manufacture;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products.Implement.ProductMaterialsConsumptionFacade
{
    public class ProductMaterialsConsumptionExportFacade
    {
        private StockDBContext _stockDbContext;
        private ISheet sheet = null;
        private int currentRow = 0;
        private int maxColumnIndex = 14;

        private IList<IEnumerable<ProductMaterialsConsumptionOutput>> materialsConsumps;

        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IManufacturingHelperService _manufacturingHelperService;

        public ProductMaterialsConsumptionExportFacade(StockDBContext stockDbContext
            , IList<IEnumerable<ProductMaterialsConsumptionOutput>> materials
            , IOrganizationHelperService organizationHelperService
            , IManufacturingHelperService manufacturingHelperService)
        {
            _stockDbContext = stockDbContext;
            materialsConsumps = materials;
            _organizationHelperService = organizationHelperService;
            _manufacturingHelperService = manufacturingHelperService;
        }

        public async Task<(Stream stream, string fileName, string contentType)> Export(string productCode)
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
            var fileName = StringUtils.RemoveDiacritics($"{productCode.NormalizeAsInternalName()} product materials consumption.xlsx").Replace(" ", "#");
            return (stream, fileName, contentType);
        }

        private async Task<bool> WriteTable()
        {
            currentRow = 0;

            var fRow = currentRow;
            var sRow = currentRow;

            sheet.EnsureCell(fRow, 0).SetCellValue($"STT");

            /* Thông tin Nvl tiêu hao */
            sheet.EnsureCell(fRow, 1).SetCellValue($"Mã Nvl tiêu hao");

            sheet.EnsureCell(fRow, 2).SetCellValue($"Tên Nvl tiêu hao");

            sheet.EnsureCell(fRow, 3).SetCellValue($"ĐVT Nvl tiêu hao");

            // sheet.EnsureCell(fRow, 4).SetCellValue($"Loại mã mặt hàng của Nvl tiêu hao");

            sheet.EnsureCell(fRow, 4).SetCellValue($"Quy cách Nvl tiêu hao");

            // sheet.EnsureCell(fRow, 6).SetCellValue($"Danh mục Nvl tiêu hao");

            /* Thông tin chi tiết sử dụng Nvl tiêu hao */
            sheet.EnsureCell(fRow, 5).SetCellValue($"Mã chi tiết sử dụng");

            sheet.EnsureCell(fRow, 6).SetCellValue($"Tên chi tiết sử dụng");

            sheet.EnsureCell(fRow, 7).SetCellValue($"ĐVT chi tiết sử dụng");

            // sheet.EnsureCell(fRow, 10).SetCellValue($"Loại mã mặt hàng của chi tiết sử dụng");

            sheet.EnsureCell(fRow, 8).SetCellValue($"Quy cách chi tiết sử dụng");

            // sheet.EnsureCell(fRow, 12).SetCellValue($"Danh mục chi tiết sử dụng");

            /* Thông tin chung */
            sheet.EnsureCell(fRow, 9).SetCellValue($"Số lượng sử dụng");

            sheet.EnsureCell(fRow, 10).SetCellValue($"Tỷ lệ hao hụt");

            sheet.EnsureCell(fRow, 11).SetCellValue($"Tổng vật tư sử dụng");

            sheet.EnsureCell(fRow, 12).SetCellValue($"Công đoạn");

            sheet.EnsureCell(fRow, 13).SetCellValue($"Mã bộ phận");

            sheet.EnsureCell(fRow, 14).SetCellValue($"Bộ phận");

            sheet.EnsureCell(fRow, 15).SetCellValue($"Ghi chú");

            sheet.EnsureCell(fRow, 16).SetCellValue($"Nhóm Nvl tiêu hao");



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

        private async Task<bool> WriteTableDetailData()
        {
            var index = 1;
            foreach (var materialsConsump in materialsConsumps)
            {
                var productId = materialsConsump.FirstOrDefault()?.ProductId ??0;

                var groupsConsump = (await _stockDbContext.ProductMaterialsConsumptionGroup
                .AsNoTracking()
                .Where(m => materialsConsump.Select(x => x.ProductMaterialsConsumptionGroupId).Contains(m.ProductMaterialsConsumptionGroupId))
                .ToListAsync())
                .ToDictionary(k => k.ProductMaterialsConsumptionGroupId, v => v.Title);

                var stepInfos = (await _manufacturingHelperService.GetStepByArrayId(materialsConsump.Select(x => x.StepId.GetValueOrDefault()).ToArray()))
                    .ToDictionary(k => k.StepId, v => v.StepName);
                var departmentInfos = (await _organizationHelperService.GetDepartmentSimples(materialsConsump.Select(x => x.DepartmentId.GetValueOrDefault()).ToArray()))
                    .ToDictionary(k => k.DepartmentId, v => new { v.DepartmentCode, v.DepartmentName }); ;

                var productInfos = (await (
                    from p in _stockDbContext.Product
                    join t in _stockDbContext.ProductType on p.ProductTypeId equals t.ProductTypeId into lt
                    from t in lt.DefaultIfEmpty()
                    join c in _stockDbContext.ProductCate on p.ProductCateId equals c.ProductCateId into lc
                    from c in lc.DefaultIfEmpty()
                    join d in _stockDbContext.ProductExtraInfo on p.ProductId equals d.ProductId
                    join u in _stockDbContext.ProductUnitConversion.Where(pu => pu.IsDefault) on p.ProductId equals u.ProductId
                    select new
                    {
                        p.ProductId,
                        p.ProductCode,
                        p.ProductName,
                        u.ProductUnitConversionName,
                        d.Specification,
                        c.ProductCateName,
                        t.ProductTypeName
                    }).ToListAsync()
                    ).GroupBy(p => p.ProductId)
                    .ToDictionary(p => p.Key, p => p.FirstOrDefault());

                var styleNumber = sheet.GetCellStyle(vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Right, isWrap: true, isBorder: false);
                var styleText = sheet.GetCellStyle(vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Left, isWrap: true, isBorder: false);

                var materialConsumptionSlice = GetMaterialConsumptionSlices(materialsConsump).Where(x => x.Quantity > 0)
                    .GroupBy(x => new { x.MaterialsConsumptionId, x.ProductId })
                    .SelectMany(x => x)
                    .OrderBy(x => x.ProductMaterialsConsumptionGroupId);
                foreach (var m in materialConsumptionSlice)
                {
                    productInfos.TryGetValue(m.MaterialsConsumptionId, out var materialConsumptionInfo);
                    productInfos.TryGetValue(m.ProductId, out var productInfo);

                    sheet.EnsureCell(currentRow, 0, styleText).SetCellValue(index);

                    if (groupsConsump.ContainsKey(m.ProductMaterialsConsumptionGroupId))
                        sheet.EnsureCell(currentRow, 16, styleText).SetCellValue(groupsConsump[m.ProductMaterialsConsumptionGroupId]);

                    if (materialConsumptionInfo != null)
                    {
                        sheet.EnsureCell(currentRow, 1, styleText).SetCellValue(materialConsumptionInfo.ProductCode);
                        sheet.EnsureCell(currentRow, 2, styleText).SetCellValue(materialConsumptionInfo.ProductName);
                        sheet.EnsureCell(currentRow, 3, styleText).SetCellValue(materialConsumptionInfo.ProductUnitConversionName);
                        // sheet.EnsureCell(currentRow, 4, styleText).SetCellValue(materialConsumptionInfo.ProductTypeName);
                        sheet.EnsureCell(currentRow, 4, styleText).SetCellValue(materialConsumptionInfo.Specification);
                        // sheet.EnsureCell(currentRow, 6, styleText).SetCellValue(materialConsumptionInfo.ProductCateName);
                    }

                    if (productInfo != null)
                    {
                        sheet.EnsureCell(currentRow, 5, styleText).SetCellValue(productInfo.ProductCode);
                        sheet.EnsureCell(currentRow, 6, styleText).SetCellValue(productInfo.ProductName);
                        sheet.EnsureCell(currentRow, 7, styleText).SetCellValue(productInfo.ProductUnitConversionName);
                        // sheet.EnsureCell(currentRow, 10, styleText).SetCellValue(productInfo.ProductTypeName);
                        sheet.EnsureCell(currentRow, 8, styleText).SetCellValue(productInfo.Specification);
                        // sheet.EnsureCell(currentRow, 12, styleText).SetCellValue(productInfo.ProductCateName);
                    }

                    sheet.EnsureCell(currentRow, 9, styleNumber).SetCellValue(Convert.ToDouble(m.Quantity));
                    sheet.EnsureCell(currentRow,10, styleNumber).SetCellValue(Convert.ToDouble(m.Wastage));
                    sheet.EnsureCell(currentRow,11, styleNumber).SetCellValue(((Convert.ToDouble(m.Quantity) * Convert.ToDouble(m.Wastage)) + Convert.ToDouble(m.TotalQuantityInheritance)) * Convert.ToDouble( m.BomQuantity));

                    if (m.StepId.HasValue && stepInfos.ContainsKey((int)m.StepId))
                        sheet.EnsureCell(currentRow, 12, styleText).SetCellValue(stepInfos[(int)m.StepId]);

                    if (m.DepartmentId.HasValue && departmentInfos.ContainsKey((int)m.DepartmentId))
                    {
                        var department = departmentInfos[(int)m.DepartmentId];
                        sheet.EnsureCell(currentRow, 13, styleText).SetCellValue(department.DepartmentCode);
                        sheet.EnsureCell(currentRow, 14, styleText).SetCellValue(department.DepartmentName);
                    }

                    sheet.EnsureCell(currentRow, 15, styleText).SetCellValue(m.Description);

                    currentRow++;
                    index++;
                }
            }
            return true;
        }

        private IEnumerable<ProductMaterialsConsumptionOutput> GetMaterialConsumptionSlices(IEnumerable<ProductMaterialsConsumptionOutput> group)
        {
            var results = new List<ProductMaterialsConsumptionOutput>();
            foreach (var item in group)
            {
                results.Add(item);
                if (item.MaterialsConsumptionInheri.Count() > 0)
                    results.AddRange(GetMaterialConsumptionSlices(item.MaterialsConsumptionInheri));
            }
            return results;
        }
    }
}
