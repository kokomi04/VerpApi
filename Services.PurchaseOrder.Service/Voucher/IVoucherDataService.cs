using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using static VErp.Services.PurchaseOrder.Service.Voucher.Implement.VoucherDataService;

namespace VErp.Services.PurchaseOrder.Service.Voucher
{
    public interface IVoucherDataService
    {
        Task<PageDataTable> GetVoucherBills(int voucherTypeId, bool isMultiRow, long? fromDate, long? toDate, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size);

        //Task<PageDataTable> GetBillInfoByMappingObject(string mappingFunctionKey, string objectId);

        Task<PageDataTable> GetVoucherBillInfoRows(int voucherTypeId, long fId, string orderByFieldName, bool asc, int page, int size);
        Task<IDictionary<long, BillInfoModel>> GetListVoucherBillInfoRows(int voucherTypeId, IList<long> fIds);

        Task<BillInfoModel> GetVoucherBillInfo(int voucherTypeId, long fId);

        Task<long> CreateVoucherBill(int voucherTypeId, BillInfoModel data);

        Task<bool> UpdateVoucherBill(int voucherTypeId, long inputValueBillId, BillInfoModel data);

        Task<bool> DeleteVoucherBill(int voucherTypeId, long inputValueBillId);

        Task<CategoryNameModel> GetFieldDataForMapping(int voucherTypeId, int? areaId = null, bool? isExport = null);

        Task<List<ValidateVoucherField>> GetVoucherFields(int voucherTypeId, int? areaId = null, bool isViewOnly = false);

        Task<bool> ImportVoucherBillFromMapping(int voucherTypeId, ImportExcelMapping mapping, Stream stream);

        Task<BillInfoModel> ParseBillFromMapping(int voucherTypeId, BillParseMapping parseMapping, Stream stream);

        Task<(MemoryStream Stream, string FileName)> ExportVoucherBill(int voucherTypeId, long fId);

        Task<bool> UpdateMultipleVoucherBills(int voucherTypeId, string fieldName, object oldValue, object newValue, long[] billIds, long[] detailIds);

        Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames, NonCamelCaseDictionary categoryRow);

        Task<BillInfoModel> GetPackingListInfo(int packingListVoucherTypeId, long voucherBill_BHXKId);

        Task<PageDataTable> OrderDetailByPurchasingRequest(string keyword, long? fromDate, long? toDate, bool? isCreatedPurchasingRequest, int page, int size);

        Task<IList<VoucherOrderDetailSimpleModel>> OrderByCodes(IList<string> orderCodes);

        Task<IList<NonCamelCaseDictionary>> OrderRowsByCodes(IList<string> orderCodes);

        Task<IList<NonCamelCaseDictionary>> OrderDetails(IList<long> fIds);

        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int voucherTypeId);
        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int voucherTypeId);
        Task<bool> CheckAllBillInList(IList<ObjectBillSimpleInfoModel> models);
        Task<bool> ApproveAllBillInList(IList<ObjectBillSimpleInfoModel> models);
    }
}
