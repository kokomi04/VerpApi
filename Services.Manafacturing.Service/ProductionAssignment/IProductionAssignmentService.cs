using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Service.ProductionAssignment
{
    public interface IProductionAssignmentService
    {
        Task<IList<ProductionAssignmentModel>> GetProductionAssignments(long productionOrderId);
        Task<ProductionAssignmentModel> GetProductionAssignment(long productionOrderId, long productionStepId, int departmentId);

        Task<bool> UpdateProductionAssignment(long productionOrderId, long productionStepId, ProductionAssignmentModel[] data, ProductionStepWorkInfoInputModel info);
        Task<bool> UpdateProductionAssignment(long productionOrderId, GeneralAssignmentModel data);
        Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment(int departmentId, string keyword, long? productionOrderId, int page, int size, string orderByFieldName, bool asc, long? fromDate, long? toDate);

        
        Task<IDictionary<int, Dictionary<int, ProductivityModel>>> GetGeneralProductivityDepartments();
        Task<CapacityOutputModel> GetGeneralCapacityDepartments(long productionOrderId);

        Task<IList<ProductionStepWorkInfoOutputModel>> GetListProductionStepWorkInfo(long productionOrderId);

        Task<bool> ChangeAssignedProgressStatus(long productionOrderId, long productionStepId, int departmentId, EnumAssignedProgressStatus status);

    }
}
