using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover
{
    public interface IMaterialAllocationService
    {
        Task<ConflictHandoverModel> GetConflictHandovers(long productionOrderId);

        Task<IList<IgnoreAllocationModel>> GetIgnoreAllocations(long productionOrderId);

        Task<IList<MaterialAllocationModel>> GetMaterialAllocations(long productionOrderId);

        Task<AllocationModel> UpdateMaterialAllocation(long productionOrderId, AllocationModel data);


        //Task<bool> UpdateIgnoreAllocation(string[] productionOrderCodes, bool ignoreEnqueueUpdateProductionOrderStatus = false);
    }
}
