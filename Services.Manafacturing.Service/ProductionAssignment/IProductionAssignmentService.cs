using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionAssignment;

namespace VErp.Services.Manafacturing.Service.ProductionAssignment
{
    public interface IProductionAssignmentService
    {
        Task<IList<ProductionAssignmentModel>> GetProductionAssignments(long scheduleTurnId);
        Task<bool> UpdateProductionAssignment( long productionStepId, long scheduleTurnId, ProductionAssignmentModel[] data);

        Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment(int departmentId, int page, int size, string orderByFieldName, bool asc);

        Task<IList<DepartmentProductionAssignmentDetailModel>> DepartmentScheduleTurnAssignment(int departmentId, long scheduleTurnId);
    }
}
