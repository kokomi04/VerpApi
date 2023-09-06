using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.ServiceCore.Model;
using static VErp.Services.Accountancy.Service.Input.Implement.InputDataServiceBase;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface IInputDataPrivateService : IInputDataServiceBase
    {

    }


    public interface IInputDataPublicService : IInputDataServiceBase
    {

    }

    public interface IInputDataServiceBase
    {
        Task<PageDataTable> GetBills(int inputTypeId, bool isMultirow, long? fromDate, long? toDate, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size);

        Task<PageDataTable> GetBillInfoRows(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size);

        Task<IDictionary<long, BillInfoModel>> GetBillInfos(int inputTypeId, IList<long> fIds);

        Task<IList<NonCamelCaseDictionary>> CalcResultAllowcation(int parentInputTypeId, long parentFId);
        Task<InputType> GetTypeInfo(int inputTypeId);

        Task<BillInfoModel> GetBillInfo(int inputTypeId, long fId);

        Task<BillInfoModel> GetBillInfoByParent(int inputTypeId, long parentFId);

        Task<IList<string>> GetAllocationDataBillCodes(int inputTypeId, long fId);

        Task<bool> UpdateAllocationDataBillCodes(int inputTypeId, long fId, IList<string> dataAllowcationBillCodes);

        Task<long> CreateBill(int inputTypeId, BillInfoModel data, bool isDeleteAllowcationBill);

        Task<bool> UpdateBill(int inputTypeId, long inputValueBillId, BillInfoModel data, bool isDeleteAllowcationBill);

        Task<bool> DeleteBill(int inputTypeId, long inputValueBillId, bool isDeleteAllowcationBill);

        Task<List<ValidateField>> GetInputFields(int inputTypeId, int? areaId = null, bool isExport = false);

        Task<CategoryNameModel> GetFieldDataForMapping(int inputTypeId, int? areaId, bool? isExport);

        Task<bool> ImportBillFromMapping(int inputTypeId, ImportExcelMapping mapping, Stream stream, bool isDeleteAllowcationBill);

        Task<BillInfoModel> ParseBillFromMapping(int inputTypeId, BillParseMapping parseMapping, Stream stream);

        Task<(MemoryStream Stream, string FileName)> ExportBill(int inputTypeId, long fId);

        Task<bool> UpdateMultipleBills(int inputTypeId, string fieldName, object oldValue, object newValue, long[] billIds, long[] detailIds, bool isDeleteAllowcationBill);

        Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames, NonCamelCaseDictionary categoryRow);

        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId);
        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId);
        Task<bool> CheckAllBillInList(IList<ObjectBillSimpleInfoModel> models);
        Task<bool> ApproveAllBillInList(IList<ObjectBillSimpleInfoModel> models);

        Task ValidateAccountantConfig(DateTime? billDate, DateTime? oldDate);
    }
}
