﻿using GrpcProto.Protos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface ICustomGenCodeHelperService
    {
        //Task<bool> MapObjectCustomGenCode(EnumObjectType objectTypeId, Dictionary<int, int> data);
        Task<bool> MapObjectCustomGenCode(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, Dictionary<long, int> objectCustomGenCodes);

        Task<CustomGenCodeOutputModelOut> CurrentConfig(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, long configObjectId);
        Task<CustomCodeModelOutput> GenerateCode(int customGenCodeId, int lastValue);
        Task<bool> ConfirmCode(int customGenCodeId);
    }

    public class CustomGenCodeHelperService : ICustomGenCodeHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        // private readonly CustomGenCodeProvider.CustomGenCodeProviderClient _customGenCodeClient;
        public CustomGenCodeHelperService(IHttpCrossService httpCrossService,
            IOptions<AppSetting> appSetting,
            ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
            //_customGenCodeClient = customGenCodeClient;
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

        public async Task<CustomGenCodeOutputModelOut> CurrentConfig(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, long configObjectId)
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
            return await _httpCrossService.Get<CustomGenCodeOutputModelOut>($"api/internal/InternalCustomGenCode/currentConfig?targetObjectTypeId={(int)targetObjectTypeId}&configObjectTypeId={(int)configObjectTypeId}&configObjectId={configObjectId}");
        }

        public async Task<CustomCodeModelOutput> GenerateCode(int customGenCodeId, int lastValue)
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
            return await _httpCrossService.Get<CustomCodeModelOutput>($"api/internal/InternalCustomGenCode/generateCode?customGenCodeId={customGenCodeId}&lastValue={lastValue}");
        }

        public async Task<bool> ConfirmCode(int customGenCodeId)
        {
            if (customGenCodeId <= 0) return true;
            //if(_appSetting.GrpcInternal?.Address?.Contains("https") == true)
            //{
            //    return (await _customGenCodeClient.ConfirmCodeAsync(new ConfirmCodeRequest
            //    {
            //        ObjectId = objectId,
            //        ObjectTypeId = (int)objectTypeId
            //    })).IsSuccess;
            //}
            return await _httpCrossService.Put<bool>($"api/internal/InternalCustomGenCode/confirmCode?customGenCodeId={customGenCodeId}", null);
        }

    }
}
