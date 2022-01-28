using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Facade;
using Verp.Resources.Stock.Dictionary;

namespace VErp.Services.Stock.Service.Dictionary.Implement
{
    public class ProductCateService : IProductCateService
    {
        private readonly StockDBContext _stockContext;
        private readonly ObjectActivityLogFacade _productCateActivityLog;

        public ProductCateService(
            StockDBContext stockContext
            , IActivityLogService activityLogService
            )
        {
            _stockContext = stockContext;
           
            _productCateActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductCate);
        }

        public async Task<int> AddProductCate(ProductCateInput req)
        {
            var validate = ValidateProductCate(req);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            if (req.ParentProductCateId.HasValue)
            {
                var parent = await _stockContext.ProductCate.FirstOrDefaultAsync(c => c.ProductCateId == req.ParentProductCateId);
                if (parent == null)
                {
                    throw new BadRequestException(ProductCateErrorCode.ParentProductCateNotfound);
                }
            }

            var sameName = await _stockContext.ProductCate.FirstOrDefaultAsync(c => c.ProductCateName == req.ProductCateName);
            if (sameName != null)
            {
                throw new BadRequestException(ProductCateErrorCode.ProductCateNameAlreadyExisted);
            }

            if (req.IsDefault && _stockContext.ProductCate.Any(pc => pc.IsDefault))
                throw new BadRequestException(GeneralCode.InvalidParams, ProductCateValidationMessage.OnlyOneProductCateDefaultWasSetSametime);

            var productCate = new ProductCate()
            {
                ProductCateName = req.ProductCateName,
                ParentProductCateId = req.ParentProductCateId,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false,
                SortOrder = req.SortOrder,
                IsDefault = req.IsDefault
            };

            await _stockContext.ProductCate.AddAsync(productCate);

            await _stockContext.SaveChangesAsync();

            await UpdateSortOrder(productCate);

            

            await _productCateActivityLog.LogBuilder(() => ProductCateActivityMessage.Create)
              .MessageResourceFormatDatas(productCate.ProductCateName)
              .ObjectId(productCate.ProductCateId)
              .JsonData(req.JsonSerialize())
              .CreateLog();


            return productCate.ProductCateId;
        }

        public async Task<bool> DeleteProductCate(int productCateId)
        {
            var productCate = await _stockContext.ProductCate.FirstOrDefaultAsync(c => c.ProductCateId == productCateId);
            if (productCate == null)
            {
                throw new BadRequestException(ProductCateErrorCode.ProductCateNotfound);
            }

            var childrenCount = await _stockContext.ProductCate.CountAsync(c => c.ParentProductCateId == productCateId);
            if (childrenCount > 0)
            {
                throw new BadRequestException(ProductCateErrorCode.CanNotDeletedParentProductCate);
            }

            if (await _stockContext.Product.AnyAsync(p => p.ProductCateId == productCateId))
            {
                throw new BadRequestException(ProductCateErrorCode.ProductCateInUsed);
            }

            if (productCate.IsDefault)
                throw new BadRequestException(GeneralCode.InvalidParams, "Không được phép xóa danh mục mặt hàng mặc định");

            productCate.IsDeleted = true;
            productCate.UpdatedDatetimeUtc = DateTime.UtcNow;

            await _stockContext.SaveChangesAsync();

            await UpdateSortOrder(productCate);
           
            await _productCateActivityLog.LogBuilder(() => ProductCateActivityMessage.Delete)
             .MessageResourceFormatDatas(productCate.ProductCateName)
             .ObjectId(productCate.ProductCateId)
             .JsonData(productCate.JsonSerialize())
             .CreateLog();

            return true;
        }

