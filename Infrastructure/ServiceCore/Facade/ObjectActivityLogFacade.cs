﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Service;
using static VErp.Infrastructure.ServiceCore.Service.ActivityLogService;

namespace VErp.Infrastructure.ServiceCore.Facade
{
    public class ObjectActivityLogFacade
    {
        private readonly EnumObjectType? _objectTypeId;
        private readonly IActivityLogService _activityLogService;

        public ObjectActivityLogFacade(EnumObjectType? objectTypeId, IActivityLogService activityLogService)
        {
            this._objectTypeId = objectTypeId;
            this._activityLogService = activityLogService;
        }


        public Task<bool> CreateLogV1(long objectId, string message, object data, EnumActionType? action = null, bool ignoreBatch = false, string messageResourceName = "", string messageResourceFormatData = "", EnumObjectType? objectTypeId = null, int? billTypeId = null)
        {
            objectTypeId = objectTypeId ?? _objectTypeId;
            if (!objectTypeId.HasValue) throw new Exception("Invalid activity log object type");
            return _activityLogService.CreateActivityLog(objectTypeId.Value, objectId, message, data, action, ignoreBatch, messageResourceName, messageResourceFormatData, billTypeId);
        }

        public Task<bool> CreateLog<T>(long objectId, Expression<Func<T>> messageResourceName, object[] messageResourceFormatData, object data, EnumActionType? action = null, bool ignoreBatch = false, EnumObjectType? objectTypeId = null, int? billTypeId = null)
        {
            objectTypeId = objectTypeId ?? _objectTypeId;
            if (!objectTypeId.HasValue) throw new Exception("Invalid activity log object type");
            return _activityLogService.CreateActivityLog(objectTypeId.Value, objectId, messageResourceName, data, action, ignoreBatch, messageResourceFormatData, billTypeId);
        }

        public ActivityLogBatchs BeginBatchLog()
        {
            return _activityLogService.BeginBatchLog();
        }

        public ObjectActivityLogModelBuilder<T> LogBuilder<T>(Expression<Func<T>> messageResourceName)
        {
            return new ObjectActivityLogModelBuilder<T>(this, messageResourceName);
        }
    }

    public class ObjectActivityLogModelBuilder<T>
    {
        public const int JSON_ACTIVITY_LOG_MAX_DEPTH = 3;
        private long objectId;
        private Expression<Func<T>> messageResourceName;
        private object[] messageResourceFormatData;
        private string jsonData;
        private EnumActionType? action = null;
        private bool ignoreBatch = false;
        private EnumObjectType? objectTypeId;
        private int? billTypeId = null;

        private readonly ObjectActivityLogFacade facade;

        public ObjectActivityLogModelBuilder(ObjectActivityLogFacade facade, Expression<Func<T>> messageResourceName)
        {
            this.facade = facade;
            this.messageResourceName = messageResourceName;
        }

        public ObjectActivityLogModelBuilder<T> ObjectId(long objectId)
        {
            this.objectId = objectId;
            return this;
        }


        public ObjectActivityLogModelBuilder<T> BillTypeId(int billTypeId)
        {
            this.billTypeId = billTypeId;
            return this;
        }

        public ObjectActivityLogModelBuilder<T> ObjectType(EnumObjectType objectTypeId)
        {
            this.objectTypeId = objectTypeId;
            return this;
        }

        public ObjectActivityLogModelBuilder<T> MessageResourceName(Expression<Func<T>> messageResourceName)
        {
            this.messageResourceName = messageResourceName;
            return this;
        }
        public ObjectActivityLogModelBuilder<T> MessageResourceFormatData(object[] messageResourceFormatData)
        {
            this.messageResourceFormatData = messageResourceFormatData;
            return this;
        }

        public ObjectActivityLogModelBuilder<T> MessageResourceFormatDatas(params object[] datas)
        {
            this.messageResourceFormatData = datas;
            return this;
        }
        public ObjectActivityLogModelBuilder<T> JsonData(object data)
        {
            this.jsonData = JsonSerialize(data);
            return this;
        }
        public ObjectActivityLogModelBuilder<T> Action(EnumActionType? action)
        {
            this.action = action;
            return this;
        }
        public ObjectActivityLogModelBuilder<T> IgnoreBatch(bool ignoreBatch)
        {
            this.ignoreBatch = ignoreBatch;
            return this;
        }

        public async Task CreateLog()
        {
            await facade.CreateLog<T>(objectId, messageResourceName, messageResourceFormatData, jsonData, action, ignoreBatch, objectTypeId, billTypeId);
        }
        private string JsonSerialize(object obj)
        {
            if (obj != null)
            {
                if (obj is string)
                {
                    if (JsonUtils.IsValidJson(obj.ToString()))
                    {
                        return obj.ToString();
                    }
                }

                return JsonUtils.GetJobjectNoneLoopDeep(obj, new Stack<object>(), 1, JSON_ACTIVITY_LOG_MAX_DEPTH).JsonSerialize();

            }
            return null;
        }
    }

}
