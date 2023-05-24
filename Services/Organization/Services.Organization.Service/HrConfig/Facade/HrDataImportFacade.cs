using Microsoft.Data.SqlClient;
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
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Services.Organization.Service.HrConfig.Abstract;
using Microsoft.EntityFrameworkCore;
using Verp.Cache.RedisCache;
using VErp.Infrastructure.ServiceCore.Facade;
using DocumentFormat.OpenXml.InkML;
using System.Drawing.Drawing2D;

namespace VErp.Services.Organization.Service.HrConfig.Facade
{
    public interface IHrDataImportDIService
    {
        ILongTaskResourceLockService LongTaskResourceLockService { get; }
        OrganizationDBContext OrganizationDBContext { get; }
        ICategoryHelperService CategoryHelperService { get; }
        ICurrentContextService CurrentContextService { get; }
        ICustomGenCodeHelperService CustomGenCodeHelperService { get; }
        IActivityLogService ActivityLogService { get; }
    }
    public class HrDataImportDIService : IHrDataImportDIService
    {
        public HrDataImportDIService(ILongTaskResourceLockService longTaskResourceLockService, OrganizationDBContext organizationDBContext, ICategoryHelperService categoryHelperService, ICurrentContextService currentContextService, ICustomGenCodeHelperService customGenCodeHelperService, IActivityLogService activityLogService)
        {
            LongTaskResourceLockService = longTaskResourceLockService;
            OrganizationDBContext = organizationDBContext;
            CategoryHelperService = categoryHelperService;
            CurrentContextService = currentContextService;
            CustomGenCodeHelperService = customGenCodeHelperService;
            ActivityLogService = activityLogService;
        }


        public ILongTaskResourceLockService LongTaskResourceLockService { get; }

        public OrganizationDBContext OrganizationDBContext { get; }

        public ICategoryHelperService CategoryHelperService { get; }

        public ICurrentContextService CurrentContextService { get; }
        public ICustomGenCodeHelperService CustomGenCodeHelperService { get; }
        public IActivityLogService ActivityLogService { get; }
    }

    public class HrDataImportFacade : HrDataUpdateServiceAbstract
    {
        private readonly HrType _hrType;
        private readonly Dictionary<int, List<HrValidateField>> _fieldsByArea = new Dictionary<int, List<HrValidateField>>();
        private readonly Dictionary<int, string> _areaTableName = new Dictionary<int, string>();

        private readonly ILongTaskResourceLockService _longTaskResourceLockService;
        private readonly ObjectActivityLogFacade _hrDataActivityLog;
        private List<ReferFieldModel> referFields;
        private ImportExcelMapping _mapping;
        private class HrImportRowData
        {
            public NonCamelCaseDictionary<string> Data { get; set; }
            public int Index { get; set; }
        }

        private readonly Dictionary<int, List<ImportExcelMappingField>> _keyMappings = new Dictionary<int, List<ImportExcelMappingField>>();

        private readonly Dictionary<string, CustomGenCodeBaseValueModel> generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();

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
            _hrDataActivityLog = services.ActivityLogService.CreateObjectTypeActivityLog(EnumObjectType.HrBill);

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
                    GroupName = field.HrAreaTitle,
                    IsMultiRow = field.IsMultiRow,
                    DataTypeId = field.DataTypeId
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

        private string GetAreaAlias(int areaId)
        {
            return $"_a{areaId}";
        }

        private string GetAreaRowFIdAlias(int areaId)
        {
            return $"_a{areaId}_F_Id";
        }

        private long? GetAreaRowId(NonCamelCaseDictionary dbRow, int areaId)
        {
            var fieldName = GetAreaRowFIdAlias(areaId);
            if (dbRow.TryGetValue(fieldName, out var id))
            {
                return Convert.ToInt64(id);
            }
            return null;
        }

        public async Task<bool> ImportHrBillFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            _mapping = mapping;

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(_hrType.HrTypeId));

