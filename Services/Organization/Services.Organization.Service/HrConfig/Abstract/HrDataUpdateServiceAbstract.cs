using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using static VErp.Commons.Library.EvalUtils;
using static VErp.Services.Organization.Service.HrConfig.HrDataService;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using Verp.Resources.Organization;
using VErp.Infrastructure.EF.EFExtensions;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using OpenXmlPowerTools;
using DocumentFormat.OpenXml.Math;
using VErp.Commons.GlobalObject.InternalDataInterface.System;

namespace VErp.Services.Organization.Service.HrConfig.Abstract
{

    public abstract class HrDataUpdateServiceAbstract
    {
        //private const string HR_TABLE_NAME_PREFIX = OrganizationConstants.HR_TABLE_NAME_PREFIX;
        protected const string HR_TABLE_F_IDENTITY = OrganizationConstants.HR_TABLE_F_IDENTITY;
        protected const string HR_BILL_ID_FIELD_IN_AREA = "HrBill_F_Id";

        protected readonly OrganizationDBContext _organizationDBContext;
        protected readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        protected readonly ICurrentContextService _currentContextService;
        protected readonly ICategoryHelperService _categoryHelperService;

        public HrDataUpdateServiceAbstract(
            OrganizationDBContext organizationDBContext,
            ICustomGenCodeHelperService customGenCodeHelperService,
            ICurrentContextService currentContextService,
            ICategoryHelperService categoryHelperService
            )
        {
            _organizationDBContext = organizationDBContext;
            _customGenCodeHelperService = customGenCodeHelperService;
            _currentContextService = currentContextService;
            _categoryHelperService = categoryHelperService;
        }


        protected async Task AddHrBillBase(int hrTypeId, long hrBill_F_Id, HrBill billInfo, Dictionary<string, CustomGenCodeBaseValueModel> generateTypeLastValues, string tableName, IEnumerable<NonCamelCaseDictionary> hrAreaData, IEnumerable<HrValidateField> hrAreaFields, IEnumerable<NonCamelCaseDictionary> newHrAreaData)
        {
            var checkData = newHrAreaData.Select(data => new ValidateRowModel(data, null))
                                                             .ToList();

            var requiredFields = hrAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire).ToList();
            var uniqueFields = hrAreaFields.Where(f => !f.IsAutoIncrement && f.IsUnique).ToList();
            var selectFields = hrAreaFields.Where(f => !f.IsAutoIncrement && f.FormTypeId.IsSelectForm()).ToList();

            // Check field required
            await CheckRequired(checkData, requiredFields, hrAreaFields);
            // Check refer
            await CheckReferAsync(checkData, selectFields, hrAreaFields);
            // Check unique
            await CheckUniqueAsync(hrTypeId, tableName, checkData, uniqueFields, hrBill_F_Id);

            // Check value
            CheckValue(checkData, hrAreaFields);


            var infoFields = hrAreaFields.Where(f => !f.IsMultiRow).ToDictionary(f => f.FieldName, f => f);

            await FillGenerateColumn(hrBill_F_Id, generateTypeLastValues, infoFields, hrAreaData);

            if (billInfo != null && hrAreaData.FirstOrDefault().TryGetStringValue(OrganizationConstants.BILL_CODE, out var sct))
            {
                Utils.ValidateCodeSpecialCharactors(sct);
                sct = sct?.ToUpper();
                hrAreaData.FirstOrDefault()[OrganizationConstants.BILL_CODE] = sct;
                billInfo.BillCode = sct;
            }

