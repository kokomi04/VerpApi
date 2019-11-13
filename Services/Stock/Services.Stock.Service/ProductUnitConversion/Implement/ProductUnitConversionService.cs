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
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Inventory.Implement;

namespace VErp.Services.Stock.Service.ProductUnitConversion.Implement
{
    public class ProductUnitConversionService : IProductUnitConversionService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly MasterDBContext _masterDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityService _activityService;
        private readonly IUnitService _unitService;

        public ProductUnitConversionService(StockDBContext stockContext, MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductUnitConversionService> logger
            , IActivityService activityService
            , IUnitService unitService
        )
        {
            _stockDbContext = stockContext;
            _masterDBContext = masterDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityService = activityService;
            _unitService = unitService;
        }

        public async Task<PageData<ProductUnitConversionOutput>> GetList(int productId, int page = 0, int size = 0)
        {
            try
            {
                var query = from p in _stockDbContext.ProductUnitConversion
                            join u in _masterDBContext.Unit on p.SecondaryUnitId equals u.UnitId
                            select new { p, u };

                if (productId > 0)
                {
                    query = query.Where(q => q.p.ProductId == productId);
                }
                var total = query.Count();
                var resultList = new List<ProductUnitConversionOutput>(total);
                if (page > 0 && size > 0)
                {
                    resultList = query.AsNoTracking().Skip((page - 1) * size).Take(size).ToList().Select(q => new ProductUnitConversionOutput
                    {
                        ProductUnitConversionId = q.p.ProductUnitConversionId,
                        ProductUnitConversionName = q.p.ProductUnitConversionName,
                        ProductId = q.p.ProductId,
                        SecondaryUnitId = q.p.SecondaryUnitId,
                        SecondaryUnitName = q.u.UnitName,
                        FactorExpression = q.p.FactorExpression,
                        ConversionDescription = q.p.ConversionDescription
                    }).ToList();
                }
                else
                    resultList = query.AsNoTracking().ToList().Select(q => new ProductUnitConversionOutput
                    {
                        ProductUnitConversionId = q.p.ProductUnitConversionId,
                        ProductUnitConversionName = q.p.ProductUnitConversionName,
                        ProductId = q.p.ProductId,
                        SecondaryUnitId = q.p.SecondaryUnitId,
                        SecondaryUnitName = q.u.UnitName,
                        FactorExpression = q.p.FactorExpression,
                        ConversionDescription = q.p.ConversionDescription
                    }).ToList();
                return (resultList, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetList");
                return (null, 0);
            }
        }
    }
}

