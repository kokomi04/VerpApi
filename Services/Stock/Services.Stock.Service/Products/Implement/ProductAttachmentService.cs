using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Stock.Product;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductAttachmentService : IProductAttachmentService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _productActivityLog;

        public ProductAttachmentService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductBomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper)
        {
            _stockDbContext = stockContext;
            _mapper = mapper;
            _productActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Product);
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
                var oldAttachment = oldAttachments.FirstOrDefault(a => a.ProductAttachmentId == newItem.ProductAttachmentId);
                if (oldAttachment != null)
                {
                    changeAttachments.Add((oldAttachment, newItem));
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
                    entity.ProductId = productId;
                    _stockDbContext.ProductAttachment.Add(entity);
                }
            }

            // update attachment
            foreach (var updateAttachment in changeAttachments)
            {
                _mapper.Map(updateAttachment.NewValue, updateAttachment.OldValue);
                updateAttachment.OldValue.ProductId = productId;
            }

            await _stockDbContext.SaveChangesAsync();

            await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.UpdateAttachment)
                .MessageResourceFormatDatas(product.ProductCode)
                .ObjectId(productId)
                .JsonData(req)
                .CreateLog();

            return true;
        }
    }
}
