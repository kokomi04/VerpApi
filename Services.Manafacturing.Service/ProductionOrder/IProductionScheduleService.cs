﻿//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using VErp.Infrastructure.EF.EFExtensions;
//using VErp.Infrastructure.ServiceCore.Model;
//using VErp.Services.Manafacturing.Model.ProductionOrder;
//using VErp.Commons.Enums.Manafacturing;

//namespace VErp.Services.Manafacturing.Service.ProductionOrder
//{
//    public interface IProductionScheduleService
//    {
//        Task<IList<ProductionPlanningOrderModel>> GetProductionPlanningOrders();
//        Task<IList<ProductionPlanningOrderDetailModel>> GetProductionPlanningOrderDetail(int productionOrderId);
//        Task<IList<ProductionScheduleModel>> GetProductionSchedules(long scheduleTurnId);
//        Task<IList<ProductionScheduleModel>> GetProductionSchedulesByProductionOrderDetail(long productionOrderDetailId);
//        Task<PageData<ProductionScheduleModel>> GetProductionSchedules(string keyword, long fromDate, long toDate, int page, int size, string orderByFieldName, bool asc, Clause filters = null);
//        Task<List<ProductionScheduleInputModel>> UpdateProductionSchedule(List<ProductionScheduleInputModel> data);
//        Task<List<ProductionScheduleInputModel>> CreateProductionSchedule(List<ProductionScheduleInputModel> data);
//        Task<bool> DeleteProductionSchedule(long[] productionScheduleIds);

//        Task<bool> UpdateProductionScheduleStatus(long scheduleTurnId, ProductionScheduleStatusModel status);
//        Task<bool> UpdateManualProductionScheduleStatus(long productionScheduleId, ProductionScheduleStatusModel status);
//        Task<IList<ProductionScheduleModel>> GetProductionSchedulesByScheduleTurnArray(long[] scheduleTurnIds);
//    }
//}