            foreach (var row in newHrAreaData)
            {
                var columns = new List<string>();
                var sqlParams = new List<SqlParameter>();

                foreach (var f in hrAreaFields)
                {
                    if (!row.ContainsKey(f.FieldName)) continue;

                    columns.Add(f.FieldName);
                    sqlParams.Add(new SqlParameter("@" + f.FieldName, f.DataTypeId.GetSqlValue(row[f.FieldName])));
                }

                columns.AddRange(GetColumnGlobal());
                sqlParams.AddRange(GetSqlParamsGlobal());

                columns.Add("HrBill_F_Id");
                sqlParams.Add(new SqlParameter("@HrBill_F_Id", hrBill_F_Id));

                var idParam = new SqlParameter("@F_Id", SqlDbType.Int) { Direction = ParameterDirection.Output };
                sqlParams.Add(idParam);


                var sql = $"INSERT INTO [{tableName}]({string.Join(",", columns.Select(c => $"[{c}]"))}) VALUES({string.Join(",", sqlParams.Where(p => p.ParameterName != "@F_Id").Select(p => $"{p.ParameterName}"))}); SELECT @F_Id = SCOPE_IDENTITY();";

                await _organizationDBContext.Database.ExecuteSqlRawAsync($"{sql}", sqlParams);

                if (null == idParam.Value)
                    throw new InvalidProgramException();

            }
            await _organizationDBContext.SaveChangesAsync();


        }

        protected string[] GetColumnGlobal()
        {
            return new string[]
            {
                "CreatedByUserId",
                "UpdatedByUserId",
                "CreatedDatetimeUtc",
                "UpdatedDatetimeUtc",
                "SubsidiaryId"
            };
        }

        protected SqlParameter[] GetSqlParamsGlobal()
        {
            return new SqlParameter[]
            {
                new SqlParameter("@CreatedByUserId", _currentContextService.UserId),
                new SqlParameter("@UpdatedByUserId", _currentContextService.UserId),
                new SqlParameter("@CreatedDatetimeUtc", DateTime.UtcNow),
                new SqlParameter("@UpdatedDatetimeUtc",DateTime.UtcNow),
                new SqlParameter("@SubsidiaryId", _currentContextService.SubsidiaryId)
            };
        }


        private async Task FillGenerateColumn(long? fId, Dictionary<string, CustomGenCodeBaseValueModel> generateTypeLastValues, Dictionary<string, HrValidateField> fields, IEnumerable<NonCamelCaseDictionary> rows)
        {
            for (var i = 0; i < rows.Count(); i++)
            {
                var row = rows.ElementAt(i);

                foreach (var infoField in fields)
                {
                    var field = infoField.Value;

                    if (field.FormTypeId == EnumFormType.Generate &&
                        (!row.TryGetStringValue(field.FieldName, out var value) || value.IsNullOrEmptyObject())
                    )
                    {

                        var code = rows.FirstOrDefault(r => r.ContainsKey(OrganizationConstants.BILL_CODE))?[OrganizationConstants.BILL_CODE]?.ToString();

                        var ngayCt = rows.FirstOrDefault(r => r.ContainsKey(OrganizationConstants.BILL_DATE))?[OrganizationConstants.BILL_DATE]?.ToString();

                        long? ngayCtValue = null;
                        if (long.TryParse(ngayCt, out var v))
                        {
                            ngayCtValue = v;
                        }

                        CustomGenCodeOutputModel currentConfig;
                        try
                        {
                            currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.HrTypeRow, EnumObjectType.HrAreaField, field.HrAreaFieldId, fId, code, ngayCtValue);

                            if (currentConfig == null)
                            {
                                throw new BadRequestException(GeneralCode.ItemNotFound, "Thiết định cấu hình sinh mã null " + field.Title);
                            }
                        }
                        catch (BadRequestException badRequest)
                        {
                            throw new BadRequestException(badRequest.Code, "Cấu hình sinh mã " + field.Title + " => " + badRequest.Message);
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        var generateType = $"{currentConfig.CustomGenCodeId}_{currentConfig.CurrentLastValue.BaseValue}";

                        if (!generateTypeLastValues.ContainsKey(generateType))
                        {
                            generateTypeLastValues.Add(generateType, currentConfig.CurrentLastValue);
                        }

                        var lastTypeValue = generateTypeLastValues[generateType];


                        try
                        {

                            var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, lastTypeValue.LastValue, fId, code, ngayCtValue);
                            if (generated == null)
                            {
                                throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã " + field.Title);
                            }


                            value = generated.CustomCode;
                            lastTypeValue.LastValue = generated.LastValue;
                            lastTypeValue.LastCode = generated.CustomCode;
                        }
                        catch (BadRequestException badRequest)
                        {
                            throw new BadRequestException(badRequest.Code, "Sinh mã " + field.Title + " => " + badRequest.Message);
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                        if (!row.ContainsKey(field.FieldName))
                        {
                            row.Add(field.FieldName, value);
                        }
                        else
                        {
                            row[field.FieldName] = value;
                        }
                    }
                }
            }
        }
        protected async Task ConfirmCustomGenCode(Dictionary<string, CustomGenCodeBaseValueModel> generateTypeLastValues)
        {
            foreach (var (_, value) in generateTypeLastValues)
            {
                await _customGenCodeHelperService.ConfirmCode(value);
            }
        }

