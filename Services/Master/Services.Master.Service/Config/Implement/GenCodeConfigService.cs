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
using NPOI.OpenXmlFormats.Dml;
using VErp.Infrastructure.EF.EFExtensions;
using Verp.Cache.RedisCache;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class GenCodeConfigService : IGenCodeConfigService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;

        public GenCodeConfigService(MasterDBContext masterDbContext
            , IOptions<AppSetting> appSetting
            , ILogger<ObjectGenCodeService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService

        )
        {
            _masterDbContext = masterDbContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
        }

        public async Task<PageData<CustomGenCodeOutputModel>> GetList(string keyword = "", int page = 1, int size = 10)
        {
            var query = from ogc in _masterDbContext.CustomGenCode.AsNoTracking()
                        where ogc.IsActived
                        select ogc;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.CustomGenCodeName.Contains(keyword) || q.CodeFormat.Contains(keyword) || q.BaseFormat.Contains(keyword)
                        select q;
            }

            var total = await query.CountAsync();
            var objList = size > 0 ? await query.OrderByDescending(c => c.IsDefault).ThenBy(c => c.SortOrder).Skip((page - 1) * size).Take(size).ToListAsync() : await query.OrderByDescending(c => c.IsDefault).ThenBy(c => c.SortOrder).ToListAsync();

            var customGenCodeIds = objList.Select(c => c.CustomGenCodeId).ToList();

            var lastValues = (await _masterDbContext.CustomGenCodeValue.Where(c => customGenCodeIds.Contains(c.CustomGenCodeId)).AsNoTracking().ToListAsync())
                .GroupBy(c => c.CustomGenCodeId)
                .ToDictionary(c => c.Key, c => c.Select(l =>
                {
                    return new CustomGenCodeBaseValueModel
                    {
                        CustomGenCodeId = c.Key,
                        BaseValue = l.BaseValue,
                        LastValue = l.LastValue,
                        LastCode = l.LastCode
                    };
                }).ToList());

            var pagedData = new List<CustomGenCodeOutputModel>();
            foreach (var item in objList)
            {
                lastValues.TryGetValue(item.CustomGenCodeId, out var lastBaseValues);
                var info = new CustomGenCodeOutputModel()
                {
                    CustomGenCodeId = item.CustomGenCodeId,
                    ParentId = item.ParentId,
                    CustomGenCodeName = item.CustomGenCodeName,
                    Description = item.Description,
                    CodeLength = item.CodeLength,
                    //Prefix = item.Prefix,
                    //Suffix = item.Suffix,
                    LastCode = item.LastCode,
                    IsActived = item.IsActived,
                    UpdatedUserId = item.UpdatedUserId,
                    CreatedTime = item.CreatedTime != null ? ((DateTime)item.CreatedTime).GetUnix() : 0,
                    UpdatedTime = item.UpdatedTime != null ? ((DateTime)item.UpdatedTime).GetUnix() : 0,
                    SortOrder = item.SortOrder,
                    IsDefault = item.IsDefault,
                    LastValues = lastBaseValues,
                    BaseFormat = item.BaseFormat,
                    CodeFormat = item.CodeFormat
                };
                pagedData.Add(info);
            }
            return (pagedData, total);
        }

        public async Task<CustomGenCodeOutputModel> GetInfo(int customGenCodeId, long? fId, string code, long? date)
        {

            var obj = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(p => p.CustomGenCodeId == customGenCodeId);
            if (obj == null)
            {
                throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
            }

            var stringNumber = string.Empty;
            for (var i = 0; i < obj.CodeLength; i++)
            {
                stringNumber += "x";

            }

            var newCode = Utils.FormatStyle(obj.CodeFormat, code, fId, date, stringNumber);

            var lastValues = await _masterDbContext.CustomGenCodeValue.Where(c => customGenCodeId == c.CustomGenCodeId).AsNoTracking()
                .Select(l => new CustomGenCodeBaseValueModel
                {
                    CustomGenCodeId = customGenCodeId,
                    BaseValue = l.BaseValue,
                    LastValue = l.LastValue,
                    LastCode = l.LastCode,
                    Example = newCode
                }).ToListAsync();

            var lastValueEntity = (await FindBaseValue(customGenCodeId, obj.BaseFormat, fId, code, date)).data;
            var currentLastValue = new CustomGenCodeBaseValueModel()
            {
                CustomGenCodeId = customGenCodeId,
                BaseValue = lastValueEntity.BaseValue,
                LastValue = lastValueEntity.LastValue,
                LastCode = lastValueEntity.LastCode,
                Example = newCode
            };

            var info = new CustomGenCodeOutputModel()
            {
                CustomGenCodeId = obj.CustomGenCodeId,
                ParentId = obj.ParentId,
                CustomGenCodeName = obj.CustomGenCodeName,
                Description = obj.Description,
                CodeLength = obj.CodeLength,
                //Prefix = obj.Prefix,
                //Suffix = obj.Suffix,
                LastCode = obj.LastCode,
                IsActived = obj.IsActived,
                UpdatedUserId = obj.UpdatedUserId,
                CreatedTime = obj.CreatedTime != null ? ((DateTime)obj.CreatedTime).GetUnix() : 0,
                UpdatedTime = obj.UpdatedTime != null ? ((DateTime)obj.UpdatedTime).GetUnix() : 0,
                SortOrder = obj.SortOrder,
                IsDefault = obj.IsDefault,
                LastValues = lastValues,
                CurrentLastValue = currentLastValue,
                BaseFormat = obj.BaseFormat,
                CodeFormat = obj.CodeFormat
            };
            return info;
        }

        public async Task<bool> Update(int customGenCodeId, CustomGenCodeInputModel model)
        {

            var obj = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(p => p.CustomGenCodeId == customGenCodeId);

            if (obj == null)
            {
                throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
            }
            obj.BaseFormat = model.BaseFormat;
            obj.CodeFormat = model.CodeFormat;
            obj.ParentId = model.ParentId;
            obj.CustomGenCodeName = model.CustomGenCodeName;
            obj.CodeLength = model.CodeLength;
            //obj.Prefix = model.Prefix;
            //obj.Suffix = model.Suffix;
            obj.Description = model.Description;
            obj.UpdatedUserId = _currentContextService.UserId;
            obj.UpdatedTime = DateTime.UtcNow;

            obj.SortOrder = model.SortOrder;

            obj.IsDefault = model.IsDefault;


            if (model.LastValues != null)
            {
                var lastValues = await _masterDbContext.CustomGenCodeValue.Where(c => customGenCodeId == c.CustomGenCodeId).ToListAsync();

                foreach (var item in model.LastValues)
                {
                    var editted = lastValues.FirstOrDefault(l => l.BaseValue == item.BaseValue);
                    if (editted != null)
                    {
                        item.LastValue = editted.LastValue;
                    }
                    else
                    {
                        _masterDbContext.CustomGenCodeValue.Add(new CustomGenCodeValue()
                        {
                            CustomGenCodeId = customGenCodeId,
                            BaseValue = item.BaseValue,
                            LastCode = string.Empty,
                            LastValue = item.LastValue,
                            TempCode = "",
                            TempValue = null
                        });
                    }
                }
            }

            await _masterDbContext.SaveChangesAsync();

            if (obj.IsDefault)
            {
                await _masterDbContext.CustomGenCode.Where(c => c.CustomGenCodeId != customGenCodeId)
                    .UpdateByBatch(c => new CustomGenCode()
                    {
                        IsDefault = false
                    });
            }

            await UpdateSortOrder();

            await _activityLogService.CreateLog(EnumObjectType.CustomGenCodeConfig, obj.CustomGenCodeId, $"Cập nhật cấu hình sinh mã tùy chọn cho {obj.CustomGenCodeName} ", model.JsonSerialize());
            return true;

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

        public async Task<bool> Delete(int customGenCodeId)
        {

            var obj = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(p => p.CustomGenCodeId == customGenCodeId);
            if (obj == null)
            {
                throw new BadRequestException(ObjectGenCodeErrorCode.ConfigNotFound);
            }
            obj.IsDeleted = true;
            obj.UpdatedUserId = _currentContextService.UserId;
            obj.UpdatedTime = DateTime.UtcNow;


            await _masterDbContext.SaveChangesAsync();
            await UpdateSortOrder();

            await _activityLogService.CreateLog(EnumObjectType.CustomGenCodeConfig, obj.CustomGenCodeId, $"Xoá cấu hình gen code tùy chọn cho {obj.CustomGenCodeName} ", obj.JsonSerialize());


            return true;

        }

        public async Task<int> Create(CustomGenCodeInputModel model)
        {

            if (_masterDbContext.CustomGenCode.Any(q => q.CustomGenCodeName == model.CustomGenCodeName))
            {
                throw new BadRequestException(ObjectGenCodeErrorCode.ConfigAlreadyExisted);
            }

            var entity = new CustomGenCode()
            {
                CustomGenCodeName = model.CustomGenCodeName,
                CodeLength = (model.CodeLength > 5) ? model.CodeLength : 5,
                //Prefix = model.Prefix ?? string.Empty,
                //Suffix = model.Suffix ?? string.Empty,
                Description = model.Description,
                BaseFormat = model.BaseFormat,
                CodeFormat = model.CodeFormat,
                //DateFormat = string.Empty,
                LastCode = string.Empty,
                IsActived = true,
                IsDefault = model.IsDefault,
                IsDeleted = false,
                UpdatedUserId = _currentContextService.UserId,
                ResetDate = DateTime.UtcNow,
                CreatedTime = DateTime.UtcNow,
                UpdatedTime = DateTime.UtcNow
            };
            _masterDbContext.CustomGenCode.Add(entity);

            await _masterDbContext.SaveChangesAsync();

            if (model.LastValues != null)
            {
                foreach (var item in model.LastValues)
                {
                    _masterDbContext.CustomGenCodeValue.Add(new CustomGenCodeValue()
                    {
                        CustomGenCodeId = entity.CustomGenCodeId,
                        BaseValue = item.BaseValue,
                        LastCode = string.Empty,
                        LastValue = item.LastValue,
                        TempCode = "",
                        TempValue = null
                    });
                }
            }


            await _masterDbContext.SaveChangesAsync();

            if (model.IsDefault)
            {
                await _masterDbContext.CustomGenCode.Where(c => c.CustomGenCodeId != entity.CustomGenCodeId)
                    .UpdateByBatch(c => new CustomGenCode()
                    {
                        IsDefault = false
                    });
            }

            await UpdateSortOrder();

            await _activityLogService.CreateLog(EnumObjectType.CustomGenCodeConfig, entity.CustomGenCodeId, $"Thêm mới cấu hình gen code tùy chọn cho {entity.CustomGenCodeName} ", model.JsonSerialize());

            return entity.CustomGenCodeId;

        }

        private async Task<(bool isFound, CustomGenCodeValue data)> FindBaseValue(int customGenCodeId, string baseFormat, long? fId = null, string code = "", long? date = null)
        {
            var baseValue = Utils.FormatStyle(baseFormat, code, fId, date, null);
            var baseValueEntity = await _masterDbContext.CustomGenCodeValue.FirstOrDefaultAsync(c => c.CustomGenCodeId == customGenCodeId && c.BaseValue == baseValue);
            if (baseValueEntity == null)
            {
                baseValueEntity = new CustomGenCodeValue()
                {
                    CustomGenCodeId = customGenCodeId,
                    BaseValue = baseValue,
                    LastCode = "",
                    LastValue = 0,
                    TempCode = "",
                    TempValue = null
                };
                return (false, baseValueEntity);
            }
            else
            {
                return (true, baseValueEntity);
            }


        }

        public async Task<CustomCodeGeneratedModel> GenerateCode(int customGenCodeId, int lastValue, long? fId = null, string code = "", long? date = null)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockGenerateCodeCustomKey(customGenCodeId)))
            {
                CustomCodeGeneratedModel result;

                using (var trans = await _masterDbContext.Database.BeginTransactionAsync())
                {
                    var config = await _masterDbContext.CustomGenCode
                        .FirstOrDefaultAsync(q => q.CustomGenCodeId == customGenCodeId);

                    if (config == null)
                    {
                        trans.Rollback();
                        throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
                    }

                    var (isExisted, baseValueEntity) = await FindBaseValue(customGenCodeId, config.BaseFormat, fId, code, date);

                    if (!isExisted)
                    {
                        _masterDbContext.CustomGenCodeValue.Add(baseValueEntity);
                        await _masterDbContext.SaveChangesAsync();
                    }


                    string newCode = string.Empty;
                    var newId = 0;
                    var maxId = (int)Math.Pow(10, config.CodeLength);

                    lastValue = lastValue > baseValueEntity.LastValue ? lastValue : baseValueEntity.LastValue;

                    if (lastValue < 1)
                    {
                        newId = 1;
                    }
                    else
                    {
                        newId = lastValue + 1;
                    }

                    var stringNumber = newId < maxId ? newId.ToString(string.Format("D{0}", config.CodeLength)) : newId.ToString(string.Format("D{0}", config.CodeLength + 1));
                    newCode = Utils.FormatStyle(config.CodeFormat, code, fId, date, stringNumber);


                    if (!(newId < maxId))
                    {
                        config.CodeLength += 1;
                        config.ResetDate = DateTime.UtcNow;
                    }
                    config.TempValue = newId;
                    config.TempCode = newCode;

                    _masterDbContext.SaveChanges();
                    trans.Commit();

                    result = new CustomCodeGeneratedModel
                    {
                        CustomCode = newCode,
                        LastValue = newId,
                        BaseValue = baseValueEntity.BaseValue,
                        CustomGenCodeId = config.CustomGenCodeId,
                    };
                }

                return result;
            }
        }

        public async Task<bool> ConfirmCode(int customGenCodeId, string baseValue)
        {

            var config = await _masterDbContext.CustomGenCodeValue.FirstOrDefaultAsync(m => m.CustomGenCodeId == customGenCodeId && m.BaseValue == baseValue);

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

    }
}