            var referMapingFields = mapping.MappingFields.Where(f => !string.IsNullOrEmpty(f.RefFieldName)).ToList();
            var referTableNames = _fieldsByArea.SelectMany(a => a.Value).Where(f => referMapingFields.Select(mf => mf.FieldName).Contains(f.FieldName)).Select(f => f.RefTableCode).ToList();
            foreach (var (areaId, areaFields) in _fieldsByArea)
            {
                var requiredField = areaFields.FirstOrDefault(f => f.IsRequire && !mapping.MappingFields.Any(m => m.FieldName == f.FieldName));

                if (requiredField != null) throw HrDataValidationMessage.FieldRequired.BadRequestFormat(requiredField.Title);
            }


            referFields = await _categoryHelperService.GetReferFields(referTableNames, referMapingFields.Select(f => f.RefFieldName).ToList());
            foreach (var (areaId, areaFields) in _fieldsByArea)
            {
                _keyMappings.Add(areaId, mapping.MappingFields.Where(m => areaFields.Any(a => a.FieldName == m.FieldName)).ToList());
            }

            using var longTask = await _longTaskResourceLockService.Accquire($"Nhập dữ liệu nhân sự \"{_hrType.Title}\" từ excel");

            var reader = new ExcelReader(stream);
            reader.RegisterLongTaskEvent(longTask);

            var dataExcel = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var ignoreIfEmptyColumns = mapping.MappingFields.Where(f => f.IsIgnoredIfEmpty).Select(f => f.Column).ToList();
            var columnKey = mapping.MappingFields.FirstOrDefault(f => f.FieldName == OrganizationConstants.BILL_CODE);
            if (columnKey == null)
            {
                throw HrDataValidationMessage.BillCodeError.BadRequest();
            }

            var sliceDataByBillCode = dataExcel.Rows.Select((r, i) => new HrImportRowData
            {
                Data = r,
                Index = i + mapping.FromRow
            })
                .Where(r => !ignoreIfEmptyColumns.Any(c => !r.Data.ContainsKey(c) || string.IsNullOrWhiteSpace(r.Data[c])))//not any empty ignore column
                .Where(r => !string.IsNullOrWhiteSpace(r.Data[columnKey.Column]))
                .GroupBy(r => r.Data[columnKey.Column])
                .ToDictionary(g => g.Key, g => g.ToIList());


            longTask.SetCurrentStep("Kiểm tra dữ liệu", sliceDataByBillCode.Count());

            var billCodes = sliceDataByBillCode.Keys.Where(k => !string.IsNullOrWhiteSpace(k)).Select(c => c.ToUpper()).ToList();

            var existedBills = await GetExistedBills(billCodes);


            var creatingBillds = new List<List<HrDataAreaModel>>();
            var updatingBills = new List<HrDataUpdateModel>();

            var hasDetailIdentity = mapping.MappingFields.Any(f => f.IsIdentityDetail);

            foreach (var (billCode, billRows) in sliceDataByBillCode)
            {
                var areaModels = new List<HrDataAreaModel>();

                var existedBill = existedBills.Where(a => a.Code?.ToLower() == billCode?.ToLower()).FirstOrDefault();

                var existedHrBillId = existedBill?.FId;

                if (!hasDetailIdentity && existedHrBillId > 0)
                {
                    if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                    {
                        var firstRow = billRows.First();
                        throw BadRequestExceptionExtensions.BadRequestFormat(HrDataValidationMessage.DuplicateBillCode, billCode, firstRow.Index, columnKey.Column);
                    }

                    if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Ignore)
                    {
                        continue;
                    }
                }