        private string[] GetFieldInFilter(Clause[] clauses)
        {
            List<string> fields = new List<string>();
            foreach (var clause in clauses)
            {
                if (clause == null) continue;

                if (clause is SingleClause)
                {
                    fields.Add((clause as SingleClause).FieldName);
                }
                else if (clause is ArrayClause)
                {
                    fields.AddRange(GetFieldInFilter((clause as ArrayClause).Rules.ToArray()));
                }
            }

            return fields.Distinct().ToArray();
        }


        private async Task CheckRequired(IEnumerable<ValidateRowModel> rows, IEnumerable<HrValidateField> requiredFields, IEnumerable<HrValidateField> hrAreaFields)
        {
            var filters = requiredFields
                .Where(f => !string.IsNullOrEmpty(f.RequireFilters))
                .ToDictionary(f => f.FieldName, f => JsonConvert.DeserializeObject<Clause>(f.RequireFilters));

            string[] filterFieldNames = GetFieldInFilter(filters.Select(f => f.Value).ToArray());
            var sfFields = hrAreaFields.Where(f => ((EnumFormType)f.FormTypeId).IsSelectForm() && filterFieldNames.Contains(f.FieldName)).ToList();
            var sfValues = new Dictionary<string, Dictionary<object, object>>();

            foreach (var field in sfFields)
            {
                var values = rows.Where(r => r.Data.ContainsKey(field.FieldName) && r.Data[field.FieldName] != null).Select(r => r.Data[field.FieldName]).ToList();

                if (values.Count > 0)
                {
                    Dictionary<object, object> mapTitles = new Dictionary<object, object>(new DataEqualityComparer(field.DataTypeId));
                    var sqlParams = new List<SqlParameter>();
                    var sql = new StringBuilder($"SELECT DISTINCT {field.RefTableField}, {field.RefTableTitle} FROM v{field.RefTableCode} WHERE {field.RefTableField} IN (");
                    var suffix = 0;
                    foreach (var value in values)
                    {
                        var paramName = $"@{field.RefTableField}_{suffix}";
                        if (suffix > 0) sql.Append(",");
                        sql.Append(paramName);
                        sqlParams.Add(new SqlParameter(paramName, value) { SqlDbType = (field.DataTypeId).GetSqlDataType() });
                        suffix++;
                    }

                    sql.Append(")");
                    var data = await _organizationDBContext.QueryDataTableRaw(sql.ToString(), sqlParams.ToArray());
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        mapTitles.Add(data.Rows[i][field.RefTableField], data.Rows[i][field.RefTableTitle]);
                    }
                    sfValues.Add(field.FieldName, mapTitles);
                }
            }


