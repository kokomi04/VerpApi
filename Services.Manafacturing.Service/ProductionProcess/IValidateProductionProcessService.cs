using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionProcess;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.ProductionProcess
{
    public interface IValidateProductionProcessService
    {
        Task<IList<ProductionProcessWarningMessage>> ValidateProductionProcess(EnumContainerType containerTypeId, long containerId, ProductionProcessModel productionProcess);
        Task<IList<ProductionProcessWarningMessage>> ValidateOutsourceStepRequest(ProductionProcessModel productionProcess);
        Task<IList<ProductionProcessWarningMessage>> ValidateOutsourcePartRequest(ProductionProcessModel productionProcess);
    }
}
