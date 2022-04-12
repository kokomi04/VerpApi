using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionHandover;
using VErp.Services.Manafacturing.Service.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionProcess
{
    public interface IProductionProgressService
    {
        Task<bool> CalcAbdUpdateProductionOrderStatus(ProductionOrderStatusDataModel data);
    }    
}
