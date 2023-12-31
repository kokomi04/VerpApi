﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Library.Model;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product.Pu;

namespace VErp.Services.Stock.Service.Products
{
    public interface IProductUnitConversionService
    {
        /// <summary>
        /// Lấy danh sách tỉ lệ chuyển đổi của các đơn vị tính phụ
        /// </summary>
        /// <param name="productId">Id kho</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<ProductUnitConversionOutput>> GetList(int productId, int page = 0, int size = 0);


        Task<PageData<ProductUnitConversionByProductOutput>> GetListByProducts(IList<int> productIds, int page = 0, int size = 0);

        Task<IList<ProductUnitConversionByProductOutput>> GetByInStockProducts(IList<int> productIds, int stockId, long unixDate);

        CategoryNameModel GetFieldDataForImportMapping();
        Task<bool> Import(ImportExcelMapping mapping, Stream stream);
    }
}
