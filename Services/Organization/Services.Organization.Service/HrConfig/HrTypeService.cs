using Services.Organization.Model.HrConfig;
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
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.HrConfig
{
    public interface IHrTypeService
    {
        Task<int> AddHrType(HrTypeModel data);
        Task<bool> UpdateHrType(int hrTypeId, HrTypeModel data);
        Task<int> CloneHrType(int hrTypeId);
        Task<bool> DeleteHrType(int hrTypeId);
        Task<IList<HrTypeFullModel>> GetAllHrTypes();
        Task<HrTypeFullModel> GetHrType(int hrTypeId);
        Task<HrTypeFullModel> GetHrType(string hrTypeCode);
        Task<PageData<HrTypeModel>> GetHrTypes(string keyword, int page, int size);
        Task<IList<HrTypeSimpleModel>> GetHrTypeSimpleList();

        Task<HrTypeGlobalSettingModel> GetHrGlobalSetting();
        Task<bool> UpdateHrGlobalSetting(HrTypeGlobalSettingModel data);
        Task<HrTypeBasicOutput> GetHrTypeBasicInfo(int hrTypeId);

        Task<int> HrTypeViewCreate(int hrTypeId, HrTypeViewModel model);
        Task<bool> HrTypeViewDelete(int hrTypeViewId);
        Task<IList<HrTypeViewModelList>> HrTypeViewList(int hrTypeId);
        Task<bool> HrTypeViewUpdate(int hrTypeViewId, HrTypeViewModel model);
        Task<HrTypeViewModel> GetHrTypeViewInfo(int hrTypeId, int hrTypeViewId);
    }

    public class HrTypeService : IHrTypeService
    {
        private const string HR_TABLE_NAME_PREFIX = OrganizationConstants.HR_TABLE_NAME_PREFIX;

        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IActionButtonHelperService _actionButtonHelperService;
        private readonly IRoleHelperService _roleHelperService;

        public HrTypeService(
            IMapper mapper,
            OrganizationDBContext organizationDBContext,
            ILogger<HrTypeService> logger,
            IActivityLogService activityLogService,
            IActionButtonHelperService actionButtonHelperService,
            IRoleHelperService roleHelperService)
        {
            _mapper = mapper;
            _organizationDBContext = organizationDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _actionButtonHelperService = actionButtonHelperService;
            _roleHelperService = roleHelperService;
        }

        public async Task<HrTypeFullModel> GetHrType(int hrTypeId)
        {
            var globalSetting = await GetHrGlobalSetting();

            var hrType = await _organizationDBContext.HrType
           .Where(i => i.HrTypeId == hrTypeId)
           .Include(t => t.HrArea)
           .ThenInclude(a => a.HrAreaField)
           .ThenInclude(af => af.HrField)
           .Include(t => t.HrArea)
           .ThenInclude(a => a.HrAreaField)
           .ThenInclude(af => af.HrField)
           .ProjectTo<HrTypeFullModel>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync();
            if (hrType == null)
            {
                throw new BadRequestException(HrErrorCode.HrTypeNotFound);
            }
            hrType.HrAreas = hrType.HrAreas.OrderBy(f => f.SortOrder).ToList();
            foreach (var item in hrType.HrAreas)
            {
                item.HrAreaFields = item.HrAreaFields.OrderBy(f => f.SortOrder).ToList();
            }

            hrType.GlobalSetting = globalSetting;
            return hrType;
        }


        public async Task<IList<HrTypeFullModel>> GetAllHrTypes()
        {
            var globalSetting = await GetHrGlobalSetting();

            var lst = await _organizationDBContext.HrType
           .Include(t => t.HrArea)
           .ThenInclude(a => a.HrAreaField)
           .ThenInclude(af => af.HrField)
           .Include(t => t.HrArea)
           .ThenInclude(a => a.HrAreaField)
           .ThenInclude(af => af.HrField)
           .ProjectTo<HrTypeFullModel>(_mapper.ConfigurationProvider)
           .ToListAsync();
            foreach (var item in lst)
            {
                item.GlobalSetting = globalSetting;
            }
            return lst;
        }

        public async Task<HrTypeFullModel> GetHrType(string hrTypeCode)
        {
            var globalSetting = await GetHrGlobalSetting();

            var hrType = await _organizationDBContext.HrType
           .Where(i => i.HrTypeCode == hrTypeCode)
           .Include(t => t.HrArea)
           .ThenInclude(a => a.HrAreaField)
           .ThenInclude(af => af.HrField)
           .Include(t => t.HrArea)
           .ThenInclude(a => a.HrAreaField)
           .ThenInclude(af => af.HrField)
           .ProjectTo<HrTypeFullModel>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync();

            if (hrType == null)
            {
                throw new BadRequestException(HrErrorCode.HrTypeNotFound);
            }

            hrType.GlobalSetting = globalSetting;
            return hrType;
        }

        public async Task<PageData<HrTypeModel>> GetHrTypes(string keyword, int page, int size)
        {
            var globalSetting = await GetHrGlobalSetting();
            keyword = (keyword ?? "").Trim();

            var query = _organizationDBContext.HrType.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(i => i.HrTypeCode.Contains(keyword) || i.Title.Contains(keyword));
            }

            query = query.OrderBy(c => c.Title);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var lst = await query.ProjectTo<HrTypeModel>(_mapper.ConfigurationProvider).OrderBy(t => t.SortOrder).ToListAsync();

            return (lst, total);
        }

        public async Task<IList<HrTypeSimpleModel>> GetHrTypeSimpleList()
        {
            var hrTypes = await _organizationDBContext.HrType.Where(x => !x.IsHide).ProjectTo<HrTypeSimpleProjectMappingModel>(_mapper.ConfigurationProvider).OrderBy(t => t.SortOrder).ToListAsync();

            var actions = (await _actionButtonHelperService.GetActionButtonConfigs(EnumObjectType.HrType, null)).OrderBy(t => t.SortOrder).ToList()
                .GroupBy(a => a.ObjectId)
                .ToDictionary(a => a.Key, a => a.ToList());

            var areaFields = await (
              from a in _organizationDBContext.HrArea
              join af in _organizationDBContext.HrAreaField on a.HrAreaId equals af.HrAreaId
              join f in _organizationDBContext.HrField on af.HrFieldId equals f.HrFieldId
              select new
              {
                  a.HrTypeId,
                  a.HrAreaId,
                  HrAreaTitle = a.Title,

                  af.HrAreaFieldId,
                  HrAreaFieldTitle = af.Title,
                  f.HrFieldId,
                  f.FormTypeId,
              }
              ).ToListAsync();

            var typeFields = areaFields.GroupBy(t => t.HrTypeId)
                .ToDictionary(t => t.Key, t => t.Select(f => new HrAreaFieldSimpleModel()
                {
                    HrAreaId = f.HrAreaId,
                    HrAreaTitle = f.HrAreaTitle,
                    HrAreaFieldId = f.HrAreaFieldId,
                    HrAreaFieldTitle = f.HrAreaFieldTitle,
                    HrFieldId = f.HrFieldId,
                    FormTypeId = (EnumFormType)f.FormTypeId
                }).ToList()
                );

            foreach (var item in hrTypes)
            {
                if (actions.TryGetValue(item.HrTypeId, out var _actions))
                {
                    item.ActionObjects = _actions.Cast<ActionButtonSimpleModel>().ToList();
                }

                if (typeFields.TryGetValue(item.HrTypeId, out var _fields))
                {
                    item.AreaFields = _fields;
                }
            }

            return hrTypes.Cast<HrTypeSimpleModel>().ToList();
        }

        public async Task<int> AddHrType(HrTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(0));
            var existedHr = await _organizationDBContext.HrType
                .FirstOrDefaultAsync(i => i.HrTypeCode == data.HrTypeCode || i.Title == data.Title);
            if (existedHr != null)
            {
                if (string.Compare(existedHr.HrTypeCode, data.HrTypeCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(HrErrorCode.HrCodeAlreadyExisted);
                }

                throw new BadRequestException(HrErrorCode.HrTitleAlreadyExisted);
            }

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                HrType hrType = _mapper.Map<HrType>(data);
                await _organizationDBContext.HrType.AddAsync(hrType);
                await _organizationDBContext.SaveChangesAsync();

                await _roleHelperService.GrantPermissionForAllRoles(EnumModule.Hr, EnumObjectType.HrType, hrType.HrTypeId);

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.HrType, hrType.HrTypeId, $"Thêm chứng từ {hrType.Title}", data.JsonSerialize());

                return hrType.HrTypeId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "HrTypeService: AddHrType");
                throw;
            }
        }

        private string GetStringClone(string source, int suffix = 0)
        {
            string suffixText = suffix > 0 ? string.Format("({0})", suffix) : string.Empty;
            string code = string.Format("{0}_{1}{2}", source, "Copy", suffixText);
            if (_organizationDBContext.HrType.Any(i => i.HrTypeCode == code))
            {
                suffix++;
                code = GetStringClone(source, suffix);
            }
            return code;
        }

        public async Task<int> CloneHrType(int hrTypeId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(0));
            var sourceHr = await _organizationDBContext.HrType
                .Include(i => i.HrArea)
                .Include(a => a.HrAreaField)
                .ThenInclude(f => f.HrField)
                .Where(i => i.HrTypeId == hrTypeId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (sourceHr == null)
            {
                throw new BadRequestException(HrErrorCode.SourceHrTypeNotFound);
            }

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var cloneType = new HrType
                {
                    HrTypeCode = GetStringClone(sourceHr.HrTypeCode),
                    Title = GetStringClone(sourceHr.Title),
                    HrTypeGroupId = sourceHr.HrTypeGroupId,
                    SortOrder = sourceHr.SortOrder,
                    PreLoadAction = sourceHr.PreLoadAction,
                    PostLoadAction = sourceHr.PostLoadAction,
                    AfterLoadAction = sourceHr.AfterLoadAction,
                    BeforeSubmitAction = sourceHr.BeforeSubmitAction,
                    BeforeSaveAction = sourceHr.BeforeSaveAction,
                    AfterSaveAction = sourceHr.AfterSaveAction,
                    AfterUpdateRowsJsAction = sourceHr.AfterUpdateRowsJsAction,
                };
                await _organizationDBContext.HrType.AddAsync(cloneType);
                await _organizationDBContext.SaveChangesAsync();

                foreach (var area in sourceHr.HrArea)
                {
                    var cloneArea = new HrArea
                    {
                        HrTypeId = cloneType.HrTypeId,
                        HrAreaCode = area.HrAreaCode,
                        Title = area.Title,
                        Description = area.Description,
                        IsMultiRow = area.IsMultiRow,
                        IsAddition = area.IsAddition,
                        Columns = area.Columns,
                        SortOrder = area.SortOrder
                    };
                    await _organizationDBContext.HrArea.AddAsync(cloneArea);
                    await _organizationDBContext.SaveChangesAsync();

                    await _organizationDBContext.ExecuteStoreProcedure("asp_Hr_Area_Table_Add", new[] {
                        new SqlParameter("@HrAreaTableName", GetHrAreaTableName(cloneType.HrTypeCode, cloneArea.HrAreaCode)),
                    });

                    foreach (var field in sourceHr.HrAreaField.Where(f => f.HrAreaId == area.HrAreaId).ToList())
                    {
                        var cloneField = new HrField
                        {
                            DataSize = field.HrField.DataSize,
                            DataTypeId = field.HrField.DataTypeId,
                            DecimalPlace = field.HrField.DecimalPlace,
                            FieldName = field.HrField.FieldName,
                            FormTypeId = field.HrField.FormTypeId,
                            IsImage = field.HrField.IsImage,
                            IsReadOnly = field.HrField.IsReadOnly,
                            OnBlur = field.HrField.OnBlur,
                            OnChange = field.HrField.OnChange,
                            OnClick = field.HrField.OnClick,
                            OnFocus = field.HrField.OnFocus,
                            OnKeydown = field.HrField.OnKeydown,
                            OnKeypress = field.HrField.OnKeypress,
                            Structure = field.HrField.Structure,
                            ReferenceUrl = field.HrField.ReferenceUrl,
                            RefTableCode = field.HrField.RefTableCode,
                            RefTableField = field.HrField.RefTableField,
                            RefTableTitle = field.HrField.RefTableTitle,
                            Title = field.HrField.Title,
                            Placeholder = field.HrField.Placeholder,
                            DefaultValue = field.HrField.DefaultValue,
                            SortOrder = field.HrField.SortOrder,
                            HrAreaId = cloneArea.HrAreaId
                        };

                        await _organizationDBContext.HrField.AddAsync(cloneField);
                        await _organizationDBContext.SaveChangesAsync();

                        if (cloneField.FormTypeId != (int)EnumFormType.ViewOnly)
                        {
                            await _organizationDBContext.AddColumn(GetHrAreaTableName(cloneType.HrTypeCode, cloneArea.HrAreaCode), cloneField.FieldName, (EnumDataType)cloneField.DataTypeId, cloneField.DataSize, cloneField.DecimalPlace, cloneField.DefaultValue, true);
                        }

                        var cloneAreaField = new HrAreaField
                        {
                            HrFieldId = cloneField.HrFieldId,
                            HrTypeId = cloneType.HrTypeId,
                            HrAreaId = cloneArea.HrAreaId,
                            Title = field.Title,
                            Placeholder = field.Placeholder,
                            SortOrder = field.SortOrder,
                            IsAutoIncrement = field.IsAutoIncrement,
                            IsRequire = field.IsRequire,
                            IsUnique = field.IsUnique,
                            IsHidden = field.IsHidden,
                            RegularExpression = field.RegularExpression,
                            DefaultValue = field.DefaultValue,
                            Filters = field.Filters,
                            Width = field.Width,
                            Height = field.Height,
                            TitleStyleJson = field.TitleStyleJson,
                            InputStyleJson = field.InputStyleJson,
                            OnFocus = field.OnFocus,
                            OnKeydown = field.OnKeydown,
                            OnKeypress = field.OnKeypress,
                            OnBlur = field.OnBlur,
                            OnChange = field.OnChange,
                            AutoFocus = field.AutoFocus,
                            Column = field.Column
                        };
                        await _organizationDBContext.HrAreaField.AddAsync(cloneAreaField);
                        

                    }
                }
                await _organizationDBContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.HrType, cloneType.HrTypeId, $"Thêm chứng từ hành chính nhân sự {cloneType.Title}", cloneType.JsonSerialize());
                return cloneType.HrTypeId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "HrTypeService: CloneHrType");
                throw;
            }
        }

        public async Task<bool> UpdateHrType(int hrTypeId, HrTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockHrTypeKey(hrTypeId));
            var hrType = await _organizationDBContext.HrType.FirstOrDefaultAsync(i => i.HrTypeId == hrTypeId);
            if (hrType == null)
            {
                throw new BadRequestException(HrErrorCode.HrTypeNotFound);
            }
            if (hrType.HrTypeCode != data.HrTypeCode || hrType.Title != data.Title)
            {
                var existedHr = await _organizationDBContext.HrType
                    .FirstOrDefaultAsync(i => i.HrTypeId != hrTypeId && (i.HrTypeCode == data.HrTypeCode || i.Title == data.Title));
                if (existedHr != null)
                {
                    if (string.Compare(existedHr.HrTypeCode, data.HrTypeCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new BadRequestException(HrErrorCode.HrCodeAlreadyExisted);
                    }

                    throw new BadRequestException(HrErrorCode.HrTitleAlreadyExisted);
                }
            }

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var oldHrTypeCode =  hrType.HrTypeCode; 
                var newHrTypeCode =  data.HrTypeCode;

                hrType.HrTypeCode = data.HrTypeCode;
                hrType.Title = data.Title;
                hrType.SortOrder = data.SortOrder;
                hrType.HrTypeGroupId = data.HrTypeGroupId;
                hrType.PreLoadAction = data.PreLoadAction;
                hrType.PostLoadAction = data.PostLoadAction;
                hrType.AfterLoadAction = data.AfterLoadAction;
                hrType.BeforeSubmitAction = data.BeforeSubmitAction;
                hrType.BeforeSaveAction = data.BeforeSaveAction;
                hrType.AfterSaveAction = data.AfterSaveAction;
                hrType.AfterUpdateRowsJsAction = data.AfterUpdateRowsJsAction;

                await _organizationDBContext.SaveChangesAsync();

                if (oldHrTypeCode != newHrTypeCode)
                {
                    var hrAreas = await _organizationDBContext.HrArea.Where(x => x.HrTypeId == hrTypeId).AsNoTracking().ToListAsync();
                    foreach (var hrArea in hrAreas)
                    {
                        await _organizationDBContext.ExecuteStoreProcedure("asp_Hr_Area_Table_Rename", new[] {
                        new SqlParameter("@OldHrAreaTableName", GetHrAreaTableName(oldHrTypeCode, hrArea.HrAreaCode)),
                        new SqlParameter("@NewHrAreaTableName", GetHrAreaTableName(newHrTypeCode, hrArea.HrAreaCode))
                        });
                    }
                }

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.HrType, hrType.HrTypeId, $"Cập nhật chứng từ hành chính nhân sự {hrType.Title}", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, $"HrTypeService: UpdateHrType({hrTypeId})");
                throw;
            }
        }

        public async Task<bool> DeleteHrType(int hrTypeId)
        {
            var hrType = await _organizationDBContext.HrType.FirstOrDefaultAsync(i => i.HrTypeId == hrTypeId);
            if (hrType == null)
            {
                throw new BadRequestException(HrErrorCode.HrTypeNotFound);
            }

            await _organizationDBContext.ExecuteStoreProcedure("asp_HrType_Delete", new[] {
                    new SqlParameter("@HrTypeId",hrTypeId ),
                    new SqlParameter("@TableNamePrefix",HR_TABLE_NAME_PREFIX ),
                    new SqlParameter("@ResStatus",0){ Direction = ParameterDirection.Output },
                    });

            try
            {
                await _actionButtonHelperService.DeleteActionButtonsByType(EnumObjectType.HrType, hrTypeId, hrType.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"HrTypeService: DeleteHrType({hrTypeId})");
            }
            await _activityLogService.CreateLog(EnumObjectType.HrType, hrType.HrTypeId, $"Xóa chứng từ hành chính nhân sự {hrType.Title}", hrType.JsonSerialize());
            return true;
        }

        public async Task<HrTypeGlobalSettingModel> GetHrGlobalSetting()
        {
            var hrTypeSetting = await _organizationDBContext.HrTypeGlobalSetting.FirstOrDefaultAsync();
            if (hrTypeSetting == null)
            {
                hrTypeSetting = new HrTypeGlobalSetting();
            }

            return _mapper.Map<HrTypeGlobalSettingModel>(hrTypeSetting);
        }

        public async Task<bool> UpdateHrGlobalSetting(HrTypeGlobalSettingModel data)
        {
            var inputTypeSetting = await _organizationDBContext.HrTypeGlobalSetting.FirstOrDefaultAsync();
            if (inputTypeSetting == null)
            {
                inputTypeSetting = new HrTypeGlobalSetting();
            }

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(data, inputTypeSetting);
                if (inputTypeSetting.HrTypeGlobalSettingId <= 0)
                {
                    _organizationDBContext.HrTypeGlobalSetting.Add(inputTypeSetting);
                }

                await _organizationDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.HrTypeGlobalSetting, 0, $"Cập nhật cấu hình chung chứng từ hành chính nhân sự", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "HrTypeService: UpdateHrGlobalSetting");
                throw;
            }
        }

        private string GetHrAreaTableName(string hrTypeCode, string hrAreaCode)
        {
            return $"{HR_TABLE_NAME_PREFIX}_{hrTypeCode}_{hrAreaCode}";
        }

        public async Task<HrTypeBasicOutput> GetHrTypeBasicInfo(int hrTypeId)
        {
            var hrTypeInfo = await _organizationDBContext.HrType.AsNoTracking().Where(t => t.HrTypeId == hrTypeId).ProjectTo<HrTypeBasicOutput>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            hrTypeInfo.Areas = await _organizationDBContext.HrArea.AsNoTracking().Where(a => a.HrTypeId == hrTypeId).OrderBy(a => a.SortOrder).ProjectTo<HrAreaBasicOutput>(_mapper.ConfigurationProvider).ToListAsync();

            var fields = await (
                from af in _organizationDBContext.HrAreaField
                join f in _organizationDBContext.HrField on af.HrFieldId equals f.HrFieldId
                where af.HrTypeId == hrTypeId
                orderby af.SortOrder
                select new HrAreaFieldBasicOutput
                {
                    HrAreaId = af.HrAreaId,
                    HrAreaFieldId = af.HrAreaFieldId,
                    FieldName = f.FieldName,
                    Title = af.Title,
                    Placeholder = af.Placeholder,
                    DataTypeId = (EnumDataType)f.DataTypeId,
                    DataSize = f.DataSize,
                    FormTypeId = (EnumFormType)f.FormTypeId,
                    DefaultValue = af.DefaultValue,
                    RefTableCode = f.RefTableCode,
                    RefTableField = f.RefTableField,
                    RefTableTitle = f.RefTableTitle,
                    IsRequire = af.IsRequire,
                    DecimalPlace = f.DecimalPlace,
                    ReferenceUrlExec = string.IsNullOrWhiteSpace(af.ReferenceUrl) ? f.ReferenceUrl : af.ReferenceUrl

                }).ToListAsync();

            var views = await _organizationDBContext.HrTypeView.AsNoTracking().Where(t => t.HrTypeId == hrTypeId).OrderByDescending(v => v.IsDefault).ProjectTo<HrTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();

            foreach (var item in hrTypeInfo.Areas)
            {
                item.Fields = fields.Where(f => f.HrAreaId == item.HrAreaId).ToList();
            }

            hrTypeInfo.Views = views;

            return hrTypeInfo;
        }

        #region InputTypeView
        public async Task<IList<HrTypeViewModelList>> HrTypeViewList(int hrTypeId)
        {
            return await _organizationDBContext.HrTypeView.Where(v => v.HrTypeId == hrTypeId).ProjectTo<HrTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<HrTypeViewModel> GetHrTypeViewInfo(int hrTypeId, int hrTypeViewId)
        {
            var info = await _organizationDBContext.HrTypeView.AsNoTracking().Where(t => t.HrTypeId == hrTypeId && t.HrTypeViewId == hrTypeViewId).ProjectTo<HrTypeViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình trong hệ thống");
            }

            var fields = await _organizationDBContext.HrTypeViewField.AsNoTracking()
                .Where(t => t.HrTypeViewId == hrTypeViewId)
                .ProjectTo<HrTypeViewFieldModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            info.Fields = fields;

            return info;
        }

        public async Task<int> HrTypeViewCreate(int hrTypeId, HrTypeViewModel model)
        {
            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var hrTypeInfo = await _organizationDBContext.HrType.FirstOrDefaultAsync(t => t.HrTypeId == hrTypeId);
                if (hrTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

                var info = _mapper.Map<HrTypeView>(model);

                info.HrTypeId = hrTypeId;

                await _organizationDBContext.HrTypeView.AddAsync(info);
                await _organizationDBContext.SaveChangesAsync();

                await HrTypeViewFieldAddRange(info.HrTypeViewId, model.Fields);

                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.HrTypeView, info.HrTypeViewId, $"Tạo bộ lọc {info.HrTypeViewName} cho chứng từ  {hrTypeInfo.Title}", model.JsonSerialize());

                return info.HrTypeViewId;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "HrTypeViewCreate");
                throw;
            }
        }

        public async Task<bool> HrTypeViewUpdate(int hrTypeViewId, HrTypeViewModel model)
        {
            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {

                var info = await _organizationDBContext.HrTypeView.FirstOrDefaultAsync(v => v.HrTypeViewId == hrTypeViewId);

                if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "View không tồn tại");

                var inputTypeInfo = await _organizationDBContext.HrType.FirstOrDefaultAsync(t => t.HrTypeId == info.HrTypeId);
                if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ hành chính nhân sự ");


                _mapper.Map(model, info);

                var oldFields = await _organizationDBContext.HrTypeViewField.Where(f => f.HrTypeViewId == hrTypeViewId).ToListAsync();

                _organizationDBContext.HrTypeViewField.RemoveRange(oldFields);

                await HrTypeViewFieldAddRange(hrTypeViewId, model.Fields);

                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.HrTypeView, info.HrTypeViewId, $"Cập nhật bộ lọc {info.HrTypeViewName} cho chứng từ hành chính nhân sự  {inputTypeInfo.Title}", model.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "HrTypeService: HrTypeViewUpdate");
                throw;
            }
        }

        public async Task<bool> HrTypeViewDelete(int hrTypeViewId)
        {
            var info = await _organizationDBContext.HrTypeView.FirstOrDefaultAsync(v => v.HrTypeViewId == hrTypeViewId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "View không tồn tại");

            info.IsDeleted = true;
            info.DeletedDatetimeUtc = DateTime.UtcNow;


            var inputTypeInfo = await _organizationDBContext.HrType.FirstOrDefaultAsync(t => t.HrTypeId == info.HrTypeId);
            if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ hành chính nhân sự ");


            await _organizationDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.HrTypeView, info.HrTypeViewId, $"Xóa bộ lọc {info.HrTypeViewName} chứng từ hành chính nhân sự  {inputTypeInfo.Title}", new { hrTypeViewId }.JsonSerialize());

            return true;

        }

        private async Task HrTypeViewFieldAddRange(int inputTypeViewId, IList<HrTypeViewFieldModel> fieldModels)
        {
            var fields = fieldModels.Select(f => _mapper.Map<HrTypeViewField>(f)).ToList();

            foreach (var f in fields)
            {
                f.HrTypeViewId = inputTypeViewId;
            }

            await _organizationDBContext.HrTypeViewField.AddRangeAsync(fields);

        }
        #endregion HrTypeView
    }
}