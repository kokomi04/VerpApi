using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.StatusProcess
{
    public interface IStatusProcessService
    {
        Task<IList<ProductionInventoryRequirementModel>> GetProductionInventoryRequirements(long productionOrderId);
        Task<IList<DepartmentHandoverDetailModel>> GetDepartmentHandoverDetail(long productionOrderId, long? productionStepId = null, int? departmentId = null, IList<ProductionInventoryRequirementEntity> inventories = null);
        Task<bool> ChangeAssignedProgressStatus(string productionOrderCode, string inventoryCode, IList<ProductionInventoryRequirementEntity> inventories = null);
        Task<bool> ChangeAssignedProgressStatus(long productionOrderId, long productionStepId, int departmentId, IList<ProductionInventoryRequirementEntity> inventories = null, IList<DepartmentHandoverDetailModel> departmentHandoverDetails = null);
        Task<bool> UpdateFullAssignedProgressStatus(long productionOrderId);
    }
}
