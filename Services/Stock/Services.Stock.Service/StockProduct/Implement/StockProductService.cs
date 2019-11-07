using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VErp.Services.Master.Service.Dictionay;
using VErp.Commons.Enums.StandardEnum;
using VErp.Services.Master.Service.Activity;
using VErp.Commons.Enums.MasterEnum;
using VErp.Services.Stock.Model.Product;
using VErp.Commons.Library;
using System.Globalization;

namespace VErp.Services.Stock.Service.StockProduct.Implement
{
    public class StockProductService : IStockProductService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityService _activityService;


        public StockProductService(StockDBContext stockDbContext
            , IOptions<AppSetting> appSetting
            , ILogger<StockProductService> logger
            , IActivityService activityService)
        {
            _stockDbContext = stockDbContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityService = activityService;
        }

        public async Task<PageData<Infrastructure.EF.StockDB.StockProduct>> GetList(int stockId = 0, int productId = 0, DateTime? beginTime = null, DateTime? endTime = null, int page = 1, int size = 10)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<Infrastructure.EF.StockDB.StockProduct>> GetStockProduct(int stockId = 0, int productId = 0)
        {
            throw new NotImplementedException();
        }

        


        #region Helper methods
        private object GetStockProductInfoForLog(VErp.Infrastructure.EF.StockDB.StockProduct obj)
        {
            return obj;
        }
        #endregion
    }
}
