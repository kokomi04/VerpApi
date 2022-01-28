using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionProcessMold;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Manafacturing.Service.ProductionProcessMold
{
    public interface IProductionProcessMoldService
    {
        Task<PageData<ProductionProcessMoldOutput>> Search(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null);
        Task<ICollection<ProductionStepMoldModel>> GetProductionProcessMold(long productionProcessMoldId);

        Task<long> AddProductionProcessMold(ProductionProcessMoldInput model);
        Task<bool> UpdateProductionProcessMold(long productionProcessMoldId, ProductionProcessMoldInput model);
        Task<bool> DeleteProductionProcessMold(long productionProcessMoldId);
    }
}
