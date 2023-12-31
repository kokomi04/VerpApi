﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper.General
{
    public interface IOutsideMappingHelperService
    {
        //Task<OutsideImportMappingObjectModel> MappingObjectInfo(string mappingFunctionKey, string objectId);

        Task<bool> MappingObjectCreate(string mappingFunctionKey, string objectId, EnumObjectType billObjectTypeId, long billFId);

        Task<bool> MappingObjectDelete(EnumObjectType billObjectTypeId, long billFId);
    }


    public class OutsideMappingHelperService : IOutsideMappingHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;

        public OutsideMappingHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<OutsideMappingHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }


        //public async Task<OutsideImportMappingObjectModel> MappingObjectInfo(string mappingFunctionKey, string objectId)
        //{
        //    return await _httpCrossService.Get<OutsideImportMappingObjectModel>($"api/internal/InternalOutsideImportMappings/MappingObjectInfo", new { mappingFunctionKey, objectId });
        //}

        public async Task<bool> MappingObjectCreate(string mappingFunctionKey, string objectId, EnumObjectType billObjectTypeId, long billFId)
        {
            return await _httpCrossService.Post<bool>($"api/internal/InternalOutsideImportMappings/MappingObjectCreate",
                new MappingObjectCreateRequest
                {
                    MappingFunctionKey = mappingFunctionKey,
                    ObjectId = objectId,
                    BillObjectTypeId = billObjectTypeId,
                    BillId = billFId
                });
        }

        public async Task<bool> MappingObjectDelete(EnumObjectType billObjectTypeId, long billFId)
        {
            return await _httpCrossService.Deleted<bool>($"api/internal/InternalOutsideImportMappings/MappingObjectDelete/{(int)billObjectTypeId}/{billFId}", new { });
        }
    }
}
