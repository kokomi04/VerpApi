﻿using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using static VErp.Infrastructure.ServiceCore.Service.ActivityLogService;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IActivityLogService
    {
        ObjectActivityLogFacade CreateObjectTypeActivityLog(EnumObjectType? objectTypeId);

        Task<bool> CreateLog(EnumObjectType objectTypeId, long objectId, string message, string jsonData, EnumActionType? action = null, bool ignoreBatch = false, string messageResourceName = "", string messageResourceFormatData = "", int? billTypeId = null);

        Task<bool> CreateLog<T>(EnumObjectType objectTypeId, long objectId, Expression<Func<T>> messageResourceName, string jsonData, EnumActionType? action = null, bool ignoreBatch = false, object[] messageResourceFormatData = null, int? billTypeId = null);

        ActivityLogBatchs BeginBatchLog();

    }


    public class ActivityLogService : IActivityLogService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly ICurrentContextService _currentContext;
        private readonly GrpcProto.Protos.InternalActivityLog.InternalActivityLogClient _internalActivityLogClient;
        private readonly object objLock = new object();
        private readonly HashSet<ActivityLogBatchs> _activityLogBatchs;

        public ActivityLogService(IHttpCrossService httpCrossService, ILogger<ActivityLogService> logger, IOptionsSnapshot<AppSetting> appSetting, ICurrentContextService currentContext, GrpcProto.Protos.InternalActivityLog.InternalActivityLogClient internalActivityLogClient)
        {
            _httpCrossService = httpCrossService;
            _logger = logger;
            _appSetting = appSetting.Value;
            _currentContext = currentContext;
            _internalActivityLogClient = internalActivityLogClient;
            _activityLogBatchs = new HashSet<ActivityLogBatchs>();
        }

        public ActivityLogBatchs BeginBatchLog()
        {
            var logBatchs = new ActivityLogBatchs(this, _activityLogBatchs);
            return logBatchs;
        }

        public async Task<bool> CreateLog(EnumObjectType objectTypeId, long objectId, string message, string jsonData, EnumActionType? action = null, bool ignoreBatch = false, string messageResourceName = "", string messageResourceFormatData = "", int? billTypeId = null)
        {
            if (ignoreBatch)
            {
                return await CreateLogRequest(objectTypeId, objectId, message, jsonData, action, messageResourceName, messageResourceFormatData, billTypeId);
            }

            ActivityLogBatchs batch = null;
            lock (objLock)
            {
                if (_activityLogBatchs.Count > 0)
                {
                    batch = _activityLogBatchs.Last();
                }
            }
            if (batch == null)
            {
                return await CreateLogRequest(objectTypeId, objectId, message, jsonData, action, messageResourceName, messageResourceFormatData, billTypeId);
            }
            else
            {
                batch.AddLog(objectTypeId, objectId, message, jsonData, action, messageResourceName, messageResourceFormatData, billTypeId);
                return true;
            }
        }

        public async Task<bool> CreateLog<T>(EnumObjectType objectTypeId, long objectId, Expression<Func<T>> messageResourceName, string jsonData, EnumActionType? action = null, bool ignoreBatch = false, object[] messageResourceFormatData = null, int? billTypeId = null)
        {
            var propertyInfo = ((MemberExpression)messageResourceName.Body).Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
            }

            var type = propertyInfo.DeclaringType.FullName + "." + propertyInfo.Name;

            var messageFormat = (string)propertyInfo.GetValue(null);// typeof(T).stat
            if (messageResourceFormatData == null)
                messageResourceFormatData = new object[0];

            var message = messageResourceFormatData.Length > 0 ? string.Format(messageFormat, messageResourceFormatData) : messageFormat;

            return await CreateLog(objectTypeId, objectId, message, jsonData, action, ignoreBatch, type, messageResourceFormatData.JsonSerialize(), billTypeId);
        }

        public ObjectActivityLogFacade CreateObjectTypeActivityLog(EnumObjectType? objectTypeId)
        {
            return new ObjectActivityLogFacade(objectTypeId, this);
        }

        private async Task<bool> CreateLogRequest(EnumObjectType objectTypeId, long objectId, string message, string jsonData, EnumActionType? action = null, string messageResourceName = "", string messageResourceFormatData = "", int? billTypeId = null)
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
                        //BillTypeId = billTypeId,
                        ObjectTypeId = (int)objectTypeId,
                        ObjectId = objectId,
                        MessageTypeId = (int)EnumMessageType.ActivityLog,
                        Message = message,
                        MessageResourceName = messageResourceName,
                        MessageResourceFormatData = messageResourceFormatData,
                        Data = jsonData,
                        SubsidiaryId = _currentContext.SubsidiaryId
                    }, headers);

                    return (bool)(reulst?.IsSuccess);
                }

                var body = new ActivityInput
                {
                    UserId = _currentContext.UserId,
                    ActionId = _currentContext.Action,
                    BillTypeId = billTypeId,
                    ObjectTypeId = objectTypeId,
                    ObjectId = objectId,
                    SubsidiaryId = _currentContext.SubsidiaryId,
                    MessageTypeId = EnumMessageType.ActivityLog,
                    MessageResourceName = messageResourceName,
                    MessageResourceFormatData = messageResourceFormatData,
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

        public class ActivityLogBatchs : IDisposable
        {
            private readonly IActivityLogService _activityLogService;
            private readonly IList<ActivityLogEntity> _logs;
            private readonly HashSet<ActivityLogBatchs> _activityLogBatchs;
            internal ActivityLogBatchs(IActivityLogService activityLogService, HashSet<ActivityLogBatchs> activityLogBatchs)
            {
                this._activityLogService = activityLogService;
                _logs = new List<ActivityLogEntity>();
                _activityLogBatchs = activityLogBatchs;
                _activityLogBatchs.Add(this);
            }

            internal void AddLog(EnumObjectType objectTypeId, long objectId, string message, string jsonData, EnumActionType? action = null, string messageResourceName = "", string messageResourceFormatData = "", int? billTypeId = null)
            {
                _logs.Add(new ActivityLogEntity()
                {
                    BillTypeId = billTypeId,
                    ObjectTypeId = objectTypeId,
                    ObjectId = objectId,
                    Message = message,
                    MessageResourceName = messageResourceName,
                    MessageResourceFormatData = messageResourceFormatData,
                    JsonData = jsonData,
                    Action = action
                });
            }

            public async Task<bool> CommitAsync()
            {
                foreach (var log in _logs)
                {
                    await _activityLogService.CreateLog(log.ObjectTypeId, log.ObjectId, log.Message, log.JsonData, log.Action, true, log.MessageResourceName, log.MessageResourceFormatData, log.BillTypeId);
                }
                return true;
            }

            public void Dispose()
            {
                this._activityLogBatchs.Remove(this);
                //Commit().Wait();
            }

            private class ActivityLogEntity
            {
                public int? BillTypeId { get; set; }
                public EnumObjectType ObjectTypeId { get; set; }
                public long ObjectId { get; set; }
                public string Message { get; set; }
                public string MessageResourceName { get; set; }
                public string MessageResourceFormatData { get; set; }
                public string JsonData { get; set; }
                public EnumActionType? Action { get; set; }
            }

        }

    }
}
