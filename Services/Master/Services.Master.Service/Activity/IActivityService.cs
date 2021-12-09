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
        Task<long> CreateActivityTask(ActivityInput input);

        Task<bool> AddNote(int? billTypeId, long objectId, int objectTypeId, string message);

        Task<PageData<UserActivityLogOuputModel>> GetListUserActivityLog(int? billTypeId, long objectId, EnumObjectType objectTypeId, int pageIdex = 1, int pageSize = 20);
    }
}
