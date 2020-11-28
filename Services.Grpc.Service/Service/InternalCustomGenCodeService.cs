using Grpc.Core;
using GrpcProto.Protos;
using GrpcProto.Protos.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Grpc.Service
{
    //public class InternalCustomGenCodeService: CustomGenCodeProvider.CustomGenCodeProviderBase
    //{
    //    private readonly MasterDBContext _masterDbContext;
    //    private readonly ILogger<InternalCustomGenCodeService> _logger;
    //    private readonly IActivityLogService _activityLogService;

    //    public InternalCustomGenCodeService(MasterDBContext masterDBContext, 
    //        ILogger<InternalCustomGenCodeService> logger,
    //        IActivityLogService activityLogService)
    //    {
    //        _masterDbContext = masterDBContext;
    //        _logger = logger;
    //        _activityLogService = activityLogService;
    //    }

    //    public async override Task<IsSuccessResponses> MapObjectCustomGenCode(MapObjectCustomGenCodeRequest request, ServerCallContext context)
    //    {
    //        var dic = new Dictionary<ObjectCustomGenCodeMapping, CustomGenCode>();

    //        foreach (var mapConfig in request.Data)
    //        {
    //            var config = await _masterDbContext.CustomGenCode
    //                .Where(c => c.IsActived)
    //                .Where(c => c.CustomGenCodeId == mapConfig.Value)
    //                .FirstOrDefaultAsync();
    //            if (config == null)
    //            {
    //                throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
    //            }
    //            var curMapConfig = await _masterDbContext.ObjectCustomGenCodeMapping
    //                .FirstOrDefaultAsync(m => m.TargetObjectTypeId == request.ObjectTypeId && m.ObjectId == mapConfig.Key);
    //            if (curMapConfig == null)
    //            {
    //                curMapConfig = new ObjectCustomGenCodeMapping
    //                {
    //                    ObjectTypeId = request.ObjectTypeId,
    //                    ObjectId = mapConfig.Key,
    //                    CustomGenCodeId = mapConfig.Value,
    //                };
    //                _masterDbContext.ObjectCustomGenCodeMapping.Add(curMapConfig);
    //            }
    //            else if (curMapConfig.CustomGenCodeId != mapConfig.Value)
    //            {
    //                curMapConfig.CustomGenCodeId = mapConfig.Value;
    //            }

    //            if (!dic.ContainsKey(curMapConfig))
    //            {
    //                dic.Add(curMapConfig, config);
    //            }
    //        }
    //        await _masterDbContext.SaveChangesAsync();

    //        foreach (var item in dic)
    //        {
    //            await _activityLogService.CreateLog(EnumObjectType.ObjectCustomGenCodeMapping, item.Key.ObjectCustomGenCodeMappingId, $"Gán sinh tùy chọn (multi) {item.Value.CustomGenCodeName}", item.Key.JsonSerialize());
    //        }

    //        return new IsSuccessResponses { IsSuccess = true };
    //    }

    //    public async override Task<IsSuccessResponses> ConfirmCode(ConfirmCodeRequest request, ServerCallContext context)
    //    {
    //        var config = await _masterDbContext.CustomGenCode
    //            .Join(_masterDbContext.ObjectCustomGenCodeMapping, c => c.CustomGenCodeId, m => m.CustomGenCodeId, (c, m) => new
    //            {
    //                CustomGenCode = c,
    //                m.ObjectId,
    //                m.ObjectTypeId
    //            })
    //            .Where(cm => cm.ObjectId == request.ObjectId && cm.ObjectTypeId == request.ObjectTypeId)
    //            .Select(cm => cm.CustomGenCode)
    //            .FirstOrDefaultAsync();
    //        if (config == null)
    //        {
    //            throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
    //        }
    //        if (config.TempValue.HasValue && config.TempValue.Value != config.LastValue)
    //        {
    //            config.LastValue = config.TempValue.Value;
    //            config.LastCode = config.TempCode;
    //            await _masterDbContext.SaveChangesAsync();
    //        }
    //        return new IsSuccessResponses { IsSuccess = true };
    //    }

    //    public async override Task<CustomCodeModelOutput> GenerateCode(GenerateCodeRequest request, ServerCallContext context)
    //    {
    //        CustomCodeModelOutput result;
    //        int lastValue = request.LastValue;
    //        string code = request.Code;
    //        int customGenCodeId = request.CustomGenCodeId;

    //        using (var trans = await _masterDbContext.Database.BeginTransactionAsync())
    //        {
    //            var config = _masterDbContext.CustomGenCode
    //                .FirstOrDefault(q => q.CustomGenCodeId == customGenCodeId);

    //            if (config == null)
    //            {
    //                trans.Rollback();
    //                throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
    //            }
    //            string newCode = string.Empty;
    //            var newId = 0;
    //            var maxId = (int)Math.Pow(10, config.CodeLength);
    //            var seperator = (string.IsNullOrEmpty(config.Seperator) || string.IsNullOrWhiteSpace(config.Seperator)) ? null : config.Seperator;

    //            lastValue = lastValue > config.LastValue ? lastValue : config.LastValue;

    //            if (lastValue < 1)
    //            {
    //                newId = 1;
    //                var stringNewId = newId < maxId ? newId.ToString(string.Format("D{0}", config.CodeLength)) : newId.ToString(string.Format("D{0}", config.CodeLength + 1));
    //                newCode = $"{config.Prefix}{seperator}{stringNewId}".Trim();
    //            }
    //            else
    //            {
    //                newId = lastValue + 1;
    //                var stringNewId = newId < maxId ? newId.ToString(string.Format("D{0}", config.CodeLength)) : newId.ToString(string.Format("D{0}", config.CodeLength + 1));
    //                newCode = $"{config.Prefix}{seperator}{stringNewId}".Trim();
    //            }

    //            newCode = Utils.FormatStyle(newCode, code, null);

    //            if (!(newId < maxId))
    //            {
    //                config.CodeLength += 1;
    //                config.ResetDate = DateTime.UtcNow;
    //            }
    //            config.TempValue = newId;
    //            config.TempCode = newCode;

    //            _masterDbContext.SaveChanges();
    //            trans.Commit();

    //            result = new CustomCodeModelOutput
    //            {
    //                CustomCode = newCode,
    //                LastValue = newId,
    //                CustomGenCodeId = config.CustomGenCodeId,
    //            };
    //        }

    //        return result;
    //    }

    //    public async override Task<CustomGenCodeOutputModelOutput> CurrentConfig(CurrentConfigRequest request, ServerCallContext context)
    //    {
    //        var obj = await _masterDbContext.ObjectCustomGenCodeMapping
    //            .Join(_masterDbContext.CustomGenCode, m => m.CustomGenCodeId, c => c.CustomGenCodeId, (m, c) => new
    //            {
    //                ObjectCustomGenCodeMapping = m,
    //                CustomGenCodeId = c
    //            })
    //            .Where(q => q.ObjectCustomGenCodeMapping.ObjectTypeId == request.ObjectTypeId
    //            && q.ObjectCustomGenCodeMapping.ObjectId == request.ObjectId
    //            && q.CustomGenCodeId.IsActived
    //            && !q.CustomGenCodeId.IsDeleted)
    //            .Select(q => q.CustomGenCodeId)
    //            .FirstOrDefaultAsync();

    //        if (obj == null)
    //        {
    //            throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotExisted);
    //        }

    //        return new CustomGenCodeOutputModelOutput()
    //        {
    //            CustomGenCodeId = obj.CustomGenCodeId,
    //            ParentId = (int)obj.ParentId,
    //            CustomGenCodeName = obj.CustomGenCodeName,
    //            Description = obj.Description,
    //            CodeLength = obj.CodeLength,
    //            Prefix = obj.Prefix,
    //            Suffix = obj.Suffix,
    //            Seperator = obj.Seperator,
    //            LastValue = obj.LastValue,
    //            LastCode = obj.LastCode,
    //            IsActived = obj.IsActived,
    //            UpdatedUserId = (int)obj.UpdatedUserId,
    //            CreatedTime = obj.CreatedTime != null ? ((DateTime)obj.CreatedTime).GetUnix() : 0,
    //            UpdatedTime = obj.UpdatedTime != null ? ((DateTime)obj.UpdatedTime).GetUnix() : 0,
    //            SortOrder = obj.SortOrder
    //        };
    //    }
    //}
}
