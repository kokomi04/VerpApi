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
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Product;
using static VErp.Commons.GlobalObject.InternalDataInterface.ProductModel;

namespace VErp.Services.Stock.Service.Products.Implement.ProductFacade
{
    public class ProductExportFacade
    {
        private StockDBContext _stockDbContext;
        private ISheet sheet = null;
        private int currentRow = 0;
        private int maxColumnIndex = 10;

        private static readonly IList<CategoryFieldNameModel> fields = Utils.GetFieldNameModels<ProductImportModel>();
        private static readonly IList<string> groups = fields.Select(g => g.GroupName).Distinct().ToList();


        IDictionary<int, RefCustomerBasic> customers;
        IDictionary<int, SimpleStockInfo> stocks;

        public ProductExportFacade(StockDBContext stockDbContext)
        {
            _stockDbContext = stockDbContext;
            customers = (_stockDbContext.RefCustomerBasic.AsNoTracking().ToList()).ToDictionary(c => c.CustomerId, c => c);
            stocks = (_stockDbContext.Stock.Select(s => new SimpleStockInfo { StockId = s.StockId, StockName = s.StockName }).ToList()).ToDictionary(c => c.StockId, c => c);
        }


        public async Task<(Stream stream, string fileName, string contentType)> Export(IList<ProductListOutput> products)
        {

            var xssfwb = new XSSFWorkbook();
            sheet = xssfwb.CreateSheet();


            var productCate = await WriteTable(products);

            var currentRowTmp = currentRow;

            if (sheet.LastRowNum < 1000)
            {
                for (var i = 0; i < fields.Count + 1; i++)
                {
                    sheet.AutoSizeColumn(i, false);
                }
            }
            else
            {
                for (var i = 0; i < fields.Count + 1; i++)
                {
                    sheet.ManualResize(i, columnMaxLineLength[i]);
                }
            }

            currentRow = currentRowTmp;


            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"product-list-{productCate.NormalizeAsInternalName()}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            return (stream, fileName, contentType);
        }

        private async Task<string> WriteTable(IList<ProductListOutput> products)
        {
            currentRow = 1;

            var fRow = currentRow;
            var sRow = currentRow;

            sheet.EnsureCell(fRow, 0).SetCellValue($"STT");
            sheet.SetHeaderCellStyle(fRow, 0);

            var sColIndex = 1;
            if (groups.Count > 1)
            {
                sRow = fRow + 1;
            }

            columnMaxLineLength = new List<int>(fields.Count + 1);
            columnMaxLineLength.Add(5);
            foreach (var g in groups)
            {
                var groupCols = fields.Where(f => f.GroupName == g);
                sheet.EnsureCell(fRow, sColIndex).SetCellValue(g);
                sheet.SetHeaderCellStyle(fRow, sColIndex);

                if (groupCols.Count() > 1)
                {
                    var region = new CellRangeAddress(fRow, fRow, sColIndex, sColIndex + groupCols.Count() - 1);
                    sheet.AddMergedRegion(region);
                    RegionUtil.SetBorderBottom(1, region, sheet);
                    RegionUtil.SetBorderLeft(1, region, sheet);
                    RegionUtil.SetBorderRight(1, region, sheet);
                    RegionUtil.SetBorderTop(1, region, sheet);
                }

                foreach (var f in groupCols)
                {
                    sheet.EnsureCell(sRow, sColIndex).SetCellValue(f.FieldTitle);
                    columnMaxLineLength.Add(f.FieldTitle?.Length ?? 10);

                    sheet.SetHeaderCellStyle(sRow, sColIndex);
                    sColIndex++;
                }
            }

            currentRow = sRow + 1;

            return await WriteTableDetailData(products);
        }


