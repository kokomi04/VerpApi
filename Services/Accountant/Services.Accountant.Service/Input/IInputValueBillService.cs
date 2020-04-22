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
        Task<PageData<InputValueBillOutputModel>> GetInputValueBills(int inputTypeId, string keyword, int page, int size);

        Task<ServiceResult<InputValueBillOutputModel>> GetInputValueBill(int inputTypeId, long inputValueBillId);

        Task<ServiceResult<long>> AddInputValueBill(int updatedUserId, int inputTypeId, InputValueBillInputModel data);

        //Task<Enum> UpdateInputValueBill(int updatedUserId, int inputTypeId, int inputValueBillId, InputValueBillInputModel data);

        //Task<Enum> DeleteInputValueBill(int updatedUserId, int inputTypeId, int inputValueBillId);
       
    }
}
