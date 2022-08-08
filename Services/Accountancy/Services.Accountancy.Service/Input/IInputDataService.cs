using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using static VErp.Services.Accountancy.Service.Input.Implement.InputDataService;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface IInputDataService
    {
        Task<PageDataTable> GetBills(int inputTypeId, bool isMultirow, long? fromDate, long? toDate, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size);

        Task<PageDataTable> GetBillInfoRows(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size);

        Task<List<NonCamelCaseDictionary>> GetListBillInfoRows(int inputTypeId, IList<long> fIds);

        Task<BillInfoModel> GetBillInfo(int inputTypeId, long fId);

        Task<long> CreateBill(int inputTypeId, BillInfoModel data);

        Task<bool> UpdateBill(int inputTypeId, long inputValueBillId, BillInfoModel data);

        Task<bool> DeleteBill(int inputTypeId, long inputValueBillId);

        Task<List<ValidateField>> GetInputFields(int inputTypeId, int? areaId = null, bool isExport = false);

        Task<CategoryNameModel> GetFieldDataForMapping(int inputTypeId, int? areaId, bool? isExport);

        Task<bool> ImportBillFromMapping(int inputTypeId, ImportExcelMapping mapping, Stream stream);

        Task<(MemoryStream Stream, string FileName)> ExportBill(int inputTypeId, long fId);

        Task<bool> UpdateMultipleBills(int inputTypeId, string fieldName, object oldValue, object newValue, long[] billIds, long[] detailIds);

        Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames, NonCamelCaseDictionary categoryRow);

        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId);
        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId);
        Task<bool> CheckAllBillInList(IList<ObjectBillSimpleInfoModel> models);
        Task<bool> ApproveAllBillInList(IList<ObjectBillSimpleInfoModel> models);
    }
}
