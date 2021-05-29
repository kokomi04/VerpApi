using GrpcProto.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface ICustomGenCodeHelperService
    {
        //Task<bool> MapObjectCustomGenCode(EnumObjectType objectTypeId, Dictionary<int, int> data);
        Task<bool> MapObjectCustomGenCode(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, Dictionary<long, int> objectCustomGenCodes);

        Task<CustomGenCodeOutputModel> CurrentConfig(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, long configObjectId, long? fId, string code, long? date);
        Task<CustomCodeGeneratedModel> GenerateCode(int customGenCodeId, int lastValue, long? fId, string code, long? date);
        //Task<bool> ConfirmCode(int? customGenCodeId, string baseValue);
        Task<bool> ConfirmCode(CustomGenCodeBaseValueModel lastBaseValue);

        GenerateCodeContext CreateGenerateCodeContext(Dictionary<string, int> baseValueChains = null);
    }

    public class CustomGenCodeHelperService : ICustomGenCodeHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        // private readonly CustomGenCodeProvider.CustomGenCodeProviderClient _customGenCodeClient;
        private readonly ICurrentContextService _currentContextService;
        public CustomGenCodeHelperService(IHttpCrossService httpCrossService,
            IOptions<AppSetting> appSetting,
            ILogger<ProductHelperService> logger,
            ICurrentContextService currentContextService)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
            //_customGenCodeClient = customGenCodeClient;
            _currentContextService = currentContextService;
        }
        public async Task<bool> MapObjectCustomGenCode(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, Dictionary<long, int> objectCustomGenCodes)
        {
            //if (_appSetting.GrpcInternal?.Address?.Contains("https") == true)
            //{
            //    var reuestData = new MapObjectCustomGenCodeRequest
            //    {
            //        ObjectTypeId = (int)objectTypeId
            //    };
            //    reuestData.Data.Add(data);

            //    return (await _customGenCodeClient.MapObjectCustomGenCodeAsync(reuestData)).IsSuccess;
            //}
            return await _httpCrossService.Post<bool>($"api/internal/InternalCustomGenCode/multiconfigs?targetObjectTypeId={(int)targetObjectTypeId}&configObjectTypeId={(int)configObjectTypeId}", objectCustomGenCodes);
        }

        public async Task<CustomGenCodeOutputModel> CurrentConfig(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, long configObjectId, long? fId, string code, long? date)
        {
            //if (_appSetting.GrpcInternal?.Address?.Contains("https") == true)
            //{
            //    var responses = await _customGenCodeClient.CurrentConfigAsync(new CurrentConfigRequest
            //    {
            //        ObjectId = objectId,
            //        ObjectTypeId = (int)objectTypeId
            //    });
            //    return new CustomGenCodeOutputModelOut
            //    {
            //        CodeLength = responses.CodeLength,
            //        CreatedTime = responses.CreatedTime,
            //        CustomGenCodeId = responses.CustomGenCodeId,
            //        CustomGenCodeName = responses.CustomGenCodeName,
            //        Description = responses.Description,
            //        IsActived = responses.IsActived,
            //        LastCode = responses.LastCode,
            //        LastValue = responses.LastValue,
            //        ParentId = responses.ParentId,
            //        Prefix = responses.Prefix,
            //        Seperator = responses.Seperator,
            //        SortOrder = responses.SortOrder,
            //        Suffix = responses.Suffix,
            //        UpdatedTime = responses.UpdatedTime,
            //        UpdatedUserId = responses.UpdatedUserId
            //    };
            //}

            var queries = new
            {
                targetObjectTypeId,
                configObjectTypeId,
                configObjectId,
                fId,
                code,
                date
            };

            return await _httpCrossService.Get<CustomGenCodeOutputModel>($"api/internal/InternalCustomGenCode/currentConfig", queries);
        }

        public async Task<CustomCodeGeneratedModel> GenerateCode(int customGenCodeId, int lastValue, long? fId, string code, long? date)
        {
            //if(_appSetting.GrpcInternal?.Address?.Contains("https") == true)
            //{
            //    var responses = await _customGenCodeClient.GenerateCodeAsync(new GenerateCodeRequest
            //    {
            //        CustomGenCodeId = customGenCodeId,
            //        LastValue = lastValue
            //    });

            //    return new CustomCodeModelOutput
            //    {
            //        LastValue = responses.LastValue,
            //        CustomCode = responses.CustomCode,
            //        CustomGenCodeId = responses.CustomGenCodeId
            //    };
            //}

            var queries = new
            {
                customGenCodeId,
                lastValue,
                fId,
                code,
                date
            };
            return await _httpCrossService.Get<CustomCodeGeneratedModel>($"api/internal/InternalCustomGenCode/generateCode", queries);
        }

        public async Task<bool> ConfirmCode(int? customGenCodeId, string baseValue)
        {
            if (!customGenCodeId.HasValue || customGenCodeId <= 0) return true;
            //if(_appSetting.GrpcInternal?.Address?.Contains("https") == true)
            //{
            //    return (await _customGenCodeClient.ConfirmCodeAsync(new ConfirmCodeRequest
            //    {
            //        ObjectId = objectId,
            //        ObjectTypeId = (int)objectTypeId
            //    })).IsSuccess;
            //}
            return await _httpCrossService.Put<bool>($"api/internal/InternalCustomGenCode/{customGenCodeId}/confirmCode?baseValue={baseValue}", null);
        }

        public async Task<bool> ConfirmCode(CustomGenCodeBaseValueModel lastBaseValue)
        {
            // GenerateCodeLoop((IQueryable<CustomGenCodeOutputModel>)null, "", (s, c) => s.IsActived == true && s.LastCode == c, null);
            return await ConfirmCode(lastBaseValue?.CustomGenCodeId, lastBaseValue?.BaseValue);
        }



        public GenerateCodeContext CreateGenerateCodeContext(Dictionary<string, int> baseValueChains = null)
        {
            return new GenerateCodeContext(this, _currentContextService);
        }

    }
}



