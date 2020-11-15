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
using Verp.Cache.RedisCache;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class ObjectGenCodeService : IObjectGenCodeService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICustomGenCodeService _customGenCodeService;
        private readonly ICurrentContextService _currentContextService;

        public ObjectGenCodeService(MasterDBContext masterDbContext
            , IOptions<AppSetting> appSetting
            , ILogger<ObjectGenCodeService> logger
            , IActivityLogService activityLogService
            , ICustomGenCodeService customGenCodeService
            , ICurrentContextService currentContextService
        )
        {
            _masterDbContext = masterDbContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _customGenCodeService = customGenCodeService;
            _currentContextService = currentContextService;
        }

        //public async Task<PageData<ObjectGenCodeOutputModel>> GetList(EnumObjectType objectType = 0, string keyword = "", int page = 1, int size = 10)
        //{

        //    var allObjectType = EnumExtensions.GetEnumMembers<EnumObjectType>().Select(e => (int)e.Enum);

        //    _masterDbContext.ObjectCustomGenCodeMapping.AsNoTracking().Where(x => allObjectType.Contains(x.ObjectTypeId));



        //    var query = from ogc in _masterDbContext.ObjectGenCode
        //                select ogc;

        //    if (objectType > 0 && Enum.IsDefined(typeof(EnumObjectType), objectType))
        //    {
        //        query = query.Where(q => q.ObjectTypeId == (int)objectType);
        //    }

        //    if (!string.IsNullOrWhiteSpace(keyword))
        //    {
        //        query = from q in query
        //                where q.ObjectTypeName.Contains(keyword) || q.Prefix.Contains(keyword) || q.Suffix.Contains(keyword) || q.Seperator.Contains(keyword)
        //                select q;
        //    }

        //    var total = await query.CountAsync();
        //    var objList = await query.Skip((page - 1) * size).Take(size).ToListAsync();

        //    var pagedData = new List<ObjectGenCodeOutputModel>();
        //    foreach (var item in objList)
        //    {
        //        var info = new ObjectGenCodeOutputModel()
        //        {
        //            ObjectGenCodeId = item.ObjectGenCodeId,
        //            ObjectTypeId = item.ObjectTypeId,
        //            ObjectTypeName = item.ObjectTypeName,
        //            CodeLength = item.CodeLength,
        //            Prefix = item.Prefix,
        //            Suffix = item.Suffix,
        //            Seperator = item.Seperator,
        //            LastCode = item.LastCode,
        //            IsActived = item.IsActived,
        //            UpdatedUserId = item.UpdatedUserId,
        //            CreatedTime = item.CreatedTime != null ? ((DateTime)item.CreatedTime).GetUnix() : 0,
        //            UpdatedTime = item.UpdatedTime != null ? ((DateTime)item.UpdatedTime).GetUnix() : 0,
        //        };
        //        pagedData.Add(info);
        //    }
        //    return (pagedData, total);
        //}

        //public async Task<ObjectGenCodeOutputModel> GetInfo(int objectGenCodeId)
        //{

        //    var obj = await _masterDbContext.ObjectGenCode.FirstOrDefaultAsync(p => p.ObjectGenCodeId == objectGenCodeId);
        //    if (obj == null)
        //    {
        //        throw new BadRequestException(ObjectGenCodeErrorCode.ConfigNotFound);
        //    }
        //    var info = new ObjectGenCodeOutputModel()
        //    {
        //        ObjectGenCodeId = obj.ObjectGenCodeId,
        //        ObjectTypeId = obj.ObjectTypeId,
        //        ObjectTypeName = obj.ObjectTypeName,
        //        CodeLength = obj.CodeLength,
        //        Prefix = obj.Prefix,
        //        Suffix = obj.Suffix,
        //        Seperator = obj.Seperator,
        //        LastCode = obj.LastCode,
        //        IsActived = obj.IsActived,
        //        UpdatedUserId = obj.UpdatedUserId,
        //        CreatedTime = obj.CreatedTime != null ? ((DateTime)obj.CreatedTime).GetUnix() : 0,
        //        UpdatedTime = obj.UpdatedTime != null ? ((DateTime)obj.UpdatedTime).GetUnix() : 0
        //    };
        //    return info;

        //}

        //public async Task<bool> Update(int objectGenCodeId, int currentUserId, ObjectGenCodeInputModel model)
        //{

        //    var obj = await _masterDbContext.ObjectGenCode.FirstOrDefaultAsync(p => p.ObjectGenCodeId == objectGenCodeId);

        //    if (obj == null)
        //    {
        //        throw new BadRequestException(ObjectGenCodeErrorCode.ConfigNotFound);
        //    }

        //    obj.CodeLength = model.CodeLength;
        //    obj.Prefix = model.Prefix;
        //    obj.Suffix = model.Suffix;
        //    obj.Seperator = model.Seperator;
        //    obj.UpdatedUserId = currentUserId;
        //    obj.UpdatedTime = DateTime.Now;

        //    await _activityLogService.CreateLog(EnumObjectType.GenCodeConfig, obj.ObjectGenCodeId, $"Cập nhật cấu hình gen code cho {obj.ObjectTypeName} ", model.JsonSerialize());

        //    await _masterDbContext.SaveChangesAsync();
        //    return true;

        //}


        //public async Task<bool> Delete(int currentUserId, int objectGenCodeId)
        //{

        //    var obj = await _masterDbContext.ObjectGenCode.FirstOrDefaultAsync(p => p.ObjectGenCodeId == objectGenCodeId);
        //    if (obj == null)
        //    {
        //        throw new BadRequestException(ObjectGenCodeErrorCode.ConfigNotFound);
        //    }
        //    obj.IsDeleted = true;
        //    obj.UpdatedUserId = currentUserId;
        //    obj.UpdatedTime = DateTime.Now;

        //    await _activityLogService.CreateLog(EnumObjectType.GenCodeConfig, obj.ObjectGenCodeId, $"Xoá cấu hình gen code cho {obj.ObjectTypeName} ", obj.JsonSerialize());

        //    await _masterDbContext.SaveChangesAsync();

        //    return true;
        //}

        //public async Task<int> Create(EnumObjectType objectType, int currentUserId, ObjectGenCodeInputModel model)
        //{
        //    if (await _masterDbContext.ObjectGenCode.AnyAsync(q => q.ObjectTypeId == (int)objectType))
        //    {
        //        throw new BadRequestException(ObjectGenCodeErrorCode.ConfigAlreadyExisted);
        //    }
        //    // Lấy thông tin tên loại đối tượng tương ứng
        //    var objectTypes = GetAllObjectType().List;
        //    var objType = objectTypes.FirstOrDefault(q => q.ObjectTypeId == objectType);
        //    if (objType == null)
        //    {
        //        throw new BadRequestException(GeneralCode.InvalidParams);
        //    }
        //    var entity = new ObjectGenCode()
        //    {
        //        ObjectTypeId = (int)objectType,
        //        ObjectTypeName = objType.ObjectTypeName,
        //        CodeLength = (model.CodeLength > 5) ? model.CodeLength : 5,
        //        Prefix = model.Prefix ?? string.Empty,
        //        Suffix = model.Suffix ?? string.Empty,
        //        Seperator = model.Seperator ?? string.Empty,
        //        DateFormat = string.Empty,
        //        LastValue = 0,
        //        LastCode = string.Empty,
        //        IsActived = true,
        //        IsDeleted = false,
        //        UpdatedUserId = currentUserId,
        //        ResetDate = DateTime.Now,
        //        CreatedTime = DateTime.Now,
        //        UpdatedTime = DateTime.Now
        //    };
        //    _masterDbContext.ObjectGenCode.Add(entity);
        //    await _activityLogService.CreateLog(EnumObjectType.GenCodeConfig, entity.ObjectGenCodeId, $"Thêm mới cấu hình gen code cho {entity.ObjectTypeName} ", model.JsonSerialize());

        //    await _masterDbContext.SaveChangesAsync();

        //    return entity.ObjectGenCodeId;

        //}

        public async Task<string> GenerateCode(EnumObjectType objectType)
        {
            /*using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockGenerateCodeKey(objectType)))
            {
                using (var trans = await _masterDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var config = _masterDbContext.ObjectGenCode.FirstOrDefault(q => q.ObjectTypeId == (int)objectType && q.IsActived && !q.IsDeleted);
                        if (config == null)
                        {
                            trans.Rollback();
                            throw new BadRequestException(ObjectGenCodeErrorCode.ConfigNotFound);
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

                        return newCode;

                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }*/
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockGenerateCodeKey(objectType)))
            {
                var currentConfig = await _customGenCodeService.GetCurrentConfig(objectType, 0);
                var customCode = await _customGenCodeService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.LastValue);

                await _customGenCodeService.ConfirmCode((int)objectType, 0);

                return customCode.CustomCode;
            }
        }

        public PageData<ObjectType> GetAllObjectType()
        {
            var allData = EnumExtensions.GetEnumMembers<EnumObjectType>()
                .Where(m => m.Attributes != null && m.Attributes.Any(a => a.GetType() == typeof(GenCodeObjectAttribute)))
                .Select(m => new ObjectType
                {
                    ObjectTypeId = m.Enum,
                    ObjectTypeName = m.Description ?? m.Name.ToString()
                }).ToList();


            return (allData, allData.Count);
        }

        public async Task<PageData<ObjectGenCodeModel>> GetList(string keyword, int page, int size)
        {
            var allObjectType = GetAllObjectType().List;

            var query = _masterDbContext.ObjectCustomGenCodeMapping
                .AsNoTracking()
                .Where(x => x.ObjectId == 0)
                .ToList()
                .Select(x => new ObjectGenCodeModel
                {
                    ObjectCustomGenCodeMappingId = x.ObjectCustomGenCodeMappingId,
                    ObjectTypeId = x.ObjectTypeId,
                    CustomGenCodeId = x.CustomGenCodeId,
                    ObjectTypeName = allObjectType.FirstOrDefault(e => (int)e.ObjectTypeId == x.ObjectTypeId)?.ObjectTypeName
                }).ToList();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.ObjectTypeName.Contains(keyword)).ToList();
            }

            var total = query.Count();
            var objList = query.Skip((page - 1) * size).Take(size).ToList();

            return (query, total);
        }

        public async Task<bool> MapObjectGenCode(ObjectGenCodeMapping model)
        {
            return await _customGenCodeService.MapObjectCustomGenCode(_currentContextService.UserId, new ObjectCustomGenCodeMapping
            {
                CustomGenCodeId = model.CustomGenCodeId,
                ObjectCustomGenCodeMappingId = model.ObjectCustomGenCodeMappingId,
                ObjectId = 0,//default
                ObjectTypeId = model.ObjectTypeId,
                UpdatedByUserId = _currentContextService.UserId
            });

        }

        public async Task<bool> DeleteMapObjectGenCode(int objectCustomGenCodeMappingId)
        {
            return await _customGenCodeService.DeleteMapObjectCustomGenCode(objectCustomGenCodeMappingId);
        }
    }
}
