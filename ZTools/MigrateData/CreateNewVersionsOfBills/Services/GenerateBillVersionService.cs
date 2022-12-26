using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Services.Accountancy.Service.Input;

namespace CreateNewVersionsOfBills.Services
{
    public interface IGenerateBillVersionService
    {
        Task Execute();
    }

    public class GenerateBillVersionService : IGenerateBillVersionService
    {
        private readonly IInputDataPrivateService _inputDataService;
        private readonly IInputPrivateConfigService _inputPrivateConfigService;

        public GenerateBillVersionService(IInputDataPrivateService inputDataService, IInputPrivateConfigService inputPrivateConfigService, AccountancyDBPrivateContext accountancyDBContext)
        {
            _inputDataService = inputDataService;
            _inputPrivateConfigService = inputPrivateConfigService;            
        }

        public async Task Execute()
        {
            var billTypes = await _inputPrivateConfigService.GetInputTypes(string.Empty, 1, int.MaxValue);
            foreach (var type in billTypes.List)
            {
                var bills = await _inputDataService.GetBills(type.InputTypeId, false, null, null, string.Empty, new Dictionary<int, object>(), null, string.Empty, true, 1, int.MaxValue);
                foreach (var bill in bills.List)
                {
                    var fId = Convert.ToInt64(bill["F_Id"]);
                    var info = await _inputDataService.GetBillInfo(type.InputTypeId, fId);
                    try
                    {
                        await _inputDataService.UpdateBill(type.InputTypeId, fId, info);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
