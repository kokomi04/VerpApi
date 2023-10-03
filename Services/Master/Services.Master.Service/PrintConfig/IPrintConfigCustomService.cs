using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.PrintConfig;

namespace VErp.Services.Master.Service.PrintConfig
{
    public interface IPrintConfigCustomService: IPrintConfigService<PrintConfigCustomModel>
    {
        Task<bool> RollbackPrintConfigCustom(int printConfigCustomId);
    }
}
