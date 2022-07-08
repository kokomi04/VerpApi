using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionAssignment;

namespace VErp.Services.Manafacturing.Service.ProductionAssignment
{
    public interface IProductionAssignmentService
    {
        Task<bool> DismissUpdateWarning(long productionOrderId);
        Task<IList<ProductionAssignmentModel>> GetProductionAssignments(long productionOrderId);
        Task<IList<ProductionAssignmentModel>> GetByProductionOrders(IList<long> productionOrderIds);

        Task<ProductionAssignmentModel> GetProductionAssignment(long productionOrderId, long productionStepId, int departmentId);

        Task<bool> UpdateProductionAssignment(long productionOrderId, long productionStepId, ProductionAssignmentModel[] data, ProductionStepWorkInfoInputModel info);
        Task<bool> UpdateProductionAssignment(long productionOrderId, GeneralAssignmentModel data);

        Task<bool> UpdateDepartmentAssignmentDate(int departmentId, IList<DepartmentAssignUpdateDateModel> data);

        Task<DepartmentAssignFreeDate> DepartmentFreeDate(int departmentId);

        Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment(int departmentId, string keyword, long? productionOrderId, int page, int size, string orderByFieldName, bool asc, long? fromDate, long? toDate);


        Task<IDictionary<int, Dictionary<int, ProductivityModel>>> GetGeneralProductivityDepartments();
        Task<CapacityOutputModel> GetGeneralCapacityDepartments(long productionOrderId);

        Task<IList<ProductionStepWorkInfoOutputModel>> GetListProductionStepWorkInfo(long productionOrderId);

        Task<bool> ChangeAssignedProgressStatus(long productionOrderId, long productionStepId, int departmentId, EnumAssignedProgressStatus status);

    }
}
