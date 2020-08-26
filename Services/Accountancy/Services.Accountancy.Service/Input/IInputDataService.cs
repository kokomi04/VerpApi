using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface IInputDataService
    {
        Task<PageDataTable> GetBills(int inputTypeId, string keyword, Dictionary<int, object> filters, Clause columnsFilters, string orderByFieldName, bool asc, int page, int size);

        Task<PageDataTable> GetBillInfoByMappingObject(string mappingFunctionKey, string objectId);

        Task<PageDataTable> GetBillInfoRows(int inputTypeId, long fId, string orderByFieldName, bool asc, int page, int size);

        Task<BillInfoModel> GetBillInfo(int inputTypeId, long fId);

        Task<long> CreateBill(int inputTypeId, BillInfoModel data);

        Task<bool> UpdateBill(int inputTypeId, long inputValueBillId, BillInfoModel data);

        Task<bool> DeleteBill(int inputTypeId, long inputValueBillId);

        Task<bool> ImportBillFromMapping(int inputTypeId, ImportBillExelMapping mapping, Stream stream);

        Task<(MemoryStream Stream, string FileName)> ExportBill(int inputTypeId, long fId);

        Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRate(long toDate, int currency, int exchangeRate);

        Task<bool> CheckExistedFixExchangeRate(long fromDate, long toDate);

        Task<bool> DeletedFixExchangeRate(long fromDate, long toDate);

        Task<ICollection<NonCamelCaseDictionary>> CalcCostTransfer(long toDate, EnumCostTransfer type, bool byDepartment, bool byCustomer, bool byFixedAsset, bool byExpenseItem, bool byFactory, bool byProduct, bool byStock);

        ICollection<CostTransferTypeModel> GetCostTransferTypes();

        Task<bool> CheckExistedCostTransfer(EnumCostTransfer type, long fromDate, long toDate);

        Task<bool> DeletedCostTransfer(EnumCostTransfer type, long fromDate, long toDate);

        Task<ICollection<NonCamelCaseDictionary>> CalcCostTransferBalanceZero(long toDate);

        Task<bool> CheckExistedCostTransferBalanceZero(long fromDate, long toDate);

        Task<bool> DeletedCostTransferBalanceZero(long fromDate, long toDate);
    }
}
