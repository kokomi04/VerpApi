using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using OpenXmlPowerTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.Attributes;
using VErp.Commons.Library.Model;
using static NPOI.HSSF.UserModel.HeaderFooter;

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


        /// <summary>
        /// Read exel data (datetime => .DateTime.ToString()  = 5/1/2009 9:00:00 AM)
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <param name="maxrows"></param>
        /// <param name="titleRow"></param>
        /// <returns></returns>
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
                    OnParseExcelDataToEntity(rowIndx + 1, rowData);
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

        public delegate Task<bool> AssignPropertyAndRefPathEvent<T>(T entity, string propertyName, string value, object refObj, string refPropertyName, string refPropertyPathSeparateByPoint);


        public delegate void ReadingExcelRowEvent(int readRows);
        public ReadingExcelRowEvent OnReadingExcelRow;

        public delegate void BeginReadingExcelRowEvent(int totalRows);
        public BeginReadingExcelRowEvent OnBeginReadingExcelRow;


        public delegate void BeginParseExcelDataToEntityEvent(int totalRows);
        public BeginParseExcelDataToEntityEvent OnBeginParseExcelDataToEntity;


        public delegate void ParseExcelDataToEntityEvent(int proccessedRows, object entity);
        public ParseExcelDataToEntityEvent OnParseExcelDataToEntity;

        public IList<T> ReadSheetEntity<T>(ImportExcelMapping mapping)
        {
            return ReadSheetEntity(mapping, (AssignPropertyAndRefEvent<T>)null);
        }

        /// <summary>
        /// Read exel data (datetime => .DateTime.ToString()  = 5/1/2009 9:00:00 AM)
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
        /// Read exel data (datetime => .DateTime.ToString()  = 5/1/2009 9:00:00 AM)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mapping"></param>
        /// <param name="OnAssignProperty"></param>
        /// <returns></returns>
        public IList<T> ReadSheetEntity<T>(ImportExcelMapping mapping, AssignPropertyAndRefEvent<T> OnAssignProperty)
        {

            if (OnAssignProperty == null)
            {
                return ReadSheetEntity<T>(mapping, (AssignPropertyAndRefPathEvent<T>)null).Result;
            }
            else
            {
                return ReadSheetEntity<T>(mapping, async (entity, propertyName, value, refObj, refPropertyName, paths) =>
                {
                    await Task.CompletedTask;
                    return OnAssignProperty(entity, propertyName, value, refObj, refPropertyName);
                }).Result;
            }
        }

        private ConcurrentDictionary<Type, PropertyInfo[]> RefTypeFields = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private ConcurrentDictionary<PropertyInfo, string> DisplayNames = new ConcurrentDictionary<PropertyInfo, string>();

        private ConcurrentDictionary<string, PropertyMappingInfo> PropertyPathSeparateByPoints = new ConcurrentDictionary<string, PropertyMappingInfo>();
        public ConcurrentDictionary<string, PropertyMappingInfo> GetPropertyPathMap()
        {
            return PropertyPathSeparateByPoints;
        }


        /// <summary>
        /// Read exel data (datetime => .DateTime.ToString()  = 5/1/2009 9:00:00 AM)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mapping"></param>
        /// <param name="OnAssignProperty">Set property manual, return true if property has processed manually, false if automatic set</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="BadRequestException"></exception>
        public async Task<IList<T>> ReadSheetEntity<T>(ImportExcelMapping mapping, AssignPropertyAndRefPathEvent<T> OnAssignProperty)
        {
            var fields = typeof(T).GetProperties();

            PropertyPathSeparateByPoints = new ConcurrentDictionary<string, PropertyMappingInfo>();

            var data = ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var lstData = new List<T>();


            if (OnBeginParseExcelDataToEntity != null)
            {
                OnBeginParseExcelDataToEntity(data.Rows.Length);
            }


            var lstDataMapping = new List<EntityMapppingInfo<T>>();

            for (var rowIndx = 0; rowIndx < data.Rows.Length; rowIndx++)
            {

                var row = data.Rows[rowIndx];

                //bool isIgnoreRow = false;


                var checkFieldsMapping = mapping.MappingFields.Where(f => f.IsIgnoredIfEmpty || f.FieldName == ImportStaticFieldConsants.CheckImportRowEmpty);

                var isCheckingFieldEmpty = checkFieldsMapping.Any(f => string.IsNullOrWhiteSpace(row[f.Column]));

                if (isCheckingFieldEmpty)
                {
                    continue;
                }

                var travelMappingFields = mapping.MappingFields.Where(f => f.FieldName != ImportStaticFieldConsants.CheckImportRowEmpty).ToList();
                var isAllCellIsEmpty = travelMappingFields.All(m => !row.ContainsKey(m.Column) || string.IsNullOrWhiteSpace(row[m.Column]));
                if (isAllCellIsEmpty)
                {
                    continue;
                }
                var entityInfo = (T)InitObjectDeep(typeof(T));
                //for (int fieldIndx = 0; fieldIndx < mapping.MappingFields.Count; fieldIndx++)//&& !isIgnoreRow
                //{

                //    var mappingField = mapping.MappingFields[fieldIndx];

                var rowNumber = mapping.FromRow + rowIndx;

                var entityMapping = new EntityMapppingInfo<T>();
                entityMapping.Entity = entityInfo;
                entityMapping.RowNumber = rowNumber;
                entityMapping.PropertyMappings = new List<PropertyMappingValue>();


                foreach (var mappingField in travelMappingFields)//&& !isIgnoreRow
                {

                    var fieldDisplay = "";
                    try
                    {
                        string value = null;
                        if (row.ContainsKey(mappingField.Column))
                            value = row[mappingField.Column]?.ToString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            isAllCellIsEmpty = false;
                        }
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

                        if (field == null) throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy trường dữ liệu {mappingField.FieldName}");


                        if (string.IsNullOrWhiteSpace(mappingField.FieldName)) continue;

                        var fileNameCombine = mappingField.FieldName;
                        if (!string.IsNullOrWhiteSpace(mappingField.RefFieldName))
                        {
                            fileNameCombine += mappingField.RefFieldName;
                        }
                        var fieldPaths = new List<PropertyInfo>();
                        var (refField, refObj) = GetRefFieldNames(fieldPaths, fileNameCombine, entityInfo);

                        fieldDisplay = GetDisplayFieldPathString(fieldPaths);

                        var refPropertyPathSeparateByPoint = string.Join(".", fieldPaths.Select(p => p.Name).ToArray());

                        var propertyMappingInfo = PropertyPathSeparateByPoints.GetOrAdd(refPropertyPathSeparateByPoint, (key) =>
                        {
                            return new PropertyMappingInfo()
                            {
                                Column = mappingField.Column,
                                DisplayTitle = fieldDisplay,
                                RefPropertyPathSeparateByPoint = refPropertyPathSeparateByPoint,

                                Property = fieldPaths.Last(),

                                IsCheckDuplicate = field.GetCustomAttribute<ValidateDuplicateByKeyCodeAttribute>() != null,
                                IsKeyCodeField = field.GetCustomAttribute<KeyCodeFieldAttribute>() != null
                            };
                        });

                        if(mapping.HandleFilterOptionId != null)
                        {
                            var handleFilterOption = field.GetCustomAttribute<RequireWhenHandleFilterAttribute>();
                            var errmess = string.Empty;
                            if(handleFilterOption != null)
                            {
                                if (mapping.HandleFilterOptionId == EnumHandleFilterOption.FitlerByNameAndSpecification)
                                {
                                    if ((handleFilterOption.EnumHandleFilterOption == EnumHandleFilterOption.FilterByName
                                        || handleFilterOption.EnumHandleFilterOption == EnumHandleFilterOption.FitlerByNameAndSpecification) && string.IsNullOrEmpty(value)
                                        && handleFilterOption.IsNotNull)
                                    {
                                        errmess = $"Lỗi dòng {rowNumber} cột {mappingField.Column} \"{fieldDisplay}\" {handleFilterOption.ErrorMessage}";
                                    }
                                }
                                else
                                {
                                    if (handleFilterOption.EnumHandleFilterOption == mapping.HandleFilterOptionId.Value
                                   && string.IsNullOrEmpty(value) && handleFilterOption.IsNotNull)
                                    {
                                        errmess = $"Lỗi dòng {rowNumber} cột {mappingField.Column} \"{fieldDisplay}\" {handleFilterOption.ErrorMessage}";
                                    }
                                }
                            }
                            
                            if (!string.IsNullOrEmpty(errmess))
                            {
                                throw new BadRequestException(errmess);
                            }
                        }

                        var isAutoSet = true;
                        if (OnAssignProperty != null)
                        {

                            fieldPaths.RemoveAt(0);

                            //mappingField.FieldName => Properties1
                            var fieldName = mappingField.FieldName;
                            if (field.GetCustomAttribute<FieldDataNestedObjectAttribute>() != null)
                            {
                                fieldName = field.Name;
                            }
                            if (refField.GetCustomAttribute<DynamicObjectCategoryMappingAttribute>() != null)
                                isAutoSet = !(await OnAssignProperty(entityInfo, fieldName, value, refObj, mappingField.RefFieldName, refPropertyPathSeparateByPoint));
                            else
                                isAutoSet = !(await OnAssignProperty(entityInfo, fieldName, value, refObj, refField.Name, refPropertyPathSeparateByPoint));

                        }

                        if (isAutoSet && !string.IsNullOrWhiteSpace(value))
                        {
                            refField.SetValue(refObj, value.ConvertValueByType(refField.PropertyType));
                        }

                        entityMapping.PropertyMappings.Add(new PropertyMappingValue()
                        {
                            MappingInfo = propertyMappingInfo,
                            RefObject = refObj,
                            Value = value
                        });

                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Lỗi dòng {rowNumber} cột {mappingField.Column} \"{fieldDisplay}\" {ex.Message}", ex);
                    }

                }



                //if (!isIgnoreRow)
                //{
                var context = new ValidationContext(entityInfo);
                ICollection<ValidationResult> results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(entityInfo, context, results, true);
                if (!isValid)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Lỗi dữ liệu dòng {rowNumber}, " + string.Join(", ", results.FirstOrDefault()?.MemberNames) + ": " + results.FirstOrDefault()?.ErrorMessage);
                }

                if (entityInfo is MappingDataRowAbstract entity)
                {
                    entity.RowNumber = rowNumber;
                }
                lstData.Add(entityInfo);
                lstDataMapping.Add(entityMapping);
                //}

                if (OnParseExcelDataToEntity != null)
                {
                    OnParseExcelDataToEntity(rowIndx + 1, entityInfo);
                }
            }

            var keyCodeColumn = PropertyPathSeparateByPoints.Values.FirstOrDefault(p => p.IsKeyCodeField);
            var checkDuplicateColumns = PropertyPathSeparateByPoints.Values.Where(p => p.IsCheckDuplicate).ToList();

            if (keyCodeColumn != null && checkDuplicateColumns.Count > 0)
            {
                var billsGroupByKeyCode = lstDataMapping.GroupBy(e => e.PropertyMappings.FirstOrDefault(p => p.MappingInfo.IsKeyCodeField)?.Value).ToList();
                foreach (var bill in billsGroupByKeyCode)
                {
                    foreach (var mappingInfo in checkDuplicateColumns)
                    {
                        var persistentValue = bill.Where(r => !ObjectUtils.IsNullOrEmptyObject(r.PropertyMappings.FirstOrDefault(m => m.MappingInfo == mappingInfo)?.Value))
                            .GroupBy(r => r.PropertyMappings.FirstOrDefault(m => m.MappingInfo == mappingInfo)?.Value)
                            .ToList();
                        if (persistentValue.Count > 1)
                        {
                            var nextValue = persistentValue.Skip(1).Take(1).First()?.First();
                            throw GeneralCode.InvalidParams.BadRequest($"Có nhiều hơn 1 giá trị {mappingInfo?.DisplayTitle}, dòng {nextValue.RowNumber}, cột {mappingInfo?.Column} cùng đơn {bill.Key}");
                        }
                    }

                }

            }

            return lstData;
        }

        private object InitObjectDeep(Type type)
        {
            var obj = Activator.CreateInstance(type);
            foreach (var p in type.GetProperties())
            {
                var propertyIsClass = p.PropertyType.IsClass();
                if (propertyIsClass)
                {
                    var propValue = InitObjectDeep(p.PropertyType);
                    p.SetValue(obj, propValue);
                }
            }
            return obj;
        }

        private (PropertyInfo refProp, object refObj) GetRefFieldNames(IList<PropertyInfo> result, string fieldName, object parentObj)
        {
            var fieldType = parentObj.GetType();

            if (!RefTypeFields.ContainsKey(fieldType))
            {
                var fields = fieldType.GetProperties();
                RefTypeFields.TryAdd(fieldType, fields);
                foreach (var field in fields)
                {
                    DisplayNames.TryAdd(field, (field.GetCustomAttributes<DisplayAttribute>().FirstOrDefault()?.Name) ?? field.Name);
                }
            }

            RefTypeFields.TryGetValue(fieldType, out var props);
            var prop = props.FirstOrDefault(f => fieldName.StartsWith(f.Name));


            if (prop == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy trường dữ liệu {fieldName} thuộc {GetDisplayFieldPathString(result)}");
            }

            result.Add(prop);
            var refFieldName = fieldName.Substring(prop.Name.Length);
            var propertyIsClass = prop.PropertyType.IsClass();
            if (propertyIsClass)
            {
                var obj = prop.GetValue(parentObj);
                return GetRefFieldNames(result, refFieldName, obj);
            }
            var displayPath = result.Select(p =>
            {
                DisplayNames.TryGetValue(p, out var d);
                return d ?? p.Name;
            }).ToArray();


            return (prop, parentObj);
        }

        private string GetDisplayFieldPathString(IList<PropertyInfo> lst)
        {
            var displayPath = lst.Select(p =>
            {
                DisplayNames.TryGetValue(p, out var d);
                return d ?? p.Name;
            }).ToArray();
            return string.Join(" => ", displayPath);
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
                    

                    var isCellInternalDateFormatted = IsCellInternalDateFormatted(cell);

                    if (DateUtil.IsCellDateFormatted(cell) || isCellInternalDateFormatted)
                    {
                        try
                        {
                            var date = DateTime.FromOADate(cell.NumericCellValue);
                            //date = date.Date;
                            return date.ToString();
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

        private static bool IsCellInternalDateFormatted(ICell cell)
        {
            if (cell == null) return false;
            bool bDate = false;

            double d = cell.NumericCellValue;

            if (DateUtil.IsValidExcelDate(d))
            {
                ICellStyle style = cell.CellStyle;
                int formatIndex = style.DataFormat;

                //https://www.tabnine.com/code/java/methods/org.apache.poi.ss.usermodel.DateUtil/isCellDateFormatted
                if (formatIndex == 14 || formatIndex == 31 || formatIndex == 57 || formatIndex == 58 || formatIndex == 20 || formatIndex == 32)
                {
                    return true;
                }

                bDate = DateUtil.IsInternalDateFormat(formatIndex);
            }
            return bDate;
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

    public class PropertyMappingInfo
    {
        public string RefPropertyPathSeparateByPoint { get; set; }
        public string DisplayTitle { get; set; }
        public string Column { get; set; }

        public PropertyInfo Property { get; set; }
        public bool IsCheckDuplicate { get; set; }
        public bool IsKeyCodeField { get; set; }
    }


    public class PropertyMappingValue
    {
        public PropertyMappingInfo MappingInfo { get; set; }

        public object RefObject { get; set; }
        public string Value { get; set; }
    }

    public class EntityMapppingInfo<T>
    {
        public T Entity { get; set; }
        public int RowNumber { get; set; }
        public IList<PropertyMappingValue> PropertyMappings { get; set; }
    }

}