        private IList<int> columnMaxLineLength = new List<int>();
        private (EnumDataType type, object value) GetProductValue(
            ProductListOutput product,
            IList<int> stockIds,
            IList<ProductCustomer> productCustomers,
            string fieldName,
            out bool isFormula
            )
        {
            isFormula = false;            
            ProductCustomer pCustomer = productCustomers?.OrderBy(c=>c.CustomerId)?.ThenByDescending(c=>c.CreatedDatetimeUtc).FirstOrDefault();
            int? customerId = pCustomer?.CustomerId;
                   
            var pus = product.ProductUnitConversions.Where(s => !s.IsDefault).ToList();
            switch (fieldName)
            {
                case nameof(ProductImportModel.ProductCode):
                    return (EnumDataType.Text, product.ProductCode);
                case nameof(ProductImportModel.ProductName):
                    return (EnumDataType.Text, product.ProductName);
                case nameof(ProductImportModel.ProductTypeCode):
                    return (EnumDataType.Text, product.ProductTypeCode);
                case nameof(ProductImportModel.ProductTypeName):
                    return (EnumDataType.Text, product.ProductTypeName);
                case nameof(ProductImportModel.ProductCate):
                    return (EnumDataType.Text, product.ProductCateName);
                case nameof(ProductImportModel.BarcodeConfigId):
                    return (EnumDataType.Text, product.BarcodeConfigName);
                case nameof(ProductImportModel.Barcode):
                    return (EnumDataType.Text, product.Barcode);
                case nameof(ProductImportModel.Unit):
                    return (EnumDataType.Text, product.UnitName);
                case nameof(ProductImportModel.DecimalPlaceDefault):
                    return (EnumDataType.Int, product.DecimalPlace);
                case nameof(ProductImportModel.EstimatePrice):
                    return (EnumDataType.Decimal, product.EstimatePrice);
                case nameof(ProductImportModel.IsProductSemi):
                    return (EnumDataType.Text, product.IsProductSemi ? "Có" : "Không");
                case nameof(ProductImportModel.IsProduct):
                    return (EnumDataType.Text, product.IsProduct ? "Có" : "Không");
                case nameof(ProductImportModel.IsMaterials):
                    return (EnumDataType.Text, product.IsMaterials ? "Có" : "Không");
                case nameof(ProductImportModel.Coefficient):
                    return (EnumDataType.Int, product.Coefficient);
                case nameof(ProductImportModel.CustomerCode):
                    if (customerId.HasValue && customers.ContainsKey(customerId.Value))
                    {
                        return (EnumDataType.Text, customers[customerId.Value].CustomerCode);
                    }
                    return (EnumDataType.Text, null);
                case nameof(ProductImportModel.CustomerName):
                    if (customerId.HasValue && customers.ContainsKey(customerId.Value))
                    {
                        return (EnumDataType.Text, customers[customerId.Value].CustomerName);
                    }
                    return (EnumDataType.Text, null);
                case nameof(ProductImportModel.CustomerProductCode):
                    return (EnumDataType.Text, pCustomer?.CustomerProductCode);

                case nameof(ProductImportModel.Specification):
                    return (EnumDataType.Text, product.Specification);
                case nameof(ProductImportModel.Description):
                    return (EnumDataType.Text, product.Description);

                case nameof(ProductImportModel.PackingMethod):
                    return (EnumDataType.Text, product.PackingMethod);
                case nameof(ProductImportModel.Quantitative):
                    return (EnumDataType.Decimal, product.Quantitative);
                case nameof(ProductImportModel.QuantitativeUnitTypeId):
                    return (EnumDataType.Text, product.QuantitativeUnitTypeId?.GetEnumDescription());

                case nameof(ProductImportModel.Long):
                    return (EnumDataType.Decimal, product.Long);
                case nameof(ProductImportModel.Width):
                    return (EnumDataType.Decimal, product.Width);
                case nameof(ProductImportModel.Height):
                    return (EnumDataType.Decimal, product.Height);
                case nameof(ProductImportModel.Measurement):
                    return (EnumDataType.Decimal, product.Measurement);
                case nameof(ProductImportModel.NetWeight):
                    return (EnumDataType.Decimal, product.NetWeight);
                case nameof(ProductImportModel.GrossWeight):
                    return (EnumDataType.Decimal, product.GrossWeight);
                case nameof(ProductImportModel.LoadAbility):
                    return (EnumDataType.Decimal, product.LoadAbility);
                case nameof(ProductImportModel.StockOutputRuleId):
                    return (EnumDataType.Text, product.StockOutputRuleId?.GetEnumDescription());
                case nameof(ProductImportModel.AmountWarningMin):
                    return (EnumDataType.Decimal, product.AmountWarningMin);
                case nameof(ProductImportModel.AmountWarningMax):
                    return (EnumDataType.Decimal, product.AmountWarningMax);
                case nameof(ProductImportModel.ExpireTimeAmount):
                    return (EnumDataType.Decimal, product.ExpireTimeAmount);
                case nameof(ProductImportModel.ExpireTimeTypeId):
                    return (EnumDataType.Text, product.ExpireTimeTypeId?.GetEnumDescription());
                case nameof(ProductImportModel.StockIds):
                    return (EnumDataType.Text, string.Join(", ", stocks.Where(s => stockIds?.Contains(s.Key) == true).Select(s => s.Value.StockName)));


            }
            var secondaryUnit = nameof(ProductImportModel.SecondaryUnit02)[..^2];
            var factorExpression = nameof(ProductImportModel.FactorExpression02)[..^2];
            var decimalPlace = nameof(ProductImportModel.DecimalPlace02)[..^2];
            if (fieldName.StartsWith(secondaryUnit))
            {
                var suffix = int.Parse(fieldName.Substring(secondaryUnit.Length));
                var index = suffix - 2;
                if (pus.Count > index)
                {
                    return (EnumDataType.Text, pus[index].ProductUnitConversionName);
                }
                return (EnumDataType.Text, null);
            }

            if (fieldName.StartsWith(factorExpression))
            {
                isFormula = true;
                var suffix = int.Parse(fieldName.Substring(factorExpression.Length));
                var index = suffix - 2;
                if (pus.Count > index)
                {
                    return (EnumDataType.Decimal, pus[index].FactorExpression);
                }
                return (EnumDataType.Decimal, null);
            }

            if (fieldName.StartsWith(decimalPlace))
            {
                var suffix = int.Parse(fieldName.Substring(decimalPlace.Length));
                var index = suffix - 2;
                if (pus.Count > index)
                {
                    return (EnumDataType.Decimal, pus[index].DecimalPlace);
                }
                return (EnumDataType.Decimal, null);
            }

            return (EnumDataType.Text, "");

        }
        private async Task<string> WriteTableDetailData(IList<ProductListOutput> products)
        {
            var stt = 1;
            var productIdPages = new List<IList<int>>();
            var idx = 0;
            var productIdPage = new List<int>();
            foreach (var p in products.Select(p => p.ProductId).ToList())
            {
                productIdPage.Add(p);
                idx++;

                if (idx % 1000 == 0)
                {
                    productIdPages.Add(productIdPage);
                    productIdPage = new List<int>();
                }

            }

            if (productIdPage.Count > 0)
                productIdPages.Add(productIdPage);


            Dictionary<int, IList<int>> stockValidations = new Dictionary<int, IList<int>>();
            Dictionary<int, IList<ProductCustomer>> productCustomers = new Dictionary<int, IList<ProductCustomer>>();
            foreach (var productIds in productIdPages)
            {
                var sValidations = (await _stockDbContext.ProductStockValidation.Where(s => productIds.Contains(s.ProductId)).ToListAsync())
                    .GroupBy(s => s.ProductId)
                    .ToDictionary(s => s.Key, s => s.Select(v => v.StockId).ToList());
                foreach (var s in sValidations)
                {
                    stockValidations.Add(s.Key, s.Value);
                }

                var pCustomers = (await _stockDbContext.ProductCustomer.Where(s => productIds.Contains(s.ProductId)).ToListAsync())
                    .GroupBy(s => s.ProductId)
                    .ToDictionary(s => s.Key, s => s.ToList());

                foreach (var s in pCustomers)
                {
                    productCustomers.Add(s.Key, s.Value);
                }
            }

            var pName = new HashSet<string>();

            var textStyle = sheet.GetCellStyle(isBorder: true);
            var intStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "#,###");
            var decimalStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "#,##0.00###");

