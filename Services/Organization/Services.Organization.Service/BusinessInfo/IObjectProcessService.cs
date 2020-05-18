using Services.Organization.Model.BusinessInfo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;

namespace Services.Organization.Service.BusinessInfo
{
    interface IObjectProcessService
    {
        IList<ObjectProcessInfoModel> ObjectProcessList();

        Task<IList<ObjectProcessInfoStepModel>> ObjectProcessSteps(EnumObjectProcessType objectProcessTypeId);

        Task<bool> ObjectProcessUpdate(EnumObjectProcessType objectProcessTypeId, IList<ObjectProcessInfoStepModel> data);
        
    }
}
