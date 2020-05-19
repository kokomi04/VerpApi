using Services.Organization.Model.BusinessInfo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;

namespace Services.Organization.Service.BusinessInfo
{
    public interface IObjectProcessService
    {
        IList<ObjectProcessInfoModel> ObjectProcessList();

        Task<IList<ObjectProcessInfoStepListModel>> ObjectProcessSteps(EnumObjectProcessType objectProcessTypeId);

        Task<int> ObjectProcessStepCreate(EnumObjectProcessType objectProcessTypeId, ObjectProcessInfoStepModel model);

        Task<bool> ObjectProcessStepUpdate(EnumObjectProcessType objectProcessTypeId, int objectProcessStepId, ObjectProcessInfoStepModel model);

        Task<bool> ObjectProcessStepDelete(EnumObjectProcessType objectProcessTypeId, int objectProcessStepId);

    }
}
