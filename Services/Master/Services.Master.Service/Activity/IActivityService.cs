using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Service.Activity
{
    public interface IActivityService
    {
        Task<Enum> CreateActivity(EnumObjectType objectTypeId, long objectId, string message, string oldJsonObject, object newObject);
    }
}
