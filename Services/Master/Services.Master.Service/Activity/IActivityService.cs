using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Activity;

namespace VErp.Services.Master.Service.Activity
{
    public interface IActivityService
    {
        void CreateActivityAsync(ActivityInput input);
        Task<long> CreateActivityTask(ActivityInput input);

        Task<bool> AddNote(int? billTypeId, long objectId, int objectTypeId, string message);

        Task<PageData<UserActivityLogOuputModel>> GetUserLogByObject(int? billTypeId, long objectId, EnumObjectType objectTypeId, int pageIdex = 1, int pageSize = 20);
        Task<PageData<UserActivityLogOuputModel>> GetListUserActivityLog(string keyword, long? fromDate, long? toDate, int? userId, int? billTypeId, long? objectId, EnumObjectType? objectTypeId, int? actionTypeId, string sortBy, bool asc, int page = 1, int size = 20);

        Task<IList<UserActivityLogOuputModel>> GetListUserActivityLogByArrayId(long[] arrActivityLogId);

        Task<PageData<UserLoginLogModel>> GetUserLoginLogs(int pageIdex, int pageSize, string keyword, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters);
    }
}
