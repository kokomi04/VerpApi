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
        Task<PageDataTable> GetSaleBills(int inputTypeId, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size);

        //Task<PageDataTable> GetBillInfoByMappingObject(string mappingFunctionKey, string objectId);

        Task<PageDataTable> GetSaleBillInfoRows(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size);

        Task<SaleBillInfoModel> GetSaleBillInfo(int inputTypeId, long fId);

        Task<long> CreateSaleBill(int inputTypeId, SaleBillInfoModel data);

        Task<bool> UpdateSaleBill(int inputTypeId, long inputValueBillId, SaleBillInfoModel data);

        Task<bool> DeleteSaleBill(int inputTypeId, long inputValueBillId);

        Task<bool> ImportSaleBillFromMapping(int inputTypeId, ImportBillExelMapping mapping, Stream stream);

        Task<(MemoryStream Stream, string FileName)> ExportSaleBill(int inputTypeId, long fId);

        Task<bool> UpdateMultipleSaleBills(int inputTypeId, string fieldName, object oldValue, object newValue, long[] fIds);

        Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames, NonCamelCaseDictionary categoryRow);
    }
}
