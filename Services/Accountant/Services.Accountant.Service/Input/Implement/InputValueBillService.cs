﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErp.Services.Accountant.Service.Input.Implement
{
    public class InputValueBillService : AccoutantBaseService, IInputValueBillService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        private readonly static EnumDataType[] TextDataTypes = new[] { EnumDataType.Email, EnumDataType.PhoneNumber, EnumDataType.Text };

        public InputValueBillService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputValueBillService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingContext, appSetting, mapper)
        {
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<InputTypeListInfo> GetInputTypeListInfo(int inputTypeId)
        {
            var inputTypeInfo = await _accountingContext.InputType.AsNoTracking().FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);

            var data = new InputTypeListInfo()
            {
                Title = inputTypeInfo.Title,
                InputTypeCode = inputTypeInfo.InputTypeCode
            };

            data.Views = await _accountingContext.InputTypeView.AsNoTracking().Where(t => t.InputTypeId == inputTypeId).OrderByDescending(v => v.IsDefault).ProjectTo<InputTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();

            data.ColumnsInList = await (
                from t in _accountingContext.InputType
                join a in _accountingContext.InputArea on t.InputTypeId equals a.InputTypeId
                join f in _accountingContext.InputAreaField on a.InputAreaId equals f.InputAreaId
                where t.InputTypeId == inputTypeId && !a.IsMultiRow
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

            if (data.ColumnsInList.Count == 0)
            {
                var firstArea = await _accountingContext.InputArea.Where(a => a.InputTypeId == inputTypeId).Select(a => new { a.InputAreaId }).FirstOrDefaultAsync();
                if (firstArea != null)
                {
                    data.ColumnsInList = await (
                          from t in _accountingContext.InputType
                          join a in _accountingContext.InputArea on t.InputTypeId equals a.InputTypeId
                          join f in _accountingContext.InputAreaField on a.InputAreaId equals f.InputAreaId
                          where t.InputTypeId == inputTypeId && a.InputAreaId == firstArea.InputAreaId
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
                }
            }

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
                var fieldIndexs = area.Value.Select(f => f.FieldIndex).Distinct().ToList();
                var versions = _accountingContext.InputValueRowVersion.AsQueryable();
                var versionsInNumbers = _accountingContext.InputValueRowVersionNumber.AsQueryable();

                var rParam = Expression.Parameter(typeof(InputValueRowVersion), "r");

                var nParam = Expression.Parameter(typeof(InputValueRowVersionNumber), "n");

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var expressions = new List<Expression>();
                    Expression<Func<string>> keywordLambda = () => keyword;

                    foreach (var fieldIndex in fieldIndexs)
                    {
                        var methodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        var prop = Expression.Property(rParam, GetFieldName(fieldIndex));

                        expressions.Add(Expression.Call(prop, methodInfo, keywordLambda.Body));
                    }

                    if (expressions.Count > 0)
                    {
                        Expression ex = Expression.Constant(false);

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

                    Expression<Func<string>> valueLambda = () => firstValue;

                    Expression<Func<long>> valueLambdaNumber = () => long.Parse(firstValue) * 100000;

                    var lstValues = new List<string>();

                    if (filter.Values != null && filter.Values.Count() > 0)
                    {
                        lstValues = filter.Values.ToList();
                    }
                    Expression<Func<List<string>>> lstValueLambda = () => lstValues;

                    var fieldIndex = area.Value.FirstOrDefault(f => f.InputAreaFieldId == filter.InputAreaFieldId)?.FieldIndex;
                    if (fieldIndex.HasValue && !string.IsNullOrWhiteSpace(firstValue))
                    {
                        var rProp = Expression.Property(rParam, GetFieldName(fieldIndex.Value));

                        var nProp = Expression.Property(nParam, GetFieldName(fieldIndex.Value));

                        switch (filter.Operator)
                        {
                            case EnumOperator.Equal:
                                rowAndExpressions.Add(Expression.Equal(rProp, valueLambda.Body));
                                break;

                            case EnumOperator.NotEqual:
                                rowAndExpressions.Add(Expression.NotEqual(rProp, valueLambda.Body));
                                break;

                            case EnumOperator.Greater:
                                rNumberAndExpressions.Add(Expression.GreaterThan(nProp, valueLambdaNumber.Body));
                                break;

                            case EnumOperator.GreaterOrEqual:
                                rNumberAndExpressions.Add(Expression.GreaterThanOrEqual(nProp, valueLambdaNumber.Body));
                                break;

                            case EnumOperator.LessThan:
                                rNumberAndExpressions.Add(Expression.LessThan(nProp, valueLambdaNumber.Body));
                                break;

                            case EnumOperator.LessThanOrEqual:
                                rNumberAndExpressions.Add(Expression.LessThanOrEqual(nProp, valueLambdaNumber.Body));
                                break;

                            case EnumOperator.Contains:
                                var methodContainInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                rowAndExpressions.Add(Expression.Call(nProp, methodContainInfo, valueLambda.Body));
                                break;

                            case EnumOperator.InList:
                                var methodInList = typeof(List<string>).GetMethod("Contains", new[] { typeof(string) });
                                rNumberAndExpressions.Add(Expression.Call(lstValueLambda.Body, methodInList, nProp));
                                break;

                        }
                    }
                }



                if (rowAndExpressions.Count > 0)
                {
                    Expression ex = Expression.Constant(true);

                    foreach (var expression in rowAndExpressions)
                    {
                        ex = Expression.AndAlso(ex, expression);
                    }

                    versions = versions.Where(Expression.Lambda<Func<InputValueRowVersion, bool>>(ex, rParam));
                }

                if (rNumberAndExpressions.Count > 0)
                {
                    Expression ex = Expression.Constant(true);

                    foreach (var expression in rNumberAndExpressions)
                    {
                        ex = Expression.AndAlso(ex, expression);
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

                    vMapFields.Add("OrderValue", GetFieldName(sortField.FieldIndex));
                    nMapFields.Add("OrderValueInNumber", GetFieldName(sortField.FieldIndex));

                    var sortVersion = versions.DynamicSelectGenerator<InputValueRowVersion, InputValueBillOrderValueModel>(vMapFields);


                    if (TextDataTypes.Contains(sortField.DataTypeId))
                    {
                        query = from b in query
                                join r in _accountingContext.InputValueRow on b.InputValueBillId equals r.InputValueBillId
                                join v in sortVersion on r.LastestInputValueRowVersionId equals v.InputValueRowVersionId
                                where r.InputAreaId == area.Key
                                select new
                                {
                                    b.InputValueBillId,
                                    OrderValue = b.OrderValue == null ? v.OrderValue : b.OrderValue,
                                    b.OrderValueInNumber
                                };
                    }
                    else
                    {
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

            var pagedData = await (asc ? query.OrderBy(b => b.OrderValueInNumber).ThenBy(b => b.OrderValue) : query.OrderByDescending(b => b.OrderValueInNumber).ThenBy(b => b.OrderValue)).Skip((page - 1) * size).Take(size).ToListAsync();

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
                properties.Add(i, type.GetProperty(GetFieldName(i)));
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
                    foreach (var areaGroup in data.GroupBy(d => d.InputAreaId))
                    {
                        foreach (var prop in props.Where(p => p.Column.InputAreaId == areaGroup.Key))
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

        public async Task<ServiceResult<InputValueOuputModel>> GetInputValueBill(int inputTypeId, long inputValueBillId)
        {

            var lstField = _accountingContext.InputAreaField
                .Where(f => f.InputTypeId == inputTypeId)
                .Select(f => new
                {
                    f.InputAreaFieldId,
                    f.InputAreaId,
                    FieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, f.FieldIndex)
                }).ToList();

            InputValueOuputModel inputValueOuputModel = new InputValueOuputModel();

            // Check exist
            var lstAreas = (from row in _accountingContext.InputValueRow
                            where row.InputValueBillId == inputValueBillId
                            join rowVersion in _accountingContext.InputValueRowVersion
                            on new { row.InputValueRowId, inputValueRowVersionId = row.LastestInputValueRowVersionId }
                            equals new { rowVersion.InputValueRowId, inputValueRowVersionId = rowVersion.InputValueRowVersionId }
                            select new
                            {
                                row.InputAreaId,
                                row.InputValueRowId,
                                Data = rowVersion
                            })
                           .ToList()
                           .GroupBy(r => r.InputAreaId);

            foreach (var group in lstAreas)
            {
                var fields = lstField.Where(f => f.InputAreaId == group.Key);

                foreach (var row in group)
                {
                    InputRowOutputModel inputRowOutputModel = new InputRowOutputModel
                    {
                        InputAreaId = group.Key,
                        InputValueRowId = row.InputValueRowId,
                    };
                    foreach (var field in fields)
                    {
                        string value = typeof(InputValueRowVersion).GetProperty(field.FieldName).GetValue(row.Data)?.ToString();
                        inputRowOutputModel.FieldValues.Add(field.InputAreaFieldId, value);
                    }
                    inputValueOuputModel.Rows.Add(inputRowOutputModel);
                }
            }
            return inputValueOuputModel;
        }

        public async Task<ServiceResult<long>> AddInputValueBill(int updatedUserId, int inputTypeId, InputValueInputModel data)
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
            var selectFields = inputAreaFields.Where(f => !f.IsAutoIncrement && AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)f.FormTypeId));

            List<Tuple<InputValueRowInputModel, int[]>> checkRows = data.Rows.Select(r => new Tuple<InputValueRowInputModel, int[]>(r, null)).ToList();

            // Check field required
            var r = CheckRequired(checkRows, requiredFields);
            if (!r.IsSuccess()) return r;

            // Check refer
            r = CheckRefer(ref checkRows, selectFields);
            if (!r.IsSuccess()) return r;

            // Check unique
            r = CheckUnique(checkRows, uniqueFields);
            if (!r.IsSuccess()) return r;

            // Check value
            r = CheckValue(checkRows, inputAreaFields);
            if (!r.IsSuccess()) return r;

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Insert bill
                var inputValueBill = _mapper.Map<InputValueBill>(data);
                inputValueBill.UpdatedByUserId = updatedUserId;
                inputValueBill.CreatedByUserId = updatedUserId;
                await _accountingContext.InputValueBill.AddAsync(inputValueBill);
                await _accountingContext.SaveChangesAsync();

                // Insert rows
                await InsertRows(checkRows.Select(r => r.Item1).ToList(), inputValueBill.InputValueBillId, inputAreaFields);

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

        private async Task InsertRows(ICollection<InputValueRowInputModel> inputValueRows, long inputValueBillId, IEnumerable<InputAreaField> inputAreaFields)
        {
            // Insert row
            foreach (var rowModel in inputValueRows)
            {
                var inputValueRow = _mapper.Map<InputValueRow>(rowModel);
                inputValueRow.InputValueBillId = inputValueBillId;
                await _accountingContext.InputValueRow.AddAsync(inputValueRow);
                await _accountingContext.SaveChangesAsync();

                // Insert row version
                var inputValueRowVersion = CreateRowVersion(rowModel.InputAreaId, inputValueRow.InputValueRowId, rowModel.Values, inputAreaFields);
                await _accountingContext.InputValueRowVersion.AddAsync(inputValueRowVersion);
                await _accountingContext.SaveChangesAsync();

                // Insert row version number
                var inputValueRowVersionNumber = CreateRowVersionNumber(rowModel.InputAreaId, inputValueRowVersion, inputAreaFields);
                await _accountingContext.InputValueRowVersionNumber.AddAsync(inputValueRowVersionNumber);
                await _accountingContext.SaveChangesAsync();

                // Update lasted version
                inputValueRow.LastestInputValueRowVersionId = inputValueRowVersion.InputValueRowVersionId;
                await _accountingContext.SaveChangesAsync();
            }
        }

        private InputValueRowVersion CreateRowVersion(int areaId, long rowId, ICollection<InputValueModel> valueModels, IEnumerable<InputAreaField> fields)
        {
            var inputValueRowVersion = new InputValueRowVersion();
            inputValueRowVersion.InputValueRowId = rowId;

            // Set value AutoIncrement
            var areaFields = fields.Where(f => f.InputAreaId == areaId);
            foreach (var field in areaFields)
            {
                string value;
                bool isRefer = AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.FormTypeId);
                string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, field.FieldIndex);
                if (field.IsAutoIncrement)
                {
                    var rvParam = Expression.Parameter(typeof(InputValueRowVersionNumber), "rvn");
                    MemberExpression memberExp = Expression.PropertyOrField(rvParam, fieldName);
                    var lambdaExp = Expression.Lambda<Func<InputValueRowVersionNumber, long>>(memberExp, rvParam);

                    long maxValue = _accountingContext.InputValueRowVersionNumber
                        .Include(rvn => rvn.InputValueRowVersion)
                        .ThenInclude(rv => rv.InputValueRow)
                        .Where(rvn => rvn.InputValueRowVersion.InputValueRow.InputAreaId == field.InputAreaId)
                        .Where(rvn => rvn.InputValueRowVersionId == rvn.InputValueRowVersion.InputValueRow.LastestInputValueRowVersionId)
                        .Max(lambdaExp);
                    maxValue = (maxValue / AccountantConstants.CONVERT_VALUE_TO_NUMBER_FACTOR) + 1;
                    value = maxValue.ToString();
                }
                else if (isRefer)
                {
                    value = valueModels.Where(v => v.InputAreaFieldId == field.InputAreaFieldId).FirstOrDefault()?.CategoryRowId.ToString() ?? null;
                }
                else
                {
                    value = valueModels.Where(v => v.InputAreaFieldId == field.InputAreaFieldId).FirstOrDefault()?.Value ?? null;
                }

                inputValueRowVersion.GetType().GetProperty(fieldName).SetValue(inputValueRowVersion, value);
            }
            return inputValueRowVersion;
        }

        private InputValueRowVersionNumber CreateRowVersionNumber(int areaId, InputValueRowVersion rowVersion, IEnumerable<InputAreaField> fields)
        {
            var inputValueRowVersionNumber = new InputValueRowVersionNumber
            {
                InputValueRowVersionId = rowVersion.InputValueRowVersionId
            };
            for (int fieldIndx = 0; fieldIndx < AccountantConstants.INPUT_TYPE_FIELD_NUMBER; fieldIndx++)
            {
                string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, fieldIndx);
                long valueInNumber = 0;
                var field = fields.Where(f => f.InputAreaId == areaId && f.FieldIndex == fieldIndx).FirstOrDefault();
                string value = (string)typeof(InputValueRowVersion).GetProperty(fieldName).GetValue(rowVersion);
                if (field != null && !string.IsNullOrEmpty(value))
                {
                    valueInNumber = value.ConvertValueToNumber((EnumDataType)field.DataTypeId);
                }
                typeof(InputValueRowVersionNumber).GetProperty(fieldName).SetValue(inputValueRowVersionNumber, valueInNumber);
            }
            return inputValueRowVersionNumber;
        }

        public async Task<Enum> UpdateInputValueBill(int updatedUserId, int inputTypeId, long inputValueBillId, InputValueInputModel data)
        {
            // Lấy thông tin bill hiện tại
            var currentBill = _accountingContext.InputValueBill
                .Where(b => b.InputValueBillId == inputValueBillId && b.InputTypeId == inputTypeId)
                .FirstOrDefault();
            if (currentBill == null)
            {
                return InputErrorCode.InputValueBillNotFound;
            }

            var inputAreaFields = _accountingContext.InputAreaField
                .Where(f => f.InputTypeId == inputTypeId).ToList();


            // Lấy các row thay đổi
            List<InputValueRow> curRows = _accountingContext.InputValueRow
                .Where(r => r.InputValueBillId == inputValueBillId)
                .Include(r => r.InputValueRowVersion)
                .ToList();
            List<InputValueRowInputModel> futureRows = new List<InputValueRowInputModel>(data.Rows);
            List<(InputValueRowInputModel Future, InputValueRow Current)> updateRows = new List<(InputValueRowInputModel Future, InputValueRow Current)>();

            List<Tuple<InputValueRowInputModel, int[]>> checkRows = new List<Tuple<InputValueRowInputModel, int[]>>();

            foreach (InputValueRowInputModel futureRow in data.Rows)
            {
                InputValueRow curRow = curRows.FirstOrDefault(r => r.InputValueRowId == futureRow.InputValueRowId);
                if (curRow == null)
                {
                    checkRows.Add(new Tuple<InputValueRowInputModel, int[]>(futureRow, null));
                }
                else
                {
                    int[] changeFieldIndexes = CompareRow(curRow, futureRow, inputAreaFields);
                    if (changeFieldIndexes.Length > 0)
                    {
                        updateRows.Add((futureRow, curRow));
                        checkRows.Add(new Tuple<InputValueRowInputModel, int[]>(futureRow, changeFieldIndexes));
                    }
                    curRows.Remove(curRow);
                    futureRows.Remove(futureRow);
                }
            }
            // Get list area id is changed (delete/edit/add)
            int[] changedAreaIds = checkRows.Select(r => r.Item1.InputAreaId).Concat(curRows.Select(cr => cr.InputAreaId)).Distinct().ToArray();

            // Lấy thông tin field
            var changeAreaFields = inputAreaFields
                .Where(f => changedAreaIds.Contains(f.InputAreaId))
                .AsEnumerable();
            var requiredFields = changeAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire);
            var uniqueFields = changeAreaFields.Where(f => !f.IsAutoIncrement && f.IsUnique);
            var selectFields = changeAreaFields.Where(f => !f.IsAutoIncrement && AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)f.FormTypeId));

            // Check field required
            Enum r = CheckRequired(checkRows, requiredFields);
            if (!r.IsSuccess()) return r;

            // Check refer
            r = CheckRefer(ref checkRows, selectFields);
            if (!r.IsSuccess()) return r;

            // Check unique
            r = CheckUnique(checkRows, uniqueFields, curRows.Select(r => r.InputValueRowId).ToArray());
            if (!r.IsSuccess()) return r;

            // Check value
            r = CheckValue(checkRows, changeAreaFields);
            if (!r.IsSuccess()) return r;

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Delete row
                foreach (var deleteRow in curRows)
                {
                    // Xóa rowValueVersion
                    foreach (var deleteRowVersion in deleteRow.InputValueRowVersion)
                    {
                        deleteRowVersion.IsDeleted = true;
                    }
                    deleteRow.IsDeleted = true;
                }

                // Insert new row
                await InsertRows(futureRows, inputValueBillId, changeAreaFields);

                // Update row
                foreach (var (future, current) in updateRows)
                {
                    // Insert row version
                    var inputValueRowVersion = CreateRowVersion(future.InputAreaId, future.InputValueRowId.Value, future.Values, changeAreaFields);
                    await _accountingContext.InputValueRowVersion.AddAsync(inputValueRowVersion);
                    await _accountingContext.SaveChangesAsync();

                    // Insert row version number
                    var inputValueRowVersionNumber = CreateRowVersionNumber(future.InputAreaId, inputValueRowVersion, changeAreaFields);
                    await _accountingContext.InputValueRowVersionNumber.AddAsync(inputValueRowVersionNumber);
                    await _accountingContext.SaveChangesAsync();

                    // Update lasted version
                    current.LastestInputValueRowVersionId = inputValueRowVersion.InputValueRowVersionId;
                    await _accountingContext.SaveChangesAsync();
                }

                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, currentBill.InputValueBillId, $"Cập nhật chứng từ {currentBill.InputValueBillId}", data.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

        private int[] CompareRow(InputValueRow curRow, InputValueRowInputModel futureRow, ICollection<InputAreaField> fields)
        {
            List<int> changeFieldIndexes = new List<int>();
            foreach (var field in fields.Where(f => f.InputAreaId == curRow.InputAreaId))
            {
                bool isRefer = AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.FormTypeId);
                string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, field.FieldIndex);
                string curValue = (string)typeof(InputValueRowVersion)
                    .GetProperty(fieldName)
                    .GetValue(curRow.InputValueRowVersion.First(rv => rv.InputValueRowVersionId == curRow.LastestInputValueRowVersionId));

                var fieldValue = futureRow.Values.Where(v => v.InputAreaFieldId == field.InputAreaFieldId).FirstOrDefault();
                string futureValue = isRefer ? fieldValue.CategoryRowId.ToString() ?? fieldValue.TitleValue : fieldValue.Value;
                if (curValue != futureValue)
                {
                    changeFieldIndexes.Add(field.FieldIndex);
                }
            }
            return changeFieldIndexes.ToArray();
        }

        private Enum CheckRequired(List<Tuple<InputValueRowInputModel, int[]>> data, IEnumerable<InputAreaField> requiredFields)
        {
            foreach (var field in requiredFields)
            {
                bool isRefer = AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.FormTypeId);
                var changeRows = data.Where(r => r.Item1.InputAreaId == field.InputAreaId)
                    .Where(r => r.Item2 == null || r.Item2.Contains(field.FieldIndex));
                if (changeRows.Count() == 0)
                {
                    return InputErrorCode.RequiredFieldIsEmpty;
                }
                foreach (var row in changeRows)
                {
                    var fieldValue = row.Item1.Values.Where(v => v.InputAreaFieldId == field.InputAreaFieldId).FirstOrDefault();
                    if (fieldValue == null
                        || (!isRefer && string.IsNullOrEmpty(fieldValue.Value))
                        || (isRefer && string.IsNullOrEmpty(fieldValue.TitleValue) && !fieldValue.CategoryRowId.HasValue))
                    {
                        return InputErrorCode.RequiredFieldIsEmpty;
                    }
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckUnique(List<Tuple<InputValueRowInputModel, int[]>> data, IEnumerable<InputAreaField> uniqueFields, long[] deleteInputValueRowId = null)
        {
            if (deleteInputValueRowId is null)
            {
                deleteInputValueRowId = Array.Empty<long>();
            }
            // Check unique
            foreach (var field in uniqueFields)
            {
                string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, field.FieldIndex);
                bool isRefer = AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.FormTypeId);
                var changeRows = data.Where(r => r.Item1.InputAreaId == field.InputAreaId)
                   .Where(r => r.Item2 == null || r.Item2.Contains(field.FieldIndex));
                var fieldValues = changeRows
                       .SelectMany(r => r.Item1.Values)
                       .Where(v => v.InputAreaFieldId == field.InputAreaFieldId);
                var rowIds = changeRows.Where(r => r.Item1.InputValueRowId.HasValue && r.Item1.InputValueRowId.Value > 0).Select(r => r.Item1.InputValueRowId.Value).ToList();

                bool isExisted = false;
                if (isRefer)
                {
                    var categoryIds = fieldValues.Select(v => v.CategoryRowId.Value).ToList();
                    // Check unique trong danh sách values thêm mới/sửa
                    if (categoryIds.Count != categoryIds.Distinct().Count())
                    {
                        return InputErrorCode.UniqueValueAlreadyExisted;
                    }

                    // Checkin unique trong db
                    var rvParam = Expression.Parameter(typeof(InputValueRowVersionNumber), "rvn");
                    var property = Expression.Property(rvParam, fieldName);
                    var methodInfo = typeof(List<int>).GetMethod("Contains");

                    Expression expression = Expression.Call(property, methodInfo, Expression.Constant(categoryIds));
                    isExisted = (from verNumber in _accountingContext.InputValueRowVersionNumber
                                 join ver in _accountingContext.InputValueRowVersion on verNumber.InputValueRowVersionId equals ver.InputValueRowVersionId
                                 join row in _accountingContext.InputValueRow
                                 on new { ver.InputValueRowId, inputValueRowVersionId = ver.InputValueRowVersionId }
                                 equals new { row.InputValueRowId, inputValueRowVersionId = row.LastestInputValueRowVersionId }
                                 where row.InputAreaId == field.InputAreaId
                                 && !deleteInputValueRowId.Contains(row.InputValueRowId)
                                 && !rowIds.Contains(row.InputValueRowId)
                                 select verNumber)
                                 .Any(Expression.Lambda<Func<InputValueRowVersionNumber, bool>>(expression, rvParam));
                }
                else
                {
                    var values = fieldValues.Select(v => v.Value).ToList();

                    // Check unique trong danh sách values thêm mới/sửa
                    if (values.Count != values.Distinct().Count())
                    {
                        return InputErrorCode.UniqueValueAlreadyExisted;
                    }

                    // Checkin unique trong db
                    var rvParam = Expression.Parameter(typeof(InputValueRowVersionNumber), "rvn");
                    var property = Expression.Property(rvParam, fieldName);
                    var methodInfo = typeof(List<string>).GetMethod("Contains");

                    Expression expression = Expression.Call(property, methodInfo, Expression.Constant(values));
                    isExisted = (from ver in _accountingContext.InputValueRowVersion
                                 join row in _accountingContext.InputValueRow
                                 on new { ver.InputValueRowId, inputValueRowVersionId = ver.InputValueRowVersionId }
                                 equals new { row.InputValueRowId, inputValueRowVersionId = row.LastestInputValueRowVersionId }
                                 where row.InputAreaId == field.InputAreaId
                                 && !deleteInputValueRowId.Contains(row.InputValueRowId)
                                 && !rowIds.Contains(row.InputValueRowId)
                                 select ver)
                                 .Any(Expression.Lambda<Func<InputValueRowVersion, bool>>(expression, rvParam));
                }

                if (isExisted)
                {
                    return InputErrorCode.UniqueValueAlreadyExisted;
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckRefer(ref List<Tuple<InputValueRowInputModel, int[]>> data, IEnumerable<InputAreaField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, field.FieldIndex);
                var changeRows = data.Where(r => r.Item1.InputAreaId == field.InputAreaId)
                   .Where(r => r.Item2 == null || r.Item2.Contains(field.FieldIndex));
                var fieldValues = changeRows
                       .SelectMany(r => r.Item1.Values)
                       .Where(v => v.InputAreaFieldId == field.InputAreaFieldId);

                bool isExisted = false;
                if (field.ReferenceCategoryFieldId.HasValue)
                {
                    int referenceFieldId = field.ReferenceCategoryTitleFieldId ?? field.ReferenceCategoryFieldId.Value;

                    CategoryField referField = _accountingContext.CategoryField.First(f => f.CategoryFieldId == referenceFieldId);
                    CategoryEntity referCategory = GetReferenceCategory(referField.CategoryId);
                    bool isOutSide = referCategory.IsOutSideData;
                    bool isRef = AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)referField.FormTypeId) && !isOutSide;

                    IQueryable<CategoryRowValue> query;
                    IQueryable<CategoryRowValue> filterQuery;

                    if (isOutSide)
                    {
                        IQueryable<CategoryRow> categoryRows = GetOutSideCategoryRows(referCategory.CategoryId);

                        filterQuery = categoryRows.SelectMany(r => r.CategoryRowValue);
                        query = filterQuery.Where(rv => rv.CategoryFieldId == referenceFieldId);
                    }
                    else
                    {
                        query = from rowValue in _accountingContext.CategoryRowValue
                                join row in _accountingContext.CategoryRow on rowValue.CategoryRowId equals row.CategoryRowId
                                where row.CategoryId == referCategory.CategoryId && rowValue.CategoryFieldId == referenceFieldId
                                select rowValue;

                        filterQuery = from rowValue in _accountingContext.CategoryRowValue
                                      join row in _accountingContext.CategoryRow on rowValue.CategoryRowId equals row.CategoryRowId
                                      where row.CategoryId == referCategory.CategoryId
                                      select rowValue;
                    }

                    if (!string.IsNullOrEmpty(field.Filters))
                    {
                        Clause filters = JsonConvert.DeserializeObject<Clause>(field.Filters);
                        List<int> filterQueryId = FilterClauseProcess(filters, filterQuery).Distinct().ToList();
                        query = query.Where(r => filterQueryId.Contains(r.CategoryRowId));
                    }

                    var categoryIds = fieldValues.Where(v => v.CategoryRowId.HasValue && v.CategoryRowId.Value > 0).Select(v => v.CategoryRowId).ToList();
                    var inputValues = fieldValues.Where(v => !v.CategoryRowId.HasValue || v.CategoryRowId.Value == 0).ToList();


                    if (categoryIds.Count > 0)
                    {
                        isExisted = categoryIds.All(ci => query.Any(rv => rv.CategoryRowId == ci));
                    }

                    if (isExisted && inputValues.Count > 0)
                    {
                        foreach (var inputValue in inputValues)
                        {
                            int referRowId = query.FirstOrDefault(rv => rv.Value == inputValue.TitleValue).CategoryRowId;

                            if (referRowId <= 0)
                            {
                                isExisted = false;
                                break;
                            }
                            else
                            {
                                inputValue.CategoryRowId = referRowId;
                            }
                        }
                    }
                    if (!isExisted)
                    {
                        return InputErrorCode.ReferValueNotFound;
                    }

                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckValue(List<Tuple<InputValueRowInputModel, int[]>> data, IEnumerable<InputAreaField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, field.FieldIndex);
                var changeRows = data.Where(r => r.Item1.InputAreaId == field.InputAreaId)
                    .Where(r => r.Item2 == null || r.Item2.Contains(field.FieldIndex));
                var fieldValues = changeRows
                       .SelectMany(r => r.Item1.Values)
                       .Where(v => v.InputAreaFieldId == field.InputAreaFieldId);

                foreach (var value in fieldValues)
                {
                    if ((AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.FormTypeId)) || field.IsAutoIncrement || string.IsNullOrEmpty(value.Value))
                    {
                        continue;
                    }

                    var r = CheckValue(value.Value, field);
                    if (!r.IsSuccess())
                    {
                        return r;
                    }
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckValue(string value, InputAreaField field)
        {
            if ((field.DataSize > 0 && value.Length > field.DataSize)
                || !string.IsNullOrEmpty(field.DataType.RegularExpression) && !Regex.IsMatch(value, field.DataType.RegularExpression)
                || !string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(value, field.RegularExpression))
            {
                return InputErrorCode.InputValueInValid;
            }

            return GeneralCode.Success;
        }

        public async Task<Enum> DeleteInputValueBill(int updatedUserId, int inputTypeId, long inputValueBillId)
        {
            // Lấy thông tin bill
            var inputValueBill = _accountingContext.InputValueBill.FirstOrDefault(i => i.InputTypeId == inputTypeId && i.InputValueBillId == inputValueBillId);
            if (inputValueBill == null)
            {
                return InputErrorCode.InputValueBillNotFound;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Delete bill
                inputValueBill.IsDeleted = true;
                inputValueBill.UpdatedByUserId = updatedUserId;

                // Delete row
                var inputValueRows = _accountingContext.InputValueRow.Where(r => r.InputValueBillId == inputValueBillId).ToList();
                foreach (var row in inputValueRows)
                {
                    row.IsDeleted = true;
                    row.UpdatedByUserId = updatedUserId;

                    // Delete row version
                    var inputValueRowVersions = _accountingContext.InputValueRowVersion.Where(rv => rv.InputValueRowId == row.InputValueRowId).ToList();
                    foreach (var rowVersion in inputValueRowVersions)
                    {
                        rowVersion.IsDeleted = true;
                        rowVersion.UpdatedByUserId = updatedUserId;
                    }
                }

                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputValueBillId, $"Xóa chứng từ {inputValueBillId}", inputValueBill.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Delete");
                return GeneralCode.InternalError;
            }
        }


        private string GetFieldName(int fieldIndex)
        {
            return string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, fieldIndex);
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