public class GenerateCodeContext
{
    internal ICustomGenCodeHelperService CustomGenCodeHelper { get; private set; }
    internal ICurrentContextService CurrentContextService { get; private set; }

    internal EnumObjectType TargetObjectTypeId { get; private set; }
    internal EnumObjectType ConfigObjectTypeId { get; private set; }
    internal long ConfigObjectId { get; private set; }
    public Dictionary<string, int> BaseValueChains { get; private set; }
    internal GenerateCodeContext(ICustomGenCodeHelperService customGenCodeHelper, ICurrentContextService currentContextService, Dictionary<string, int> baseValueChains = null)
    {
        CustomGenCodeHelper = customGenCodeHelper;
        CurrentContextService = currentContextService;
        BaseValueChains = baseValueChains;
    }

    /// <summary>
    /// Set parameters which has configured code structure
    /// </summary>
    /// <param name="targetObjectTypeId"></param>
    /// <param name="configObjectTypeId"></param>
    /// <param name="configObjectId"></param>
    /// <returns></returns>
    public GenerateCodeConfig SetConfig(EnumObjectType targetObjectTypeId, EnumObjectType? configObjectTypeId = null, long configObjectId = 0)
    {
        TargetObjectTypeId = targetObjectTypeId;
        ConfigObjectTypeId = configObjectTypeId ?? targetObjectTypeId;
        ConfigObjectId = configObjectId;
        return new GenerateCodeConfig(this);
    }

    private CustomGenCodeBaseValueModel configBaseValue;
    internal void SetconfigBaseValue(CustomGenCodeBaseValueModel configBaseValue)
    {
        this.configBaseValue = configBaseValue;
    }

    /// <summary>
    /// Confirm code value has used
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ConfirmCode()
    {
        if (configBaseValue.IsNullObject()) return true;

        return await CustomGenCodeHelper.ConfirmCode(configBaseValue);
    }
}

public class GenerateCodeConfig
{
    internal GenerateCodeContext Ctx { get; private set; }
    internal string RefCode { get; private set; }
    internal long FId { get; private set; }
    internal long? Date { get; private set; }
    internal GenerateCodeConfig(GenerateCodeContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException();
        Ctx = ctx;
    }

