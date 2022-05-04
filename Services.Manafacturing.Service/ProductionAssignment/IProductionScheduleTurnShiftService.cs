//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using VErp.Services.Manafacturing.Model.ProductionAssignment;

//namespace VErp.Services.Manafacturing.Service.ProductionAssignment
//{
//    public interface IProductionScheduleTurnShiftService
//    {
//        Task<IDictionary<long, List<ProductionScheduleTurnShiftModel>>> GetShifts(int departmentId, long productionOrderId, long[] productionStepIds);
//        Task<long> CreateShift(int departmentId, long productionOrderId, long productionStepId, ProductionScheduleTurnShiftModel model);
//        Task<bool> UpdateShift(int departmentId, long productionOrderId, long productionStepId, long productionScheduleTurnShiftId, ProductionScheduleTurnShiftModel model);
//        Task<bool> DeleteShift(int departmentId, long productionOrderId, long productionStepId, long productionScheduleTurnShiftId);
//    }
//}
