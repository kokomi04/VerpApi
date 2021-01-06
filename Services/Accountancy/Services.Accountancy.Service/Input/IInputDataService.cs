using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface IInputDataService
    {
        Task<PageDataTable> GetBills(int inputTypeId, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size);

        Task<PageDataTable> GetBillInfoRows(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size);

        Task<BillInfoModel> GetBillInfo(int inputTypeId, long fId);

        Task<long> CreateBill(int inputTypeId, BillInfoModel data);

        Task<bool> UpdateBill(int inputTypeId, long inputValueBillId, BillInfoModel data);

        Task<bool> DeleteBill(int inputTypeId, long inputValueBillId);

        Task<bool> ImportBillFromMapping(int inputTypeId, ImportBillExelMapping mapping, Stream stream);

        Task<(MemoryStream Stream, string FileName)> ExportBill(int inputTypeId, long fId);

        Task<bool> UpdateMultipleBills(int inputTypeId, string fieldName, object oldValue, object newValue, long[] fIds);

        Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames, NonCamelCaseDictionary categoryRow);
    }
}
