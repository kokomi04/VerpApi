using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Activity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Service;
using NPOI.SS.Formula.Functions;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class CustomGenCodeService : ICustomGenCodeService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public CustomGenCodeService(MasterDBContext masterDbContext
            , IOptions<AppSetting> appSetting
            , ILogger<ObjectGenCodeService> logger
            , IActivityLogService activityLogService

        )
        {
            _masterDbContext = masterDbContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;

        }

        public async Task<PageData<CustomGenCodeOutputModel>> GetList(string keyword = "", int page = 1, int size = 10)
        {
            var query = from ogc in _masterDbContext.CustomGenCode
                        where ogc.IsActived
                        select ogc;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.CustomGenCodeName.Contains(keyword) || q.Prefix.Contains(keyword) || q.Suffix.Contains(keyword) || q.Seperator.Contains(keyword)
                        select q;
            }

            var total = await query.CountAsync();
            var objList = size > 0 ? await query.OrderBy(c => c.SortOrder).Skip((page - 1) * size).Take(size).ToListAsync() : await query.OrderBy(c => c.SortOrder).ToListAsync();

            var pagedData = new List<CustomGenCodeOutputModel>();
            foreach (var item in objList)
            {
                var info = new CustomGenCodeOutputModel()
                {
                    CustomGenCodeId = item.CustomGenCodeId,
                    ParentId = item.ParentId,
                    CustomGenCodeName = item.CustomGenCodeName,
                    Description = item.Description,
                    CodeLength = item.CodeLength,
                    Prefix = item.Prefix,
                    Suffix = item.Suffix,
                    Seperator = item.Seperator,
                    LastValue = item.LastValue,
                    LastCode = item.LastCode,
                    IsActived = item.IsActived,
                    UpdatedUserId = item.UpdatedUserId,
                    CreatedTime = item.CreatedTime != null ? ((DateTime)item.CreatedTime).GetUnix() : 0,
                    UpdatedTime = item.UpdatedTime != null ? ((DateTime)item.UpdatedTime).GetUnix() : 0,
                    SortOrder = item.SortOrder
                };
                pagedData.Add(info);
            }
            return (pagedData, total);
        }

        public async Task<ServiceResult<CustomGenCodeOutputModel>> GetInfo(int customGenCodeId)
        {

            var obj = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(p => p.CustomGenCodeId == customGenCodeId);
            if (obj == null)
            {
                return CustomGenCodeErrorCode.CustomConfigNotFound;
            }
            var info = new CustomGenCodeOutputModel()
            {
                CustomGenCodeId = obj.CustomGenCodeId,
                ParentId = obj.ParentId,
                CustomGenCodeName = obj.CustomGenCodeName,
                Description = obj.Description,
                CodeLength = obj.CodeLength,
                Prefix = obj.Prefix,
                Suffix = obj.Suffix,
                Seperator = obj.Seperator,
                LastValue = obj.LastValue,
                LastCode = obj.LastCode,
                IsActived = obj.IsActived,
                UpdatedUserId = obj.UpdatedUserId,
                CreatedTime = obj.CreatedTime != null ? ((DateTime)obj.CreatedTime).GetUnix() : 0,
                UpdatedTime = obj.UpdatedTime != null ? ((DateTime)obj.UpdatedTime).GetUnix() : 0,
                SortOrder = obj.SortOrder
            };
            return info;
        }

        public async Task<CustomGenCodeOutputModel> GetCurrentConfig(int objectTypeId, int objectId)
        {
            var obj = await _masterDbContext.ObjectCustomGenCodeMapping
                .Join(_masterDbContext.CustomGenCode, m => m.CustomGenCodeId, c => c.CustomGenCodeId, (m, c) => new
                {
                    ObjectCustomGenCodeMapping = m,
                    CustomGenCodeId = c
                })
                .Where(q => q.ObjectCustomGenCodeMapping.ObjectTypeId == objectTypeId
                && q.ObjectCustomGenCodeMapping.ObjectId == objectId
                && q.CustomGenCodeId.IsActived
                && !q.CustomGenCodeId.IsDeleted)
                .Select(q => q.CustomGenCodeId)
                .FirstOrDefaultAsync();

            if (obj == null)
            {
                throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotExisted);
            }

            var info = new CustomGenCodeOutputModel()
            {
                CustomGenCodeId = obj.CustomGenCodeId,
                ParentId = obj.ParentId,
                CustomGenCodeName = obj.CustomGenCodeName,
                Description = obj.Description,
                CodeLength = obj.CodeLength,
                Prefix = obj.Prefix,
                Suffix = obj.Suffix,
                Seperator = obj.Seperator,
                LastValue = obj.LastValue,
                LastCode = obj.LastCode,
                IsActived = obj.IsActived,
                UpdatedUserId = obj.UpdatedUserId,
                CreatedTime = obj.CreatedTime != null ? ((DateTime)obj.CreatedTime).GetUnix() : 0,
                UpdatedTime = obj.UpdatedTime != null ? ((DateTime)obj.UpdatedTime).GetUnix() : 0,
                SortOrder = obj.SortOrder
            };
            return info;
        }

        public async Task<Enum> Update(int customGenCodeId, int currentUserId, CustomGenCodeInputModel model)
        {
            try
            {
                var obj = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(p => p.CustomGenCodeId == customGenCodeId);

                if (obj == null)
                {
                    return CustomGenCodeErrorCode.CustomConfigNotFound;
                }
                obj.ParentId = model.ParentId;
                obj.CustomGenCodeName = model.CustomGenCodeName;
                obj.CodeLength = model.CodeLength;
                obj.Prefix = model.Prefix;
                obj.Suffix = model.Suffix;
                obj.Seperator = model.Seperator;
                obj.Description = model.Description;
                obj.UpdatedUserId = currentUserId;
                obj.UpdatedTime = DateTime.UtcNow;

                obj.LastValue = model.LastValue;
                obj.SortOrder = model.SortOrder;

                await _masterDbContext.SaveChangesAsync();
                await UpdateSortOrder();

                await _activityLogService.CreateLog(EnumObjectType.CustomGenCodeConfig, obj.CustomGenCodeId, $"Cập nhật cấu hình gen code tùy chọn cho {obj.CustomGenCodeName} ", model.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }


        private async Task UpdateSortOrder()
        {
            var lst = await _masterDbContext.CustomGenCode.OrderBy(c => c.SortOrder).ToListAsync();
            var st = new Stack<CustomGenCode>();
            st.Push(null);
            var idx = 0;
            while (st.Count > 0)
            {
                var customCode = st.Pop();
                if (customCode != null)
                {
                    customCode.SortOrder = ++idx;
                }

                foreach (var child in lst.Where(c => c.ParentId == customCode?.CustomGenCodeId).Reverse())
                {
                    st.Push(child);
                }

            }

            await _masterDbContext.SaveChangesAsync();
        }

        public async Task<Enum> MapObjectCustomGenCode(int currentUserId, ObjectCustomGenCodeMapping model)
        {
            try
            {
                var config = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(c => c.IsActived && !c.IsDeleted && c.CustomGenCodeId == model.CustomGenCodeId);
                if (config == null)
                {
                    return CustomGenCodeErrorCode.CustomConfigNotFound;
                }
                var obj = await _masterDbContext.ObjectCustomGenCodeMapping.FirstOrDefaultAsync(m => m.ObjectTypeId == model.ObjectTypeId && m.ObjectId == model.ObjectId);
                if (obj == null)
                {
                    _masterDbContext.ObjectCustomGenCodeMapping.Add(model);
                }
                else
                {
                    obj.CustomGenCodeId = model.CustomGenCodeId;
                    obj.UpdatedUserId = currentUserId;
                }
                await _masterDbContext.SaveChangesAsync();
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }
        public async Task<Enum> UpdateMultiConfig(int objectTypeId, Dictionary<int, int> data)
        {
            try
            {
                foreach (var mapConfig in data)
                {
                    var config = await _masterDbContext.CustomGenCode
                        .Where(c => c.IsActived)
                        .Where(c => c.CustomGenCodeId == mapConfig.Value)
                        .FirstOrDefaultAsync();
                    if (config == null)
                    {
                        return CustomGenCodeErrorCode.CustomConfigNotFound;
                    }
                    var curMapConfig = await _masterDbContext.ObjectCustomGenCodeMapping
                        .FirstOrDefaultAsync(m => m.ObjectTypeId == objectTypeId && m.ObjectId == mapConfig.Key);
                    if (curMapConfig == null)
                    {
                        curMapConfig = new ObjectCustomGenCodeMapping
                        {
                            ObjectTypeId = objectTypeId,
                            ObjectId = mapConfig.Key,
                            CustomGenCodeId = mapConfig.Value,
                        };
                        _masterDbContext.ObjectCustomGenCodeMapping.Add(curMapConfig);
                    }
                    else if (curMapConfig.CustomGenCodeId != mapConfig.Value)
                    {
                        curMapConfig.CustomGenCodeId = mapConfig.Value;
                    }
                }
                await _masterDbContext.SaveChangesAsync();
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> Delete(int currentUserId, int customGenCodeId)
        {
            try
            {
                var obj = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(p => p.CustomGenCodeId == customGenCodeId);
                if (obj == null)
                {
                    return ObjectGenCodeErrorCode.ConfigNotFound;
                }
                obj.IsDeleted = true;
                obj.UpdatedUserId = currentUserId;
                obj.UpdatedTime = DateTime.UtcNow;


                await _masterDbContext.SaveChangesAsync();
                await UpdateSortOrder();

                await _activityLogService.CreateLog(EnumObjectType.CustomGenCodeConfig, obj.CustomGenCodeId, $"Xoá cấu hình gen code tùy chọn cho {obj.CustomGenCodeName} ", obj.JsonSerialize());


                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

        public async Task<ServiceResult<int>> Create(int currentUserId, CustomGenCodeInputModel model)
        {
            try
            {
                if (_masterDbContext.CustomGenCode.Any(q => q.CustomGenCodeName == model.CustomGenCodeName))
                {
                    return ObjectGenCodeErrorCode.ConfigAlreadyExisted;
                }

                var entity = new CustomGenCode()
                {
                    CustomGenCodeName = model.CustomGenCodeName,
                    CodeLength = (model.CodeLength > 5) ? model.CodeLength : 5,
                    Prefix = model.Prefix ?? string.Empty,
                    Suffix = model.Suffix ?? string.Empty,
                    Seperator = model.Seperator ?? string.Empty,
                    Description = model.Description,
                    DateFormat = string.Empty,
                    LastValue = model.LastValue,
                    LastCode = string.Empty,
                    IsActived = true,
                    IsDeleted = false,
                    UpdatedUserId = currentUserId,
                    ResetDate = DateTime.UtcNow,
                    CreatedTime = DateTime.UtcNow,
                    UpdatedTime = DateTime.UtcNow
                };
                _masterDbContext.CustomGenCode.Add(entity);

                await _masterDbContext.SaveChangesAsync();
                await UpdateSortOrder();

                await _activityLogService.CreateLog(EnumObjectType.CustomGenCodeConfig, entity.CustomGenCodeId, $"Thêm mới cấu hình gen code tùy chọn cho {entity.CustomGenCodeName} ", model.JsonSerialize());

                return entity.CustomGenCodeId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create");
                return GeneralCode.InternalError;
            }
        }

        public async Task<CustomCodeModel> GenerateCode(int customGenCodeId, int lastValue, string code = "")
        {
            CustomCodeModel result;
            try
            {
                using (var trans = await _masterDbContext.Database.BeginTransactionAsync())
                {
                    var config = _masterDbContext.CustomGenCode
                        .FirstOrDefault(q => q.CustomGenCodeId == customGenCodeId);

                    if (config == null)
                    {
                        trans.Rollback();
                        throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
                    }
                    string newCode = string.Empty;
                    var newId = 0;
                    var maxId = (int)Math.Pow(10, config.CodeLength);
                    var seperator = (string.IsNullOrEmpty(config.Seperator) || string.IsNullOrWhiteSpace(config.Seperator)) ? null : config.Seperator;

                    lastValue = lastValue > config.LastValue ? lastValue : config.LastValue;

                    if (lastValue < 1)
                    {
                        newId = 1;
                        var stringNewId = newId < maxId ? newId.ToString(string.Format("D{0}", config.CodeLength)) : newId.ToString(string.Format("D{0}", config.CodeLength + 1));
                        newCode = $"{config.Prefix}{seperator}{stringNewId}".Trim();
                    }
                    else
                    {
                        newId = lastValue + 1;
                        var stringNewId = newId < maxId ? newId.ToString(string.Format("D{0}", config.CodeLength)) : newId.ToString(string.Format("D{0}", config.CodeLength + 1));
                        newCode = $"{config.Prefix}{seperator}{stringNewId}".Trim();
                    }

                    newCode = Utils.FormatStyle(newCode, code, null);

                    if (!(newId < maxId))
                    {
                        config.CodeLength += 1;
                        config.ResetDate = DateTime.UtcNow;
                    }
                    config.TempValue = newId;
                    config.TempCode = newCode;

                    _masterDbContext.SaveChanges();
                    trans.Commit();

                    result = new CustomCodeModel
                    {
                        CustomCode = newCode,
                        LastValue = newId,
                        CustomGenCodeId = config.CustomGenCodeId,
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateCode");

                throw;
            }
            return result;
        }

        public async Task<PageData<ObjectType>> GetAllObjectType()
        {

            var total = await _masterDbContext.ObjectType.CountAsync();
            var allData = await _masterDbContext.ObjectType.AsNoTracking().ToListAsync();

            return (allData, total);

        }

        public async Task<bool> ConfirmCode(int objectTypeId, int objectId)
        {
            try
            {
                var config = await _masterDbContext.CustomGenCode
                    .Join(_masterDbContext.ObjectCustomGenCodeMapping, c => c.CustomGenCodeId, m => m.CustomGenCodeId, (c, m) => new
                    {
                        CustomGenCode = c,
                        m.ObjectId,
                        m.ObjectTypeId
                    })
                    .Where(cm => cm.ObjectId == objectId && cm.ObjectTypeId == objectTypeId)
                    .Select(cm => cm.CustomGenCode)
                    .FirstOrDefaultAsync();
                if (config == null)
                {
                    throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
                }
                if (config.TempValue.HasValue && config.TempValue.Value != config.LastValue)
                {
                    config.LastValue = config.TempValue.Value;
                    config.LastCode = config.TempCode;
                    await _masterDbContext.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConfirmCode");
                throw;
            }
        }
    }
}
