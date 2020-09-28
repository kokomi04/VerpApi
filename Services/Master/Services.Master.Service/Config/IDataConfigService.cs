using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config
{
    public interface IDataConfigService
    {
        Task<DataConfigModel> GetConfig();

        Task<bool> UpdateConfig(DataConfigModel req);
    }
}
