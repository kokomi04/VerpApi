using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover
{
    public interface IMaterialAllocationService
    {
        Task<IList<IgnoreAllocationModel>> GetIgnoreAllocations(long productionOrderId);

        Task<IList<MaterialAllocationModel>> GetMaterialAllocations(long productionOrderId);

        Task<AllocationModel> UpdateMaterialAllocation(long productionOrderId, AllocationModel data);
    }
}
