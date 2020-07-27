using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface IInputDataService
    {
        Task<PageDataTable> GetBills(int inputTypeId, string keyword, Dictionary<int, object> filters, string orderByFieldName, bool asc, int page, int size);

        Task<PageDataTable> GetBillInfoByMappingObject(string mappingFunctionKey, string objectId);

        Task<PageDataTable> GetBillInfoRows(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size);

        Task<BillInfoModel> GetBillInfo(int inputTypeId, long fId);

        Task<long> CreateBill(int inputTypeId, BillInfoModel data);

        Task<bool> UpdateBill(int inputTypeId, long inputValueBillId, BillInfoModel data);

        Task<bool> DeleteBill(int inputTypeId, long inputValueBillId);

        Task<bool> ImportBillFromMapping(int inputTypeId, ImportBillExelMapping mapping, Stream stream);
    }
}