    /// <summary>
    /// Set replace data for generate code structure
    /// </summary>
    /// <param name="fId">ID of current object</param>
    /// <param name="date">%DATE(xxx)%</param>
    /// <param name="refCode">%CODE%</param>
    /// <returns></returns>
    public GenerateCodeConfigData SetConfigData(long fId, long? date = null, string refCode = "")
    {
        RefCode = refCode;
        FId = fId;
        Date = date;
        return new GenerateCodeConfigData(this);
    }
}

public class GenerateCodeConfigData
{
    private GenerateCodeConfig configOption { get; set; }
    internal GenerateCodeConfigData(GenerateCodeConfig configOption)
    {
        if (configOption == null) throw new ArgumentNullException();
        this.configOption = configOption;
    }

    /// <summary>
    /// Validate existed code if currentCode is not empty, try to generate code (10 times) if currentCode is empty
    /// </summary>
    /// <typeparam name="TSource">Entity</typeparam>
    /// <param name="query">DbSet<Entity></param>
    /// <param name="currentCode">currentCode</param>
    /// <param name="checkExisted">expression to check if code/ generated code had been existed</param>
    /// <returns></returns>
    public async Task<string> TryValidateAndGenerateCode<TSource>(DbSet<TSource> query, string currentCode, Expression<Func<TSource, string, bool>> checkExisted) where TSource : class
    {
        TSource existedItem;

        if (!string.IsNullOrWhiteSpace(currentCode))
        {

            existedItem = await GetExistedItem(query, currentCode, checkExisted);
            if (existedItem != null) throw new BadRequestException(GeneralCode.ItemCodeExisted);
            return currentCode;
        }
        else
        {
            var customGenCodeHelper = configOption.Ctx.CustomGenCodeHelper;
            var targetObjectTypeId = configOption.Ctx.TargetObjectTypeId;
            var configObjectTypeId = configOption.Ctx.ConfigObjectTypeId;
            var configObjectId = configOption.Ctx.ConfigObjectId;

            var refCode = configOption.RefCode;
            var fId = configOption.FId;
            var date = configOption.Date ?? configOption.Ctx.CurrentContextService.GetNowUtc().GetUnix();

            var config = await customGenCodeHelper.CurrentConfig(targetObjectTypeId, configObjectTypeId, configObjectId, fId, refCode, date);

            if (config == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết lập cấu hình sinh mã");

            configOption.Ctx.SetconfigBaseValue(config.CurrentLastValue);

            var lastValue = config.CurrentLastValue.LastValue;
            var genChainKey = config.CustomGenCodeId + "|" + config.CurrentLastValue.BaseValue;

            var baseValueChains = configOption.Ctx.BaseValueChains;
            if (baseValueChains?.ContainsKey(genChainKey) == true)
            {
                lastValue = baseValueChains[genChainKey];
            }

            int dem = 0;
            string code;
            do
            {
                code = (await customGenCodeHelper.GenerateCode(config.CustomGenCodeId, lastValue, fId, refCode, date))?.CustomCode;
                existedItem = await GetExistedItem(query, code, checkExisted);

                lastValue++;

                if (baseValueChains != null)
                {
                    if (baseValueChains.ContainsKey(genChainKey))
                    {
                        baseValueChains[genChainKey] = lastValue;
                    }
                    else
                    {
                        baseValueChains.Add(genChainKey, lastValue);
                    }
                }

                dem++;
                if (dem == 10)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể sinh mã hoặc cấu hình sinh mã chưa đúng!");
                }
            } while (existedItem != null && dem < 10);
            return code;
        }

    }

    private async Task<TSource> GetExistedItem<TSource>(DbSet<TSource> query, string code, Expression<Func<TSource, string, bool>> checkExisted) where TSource : class
    {
        var entityParameter = Expression.Parameter(typeof(TSource), "p");

        //Expression<Func<string>> codeFunction = () => code;
        //var codeParametter = Expression.Invoke(codeFunction);
        var codeParametter = Expression.Property(Expression.Constant(new { code }), "code");

        Expression<Func<TSource, bool>> conditionExpression = Expression.Lambda<Func<TSource, bool>>(Expression.Invoke(checkExisted, entityParameter, codeParametter), entityParameter);

        return await query.Where(conditionExpression).FirstOrDefaultAsync();
    }


}
