using Microsoft.Data.SqlClient;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Organization;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Extensions;
using VErp.Infrastructure.ServiceCore.Service;
using static VErp.Services.Organization.Service.HrConfig.HrDataService;
using static VErp.Commons.Library.ExcelReader;
using Verp.Resources.GlobalObject;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Text;
using VErp.Infrastructure.EF.EFExtensions;
using static VErp.Commons.Library.EvalUtils;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Concurrent;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Services.Organization.Service.HrConfig.Abstract;
using Microsoft.EntityFrameworkCore;

namespace VErp.Services.Organization.Service.HrConfig.Facade
{
    public interface IHrDataImportDIService
    {        
        ILongTaskResourceLockService LongTaskResourceLockService { get; }
        OrganizationDBContext OrganizationDBContext { get; }
        ICategoryHelperService CategoryHelperService { get; }
        ICurrentContextService CurrentContextService { get; }
        ICustomGenCodeHelperService CustomGenCodeHelperService { get; }
    }
    public class HrDataImportDIService : IHrDataImportDIService
    {
        public HrDataImportDIService(ILongTaskResourceLockService longTaskResourceLockService, OrganizationDBContext organizationDBContext, ICategoryHelperService categoryHelperService, ICurrentContextService currentContextService, ICustomGenCodeHelperService customGenCodeHelperService)
        {           
            LongTaskResourceLockService = longTaskResourceLockService;
            OrganizationDBContext = organizationDBContext;
            CategoryHelperService = categoryHelperService;
            CurrentContextService = currentContextService;
            CustomGenCodeHelperService = customGenCodeHelperService;
        }
      

        public ILongTaskResourceLockService LongTaskResourceLockService { get; }

        public OrganizationDBContext OrganizationDBContext { get; }

        public ICategoryHelperService CategoryHelperService { get; }

        public ICurrentContextService CurrentContextService { get; }
        public ICustomGenCodeHelperService CustomGenCodeHelperService { get; }
    }

    public class HrDataImportFacade : HrDataUpdateServiceAbstract
    {
        private readonly HrType _hrType;
        private readonly Dictionary<int, List<HrValidateField>> _fieldsByArea = new Dictionary<int, List<HrValidateField>>();
        private readonly Dictionary<int, string> _areaTableName = new Dictionary<int, string>();

        private readonly ILongTaskResourceLockService _longTaskResourceLockService;

        private List<ReferFieldModel> referFields;
        private class HrImportRowData
        {
            public NonCamelCaseDictionary<string> Data { get; set; }
            public int Index { get; set; }
        }
        private IDictionary<string, IList<HrImportRowData>> sliceDataByBillCode;
        public HrDataImportFacade(HrType hrType, List<HrValidateField> fields, IHrDataImportDIService services)
            : base(services.OrganizationDBContext, services.CustomGenCodeHelperService, services.CurrentContextService, services.CategoryHelperService)
        {
            _hrType = hrType;

            _fieldsByArea = fields.GroupBy(f => f.HrAreaId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var (areaId, areaFields) in _fieldsByArea)
            {
                _areaTableName.Add(areaId, OrganizationConstants.GetHrAreaTableName(_hrType.HrTypeCode, areaFields.First().HrAreaCode));
            }

            _longTaskResourceLockService = services.LongTaskResourceLockService;
        }

