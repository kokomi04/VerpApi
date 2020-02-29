using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Services.Master.Service.Activity
{
    public interface IActivityService
    {
        void CreateActivityAsync(ActivityInput input);
        Task<Enum> CreateActivityTask(ActivityInput input);
    }
}