                foreach (var (areaId, areaFields) in _fieldsByArea)
                {
                    var firstField = areaFields.First();

                    var areaData = new HrDataAreaModel()
                    {
                        AreaId = areaId,
                        Creatings = new List<NonCamelCaseDictionary>(),
                        Updatings = new List<AreaRowUpdate>()
                    };

                    await ValidateUniqueValue(existedHrBillId, areaId, billRows);

                    int count = billRows.Count();

                    var updateAreaFIds = new HashSet<long>();


                    for (int rowIndex = 0; rowIndex < count; rowIndex++)
                    {
                        if (!firstField.IsMultiRow && rowIndex > 0) continue;

                        var row = billRows.ElementAt(rowIndex);

                        var areaRowInfo = await GetRowData(existedHrBillId, areaFields, row, rowIndex, billCode);

                        if (areaRowInfo.RowModelData?.Count == 0) continue;

                        if (existedHrBillId > 0)
                        {
                            var (areaRowFId, ignore) = GetExistsAreaRowIdAndValidateOption(firstField.IsMultiRow, updateAreaFIds, areaRowInfo, existedBill.AreaData[areaId]);
                            if (ignore) continue;

                            if (areaRowFId > 0)
                            {
                                areaData.Updatings.Add(new AreaRowUpdate()
                                {
                                    RowInfo = areaRowInfo.RowModelData,
                                    ExistedRowId_Id = areaRowFId.Value,
                                });
                            }
                            else
                            {
                                areaData.Creatings.Add(areaRowInfo.RowModelData);
                            }
                        }
                        else
                        {
                            areaData.Creatings.Add(areaRowInfo.RowModelData);
                        }
                    }

                    if (areaData.Creatings.Count > 0 || areaData.Updatings.Count > 0)
                    {
                        areaModels.Add(areaData);
                    }
                }

