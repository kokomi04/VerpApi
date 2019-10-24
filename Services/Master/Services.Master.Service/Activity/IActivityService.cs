using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Service.Activity
{
    public interface IActivityService
    {
        void CreateActivityAsync(EnumObjectType objectTypeId, long objectId, string message, string oldJsonObject, object newObject);
        Task<Enum> CreateActivity(EnumObjectType objectTypeId, long objectId, string message, string oldJsonObject, object newObject);
    }
}
