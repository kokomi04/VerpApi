﻿using System;
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
        Task<IList<ProductionAssignmentModel>> GetProductionAssignments(long productionOrderId);
        Task<bool> UpdateProductionAssignment(long productionStepId, long productionOrderId, ProductionAssignmentModel[] data, ProductionStepWorkInfoInputModel info, DepartmentTimeTableModel[] timeTable);
        Task<bool> UpdateProductionAssignment(long productionOrderId, GeneralAssignmentModel data);
        Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment(int departmentId, long? productionOrderId, int page, int size, string orderByFieldName, bool asc);

        Task<IDictionary<int, ProductivityModel>> GetProductivityDepartments(long productionStepId);

        Task<IDictionary<long, Dictionary<int, ProductivityModel>>> GetGeneralProductivityDepartments(long productionOrderId);

        Task<CapacityOutputModel> GetCapacityDepartments(long productionOrderId, long productionStepId, long startDate, long endDate);
        Task<CapacityOutputModel> GetGeneralCapacityDepartments(long productionOrderId);

        Task<IList<CapacityDepartmentChartsModel>> GetCapacity(long startDate, long endDate);

        Task<IList<DepartmentTimeTableModel>> GetDepartmentTimeTable(int[] departmentIds, long startDate, long endDate);

        Task<IList<ProductionStepWorkInfoOutputModel>> GetListProductionStepWorkInfo(long productionOrderId);

        Task<bool> FinishProductionAssignment(long productionStepId, long productionOrderId, int departmentId);
    }
}
