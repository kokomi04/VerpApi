using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Stock.Model.Product;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using Microsoft.Data.SqlClient;
using VErp.Infrastructure.EF.EFExtensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductAttachmentService : IProductAttachmentService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        public ProductAttachmentService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductBomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<IList<ProductAttachmentModel>> GetAttachments(int productId)
        {
            if (!_stockDbContext.Product.Any(p => p.ProductId == productId))
                throw new BadRequestException(ProductErrorCode.ProductNotFound);

            return await _stockDbContext.ProductAttachment
                .Where(a => a.ProductId == productId)
                .ProjectTo<ProductAttachmentModel>(_mapper.ConfigurationProvider)
                .ToListAsync(); ;
        }

        public async Task<bool> Update(int productId, IList<ProductAttachmentModel> req)
        {
            var product = _stockDbContext.Product.FirstOrDefault(p => p.ProductId == productId);
            if (product == null) throw new BadRequestException(ProductErrorCode.ProductNotFound);

            var oldAttachments = _stockDbContext.ProductAttachment.Where(a => a.ProductId == productId).ToList();

            var newAttachments = new List<ProductAttachmentModel>(req);
            var changeAttachments = new List<(ProductAttachment OldValue, ProductAttachmentModel NewValue)>();

            foreach (var newItem in req)
            {
                var oldAttachment = oldAttachments.FirstOrDefault(a => a.ProductId == newItem.ProductId && a.AttachmentFileId == newItem.AttachmentFileId);
                if (oldAttachment != null)
                {
                    if (oldAttachment.Title != newItem.Title)
                    {
                        changeAttachments.Add((oldAttachment, newItem));
                    }
                    newAttachments.Remove(newItem);
                    oldAttachments.Remove(oldAttachment);
                }
            }

            // delete old attachment
            if (oldAttachments.Count > 0)
            {
                foreach (var deleteBom in oldAttachments)
                {
                    deleteBom.IsDeleted = true;
                }
            }

            // create new attachment
            if (newAttachments.Count > 0)
            {
                foreach (var newAttachment in newAttachments)
                {
                    var entity = _mapper.Map<ProductAttachment>(newAttachment);
                    _stockDbContext.ProductAttachment.Add(entity);
                }
            }

            // update attachment
            foreach (var updateAttachment in changeAttachments)
            {
                updateAttachment.OldValue.Title = updateAttachment.NewValue.Title;
            }

            await _stockDbContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.ProductBom, productId, $"Cập nhật chi tiết bom cho mặt hàng {product.ProductCode}, tên hàng {product.ProductName}", req.JsonSerialize());
            return true;
        }
    }
}
