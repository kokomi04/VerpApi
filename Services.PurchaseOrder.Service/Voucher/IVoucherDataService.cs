using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;

namespace VErp.Services.PurchaseOrder.Service.Voucher
{
    public interface IVoucherDataService
    {
        Task<PageDataTable> GetVoucherBills(int inputTypeId, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size);

        //Task<PageDataTable> GetBillInfoByMappingObject(string mappingFunctionKey, string objectId);

        Task<PageDataTable> GetVoucherBillInfoRows(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size);

        Task<VoucherBillInfoModel> GetVoucherBillInfo(int inputTypeId, long fId);

        Task<long> CreateVoucherBill(int inputTypeId, VoucherBillInfoModel data);

        Task<bool> UpdateVoucherBill(int inputTypeId, long inputValueBillId, VoucherBillInfoModel data);

        Task<bool> DeleteVoucherBill(int inputTypeId, long inputValueBillId);

        Task<bool> ImportVoucherBillFromMapping(int inputTypeId, ImportBillExelMapping mapping, Stream stream);

        Task<(MemoryStream Stream, string FileName)> ExportVoucherBill(int inputTypeId, long fId);

        Task<bool> UpdateMultipleVoucherBills(int inputTypeId, string fieldName, object oldValue, object newValue, long[] fIds);

        Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames, NonCamelCaseDictionary categoryRow);

        Task<VoucherBillInfoModel> GetPackingListInfo(int inputTypeId, long fId);
    }
}
