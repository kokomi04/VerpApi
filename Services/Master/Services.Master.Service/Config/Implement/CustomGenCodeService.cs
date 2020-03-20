﻿using System;
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
using VErp.Infrastructure.ApiCore.Model;

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
            var objList = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var pagedData = new List<CustomGenCodeOutputModel>();
            foreach (var item in objList)
            {
                var info = new CustomGenCodeOutputModel()
                {
                    CustomGenCodeId = item.CustomGenCodeId,
                    CustomGenCodeName = item.CustomGenCodeName,
                    Description = item.Description,
                    CodeLength = item.CodeLength,
                    Prefix = item.Prefix,
                    Suffix = item.Suffix,
                    Seperator = item.Seperator,
                    LastCode = item.LastCode,
                    IsActived = item.IsActived,
                    UpdatedUserId = item.UpdatedUserId,
                    CreatedTime = item.CreatedTime != null ? ((DateTime)item.CreatedTime).GetUnix() : 0,
                    UpdatedTime = item.UpdatedTime != null ? ((DateTime)item.UpdatedTime).GetUnix() : 0
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
                CustomGenCodeName = obj.CustomGenCodeName,
                Description = obj.Description,
                CodeLength = obj.CodeLength,
                Prefix = obj.Prefix,
                Suffix = obj.Suffix,
                Seperator = obj.Seperator,
                LastCode = obj.LastCode,
                IsActived = obj.IsActived,
                UpdatedUserId = obj.UpdatedUserId,
                CreatedTime = obj.CreatedTime != null ? ((DateTime)obj.CreatedTime).GetUnix() : 0,
                UpdatedTime = obj.UpdatedTime != null ? ((DateTime)obj.UpdatedTime).GetUnix() : 0
            };
            return info;
        }

        public async Task<ServiceResult<CustomGenCodeOutputModel>> GetCurrentConfig(int objectTypeId, int objectId)
        {
            var obj = await _masterDbContext.ObjectCustomGenCodeMapping
                .Join(_masterDbContext.CustomGenCode, m => m.CustomGenCodeId, c => c.CustomGenCodeId, (m, c) => new
                {
                    ObjectCustomGenCodeMapping = m,
                    CustomGenCodeId = c
                })
                .Where(q => q.ObjectCustomGenCodeMapping.ObjectTypeId == objectTypeId && q.ObjectCustomGenCodeMapping.ObjectId == objectId && q.CustomGenCodeId.IsActived && !q.CustomGenCodeId.IsDeleted)
                .Select(q => q.CustomGenCodeId)
                .FirstOrDefaultAsync();
            CustomGenCodeOutputModel info = null;
            if (obj != null)
            {
                info = new CustomGenCodeOutputModel()
                {
                    CustomGenCodeId = obj.CustomGenCodeId,
                    CustomGenCodeName = obj.CustomGenCodeName,
                    Description = obj.Description,
                    CodeLength = obj.CodeLength,
                    Prefix = obj.Prefix,
                    Suffix = obj.Suffix,
                    Seperator = obj.Seperator,
                    LastCode = obj.LastCode,
                    IsActived = obj.IsActived,
                    UpdatedUserId = obj.UpdatedUserId,
                    CreatedTime = obj.CreatedTime != null ? ((DateTime)obj.CreatedTime).GetUnix() : 0,
                    UpdatedTime = obj.UpdatedTime != null ? ((DateTime)obj.UpdatedTime).GetUnix() : 0
                };
            }
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
                obj.CustomGenCodeName = model.CustomGenCodeName;
                obj.CodeLength = model.CodeLength;
                obj.Prefix = model.Prefix;
                obj.Suffix = model.Suffix;
                obj.Seperator = model.Seperator;
                obj.Description = model.Description;
                obj.UpdatedUserId = currentUserId;
                obj.UpdatedTime = DateTime.Now;

                await _activityLogService.CreateLog(EnumObjectType.CustomGenCodeConfig, obj.CustomGenCodeId, $"Cập nhật cấu hình gen code tùy chọn cho {obj.CustomGenCodeName} ", model.JsonSerialize());

                await _masterDbContext.SaveChangesAsync();
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
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
                obj.UpdatedTime = DateTime.Now;

                await _activityLogService.CreateLog(EnumObjectType.CustomGenCodeConfig, obj.CustomGenCodeId, $"Xoá cấu hình gen code tùy chọn cho {obj.CustomGenCodeName} ", obj.JsonSerialize());

                await _masterDbContext.SaveChangesAsync();

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
            var result = new ServiceResult<int>() { Data = 0 };
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
                    LastValue = 0,
                    LastCode = string.Empty,
                    IsActived = true,
                    IsDeleted = false,
                    UpdatedUserId = currentUserId,
                    ResetDate = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    UpdatedTime = DateTime.Now
                };
                _masterDbContext.CustomGenCode.Add(entity);
                await _activityLogService.CreateLog(EnumObjectType.CustomGenCodeConfig, entity.CustomGenCodeId, $"Thêm mới cấu hình gen code tùy chọn cho {entity.CustomGenCodeName} ", model.JsonSerialize());

                await _masterDbContext.SaveChangesAsync();

                result.Code = GeneralCode.Success;
                result.Data = entity.CustomGenCodeId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create");
                result.Message = ex.Message;
                result.Code = GeneralCode.InternalError;
            }
            return result;
        }

        public async Task<ServiceResult<string>> GenerateCode(int objectTypeId, int objectId)
        {
            var result = new ServiceResult<string>() { Data = string.Empty };
            try
            {
                using (var trans = await _masterDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var config = _masterDbContext.ObjectCustomGenCodeMapping
                            .Join(_masterDbContext.CustomGenCode, m => m.CustomGenCodeId, c => c.CustomGenCodeId, (m, c) => new
                            {
                                ObjectCustomGenCodeMapping = m,
                                CustomGenCodeId = c
                            })
                            .Where(q => q.ObjectCustomGenCodeMapping.ObjectTypeId == objectTypeId && q.ObjectCustomGenCodeMapping.ObjectId == objectId && q.CustomGenCodeId.IsActived && !q.CustomGenCodeId.IsDeleted)
                            .Select(q => q.CustomGenCodeId)
                            .FirstOrDefault();
                        if (config == null)
                        {
                            trans.Rollback();
                            return CustomGenCodeErrorCode.CustomConfigNotFound;
                        }
                        string newCode = string.Empty;
                        var newId = 0;
                        var maxId = (int)Math.Pow(10, config.CodeLength);
                        var seperator = (string.IsNullOrEmpty(config.Seperator) || string.IsNullOrWhiteSpace(config.Seperator)) ? null : config.Seperator;
                        if (config.LastValue < 1)
                        {
                            newId = 1;
                            var stringNewId = newId < maxId ? newId.ToString(string.Format("D{0}", config.CodeLength)) : newId.ToString(string.Format("D{0}", config.CodeLength + 1));
                            newCode = $"{config.Prefix}{seperator}{stringNewId}".Trim();
                        }
                        else
                        {
                            newId = config.LastValue + 1;
                            var stringNewId = newId < maxId ? newId.ToString(string.Format("D{0}", config.CodeLength)) : newId.ToString(string.Format("D{0}", config.CodeLength + 1));
                            newCode = $"{config.Prefix}{seperator}{stringNewId}".Trim();
                        }
                        if (!(newId < maxId))
                        {
                            config.CodeLength += 1;
                            config.ResetDate = DateTime.Now;
                        }
                        config.TempValue = newId;
                        config.TempCode = newCode;

                        _masterDbContext.SaveChanges();
                        trans.Commit();

                        result.Data = newCode;
                        result.Code = GeneralCode.Success;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "GenerateCode");
                        return GeneralCode.InternalError;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateCode");
                result.Message = ex.Message;
                result.Code = GeneralCode.InternalError;

            }
            return result;
        }

        public async Task<PageData<ObjectType>> GetAllObjectType()
        {

            var total = _masterDbContext.ObjectType.Count();
            var allData = _masterDbContext.ObjectType.AsNoTracking().ToList();

            return (allData, total);

        }

        public async Task<Enum> ConfirmCode(int objectTypeId, int objectId)
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
                    return CustomGenCodeErrorCode.CustomConfigNotFound;
                }
                if (config.TempValue.HasValue && config.TempValue.Value != config.LastValue)
                {
                    config.LastValue = config.TempValue.Value;
                    config.LastCode = config.TempCode;
                    await _masterDbContext.SaveChangesAsync();
                }
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }
    }
}
