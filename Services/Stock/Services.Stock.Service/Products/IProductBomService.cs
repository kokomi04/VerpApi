using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductBomService
    {
        Task<IDictionary<int,IList<ProductBomOutput>>> GetBoms(IList<int> productIds);
        Task<IList<ProductBomOutput>> GetBom(int productId);
        Task<IList<ProductElementModel>> GetProductElements(IList<int> productIds);
        Task<(Stream stream, string fileName, string contentType)> ExportBom(IList<int> productIds);

        Task<bool> UpdateProductBomDb(int productId, IList<ProductBomInput> productBoms, IList<ProductMaterialModel> productMaterials, IList<ProductPropertyModel> productProperties, bool isCleanOldMaterial);

        Task<bool> Update(int productId, IList<ProductBomInput> productBoms, IList<ProductMaterialModel> productMaterials, IList<ProductPropertyModel> productProperties, bool isCleanOldMaterial);

        CategoryNameModel GetCustomerFieldDataForMapping();
        Task<bool> ImportBomFromMapping(ImportExcelMapping importExcelMapping, Stream stream);
    }
}