        public async Task<CategoryNameModel> GetFieldDataForMapping()
        {
            var inputTypeInfo = await _organizationDBContext.HrType.AsNoTracking().FirstOrDefaultAsync(t => t.HrTypeId == _hrType.HrTypeId);

            var result = new CategoryNameModel()
            {
                //CategoryId = inputTypeInfo.HrTypeId,
                CategoryCode = inputTypeInfo.HrTypeCode,
                CategoryTitle = inputTypeInfo.Title,
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = _fieldsByArea.SelectMany(a => a.Value)
                .Where(f => !f.IsHidden && !f.IsAutoIncrement && f.FieldName != OrganizationConstants.HR_TABLE_F_IDENTITY)
                .ToList();

            var referTableNames = fields.Select(f => f.RefTableCode).ToList();

            var referFields = await _categoryHelperService.GetReferFields(referTableNames, null);
            var refCategoryFields = referFields.GroupBy(f => f.CategoryCode).ToDictionary(f => f.Key, f => f.ToList());

            foreach (var field in fields)
            {
                var fileData = new CategoryFieldNameModel()
                {
                    //CategoryFieldId = field.HrAreaFieldId,
                    FieldName = field.FieldName,
                    FieldTitle = GetTitleCategoryField(field),
                    RefCategory = null,
                    IsRequired = field.IsRequire,
                    GroupName = field.HrAreaTitle
                };

                if (!string.IsNullOrWhiteSpace(field.RefTableCode))
                {
                    if (!refCategoryFields.TryGetValue(field.RefTableCode, out var refCategory))
                    {
                        throw HrDataValidationMessage.RefTableNotFound.BadRequestFormat(field.RefTableCode);
                    }


                    fileData.RefCategory = new CategoryNameModel()
                    {
                        //CategoryId = 0,
                        CategoryCode = refCategory.FirstOrDefault()?.CategoryCode,
                        CategoryTitle = refCategory.FirstOrDefault()?.CategoryTitle,
                        IsTreeView = false,

                        Fields = GetRefFields(refCategory)
                        .Select(f => new CategoryFieldNameModel()
                        {
                            //CategoryFieldId = f.id,
                            FieldName = f.CategoryFieldName,
                            FieldTitle = f.GetTitleCategoryField(),
                            RefCategory = null,
                            IsRequired = false
                        }).ToList()
                    };
                }

                result.Fields.Add(fileData);
            }

            result.Fields.Add(new CategoryFieldNameModel
            {
                FieldName = ImportStaticFieldConsants.CheckImportRowEmpty,
                FieldTitle = "Cột kiểm tra",
            });

            return result;
        }

        public async Task<bool> ImportHrBillFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var referMapingFields = mapping.MappingFields.Where(f => !string.IsNullOrEmpty(f.RefFieldName)).ToList();
            var referTableNames = _fieldsByArea.SelectMany(a => a.Value).Where(f => referMapingFields.Select(mf => mf.FieldName).Contains(f.FieldName)).Select(f => f.RefTableCode).ToList();
            foreach (var (areaId, areaFields) in _fieldsByArea)
            {
                var requiredField = areaFields.FirstOrDefault(f => f.IsRequire && !mapping.MappingFields.Any(m => m.FieldName == f.FieldName));

                if (requiredField != null) throw HrDataValidationMessage.FieldRequired.BadRequestFormat(requiredField.Title);
            }


            referFields = await _categoryHelperService.GetReferFields(referTableNames, referMapingFields.Select(f => f.RefFieldName).ToList());


            using (var longTask = await _longTaskResourceLockService.Accquire($"Nhập dữ liệu nhân sự \"{_hrType.Title}\" từ excel"))
            {
                var reader = new ExcelReader(stream);
                reader.RegisterLongTaskEvent(longTask);

                var dataExcel = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

                var ignoreIfEmptyColumns = mapping.MappingFields.Where(f => f.IsIgnoredIfEmpty).Select(f => f.Column).ToList();
                var columnKey = mapping.MappingFields.FirstOrDefault(f => f.FieldName == OrganizationConstants.BILL_CODE);
                if (columnKey == null)
                {
                    throw HrDataValidationMessage.BillCodeError.BadRequest();
                }

                sliceDataByBillCode = dataExcel.Rows.Select((r, i) => new HrImportRowData
                {
                    Data = r,
                    Index = i + mapping.FromRow
                })
                    .Where(r => !ignoreIfEmptyColumns.Any(c => !r.Data.ContainsKey(c) || string.IsNullOrWhiteSpace(r.Data[c])))//not any empty ignore column
                    .Where(r => !string.IsNullOrWhiteSpace(r.Data[columnKey.Column]))
                    .GroupBy(r => r.Data[columnKey.Column])
                    .ToDictionary(g => g.Key, g => g.ToIList());

                var bills = new List<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>>();

                longTask.SetCurrentStep("Kiểm tra dữ liệu", sliceDataByBillCode.Count());


                foreach (var (billCode, billRows) in sliceDataByBillCode)
                {
                    var modelBill = new NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>();
                    foreach (var (areaId, areaFields) in _fieldsByArea)
                    {
                        var firstField = areaFields.First();

                        var rows = new List<NonCamelCaseDictionary>();

                        await ValidateUniqueValue(mapping, areaFields);

                        int count = billRows.Count();
                        for (int rowIndex = 0; rowIndex < count; rowIndex++)
                        {
                            var mapRow = new NonCamelCaseDictionary();
                            var row = billRows.ElementAt(rowIndex);
                            foreach (var field in areaFields)
                            {
                                var mappingField = mapping.MappingFields.FirstOrDefault(f => f.FieldName == field.FieldName);

                                if (mappingField == null && !field.IsRequire)
                                    continue;
                                else if (mappingField == null && field.IsRequire)
                                    throw BadRequestExceptionExtensions.BadRequestFormat(HrDataValidationMessage.FieldNameNotFound, field.FieldName);

                                if (!field.IsMultiRow && rowIndex > 0) continue;

                                string value = null;
                                if (row.Data.ContainsKey(mappingField.Column))
                                    value = row.Data[mappingField.Column]?.ToString();
                                // Validate require
                                if (string.IsNullOrWhiteSpace(value) && field.IsRequire) throw new BadRequestException(HrErrorCode.RequiredFieldIsEmpty, new object[] { row.Index, field.Title });

                                if (string.IsNullOrWhiteSpace(value)) continue;

                                value = value.Trim();

                                if (value.StartsWith(PREFIX_ERROR_CELL))
                                {
                                    throw ValidatorResources.ExcelFormulaNotSupported.BadRequestFormat(row.Index, mappingField.Column, $"\"{field.Title}\" {value}");
                                }

                                if (new[] { EnumDataType.Date, EnumDataType.Month, EnumDataType.QuarterOfYear, EnumDataType.Year }.Contains(field.DataTypeId))
                                {
                                    if (!DateTime.TryParse(value.ToString(), out DateTime date))
                                        throw HrDataValidationMessage.CannotConvertValueInRowFieldToDateTime.BadRequestFormat(value?.JsonSerialize(), row.Index, field.Title);
                                    value = date.AddMinutes(_currentContextService.TimeZoneOffset.Value).GetUnix().ToString();
                                }

                                // Validate refer
                                if (!(field.FormTypeId).IsSelectForm())
                                {
                                    // Validate value
                                    if (!field.IsAutoIncrement && !string.IsNullOrEmpty(value))
                                    {
                                        string regex = (field.DataTypeId).GetRegex();
                                        if ((field.DataSize > 0 && value.Length > field.DataSize)
                                            || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(value, regex))
                                            || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(value, field.RegularExpression)))
                                        {
                                            throw new BadRequestException(HrErrorCode.HrValueInValid, new object[] { value?.JsonSerialize(), row.Index, field.Title });
                                        }
                                    }
                                }
                                else
                                {

                                    value = await GetRefValue(field, mappingField, row, mapRow, value);
                                }

                                mapRow.Add(field.FieldName, value);
                            }
                            rows.Add(mapRow);
                        }

                        modelBill.Add(firstField.HrAreaCode, rows);
                    }

                    if (modelBill.Count > 0)
                        bills.Add(modelBill);

                    longTask.IncProcessedRows();

                }

