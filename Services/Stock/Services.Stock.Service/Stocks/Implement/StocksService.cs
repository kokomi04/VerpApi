using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Stocks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VErp.Services.Master.Service.Dictionay;
using VErp.Commons.Enums.StandardEnum;
using VErp.Services.Master.Service.Activity;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using static VErp.Services.Stock.Model.Stocks.StocksModel;

namespace VErp.Services.Stock.Service.Stocks.Implement
{
    public class StocksService : IStocksService
    {
        private readonly StockDBContext _stockContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IActivityService _activityService;

        public StocksService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<StocksService> logger
            , IUnitService unitService
            , IActivityService activityService
            )
        {
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _unitService = unitService;
            _activityService = activityService;
        }

        public async Task<ServiceResult<int>> AddStocks(StocksModel req)
        {
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var stocksInfo = new VErp.Infrastructure.EF.StockDB.Stock()
                    {
                        //StockId = req.StockId,
                        StockName = req.StockName,
                        CreatedDatetimeUtc = DateTime.Now,
                        UpdatedDatetimeUtc = DateTime.Now,
                        IsDeleted = false                        
                    };

                    await _stockContext.AddAsync(stocksInfo);

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    var objLog = GetStocksForLog(stocksInfo);

                    await _activityService.CreateActivity(EnumObjectType.Stocks, stocksInfo.StockId, $"Thêm mới sản phẩm {stocksInfo.StockName}", null, objLog);

                    return stocksInfo.StockId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "AddStocks");
                    return GeneralCode.InternalError;
                }
            }
        }


        public async Task<ServiceResult<StocksOutput>> StocksInfo(int stocksId)
        {
            var stocksInfo = await _stockContext.Stock.FirstOrDefaultAsync(p => p.StockId == stocksId);
            if (stocksInfo == null)
            {
                return StocksErrorCode.StocksNotFound;
            }
            return new StocksOutput()
            {
                StockId = stocksInfo.StockId,
                StockName = stocksInfo.StockName                
            };
        }


        public async Task<Enum> UpdateStocks(int stocksId, StocksModel req)
        {
            req.StockName = (req.StockName ?? "").Trim();

            var checkExistsName = await _stockContext.Stock.AnyAsync(p => p.StockName == req.StockName && p.StockId != stocksId);
            if (checkExistsName)
            {
                return ProductErrorCode.ProductCodeAlreadyExisted;
            }

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //Getdata
                    var stocksInfo = await _stockContext.Stock.FirstOrDefaultAsync(p => p.StockId == stocksId);
                    if (stocksInfo == null)
                    {
                        return StocksErrorCode.StocksNotFound;
                    }

                    var productExtra = await _stockContext.ProductExtraInfo.FirstOrDefaultAsync(p => p.ProductId == stocksId);
                    var productStockInfo = await _stockContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == stocksId);
                    var stockValidations = await _stockContext.ProductStockValidation.Where(p => p.ProductId == stocksId).ToListAsync();
                    var unitConverions = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == stocksId).ToListAsync();

                    var originalObj = GetStocksForLog(stocksInfo);

                    //Update
                                       
                    //stocksInfo.StockId = req.StockId;
                    stocksInfo.StockName = req.StockName;
                    
                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    var objLog = GetStocksForLog(stocksInfo);

                    await _activityService.CreateActivity(EnumObjectType.Stocks, stocksInfo.StockId, $"Cập nhật thông tin kho hàng {stocksInfo.StockName}", originalObj.JsonSerialize(), objLog);

                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "UpdateStocks");
                    return GeneralCode.InternalError;
                }
            }
        }


        public async Task<Enum> DeleteStocks(int stocksId)
        {
            var stocksInfo = await _stockContext.Stock.FirstOrDefaultAsync(p => p.StockId == stocksId);

            if (stocksInfo == null)
            {
                return ProductErrorCode.ProductNotFound;
            }

            stocksInfo.IsDeleted = true;
            stocksInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

           
            var objLog = GetStocksForLog(stocksInfo);
            var dataBefore = objLog.JsonSerialize();

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    stocksInfo.IsDeleted = true;
                    stocksInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
                                        
                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityService.CreateActivity(EnumObjectType.Product, stocksInfo.StockId, $"Xóa sản phẩm {stocksInfo.StockName}", dataBefore, null);

                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "DeleteStocks");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<PageData<StocksOutput>> GetList(string keyword, int page, int size)
        {
            var query = from p in _stockContext.Stock
                        select p;
                

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.StockName.Contains(keyword)                        
                        select q;
            }

            var total = await query.CountAsync();
            var lstData = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var pageData = new List<StocksOutput>();
            foreach (var item in lstData)
            {
                var stocksInfo = new StocksOutput()
                {
                    StockId = item.StockId,
                    StockName = item.StockName,
                };
                pageData.Add(stocksInfo);
            }


            return (pageData, total);
        }

        private object GetStocksForLog(VErp.Infrastructure.EF.StockDB.Stock stocksInfo)
        {
            return new
            {
                StocksInfo = stocksInfo
            };
        }
    }
}
