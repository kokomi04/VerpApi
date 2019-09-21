using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;

namespace VErp.Services.Stock.Service.Dictionary.Implement
{
    public class ProductCateService : IProductCateService
    {
        private readonly StockDBContext _stockContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;

        public ProductCateService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductCateService> logger
            )
        {
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<ServiceResult<int>> AddProductCate(ProductCateInput req)
        {
            var validate = ValidateProductCate(req);
            if (!validate.IsSuccess())
            {
                return validate;
            }

            if (req.ParentProductCateId.HasValue)
            {
                var parent = await _stockContext.ProductCate.FirstOrDefaultAsync(c => c.ProductCateId == req.ParentProductCateId);
                if (parent == null)
                {
                    return ProductCateErrorCode.ParentProductCateNotfound;
                }
            }

            var productCate = new ProductCate()
            {
                ProductCateName = req.ProductCateName,
                ParentProductCateId = req.ParentProductCateId,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false,
            };

            await _stockContext.ProductCate.AddAsync(productCate);

            await _stockContext.SaveChangesAsync();

            return productCate.ProductCateId;
        }

        public async Task<Enum> DeleteProductCate(int productCateId)
        {
            var productCate = await _stockContext.ProductCate.FirstOrDefaultAsync(c => c.ProductCateId == productCateId);
            if (productCate == null)
            {
                return ProductCateErrorCode.ProductCateNotfound;
            }
            productCate.IsDeleted = true;
            productCate.UpdatedDatetimeUtc = DateTime.UtcNow;

            await _stockContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        public async Task<ServiceResult<ProductCateOutput>> GetInfoProductCate(int productCateId)
        {
            var productCate = await _stockContext.ProductCate.Where(c => c.ProductCateId == productCateId)
                .Select(c => new ProductCateOutput
                {
                    ProductCateId = c.ProductCateId,
                    ParentProductCateId = c.ParentProductCateId,
                    ProductCateName = c.ProductCateName
                })
                .FirstOrDefaultAsync();

            if (productCate == null)
            {
                return ProductCateErrorCode.ProductCateNotfound;
            }

            return productCate;
        }

        public async Task<PageData<ProductCateOutput>> GetList(string keyword, int page, int size)
        {
            var query = (from c in _stockContext.ProductCate select c);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from c in query
                        where c.ProductCateName.Contains(keyword)
                        select c;
            }

            var total = await query.CountAsync();

            var lst = await query.Select(c => new ProductCateOutput()
                {
                    ParentProductCateId = c.ParentProductCateId,
                    ProductCateId = c.ProductCateId,
                    ProductCateName = c.ProductCateName
                }
                )
                .Skip((page - 1) * size).Take(size).ToListAsync();

            return (lst, total);
        }

        public async Task<Enum> UpdateProductCate(int productCateId, ProductCateInput req)
        {
            var productCate = await _stockContext.ProductCate.FirstOrDefaultAsync(c => c.ProductCateId == productCateId);
            if (productCate == null)
            {
                return ProductCateErrorCode.ProductCateNotfound;
            }
            productCate.ProductCateName = req.ProductCateName;
            productCate.ParentProductCateId = req.ParentProductCateId;
            productCate.UpdatedDatetimeUtc = DateTime.UtcNow;


            await _stockContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        #region private
        private Enum ValidateProductCate(ProductCateInput req)
        {
            if (string.IsNullOrWhiteSpace(req.ProductCateName))
            {
                return ProductCateErrorCode.EmptyProductCateName;
            }
            return GeneralCode.Success;
        }
        #endregion
    }
}
