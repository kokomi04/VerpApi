﻿using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products.Implement.ProductMaterialsConsumptionFacade
{
    public class ProductMaterialsConsumptionExportFacade
    {
        private StockDBContext _stockDbContext;
        private ISheet sheet = null;
        private int currentRow = 0;
        private int maxColumnIndex = 10;

        private IEnumerable<ProductMaterialsConsumptionOutput> materialsConsumps;

        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IManufacturingHelperService _manufacturingHelperService;

        public ProductMaterialsConsumptionExportFacade(StockDBContext stockDbContext
            , IEnumerable<ProductMaterialsConsumptionOutput> materials
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
            var fileName = $"product-materials-consumption-{productCode.NormalizeAsInternalName()}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            return (stream, fileName, contentType);
        }

        private async Task<bool> WriteTable()
        {
            currentRow = 0;

            var fRow = currentRow;
            var sRow = currentRow;

            sheet.EnsureCell(fRow, 0).SetCellValue($"Nhóm NVL");

            sheet.EnsureCell(fRow, 1).SetCellValue($"Mã NVL");

            sheet.EnsureCell(fRow, 2).SetCellValue($"Tên NVL");

            sheet.EnsureCell(fRow, 3).SetCellValue($"ĐVT");

            sheet.EnsureCell(fRow, 4).SetCellValue($"Loại mã NVL");

            sheet.EnsureCell(fRow, 5).SetCellValue($"Quy cách NVL");

            sheet.EnsureCell(fRow, 6).SetCellValue($"Danh mục NVL");

            sheet.EnsureCell(fRow, 7).SetCellValue($"Số lượng");

            sheet.EnsureCell(fRow, 8).SetCellValue($"Công đoạn");

            sheet.EnsureCell(fRow, 9).SetCellValue($"Mã bộ phận");

            sheet.EnsureCell(fRow, 10).SetCellValue($"Bộ phận");



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
            var groupsConsump = (await _stockDbContext.ProductMaterialsConsumptionGroup
                .AsNoTracking()
                .Where(m => materialsConsumps.Select(x=>x.ProductMaterialsConsumptionGroupId).Contains(m.ProductMaterialsConsumptionGroupId))
                .ToListAsync())
                .ToDictionary( k=> k.ProductMaterialsConsumptionGroupId, v=>v.Title);

            var stepInfos = (await _manufacturingHelperService.GetStepByArrayId(materialsConsumps.Select(x => x.StepId.GetValueOrDefault()).ToArray()))
                .ToDictionary(k => k.StepId, v => v.StepName);
            var departmentInfos = (await _organizationHelperService.GetDepartmentSimples(materialsConsumps.Select(x => x.DepartmentId.GetValueOrDefault()).ToArray()))
                .ToDictionary(k => k.DepartmentId, v => new { v.DepartmentCode , v.DepartmentName}); ;

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

            var styleNumber = sheet.GetCellStyle(vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Right, isWrap: true, isBorder: false, dataFormat: "#,##0.00");
            var styleText = sheet.GetCellStyle(vAlign: VerticalAlignment.Top, hAlign: HorizontalAlignment.Left, isWrap: true, isBorder: false);

            foreach (var group in materialsConsumps.GroupBy(x=>x.ProductMaterialsConsumptionGroupId))
            {
                if(groupsConsump.ContainsKey(group.Key))
                    sheet.EnsureCell(currentRow, 0, styleText).SetCellValue(groupsConsump[group.Key]);

                foreach(var m in group)
                {
                    productInfos.TryGetValue(m.MaterialsConsumptionId, out var productInfo);

                        if (productInfo != null)
                    {
                        sheet.EnsureCell(currentRow, 1, styleText).SetCellValue(productInfo.ProductCode);
                        sheet.EnsureCell(currentRow, 2, styleText).SetCellValue(productInfo.ProductName);
                        sheet.EnsureCell(currentRow, 3, styleText).SetCellValue(productInfo.ProductUnitConversionName);
                        sheet.EnsureCell(currentRow, 4, styleText).SetCellValue(productInfo.ProductTypeName);
                        sheet.EnsureCell(currentRow, 5, styleText).SetCellValue(productInfo.Specification);
                        sheet.EnsureCell(currentRow, 6, styleText).SetCellValue(productInfo.ProductCateName);
                    }
                    sheet.EnsureCell(currentRow, 7, styleNumber).SetCellValue(Convert.ToDouble(m.Quantity + m.TotalQuantityInheritance));

                    if (m.StepId.HasValue && stepInfos.ContainsKey((int)m.StepId))
                        sheet.EnsureCell(currentRow, 8, styleText).SetCellValue(stepInfos[(int)m.StepId]);

                    if (m.DepartmentId.HasValue && departmentInfos.ContainsKey((int)m.DepartmentId))
                    {
                        var department = departmentInfos[(int)m.DepartmentId];
                        sheet.EnsureCell(currentRow, 9, styleText).SetCellValue(department.DepartmentCode);
                        sheet.EnsureCell(currentRow, 10, styleText).SetCellValue(department.DepartmentName);
                    }

                    currentRow++;
                }
                
            }

            return true;
        }
    }
}