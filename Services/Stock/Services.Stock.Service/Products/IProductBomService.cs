﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Library.Model;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Product.Bom;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductBomService
    {
        Task<IDictionary<int, IList<ProductBomOutput>>> GetBoms(IList<int> productIds);
        Task<IList<ProductBomOutput>> GetBomsV2(IList<int> productIds);

        Task<IList<ProductBomOutput>> GetBom(int productId);
        Task<IList<ProductElementModel>> GetProductElements(IList<int> productIds);
        Task<(Stream stream, string fileName, string contentType)> ExportBom(IList<int> productIds, bool isFindTopBOM = false, bool isExportTopBOM = false);
        Task<bool> UpdateProductBomDb(int productId, ProductBomUpdateInfoModel bomInfo);

        Task<bool> Update(int productId, ProductBomUpdateInfoModel bomInfo);

        Task<CategoryNameModel> GetBomFieldDataForMapping();

        Task<bool> ImportBomFromMapping(ImportExcelMapping importExcelMapping, Stream stream);

        Task<IList<ProductBomByProduct>> PreviewBomFromMapping(ImportExcelMapping mapping, Stream stream);

    }
}
