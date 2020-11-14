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
        Task<IList<ProductionAssignmentModel>> UpdateProductionAssignment(ProductionAssignmentModel data);
        Task<IList<ProductionAssignmentModel>> CreateProductionAssignment(ProductionAssignmentModel data);
        Task<bool> DeleteProductionAssignment(int productionAssignmentId);
    }
}
