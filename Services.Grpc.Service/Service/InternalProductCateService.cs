using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Grpc.Protos;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Grpc.Service
{
    public class InternalProductCateService : VErp.Grpc.Protos.ProductCate.ProductCateBase
    {
        private readonly StockDBContext _stockContext;
        private readonly ILogger<InternalProductCateService> _logger;

        public InternalProductCateService(StockDBContext stockContext, ILogger<InternalProductCateService> logger)
        {
            _stockContext = stockContext;
            _logger = logger;
        }

        public override async Task<ProductCateResponses> GetProductCate(ProductCateRequest request, ServerCallContext context)
        {
            var productCate = await _stockContext.ProductCate.Where(c => c.ProductCateId == request.ProductCateId)
                .Select(c => new ProductCateResponses
                {
                    ProductCateId = c.ProductCateId,
                    ParentProductCateId = (int)c.ParentProductCateId,
                    ProductCateName = c.ProductCateName,
                    SortOrder = c.SortOrder
                })
                .FirstOrDefaultAsync();

            if (productCate == null)
            {
                throw new BadRequestException(ProductCateErrorCode.ProductCateNotfound);
            }

            return await Task.FromResult(productCate);
        }
    }
}
