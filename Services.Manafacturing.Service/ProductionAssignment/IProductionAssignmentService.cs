using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Service.ProductionAssignment
{
    public interface IProductionAssignmentService
    {
        Task<IList<ProductionAssignmentModel>> GetProductionAssignments(long scheduleTurnId);
        Task<bool> UpdateProductionAssignment( long productionStepId, long scheduleTurnId, ProductionAssignmentModel[] data);

        Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment(int departmentId, long? scheduleTurnId, int page, int size, string orderByFieldName, bool asc);

        Task<bool> SetProductionStepWorldload(IList<ProductionStepWorkload> productionStepWorldload);
    }
}
