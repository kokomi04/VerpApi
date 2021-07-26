using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ServiceCore.Service;
using static VErp.Infrastructure.ServiceCore.Service.ActivityLogService;

namespace VErp.Infrastructure.ServiceCore.Facade
{
    public class ObjectActivityLogFacade
    {
        private readonly EnumObjectType objectTypeId;
        private readonly IActivityLogService activityLogService;

        public ObjectActivityLogFacade(EnumObjectType objectTypeId, IActivityLogService activityLogService)
        {
            this.objectTypeId = objectTypeId;
            this.activityLogService = activityLogService;
        }

        public Task<bool> CreateLog(long objectId, string message, string jsonData, EnumActionType? action = null, bool ignoreBatch = false, string messageResourceName = "", string messageResourceFormatData = "")
        {
            return activityLogService.CreateLog(objectTypeId, objectId, message, jsonData, action, ignoreBatch, messageResourceName, messageResourceFormatData);
        }

        public Task<bool> CreateLog<T>(long objectId, Expression<Func<T>> messageResourceName, object[] messageResourceFormatData, string jsonData, EnumActionType? action = null, bool ignoreBatch = false)
        {
            return activityLogService.CreateLog(objectTypeId, objectId, messageResourceName, jsonData, action, ignoreBatch, messageResourceFormatData);
        }

        public ActivityLogBatchs BeginBatchLog()
        {
            return activityLogService.BeginBatchLog();
        }

        public ObjectActivityLogModelBuilder<T> LogBuilder<T>(Expression<Func<T>> messageResourceName)
        {
            return new ObjectActivityLogModelBuilder<T>(this, messageResourceName);
        }
    }

    public class ObjectActivityLogModelBuilder<T>
    {
        private long objectId;
        private Expression<Func<T>> messageResourceName;
        private object[] messageResourceFormatData;
        private string jsonData;
        private EnumActionType? action = null;
        private bool ignoreBatch  = false;

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


        public ObjectActivityLogModelBuilder<T> JsonData(string jsonData)
        {
            this.jsonData = jsonData;
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
            await facade.CreateLog<T>(objectId, messageResourceName, messageResourceFormatData, jsonData, action, ignoreBatch);
        }
    }

}
