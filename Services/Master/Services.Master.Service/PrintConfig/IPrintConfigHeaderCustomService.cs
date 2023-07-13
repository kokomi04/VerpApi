using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Master.Model.PrintConfig;

namespace VErp.Services.Master.Service.PrintConfig
{
    public interface IPrintConfigHeaderCustomService : IPrintConfigHeaderService<PrintConfigHeaderCustomModel, PrintConfigHeaderCustomViewModel>
    {
        Task<bool> RollbackPrintConfigHeaderCustom(int printConfigHeaderCustomId);
    }
}