        public async Task<ProductCateOutput> GetInfoProductCate(int productCateId)
        {
            var productCate = await _stockContext.ProductCate.Where(c => c.ProductCateId == productCateId)
                .Select(c => new ProductCateOutput
                {
                    ProductCateId = c.ProductCateId,
                    ParentProductCateId = c.ParentProductCateId,
                    ProductCateName = c.ProductCateName,
                    SortOrder = c.SortOrder,
                    IsDefault = c.IsDefault
                })
                .FirstOrDefaultAsync();

            if (productCate == null)
            {
                throw new BadRequestException(ProductCateErrorCode.ProductCateNotfound);
            }

            return productCate;
        }

        public async Task<PageData<ProductCateOutput>> GetList(string keyword, int page, int size, string orderBy, bool asc, Clause filters = null)
        {
            var query = _stockContext.ProductCate.Include(c => c.ParentProductCate).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c => c.ProductCateName.Contains(keyword));
            }
            query = query.InternalFilter(filters);
            var total = await query.CountAsync();
            if (string.IsNullOrEmpty(orderBy))
            {
                query = query.OrderByDescending(c => c.IsDefault).ThenBy(c => c.SortOrder);
            }
            else
            {
                query = query.InternalOrderBy(orderBy, asc);
            }

            var lst = query.Select(c => new ProductCateOutput()
            {
                ParentProductCateId = c.ParentProductCateId,
                ProductCateId = c.ProductCateId,
                ProductCateName = c.ProductCateName,
                SortOrder = c.SortOrder,
                IsDefault = c.IsDefault
            });


            if (size > 0)
            {
                lst = lst.Skip((page - 1) * size).Take(size);
            }

            return (await lst.ToListAsync(), total);
        }

        public async Task<bool> UpdateProductCate(int productCateId, ProductCateInput req)
        {
            var productCate = await _stockContext.ProductCate.FirstOrDefaultAsync(c => c.ProductCateId == productCateId);
            if (productCate == null)
            {
                throw new BadRequestException(ProductCateErrorCode.ProductCateNotfound);
            }

            var sameName = await _stockContext.ProductCate.FirstOrDefaultAsync(c => c.ProductCateId != productCateId && c.ProductCateName == req.ProductCateName);
            if (sameName != null)
            {
                throw new BadRequestException(ProductCateErrorCode.ProductCateNameAlreadyExisted);
            }

            if (req.IsDefault && _stockContext.ProductCate.Any(pc => pc.ProductCateId != productCateId && pc.IsDefault))
                throw new BadRequestException(GeneralCode.InvalidParams, "Chỉ được phép chọn tối đa môt loại danh mục mặt hàng là mặc định");

            productCate.ProductCateName = req.ProductCateName;
            productCate.ParentProductCateId = req.ParentProductCateId;
            productCate.UpdatedDatetimeUtc = DateTime.UtcNow;
            productCate.SortOrder = req.SortOrder;
            productCate.IsDefault = req.IsDefault;

            await _stockContext.SaveChangesAsync();

            await UpdateSortOrder(productCate);

            await _productCateActivityLog.LogBuilder(() => ProductCateActivityMessage.Update)
           .MessageResourceFormatDatas(productCate.ProductCateName)
           .ObjectId(productCate.ProductCateId)
           .JsonData(req.JsonSerialize())
           .CreateLog();

            return true;
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

        private async Task UpdateSortOrder(ProductCate currentProductCate)
        {
            var lst = await _stockContext.ProductCate.OrderBy(c => c.SortOrder).ToListAsync();

            var outList = new Stack<ProductCate>();

            var st = new Stack<ProductCate>();
            st.Push(null);
            var idx = 1;
            while (st.Count > 0)
            {
                var info = st.Pop();
                if (info != null)
                {
                    info.SortOrder = ++idx;
                    if (currentProductCate.IsDefault && currentProductCate.ProductCateId != info.ProductCateId)
                    {
                        info.IsDefault = false;
                    }
                }

                foreach (var child in lst.Where(c => c.ParentProductCateId == info?.ProductCateId).Reverse())
                {
                    st.Push(child);
                }

            }


            await _stockContext.SaveChangesAsync();
        }
        #endregion
    }
}
