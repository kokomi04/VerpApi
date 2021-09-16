using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Organization.Model.HrConfig;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.HrConfig
{
    public interface IHrAreaService
    {
        Task<int> AddHrArea(int hrTypeId, HrAreaInputModel data);
        Task<HrFieldInputModel> AddHrField(int hrAreaId, HrFieldInputModel data);
        Task<bool> DeleteHrArea(int hrTypeId, int hrAreaId);
        Task<bool> DeleteHrField(int hrAreaId, int hrFieldId);
        Task<HrAreaModel> GetHrArea(int hrTypeId, int hrAreaId);
        Task<HrAreaFieldOutputFullModel> GetHrAreaField(int hrTypeId, int hrAreaId, int inputAreaFieldId);
        Task<PageData<HrAreaFieldOutputFullModel>> GetHrAreaFields(int hrTypeId, int hrAreaId, string keyword, int page, int size);
        Task<PageData<HrAreaModel>> GetHrAreas(int hrTypeId, string keyword, int page, int size);
        Task<bool> UpdateHrArea(int hrTypeId, int hrAreaId, HrAreaInputModel data);
        Task<HrFieldInputModel> UpdateHrField(int hrAreaId, int hrFieldId, HrFieldInputModel data);
        Task<bool> UpdateMultiField(int hrTypeId, List<HrAreaFieldInputModel> fields);
        Task<PageData<HrFieldOutputModel>> GetHrFields(int hrAreaId, string keyword, int page, int size);
    }

    public class HrAreaService : IHrAreaService
    {
        private const string HR_TABLE_NAME_PREFIX = OrganizationConstants.HR_TABLE_NAME_PREFIX;

        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICategoryHelperService _categoryHelperService;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public HrAreaService(
            ILogger<HrAreaService> logger,
            IMapper mapper,
            IActivityLogService activityLogService,
            OrganizationDBContext organizationDBContext,
            ICategoryHelperService categoryHelperService,
            ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _logger = logger;
            _mapper = mapper;
            _activityLogService = activityLogService;
            _organizationDBContext = organizationDBContext;
            _categoryHelperService = categoryHelperService;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        #region Area
        public async Task<HrAreaModel> GetHrArea(int hrTypeId, int hrAreaId)
        {
            var inputArea = await _organizationDBContext.HrArea
                .Where(i => i.HrTypeId == hrTypeId && i.HrAreaId == hrAreaId)
                .ProjectTo<HrAreaModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (inputArea == null)
            {
                throw new BadRequestException(HrErrorCode.HrTypeNotFound);
            }
            return inputArea;
        }

        public async Task<PageData<HrAreaModel>> GetHrAreas(int hrTypeId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _organizationDBContext.HrArea.Where(a => a.HrTypeId == hrTypeId).AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.HrAreaCode.Contains(keyword) || a.Title.Contains(keyword));
            }
            query = query.OrderBy(c => c.Title);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var lst = await query.ProjectTo<HrAreaModel>(_mapper.ConfigurationProvider).OrderBy(a => a.SortOrder).ToListAsync();
            return (lst, total);
        }

        public async Task<int> AddHrArea(int hrTypeId, HrAreaInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(hrTypeId));

            var hrType = await _organizationDBContext.HrType.FirstOrDefaultAsync(x => x.HrTypeId == hrTypeId);

            _ = hrType ?? throw new BadRequestException(HrErrorCode.HrTypeNotFound);

            var existedHr = await _organizationDBContext.HrArea
                .FirstOrDefaultAsync(a => a.HrTypeId == hrTypeId && (a.HrAreaCode == data.HrAreaCode || a.Title == data.Title));
            if (existedHr != null)
            {
                if (string.Compare(existedHr.HrAreaCode, data.HrAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(HrErrorCode.HrCodeAlreadyExisted);
                }

                throw new BadRequestException(HrErrorCode.HrTitleAlreadyExisted);
            }

            // if (data.IsMultiRow && _organizationDBContext.HrArea.Any(a => a.HrTypeId == hrTypeId && a.IsMultiRow))
            // {
            //     throw new BadRequestException(HrErrorCode.MultiRowAreaAlreadyExisted);
            // }

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                HrArea inputArea = _mapper.Map<HrArea>(data);
                inputArea.HrTypeId = hrTypeId;
                await _organizationDBContext.HrArea.AddAsync(inputArea);
                await _organizationDBContext.SaveChangesAsync();

                await _organizationDBContext.ExecuteStoreProcedure("asp_Hr_Area_Table_Add", new[] {
                        new SqlParameter("@HrAreaTableName", GetHrAreaTableName(hrType.HrTypeCode, data.HrAreaCode)),
                    });

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.HrType, inputArea.HrAreaId, $"Thêm vùng thông tin {inputArea.Title} của chứng từ hành chính nhân sự {hrTypeId}", data.JsonSerialize());
                return inputArea.HrAreaId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "HrAreaService: AddHrArea");
                throw;
            }
        }

        public async Task<bool> UpdateHrArea(int hrTypeId, int hrAreaId, HrAreaInputModel data)
        {
            data.HrTypeId = hrTypeId;
            data.HrAreaId = hrAreaId;

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(hrTypeId));
            var hrType = await _organizationDBContext.HrType.FirstOrDefaultAsync(x => x.HrTypeId == hrTypeId);

            _ = hrType ?? throw new BadRequestException(HrErrorCode.HrTypeNotFound);

            var hrArea = await _organizationDBContext.HrArea.FirstOrDefaultAsync(a => a.HrTypeId == hrTypeId && a.HrAreaId == hrAreaId);
            if (hrArea == null)
            {
                throw new BadRequestException(HrErrorCode.HrAreaNotFound);
            }
            if (hrArea.HrAreaCode != data.HrAreaCode || hrArea.Title != data.Title)
            {
                var existedHr = await _organizationDBContext.HrArea
                    .FirstOrDefaultAsync(a => a.HrTypeId == hrTypeId && a.HrAreaId != hrAreaId && (a.HrAreaCode == data.HrAreaCode || a.Title == data.Title));
                if (existedHr != null)
                {
                    if (string.Compare(existedHr.HrAreaCode, data.HrAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new BadRequestException(HrErrorCode.HrAreaCodeAlreadyExisted);
                    }

                    throw new BadRequestException(HrErrorCode.HrAreaTitleAlreadyExisted);
                }
            }
            // if (data.IsMultiRow && _organizationDBContext.HrArea.Any(a => a.HrTypeId == hrTypeId && a.HrAreaId != hrAreaId && a.IsMultiRow))
            // {
            //     throw new BadRequestException(HrErrorCode.MultiRowAreaAlreadyExisted);
            // }

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var oldHrAreaCode = hrArea.HrAreaCode;
                var newHrAreaCode = data.HrAreaCode;

                _mapper.Map(data, hrArea);
                await _organizationDBContext.SaveChangesAsync();

                if (oldHrAreaCode != newHrAreaCode)
                {
                    await _organizationDBContext.ExecuteStoreProcedure("asp_Hr_Area_Table_Rename", new[] {
                        new SqlParameter("@OldHrAreaTableName", GetHrAreaTableName(hrType.HrTypeCode, oldHrAreaCode)),
                        new SqlParameter("@NewHrAreaTableName", GetHrAreaTableName(hrType.HrTypeCode, newHrAreaCode)),
                    });
                }

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.HrType, hrArea.HrAreaId, $"Cập nhật vùng thông tin {hrArea.Title} của chứng từ hành chính nhân sự {hrTypeId}", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "HrAreaService: UpdateHrArea");
                throw;
            }
        }

        public async Task<bool> DeleteHrArea(int hrTypeId, int hrAreaId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(hrTypeId));

            var hrType = await _organizationDBContext.HrType.FirstOrDefaultAsync(x => x.HrTypeId == hrTypeId);

            _ = hrType ?? throw new BadRequestException(HrErrorCode.HrTypeNotFound);

            var hrArea = await _organizationDBContext.HrArea.FirstOrDefaultAsync(a => a.HrTypeId == hrTypeId && a.HrAreaId == hrAreaId);
            if (hrArea == null)
            {
                throw new BadRequestException(HrErrorCode.HrAreaNotFound);
            }

            await _organizationDBContext.ExecuteStoreProcedure("asp_HrArea_Delete", new[] {
                    new SqlParameter("@HrTypeId",hrTypeId ),
                    new SqlParameter("@HrAreaId",hrAreaId ),
                    new SqlParameter("@TableNamePrefix",HR_TABLE_NAME_PREFIX ),
                    new SqlParameter("@ResStatus",0){ Direction = ParameterDirection.Output },
                    });

            hrArea.IsDeleted = true;
            await _organizationDBContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.HrType, hrArea.HrTypeId, $"Xóa vùng thông tin {hrArea.Title} của chứng từ hành chính nhân sự { hrTypeId}", hrArea.JsonSerialize());
            return true;
        }
        #endregion

        #region Field

        public async Task<PageData<HrAreaFieldOutputFullModel>> GetHrAreaFields(int hrTypeId, int hrAreaId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _organizationDBContext.HrAreaField
                .Include(f => f.HrField)
                .Where(f => f.HrTypeId == hrTypeId && f.HrAreaId == hrAreaId);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(f => f.Title.Contains(keyword));
            }

            query = query.OrderBy(c => c.SortOrder);

            var total = await query.CountAsync();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query)
            .ProjectTo<HrAreaFieldOutputFullModel>(_mapper.ConfigurationProvider)
            .ToListAsync();

            return (lst, total);
        }

        public async Task<PageData<HrFieldOutputModel>> GetHrFields(int hrAreaId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _organizationDBContext.HrField
                .Where(x=>x.HrAreaId == hrAreaId)
                .AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(f => f.FieldName.Contains(keyword) || f.Title.Contains(keyword));
            }

            query = query.OrderBy(f => f.SortOrder);

            var total = await query.CountAsync();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ProjectTo<HrFieldOutputModel>(_mapper.ConfigurationProvider).ToListAsync();

            return (lst, total);
        }

        public async Task<HrAreaFieldOutputFullModel> GetHrAreaField(int hrTypeId, int hrAreaId, int inputAreaFieldId)
        {
            var inputAreaField = await _organizationDBContext.HrAreaField
                .Where(f => f.HrAreaFieldId == inputAreaFieldId && f.HrTypeId == hrTypeId && f.HrAreaId == hrAreaId)
                .Include(f => f.HrField)
                .ProjectTo<HrAreaFieldOutputFullModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            _ = inputAreaField ?? throw new BadRequestException(HrErrorCode.HrAreaFieldNotFound);

            return inputAreaField;
        }

        private void ValidateHrField(HrFieldInputModel data, HrField hrField = null, int? hrFieldId = null)
        {
            if (hrFieldId.HasValue && hrFieldId.Value > 0)
            {
                if (hrField == null)
                {
                    throw new BadRequestException(HrErrorCode.HrFieldNotFound);
                }
                if (_organizationDBContext.HrField.Any(f => f.HrFieldId != hrFieldId.Value && f.FieldName == data.FieldName))
                {
                    throw new BadRequestException(HrErrorCode.HrFieldAlreadyExisted);
                }
                if (!((EnumDataType)hrField.DataTypeId).Convertible((EnumDataType)data.DataTypeId))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển đổi kiểu dữ liệu từ {((EnumDataType)hrField.DataTypeId).GetEnumDescription()} sang {((EnumDataType)data.DataTypeId).GetEnumDescription()}");
                }
            }
            if (!string.IsNullOrEmpty(data.RefTableCode) && !string.IsNullOrEmpty(data.RefTableField))
            {
                var categoryCode = data.RefTableCode;
                var fieldName = data.RefTableField;
                var task = Task.Run(async () => (await _categoryHelperService.GetReferFields(new List<string>() { categoryCode }, new List<string>() { fieldName })).FirstOrDefault());
                task.Wait();
                var sourceCategoryField = task.Result;
                if (sourceCategoryField == null)
                {
                    throw new BadRequestException(HrErrorCode.SourceCategoryFieldNotFound);
                }
            }
        }

        private void FieldDataProcess(ref HrFieldInputModel data)
        {
            if (!string.IsNullOrEmpty(data.RefTableCode) && !string.IsNullOrEmpty(data.RefTableField))
            {
                var categoryCode = data.RefTableCode;
                var fieldName = data.RefTableField;
                var task = Task.Run(async () => (await _categoryHelperService.GetReferFields(new List<string>() { categoryCode }, new List<string>() { fieldName })).FirstOrDefault());
                task.Wait();
                var sourceCategoryField = task.Result;
                if (sourceCategoryField != null)
                {
                    data.DataTypeId = (EnumDataType)sourceCategoryField.DataTypeId;
                    data.DataSize = sourceCategoryField.DataSize;
                }
            }

            if (data.FormTypeId == EnumFormType.Generate)
            {
                data.DataTypeId = EnumDataType.Text;
                data.DataSize = -1;
            }
            if (!DataTypeConstants.SELECT_FORM_TYPES.Contains(data.FormTypeId))
            {
                data.RefTableField = null;
                if (data.FormTypeId != EnumFormType.Input)
                {
                    data.RefTableCode = null;
                    data.RefTableTitle = null;
                }
                else if (string.IsNullOrEmpty(data.RefTableCode))
                {
                    data.RefTableTitle = null;
                }
            }
        }

        public async Task<bool> UpdateMultiField(int hrTypeId, List<HrAreaFieldInputModel> fields)
        {
            var hrTypeInfo = await _organizationDBContext.HrType.AsNoTracking()
                .Where(t => t.HrTypeId == hrTypeId)
                .FirstOrDefaultAsync();

            if (hrTypeInfo == null)
            {
                throw new BadRequestException(HrErrorCode.HrTypeNotFound);
            }

            var areaIds = fields.Select(f => f.HrAreaId).Distinct().ToList();

            var inputAreas = await _organizationDBContext.HrArea.Where(a => a.HrTypeId == hrTypeId).AsNoTracking().ToListAsync();

            foreach (var areaId in areaIds)
            {
                if (!inputAreas.Any(a => a.HrAreaId == areaId))
                {
                    throw new BadRequestException(HrErrorCode.HrAreaNotFound);
                }
            }

            foreach (var field in fields)
            {
                field.HrTypeId = hrTypeId;
            }

            // Validate trùng trong danh sách
            if (fields.Select(f => new { f.HrTypeId, f.HrFieldId }).Distinct().Count() != fields.Count)
            {
                throw new BadRequestException(HrErrorCode.HrAreaFieldAlreadyExisted);
            }

            var curFields = _organizationDBContext.HrAreaField
                .Include(af => af.HrField)
                .IgnoreQueryFilters()
                .Where(f => f.HrTypeId == hrTypeId)
                .ToList();

            var deleteFields = curFields
                .Where(cf => !cf.IsDeleted)
                .Where(cf => fields.All(f => f.HrFieldId != cf.HrFieldId))
                .ToList();

            List<int> singleNewFieldIds = new List<int>();

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                // delete
                foreach (var deleteField in deleteFields)
                {
                    deleteField.IsDeleted = true;

                    await _organizationDBContext.ExecuteStoreProcedure("asp_HrType_Clear_FieldData", new[] {
                        new SqlParameter("@HrTypeId",hrTypeId ),
                        new SqlParameter("@FieldName", deleteField.HrField.FieldName ),
                        new SqlParameter("@ResStatus", 0){ Direction = ParameterDirection.Output },
                    });
                }

                foreach (var field in fields)
                {
                    // validate
                    var curField = curFields.FirstOrDefault(f => f.HrFieldId == field.HrFieldId && f.HrTypeId == field.HrTypeId);
                    if (curField == null)
                    {
                        // create new
                        curField = _mapper.Map<HrAreaField>(field);
                        await _organizationDBContext.HrAreaField.AddAsync(curField);
                    }
                    else if (!field.Compare(curField))
                    {
                        // update field
                        curField.HrAreaId = field.HrAreaId;
                        curField.Title = field.Title;
                        curField.Placeholder = field.Placeholder;
                        curField.SortOrder = field.SortOrder;
                        curField.IsAutoIncrement = field.IsAutoIncrement;
                        curField.IsRequire = field.IsRequire;
                        curField.IsUnique = field.IsUnique;
                        curField.IsHidden = field.IsHidden;
                        curField.IsCalcSum = field.IsCalcSum;
                        curField.RegularExpression = field.RegularExpression;
                        curField.DefaultValue = field.DefaultValue;
                        curField.Filters = field.Filters;
                        curField.IsDeleted = false;
                        // update field id
                        curField.HrFieldId = field.HrFieldId;
                        // update style
                        curField.Width = field.Width;
                        curField.Height = field.Height;
                        curField.TitleStyleJson = field.TitleStyleJson;
                        curField.InputStyleJson = field.InputStyleJson;
                        curField.OnFocus = field.OnFocus;
                        curField.OnKeydown = field.OnKeydown;
                        curField.OnKeypress = field.OnKeypress;
                        curField.OnBlur = field.OnBlur;
                        curField.OnChange = field.OnChange;
                        curField.AutoFocus = field.AutoFocus;
                        curField.Column = field.Column;
                        curField.RequireFilters = field.RequireFilters;
                        curField.ReferenceUrl = field.ReferenceUrl;
                        curField.IsBatchSelect = field.IsBatchSelect;
                        curField.OnClick = field.OnClick;
                    }
                }

                await _organizationDBContext.SaveChangesAsync();

                // Get list gen code
                var genCodeConfigs = fields
                    .Where(f => f.IdGencode.HasValue)
                    .Select(f => new
                    {
                        HrAreaFieldId = f.HrAreaFieldId.Value,
                        IdGencode = f.IdGencode.Value
                    })
                    .ToDictionary(c => (long)c.HrAreaFieldId, c => c.IdGencode);

                var result = await _customGenCodeHelperService.MapObjectCustomGenCode(EnumObjectType.HrTypeRow, EnumObjectType.HrAreaField, genCodeConfigs);

                if (!result)
                {
                    trans.TryRollbackTransaction();
                    throw new BadRequestException(HrErrorCode.MapGenCodeConfigFail);
                }

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.HrType, hrTypeId, $"Cập nhật trường dữ liệu chứng từ {hrTypeInfo.Title}", fields.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "HrAreaService: UpdateMultiField");
                throw;
            }
        }

        public async Task<HrFieldInputModel> AddHrField(int hrAreaId, HrFieldInputModel data)
        {
            var hrArea = await (from a in _organizationDBContext.HrArea
                                join t in _organizationDBContext.HrType on a.HrTypeId equals t.HrTypeId
                                where a.HrAreaId == hrAreaId
                                select new
                                {
                                    t.HrTypeId,
                                    a.HrAreaId,
                                    t.HrTypeCode,
                                    a.HrAreaCode
                                }).FirstOrDefaultAsync();

            _ = hrArea ?? throw new BadRequestException(HrErrorCode.HrAreaNotFound);

            FieldDataProcess(ref data);
            ValidateHrField(data);

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var hrField = _mapper.Map<HrField>(data);

                await _organizationDBContext.HrField.AddAsync(hrField);
                await _organizationDBContext.SaveChangesAsync();

                if (hrField.FormTypeId != (int)EnumFormType.ViewOnly)
                {
                    await _organizationDBContext.AddColumn(GetHrAreaTableName(hrArea.HrTypeCode, hrArea.HrAreaCode), data.FieldName, data.DataTypeId, data.DataSize, data.DecimalPlace, data.DefaultValue, true);
                }
                // await UpdateHrValueView();
                // await UpdateHrTableType();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.HrType, hrField.HrFieldId, $"Thêm trường dữ liệu {hrField.Title} cho vùng thông tin {hrAreaId}", data.JsonSerialize());
                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "HrAreaService: AddHrField");
                throw;
            }
        }

        public async Task<HrFieldInputModel> UpdateHrField(int hrAreaId, int hrFieldId, HrFieldInputModel data)
        {
            var hrArea = await (from a in _organizationDBContext.HrArea
                                join t in _organizationDBContext.HrType on a.HrTypeId equals t.HrTypeId
                                where a.HrAreaId == hrAreaId
                                select new
                                {
                                    t.HrTypeId,
                                    a.HrAreaId,
                                    t.HrTypeCode,
                                    a.HrAreaCode
                                }).FirstOrDefaultAsync();

            _ = hrArea ?? throw new BadRequestException(HrErrorCode.HrAreaNotFound);

            var inputField = await _organizationDBContext.HrField.FirstOrDefaultAsync(f => f.HrFieldId == hrFieldId);
            FieldDataProcess(ref data);
            ValidateHrField(data, inputField, hrFieldId);

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                if (inputField.FormTypeId != (int)EnumFormType.ViewOnly)
                {
                    if (data.FieldName != inputField.FieldName)
                    {
                        await _organizationDBContext.RenameColumn(GetHrAreaTableName(hrArea.HrTypeCode, hrArea.HrAreaCode), inputField.FieldName, data.FieldName);
                    }
                    await _organizationDBContext.UpdateColumn(GetHrAreaTableName(hrArea.HrTypeCode, hrArea.HrAreaCode), data.FieldName, data.DataTypeId, data.DataSize, data.DecimalPlace, data.DefaultValue, true);
                }
                _mapper.Map(data, inputField);

                await _organizationDBContext.SaveChangesAsync();

                // await UpdateHrValueView();
                // await UpdateHrTableType();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.HrType, inputField.HrFieldId, $"Cập nhật trường dữ liệu {inputField.Title} cho vùng thông tin {hrAreaId}", data.JsonSerialize());
                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "HrAreaService: UpdateHrField");
                throw;
            }
        }

        public async Task<bool> DeleteHrField(int hrAreaId, int hrFieldId)
        {
            var hrArea = await (from a in _organizationDBContext.HrArea
                                join t in _organizationDBContext.HrType on a.HrTypeId equals t.HrTypeId
                                where a.HrAreaId == hrAreaId
                                select new
                                {
                                    t.HrTypeId,
                                    a.HrAreaId,
                                    t.HrTypeCode,
                                    a.HrAreaCode
                                }).FirstOrDefaultAsync();

            _ = hrArea ?? throw new BadRequestException(HrErrorCode.HrAreaNotFound);

            var inputField = await _organizationDBContext.HrField.FirstOrDefaultAsync(f => f.HrFieldId == hrFieldId);
            if (inputField == null)
            {

                throw new BadRequestException(HrErrorCode.HrFieldNotFound);
            }
            // Check used
            bool isUsed = _organizationDBContext.HrAreaField.Any(af => af.HrFieldId == hrFieldId);
            if (isUsed)
            {
                throw new BadRequestException(HrErrorCode.HrFieldIsUsed);
            }
            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                // Delete field
                inputField.IsDeleted = true;
                await _organizationDBContext.SaveChangesAsync();
                if (inputField.FormTypeId != (int)EnumFormType.ViewOnly)
                {
                    await _organizationDBContext.DeleteColumn(GetHrAreaTableName(hrArea.HrTypeCode, hrArea.HrAreaCode), inputField.FieldName);
                }
                // await UpdateHrValueView();
                // await UpdateHrTableType();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.HrType, inputField.HrFieldId, $"Xóa trường dữ liệu chung {inputField.Title}", inputField.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "HrAreaService: DeleteHrField");
                throw;
            }
        }

        private string GetHrAreaTableName(string hrTypeCode, string hrAreaCode)
        {
            return $"{HR_TABLE_NAME_PREFIX}_{hrTypeCode}_{hrAreaCode}";
        }

        #endregion
    }
}