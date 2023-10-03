using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;

namespace VErp.Services.Manafacturing.Service.ProductionAssignment
{
    public interface IProductionAssignmentService
    {
        Task<bool> DismissUpdateWarning(long productionOrderId);
        Task<IList<ProductionAssignmentModel>> GetProductionAssignments(long productionOrderId);
        Task<IList<ProductionAssignmentModel>> GetByProductionOrders(IList<long> productionOrderIds);
        Task<IList<ProductionAssignmentModel>> GetByDateRange(long fromDate, long toDate);

        Task<ProductionAssignmentModel> GetProductionAssignment(long productionOrderId, long productionStepId, int departmentId);
        /*
        Task<bool> UpdateProductionAssignment(long productionOrderId, long productionStepId, ProductionAssignmentModel[] data, ProductionStepWorkInfoInputModel info);
        */

        Task DeleteAssignmentRef(long productionOrderId, IList<ProductionAssignmentEntity> deletedProductionStepAssignments);

        Task<bool> UpdateProductionAssignment(long productionOrderId, GeneralAssignmentModel data);

        Task<bool> UpdateDepartmentAssignmentDate(int departmentId, IList<DepartmentAssignUpdateDateModel> data);
        

        Task<IList<DepartmentAssignFreeDate>> DepartmentsFreeDates(DepartmentAssignFreeDateInput req);

        Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment(int departmentId, string keyword, long? productionOrderId, int page, int size, string orderByFieldName, bool asc, long? fromDate, long? toDate);


        //Task<IDictionary<int, Dictionary<int, ProductivityModel>>> GetGeneralProductivityDepartments();
        Task<CapacityOutputModel> GetGeneralCapacityDepartments(long productionOrderId);

        Task<IList<ProductionStepWorkInfoOutputModel>> GetListProductionStepWorkInfo(long productionOrderId);

        Task<bool> ChangeAssignedProgressStatus(long productionOrderId, long productionStepId, int departmentId, EnumAssignedProgressStatus status);

        //Task UpdateProductionOrderAssignmentStatus(IList<long> productionOrderIds);

    }
}
