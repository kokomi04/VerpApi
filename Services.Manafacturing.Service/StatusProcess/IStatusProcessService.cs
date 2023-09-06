using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.StatusProcess
{
    public interface IStatusProcessService
    {
        Task<IList<ProductionInventoryRequirementModel>> GetProductionInventoryRequirements(long productionOrderId);
        Task<IList<DepartmentHandoverDetailModel>> GetDepartmentHandoverDetail(long productionOrderId, long? productionStepId = null, int? departmentId = null, IList<InternalProductionInventoryRequirementModel> inventories = null);
        Task<bool> ChangeAssignedProgressStatus(string productionOrderCode, string description, IList<InternalProductionInventoryRequirementModel> inventories = null);
        Task<bool> ChangeAssignedProgressStatus(long productionOrderId, long productionStepId, int departmentId, IList<InternalProductionInventoryRequirementModel> inventories = null, IList<DepartmentHandoverDetailModel> departmentHandoverDetails = null);
        //Task<bool> UpdateFullAssignedProgressStatus(long productionOrderId);

        Task<IList<DepartmentHandoverDetailModel>> GetDepartmentHandoverDetailByArrayProductionOrderId(IList<RequestObjectGetProductionOrderHandover> requestObject, int? departmentId = null);
    }
}