            foreach (var p in products)
            {
                var sColIndex = 1;
                sheet.EnsureCell(currentRow, 0, intStyle).SetCellValue(stt);
                if (!pName.Contains(p.ProductCateName))
                {
                    pName.Add(p.ProductCateName);
                }
                foreach (var g in groups)
                {
                    var groupCols = fields.Where(f => f.GroupName == g);

                    foreach (var f in groupCols)
                    {
                        stockValidations.TryGetValue(p.ProductId, out var stockIds);
                        if (stockIds == null) stockIds = new List<int>();

                        productCustomers.TryGetValue(p.ProductId, out var pCustomers);
                        if (pCustomers == null) pCustomers = new List<ProductCustomer>();

                        var v = GetProductValue(p, stockIds, pCustomers, f.FieldName, out var isFormula);
                        switch (v.type)
                        {
                            case EnumDataType.BigInt:
                            case EnumDataType.Int:
                                if (!v.IsNullObject())
                                {
                                    if (isFormula)
                                    {
                                        sheet.EnsureCell(currentRow, sColIndex, intStyle)
                                            .SetCellFormula(v.value?.ToString());
                                    }
                                    else
                                    {
                                        sheet.EnsureCell(currentRow, sColIndex, intStyle)
                                            .SetCellValue(Convert.ToDouble(v.value));
                                    }
                                }
                                else
                                {
                                    sheet.EnsureCell(currentRow, sColIndex, intStyle);
                                }
                                break;                           
                            case EnumDataType.Decimal:
                                if (!v.IsNullObject())
                                {
                                    if (isFormula)
                                    {
                                        sheet.EnsureCell(currentRow, sColIndex, decimalStyle)
                                            .SetCellFormula(v.value?.ToString());
                                    }
                                    else
                                    {
                                        sheet.EnsureCell(currentRow, sColIndex, decimalStyle)
                                            .SetCellValue(Convert.ToDouble(v.value));
                                    }
                                }
                                else
                                {
                                    sheet.EnsureCell(currentRow, sColIndex, decimalStyle);
                                }
                                break;
                            default:
                                sheet.EnsureCell(currentRow, sColIndex, textStyle).SetCellValue(v.value?.ToString());
                                break;
                        }
                        if (v.value?.ToString()?.Length > columnMaxLineLength[sColIndex])
                        {
                            columnMaxLineLength[sColIndex] = v.value?.ToString()?.Length ?? 10;
                        }

                        sColIndex++;
                    }
                }
                currentRow++;
                stt++;
            }


            return pName.Count == 1 ? string.Join("", pName) : "";
        }


    }
}
