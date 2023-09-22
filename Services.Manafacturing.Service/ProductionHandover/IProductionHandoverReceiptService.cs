using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Service.StatusProcess;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.ProductionHandover
{
    public interface IProductionHandoverReceiptService : IStatusProcessService
    {
        Task<PageData<DepartmentHandoverModel>> GetDepartmentHandovers(long departmentId, string keyword, int page, int size, long fromDate, long toDate, int? stepId, int? productId, bool? isInFinish, bool? isOutFinish, EnumProductionStepLinkDataRoleType? productionStepLinkDataRoleTypeId);

        Task<PageData<ProductionStepHandoverByDepartmentModel>> GetProductionStepDepartmentHandovers(long departmentId, string keyword, int page, int size, long? fromDate, long? toDate, int? stepId, int? productId, bool? isInFinish, bool? isOutFinish);

        Task<PageData<ProductionHandoverReceiptByDateModel>> GetDepartmentHandoversByDate(IList<long> fromDepartmentIds, IList<long> toDepartmentIds, IList<long> fromStepIds, IList<long> toStepIds, long? fromDate, long? toDate, bool? isInFinish, bool? isOutFinish, int page, int size);

        Task<IList<ProductionHandoverModel>> GetProductionHandovers(long productionOrderId);
        Task<long> Create(long productionOrderId, ProductionHandoverReceiptModel data);
        Task<PageData<ProductionHandoverHistoryReceiptModel>> GetList(string keyword, long? fromDate, long? toDate, int page, int size, string orderByFieldName, bool asc, Clause filters = null);
        Task<ProductionHandoverReceiptModel> Info(long receiptId);
        Task<long> CreateStatictic(long productionOrderId, ProductionHandoverReceiptModel data, EnumHandoverStatus status);
      
        Task<bool> Confirm(long receiptId, EnumHandoverStatus status);
        Task<bool> AcceptBatch(IList<long> receiptIds);

        Task<bool> Delete(long productionHandoverReceiptId);

        Task<bool> Update(long productionHandoverReceiptId, ProductionHandoverReceiptModel data, EnumHandoverStatus status);

        Task<bool> CreateBatch(IList<ProductionHandoverReceiptModel> datas);
        Task<long> Create(ProductionHandoverReceiptModel data);
    }
}