﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover
{
    public interface IProductionHandoverService
    {

        Task<PageData<DepartmentHandoverModel>> GetDepartmentHandovers(long departmentId, string keyword, int page, int size, long fromDate, long toDate, int? stepId, int? productId);

        Task<IList<ProductionHandoverModel>> GetProductionHandovers(long productionOrderId);
        Task<IList<ProductionInventoryRequirementModel>> GetProductionInventoryRequirements(long productionOrderId);

        Task<ProductionHandoverModel> CreateProductionHandover(long productionOrderId, ProductionHandoverInputModel data);
        Task<ProductionHandoverModel> CreateStatictic(long productionOrderId, ProductionHandoverInputModel data);
        Task<IList<ProductionHandoverModel>> CreateMultipleStatictic(long productionOrderId, IList<ProductionHandoverInputModel> data);
        Task<ProductionHandoverModel> ConfirmProductionHandover(long productionOrderId, long productionHandoverId, EnumHandoverStatus status);
        Task<bool> DeleteProductionHandover(long productionHandoverId);
        Task<IList<DepartmentHandoverDetailModel>> GetDepartmentHandoverDetail(long productionOrderId, long? productionStepId = null, int? departmentId = null, IList<ProductionInventoryRequirementEntity> inventories = null);

        Task<bool> ChangeAssignedProgressStatus(string productionOrderCode, int departmentId,IList<ProductionInventoryRequirementEntity> inventories = null);

    }
}
