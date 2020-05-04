using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountant.Model.Category;
using VErp.Services.Accountant.Model.Input;

namespace VErp.Services.Accountant.Service.Input.Implement
{

    public class InputValueBillService : InputBaseService, IInputValueBillService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        public InputValueBillService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputValueBillService> logger
            , IActivityLogService activityLogService
             , IMapper mapper
            ) : base(accountingContext)
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }


        public async Task<InputTypeListInfo> GetInputTypeListInfo(int inputTypeId)
        {
            var inputTypeInfo = await _accountingContext.InputType.AsNoTracking().FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);

            var data = new InputTypeListInfo()
            {
                Title = inputTypeInfo.Title,
                InputTypeCode = inputTypeInfo.InputTypeCode
            };

            data.ColumnsInList = await (
                from t in _accountingContext.InputType
                join a in _accountingContext.InputArea on t.InputTypeId equals a.InputTypeId
                join f in _accountingContext.InputAreaField on a.InputAreaId equals f.InputAreaId
                where t.InputTypeId == inputTypeId// && !a.IsMultiRow
                select new InputTypeListColumn
                {
                    InputAreaId = a.InputAreaId,
                    FieldIndex = f.FieldIndex,
                    InputAreaFieldId = f.InputAreaFieldId,
                    FieldName = f.FieldName,
                    FieldTitle = f.Title,
                    IsMultiRow = a.IsMultiRow,
                    DataTypeId = (EnumDataType)f.DataTypeId
                })
                .ToListAsync();

            return data;
        }



        public async Task<PageData<InputValueBillListOutput>> GetInputValueBills(int inputTypeId, string keyword, IList<InputValueFilterModel> fieldFilters, int orderByFieldId, bool asc, int page, int size)
        {
            var typeListInfo = await GetInputTypeListInfo(inputTypeId);

            var areas = typeListInfo.ColumnsInList
                .GroupBy(c => c.InputAreaId)
                .ToDictionary(c => c.Key, c => c.ToList());

            var query = from b in _accountingContext.InputValueBill
                        select new
                        {
                            b.InputValueBillId,
                            OrderValue = "",
                            OrderValueInNumber = 0L,
                        };

            foreach (var area in areas)
            {
                var fieldIndexs = area.Value.Select(f => f.FieldIndex).ToList();
                var versions = _accountingContext.InputValueRowVersion.AsQueryable();
                var versionsInNumbers = _accountingContext.InputValueRowVersionNumber.AsQueryable();

                var rParam = Expression.Parameter(typeof(InputValueRowVersion), "r");

                var nParam = Expression.Parameter(typeof(InputValueRowVersionNumber), "n");

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var expressions = new List<Expression>();

                    foreach (var fieldIndex in fieldIndexs)
                    {
                        var methodInfo = typeof(string).GetMethod("Contains");
                        var prop = Expression.Property(rParam, "Filed" + fieldIndex);

                        expressions.Add(Expression.Call(prop, methodInfo, Expression.Constant(keyword)));
                    }


                    if (expressions.Count > 0)
                    {
                        Expression ex = Expression.Constant(true);

                        foreach (var expression in expressions)
                        {
                            ex = Expression.OrElse(ex, expression);
                        }

                        versions = versions.Where(Expression.Lambda<Func<InputValueRowVersion, bool>>(ex, rParam));
                    }
                }


                var rowAndExpressions = new List<Expression>();
                var rNumberAndExpressions = new List<Expression>();

                foreach (var filter in fieldFilters)
                {
                    var firstValue = filter.Values?.FirstOrDefault();
                    var fieldIndex = area.Value.FirstOrDefault(f => f.InputAreaFieldId == filter.InputAreaFieldId)?.FieldIndex;
                    if (fieldIndex.HasValue && !string.IsNullOrWhiteSpace(firstValue))
                    {
                        var rProp = Expression.Property(rParam, "Field" + fieldIndex);

                        var nProp = Expression.Property(nParam, "Field" + fieldIndex);

                        switch (filter.Operator)
                        {
                            case EnumOperator.Equal:
                                rowAndExpressions.Add(Expression.Equal(rProp, Expression.Constant(firstValue)));
                                break;

                            case EnumOperator.NotEqual:
                                rowAndExpressions.Add(Expression.NotEqual(rProp, Expression.Constant(firstValue)));
                                break;

                            case EnumOperator.Greater:
                                rNumberAndExpressions.Add(Expression.NotEqual(nProp, Expression.Constant(firstValue)));
                                break;

                        }
                    }
                }



                if (rowAndExpressions.Count > 0)
                {
                    Expression ex = Expression.Constant(true);

                    foreach (var expression in rowAndExpressions)
                    {
                        ex = Expression.OrElse(ex, expression);
                    }

                    versions = versions.Where(Expression.Lambda<Func<InputValueRowVersion, bool>>(ex, rParam));
                }

                if (rNumberAndExpressions.Count > 0)
                {
                    Expression ex = Expression.Constant(true);

                    foreach (var expression in rNumberAndExpressions)
                    {
                        ex = Expression.OrElse(ex, expression);
                    }

                    versionsInNumbers = versionsInNumbers.Where(Expression.Lambda<Func<InputValueRowVersionNumber, bool>>(ex, nParam));
                }


              

                var sortField = area.Value.FirstOrDefault(f => f.InputAreaFieldId == orderByFieldId);
                if (sortField != null)
                {
                    var vMapFields = new Dictionary<string, string>();
                    vMapFields.Add("InputValueRowVersionId", "InputValueRowVersionId");

                    var nMapFields = new Dictionary<string, string>();
                    nMapFields.Add("InputValueRowVersionId", "InputValueRowVersionId");

                    vMapFields.Add("OrderValue", "Field" + sortField.FieldIndex);
                    nMapFields.Add("OrderValueInNumber", "Field" + sortField.FieldIndex);

                    var sortVersion = versions.DynamicSelectGenerator<InputValueRowVersion, InputValueBillOrderValueModel>(vMapFields);

                    var sortVersionInNumbers = versionsInNumbers.DynamicSelectGenerator<InputValueRowVersionNumber, InputValueBillOrderValueInNumberModel>(nMapFields);

                    query = from b in query
                            join r in _accountingContext.InputValueRow on b.InputValueBillId equals r.InputValueBillId
                            join v in sortVersion on r.LastestInputValueRowVersionId equals v.InputValueRowVersionId
                            join n in sortVersionInNumbers on r.LastestInputValueRowVersionId equals n.InputValueRowVersionId
                            where r.InputAreaId == area.Key
                            select new
                            {
                                b.InputValueBillId,
                                OrderValue = b.OrderValue == null ? v.OrderValue : b.OrderValue,
                                OrderValueInNumber = b.OrderValueInNumber == 0 ? n.OrderValueInNumber : b.OrderValueInNumber,
                            };
                }
                else
                {
                    query = from b in query
                            join r in _accountingContext.InputValueRow on b.InputValueBillId equals r.InputValueBillId
                            join v in versions on r.LastestInputValueRowVersionId equals v.InputValueRowVersionId
                            join n in versionsInNumbers on r.LastestInputValueRowVersionId equals n.InputValueRowVersionId
                            where r.InputAreaId == area.Key
                            select new
                            {
                                b.InputValueBillId,
                                b.OrderValue,
                                b.OrderValueInNumber,
                            };
                }



                //query = from b in query
                //        join r in _accountingContext.InputValueRow on b.InputValueBillId equals r.InputValueBillId
                //        join v in versions on r.LastestInputValueRowVersionId equals v.InputValueRowVersionId
                //        join n in versionsInNumbers on r.LastestInputValueRowVersionId equals n.InputValueRowVersionId
                //        where r.InputAreaId == area.InputAreaId
                //        select new
                //        {
                //            b.InputValueBillId,
                //            OrderValue = ,
                //            OrderValueInNumber = 0,
                //        };

               

            }

            query = query.Distinct();

            var total = await query.CountAsync();

            var pagedData = await (asc ? query.OrderBy(b => b.OrderValueInNumber) : query.OrderByDescending(b => b.OrderValueInNumber)).Skip((page - 1) * size).Take(size).ToListAsync();

            var billIds = pagedData.Select(b => b.InputValueBillId).ToList();
            var rowData = (await (
                   from r in _accountingContext.InputValueRow
                   join v in _accountingContext.InputValueRowVersion on r.LastestInputValueRowVersionId equals v.InputValueRowVersionId
                   where billIds.Contains(r.InputValueBillId)
                   select new
                   {
                       r.InputValueBillId,
                       r.InputAreaId,
                       Row = v
                   })
                   .ToListAsync()
                   )
                   .GroupBy(r => r.InputValueBillId)
                   .ToDictionary(r => r.Key, r => r.ToList());


            var type = typeof(InputValueRowVersion);
            var properties = new Dictionary<int, PropertyInfo>();

            for (var i = 0; i <= 20; i++)
            {
                properties.Add(i, type.GetProperty("Field" + i));
            }


            var props = (
                from c in typeListInfo.ColumnsInList
                join p in properties on c.FieldIndex equals p.Key
                select new
                {
                    Column = c,
                    Property = p.Value
                })
                .ToList();

            var lst = new List<InputValueBillListOutput>();

            foreach (var bill in pagedData)
            {
                var row = new InputValueBillListOutput();
                row.InputValueBillId = bill.InputValueBillId;

                row.FieldValues = new Dictionary<int, string>();
                if (rowData.TryGetValue(bill.InputValueBillId, out var data))
                {
                    foreach(var areaGroup in data.GroupBy(d => d.InputAreaId))
                    {
                        foreach(var prop in props.Where(p => p.Column.InputAreaId == areaGroup.Key))
                        {
                            var value = prop.Property.GetValue(areaGroup.First().Row)?.ToString();
                            row.FieldValues.Add(prop.Column.InputAreaFieldId, value);
                        }
                    }
                }


                //row.AreaValues = data.GroupBy(d => d.InputAreaId)
                //    .ToDictionary(
                //    d => d.Key,
                //    d => props
                //        .Where(p => p.Column.InputAreaId == d.Key)
                //        .Select(p => new
                //        {
                //            p.Column.InputAreaFieldId,
                //            Value = p.Property.GetValue(d.First().Row)?.ToString()
                //        })
                //        .ToDictionary(p => p.InputAreaFieldId, p => p.Value)
                //    );

                lst.Add(row);
            }

            return (lst, total);
        }


        public async Task<PageData<InputValueBillOutputModel>> GetInputValueBills(int inputTypeId, string keyword, int page, int size)
        {
            var lst = new List<InputValueBillOutputModel>();

            // TODO

            return (lst, 0);
        }



        public async Task<ServiceResult<InputValueBillOutputModel>> GetInputValueBill(int inputTypeId, long inputValueBillId)
        {
            // Check exist
            var inputValueBill = await _accountingContext.InputValueBill
                .Include(b => b.InputValueRows)
                .ThenInclude(r => r.InputValueRowVersions)
                .FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId && i.InputValueBillId == inputValueBillId);
            if (inputValueBill == null)
            {
                return InputErrorCode.InputValueBillNotFound;
            }

            var output = _mapper.Map<InputValueBillOutputModel>(inputValueBill);

            return output;
        }

        public async Task<ServiceResult<long>> AddInputValueBill(int updatedUserId, int inputTypeId, InputValueBillInputModel data)
        {
            // Validate
            var inputType = _accountingContext.InputType.FirstOrDefault(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }

            // Lấy thông tin field
            var inputAreaFields = _accountingContext.InputAreaField
                .Include(f => f.DataType)
                .Where(f => f.InputTypeId == inputTypeId).AsEnumerable();

            var requiredFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire);
            var uniqueFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsUnique);
            var selectFields = inputAreaFields.Where(f => !f.IsAutoIncrement && (f.FormTypeId == (int)EnumFormType.SearchTable || f.FormTypeId == (int)EnumFormType.Select));


            // Check field required
            var r = CheckRequired(data, requiredFields);
            if (!r.IsSuccess()) return r;

            // Check unique
            r = CheckUnique(data, uniqueFields);
            if (!r.IsSuccess()) return r;

            // Check refer
            r = CheckRefer(data, selectFields);
            if (!r.IsSuccess()) return r;

            // Check value
            r = CheckValue(data, inputAreaFields);
            if (!r.IsSuccess()) return r;

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Insert bill
                    var inputValueBill = _mapper.Map<InputValueBill>(data);
                    inputValueBill.UpdatedByUserId = updatedUserId;
                    inputValueBill.CreatedByUserId = updatedUserId;
                    await _accountingContext.InputValueBill.AddAsync(inputValueBill);
                    await _accountingContext.SaveChangesAsync();

                    // Insert row
                    foreach (var rowModel in data.InputValueRows)
                    {

                        var inputValueRow = _mapper.Map<InputValueRow>(rowModel);
                        inputValueRow.UpdatedByUserId = updatedUserId;
                        inputValueRow.CreatedByUserId = updatedUserId;
                        inputValueRow.InputValueBillId = inputValueBill.InputValueBillId;
                        await _accountingContext.InputValueRow.AddAsync(inputValueRow);
                        await _accountingContext.SaveChangesAsync();

                        // Insert row version
                        var inputValueRowVersion = _mapper.Map<InputValueRowVersion>(rowModel.InputValueRowVersion);
                        inputValueRowVersion.UpdatedByUserId = updatedUserId;
                        inputValueRowVersion.CreatedByUserId = updatedUserId;
                        inputValueRowVersion.InputValueRowId = inputValueRow.InputValueRowId;

                        // Set value AutoIncrement
                        var autoIncrementFields = inputAreaFields.Where(f => f.IsAutoIncrement && f.InputAreaId == rowModel.InputAreaId);
                        foreach (var autoIncrementField in autoIncrementFields)
                        {
                            string fieldName = string.Format(StringFormats.INPUT_TYPE_FIELDNAME_FORMAT, autoIncrementField.FieldIndex);
                            long maxValue = _accountingContext.InputValueRowVersionNumber
                                .Include(rvn => rvn.InputValueRowVersion)
                                .ThenInclude(rv => rv.InputValueRow)
                                .Where(rvn => rvn.InputValueRowVersion.InputValueRow.InputAreaId == autoIncrementField.InputAreaId)
                                .Where(rvn => rvn.InputValueRowVersionId == rvn.InputValueRowVersion.InputValueRow.LastestInputValueRowVersionId)
                                .Max(rvn => (long)rvn.GetType().GetProperty(fieldName).GetValue(rvn));
                            maxValue = (maxValue / Numbers.CONVERT_VALUE_TO_NUMBER_FACTOR) + 1;
                            string value = maxValue.ToString();
                            inputValueRowVersion.GetType().GetProperty(fieldName).SetValue(inputValueRowVersion, value);
                        }

                        await _accountingContext.InputValueRowVersion.AddAsync(inputValueRowVersion);
                        await _accountingContext.SaveChangesAsync();

                        // Insert row version number
                        var inputValueRowVersionNumber = new InputValueRowVersionNumber
                        {
                            InputValueRowVersionId = inputValueRowVersion.InputValueRowVersionId
                        };
                        for (int fieldIndx = 0; fieldIndx < Numbers.INPUT_TYPE_FIELD_NUMBER; fieldIndx++)
                        {
                            string fieldName = string.Format(StringFormats.INPUT_TYPE_FIELDNAME_FORMAT, fieldIndx);
                            long valueInNumber = 0;
                            var field = inputAreaFields.Where(f => f.InputAreaId == rowModel.InputAreaId && f.FieldIndex == fieldIndx).FirstOrDefault();
                            string value = (string)typeof(InputValueRowVersion).GetProperty(fieldName).GetValue(inputValueRowVersion);
                            if (field != null && !string.IsNullOrEmpty(value))
                            {
                                valueInNumber = value.ConvertValueToNumber((EnumDataType)field.DataTypeId);
                            }
                            typeof(InputValueRowVersionNumber).GetProperty(fieldName).SetValue(inputValueRowVersionNumber, valueInNumber);
                        }
                        await _accountingContext.InputValueRowVersionNumber.AddAsync(inputValueRowVersionNumber);
                        await _accountingContext.SaveChangesAsync();

                        // Update lasted version
                        inputValueRow.LastestInputValueRowVersionId = inputValueRowVersion.InputValueRowVersionId;
                        await _accountingContext.SaveChangesAsync();
                    }

                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.InputType, inputValueBill.InputValueBillId, $"Thêm chứng từ cho loại chứng từ {inputType.Title}", data.JsonSerialize());
                    return inputValueBill.InputValueBillId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

        private Enum CheckRequired(InputValueBillInputModel data, IEnumerable<InputAreaField> requiredFields)
        {
            if (requiredFields.Count() > 0)
            {
                foreach (var field in requiredFields)
                {
                    string fieldName = string.Format(StringFormats.INPUT_TYPE_FIELDNAME_FORMAT, field.FieldIndex);

                    var valueRows = data.InputValueRows.Where(r => r.InputAreaId == field.InputAreaId).ToList();
                    if (valueRows.Count == 0)
                    {
                        return InputErrorCode.RequiredFieldIsEmpty;
                    }

                    foreach (var valueRow in valueRows)
                    {
                        if (string.IsNullOrEmpty((string)valueRow.InputValueRowVersion.GetType().GetProperty(fieldName).GetValue(valueRow.InputValueRowVersion)))
                        {
                            return InputErrorCode.RequiredFieldIsEmpty;
                        }
                    }
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckUnique(InputValueBillInputModel data, IEnumerable<InputAreaField> uniqueFields, int? inputValueBillId = null)
        {
            // Check unique
            if (uniqueFields.Count() > 0)
            {
                foreach (var field in uniqueFields)
                {
                    string fieldName = string.Format(StringFormats.INPUT_TYPE_FIELDNAME_FORMAT, field.FieldIndex);
                    var values = data.InputValueRows
                        .Where(rv => rv.InputAreaId == field.InputAreaId)
                        .Select(r => (string)r.InputValueRowVersion.GetType().GetProperty(fieldName).GetValue(r.InputValueRowVersion))
                        .ToList();

                    // Check unique trong danh sách values thêm mới/sửa
                    if (values.Count != values.Distinct().Count())
                    {
                        return InputErrorCode.UniqueValueAlreadyExisted;
                    }

                    // Checkin unique trong db
                    foreach (var value in values)
                    {
                        bool isExisted = _accountingContext.InputValueRowVersion
                            .Include(rv => rv.InputValueRow)
                            .Where(rv => inputValueBillId.HasValue ? rv.InputValueRow.InputValueBillId != inputValueBillId : true)
                            .Where(rv => rv.InputValueRow.InputAreaId == field.InputAreaId)
                            .Where(rv => rv.InputValueRowVersionId == rv.InputValueRow.LastestInputValueRowVersionId)
                            .Any(rv => (string)rv.GetType().GetProperty(fieldName).GetValue(rv) == value);

                        if (isExisted)
                        {
                            return InputErrorCode.UniqueValueAlreadyExisted;
                        }
                    }
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckRefer(InputValueBillInputModel data, IEnumerable<InputAreaField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                string fieldName = string.Format(StringFormats.INPUT_TYPE_FIELDNAME_FORMAT, field.FieldIndex);
                var values = data.InputValueRows
                    .Where(rv => rv.InputAreaId == field.InputAreaId)
                    .Select(r => (string)r.InputValueRowVersion.GetType().GetProperty(fieldName).GetValue(r.InputValueRowVersion))
                    .ToList();
                foreach (var value in values)
                {
                    int referValueId = 0;
                    if (field.ReferenceCategoryFieldId.HasValue)
                    {
                        CategoryField referField = _accountingContext.CategoryField.First(f => f.CategoryFieldId == field.ReferenceCategoryFieldId.Value);
                        bool isRef = ((EnumFormType)referField.FormTypeId).IsRef();
                        IQueryable<CategoryRow> tempQuery = _accountingContext.CategoryRow
                            .Where(r => r.CategoryId == referField.CategoryId)
                            .Include(r => r.CategoryRowValues)
                            .ThenInclude(rv => rv.SourceCategoryRowValue)
                            .Include(r => r.CategoryRowValues)
                            .ThenInclude(rv => rv.CategoryField);

                        //if (!string.IsNullOrEmpty(field.Filters))
                        //{
                        //    FilterModel[] filters = JsonConvert.DeserializeObject<FilterModel[]>(field.Filters);
                        //    FillterProcess(ref tempQuery, filters);
                        //}

                        referValueId = tempQuery
                            .Select(r => r.CategoryRowValues
                            .FirstOrDefault(rv => rv.CategoryFieldId == field.ReferenceCategoryFieldId.Value && (isRef ? rv.SourceCategoryRowValue.Value == value : rv.Value == value)).CategoryRowValueId)
                            .FirstOrDefault();
                    }
                    if (referValueId <= 0)
                    {
                        return InputErrorCode.ReferValueNotFound;
                    }
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckValue(InputValueBillInputModel data, IEnumerable<InputAreaField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                string fieldName = string.Format(StringFormats.INPUT_TYPE_FIELDNAME_FORMAT, field.FieldIndex);
                var values = data.InputValueRows
                    .Where(rv => rv.InputAreaId == field.InputAreaId)
                    .Select(r => (string)r.InputValueRowVersion.GetType().GetProperty(fieldName).GetValue(r.InputValueRowVersion))
                    .ToList();
                foreach (string value in values)
                {
                    if (((EnumFormType)field.FormTypeId).IsRef() || field.IsAutoIncrement || string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    var r = CheckValue(value, field);
                    if (!r.IsSuccess())
                    {
                        return r;
                    }
                }
            }
            return GeneralCode.Success;
        }
        protected private Enum CheckValue(string value, InputAreaField field)
        {
            if ((field.DataSize > 0 && value.Length > field.DataSize)
                || !string.IsNullOrEmpty(field.DataType.RegularExpression) && !Regex.IsMatch(value, field.DataType.RegularExpression)
                || !string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(value, field.RegularExpression))
            {
                return InputErrorCode.InputValueInValid;
            }

            return GeneralCode.Success;
        }
    }


    public class InputValueBillOrderValueModel
    {
        public long InputValueRowVersionId { get; set; }
        public string OrderValue { get; set; }
    }

    public class InputValueBillOrderValueInNumberModel
    {
        public long InputValueRowVersionId { get; set; }
        public long OrderValueInNumber { get; set; }
    }
}
