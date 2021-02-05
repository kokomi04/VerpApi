using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionOrder
{
    public interface IProductionOrderMaterialsService
    {
        Task<IList<ProductionOrderMaterialsModel>> GetProductionOrderMaterials(long productionOrderId);
    }
}
