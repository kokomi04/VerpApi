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

        Task<PageData<DepartmentHandoverModel>> GetDepartmentHandovers(long departmentId, string keyword, int page, int size, Clause filters = null);

        Task<IList<ProductionHandoverModel>> GetProductionHandovers(long productionOrderId);
        Task<IList<ProductionInventoryRequirementModel>> GetProductionInventoryRequirements(long productionOrderId);

        Task<ProductionHandoverModel> CreateProductionHandover(long productionOrderId, ProductionHandoverInputModel data);

        Task<ProductionHandoverModel> ConfirmProductionHandover(long productionOrderId, long productionHandoverId, EnumHandoverStatus status);
    }
}
