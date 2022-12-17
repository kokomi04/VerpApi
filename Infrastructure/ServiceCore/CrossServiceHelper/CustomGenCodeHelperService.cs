using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface ICustomGenCodeHelperService
    {
        //Task<bool> MapObjectCustomGenCode(EnumObjectType objectTypeId, Dictionary<int, int> data);
        Task<bool> MapObjectCustomGenCode(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, Dictionary<long, int> objectCustomGenCodes);

        Task<CustomGenCodeOutputModel> CurrentConfig(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, long configObjectId, long? fId, string code, long? date);
        Task<CustomCodeGeneratedModel> GenerateCode(int customGenCodeId, int lastValue, long? fId, string parentCode, long? date);
        //Task<bool> ConfirmCode(int? customGenCodeId, string baseValue);
        Task<bool> ConfirmCode(CustomGenCodeBaseValueModel lastBaseValue);

        IGenerateCodeContext CreateGenerateCodeContext(IDictionary<string, int> baseValueChains = null);
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

        public async Task<CustomCodeGeneratedModel> GenerateCode(int customGenCodeId, int lastValue, long? fId, string parentCode, long? date)
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
                code = parentCode,
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



        public IGenerateCodeContext CreateGenerateCodeContext(IDictionary<string, int> baseValueChains = null)
        {
            return new GenerateCodeContext(this, _currentContextService, baseValueChains);
        }

    }
}



internal class GenerateCodeContext : IGenerateCodeContext, IGenerateCodeConfig, IGenerateCodeAction
{
    private ICustomGenCodeHelperService _customGenCodeHelper;
    private ICurrentContextService _currentContextService;

    private EnumObjectType _targetObjectTypeId;
    private EnumObjectType _configObjectTypeId;
    private long _configObjectId;
    private IDictionary<string, int> _baseValueChains;

    private string _refCode;
    private long _fId;
    private long? _date;

    internal GenerateCodeContext(ICustomGenCodeHelperService customGenCodeHelper, ICurrentContextService currentContextService, IDictionary<string, int> baseValueChains = null)
    {
        _customGenCodeHelper = customGenCodeHelper;
        _currentContextService = currentContextService;
        _baseValueChains = baseValueChains;
    }

    /// <summary>
    /// Set parameters which has configured code structure
    /// </summary>
    /// <param name="targetObjectTypeId"></param>
    /// <param name="configObjectTypeId"></param>
    /// <param name="configObjectId"></param>
    /// <returns></returns>
    public IGenerateCodeConfig SetConfig(EnumObjectType targetObjectTypeId, EnumObjectType? configObjectTypeId = null, long configObjectId = 0)
    {
        _targetObjectTypeId = targetObjectTypeId;
        _configObjectTypeId = configObjectTypeId ?? targetObjectTypeId;
        _configObjectId = configObjectId;
        return this;
    }


    /// <summary>
    /// Set replace data for generate code structure
    /// </summary>
    /// <param name="fId">ID of current object</param>
    /// <param name="date">%DATE(xxx)%</param>
    /// <param name="parentCode">%CODE%</param>
    /// <returns></returns>
    public IGenerateCodeAction SetConfigData(long fId, long? date = null, string parentCode = "")
    {
        _refCode = parentCode;
        _fId = fId;
        _date = date;
        return this;
    }

    /// <summary>
    /// Validate existed code if currentCode is not empty, try to generate code (10 times) if currentCode is empty
    /// </summary>
    /// <typeparam name="TSource">Entity</typeparam>
    /// <param name="query">DbSet<Entity></param>
    /// <param name="currentCode">currentCode</param>
    /// <param name="checkExisted">expression to check if code/ generated code had been existed</param>
    /// <param name="checkExistedFormat">Raw sql to check if code/ generated code had been existed</param>
    /// <returns></returns>
    public async Task<string> TryValidateAndGenerateCode<TSource>(DbSet<TSource> query, string currentCode, Expression<Func<TSource, string, bool>> checkExisted, Func<string, TSource> checkExistedFormat = null) where TSource : class
    {
        TSource existedItem;

        if (!string.IsNullOrWhiteSpace(currentCode))
        {

            existedItem = await GetExistedItem(query, currentCode, checkExisted, checkExistedFormat);
            if (existedItem != null) throw new BadRequestException(GeneralCode.ItemCodeExisted);
            return currentCode;
        }
        else
        {


            var date = _date ?? _currentContextService.GetNowUtc().GetUnix();

            var config = await _customGenCodeHelper.CurrentConfig(_targetObjectTypeId, _configObjectTypeId, _configObjectId, _fId, _refCode, date);

            if (config == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết lập cấu hình sinh mã");

            configBaseValue = config.CurrentLastValue;


            var lastValue = config.CurrentLastValue.LastValue;
            var genChainKey = config.CustomGenCodeId + "|" + config.CurrentLastValue.BaseValue;

            if (_baseValueChains?.ContainsKey(genChainKey) == true)
            {
                lastValue = _baseValueChains[genChainKey];
            }

            int dem = 0;
            string code;
            do
            {
                code = (await _customGenCodeHelper.GenerateCode(config.CustomGenCodeId, lastValue, _fId, _refCode, date))?.CustomCode;
                existedItem = await GetExistedItem(query, code, checkExisted, checkExistedFormat);
                if (existedItem != null)
                {
                    await ConfirmCode();
                }
                lastValue++;

                if (_baseValueChains != null)
                {
                    if (_baseValueChains.ContainsKey(genChainKey))
                    {
                        _baseValueChains[genChainKey] = lastValue;
                    }
                    else
                    {
                        _baseValueChains.Add(genChainKey, lastValue);
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

    private async Task<TSource> GetExistedItem<TSource>(DbSet<TSource> query, string code, Expression<Func<TSource, string, bool>> checkExisted, Func<string, TSource> checkExistedFormat) where TSource : class
    {
        if (checkExisted == null) return null;

        if (checkExistedFormat != null)
        {
            return checkExistedFormat.Invoke(code);
        }

        var entityParameter = Expression.Parameter(typeof(TSource), "p");

        //Expression<Func<string>> codeFunction = () => code;
        //var codeParametter = Expression.Invoke(codeFunction);
        var codeParametter = Expression.Property(Expression.Constant(new { code }), "code");

        Expression<Func<TSource, bool>> conditionExpression = Expression.Lambda<Func<TSource, bool>>(Expression.Invoke(checkExisted, entityParameter, codeParametter), entityParameter);

        return await query.Where(conditionExpression).FirstOrDefaultAsync();
    }

    private CustomGenCodeBaseValueModel configBaseValue;

    /// <summary>
    /// Confirm code value has used
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ConfirmCode()
    {
        if (configBaseValue.IsNullOrEmptyObject()) return true;

        return await _customGenCodeHelper.ConfirmCode(configBaseValue);
    }
}

public interface IGenerateCodeContext
{
    IGenerateCodeConfig SetConfig(EnumObjectType targetObjectTypeId, EnumObjectType? configObjectTypeId = null, long configObjectId = 0);

    Task<bool> ConfirmCode();
}
public interface IGenerateCodeConfig
{
    IGenerateCodeAction SetConfigData(long fId, long? date = null, string parentCode = "");
}

public interface IGenerateCodeAction
{
    Task<string> TryValidateAndGenerateCode<TSource>(DbSet<TSource> query, string currentCode, Expression<Func<TSource, string, bool>> checkExisted, Func<string, TSource> checkExistedFormat = null) where TSource : class;
}
