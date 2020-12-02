using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductAttachmentService
    {
        Task<IList<ProductAttachmentModel>> GetAttachments(int productId);

        Task<bool> Update(int productId, IList<ProductAttachmentModel> req);
    }
}
