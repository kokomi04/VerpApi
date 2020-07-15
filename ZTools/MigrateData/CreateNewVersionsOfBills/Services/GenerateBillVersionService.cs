using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
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
        private readonly IInputDataService _inputDataService;
        private readonly IInputConfigService _inputConfigService;
        private readonly AccountancyDBContext _accountancyDBContext;

        public GenerateBillVersionService(IInputDataService inputDataService, IInputConfigService inputConfigService, AccountancyDBContext accountancyDBContext)
        {
            _inputDataService = inputDataService;
            _inputConfigService = inputConfigService;
            _accountancyDBContext = accountancyDBContext;
        }

        public async Task Execute()
        {
            var billTypes = await _inputConfigService.GetInputTypes(string.Empty, 1, int.MaxValue);
            foreach (var type in billTypes.List)
            {
                var bills = await _inputDataService.GetBills(type.InputTypeId, string.Empty, new Dictionary<int, object>(), string.Empty, true, 1, int.MaxValue);
                foreach (var bill in bills.List)
                {
                    var fId = Convert.ToInt64(bill["F_Id"]);
                    var info = await _inputDataService.GetBillInfo(type.InputTypeId, fId);
                    await _inputDataService.UpdateBill(type.InputTypeId, fId, info);
                }

            }
        }
    }
}
