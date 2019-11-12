using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Inventory.Implement;

namespace VErp.Services.Stock.Service.ProductUnitConversion.Implement
{
    public class ProductUnitConversionService : IProductUnitConversionService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityService _activityService;
        private readonly IUnitService _unitService;

        public ProductUnitConversionService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductUnitConversionService> logger
            , IActivityService activityService
            , IUnitService unitService
        )
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityService = activityService;
            _unitService = unitService;
        }

        public async Task<PageData<Infrastructure.EF.StockDB.ProductUnitConversion>> GetList(int productId, int page = 1, int size = 10)
        {
            try
            {
                var query = from p in _stockDbContext.ProductUnitConversion
                            select p;

                if (productId > 0)
                {
                    query = query.Where(q => q.ProductId == productId);
                }
                var total = query.Count();
                var pagedData = query.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();
                return (pagedData, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetList");
                return (null, 0);
            }
        }
    }
}

