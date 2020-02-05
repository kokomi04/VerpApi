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

namespace VErp.Services.Master.Service.Config.Implement
{
    public class ObjectGenCodeService : IObjectGenCodeService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public ObjectGenCodeService(MasterDBContext masterDbContext
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

        public async Task<PageData<ObjectGenCodeOutputModel>> GetList(EnumObjectType objectType = 0, string keyword = "", int page = 1, int size = 10)
        {
            var query = from ogc in _masterDbContext.ObjectGenCode
                        select ogc;

            if (objectType > 0 && Enum.IsDefined(typeof(EnumObjectType), objectType))
            {
                query = query.Where(q => q.ObjectTypeId == (int)objectType);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.ObjectTypeName.Contains(keyword) || q.Prefix.Contains(keyword) || q.Suffix.Contains(keyword) || q.Seperator.Contains(keyword)
                        select q;
            }

            var total = await query.CountAsync();
            var objList = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var pagedData = new List<ObjectGenCodeOutputModel>();
            foreach (var item in objList)
            {
                var info = new ObjectGenCodeOutputModel()
                {
                    ObjectGenCodeId = item.ObjectGenCodeId,
                    ObjectTypeId = item.ObjectTypeId,
                    ObjectTypeName = item.ObjectTypeName,
                    CodeLength = item.CodeLength,
                    Prefix = item.Prefix,
                    Suffix = item.Suffix,
                    Seperator = item.Seperator,
                    LastCode = item.LastCode,
                    IsActived = item.IsActived,
                    UpdatedUserId = item.UpdatedUserId,
                    CreatedTime = item.CreatedTime,
                    UpdatedTime = item.UpdatedTime,

                };
                pagedData.Add(info);
            }
            return (pagedData, total);
        }

        public async Task<ServiceResult<ObjectGenCodeOutputModel>> GetInfo(int objectGenCodeId)
        {
            try
            {
                var obj = await _masterDbContext.ObjectGenCode.FirstOrDefaultAsync(p => p.ObjectGenCodeId == objectGenCodeId);
                if (obj == null)
                {
                    return ObjectGenCodeErrorCode.ConfigNotFound;
                }
                var info = new ObjectGenCodeOutputModel()
                {
                    ObjectGenCodeId = obj.ObjectGenCodeId,
                    ObjectTypeId = obj.ObjectTypeId,
                    ObjectTypeName = obj.ObjectTypeName,
                    CodeLength = obj.CodeLength,
                    Prefix = obj.Prefix,
                    Suffix = obj.Suffix,
                    Seperator = obj.Seperator,
                    LastCode = obj.LastCode,
                    IsActived = obj.IsActived,
                    UpdatedUserId = obj.UpdatedUserId,
                    CreatedTime = obj.CreatedTime,
                    UpdatedTime = obj.UpdatedTime,
                };
                return info;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetInfo");
                return null;
            }
        }

        public async Task<Enum> Update(int objectGenCodeId, int currentUserId, ObjectGenCodeInputModel model)
        {
            try
            {
                var obj = await _masterDbContext.ObjectGenCode.FirstOrDefaultAsync(p => p.ObjectGenCodeId == objectGenCodeId);

                if (obj == null)
                {
                    return ObjectGenCodeErrorCode.ConfigNotFound;
                }
               
                obj.CodeLength = model.CodeLength;
                obj.Prefix = model.Prefix;
                obj.Suffix = model.Suffix;
                obj.Seperator = model.Seperator;
                obj.UpdatedUserId = currentUserId;
                obj.UpdatedTime = DateTime.Now;

                await _activityLogService.CreateLog(EnumObjectType.GenCodeConfig, obj.ObjectGenCodeId, $"Cập nhật cấu hình gen code cho {obj.ObjectTypeName} ", model.JsonSerialize());

                await _masterDbContext.SaveChangesAsync();
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }


        public async Task<Enum> Delete(int currentUserId, int objectGenCodeId)
        {
            try
            {
                var obj = await _masterDbContext.ObjectGenCode.FirstOrDefaultAsync(p => p.ObjectGenCodeId == objectGenCodeId);
                if (obj == null)
                {
                    return ObjectGenCodeErrorCode.ConfigNotFound;
                }
                obj.IsDeleted = true;
                obj.UpdatedUserId = currentUserId;
                obj.UpdatedTime = DateTime.Now;

                await _activityLogService.CreateLog(EnumObjectType.GenCodeConfig, obj.ObjectGenCodeId, $"Xoá cấu hình gen code cho {obj.ObjectTypeName} ", obj.JsonSerialize());

                await _masterDbContext.SaveChangesAsync();

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

        public async Task<ServiceResult<int>> Create(EnumObjectType objectType, int currentUserId, ObjectGenCodeInputModel model)
        {
            var result = new ServiceResult<int>() { Data = 0 };
            try
            {
                if (Enum.IsDefined(typeof(EnumObjectType), objectType) == false)
                {
                    return GeneralCode.InvalidParams;
                }

                if (_masterDbContext.ObjectGenCode.Any(q => q.ObjectTypeId == (int)objectType))
                {
                    return ObjectGenCodeErrorCode.ConfigAlreadyExisted;
                }
                // Lấy thông tin tên loại đối tượng tương ứng
                var objType = _masterDbContext.ObjectType.FirstOrDefault(q => q.ObjectTypeId == (int)objectType);
                if (objType == null)
                {
                    return GeneralCode.InvalidParams;
                }
                var entity = new ObjectGenCode()
                {
                    ObjectTypeId = (int)objectType,
                    ObjectTypeName = objType.ObjectTypeName,
                    CodeLength = (model.CodeLength > 5) ? model.CodeLength : 5,
                    Prefix = model.Prefix ?? string.Empty,
                    Suffix = model.Suffix ?? string.Empty,
                    Seperator = model.Seperator ?? string.Empty,
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
                _masterDbContext.ObjectGenCode.Add(entity);
                await _activityLogService.CreateLog(EnumObjectType.GenCodeConfig, entity.ObjectGenCodeId, $"Thêm mới cấu hình gen code cho {entity.ObjectTypeName} ", model.JsonSerialize());

                await _masterDbContext.SaveChangesAsync();

                result.Code = GeneralCode.Success;
                result.Data = entity.ObjectGenCodeId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create");
                result.Message = ex.Message;
                result.Code = GeneralCode.InternalError;
            }
            return result;
        }

        public async Task<ServiceResult<string>> GenerateCode(EnumObjectType objectType)
        {
            var result = new ServiceResult<string>() { Data = string.Empty };
            try
            {
                if (Enum.IsDefined(typeof(EnumObjectType), objectType) == false)
                {
                    return GeneralCode.InvalidParams;
                }

                using (var trans = await _masterDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var config = _masterDbContext.ObjectGenCode.FirstOrDefault(q => q.ObjectTypeId == (int)objectType && q.IsActived && !q.IsDeleted);
                        if (config == null)
                        {
                            trans.Rollback();
                            return ObjectGenCodeErrorCode.ConfigNotFound;
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
                        config.LastValue = newId;
                        config.LastCode = newCode;

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
            try
            {
                var total = _masterDbContext.ObjectType.Count();
                var allData = _masterDbContext.ObjectType.AsNoTracking().ToList();

                return (allData, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllObjectType");
                return (null, 0);
            }
        }      
    }
}
