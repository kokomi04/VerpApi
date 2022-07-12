using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Services.Accountancy.Model.Input;
using static VErp.Services.Accountancy.Service.Input.Implement.InputDataService;

namespace VErp.Services.Accountancy.Service.Input.Implement.Facade
{
    public interface IInpuDataExportFacadeService
    {
        Task<(Stream stream, string fileName, string contentType)> Export(int inputTypeId, InputTypeBillsExporttFilterModel req);
    }

    public class InpuDataExportFacadeService : IInpuDataExportFacadeService
    {
        private AccountancyDBContext accountancyDBContext;
        private readonly IInputDataService inputDataService;
        private readonly ICurrentContextService currentContextService;
        private ISheet sheet = null;
        private int currentRow = 0;
        //private int maxColumnIndex = 10;

        private IList<ValidateField> fields;
        private IList<string> groups;
        private InputType typeInfo;
        private bool isMultirow;

        public InpuDataExportFacadeService(AccountancyDBContext accountancyDBContext, IInputDataService inputDataService, ICurrentContextService currentContextService)
        {
            this.accountancyDBContext = accountancyDBContext;
            this.inputDataService = inputDataService;
            this.currentContextService = currentContextService;
        }

        private async Task LoadFields(int inputTypeId, InputTypeBillsExporttFilterModel req)
        {
            //fields = (await inputDataService.GetFieldDataForMapping(inputTypeId, null))
            //    .Fields
            //    .Where(f => f.FieldName != ImportStaticFieldConsants.CheckImportRowEmpty
            //    && ((fieldNames?.Count ?? 0) == 0 || fieldNames.Contains(f.FieldName))
            //    )
            //    .ToList();
            fields = (await inputDataService.GetInputFields(inputTypeId, null, true))
                .Where(f => ((req.FieldNames?.Count ?? 0) == 0 || req.FieldNames.Contains(f.FieldName))
                ).ToList();

            isMultirow = fields.Any(f => f.IsMultiRow);

            groups = fields.Select(g => g.AreaTitle).Distinct().ToList();
        }

        public async Task<(Stream stream, string fileName, string contentType)> Export(int inputTypeId, InputTypeBillsExporttFilterModel req)
        {
            typeInfo = await accountancyDBContext.InputType.AsNoTracking().FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);

            await LoadFields(inputTypeId, req);

            var lst = await inputDataService.GetBills(inputTypeId, isMultirow, req.FromDate, req.ToDate, req.Keyword, req.Filters, req.ColumnsFilters, req.OrderBy, req.Asc, 1, -1);

            var xssfwb = new XSSFWorkbook();
            sheet = xssfwb.CreateSheet();


            WriteTable(lst.List);

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
            //var fileName = $"{typeInfo.Title.NormalizeAsInternalName()}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            var fromDate = req.FromDate.HasValue ? req.FromDate.Value.UnixToDateTime(currentContextService.TimeZoneOffset).ToString("dd_MM_yyyy") : "";
            var toDate = req.ToDate.HasValue ? req.ToDate.Value.UnixToDateTime(currentContextService.TimeZoneOffset).ToString("dd_MM_yyyy") : "";
            var fileName = typeInfo.Title.ToString();
            if (!"".Equals(fromDate)) fileName += $" {fromDate}";
            if (!"".Equals(toDate)) fileName += $" {toDate}";
            fileName = StringUtils.RemoveDiacritics($"{fileName}.xlsx").Replace(" ", "#");
            return (stream, fileName, contentType);
        }

        private void WriteTable(IList<NonCamelCaseDictionary> data)
        {
            currentRow = 1;

            var fRow = currentRow;
            var sRow = currentRow;

            sheet.EnsureCell(fRow, 0).SetCellValue($"STT");
            sheet.SetHeaderCellStyle(fRow, 0);

            var sColIndex = 1;
            if (groups.Count > 0)
            {
                sRow = fRow + 1;
            }

            columnMaxLineLength = new List<int>(fields.Count + 1);
            columnMaxLineLength.Add(5);
            foreach (var g in groups)
            {
                var groupCols = fields.Where(f => f.AreaTitle == g);
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
                    sheet.EnsureCell(sRow, sColIndex).SetCellValue(f.Title);
                    columnMaxLineLength.Add(f.Title?.Length ?? 10);

                    sheet.SetHeaderCellStyle(sRow, sColIndex);
                    sColIndex++;
                }
            }

            currentRow = sRow + 1;

            WriteTableDetailData(data);
        }


        private IList<int> columnMaxLineLength = new List<int>();
        private (EnumDataType type, object value) GetFieldValue(NonCamelCaseDictionary row, ValidateField field, out bool isFormula)
        {
            isFormula = false;

            if (string.IsNullOrWhiteSpace(field.RefTableCode) || field.FormTypeId == (int)EnumFormType.Input)
            {
                return ((EnumDataType)field.DataTypeId, row[field.FieldName]);
            }
            else
            {
                var refField = $"{field.FieldName}_{field.RefTableTitle?.Split(',')[0]}";
                return (EnumDataType.Text, row[refField]);
            }
        }
        private void WriteTableDetailData(IList<NonCamelCaseDictionary> data)
        {
            var stt = 1;

            var textStyle = sheet.GetCellStyle(isBorder: true);
            var intStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "#,###");
            var decimalStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "#,##0.00###");

            var dateStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "dd/MM/yyyy");

            foreach (var p in data)
            {
                var sColIndex = 1;
                sheet.EnsureCell(currentRow, 0, intStyle).SetCellValue(stt);

                foreach (var g in groups)
                {
                    var groupCols = fields.Where(f => f.AreaTitle == g);

                    foreach (var f in groupCols)
                    {
                        var v = GetFieldValue(p, f, out var isFormula);
                        switch (v.type)
                        {
                            case EnumDataType.BigInt:
                            case EnumDataType.Int:
                                if (!v.value.IsNullObject())
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
                                if (!v.value.IsNullObject())
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
                            case EnumDataType.DateRange:
                            case EnumDataType.Date:
                                if (!v.value.IsNullObject())
                                {
                                    sheet.EnsureCell(currentRow, sColIndex, dateStyle).SetCellValue(((long)v.value).UnixToDateTime(currentContextService.TimeZoneOffset));
                                }

                                else
                                {
                                    sheet.EnsureCell(currentRow, sColIndex, dateStyle);
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



        }


    }
}
