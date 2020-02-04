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
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.FileResources;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductUnitConversionService : IProductUnitConversionService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly MasterDBContext _masterDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IUnitService _unitService;

        public ProductUnitConversionService(StockDBContext stockContext, MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductUnitConversionService> logger
            , IActivityLogService activityLogService
            , IUnitService unitService
        )
        {
            _stockDbContext = stockContext;
            _masterDBContext = masterDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _unitService = unitService;
        }

        public async Task<PageData<ProductUnitConversionOutput>> GetList(int productId, int page = 0, int size = 0)
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
                var secondUnitIdList = query.Select(q => q.SecondaryUnitId).ToList();

                var unitList = await _masterDBContext.Unit.AsNoTracking().Where(q => secondUnitIdList.Contains(q.UnitId)).ToListAsync();

                var resultFromDb = new List<VErp.Infrastructure.EF.StockDB.ProductUnitConversion>(total);
                if (page > 0 && size > 0)
                    resultFromDb = query.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();
                else
                    resultFromDb = query.AsNoTracking().ToList();

                var resultList = new List<ProductUnitConversionOutput>(total);
                foreach (var item in resultFromDb)
                {
                    var unitObj = unitList.FirstOrDefault(q => q.UnitId == item.SecondaryUnitId);
                    var p = new ProductUnitConversionOutput
                    {
                        ProductUnitConversionId = item.ProductUnitConversionId,
                        ProductUnitConversionName = item.ProductUnitConversionName,
                        ProductId = item.ProductId,
                        SecondaryUnitId = item.SecondaryUnitId,
                        SecondaryUnitName = unitObj != null ? unitObj.UnitName : null,
                        FactorExpression = item.FactorExpression,
                        ConversionDescription = item.ConversionDescription,
                        IsDefault = item.IsDefault
                    };
                    resultList.Add(p);
                }
                return (resultList, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetList");
                return (new PageData<ProductUnitConversionOutput> { Total = 0, List = null });
            }
        }

        public async Task<ServiceResult<PageData<ProductUnitConversionByProductOutput>>> GetListByProducts(IList<int> productIds, int page = 0, int size = 0)
        {
            try
            {
                if (productIds == null || productIds.Count == 0)
                    return (PageData<ProductUnitConversionByProductOutput>)(new List<ProductUnitConversionByProductOutput>(), 0);

                var query = from c in _stockDbContext.ProductUnitConversion
                            where productIds.Contains(c.ProductId)
                            select new ProductUnitConversionByProductOutput()
                            {
                                ProductUnitConversionId = c.ProductUnitConversionId,
                                ProductUnitConversionName = c.ProductUnitConversionName,
                                ProductId = c.ProductId,
                                SecondaryUnitId = c.SecondaryUnitId,
                                FactorExpression = c.FactorExpression,
                                ConversionDescription = c.ConversionDescription,
                                IsFreeStyle = c.IsFreeStyle,
                                IsDefault = c.IsDefault,
                            };

                var total = query.Count();

                IList<ProductUnitConversionByProductOutput> pagedData;
                if (size <= 0)
                {
                    pagedData = await query.ToListAsync();
                }
                else
                {
                    pagedData = await query.Skip((page - 1) * size).Take(size).ToListAsync();
                }

                return (PageData<ProductUnitConversionByProductOutput>)(pagedData, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetListByProducts");
                return GeneralCode.InternalError;
            }
        }
    }
}

