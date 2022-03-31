﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;
using static VErp.Services.PurchaseOrder.Service.Voucher.Implement.VoucherDataService;

namespace VErp.Services.PurchaseOrder.Service.Voucher
{
    public interface IVoucherDataService
    {
        Task<PageDataTable> GetVoucherBills(int inputTypeId, bool isMultiRow, long? fromDate, long? toDate, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size);

        //Task<PageDataTable> GetBillInfoByMappingObject(string mappingFunctionKey, string objectId);

        Task<PageDataTable> GetVoucherBillInfoRows(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size);

        Task<BillInfoModel> GetVoucherBillInfo(int inputTypeId, long fId);

        Task<long> CreateVoucherBill(int inputTypeId, BillInfoModel data);

        Task<bool> UpdateVoucherBill(int inputTypeId, long inputValueBillId, BillInfoModel data);

        Task<bool> DeleteVoucherBill(int inputTypeId, long inputValueBillId);

        Task<CategoryNameModel> GetFieldDataForMapping(int voucherTypeId, int? areaId = null);

        Task<List<ValidateVoucherField>> GetVoucherFields(int voucherTypeId, int? areaId = null);

        Task<bool> ImportVoucherBillFromMapping(int inputTypeId, ImportExcelMapping mapping, Stream stream);

        Task<(MemoryStream Stream, string FileName)> ExportVoucherBill(int inputTypeId, long fId);

        Task<bool> UpdateMultipleVoucherBills(int inputTypeId, string fieldName, object oldValue, object newValue, long[] fIds);

        Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames, NonCamelCaseDictionary categoryRow);

        Task<BillInfoModel> GetPackingListInfo(int inputTypeId, long fId);

        Task<PageDataTable> OrderDetailByPurchasingRequest(string keyword, long? fromDate, long? toDate, bool? isCreatedPurchasingRequest, int page, int size);

        Task<IList<VoucherOrderDetailSimpleModel>> OrderByCodes(IList<string> orderCodes);

        Task<IList<NonCamelCaseDictionary>> OrderRowsByCodes(IList<string> orderCodes);

        Task<IList<NonCamelCaseDictionary>> OrderDetails(IList<long> fIds);

        Task<IList<BillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId);
        Task<IList<BillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId);
        Task<bool> CheckAllBillInList(IList<BillSimpleInfoModel> models);
        Task<bool> ApproveAllBillInList(IList<BillSimpleInfoModel> models);
    }
}
