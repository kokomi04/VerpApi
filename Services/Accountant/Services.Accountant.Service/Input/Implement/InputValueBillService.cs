using AutoMapper;
using AutoMapper.QueryableExtensions;
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
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
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

            data.ColumnsInList = await GetInputTypeListColumn(inputTypeId);

            return data;
        }

        private async Task<IList<InputTypeListColumn>> GetInputTypeListColumn(int inputTypeId)
        {
            var columnList = await (
                from t in _accountingContext.InputType
                join a in _accountingContext.InputArea on t.InputTypeId equals a.InputTypeId
                join af in _accountingContext.InputAreaField on a.InputAreaId equals af.InputAreaId
                join f in _accountingContext.InputField on af.InputFieldId equals f.InputFieldId
                join rtf in _accountingContext.CategoryField on f.ReferenceCategoryTitleFieldId equals rtf.CategoryFieldId into rtfs
                from rtf in rtfs.DefaultIfEmpty()
                where t.InputTypeId == inputTypeId && !a.IsMultiRow
                orderby a.SortOrder, af.SortOrder
                select new InputTypeListColumn
                {
                    InputAreaId = a.InputAreaId,
                    FieldIndex = f.FieldIndex,
                    InputAreaFieldId = af.InputAreaFieldId,
                    FieldName = f.FieldName,
                    FieldTitle = af.Title,
                    IsMultiRow = a.IsMultiRow,
                    ReferenceCategoryFieldId = f.ReferenceCategoryFieldId,
                    ReferenceCategoryTitleFieldId = f.ReferenceCategoryTitleFieldId,
                    ReferenceCategoryTitleFieldName = rtf.CategoryFieldName,
                    DataTypeId = (EnumDataType)f.DataTypeId
                })
                .ToListAsync();

            if (columnList.Count == 0)
            {
                var firstArea = await _accountingContext.InputArea.Where(a => a.InputTypeId == inputTypeId).Select(a => new { a.InputAreaId }).FirstOrDefaultAsync();
                if (firstArea != null)
                {
                    columnList = await (
                          from t in _accountingContext.InputType
                          join a in _accountingContext.InputArea on t.InputTypeId equals a.InputTypeId
                          join af in _accountingContext.InputAreaField on a.InputAreaId equals af.InputAreaId
                          join f in _accountingContext.InputField on af.InputFieldId equals f.InputFieldId
                          join rtf in _accountingContext.CategoryField on f.ReferenceCategoryTitleFieldId equals rtf.CategoryFieldId into rtfs
                          from rtf in rtfs.DefaultIfEmpty()
                          where t.InputTypeId == inputTypeId && a.InputAreaId == firstArea.InputAreaId
                          orderby a.SortOrder, f.SortOrder
                          select new InputTypeListColumn
                          {
                              InputAreaId = a.InputAreaId,
                              FieldIndex = f.FieldIndex,
                              InputAreaFieldId = af.InputAreaFieldId,
                              FieldName = f.FieldName,
                              FieldTitle = f.Title,
                              IsMultiRow = a.IsMultiRow,
                              ReferenceCategoryFieldId = f.ReferenceCategoryFieldId,
                              ReferenceCategoryTitleFieldId = f.ReferenceCategoryTitleFieldId,
                              ReferenceCategoryTitleFieldName = rtf.CategoryFieldName,
                              DataTypeId = (EnumDataType)f.DataTypeId
                          })
                          .ToListAsync();
                }
            }

            return columnList;
        }

        public async Task<PageData<InputValueBillListOutput>> GetInputValueBills(int inputTypeId, string keyword, IList<InputValueFilterModel> fieldFilters, string orderBy, bool asc, int page, int size)
        {
            var fields = await _accountingContext.InputAreaField
                .Include(af => af.InputField)
                .Include(af => af.InputArea)
                .Where(a => a.InputTypeId == inputTypeId && a.InputArea.IsMultiRow == false)
                .Select(f => new { f.InputAreaId, f.InputAreaFieldId, f.InputField.FieldIndex, f.InputField.FieldName, f.InputField.DataTypeId }).ToListAsync();

            //var fieldsByAreas = fields.GroupBy(f => f.InputAreaId).ToDictionary(f => f.Key, f => f.ToList());

            var query = from b in _accountingContext.InputValueBill
                        where b.InputTypeId == inputTypeId
                        select new
                        {
                            b.InputValueBillId,
                            OrderValue = "",
                            OrderValueInNumber = (long?)0L,
                            b.CreatedDatetimeUtc
                        };

            //var filterAreaIds = (
            //    from filter in fieldFilters
            //    join f in fields on filter.InputAreaFieldId equals f.InputAreaFieldId
            //    select f.InputAreaId
            //   ).ToList();

            //foreach (var area in fieldsByAreas.Where(a => filterAreaIds.Contains(a.Key)))
            //{
            var fieldIndexs = fields.Select(f => f.FieldIndex).Distinct().ToList();

            var versions = from r in _accountingContext.InputValueRow
                           join v in _accountingContext.InputValueRowVersion on r.LastestInputValueRowVersionId equals v.InputValueRowVersionId
                           join b in _accountingContext.InputValueBill on r.InputValueBillId equals b.InputValueBillId
                           where b.InputTypeId == inputTypeId && r.IsMultiRow == false
                           select new InputValueRowVersionTextEntity
                           {
                               InputValueBillId = r.InputValueBillId,
                               VersionText = v
                           };

            var versionsInNumbers = from r in _accountingContext.InputValueRow
                                    join n in _accountingContext.InputValueRowVersionNumber on r.LastestInputValueRowVersionId equals n.InputValueRowVersionId
                                    join b in _accountingContext.InputValueBill on r.InputValueBillId equals b.InputValueBillId
                                    where b.InputTypeId == inputTypeId && r.IsMultiRow == false
                                    select new InputValueRowVersionInNumberEntity
                                    {
                                        InputValueBillId = r.InputValueBillId,
                                        VersionNumber = n
                                    };

            var tObj = Expression.Parameter(typeof(InputValueRowVersionTextEntity), "t");

            var nObj = Expression.Parameter(typeof(InputValueRowVersionInNumberEntity), "n");

            var tParam = Expression.Property(tObj, nameof(InputValueRowVersionTextEntity.VersionText));

            var nParam = Expression.Property(nObj, nameof(InputValueRowVersionInNumberEntity.VersionNumber));

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var expressions = new List<Expression>();
                Expression<Func<string>> keywordLambda = () => keyword;

                foreach (var fieldIndex in fieldIndexs)
                {
                    var methodInfo = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
                    var prop = Expression.Property(tParam, GetFieldName(fieldIndex));

                    expressions.Add(Expression.Call(prop, methodInfo, keywordLambda.Body));
                }

                if (expressions.Count > 0)
                {
                    Expression ex = Expression.Constant(false);

                    foreach (var expression in expressions)
                    {
                        ex = Expression.OrElse(ex, expression);
                    }

                    versions = versions.Where(Expression.Lambda<Func<InputValueRowVersionTextEntity, bool>>(ex, tObj));
                }
            }


            var rowAndExpressions = new List<Expression>();
            var rNumberAndExpressions = new List<Expression>();

            foreach (var filter in fieldFilters)
            {
                var firstValue = filter.Values?.FirstOrDefault();

                Expression<Func<string>> valueLambda = () => firstValue;

                Expression<Func<long>> valueLambdaNumber = () => (long)(double.Parse(firstValue) * AccountantConstants.CONVERT_VALUE_TO_NUMBER_FACTOR);

                var lstValues = new List<string>();

                if (filter.Values != null && filter.Values.Count() > 0)
                {
                    lstValues = filter.Values.ToList();
                }
                Expression<Func<List<string>>> lstValueLambda = () => lstValues;

                var fieldIndex = fields.FirstOrDefault(f => f.InputAreaFieldId == filter.InputAreaFieldId)?.FieldIndex;
                if (fieldIndex.HasValue && !string.IsNullOrWhiteSpace(firstValue))
                {
                    var rProp = Expression.Property(tParam, GetFieldName(fieldIndex.Value));

                    var nProp = Expression.Property(nParam, GetFieldName(fieldIndex.Value));

                    switch (filter.Operator)
                    {
                        //text
                        case EnumOperator.Equal:
                            rowAndExpressions.Add(Expression.Equal(rProp, valueLambda.Body));
                            break;

                        case EnumOperator.Contains:
                            var methodContainInfo = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
                            rowAndExpressions.Add(Expression.Call(rProp, methodContainInfo, valueLambda.Body));
                            break;

                        case EnumOperator.InList:
                            var methodInList = typeof(List<string>).GetMethod(nameof(List<string>.Contains), new[] { typeof(string) });
                            rNumberAndExpressions.Add(Expression.Call(lstValueLambda.Body, methodInList, nProp));
                            break;


                        //number
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



                    }
                }
                //}



                if (rowAndExpressions.Count > 0)
                {
                    Expression ex = Expression.Constant(true);

                    foreach (var expression in rowAndExpressions)
                    {
                        ex = Expression.AndAlso(ex, expression);
                    }

                    versions = versions.Where(Expression.Lambda<Func<InputValueRowVersionTextEntity, bool>>(ex, tObj));
                }

                if (rNumberAndExpressions.Count > 0)
                {
                    Expression ex = Expression.Constant(true);

                    foreach (var expression in rNumberAndExpressions)
                    {
                        ex = Expression.AndAlso(ex, expression);
                    }

                    versionsInNumbers = versionsInNumbers.Where(Expression.Lambda<Func<InputValueRowVersionInNumberEntity, bool>>(ex, nObj));
                }


                query = from b in query
                        join v in versions on b.InputValueBillId equals v.InputValueBillId
                        join n in versionsInNumbers on b.InputValueBillId equals n.InputValueBillId
                        select new
                        {
                            b.InputValueBillId,
                            b.OrderValue,
                            b.OrderValueInNumber,
                            b.CreatedDatetimeUtc
                        };



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

            var sortField = fields.FirstOrDefault(f => f.FieldName == orderBy);
            if (sortField != null)
            {
                //    versions = from r in _accountingContext.InputValueRow
                //                   join v in _accountingContext.InputValueRowVersion on r.LastestInputValueRowVersionId equals v.InputValueRowVersionId
                //                   where r.InputAreaId == sortField.InputAreaId
                //                   select new InputValueRowVersionTextEntity
                //                   {
                //                       InputValueBillId = r.InputValueBillId,
                //                       InputValueRowId = r.InputValueRowId,
                //                       VersionText = v
                //                   };

                //    versionsInNumbers = from r in _accountingContext.InputValueRow
                //                            join n in _accountingContext.InputValueRowVersionNumber on r.LastestInputValueRowVersionId equals n.InputValueRowVersionId
                //                            where r.InputAreaId == sortField.InputAreaId
                //                            select new InputValueRowVersionInNumberEntity
                //                            {
                //                                InputValueBillId = r.InputValueBillId,
                //                                InputValueRowId = r.InputValueRowId,
                //                                VersionNumber = n
                //                            };


                var sortFieldName = GetFieldName(sortField.FieldIndex);


                var sortVersion = versions.ProjectTo<InputValueBillOrderValueModel>(new MapperConfiguration(cfg =>
                {
                    var map = cfg.CreateMap<InputValueRowVersionTextEntity, InputValueBillOrderValueModel>();

                    var param = Expression.Parameter(typeof(InputValueRowVersionTextEntity), "f");

                    var versionText = Expression.Property(param, nameof(InputValueRowVersionTextEntity.VersionText));
                    var field = Expression.Property(versionText, sortFieldName);

                    map.ForMember(nameof(InputValueBillOrderValueModel.OrderValue), m => m.MapFrom(Expression.Lambda<Func<InputValueRowVersionTextEntity, string>>(field, param)));
                }));

                var sortVersionInNumbers = versionsInNumbers.ProjectTo<InputValueBillOrderValueInNumberModel>(new MapperConfiguration(cfg =>
                {
                    var map = cfg.CreateMap<InputValueRowVersionInNumberEntity, InputValueBillOrderValueInNumberModel>();

                    var param = Expression.Parameter(typeof(InputValueRowVersionInNumberEntity), "f");

                    var versionNumber = Expression.Property(param, nameof(InputValueRowVersionInNumberEntity.VersionNumber));
                    var field = Expression.Property(versionNumber, sortFieldName);

                    map.ForMember(nameof(InputValueBillOrderValueInNumberModel.OrderValueInNumber), m => m.MapFrom(Expression.Lambda<Func<InputValueRowVersionInNumberEntity, long>>(field, param)));
                }));
                //versionsInNumbers.DynamicSelectGenerator<InputValueRowVersionInNumberEntity, InputValueBillOrderValueInNumberModel>(nMapFields);

                query = from b in query
                        join v in sortVersion on b.InputValueBillId equals v.InputValueBillId into vs
                        from v in vs.DefaultIfEmpty()
                        join n in sortVersionInNumbers on b.InputValueBillId equals n.InputValueBillId into ns
                        from n in ns.DefaultIfEmpty()
                        select new
                        {
                            b.InputValueBillId,
                            OrderValue = v == null ? null : v.OrderValue,
                            // OrderValueInNumber = 0L,
                            OrderValueInNumber = n == null ? (long?)null : n.OrderValueInNumber,//Get number
                            b.CreatedDatetimeUtc
                        };
            }


            query = query.Distinct();

            var total = await query.CountAsync();

            IQueryable<long> orderedInputValueRowIds;

            if (sortField != null)
            {
                if (TextDataTypes.Contains((EnumDataType)sortField.DataTypeId))
                {
                    if (asc)
                    {
                        orderedInputValueRowIds = query.OrderBy(b => b.OrderValue).ThenBy(b => b.CreatedDatetimeUtc).Select(b => b.InputValueBillId);
                    }
                    else
                    {
                        orderedInputValueRowIds = query.OrderByDescending(b => b.OrderValue).ThenByDescending(b => b.CreatedDatetimeUtc).Select(b => b.InputValueBillId);
                    }

                }
                else
                {
                    if (asc)
                    {
                        orderedInputValueRowIds = query.OrderBy(b => b.OrderValueInNumber).ThenBy(b => b.OrderValue).ThenBy(b => b.CreatedDatetimeUtc).Select(b => b.InputValueBillId);
                    }
                    else
                    {
                        orderedInputValueRowIds = query.OrderByDescending(b => b.OrderValueInNumber).ThenByDescending(b => b.OrderValue).ThenBy(b => b.CreatedDatetimeUtc).Select(b => b.InputValueBillId);
                    }
                }
            }
            else
            {
                orderedInputValueRowIds = query.Select(q => q.InputValueBillId);
            }

            var billIds = await orderedInputValueRowIds.Skip((page - 1) * size).Take(size).ToListAsync();

            var rowData = (await (
                   from r in _accountingContext.InputValueRow
                   join v in _accountingContext.InputValueRowVersion on r.LastestInputValueRowVersionId equals v.InputValueRowVersionId
                   where billIds.Contains(r.InputValueBillId)
                   select new
                   {
                       r.InputValueBillId,
                       r.IsMultiRow,
                       Row = v
                   })
                   .ToListAsync()
                   )
                   .GroupBy(r => r.InputValueBillId)
                   .ToDictionary(r => r.Key, r => r.ToList());


            var type = typeof(InputValueRowVersion);
            var properties = new Dictionary<int, PropertyInfo>();

            for (var i = 0; i <= AccountantConstants.INPUT_TYPE_FIELD_NUMBER; i++)
            {
                properties.Add(i, type.GetProperty(GetFieldName(i)));
            }


            var columnList = await GetInputTypeListColumn(inputTypeId);
            var props = (
                from c in columnList
                join p in properties on c.FieldIndex equals p.Key
                select new
                {
                    Column = c,
                    Property = p.Value
                })
                .ToList();

            var lst = new List<InputValueBillListOutput>();

            foreach (var billId in billIds)
            {
                var row = new InputValueBillListOutput();
                row.InputValueBillId = billId;

                row.FieldValues = new Dictionary<int, string>();
                if (rowData.TryGetValue(billId, out var data))
                {
                    foreach (var areaGroup in data.GroupBy(d => d.IsMultiRow))
                    {
                        foreach (var prop in props.Where(p => p.Column.IsMultiRow == areaGroup.Key))
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
            // Get area
            var areas = _accountingContext.InputArea.Where(a => a.InputTypeId == inputTypeId).ToList();

            var lstField = _accountingContext.InputAreaField
                .Include(f => f.InputField)
                .Where(f => f.InputTypeId == inputTypeId)
                .Select(f => new
                {
                    f.InputAreaFieldId,
                    f.InputAreaId,
                    FieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, f.InputField.FieldIndex)
                }).ToList();
            InputValueOuputModel inputValueOuputModel = new InputValueOuputModel();
            // Check exist
            var lstGroups = (from row in _accountingContext.InputValueRow
                             where row.InputValueBillId == inputValueBillId
                             join rowVersion in _accountingContext.InputValueRowVersion
                             on new { row.InputValueRowId, inputValueRowVersionId = row.LastestInputValueRowVersionId }
                             equals new { rowVersion.InputValueRowId, inputValueRowVersionId = rowVersion.InputValueRowVersionId }
                             select new
                             {
                                 //row.InputAreaId,
                                 row.IsMultiRow,
                                 row.InputValueRowId,
                                 Data = rowVersion
                             })
                           .ToList()
                           .GroupBy(r => r.IsMultiRow);
            foreach (var group in lstGroups)
            {
                foreach (var area in areas.Where(a => a.IsMultiRow == group.Key))
                {
                    var fields = lstField.Where(f => f.InputAreaId == area.InputAreaId);
                    foreach (var row in group)
                    {
                        InputRowOutputModel inputRowOutputModel = new InputRowOutputModel
                        {
                            InputAreaId = area.InputAreaId,
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
            }
            return inputValueOuputModel;
        }

        public async Task<ServiceResult<long>> AddInputValueBill(int inputTypeId, InputValueInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            // Validate
            var inputType = _accountingContext.InputType.FirstOrDefault(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }

            // Lấy thông tin field
            var inputAreaFields = _accountingContext.InputAreaField
                .Include(f => f.InputField.DataType)
                .Where(f => f.InputTypeId == inputTypeId).AsEnumerable();

            var requiredFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire);
            var uniqueFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsUnique);
            var selectFields = inputAreaFields.Where(f => !f.IsAutoIncrement && AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)f.InputField.FormTypeId));

            List<ValidateRowModel> checkRows = data.Rows.Select(r => new ValidateRowModel(r, null)).ToList();

            // Get area
            var areas = _accountingContext.InputArea.Where(a => data.Rows.Select(r => r.InputAreaId).Contains(a.InputAreaId)).ToList();

            // Validate multiRow existed
            if (!areas.Any(r => r.IsMultiRow))
            {
                throw new BadRequestException(InputErrorCode.MultiRowAreaEmpty);
            }
            // Check field required
            CheckRequired(checkRows, requiredFields);
            // Check refer
            CheckRefer(ref checkRows, selectFields);
            // Check unique
            CheckUnique(checkRows, uniqueFields);
            // Check value
            CheckValue(checkRows, inputAreaFields);

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Insert bill
                var inputValueBill = new InputValueBill
                {
                    InputTypeId = inputTypeId
                };
                await _accountingContext.InputValueBill.AddAsync(inputValueBill);
                await _accountingContext.SaveChangesAsync();

                // Merge singerRow
                var singerRows = checkRows.Select(r => r.Data).Where(r => areas.Where(a => !a.IsMultiRow).Select(a => a.InputAreaId).Contains(r.InputAreaId)).ToList();
                if (singerRows.Count > 0)
                {
                    var mergeData = singerRows
                       .Select(r => r.Values)
                       .Aggregate(new List<InputValueModel>(), (x, y) => x.Concat(y).ToList());
                    ICollection<ICollection<InputValueModel>> singerData = new List<ICollection<InputValueModel>>
                    {
                        mergeData
                    };
                    // Insert singer row data
                    await InsertRows(false, singerData, inputValueBill.InputValueBillId, inputAreaFields);
                }
                var multiRows = checkRows.Select(r => r.Data).Where(r => areas.Where(a => a.IsMultiRow).Select(a => a.InputAreaId).Contains(r.InputAreaId)).ToList();
                // Insert multi row data
                await InsertRows(true, multiRows.Select(r => r.Values).ToList(), inputValueBill.InputValueBillId, inputAreaFields);

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

        private async Task InsertRows(bool isMultiRow, ICollection<ICollection<InputValueModel>> inputValueRows, long inputValueBillId, IEnumerable<InputAreaField> inputAreaFields)
        {
            // Insert row
            foreach (var rowModel in inputValueRows)
            {
                var inputValueRow = new InputValueRow
                {
                    IsMultiRow = isMultiRow,
                    InputValueBillId = inputValueBillId
                };
                await _accountingContext.InputValueRow.AddAsync(inputValueRow);
                await _accountingContext.SaveChangesAsync();

                // Insert row version
                var inputValueRowVersion = CreateRowVersion(isMultiRow, inputValueRow.InputValueRowId, rowModel, inputAreaFields);
                await _accountingContext.InputValueRowVersion.AddAsync(inputValueRowVersion);
                await _accountingContext.SaveChangesAsync();

                // Insert row version number
                var inputValueRowVersionNumber = CreateRowVersionNumber(isMultiRow, inputValueRowVersion, inputAreaFields);
                await _accountingContext.InputValueRowVersionNumber.AddAsync(inputValueRowVersionNumber);
                await _accountingContext.SaveChangesAsync();

                // Update lasted version
                inputValueRow.LastestInputValueRowVersionId = inputValueRowVersion.InputValueRowVersionId;
                await _accountingContext.SaveChangesAsync();
            }
        }

        private InputValueRowVersion CreateRowVersion(bool isMultiField, long rowId, ICollection<InputValueModel> valueModels, IEnumerable<InputAreaField> fields)
        {
            var inputValueRowVersion = new InputValueRowVersion
            {
                InputValueRowId = rowId
            };

            // Set value AutoIncrement
            var areaFields = fields.Where(f => f.InputArea.IsMultiRow == isMultiField);
            foreach (var field in areaFields)
            {
                string value;
                string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, field.InputField.FieldIndex);
                if (field.IsAutoIncrement)
                {
                    var rvParam = Expression.Parameter(typeof(InputValueRowVersionNumber), "rvn");
                    MemberExpression memberExp = Expression.PropertyOrField(rvParam, fieldName);
                    var lambdaExp = Expression.Lambda<Func<InputValueRowVersionNumber, long>>(memberExp, rvParam);

                    long maxValue = _accountingContext.InputValueRowVersionNumber
                        .Include(rvn => rvn.InputValueRowVersion)
                        .ThenInclude(rv => rv.InputValueRow)
                        .Where(rvn => rvn.InputValueRowVersion.InputValueRow.IsMultiRow == field.InputArea.IsMultiRow)
                        .Where(rvn => rvn.InputValueRowVersionId == rvn.InputValueRowVersion.InputValueRow.LastestInputValueRowVersionId)
                        .Max(lambdaExp);
                    maxValue = (maxValue / AccountantConstants.CONVERT_VALUE_TO_NUMBER_FACTOR) + 1;
                    value = maxValue.ToString();
                }
                value = valueModels.Where(v => v.InputAreaFieldId == field.InputAreaFieldId).FirstOrDefault()?.Value ?? null;
                inputValueRowVersion.GetType().GetProperty(fieldName).SetValue(inputValueRowVersion, value);
            }
            return inputValueRowVersion;
        }

        private InputValueRowVersionNumber CreateRowVersionNumber(bool isMultiRow, InputValueRowVersion rowVersion, IEnumerable<InputAreaField> fields)
        {
            var inputValueRowVersionNumber = new InputValueRowVersionNumber
            {
                InputValueRowVersionId = rowVersion.InputValueRowVersionId
            };
            for (int fieldIndx = 0; fieldIndx < AccountantConstants.INPUT_TYPE_FIELD_NUMBER; fieldIndx++)
            {
                string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, fieldIndx);
                long valueInNumber = 0;
                var field = fields.Where(f => f.InputArea.IsMultiRow == isMultiRow && f.InputField.FieldIndex == fieldIndx).FirstOrDefault();
                string value = (string)typeof(InputValueRowVersion).GetProperty(fieldName).GetValue(rowVersion);
                if (field != null && !string.IsNullOrEmpty(value))
                {
                    valueInNumber = value.ConvertValueToNumber((EnumDataType)field.InputField.DataTypeId);
                }
                typeof(InputValueRowVersionNumber).GetProperty(fieldName).SetValue(inputValueRowVersionNumber, valueInNumber);
            }
            return inputValueRowVersionNumber;
        }

        public async Task<ServiceResult<long>> UpdateInputValueBill(int inputTypeId, long inputValueBillId, InputValueInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            // Lấy thông tin bill hiện tại
            var currentBill = _accountingContext.InputValueBill
                .Where(b => b.InputValueBillId == inputValueBillId && b.InputTypeId == inputTypeId)
                .FirstOrDefault();
            if (currentBill == null)
            {
                return InputErrorCode.InputValueBillNotFound;
            }
            // Get area
            var areas = _accountingContext.InputArea.Where(a => data.Rows.Select(r => r.InputAreaId).Contains(a.InputAreaId)).ToList();

            // Validate multiRow existed
            if (!areas.Any(r => r.IsMultiRow))
            {
                throw new BadRequestException(InputErrorCode.MultiRowAreaEmpty);
            }
            var inputAreaFields = _accountingContext.InputAreaField
                .Include(f => f.InputArea)
                .Include(f => f.InputField.DataType)
                .Where(f => f.InputTypeId == inputTypeId).ToList();


            // Lấy các row thay đổi
            List<InputValueRow> curRows = _accountingContext.InputValueRow
                .Where(r => r.InputValueBillId == inputValueBillId)
                .Include(r => r.InputValueRowVersion)
                .ToList();
            List<InputValueRowInputModel> futureRows = new List<InputValueRowInputModel>(data.Rows);
            List<(InputValueRowInputModel Future, InputValueRow Current)> updateRows = new List<(InputValueRowInputModel Future, InputValueRow Current)>();

            var checkRows = new List<ValidateRowModel>();

            foreach (InputValueRowInputModel futureRow in data.Rows)
            {
                InputValueRow curRow = curRows.FirstOrDefault(r => r.InputValueRowId == futureRow.InputValueRowId);
                if (curRow == null)
                {
                    checkRows.Add(new ValidateRowModel(futureRow, null));
                }
                else
                {
                    int[] changeFieldIndexes = CompareRow(curRow, futureRow, inputAreaFields);
                    if (changeFieldIndexes.Length > 0)
                    {
                        updateRows.Add((futureRow, curRow));
                        checkRows.Add(new ValidateRowModel(futureRow, changeFieldIndexes));
                    }
                    curRows.Remove(curRow);
                    futureRows.Remove(futureRow);
                }
            }
            // Lấy thông tin field
            var requiredFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire);
            var uniqueFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsUnique);
            var selectFields = inputAreaFields.Where(f => !f.IsAutoIncrement && AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)f.InputField.FormTypeId));

            // Check field required
            CheckRequired(checkRows, requiredFields);
            // Check refer
            CheckRefer(ref checkRows, selectFields);
            // Check unique
            CheckUnique(checkRows, uniqueFields, curRows.Select(r => r.InputValueRowId).ToArray());
            // Check value
            CheckValue(checkRows, inputAreaFields);

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

                #region Insert new data

                // Merge singerRow
                var singerRows = futureRows.Where(r => areas.Where(a => !a.IsMultiRow).Select(a => a.InputAreaId).Contains(r.InputAreaId)).ToList();
                if (singerRows.Count > 0)
                {
                    var mergeData = singerRows
                       .Select(r => r.Values)
                       .Aggregate(new List<InputValueModel>(), (x, y) => x.Concat(y).ToList());
                    ICollection<ICollection<InputValueModel>> singerData = new List<ICollection<InputValueModel>>
                    {
                        mergeData
                    };
                    // Insert singer row data
                    await InsertRows(false, singerData, inputValueBillId, inputAreaFields);
                }
                var multiRows = futureRows.Where(r => areas.Where(a => a.IsMultiRow).Select(a => a.InputAreaId).Contains(r.InputAreaId)).ToList();
                // Insert multi row data
                await InsertRows(true, multiRows.Select(r => r.Values).ToList(), inputValueBillId, inputAreaFields);
                #endregion

                #region Update row

                // Merge singerRow
                var singerUpdateRows = updateRows.Where(r => areas.Where(a => !a.IsMultiRow).Select(a => a.InputAreaId).Contains(r.Future.InputAreaId)).ToList();
                if (singerUpdateRows.Count > 0)
                {
                    var current = singerUpdateRows.First().Current;
                    var mergeUpdateData = singerUpdateRows
                       .Select(r => r.Future.Values)
                       .Aggregate(new List<InputValueModel>(), (x, y) => x.Concat(y).ToList());

                    // Insert row version
                    var inputValueRowVersion = CreateRowVersion(false, current.InputValueRowId, mergeUpdateData, inputAreaFields);
                    await _accountingContext.InputValueRowVersion.AddAsync(inputValueRowVersion);
                    await _accountingContext.SaveChangesAsync();

                    // Insert row version number
                    var inputValueRowVersionNumber = CreateRowVersionNumber(false, inputValueRowVersion, inputAreaFields);
                    await _accountingContext.InputValueRowVersionNumber.AddAsync(inputValueRowVersionNumber);
                    await _accountingContext.SaveChangesAsync();

                    // Update lasted version
                    current.LastestInputValueRowVersionId = inputValueRowVersion.InputValueRowVersionId;
                    await _accountingContext.SaveChangesAsync();
                }

                var multiUpdateRows = updateRows.Where(r => areas.Where(a => a.IsMultiRow).Select(a => a.InputAreaId).Contains(r.Future.InputAreaId)).ToList();

                foreach (var (future, current) in multiUpdateRows)
                {

                    // Insert row version
                    var inputValueRowVersion = CreateRowVersion(true, future.InputValueRowId.Value, future.Values, inputAreaFields);
                    await _accountingContext.InputValueRowVersion.AddAsync(inputValueRowVersion);
                    await _accountingContext.SaveChangesAsync();

                    // Insert row version number
                    var inputValueRowVersionNumber = CreateRowVersionNumber(true, inputValueRowVersion, inputAreaFields);
                    await _accountingContext.InputValueRowVersionNumber.AddAsync(inputValueRowVersionNumber);
                    await _accountingContext.SaveChangesAsync();

                    // Update lasted version
                    current.LastestInputValueRowVersionId = inputValueRowVersion.InputValueRowVersionId;
                    await _accountingContext.SaveChangesAsync();
                }
                #endregion

                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, currentBill.InputValueBillId, $"Cập nhật chứng từ {currentBill.InputValueBillId}", data.JsonSerialize());
                return currentBill.InputValueBillId;
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
            foreach (var field in fields.Where(f => f.InputAreaId == futureRow.InputAreaId))
            {
                string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, field.InputField.FieldIndex);
                string curValue = (string)typeof(InputValueRowVersion)
                    .GetProperty(fieldName)
                    .GetValue(curRow.InputValueRowVersion.First(rv => rv.InputValueRowVersionId == curRow.LastestInputValueRowVersionId));

                var fieldValue = futureRow.Values.Where(v => v.InputAreaFieldId == field.InputAreaFieldId).FirstOrDefault();
                string futureValue = fieldValue.Value;
                if (curValue != futureValue)
                {
                    changeFieldIndexes.Add(field.InputField.FieldIndex);
                }
            }
            return changeFieldIndexes.ToArray();
        }

        private void CheckRequired(List<ValidateRowModel> data, IEnumerable<InputAreaField> requiredFields)
        {
            foreach (var field in requiredFields)
            {
                //bool isRefer = AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.FormTypeId);
                var changeRows = data.Where(r => r.Data.InputAreaId == field.InputArea.InputAreaId)
                    .Where(r => r.CheckFields == null || r.CheckFields.Contains(field.InputField.FieldIndex));
                if (changeRows.Count() == 0)
                {
                    throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new string[] { field.Title });
                }
                foreach (var row in changeRows)
                {
                    var fieldValue = row.Data.Values.Where(v => v.InputAreaFieldId == field.InputAreaFieldId).FirstOrDefault();
                    if (fieldValue == null || string.IsNullOrEmpty(fieldValue.Value))
                    {
                        throw new BadRequestException(InputErrorCode.RequiredFieldIsEmpty, new string[] { field.Title });
                    }
                }
            }
        }

        private void CheckUnique(List<ValidateRowModel> data, IEnumerable<InputAreaField> uniqueFields, long[] deleteInputValueRowId = null)
        {
            if (deleteInputValueRowId is null)
            {
                deleteInputValueRowId = Array.Empty<long>();
            }
            // Check unique
            foreach (var field in uniqueFields)
            {
                string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, field.InputField.FieldIndex);
                var changeRows = data.Where(r => r.Data.InputAreaId == field.InputArea.InputAreaId)
                   .Where(r => r.CheckFields == null || r.CheckFields.Contains(field.InputField.FieldIndex));
                var fieldValues = changeRows
                       .SelectMany(r => r.Data.Values)
                       .Where(v => v.InputAreaFieldId == field.InputAreaFieldId);
                var rowIds = changeRows.Where(r => r.Data.InputValueRowId.HasValue && r.Data.InputValueRowId.Value > 0).Select(r => r.Data.InputValueRowId.Value).ToList();

                bool isExisted = false;

                var values = fieldValues.Select(v => v.Value).ToList();

                // Check unique trong danh sách values thêm mới/sửa
                if (values.Count != values.Distinct().Count())
                {
                    throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                }

                // Checkin unique trong db
                var rvParam = Expression.Parameter(typeof(InputValueRowVersion), "rv");
                var property = Expression.Property(rvParam, fieldName);
                var methodInfo = typeof(List<string>).GetMethod("Contains");

                Expression expression = Expression.Call(Expression.Constant(values), methodInfo, property);
                isExisted = (from ver in _accountingContext.InputValueRowVersion
                             join row in _accountingContext.InputValueRow
                             on new { ver.InputValueRowId, inputValueRowVersionId = ver.InputValueRowVersionId }
                             equals new { row.InputValueRowId, inputValueRowVersionId = row.LastestInputValueRowVersionId }
                             join bill in _accountingContext.InputValueBill on row.InputValueBillId equals bill.InputValueBillId
                             join area in _accountingContext.InputArea on field.InputAreaId equals area.InputAreaId
                             where bill.InputTypeId == field.InputTypeId && row.IsMultiRow == area.IsMultiRow
                             && !deleteInputValueRowId.Contains(row.InputValueRowId)
                             && !rowIds.Contains(row.InputValueRowId)
                             select ver)
                             .Any(Expression.Lambda<Func<InputValueRowVersion, bool>>(expression, rvParam));

                if (isExisted)
                {
                    throw new BadRequestException(InputErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                }
            }
        }

        private void CheckRefer(ref List<ValidateRowModel> data, IEnumerable<InputAreaField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                var changeRows = data.Where(r => r.Data.InputAreaId == field.InputArea.InputAreaId)
                   .Where(r => r.CheckFields == null || r.CheckFields.Contains(field.InputField.FieldIndex));
                var fieldValues = changeRows
                       .SelectMany(r => r.Data.Values)
                       .Where(v => v.InputAreaFieldId == field.InputAreaFieldId);

                bool isExisted = true;
                if (field.InputField.ReferenceCategoryFieldId.HasValue)
                {
                    CategoryField referField = _accountingContext.CategoryField.First(f => f.CategoryFieldId == field.InputField.ReferenceCategoryFieldId.Value);
                    CategoryEntity referCategory = _accountingContext.Category.First(c => c.CategoryId == referField.CategoryId);
                    IQueryable<CategoryRow> query;
                    Clause filters = null;
                    if (!string.IsNullOrEmpty(field.Filters))
                    {

                        filters = JsonConvert.DeserializeObject<Clause>(field.Filters);
                        var fields = _accountingContext.CategoryField.Where(f => f.CategoryId == referCategory.CategoryId).ToList();
                        filters = AddFieldName(filters, fields);
                    }

                    if (referCategory.IsOutSideData && !referCategory.IsTreeView)
                    {
                        query = GetOutSideCategoryRows(referCategory.CategoryId, filters);
                    }
                    else
                    {
                        if (referCategory.IsOutSideData)
                        {
                            query = GetOutSideCategoryRows(referCategory.CategoryId);
                        }
                        else
                        {
                            query = _accountingContext.CategoryRow
                             .Where(r => r.CategoryId == referCategory.CategoryId)
                             .Include(r => r.CategoryRowValue);
                        }
                        if (filters != null)
                        {
                            ParameterExpression param = Expression.Parameter(typeof(CategoryRow), "r");
                            Expression filter = FilterClauseProcess(param, filters, query);
                            query = query.Where(Expression.Lambda<Func<CategoryRow, bool>>(filter, param));
                        }
                    }
                    var values = fieldValues.Select(v => v.Value).ToList();
                    if (values.Count > 0)
                    {
                        isExisted = values.All(v => query.Any(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == referField.CategoryFieldId && rv.Value == v)));
                    }
                    if (!isExisted)
                    {
                        throw new BadRequestException(InputErrorCode.ReferValueNotFound, new string[] { field.Title });
                    }
                }
            }
        }

        private void CheckValue(List<ValidateRowModel> data, IEnumerable<InputAreaField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, field.InputField.FieldIndex);
                var changeRows = data.Where(r => r.Data.InputAreaId == field.InputArea.InputAreaId)
                    .Where(r => r.CheckFields == null || r.CheckFields.Contains(field.InputField.FieldIndex));
                var fieldValues = changeRows
                       .SelectMany(r => r.Data.Values)
                       .Where(v => v.InputAreaFieldId == field.InputAreaFieldId);

                foreach (var value in fieldValues)
                {
                    if ((AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.InputField.FormTypeId)) || field.IsAutoIncrement || string.IsNullOrEmpty(value.Value))
                    {
                        continue;
                    }
                    CheckValue(value.Value, field);
                }
            }
        }

        private void CheckValue(string value, InputAreaField field)
        {
            if ((field.InputField.DataSize > 0 && value.Length > field.InputField.DataSize)
                || !string.IsNullOrEmpty(field.InputField.DataType.RegularExpression) && !Regex.IsMatch(value, field.InputField.DataType.RegularExpression)
                || !string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(value, field.RegularExpression))
            {
                throw new BadRequestException(InputErrorCode.InputValueInValid, new string[] { field.Title });
            }
        }

        public async Task<Enum> DeleteInputValueBill(int inputTypeId, long inputValueBillId)
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

                // Delete row
                var inputValueRows = _accountingContext.InputValueRow.Where(r => r.InputValueBillId == inputValueBillId).ToList();
                foreach (var row in inputValueRows)
                {
                    row.IsDeleted = true;

                    // Delete row version
                    var inputValueRowVersions = _accountingContext.InputValueRowVersion.Where(rv => rv.InputValueRowId == row.InputValueRowId).ToList();
                    foreach (var rowVersion in inputValueRowVersions)
                    {
                        rowVersion.IsDeleted = true;
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


        public class ValidateRowModel
        {
            public InputValueRowInputModel Data { get; set; }
            public int[] CheckFields { get; set; }

            public ValidateRowModel(InputValueRowInputModel Data, int[] CheckFields)
            {
                this.Data = Data;
                this.CheckFields = CheckFields;
            }
        }

    }


    public class InputValueRowVersionTextEntity
    {
        public long InputValueBillId { get; set; }
        public long InputValueRowId { get; set; }
        public InputValueRowVersion VersionText { get; set; }
    }

    public class InputValueRowVersionInNumberEntity
    {
        public long InputValueBillId { get; set; }
        public long InputValueRowId { get; set; }
        public InputValueRowVersionNumber VersionNumber { get; set; }
    }

    public class InputValueBillOrderValueModel
    {
        public long InputValueBillId { get; set; }
        public string OrderValue { get; set; }
    }

    public class InputValueBillOrderValueInNumberModel
    {
        public long InputValueBillId { get; set; }
        public long OrderValueInNumber { get; set; }
    }
}
