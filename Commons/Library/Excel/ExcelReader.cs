using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using VErp.Commons.Constants;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;

namespace VErp.Commons.Library
{
    public class ExcelReader
    {
        private IWorkbook _xssfwb;
        private DataFormatter _dataFormatter = new DataFormatter(CultureInfo.CurrentCulture);

        public ExcelReader(string filePath) : this(new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {

        }

        public ExcelReader(Stream file)
        {
            //hssfwb = WorkbookFactory.Create(file);// new XSSFWorkbook(file);
            _xssfwb = new XSSFWorkbook(file);
            file.Close();
        }

        private string GetCellValue(ICell cell)
        {
            var dataFormatter = new DataFormatter(CultureInfo.CurrentCulture);

            // If this is not part of a merge cell,
            // just get this cell's value like normal.
            if (!cell.IsMergedCell)
            {
                return dataFormatter.FormatCellValue(cell);
            }

            // Otherwise, we need to find the value of this merged cell.
            else
            {
                // Get current sheet.
                var currentSheet = cell.Sheet;

                // Loop through all merge regions in this sheet.
                for (int i = 0; i < currentSheet.NumMergedRegions; i++)
                {
                    var mergeRegion = currentSheet.GetMergedRegion(i);

                    // If this merged region contains this cell.
                    if (mergeRegion.FirstRow <= cell.RowIndex && cell.RowIndex <= mergeRegion.LastRow &&
                        mergeRegion.FirstColumn <= cell.ColumnIndex && cell.ColumnIndex <= mergeRegion.LastColumn)
                    {
                        // Find the top-most and left-most cell in this region.
                        var firstRegionCell = currentSheet.GetRow(mergeRegion.FirstRow)
                                                .GetCell(mergeRegion.FirstColumn);

                        // And return its value.
                        return dataFormatter.FormatCellValue(firstRegionCell);
                    }
                }
                // This should never happen.
                throw new Exception("Cannot find this cell in any merged region");
            }
        }

        public string[][] ReadFile(int collumnLength, int sheetAt = 0, int startRow = 0, int startCollumn = 0)
        {
            List<string[]> data = new List<string[]>();
            ISheet sheet = _xssfwb.GetSheetAt(sheetAt);
            int rowIdx = startRow;
            IRow row;
            while ((row = sheet.GetRow(rowIdx)) != null)
            {
                if (row.GetCell(0) == null || string.IsNullOrEmpty(GetCellValue(row.GetCell(0))))
                {
                    break;
                }
                List<string> info = new List<string>();
                for (int collumnIdx = 0; collumnIdx < collumnLength; collumnIdx++)
                {
                    ICell cell = row.GetCell(collumnIdx + startCollumn);
                    if (cell != null)
                    {
                        info.Add(GetCellValue(cell));
                    }
                    else
                    {
                        info.Add(string.Empty);
                    }
                }
                data.Add(info.ToArray());
                rowIdx++;
            }
            return data.ToArray();
        }



        public IList<ExcelSheetDataModel> ReadSheets(string sheetName, int fromRow = 1, int? toRow = null, int? maxrows = null, int? titleRow = null)
        {
            var sheetDatas = new List<ExcelSheetDataModel>();

            //hssfwb.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();
            try
            {
                // BaseFormulaEvaluator.EvaluateAllFormulaCells(_xssfwb);

                var eval = _xssfwb.GetCreationHelper().CreateFormulaEvaluator();
                eval.IgnoreMissingWorkbooks = true;
                eval.EvaluateAll();
            }
            catch (Exception)
            {

            }


            //if (hssfwb is XSSFWorkbook)
            //{
            //    NPOI.SS.Formula.BaseFormulaEvaluator.EvaluateAllFormulaCells(hssfwb);
            //}
            //else
            //{
            //    HSSFFormulaEvaluator.EvaluateAllFormulaCells(hssfwb);
            //}

            var fromRowIndex = fromRow - 1;
            var toRowIndex = toRow.HasValue && toRow > 0 ? toRow - 1 : null;

            var titleRowIndex = titleRow.HasValue && titleRow > 0 ? fromRowIndex > 0 ? titleRow.Value - 1 : fromRowIndex - 1 : 0;

           

            for (int i = 0; i < _xssfwb.NumberOfSheets; i++)
            {

                var sheet = _xssfwb.GetSheetAt(i);

                var sName = (sheet.SheetName ?? "").Trim();
                sheetName = (sheetName ?? "").Trim();

                if (!string.IsNullOrWhiteSpace(sheetName) && !sName.Equals(sheetName))
                    continue;

                var maxrowsCount = maxrows;
                if (!maxrowsCount.HasValue)
                {
                    maxrowsCount = sheet.LastRowNum + 1;
                }
                else
                {
                    if (maxrowsCount > sheet.LastRowNum)
                    {
                        maxrowsCount = sheet.LastRowNum + 1;
                    }
                }

                var sheetData = new List<NonCamelCaseDictionary<string>>();

                var columns = new HashSet<string>();

                var mergeRegions = new CellRangeAddress[sheet.NumMergedRegions];

                var regionValues = new ICell[sheet.NumMergedRegions];

                for (var re = 0; re < sheet.NumMergedRegions; re++)
                {
                    var region = sheet.GetMergedRegion(re);

                    mergeRegions[re] = region;

                    var isFirstRowValue = false;

                    ICell cell = null;

                    var r = sheet.GetRow(region.FirstRow);

                    var c = r.Cells.FirstOrDefault(c => c.ColumnIndex == region.FirstColumn);

                    var v = GetCellString(c);
                    if (!string.IsNullOrWhiteSpace(v))
                    {
                        cell = c;
                        isFirstRowValue = true;
                    }


                    if (!isFirstRowValue)
                    {
                        r = sheet.GetRow(region.LastRow);

                        c = r.Cells.FirstOrDefault(c => c.ColumnIndex == region.LastColumn);

                        v = GetCellString(c);
                        if (!string.IsNullOrWhiteSpace(v))
                        {
                            cell = c;
                        }
                    }

                    regionValues[re] = cell;
                }

                var continuousRowEmpty = 0;

                if (OnBeginReadingExcelRow != null)
                {
                    OnBeginReadingExcelRow(maxrowsCount ?? 0);
                }
                for (int row = fromRowIndex; row < fromRowIndex + maxrowsCount && (!toRowIndex.HasValue || row <= toRowIndex); row++)
                {

                    var rowData = new NonCamelCaseDictionary<string>();
                    if (sheet.GetRow(row) == null) //null is when the row only contains empty cells 
                    {
                        continuousRowEmpty++;
                        //continue;
                        if (continuousRowEmpty > 1000)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (continuousRowEmpty > 1000)
                        {
                            break;
                        }

                        var continuousColumnEmpty = 0;
                        var isRowEmpty = true;
                        foreach (var col in sheet.GetRow(row).Cells)
                        {

                            var columnName = GetExcelColumnName(col.ColumnIndex + 1);
                            if (!columns.Contains(columnName))
                            {
                                columns.Add(columnName);
                            }

                            var cell = col;

                            if (cell.IsMergedCell)
                            {
                                for (var regionIdx = 0; regionIdx < mergeRegions.Length; regionIdx++)
                                {
                                    var region = mergeRegions[regionIdx];
                                    if (region.IsInRange(row, col.ColumnIndex))
                                    {
                                        var c = regionValues[regionIdx];
                                        var v = GetCellString(c);
                                        if (!string.IsNullOrWhiteSpace(v))
                                        {
                                            cell = c;
                                        }
                                    }
                                }
                            }


                            try
                            {
                                rowData.Add(columnName, GetCellString(cell)?.Trim()?.Trim('\''));
                            }
                            catch (Exception)
                            {
                                rowData.Add(columnName, cell.StringCellValue.ToString()?.Trim()?.Trim('\''));

                            }
                            if (string.IsNullOrWhiteSpace(rowData[columnName]))
                            {
                                continuousColumnEmpty++;
                                if (continuousColumnEmpty > 100)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                isRowEmpty = false;
                                continuousColumnEmpty = 0;
                            }

                        }

                        if (isRowEmpty)
                        {
                            continuousRowEmpty++;
                        }
                        else
                        {
                            continuousRowEmpty = 0;
                        }
                    }

                    sheetData.Add(rowData);

                    if (OnReadingExcelRow != null)
                    {
                        OnReadingExcelRow(row + 1);
                    }
                }

                //set default value for null column
                foreach (var column in columns)
                {
                    foreach (var row in sheetData)
                    {
                        if (!row.ContainsKey(column))
                        {
                            row.Add(column, null);
                        }
                    }
                }

                sheetDatas.Add(new ExcelSheetDataModel() { SheetName = sheet.SheetName, Rows = sheetData.ToArray() });
            }

            return sheetDatas;
        }

        public List<List<ImportExcelRowData>> ReadSheetData<T>(ImportExcelMapping mapping)
        {
            var fields = typeof(T).GetProperties();

            var data = ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var rowDatas = new List<List<ImportExcelRowData>>();

            if (OnBeginParseExcelDataToEntity != null)
            {
                OnBeginParseExcelDataToEntity(data.Rows.Length);
            }
            for (var rowIndx = 0; rowIndx < data.Rows.Length; rowIndx++)
            {
                var row = data.Rows[rowIndx];

                var rowData = new List<ImportExcelRowData>();
                bool isIgnoreRow = false;
                for (int fieldIndx = 0; fieldIndx < mapping.MappingFields.Count && !isIgnoreRow; fieldIndx++)
                {
                    var mappingField = mapping.MappingFields[fieldIndx];

                    string value = null;
                    if (row.ContainsKey(mappingField.Column))
                        value = row[mappingField.Column]?.ToString();

                    if (string.IsNullOrWhiteSpace(value) && mappingField.IsIgnoredIfEmpty)
                    {
                        isIgnoreRow = true;
                        continue;
                    }

                    var field = fields.FirstOrDefault(f => f.Name == mappingField.FieldName);

                    if (field == null) throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy field {mappingField.FieldName}");

                    rowData.Add(new ImportExcelRowData()
                    {
                        FieldMapping = mappingField,
                        PropertyInfo = field,
                        CellValue = value
                    });
                }

                if (!isIgnoreRow)
                    rowDatas.Add(rowData);

                if (OnParseExcelDataToEntity != null)
                {
                    OnParseExcelDataToEntity(rowIndx + 1);
                }
            }

            return rowDatas;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns>true - property is proccessed and not process automatic, false - set automatic</returns>
        public delegate bool AssignPropertyEvent<T>(T entity, string propertyName, string value);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="refObj"></param>
        /// <param name="refPropertyName"></param>
        /// <returns>true - property is proccessed and not process automatic, false - set automatic</returns>
        public delegate bool AssignPropertyAndRefEvent<T>(T entity, string propertyName, string value, object refObj, string refPropertyName);



        public delegate void ReadingExcelRowEvent(int readRows);
        public ReadingExcelRowEvent OnReadingExcelRow;

        public delegate void BeginReadingExcelRowEvent(int totalRows);
        public BeginReadingExcelRowEvent OnBeginReadingExcelRow;


        public delegate void BeginParseExcelDataToEntityEvent(int totalRows);
        public BeginParseExcelDataToEntityEvent OnBeginParseExcelDataToEntity;


        public delegate void ParseExcelDataToEntityEvent(int proccessedRows);
        public ParseExcelDataToEntityEvent OnParseExcelDataToEntity;

        public IList<T> ReadSheetEntity<T>(ImportExcelMapping mapping)
        {
            return ReadSheetEntity(mapping, (AssignPropertyAndRefEvent<T>)null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mapping"></param>
        /// <param name="OnAssignProperty">Set property manual, return true if property has processed manually, false if automatic set</param>
        /// <returns></returns>
        public IList<T> ReadSheetEntity<T>(ImportExcelMapping mapping, AssignPropertyEvent<T> OnAssignProperty)
        {
            if (OnAssignProperty == null)
            {
                return ReadSheetEntity(mapping, (AssignPropertyAndRefEvent<T>)null);
            }
            else
            {
                return ReadSheetEntity<T>(mapping, (entity, propertyName, value, refObj, refPropertyName) =>
                    {
                        return OnAssignProperty(entity, propertyName, value);
                    });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mapping"></param>
        /// <param name="OnAssignProperty">Set property manual, return true if property has processed manually, false if automatic set</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="BadRequestException"></exception>
        public IList<T> ReadSheetEntity<T>(ImportExcelMapping mapping, AssignPropertyAndRefEvent<T> OnAssignProperty)
        {
            var fields = typeof(T).GetProperties();

            var refTypeFields = new Dictionary<Type, PropertyInfo[]>();

            var displayNames = new Dictionary<PropertyInfo, string>();

            foreach (var prop in fields)
            {
                displayNames.Add(prop, prop.GetCustomAttributes<DisplayAttribute>().FirstOrDefault()?.Name ?? prop.Name);
                if (string.IsNullOrWhiteSpace(displayNames[prop]))
                {
                    displayNames[prop] = prop.Name;
                }


                if (prop.PropertyType.IsClass() && !refTypeFields.ContainsKey(prop.PropertyType))
                {
                    var childFields = prop.PropertyType.GetProperties();
                    refTypeFields.Add(prop.PropertyType, childFields);

                    foreach (var c in childFields)
                    {
                        displayNames.Add(c, c.GetCustomAttributes<DisplayAttribute>().FirstOrDefault()?.Name ?? c.Name);
                        if (string.IsNullOrWhiteSpace(displayNames[c]))
                        {
                            displayNames[c] = c.Name;
                        }
                    }
                }

            }


            var data = ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var lstData = new List<T>();


            if (OnBeginParseExcelDataToEntity != null)
            {
                OnBeginParseExcelDataToEntity(data.Rows.Length);
            }
            for (var rowIndx = 0; rowIndx < data.Rows.Length; rowIndx++)
            {

                var row = data.Rows[rowIndx];

                //bool isIgnoreRow = false;


                var checkFieldsMapping = mapping.MappingFields.Where(f => f.IsIgnoredIfEmpty || f.FieldName == ImportStaticFieldConsants.CheckImportRowEmpty);

                if (checkFieldsMapping.Any(f => string.IsNullOrWhiteSpace(row[f.Column])))
                {
                    continue;
                }


                var entityInfo = Activator.CreateInstance<T>();
                //for (int fieldIndx = 0; fieldIndx < mapping.MappingFields.Count; fieldIndx++)//&& !isIgnoreRow
                //{

                //    var mappingField = mapping.MappingFields[fieldIndx];
                foreach (var mappingField in mapping.MappingFields.Where(f => f.FieldName != ImportStaticFieldConsants.CheckImportRowEmpty).ToList())//&& !isIgnoreRow
                {

                    var fieldDisplay = "";
                    try
                    {
                        string value = null;
                        if (row.ContainsKey(mappingField.Column))
                            value = row[mappingField.Column]?.ToString();

                        //if (string.IsNullOrWhiteSpace(value) && mappingField.IsRequire)
                        //{
                        //    isIgnoreRow = true;
                        //    break;
                        //}

                        var field = fields.FirstOrDefault(f => f.Name == mappingField.FieldName);

                        if (field == null)
                        {
                            field = fields.FirstOrDefault(f => mappingField.FieldName.StartsWith(f.Name));

                        }

                        if (field == null) throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy field {mappingField.FieldName}");

                        fieldDisplay = displayNames[field];

                        if (string.IsNullOrWhiteSpace(mappingField.FieldName)) continue;


                        object refObj = null;

                        PropertyInfo refField = null;

                        var propertyIsClass = field.PropertyType.IsClass();
                        if (propertyIsClass)
                        {
                            var v = field.GetValue(entityInfo, null);
                            if (v == null)
                            {
                                v = Activator.CreateInstance(field.PropertyType);
                                field.SetValue(entityInfo, v);
                            }

                            refObj = v;

                            if (string.IsNullOrWhiteSpace(mappingField.RefFieldName))
                            {
                                mappingField.RefFieldName = mappingField.FieldName.Substring(field.Name.Length);
                                if (!string.IsNullOrWhiteSpace(mappingField.RefFieldName))
                                {
                                    mappingField.FieldName = field.Name;
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(mappingField.RefFieldName))
                            {
                                refField = refTypeFields[field.PropertyType].FirstOrDefault(f => f.Name == mappingField.RefFieldName);
                                if (refField == null)
                                {
                                    refField = refTypeFields[field.PropertyType].FirstOrDefault(f => mappingField.RefFieldName.StartsWith(f.Name));
                                }

                                if (refField == null) throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy field {mappingField.RefFieldName} thuộc {mappingField.FieldName}");
                            }


                            fieldDisplay += " > " + displayNames[refField];
                        }

                        var isAutoSet = true;
                        if (OnAssignProperty != null)
                        {
                            isAutoSet = !OnAssignProperty(entityInfo, mappingField.FieldName, value, refObj, mappingField.RefFieldName);
                        }

                        if (isAutoSet && !string.IsNullOrWhiteSpace(value))
                        {
                            if (propertyIsClass)
                            {
                                refField.SetValue(refObj, value.ConvertValueByType(refField.PropertyType));
                            }
                            else
                            {
                                field.SetValue(entityInfo, value.ConvertValueByType(field.PropertyType));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Lỗi dòng {mapping.FromRow + rowIndx} cột {mappingField.Column} \"{fieldDisplay}\" {ex.Message}", ex);
                    }

                }

                //if (!isIgnoreRow)
                //{
                var context = new ValidationContext(entityInfo);
                ICollection<ValidationResult> results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(entityInfo, context, results, true);
                if (!isValid)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Lỗi dữ liệu dòng {mapping.FromRow + rowIndx}, " + string.Join(", ", results.FirstOrDefault()?.MemberNames) + ": " + results.FirstOrDefault()?.ErrorMessage);
                }

                if (entityInfo is MappingDataRowAbstract entity)
                {
                    entity.RowNumber = mapping.FromRow + rowIndx;
                }
                lstData.Add(entityInfo);
                //}

                if (OnParseExcelDataToEntity != null)
                {
                    OnParseExcelDataToEntity(rowIndx + 1);
                }
            }

            return lstData;
        }
       

        private string GetCellString(ICell cell)
        {
            if (cell == null) return null;

            var type = cell.CellType;

            // string formulaMessage = "";
            string cellFormular = "";
            if (cell.CellType == CellType.Formula)
            {
                try
                {
                    cellFormular = cell.CellFormula;

                }
                catch (Exception)
                {

                }

                try
                {
                    //hssfwb.GetCreationHelper().CreateFormulaEvaluator().EvaluateFormulaCell(cell);
                    type = cell.CachedFormulaResultType;
                }
                catch (Exception)
                {
                    //formulaMessage = cellFormular + " => " + ex.Message;
                }



            }

            switch (type)
            {
                case CellType.String:
                    return cell.StringCellValue?.Trim();

                case CellType.Formula:
                    return PREFIX_ERROR_CELL + ((XSSFCell)cell).ErrorCellString + " => " + cell.Address.ToString() + " (" + cellFormular + ")";

                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        try
                        {
                            return DateTime.FromOADate(cell.NumericCellValue).ToString();
                        }
                        catch (Exception)
                        {

                            return Convert.ToDecimal(cell.NumericCellValue).ToString();
                        }

                        //try
                        //{
                        //    return cell.DateCellValue.ToString();
                        //}
                        //catch
                        //{
                        //    return DateTime.FromOADate(cell.NumericCellValue).ToString();
                        //}
                    }
                    else
                    {
                        return Convert.ToDecimal(cell.NumericCellValue).ToString();
                    }

                case CellType.Error:

                    return PREFIX_ERROR_CELL + ((XSSFCell)cell).ErrorCellString + " => " + cell.Address.ToString() + " (" + cellFormular + ")";
            }

            return _dataFormatter.FormatCellValue(cell);
            // return cell.StringCellValue?.Trim();

        }

        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public const string PREFIX_ERROR_CELL = "#ERROR#";


    }
}
