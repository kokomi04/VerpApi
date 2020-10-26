using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Grpc.Protos;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IStockHelperService
    {
        Task<SimpleStockInfo> StockInfo(int stockId);
        Task<IList<SimpleStockInfo>> GetAllStock();
    }


    public class StockHelperService : IStockHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly Stock.StockClient _stockClient;

        public StockHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<ProductHelperService> logger, Stock.StockClient stockClient)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
            _stockClient = stockClient;
        }


        public async Task<SimpleStockInfo> StockInfo(int stockId)
        {
            if (_appSetting.GrpcInternal?.Address?.Contains("https") == true)
            {
                var result = await _stockClient.StockInfoAsync(new StockInfoRequest { StockId = stockId });
                if (result?.StockOutPut?.StockId != 0)
                {
                    return new SimpleStockInfo
                    {
                        StockId = result.StockOutPut.StockId,
                        StockName = result.StockOutPut.StockName
                    };
                }
                return null;
            }
            return await _httpCrossService.Get<SimpleStockInfo>($"api/internal/InternalStock/{stockId}");
        }

        public async Task<IList<SimpleStockInfo>> GetAllStock()
        {
            var lst = await _httpCrossService.Post<PageData<SimpleStockInfo>>($"api/internal/InternalStock", new { page = 1, size = int.MaxValue });
            return lst.List;
        }
    }
}
