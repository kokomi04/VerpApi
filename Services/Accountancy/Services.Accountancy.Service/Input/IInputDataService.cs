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
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;
using static VErp.Services.Accountancy.Service.Input.Implement.InputDataService;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface IInputDataService
    {
        Task<PageDataTable> GetBills(int inputTypeId, bool isMultirow, long? fromDate, long? toDate, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size);

        Task<PageDataTable> GetBillInfoRows(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size);

        Task<BillInfoModel> GetBillInfo(int inputTypeId, long fId);

        Task<long> CreateBill(int inputTypeId, BillInfoModel data);

        Task<bool> UpdateBill(int inputTypeId, long inputValueBillId, BillInfoModel data);

        Task<bool> DeleteBill(int inputTypeId, long inputValueBillId);

        Task<List<ValidateField>> GetInputFields(int inputTypeId, int? areaId = null);

        Task<CategoryNameModel> GetFieldDataForMapping(int inputTypeId, int? areaId);

        Task<bool> ImportBillFromMapping(int inputTypeId, ImportExcelMapping mapping, Stream stream);

        Task<(MemoryStream Stream, string FileName)> ExportBill(int inputTypeId, long fId);

        Task<bool> UpdateMultipleBills(int inputTypeId, string fieldName, object oldValue, object newValue, long[] fIds);

        Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames, NonCamelCaseDictionary categoryRow);

        Task<IList<BillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId);
        Task<IList<BillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId);
        Task<bool> CheckAllBillInList(IList<BillSimpleInfoModel> models);
        Task<bool> ApproveAllBillInList(IList<BillSimpleInfoModel> models);
    }
}
