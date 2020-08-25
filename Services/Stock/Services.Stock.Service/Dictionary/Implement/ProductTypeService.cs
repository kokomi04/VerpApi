using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Dictionary;

namespace VErp.Services.Stock.Service.Dictionary.Implement
{
    public class ProductTypeService : IProductTypeService
    {
        private readonly StockDBContext _stockContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public ProductTypeService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductTypeService> logger
            , IActivityLogService activityLogService
            )
        {
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<int> AddProductType(ProductTypeInput req)
        {
            var validate = ValidateProductType(req);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            if (req.ParentProductTypeId.HasValue)
            {
                var parent = await _stockContext.ProductType.FirstOrDefaultAsync(c => c.ProductTypeId == req.ParentProductTypeId);
                if (parent == null)
                {
                    throw new BadRequestException(ProductTypeErrorCode.ParentProductTypeNotfound);
                }
            }

            var sameName = await _stockContext.ProductType.FirstOrDefaultAsync(c => c.ProductTypeName == req.ProductTypeName);
            if (sameName != null)
            {
                throw new BadRequestException(ProductTypeErrorCode.ProductTypeNameAlreadyExisted);
            }

            var productType = new ProductType()
            {
                ProductTypeName = req.ProductTypeName,
                IdentityCode = req.IdentityCode ?? "",
                ParentProductTypeId = req.ParentProductTypeId,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false,
                SortOrder = req.SortOrder
            };

            await _stockContext.ProductType.AddAsync(productType);

            await _stockContext.SaveChangesAsync();

            await UpdateSortOrder();

            await _activityLogService.CreateLog(EnumObjectType.ProductType, productType.ProductTypeId, $"Thêm mới loại sản phẩm {productType.ProductTypeName}", req.JsonSerialize());

            return productType.ProductTypeId;
        }

        public async Task<bool> DeleteProductType(int productTypeId)
        {
            var productType = await _stockContext.ProductType.FirstOrDefaultAsync(c => c.ProductTypeId == productTypeId);
            if (productType == null)
            {
                throw new BadRequestException(ProductTypeErrorCode.ProductTypeNotfound);
            }
            var childrenCount = await _stockContext.ProductType.CountAsync(c => c.ParentProductTypeId == productTypeId);
            if (childrenCount > 0)
            {
                throw new BadRequestException(ProductTypeErrorCode.CanNotDeletedParentProductType);
            }

            if (await _stockContext.Product.AnyAsync(p => p.ProductTypeId == productTypeId))
            {
                throw new BadRequestException(ProductTypeErrorCode.ProductTypeInUsed);
            }

            productType.IsDeleted = true;
            productType.UpdatedDatetimeUtc = DateTime.UtcNow;

            await _stockContext.SaveChangesAsync();

            await UpdateSortOrder();

            await _activityLogService.CreateLog(EnumObjectType.ProductType, productType.ProductTypeId, $"Xóa loại sản phẩm {productType.ProductTypeName}", productType.JsonSerialize());

            return true;
        }

        public async Task<ProductTypeOutput> GetInfoProductType(int productTypeId)
        {
            var productType = await _stockContext.ProductType.Where(c => c.ProductTypeId == productTypeId)
                .Select(c => new ProductTypeOutput
                {
                    ProductTypeId = c.ProductTypeId,
                    IdentityCode = c.IdentityCode,
                    ParentProductTypeId = c.ParentProductTypeId,
                    ProductTypeName = c.ProductTypeName,
                    SortOrder = c.SortOrder
                })
                .FirstOrDefaultAsync();

            if (productType == null)
            {
                throw new BadRequestException(ProductTypeErrorCode.ProductTypeNotfound);
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

            var lstQuery = query.Select(c => new ProductTypeOutput()
            {
                ParentProductTypeId = c.ParentProductTypeId,
                ProductTypeId = c.ProductTypeId,
                IdentityCode = c.IdentityCode,
                ProductTypeName = c.ProductTypeName,
                SortOrder = c.SortOrder
            });


            var lst = size > 0
                ? await lstQuery.OrderBy(t => t.SortOrder).Skip((page - 1) * size).Take(size).ToListAsync()
                : await lstQuery.ToListAsync();

            return (lst, total);
        }

        public async Task<bool> UpdateProductType(int productTypeId, ProductTypeInput req)
        {
            var productType = await _stockContext.ProductType.FirstOrDefaultAsync(c => c.ProductTypeId == productTypeId);
            if (productType == null)
            {
                throw new BadRequestException(ProductTypeErrorCode.ProductTypeNotfound);
            }

            var sameName = await _stockContext.ProductType.FirstOrDefaultAsync(c => c.ProductTypeId != productTypeId && c.ProductTypeName == req.ProductTypeName);
            if (sameName != null)
            {
                throw new BadRequestException(ProductTypeErrorCode.ProductTypeNameAlreadyExisted);
            }


            productType.ProductTypeName = req.ProductTypeName;
            productType.IdentityCode = req.IdentityCode;
            productType.ParentProductTypeId = req.ParentProductTypeId;
            productType.UpdatedDatetimeUtc = DateTime.UtcNow;
            productType.SortOrder = req.SortOrder;


            await _stockContext.SaveChangesAsync();

            await UpdateSortOrder();

            await _activityLogService.CreateLog(EnumObjectType.ProductType, productType.ProductTypeId, $"Cập nhật loại sản phẩm {productType.ProductTypeName}", req.JsonSerialize());

            return true;
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

        private async Task UpdateSortOrder()
        {
            var lst = await _stockContext.ProductType.OrderBy(c => c.SortOrder).ToListAsync();
            var st = new Stack<ProductType>();
            st.Push(null);
            var idx = 0;
            while (st.Count > 0)
            {
                var info = st.Pop();
                if (info != null)
                {
                    info.SortOrder = ++idx;
                }

                foreach (var child in lst.Where(c => c.ParentProductTypeId == info?.ProductTypeId).Reverse())
                {
                    st.Push(child);
                }

            }

            await _stockContext.SaveChangesAsync();
        }
        #endregion
    }
}
