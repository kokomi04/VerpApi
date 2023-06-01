using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Master.Config.GenCodeConfig;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class GenCodeConfigService : IGenCodeConfigService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly ObjectActivityLogFacade _genCodeConfigActivityLog;

        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        public GenCodeConfigService(MasterDBContext masterDbContext
            , IOptions<AppSetting> appSetting
            , ILogger<ObjectGenCodeService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            , IMapper mapper
        )
        {
            _masterDbContext = masterDbContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _currentContextService = currentContextService;
            _genCodeConfigActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.CustomGenCodeConfig);
            _mapper = mapper;
        }

        public async Task<PageData<CustomGenCodeOutputModel>> GetList(string keyword = "", int page = 1, int size = 10)
        {
            keyword = (keyword ?? "").Trim();

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
                    //LastValues = lastBaseValues,
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

            var newCode = StringUtils.FormatStyle(obj.CodeFormat, code, fId, date.UnixToDateTime(_currentContextService.TimeZoneOffset), stringNumber);

            //var lastValues = await _masterDbContext.CustomGenCodeValue.Where(c => customGenCodeId == c.CustomGenCodeId).AsNoTracking()
            //    .Select(l => new CustomGenCodeBaseValueModel
            //    {
            //        CustomGenCodeId = customGenCodeId,
            //        BaseValue = l.BaseValue,
            //        LastValue = l.LastValue,
            //        LastCode = l.LastCode,
            //        Example = newCode
            //    }).ToListAsync();

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
                //LastValues = lastValues,
                CurrentLastValue = currentLastValue,
                BaseFormat = obj.BaseFormat,
                CodeFormat = obj.CodeFormat
            };
            return info;
        }

        public async Task<PageData<CustomGenCodeBaseValueModel>> GetBaseValues(int customGenCodeId, long? fId, string code, long? date, int page, int size)
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

            var newCode = StringUtils.FormatStyle(obj.CodeFormat, code, fId, date.UnixToDateTime(_currentContextService.TimeZoneOffset), stringNumber);

            var query = _masterDbContext.CustomGenCodeValue.Where(c => customGenCodeId == c.CustomGenCodeId).AsNoTracking().OrderBy(c => c.BaseValue).AsQueryable();

            var total = await query.CountAsync();

            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }

            var pagedData = await query
                .Select(l => new CustomGenCodeBaseValueModel
                {
                    CustomGenCodeId = customGenCodeId,
                    BaseValue = l.BaseValue,
                    LastValue = l.LastValue,
                    LastCode = l.LastCode,
                    Example = newCode
                }).ToListAsync();

            return (pagedData, total);

        }


        public async Task<bool> Update(int customGenCodeId, CustomGenCodeInputModel model)
        {

            var obj = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(p => p.CustomGenCodeId == customGenCodeId);

            if (obj == null)
            {
                throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
            }

            _mapper.Map(model, obj);

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

            await _genCodeConfigActivityLog.LogBuilder(() => GenCodeConfigActivityLogMessage.Update)
             .MessageResourceFormatDatas(obj.CustomGenCodeName)
             .ObjectId(obj.CustomGenCodeId)
             .JsonData(model.JsonSerialize())
             .CreateLog();

            return true;

        }

        public async Task<bool> SetLastValue(int customGenCodeId, CustomGenCodeBaseValueModel model)
        {
            if (string.IsNullOrWhiteSpace(model.BaseValue)) model.BaseValue = string.Empty;

            var obj = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(p => p.CustomGenCodeId == customGenCodeId);
            if (obj == null)
            {
                throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
            }

            var (isExisted, baseValueEntity) = await FindBaseValue(customGenCodeId, model.BaseValue, null, null, null);

            baseValueEntity.LastValue = model.LastValue;
            if (!isExisted)
            {
                _masterDbContext.CustomGenCodeValue.Add(baseValueEntity);
            }

            await _masterDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteLastValue(int customGenCodeId, string baseValue)
        {
            var obj = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(p => p.CustomGenCodeId == customGenCodeId);
            if (obj == null)
            {
                throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
            }

            var baseValueEntity = await _masterDbContext.CustomGenCodeValue.FirstOrDefaultAsync(c => c.CustomGenCodeId == customGenCodeId && c.BaseValue == baseValue);


            if (baseValueEntity != null)
            {
                _masterDbContext.CustomGenCodeValue.Remove(baseValueEntity);
            }

            await _masterDbContext.SaveChangesAsync();

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

            await _genCodeConfigActivityLog.LogBuilder(() => GenCodeConfigActivityLogMessage.Delete)
             .MessageResourceFormatDatas(obj.CustomGenCodeName)
             .ObjectId(obj.CustomGenCodeId)
             .JsonData(obj.JsonSerialize())
             .CreateLog();



            return true;

        }

        public async Task<int> Create(CustomGenCodeInputModel model)
        {

            if (_masterDbContext.CustomGenCode.Any(q => q.CustomGenCodeName == model.CustomGenCodeName))
            {
                throw new BadRequestException(ObjectGenCodeErrorCode.ConfigAlreadyExisted);
            }

            var entity = _mapper.Map<CustomGenCode>(model);
            entity.LastCode = string.Empty;
            entity.IsActived = true;

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

            await _genCodeConfigActivityLog.LogBuilder(() => GenCodeConfigActivityLogMessage.Update)
             .MessageResourceFormatDatas(model.CustomGenCodeName)
             .ObjectId(entity.CustomGenCodeId)
             .JsonData(model.JsonSerialize())
             .CreateLog();


            return entity.CustomGenCodeId;

        }

        private async Task<(bool isFound, CustomGenCodeValue data)> FindBaseValue(int customGenCodeId, string baseFormat, long? fId = null, string code = "", long? date = null)
        {
            var baseValue = StringUtils.FormatStyle(baseFormat, code, fId, date.UnixToDateTime(_currentContextService.TimeZoneOffset), null);
            baseValue ??= string.Empty;
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
                    if (date == null)
                    {
                        date = _currentContextService.GetNowUtc().GetUnix();
                    }

                    var (isExisted, baseValueEntity) = await FindBaseValue(customGenCodeId, config.BaseFormat, fId, code, date);

                    if (string.IsNullOrWhiteSpace(baseValueEntity.BaseValue)) baseValueEntity.BaseValue = string.Empty;

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

                    date = date.HasValue ? date : _currentContextService.GetNowUtc().GetUnix();

                    newCode = StringUtils.FormatStyle(config.CodeFormat, code, fId, date.UnixToDateTime(_currentContextService.TimeZoneOffset), stringNumber);


                    if (!(newId < maxId))
                    {
                        config.CodeLength += 1;
                        config.ResetDate = DateTime.UtcNow;
                    }
                    baseValueEntity.TempValue = newId;
                    baseValueEntity.TempCode = newCode;

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
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockGenerateCodeCustomKey(customGenCodeId)))
            {
                if (string.IsNullOrWhiteSpace(baseValue)) baseValue = string.Empty;

                var config = await _masterDbContext.CustomGenCodeValue.FirstOrDefaultAsync(m => m.CustomGenCodeId == customGenCodeId && m.BaseValue == baseValue);

                if (config == null)
                {
                    throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
                }
                if (config.TempValue.HasValue && config.TempValue.Value > config.LastValue)
                {
                    config.LastValue = config.TempValue.Value;
                    config.LastCode = config.TempCode;
                    await _masterDbContext.SaveChangesAsync();
                }
                return true;
            }
        }


    }
}
