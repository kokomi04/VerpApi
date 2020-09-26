using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Stock.Model.Config;

namespace VErp.Services.Stock.Service.Stock
{
    public interface IInventoryConfigService
    {
        Task<InventoryConfigModel> GetConfig();
        Task<bool> UpdateConfig(InventoryConfigModel req);
    }
}
