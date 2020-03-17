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
        Task<Enum> CreateActivityTask(ActivityInput input);

        Task<Enum> CreateUserActivityLog(long objectId, int objectTypeId, int userId, int actionTypeId, EnumMessageType messageTypeId, string message);

        Task<PageData<UserActivityLogOuputModel>> GetListUserActivityLog(long objectId, int objectTypeId, int pageIdex = 1, int pageSize = 20);
    }
}
