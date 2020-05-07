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

        Task<PageData<InputValueBillListOutput>> GetInputValueBills(int inputTypeId, string keyword, IList<InputValueFilterModel> fieldFilters, int orderByFieldId, bool asc, int page, int size);

        Task<ServiceResult<InputValueBillOutputModel>> GetInputValueBill(int inputTypeId, long inputValueBillId);

        Task<ServiceResult<long>> AddInputValueBill(int updatedUserId, int inputTypeId, InputValueBillInputModel data);

        Task<Enum> UpdateInputValueBill(int updatedUserId, int inputTypeId, long inputValueBillId, InputValueBillInputModel data);

        Task<Enum> DeleteInputValueBill(int updatedUserId, int inputTypeId, long inputValueBillId);
       
    }
}
