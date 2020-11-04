using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products    
{
    public interface IProductBomService
    {
        Task<IList<ProductBomOutput>> GetBOM(int productId);

        Task<bool> Update(int productId, IList<ProductBomInput> req);
    }
}
