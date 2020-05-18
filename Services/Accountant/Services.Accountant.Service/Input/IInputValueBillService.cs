using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Category;
using VErp.Services.Accountant.Model.Input;

namespace VErp.Services.Accountant.Service.Input
{
    public interface IInputValueBillService
    {
        Task<InputTypeListInfo> GetInputTypeListInfo(int inputTypeId);

        Task<PageData<InputValueBillListOutput>> GetInputValueBills(int inputTypeId, string keyword, IList<InputValueFilterModel> fieldFilters, string orderByFieldName, bool asc, int page, int size);

        Task<ServiceResult<InputValueOuputModel>> GetInputValueBill(int inputTypeId, long inputValueBillId);

        Task<ServiceResult<long>> AddInputValueBill(int inputTypeId, InputValueInputModel data);

        Task<Enum> UpdateInputValueBill(int inputTypeId, long inputValueBillId, InputValueInputModel data);

        Task<Enum> DeleteInputValueBill(int inputTypeId, long inputValueBillId);
       
    }
}
