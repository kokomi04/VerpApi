using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;

namespace VErp.Services.Stock.Service.Dictionary.Implement
{
    public class ProductTypeService : IProductTypeService
    {
        private readonly StockDBContext _stockContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;

        public ProductTypeService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductTypeService> logger
            )
        {
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<ServiceResult<int>> AddProductType(ProductTypeInput req)
        {
            var validate = ValidateProductType(req);
            if (!validate.IsSuccess())
            {
                return validate;
            }

            if (req.ParentProductTypeId.HasValue)
            {
                var parent = await _stockContext.ProductType.FirstOrDefaultAsync(c => c.ProductTypeId == req.ParentProductTypeId);
                if (parent == null)
                {
                    return ProductTypeErrorCode.ParentProductTypeNotfound;
                }
            }

            var productType = new ProductType()
            {
                ProductTypeName = req.ProductTypeName,
                ParentProductTypeId = req.ParentProductTypeId,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false,
            };

            await _stockContext.ProductType.AddAsync(productType);

            await _stockContext.SaveChangesAsync();

            return productType.ProductTypeId;
        }

        public async Task<Enum> DeleteProductType(int productTypeId)
        {
            var productType = await _stockContext.ProductType.FirstOrDefaultAsync(c => c.ProductTypeId == productTypeId);
            if (productType == null)
            {
                return ProductTypeErrorCode.ProductTypeNotfound;
            }
            productType.IsDeleted = true;
            productType.UpdatedDatetimeUtc = DateTime.UtcNow;

            await _stockContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        public async Task<ServiceResult<ProductTypeOutput>> GetInfoProductType(int productTypeId)
        {
            var productType = await _stockContext.ProductType.Where(c => c.ProductTypeId == productTypeId)
                .Select(c => new ProductTypeOutput
                {
                    ProductTypeId = c.ProductTypeId,
                    ParentProductTypeId = c.ParentProductTypeId,
                    ProductTypeName = c.ProductTypeName
                })
                .FirstOrDefaultAsync();

            if (productType == null)
            {
                return ProductTypeErrorCode.ProductTypeNotfound;
            }

            return productType;
        }

        public async Task<PageData<ProductTypeOutput>> GetList(string keyword, int page, int size)
        {
            var query = (from c in _stockContext.ProductType select c);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from c in query
                        where c.ProductTypeName.Contains(keyword)
                        select c;
            }

            var total = await query.CountAsync();

            var lst = await query.Select(c => new ProductTypeOutput()
                {
                    ParentProductTypeId = c.ParentProductTypeId,
                    ProductTypeId = c.ProductTypeId,
                    ProductTypeName = c.ProductTypeName
                }
                )
                .Skip((page - 1) * size).Take(size).ToListAsync();

            return (lst, total);
        }

        public async Task<Enum> UpdateProductType(int productTypeId, ProductTypeInput req)
        {
            var productType = await _stockContext.ProductType.FirstOrDefaultAsync(c => c.ProductTypeId == productTypeId);
            if (productType == null)
            {
                return ProductTypeErrorCode.ProductTypeNotfound;
            }
            productType.ProductTypeName = req.ProductTypeName;
            productType.ParentProductTypeId = req.ParentProductTypeId;
            productType.UpdatedDatetimeUtc = DateTime.UtcNow;


            await _stockContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        #region private
        private Enum ValidateProductType(ProductTypeInput req)
        {
            if (string.IsNullOrWhiteSpace(req.ProductTypeName))
            {
                return ProductTypeErrorCode.EmptyProductTypeName;
            }
            return GeneralCode.Success;
        }
        #endregion
    }
}
