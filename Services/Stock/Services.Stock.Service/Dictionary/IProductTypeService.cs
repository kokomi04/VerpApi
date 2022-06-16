﻿using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;

namespace VErp.Services.Stock.Service.Dictionary
{
    public interface IProductTypeService
    {
        Task<PageData<ProductTypeOutput>> GetList(string keyword, int page, int size, Clause filters = null);
        Task<int> AddProductType(ProductTypeInput req);
        Task<bool> UpdateProductType(int productTypeId, ProductTypeInput req);
        Task<bool> DeleteProductType(int productTypeId);
        Task<ProductTypeOutput> GetInfoProductType(int productTypeId);
    }
}
