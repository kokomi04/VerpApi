using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IActivityLogService
    {
        Task<bool> CreateLog(EnumObjectType objectTypeId, long objectId, string message, string jsonData, EnumAction? action = null);
    }

    public class ActivityLogService : IActivityLogService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly ICurrentContextService _currentContext;
        private readonly GrpcProto.Protos.InternalActivityLog.InternalActivityLogClient _internalActivityLogClient;

        public ActivityLogService(IHttpCrossService httpCrossService, ILogger<ActivityLogService> logger, IOptionsSnapshot<AppSetting> appSetting, ICurrentContextService currentContext, GrpcProto.Protos.InternalActivityLog.InternalActivityLogClient internalActivityLogClient)
        {
            _httpCrossService = httpCrossService;
            _logger = logger;
            _appSetting = appSetting.Value;
            _currentContext = currentContext;
            _internalActivityLogClient = internalActivityLogClient;
        }

        public async Task<bool> CreateLog(EnumObjectType objectTypeId, long objectId, string message, string jsonData, EnumAction? action = null)
        {
            try
            {
                if (_appSetting.GrpcInternal?.Address?.Contains("https") == true)
                {
                    var headers = new Metadata();
                    headers.Add(Headers.CrossServiceKey, _appSetting?.Configuration?.InternalCrossServiceKey);

                    var reulst = await _internalActivityLogClient.LogAsync(new GrpcProto.Protos.ActivityInput
                    {
                        UserId = _currentContext.UserId,
                        ActionId = action == null ? (int)_currentContext.Action : (int)action.Value,
                        ObjectTypeId = (int)objectTypeId,
                        ObjectId = objectId,
                        MessageTypeId = (int)EnumMessageType.ActivityLog,
                        Message = message,
                        Data = jsonData,
                        SubsidiaryId = _currentContext.SubsidiaryId
                    }, headers);

                    return (bool)(reulst?.IsSuccess);
                }

                var body = new ActivityInput
                {
                    UserId = _currentContext.UserId,
                    ActionId = _currentContext.Action,
                    ObjectTypeId = objectTypeId,
                    ObjectId = objectId,
                    SubsidiaryId = _currentContext.SubsidiaryId,
                    MessageTypeId = EnumMessageType.ActivityLog,
                    Message = message,
                    Data = jsonData
                };


                return await _httpCrossService.Post<bool>($"/api/internal/InternalActivityLog/Log", body);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ActivityLogService:CreateLog");
                return false;
            }
        }

    }
}