                var generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();

                var @trans = await _organizationDBContext.Database.BeginTransactionAsync();
                try
                {
                    longTask.SetCurrentStep("Lưu vào cơ sở dữ liệu", bills.Count());

                    foreach (var data in bills)
                    {
                        var billInfo = new HrBill()
                        {
                            HrTypeId = _hrType.HrTypeId,
                            LatestBillVersion = 1,
                            SubsidiaryId = _currentContextService.SubsidiaryId,
                            IsDeleted = false
                        };

                        await _organizationDBContext.HrBill.AddAsync(billInfo);

                        await _organizationDBContext.SaveChangesAsync();

                        foreach (var (areaId, areaFields) in _fieldsByArea)
                        {
                            var firstField = areaFields.First();

                            if (!data.ContainsKey(firstField.HrAreaCode) || data[firstField.HrAreaCode].Count == 0) continue;

                            var tableName = OrganizationConstants.GetHrAreaTableName(_hrType.HrTypeCode, firstField.HrAreaCode);

                            var hrAreaData = firstField.IsMultiRow ? data[firstField.HrAreaCode] : new[] { data[firstField.HrAreaCode][0] };

                            await AddHrBillBase(_hrType.HrTypeId, billInfo.FId, billInfo, generateTypeLastValues, tableName, hrAreaData, areaFields, hrAreaData);

                        }

                        longTask.IncProcessedRows();
                    }

                    await @trans.CommitAsync();

                    await ConfirmCustomGenCode(generateTypeLastValues);
                }
                catch (Exception)
                {
                    await @trans.TryRollbackTransactionAsync();
                    throw;
                }
                return true;
            }
        }

        private string GetTitleCategoryField(HrValidateField field)
        {
            var rangeValue = field.DataTypeId.GetRangeValue();
            if (rangeValue.Length > 0)
            {
                return $"{field.Title} ({string.Join(", ", field.DataTypeId.GetRangeValue())})";
            }

            return field.Title;
        }

        private IList<ReferFieldModel> GetRefFields(IList<ReferFieldModel> fields)
        {
            return fields.Where(x => !x.IsHidden && x.DataTypeId != (int)EnumDataType.Boolean && !((EnumDataType)x.DataTypeId).IsTimeType())
                 .ToList();
        }


        private async Task ValidateUniqueValue(ImportExcelMapping mapping, List<HrValidateField> areaFields)
        {
            var firstField = areaFields.First();

            var tableName = OrganizationConstants.GetHrAreaTableName(_hrType.HrTypeCode, firstField.HrAreaCode);

            foreach (var field in areaFields.Where(f => f.IsUnique))
            {
                var mappingField = mapping.MappingFields.FirstOrDefault(mf => mf.FieldName == field.FieldName);
                if (mappingField == null) continue;


                var values = field.IsMultiRow
                ? sliceDataByBillCode.SelectMany(b => b.Value.Select(r => r.Data[mappingField.Column]?.ToString())).ToList()
                : sliceDataByBillCode.Where(b => b.Value.Count() > 0).Select(b => b.Value.First().Data[mappingField.Column]?.ToString()).ToList();

                // Check unique trong danh sách values thêm mới
                if (values.Distinct().Count() < values.Count)
                {
                    throw new BadRequestException(HrErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                }

                var sql = @$"SELECT v.[F_Id] FROM {tableName} v WHERE v.[{field.FieldName}] IN (";

                List<SqlParameter> sqlParams = new List<SqlParameter>();
                var suffix = 0;
                foreach (var value in values)
                {
                    var paramName = $"@{field.FieldName}_{suffix}";
                    if (suffix > 0)
                    {
                        sql += ",";
                    }
                    sql += paramName;
                    sqlParams.Add(new SqlParameter(paramName, value));
                    suffix++;
                }
                sql += ")";

                var result = await _organizationDBContext.QueryDataTableRaw(sql, sqlParams.ToArray());

                bool isExisted = result != null && result.Rows.Count > 0;
                if (isExisted)
                    throw new BadRequestException(HrErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
            }
        }


        private async Task<string> GetRefValue(HrValidateField field, ImportExcelMappingField mappingField, HrImportRowData excelRow, NonCamelCaseDictionary mapRow, string value)
        {
            int suffix = 0;
            var paramName = $"@{mappingField.RefFieldName}_{suffix}";
            var referField = referFields.FirstOrDefault(f => f.CategoryCode == field.RefTableCode && f.CategoryFieldName == mappingField.RefFieldName);
            if (referField == null)
            {
                throw HrDataValidationMessage.RefFieldNotExisted.BadRequestFormat(field.Title, field.FieldName);
            }
            var referSql = $"SELECT TOP 1 {field.RefTableField} FROM v{field.RefTableCode} WHERE {mappingField.RefFieldName} = {paramName}";
            var referParams = new List<SqlParameter>() { new SqlParameter(paramName, ((EnumDataType)referField.DataTypeId).GetSqlValue(value)) };
            suffix++;
            if (!string.IsNullOrEmpty(field.Filters))
            {
                var filters = field.Filters;
                var pattern = @"@{(?<word>\w+)}\((?<start>\d*),(?<length>\d*)\)";
                Regex rx = new Regex(pattern);
                MatchCollection match = rx.Matches(field.Filters);

                for (int i = 0; i < match.Count; i++)
                {
                    var fieldName = match[i].Groups["word"].Value;
                    var startText = match[i].Groups["start"].Value;
                    var lengthText = match[i].Groups["length"].Value;
                    mapRow.TryGetStringValue(fieldName, out string filterValue);

                    if (!string.IsNullOrEmpty(startText) && !string.IsNullOrEmpty(lengthText) && int.TryParse(startText, out int start) && int.TryParse(lengthText, out int length))
                    {
                        filterValue = filterValue.Substring(start, length);
                    }


                    if (string.IsNullOrEmpty(filterValue))
                    {
                        var beforeField = _fieldsByArea.SelectMany(f => f.Value)?.FirstOrDefault(f => f.FieldName == fieldName)?.Title;
                        throw HrDataValidationMessage.RequireFieldBeforeField.BadRequestFormat(beforeField, field.Title);
                    }
                    filters = filters.Replace(match[i].Value, filterValue);
                }

                Clause filterClause = JsonConvert.DeserializeObject<Clause>(filters);
                if (filterClause != null)
                {
                    var whereCondition = new StringBuilder();


                    try
                    {
                        var parameters = mapRow?.Where(d => !d.Value.IsNullOrEmptyObject())?.ToNonCamelCaseDictionary(k => k.Key, v => v.Value);


                        suffix = filterClause.FilterClauseProcess($"v{field.RefTableCode}", $"v{field.RefTableCode}", whereCondition, referParams, suffix, refValues: parameters);

                    }
                    catch (EvalObjectArgException agrEx)
                    {
                        var fieldBefore = (_fieldsByArea.SelectMany(f => f.Value).FirstOrDefault(f => f.FieldName == agrEx.ParamName)?.Title) ?? agrEx.ParamName;
                        throw HrDataValidationMessage.RequireFieldBeforeField.BadRequestFormat(fieldBefore, field.Title);
                    }
                    catch (Exception)
                    {
                        throw;
                    }



                    if (whereCondition.Length > 0) referSql += $" AND {whereCondition}";
                }
            }

            var referData = await _organizationDBContext.QueryDataTableRaw(referSql, referParams.ToArray());
            if (referData == null || referData.Rows.Count == 0)
            {
                // Check tồn tại
                var checkExistedReferSql = $"SELECT TOP 1 {field.RefTableField} FROM v{field.RefTableCode} WHERE {mappingField.RefFieldName} = {paramName}";
                var checkExistedReferParams = new List<SqlParameter>() { new SqlParameter(paramName, ((EnumDataType)referField.DataTypeId).GetSqlValue(value)) };
                referData = await _organizationDBContext.QueryDataTableRaw(checkExistedReferSql, checkExistedReferParams.ToArray());
                if (referData == null || referData.Rows.Count == 0)
                {
                    throw new BadRequestException(HrErrorCode.ReferValueNotFound, new object[] { excelRow.Index, field.Title + ": " + value });
                }
                else
                {
                    throw new BadRequestException(HrErrorCode.ReferValueNotValidFilter, new object[] { excelRow.Index, field.Title + ": " + value });
                }
            }
            value = referData.Rows[0][field.RefTableField]?.ToString() ?? string.Empty;
            return value;
        }
    }

}