                if (areaModels.Count > 0)
                {
                    if (existedHrBillId > 0)
                    {
                        updatingBills.Add(new HrDataUpdateModel()
                        {
                            HrBillId = existedHrBillId,
                            SoCt = billCode,
                            ExistedData = existedBill,
                            Areas = areaModels,
                        });
                    }
                    else
                    {
                        creatingBillds.Add(areaModels);
                    }
                }
                longTask.IncProcessedRows();

            }
            var fieldNames = new List<string>();


            var generateTypeLastValues = new Dictionary<string, CustomGenCodeBaseValueModel>();

            var @logBatch = _hrDataActivityLog.BeginBatchLog();
            var @trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                longTask.SetCurrentStep("Lưu vào cơ sở dữ liệu", creatingBillds.Count + updatingBills.Count);

                await Creates(creatingBillds, longTask);

                await Updates(updatingBills, longTask);

                await @trans.CommitAsync();

                await @logBatch.CommitAsync();

                await ConfirmCustomGenCode(generateTypeLastValues);
            }
            catch (Exception)
            {
                await @trans.TryRollbackTransactionAsync();
                throw;
            }
            return true;

        }

        private (long? areaRowFId, bool isIgnore) GetExistsAreaRowIdAndValidateOption(bool isMultiRow, HashSet<long> updateAreaFIds, AreaRowInfoModel areaRowInfo, List<NonCamelCaseDictionary> existedBillData)
        {
            var areaFields = _fieldsByArea[areaRowInfo.AreaId];


            if (!isMultiRow)
            {
                var areaRowFId = existedBillData.Select(e => GetAreaRowId(e, areaRowInfo.AreaId)).FirstOrDefault();

                return (areaRowFId, _mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Ignore);
            }
            else
            {
                if (areaRowInfo.RowModelKey.Count == 0) return (null, false);

                var areaRowFId = GetAndValidateExistedAreaRowId(areaRowInfo, existedBillData);

                if (areaRowFId > 0)
                {
                    if (_mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                    {
                        var (dataMessage, firstMapping) = GetIdentityValueDataMessage(areaRowInfo.RowModelKey, areaFields);

                        throw BadRequestExceptionExtensions.BadRequestFormat(HrDataValidationMessage.MultiRowIdentityExisted, dataMessage, areaRowInfo.BillCode, areaRowInfo.RowExcelData.Index, firstMapping.Column);
                    }

                    if (_mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Ignore)
                    {
                        return (null, true);
                    }
                }


                if (updateAreaFIds.Contains(areaRowFId))
                {
                    var (dataMessage, firstMapping) = GetIdentityValueDataMessage(areaRowInfo.RowModelKey, areaFields);

                    throw BadRequestExceptionExtensions.BadRequestFormat(HrDataValidationMessage.MultiRowIdentityIncorrectInExcel, dataMessage, areaRowInfo.BillCode, areaRowInfo.RowExcelData.Index, firstMapping.Column);
                }
                else
                {
                    if (areaRowFId > 0)
                        updateAreaFIds.Add(areaRowFId);
                    return (areaRowFId, false);
                }

            }
        }


        private long GetAndValidateExistedAreaRowId(AreaRowInfoModel areaRowInfo, List<NonCamelCaseDictionary> existedBillData)
        {
            List<HrValidateField> areaFields = _fieldsByArea[areaRowInfo.AreaId];
            var existedRowsInDb = new List<NonCamelCaseDictionary>();

            foreach (var existedRow in existedBillData)
            {
                var isExist = true;
                foreach (var (keyFieldName, value) in areaRowInfo.RowModelKey)
                {
                    existedRow.TryGetStringValue(keyFieldName, out var existedValue);
                    if (value?.ToString() != existedValue)
                    {
                        isExist = false;
                    }
                }

                if (isExist)
                {
                    existedRowsInDb.Add(existedRow);
                }
            }

            var idInDb = existedRowsInDb.Select(e => GetAreaRowId(e, areaRowInfo.AreaId)).Where(id => id.HasValue).Distinct();

            var totalExistedRowsInDb = idInDb.Count();

            if (totalExistedRowsInDb > 1)
            {
                var (dataMessage, firstMapping) = GetIdentityValueDataMessage(areaRowInfo.RowModelKey, areaFields);

                throw BadRequestExceptionExtensions.BadRequestFormat(HrDataValidationMessage.MultiRowIdentityIncorrectInDb, dataMessage, areaRowInfo.BillCode, areaRowInfo.RowExcelData.Index, firstMapping.Column);
            }

            if (totalExistedRowsInDb == 1)
            {
                return idInDb.First() ?? 0;
            }

            return 0;
        }

        private async Task<AreaRowInfoModel> GetRowData(long? existedHrBillId, List<HrValidateField> areaFields, HrImportRowData row, int rowIndex, string billCode)
        {
            var areaRowKey = new NonCamelCaseDictionary();
            var rowInfo = new NonCamelCaseDictionary();

            foreach (var field in areaFields)
            {
                var mappingField = _mapping.MappingFields.FirstOrDefault(f => f.FieldName == field.FieldName);
                if (mappingField == null)
                {
                    if ((existedHrBillId == 0 || !existedHrBillId.HasValue) && field.IsRequire)
                        throw BadRequestExceptionExtensions.BadRequestFormat(HrDataValidationMessage.FieldNameNotFound, field.FieldName);
                    continue;
                }

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
                        var strValue = value.NormalizeAsInternalName();
                        string regex = (field.DataTypeId).GetRegex();
                        if ((field.DataSize > 0 && value.Length > field.DataSize)
                            || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(strValue, regex))
                            || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(strValue, field.RegularExpression)))
                        {
                            throw new BadRequestException(HrErrorCode.HrValueInValid, new object[] { value?.JsonSerialize(), row.Index, field.Title });
                        }
                    }
                }
                else
                {

                    value = await GetRefValue(field, mappingField, row, rowInfo, value);
                }

                rowInfo.Add(field.FieldName, value);

                if (mappingField.IsIdentityDetail)
                {
                    areaRowKey.Add(field.FieldName, value);
                }
            }

            var firstField = areaFields.First();
            return new AreaRowInfoModel()
            {
                AreaId = firstField.HrAreaId,
                BillCode = billCode,
                RowExcelData = row,
                RowModelData = rowInfo,
                RowModelKey = areaRowKey
            };
        }

        private async Task<List<HrBillInforByAreaModel>> GetExistedBills(IList<string> billCodes)
        {
            
            var codeField = _fieldsByArea.SelectMany(a => a.Value).FirstOrDefault(v => v.FieldName == OrganizationConstants.BILL_CODE);

            if (codeField == null) throw GeneralCode.NotYetSupported.BadRequest();

            var codeAreaAlias = GetAreaAlias(codeField.HrAreaId);
          
            var sql = $"SELECT {SelectAreaColumns(codeField.HrAreaId, codeAreaAlias, _fieldsByArea[codeField.HrAreaId])} FROM dbo.HrBill bill " +
               $"JOIN {_areaTableName[codeField.HrAreaId]} {codeAreaAlias} ON bill.F_Id = {codeAreaAlias}.[HrBill_F_Id] " +
               $"WHERE bill.SubSidiaryId = @SubId AND bill.IsDeleted = 0 AND {codeAreaAlias}.IsDeleted = 0 " +
               $"AND bill.HrTypeId = @HrTypeId " +
               $"AND {OrganizationConstants.BILL_CODE} IN (SELECT NValue FROM @billCodes) ";

            var queryParams = new[]
            {
                    new SqlParameter("@HrTypeId", _hrType.HrTypeId),
                    billCodes.ToSqlParameter("@billCodes")
            };

            var codeAreaData = (await _organizationDBContext.QueryDataTableRaw(sql, queryParams)).ConvertData();


            var identityBills = codeAreaData.Select(d => new
            {
                FId = Convert.ToInt64(d["HrBill_F_Id"]),
                Code = d[OrganizationConstants.BILL_CODE]
            }).Distinct().ToList();
            var fIds = identityBills.Select(b => b.FId).Distinct().ToList();


            var result = identityBills.Select(b =>
            new HrBillInforByAreaModel()
            {
                FId = b.FId,
                Code = b.Code?.ToString(),
                AreaData = _fieldsByArea.ToDictionary(a => a.Key, a =>

                    a.Key == codeField.HrAreaId ? codeAreaData.Where(d => Convert.ToInt64(d["HrBill_F_Id"]) == b.FId).ToList() 
                                    : new List<NonCamelCaseDictionary>()
                )
            }).ToList();

            foreach (var (areaId, areaFields) in _fieldsByArea)
            {
                if (areaId == codeField.HrAreaId) continue;
                var alias = GetAreaAlias(areaId);


                sql = $"SELECT {SelectAreaColumns(areaId, alias, _fieldsByArea[areaId])} FROM {_areaTableName[areaId]} {alias} JOIN @fIds fId ON {alias}.HrBill_F_Id = fId.[Value] " +
                  $"WHERE {alias}.IsDeleted = 0 ";

                queryParams = new[]
                {
                     fIds.ToSqlParameter("@fIds")
                };

                var areaData = (await _organizationDBContext.QueryDataTableRaw(sql, queryParams)).ConvertData();

                foreach (var d in areaData)
                {
                    var fId = Convert.ToInt64(d["HrBill_F_Id"]);
                    var info = result.First(r => r.FId == fId);
                    info.AreaData[areaId].Add(d);
                }

            }

            return result;
        }

        private string SelectAreaColumns(int areaId,string alias,  List<HrValidateField> areaFields)
        {
            var areaFIdColumn = GetAreaRowFIdAlias(areaId);
            var columns = new List<string>()
            {
                $"{alias}.HrBill_F_Id",
                $"{alias}.F_Id AS {areaFIdColumn}",
            };
            foreach(var f in areaFields)
            {
                columns.Add($"{alias}.[{f.FieldName}]");
            }
            return string.Join(",", columns.ToArray());
        }
        private async Task Creates(List<List<HrDataAreaModel>> creatingBillds, LongTaskResourceLock longTask)
        {
            foreach (var data in creatingBillds)
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
                    var hrAreaData = data.FirstOrDefault(d => d.AreaId == areaId);
                    if (hrAreaData == null) continue;

                    var firstField = areaFields.First();

                    var hrAreaRows = firstField.IsMultiRow ? hrAreaData.Creatings : new[] { hrAreaData.Creatings.Count > 0 ? hrAreaData.Creatings.First() : new NonCamelCaseDictionary() };

                    await AddHrBillBase(_hrType.HrTypeId, billInfo.FId, billInfo, generateTypeLastValues, _areaTableName[areaId], hrAreaRows, areaFields, hrAreaRows);

                }

                await _hrDataActivityLog.LogBuilder(() => HrBillActivityLogMessage.CreateFromExcel)
                  .MessageResourceFormatDatas(_hrType.Title, billInfo.BillCode)
                  .BillTypeId(_hrType.HrTypeId)
                  .ObjectId(billInfo.FId)
                  .JsonData(data.JsonSerialize())
                  .CreateLog();

                longTask.IncProcessedRows();
            }
        }
        private async Task Updates(List<HrDataUpdateModel> updatingBills, LongTaskResourceLock longTask)
        {
            foreach (var data in updatingBills)
            {
                var billInfo = await _organizationDBContext.HrBill.FirstOrDefaultAsync(b => b.FId == data.HrBillId);
                billInfo.LatestBillVersion++;

                await _organizationDBContext.SaveChangesAsync();

                foreach (var (areaId, areaFields) in _fieldsByArea)
                {
                    var hrAreaData = data.Areas.FirstOrDefault(d => d.AreaId == areaId);
                    if (hrAreaData == null) continue;


                    //var hrAreaRows = new List<NonCamelCaseDictionary>();

                    var oldData = new Dictionary<long, NonCamelCaseDictionary>();
                    foreach (var dbRow in data.ExistedData.AreaData[areaId])
                    {
                        dbRow.TryGetValue(GetAreaRowFIdAlias(areaId), out var areaFId);
                        if (!areaFId.IsNullOrEmptyObject())
                        {
                            var fId = Convert.ToInt64(areaFId);
                            if (fId > 0 && !oldData.ContainsKey(fId))
                            {
                                var row = new NonCamelCaseDictionary();
                                foreach (var f in areaFields)
                                {
                                    row.Add(f.FieldName, dbRow[f.FieldName]);
                                }
                                oldData.Add(fId, row);
                            }
                        }
                    }


                    foreach (var row in hrAreaData.Updatings)
                    {
                        var updateSql = new StringBuilder();

                        updateSql.AppendLine($" UPDATE {_areaTableName[areaId]} ");
                        updateSql.AppendLine($" SET [UpdatedByUserId] = @UpdatedByUserId, [UpdatedDatetimeUtc] = @UpdatedDatetimeUtc ");

                        var sqlParams = new List<SqlParameter>()
                            {
                                new SqlParameter($"@{HR_TABLE_F_IDENTITY}", row.ExistedRowId_Id),
                                new SqlParameter("@UpdatedDatetimeUtc", DateTime.UtcNow),
                                new SqlParameter("@UpdatedByUserId", _currentContextService.UserId)
                            };

                        var oldRow = oldData[row.ExistedRowId_Id];

                        foreach (var f in row.RowInfo)
                        {
                            var fieldInfo = areaFields.First(af => af.FieldName == f.Key);

                            updateSql.Append($", [{f.Key}] = @{f.Key} ");
                            oldRow[f.Key] = f.Value;
                            sqlParams.Add(new SqlParameter($"@{f.Key}", fieldInfo.DataTypeId.GetSqlValue(f.Value)));
                        }

                        updateSql.AppendLine($" WHERE {HR_TABLE_F_IDENTITY} = @{HR_TABLE_F_IDENTITY} ");

                        await _organizationDBContext.Database.ExecuteSqlRawAsync(updateSql.ToString(), sqlParams);
                    }

                    var allRows = oldData.Select(d => d.Value).ToList();
                    var toCreate = hrAreaData.Creatings.ToList();

                    allRows.AddRange(toCreate);

                    await AddHrBillBase(_hrType.HrTypeId, billInfo.FId, billInfo, generateTypeLastValues, _areaTableName[areaId], allRows, areaFields, toCreate);

                }

                await _hrDataActivityLog.LogBuilder(() => HrBillActivityLogMessage.UpdateFromExcel)
                .MessageResourceFormatDatas(_hrType.Title, billInfo.BillCode)
                .BillTypeId(_hrType.HrTypeId)
                .ObjectId(billInfo.FId)
                .JsonData(data.JsonSerialize())
                .CreateLog();

                longTask.IncProcessedRows();
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


        private (string dataMessage, ImportExcelMappingField firstField) GetIdentityValueDataMessage(NonCamelCaseDictionary areaRowKey, List<HrValidateField> areaFields)
        {
            var indentityValues = areaRowKey.Select(v =>
            {
                var f = areaFields.FirstOrDefault(af => af.FieldName == v.Key);
                return $"{f.Title}: {v.Value}";
            }).ToArray();
            var strValue = string.Join(",", indentityValues);

            var mappingField = _mapping.MappingFields.FirstOrDefault(f => f.FieldName == areaRowKey.First().Key);
            return (strValue, mappingField);
        }


        private async Task ValidateUniqueValue(long? hrBill_F_Id, int areaId, IList<HrImportRowData> rows)
        {
            var areaFields = _fieldsByArea[areaId];

            var tableName = _areaTableName[areaId];

            foreach (var field in areaFields.Where(f => f.IsUnique))
            {
                var mappingField = _mapping.MappingFields.FirstOrDefault(mf => mf.FieldName == field.FieldName);
                if (mappingField == null) continue;


                var values = field.IsMultiRow
                ? rows.Select(r => r.Data[mappingField.Column]?.ToString()).ToArray()
                : new[] { rows.First().Data[mappingField.Column]?.ToString() };

                // Check unique trong danh sách values thêm mới
                if (values.Distinct().Count() < values.Length)
                {
                    throw new BadRequestException(HrErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                }

                var sql = @$"SELECT v.[F_Id], v.[{field.FieldName}] FROM {tableName} v WHERE v.HrBill_F_Id <> @HrBill_F_Id AND v.[{field.FieldName}] IN (SELECT NValue FROM @Values) AND v.IsDeleted = 0";

                List<SqlParameter> sqlParams = new List<SqlParameter>
                {
                    values.ToSqlParameter("@Values"),
                    new SqlParameter("@HrBill_F_Id", hrBill_F_Id??0)
                };

                var result = await _organizationDBContext.QueryDataTableRaw(sql, sqlParams.ToArray());

                bool isExisted = result != null && result.Rows.Count > 0;
                if (isExisted)
                    throw new BadRequestException(HrErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title + $": {result.Rows[0][field.FieldName]}, cột {mappingField.Column}" });
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

        private class HrDataUpdateModel
        {

            public IList<HrDataAreaModel> Areas { get; set; }
            public HrBillInforByAreaModel ExistedData { get; internal set; }
            public long? HrBillId { get; internal set; }
            public string SoCt { get; internal set; }
        }


        private sealed class HrDataAreaModel
        {
            public int AreaId { get; set; }
            public IList<AreaRowUpdate> Updatings { get; set; }
            public IList<NonCamelCaseDictionary> Creatings { get; set; }
        }


        private sealed class AreaRowUpdate
        {
            public AreaRowUpdate()
            {
                RowInfo = new NonCamelCaseDictionary();
            }
            public NonCamelCaseDictionary RowInfo { get; set; }
            public long ExistedRowId_Id { get; set; }
        }

        private sealed class AreaRowInfoModel
        {
            public int AreaId { get; set; }
            public HrImportRowData RowExcelData { get; set; }
            public NonCamelCaseDictionary RowModelData { get; set; }
            public NonCamelCaseDictionary RowModelKey { get; set; }
            public string BillCode { get; set; }

        }

        private sealed class HrBillInforByAreaModel
        {
            public long FId { get; set; }
            public string Code { get; set; }
            public Dictionary<int, List<NonCamelCaseDictionary>> AreaData { get; set; }

        }
    }

}