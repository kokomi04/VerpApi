﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;
using System.IO;
using VErp.Commons.Library.Model;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductService
    {
        Task<PageData<ProductListOutput>> GetList(string keyword, int[] productTypeIds, int[] productCateIds, int page, int size, Clause filters = null);
        Task<IList<ProductListOutput>> GetListByIds(IList<int> productIds);
        Task<IList<ProductModel>> GetListProductsByIds(IList<int> productIds);
        Task<IList<ProductModel>> GetListByCodeAndInternalNames(ProductQueryByProductCodeOrInternalNameRequest req);

        Task<int> AddProduct(ProductModel req);
        Task<int> AddProductDefault(ProductDefaultModel req);
        Task<int> AddProductToDb(ProductModel req);

        Task<ProductModel> ProductInfo(int productId);
        Task<bool> UpdateProduct(int productId, ProductModel req);
        Task<bool> DeleteProduct(int productId);

        Task<bool> ValidateProductUnitConversions(Dictionary<int, int> productUnitConvertsionProduct);
        List<EntityField> GetFields(Type type);
        Task<int> ImportProductFromMapping(ImportExcelMapping mapping, Stream stream);
    }
}
