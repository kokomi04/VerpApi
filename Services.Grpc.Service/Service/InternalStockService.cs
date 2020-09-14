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
    public class InternalStockService: VErp.Grpc.Protos.Stock.StockBase
    {
        private readonly StockDBContext _stockContext;
        private readonly ILogger<InternalStockService> _logger;

        public InternalStockService(StockDBContext stockContext, ILogger<InternalStockService> logger)
        {
            _stockContext = stockContext;
            _logger = logger;
        }

        public override async Task<StockInfoResponses> StockInfo(StockInfoRequest request, ServerCallContext context)
        {
            var stockInfo = await _stockContext.Stock.IgnoreQueryFilters().Where(q => !q.IsDeleted).FirstOrDefaultAsync(p => p.StockId == request.StockId);
            if (stockInfo == null)
            {
                throw new BadRequestException(StockErrorCode.StockNotFound);
            }
            return await Task.FromResult( new StockInfoResponses
            {
                StockOutPut = new StockOutput{
                    StockId = stockInfo.StockId,
                    StockName = stockInfo.StockName,
                    Description = stockInfo.Description,
                    StockKeeperId = (int)stockInfo.StockKeeperId,
                    StockKeeperName = stockInfo.StockKeeperName,
                    Type = (int)stockInfo.Type,
                    Status = (int)stockInfo.Status
                }
            });
        }
    }
}
