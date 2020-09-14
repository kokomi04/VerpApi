using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface IPrintConfigService
    {
        Task<PrintConfigModel> GetPrintConfig(int printConfigId);
        Task<ICollection<PrintConfigModel>> GetPrintConfigs(int inputTypeId);
        Task<int> AddPrintConfig(PrintConfigModel data);
        Task<bool> UpdatePrintConfig(int printConfigId, PrintConfigModel data);
        Task<bool> DeletePrintConfig(int printConfigId);
    }
}