            foreach (var field in requiredFields)
            {
                // ignore auto generate field
                if (field.FormTypeId == EnumFormType.Generate) continue;


                foreach (var (row, index) in rows.Select((value, i) => (value, i + 1)))
                {
                    if (row.CheckFields != null && !row.CheckFields.Contains(field.FieldName))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(field.RequireFilters))
                    {
                        Clause filterClause = JsonConvert.DeserializeObject<Clause>(field.RequireFilters);
                        if (filterClause != null && !(await CheckRequireFilter(filterClause, rows, hrAreaFields, sfValues)))
                        {
                            continue;
                        }
                    }

                    row.Data.TryGetStringValue(field.FieldName, out string value);
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new BadRequestException(HrErrorCode.RequiredFieldIsEmpty, new object[] { index, field.Title });
                    }
                }
            }
        }

        private async Task CheckUniqueAsync(int hrTypeId, string tableName, List<ValidateRowModel> rows, List<HrValidateField> uniqueFields, long? hrValueBillId = null)
        {
            // Check unique
            foreach (var field in uniqueFields)
            {
                foreach (var row in rows)
                {
                    if (row.CheckFields != null && !row.CheckFields.Contains(field.FieldName))
                    {
                        continue;
                    }
                    // Get list change value
                    List<object> values = new List<object>();
                    row.Data.TryGetValue(field.FieldName, out object value);
                    if (value != null)
                    {
                        values.Add(((EnumDataType)field.DataTypeId).GetSqlValue(value));
                    }
                    // Check unique trong danh sách values thêm mới/sửa
                    if (values.Count != values.Distinct().Count())
                    {
                        throw new BadRequestException(HrErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                    }
                    if (values.Count == 0)
                    {
                        continue;
                    }
                    // Checkin unique trong db
                    await ValidUniqueAsync(hrTypeId, tableName, values, field, hrValueBillId);
                }
            }
        }

        private async Task ValidUniqueAsync(int hrTypeId, string tableName, List<object> values, HrValidateField field, long? HrValueBillId = null)
        {
            var existSql = $"SELECT F_Id FROM {tableName} WHERE IsDeleted = 0 ";
            if (HrValueBillId.HasValue)
            {
                existSql += $"AND HrBill_F_Id != {HrValueBillId}";
            }
            existSql += $" AND {field.FieldName} IN (";
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            var suffix = 0;
            foreach (var value in values)
            {
                var paramName = $"@{field.FieldName}_{suffix}";
                if (suffix > 0)
                {
                    existSql += ",";
                }
                existSql += paramName;
                sqlParams.Add(new SqlParameter(paramName, value));
                suffix++;
            }
            existSql += ")";
            var result = await _organizationDBContext.QueryDataTableRaw(existSql, sqlParams.ToArray());
            bool isExisted = result != null && result.Rows.Count > 0;

            if (isExisted)
            {
                throw new BadRequestException(HrErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
            }
        }

        private async Task CheckReferAsync(List<ValidateRowModel> rows, List<HrValidateField> selectFields, IEnumerable<HrValidateField> hrAreaFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    await ValidReferAsync(rows[i], field, hrAreaFields);
                }
            }
        }

        private async Task ValidReferAsync(ValidateRowModel checkData, HrValidateField field, IEnumerable<HrValidateField> hrAreaFields)
        {
            string tableName = $"v{field.RefTableCode}";

            if (checkData.CheckFields != null && !checkData.CheckFields.Contains(field.FieldName))
            {
                return;
            }

            checkData.Data.TryGetStringValue(field.FieldName, out string textValue);

            if (string.IsNullOrEmpty(textValue))
            {
                return;
            }

            var value = ((EnumDataType)field.DataTypeId).GetSqlValue(textValue);
            var whereCondition = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            int suffix = 0;
            var existSql = $"SELECT F_Id FROM {tableName} WHERE {field.RefTableField}";

            var referField = (await _categoryHelperService.GetReferFields(new[] { field.RefTableCode }, new[] { field.RefTableField })).FirstOrDefault();

            if (field.FormTypeId == EnumFormType.MultiSelect)
            {
                var sValue = ((string)value).TrimEnd(']').TrimStart('[').Trim();
                existSql += " IN (";
                foreach (var v in sValue.Split(','))
                {
                    var paramName = $"@{field.RefTableField}_{suffix}";
                    existSql += $"{paramName},";
                    sqlParams.Add(new SqlParameter(paramName, ((EnumDataType)referField.DataTypeId).GetSqlValue(v)));
                    suffix++;
                }
                existSql = existSql.TrimEnd(',');
                existSql += ") ";
            }
            else
            {
                var paramName = $"@{field.RefTableField}_{suffix}";
                existSql += $" = {paramName}";
                sqlParams.Add(new SqlParameter(paramName, value));
            }

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
                    checkData.Data.TryGetStringValue(fieldName, out string filterValue);

                    if (!string.IsNullOrEmpty(startText) && !string.IsNullOrEmpty(lengthText) && int.TryParse(startText, out int start) && int.TryParse(lengthText, out int length))
                    {
                        filterValue = filterValue.Substring(start, length);
                    }
                    if (string.IsNullOrEmpty(filterValue))
                    {
                        var fieldBefore = (hrAreaFields.FirstOrDefault(f => f.FieldName == fieldName)?.Title) ?? fieldName;
                        throw HrDataValidationMessage.RequireFieldBeforeField.BadRequestFormat(fieldBefore, field.Title);
                    }

                    filters = filters.Replace(match[i].Value, filterValue);
                }

                Clause filterClause = JsonConvert.DeserializeObject<Clause>(filters);
                if (filterClause != null)
                {


                    try
                    {
                        var parameters = checkData.Data?.Where(d => !d.Value.IsNullOrEmptyObject())?.ToNonCamelCaseDictionary(k => k.Key, v => v.Value);
                        suffix = filterClause.FilterClauseProcess(tableName, tableName, whereCondition, sqlParams, suffix, refValues: parameters);

                    }
                    catch (EvalObjectArgException agrEx)
                    {
                        var fieldBefore = (hrAreaFields.FirstOrDefault(f => f.FieldName == agrEx.ParamName)?.Title) ?? agrEx.ParamName;
                        throw HrDataValidationMessage.RequireFieldBeforeField.BadRequestFormat(fieldBefore, field.Title);
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                }
            }

            var checkExistedReferSql = existSql;
            if (whereCondition.Length > 0)
            {
                existSql += $" AND {whereCondition}";
            }

            var result = await _organizationDBContext.QueryDataTableRaw(existSql, sqlParams.CloneSqlParams());
            bool isExisted = result != null && result.Rows.Count > 0;
            if (!isExisted)
            {

                // Check tồn tại
                result = await _organizationDBContext.QueryDataTableRaw(checkExistedReferSql, sqlParams.CloneSqlParams());
                if (result == null || result.Rows.Count == 0)
                {
                    throw new BadRequestException(HrErrorCode.ReferValueNotFound, new object[] { field.HrAreaTitle, field.Title + ": " + value });
                }
                else
                {
                    throw new BadRequestException(HrErrorCode.ReferValueNotValidFilter, new object[] { field.HrAreaTitle, field.Title + ": " + value });
                }
            }
        }
        private void CheckValue(IEnumerable<ValidateRowModel> rows, IEnumerable<HrValidateField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                foreach (var row in rows)
                {
                    ValidValueAsync(row, field);
                }
            }
        }

        private void ValidValueAsync(ValidateRowModel checkData, HrValidateField field)
        {
            if (checkData.CheckFields != null && !checkData.CheckFields.Contains(field.FieldName))
            {
                return;
            }

            checkData.Data.TryGetStringValue(field.FieldName, out string value);

            if (string.IsNullOrEmpty(value))
                return;

            if (field.FormTypeId.IsSelectForm() || field.IsAutoIncrement || string.IsNullOrEmpty(value))
                return;

            string regex = (field.DataTypeId).GetRegex();
            var strValue = value?.NormalizeAsInternalName();
            if ((field.DataSize > 0 && value.Length > field.DataSize)
                || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(strValue, regex))
                || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(strValue, field.RegularExpression)))
            {
                throw new BadRequestException(HrErrorCode.HrValueInValid, new object[] { value?.JsonSerialize(), field.HrAreaCode, field.Title });
            }
        }


        private async Task<bool> CheckRequireFilter(Clause clause, IEnumerable<ValidateRowModel> rows, IEnumerable<HrValidateField> hrAreaFields, Dictionary<string, Dictionary<object, object>> sfValues, bool not = false)
        {
            bool? isRequire = null;
            if (clause != null)
            {
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;
                    var field = hrAreaFields.First(f => f.FieldName == singleClause.FieldName);
                    // Data check nằm trong thông tin chung và data điều kiện nằm trong thông tin chi tiết
                    var rowValues = rows.Select(r =>
                    r.Data.ContainsKey(field.FieldName) ?
                        (sfValues.ContainsKey(field.FieldName) ?
                            (sfValues[field.FieldName].ContainsKey(r.Data[field.FieldName]) ?
                                sfValues[field.FieldName][r.Data[field.FieldName]]
                                    : null)
                        : r.Data[field.FieldName])
                    : null).ToList();
                    switch (singleClause.Operator)
                    {
                        case EnumOperator.Equal:
                            isRequire = rowValues.Any(v => field.DataTypeId.CompareValue(v, singleClause.Value) == 0);
                            break;
                        case EnumOperator.NotEqual:
                            isRequire = rowValues.Any(v => field.DataTypeId.CompareValue(v, singleClause.Value) != 0);
                            break;
                        case EnumOperator.Contains:
                            isRequire = rowValues.Any(v => v.StringContains(singleClause.Value));
                            break;
                        case EnumOperator.NotContains:
                            isRequire = rowValues.All(v => !v.StringContains(singleClause.Value));
                            break;
                        case EnumOperator.InList:
                            var arrValues = singleClause.Value.ToString().Split(",");
                            isRequire = rowValues.Any(v => v != null && arrValues.Contains(v.ToString()));
                            break;
                        case EnumOperator.IsLeafNode:
                            // Check is leaf node
                            var paramName = $"@{field.RefTableField}";
                            var sql = $"SELECT F_Id FROM {field.RefTableCode} t WHERE {field.RefTableField} = {paramName} AND NOT EXISTS( SELECT F_Id FROM {field.RefTableCode} WHERE ParentId = t.F_Id)";
                            var sqlParams = new List<SqlParameter>() { new SqlParameter(paramName, singleClause.Value) { SqlDbType = field.DataTypeId.GetSqlDataType() } };
                            var result = await _organizationDBContext.QueryDataTableRaw(sql.ToString(), sqlParams.ToArray());
                            isRequire = result != null && result.Rows.Count > 0;
                            break;
                        case EnumOperator.StartsWith:
                            isRequire = rowValues.Any(v => v.StringStartsWith(singleClause.Value));
                            break;
                        case EnumOperator.NotStartsWith:
                            isRequire = rowValues.All(v => !v.StringStartsWith(singleClause.Value));
                            break;
                        case EnumOperator.EndsWith:
                            isRequire = rowValues.Any(v => v.StringEndsWith(singleClause.Value));
                            break;
                        case EnumOperator.NotEndsWith:
                            isRequire = rowValues.All(v => !v.StringEndsWith(singleClause.Value));
                            break;
                        case EnumOperator.IsNull:
                            isRequire = rowValues.Any(v => v == null);
                            break;
                        case EnumOperator.IsEmpty:
                            isRequire = rowValues.Any(v => v != null && string.IsNullOrEmpty(v.ToString()));
                            break;
                        case EnumOperator.IsNullOrEmpty:
                            isRequire = rowValues.Any(v => v == null || string.IsNullOrEmpty(v.ToString()));
                            break;
                        case EnumOperator.Greater:
                            isRequire = rowValues.Any(value => field.DataTypeId.CompareValue(value, singleClause.Value) > 0);
                            break;
                        case EnumOperator.GreaterOrEqual:
                            isRequire = rowValues.Any(value => field.DataTypeId.CompareValue(value, singleClause.Value) >= 0);
                            break;
                        case EnumOperator.LessThan:
                            isRequire = rowValues.Any(value => field.DataTypeId.CompareValue(value, singleClause.Value) < 0);
                            break;
                        case EnumOperator.LessThanOrEqual:
                            isRequire = rowValues.Any(value => field.DataTypeId.CompareValue(value, singleClause.Value) <= 0);
                            break;
                        default:
                            isRequire = true;
                            break;
                    }

                    isRequire = not ? !isRequire : isRequire;
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;
                    bool isNot = not ^ arrClause.Not;
                    bool isOr = (!isNot && arrClause.Condition == EnumLogicOperator.Or) || (isNot && arrClause.Condition == EnumLogicOperator.And);
                    for (int i = 0; i < arrClause.Rules.Count; i++)
                    {
                        bool clauseResult = await CheckRequireFilter(arrClause.Rules.ElementAt(i), rows, hrAreaFields, sfValues, isNot);
                        isRequire = isRequire.HasValue ? isOr ? isRequire.Value || clauseResult : isRequire.Value && clauseResult : clauseResult;
                    }
                }
            }
            return isRequire.Value;
        }


        protected string SelectAreaColumns(int areaId, string alias, List<HrValidateField> areaFields)
        {
            var areaFIdColumn = GetAreaRowFIdAlias(areaId);
            var columns = new List<string>()
            {
                $"{alias}.{HR_BILL_ID_FIELD_IN_AREA}",
                $"{alias}.F_Id AS {areaFIdColumn}",
            };
            foreach (var f in areaFields)
            {
                columns.Add($"{alias}.[{f.FieldName}]");

                if (f.HasRefField)
                {
                    foreach (var refTitle in f.RefTableTitle.Split(',').Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()))
                    {
                        columns.Add($"v{f.FieldName}.[{refTitle}] AS [{f.FieldName}_{refTitle}]");
                    }

                }

            }
            return string.Join(",", columns.ToArray());
        }

        protected string AreaRefJoins(string alias, List<HrValidateField> areaFields)
        {
            var joins = new StringBuilder();
            foreach (var f in areaFields.Where(f => f.HasRefField))
            {
                joins.AppendLine($"LEFT JOIN v{f.RefTableCode} v{f.FieldName} ON {alias}.{f.FieldName} = v{f.FieldName}.{f.RefTableField}");
            }
            return joins.ToString();
        }


        protected async Task<IDictionary<long, IList<NonCamelCaseDictionary>>> GetAreaData(string hrTypeCode, List<HrValidateField> areaFields, IList<long> fIds)
        {
            var firstField = areaFields.First();
            var areaId = firstField.HrAreaId;
            var areaCode = firstField.HrAreaCode;

            var alias = GetAreaAlias(areaId);

            var tableName = OrganizationConstants.GetHrAreaTableName(hrTypeCode, areaCode);


            var sql = $"SELECT {SelectAreaColumns(areaId, alias, areaFields)} " +
                $"FROM {tableName} {alias} JOIN @fIds fId ON {alias}.{HR_BILL_ID_FIELD_IN_AREA} = fId.[Value] " +
                $"{AreaRefJoins(alias, areaFields)} " +
                $"WHERE {alias}.IsDeleted = 0 ";

            var queryParams = new[]
            {
                 fIds.ToSqlParameter("@fIds")
            };

            var data = (await _organizationDBContext.QueryDataTableRaw(sql, queryParams)).ConvertData();
            return data.GroupBy(d => Convert.ToInt64(d[HR_BILL_ID_FIELD_IN_AREA])).ToDictionary(g => g.Key, g => g.ToIList());
        }

        protected string GetAreaAlias(int areaId)
        {
            return $"_a{areaId}";
        }

        protected string GetAreaRowFIdAlias(int areaId)
        {
            return $"_a{areaId}_F_Id";
        }

        protected class ValidateRowModel
        {
            public NonCamelCaseDictionary Data { get; set; }
            public string[] CheckFields { get; set; }

            public ValidateRowModel(NonCamelCaseDictionary Data, string[] CheckFields)
            {
                this.Data = Data;
                this.CheckFields = CheckFields;
            }
        }

        private class DataEqualityComparer : IEqualityComparer<object>
        {
            private readonly EnumDataType dataType;

            public DataEqualityComparer(EnumDataType dataType)
            {
                this.dataType = dataType;
            }

            public new bool Equals(object x, object y)
            {
                return dataType.CompareValue(x, y) == 0;
            }

            public int GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }
        protected sealed class HrBillInforByAreaModel
        {
            public long FId { get; set; }
            public string Code { get; set; }
            public Dictionary<int, List<NonCamelCaseDictionary>> AreaData { get; set; }

        }
    }

}
