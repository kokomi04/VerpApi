using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Activity;

namespace VErp.Services.Master.Service.Activity
{
    public interface IActivityService
    {
        void CreateActivityAsync(ActivityInput input);
        Task<bool> CreateActivityTask(ActivityInput input);

        Task<bool> CreateUserActivityLog(long objectId, int objectTypeId, int userId, int subsidiaryId, int actionTypeId, EnumMessageType messageTypeId, string message, string messageResourceName = null, string messageResourceFormatData = null);

        Task<PageData<UserActivityLogOuputModel>> GetListUserActivityLog(long objectId, EnumObjectType objectTypeId, int pageIdex = 1, int pageSize = 20);
    }
}
