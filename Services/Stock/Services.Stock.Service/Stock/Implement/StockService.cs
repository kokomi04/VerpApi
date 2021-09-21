using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Stock;
using VErp.Infrastructure.EF.EFExtensions;
using StockEntity = VErp.Infrastructure.EF.StockDB.Stock;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using Verp.Resources.Stock.Stock;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class StockService : IStockService
    {
        private readonly StockDBContext _stockContext;
        private readonly IRoleHelperService _roleHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly ObjectActivityLogFacade _stockActivityLog;


        public StockService(
            StockDBSubsidiaryContext stockContext
            , IActivityLogService activityLogService
            , IRoleHelperService roleHelperService
            , ICurrentContextService currentContextService
            )
        {
            _stockContext = stockContext;
            _roleHelperService = roleHelperService;
            _currentContextService = currentContextService;
            _stockActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Stock);
        }


        #region CRUD Stocks
        public async Task<int> AddStock(StockModel req)
        {
            if (_stockContext.Stock.Any(q => q.StockName.ToLower() == req.StockName.ToLower()))
                throw new BadRequestException(StockErrorCode.StockNameAlreadyExisted);

            // Validate unique
            if (_stockContext.Stock.Any(st => st.StockCode == req.StockCode))
                throw new BadRequestException(StockErrorCode.StockCodeAlreadyExisted);

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var stockInfo = new StockEntity()
                    {
                        //StockId = req.StockId,
                        StockName = req.StockName,
                        StockCode = req.StockCode,
                        Description = req.Description,
                        StockKeeperId = req.StockKeeperId,
                        StockKeeperName = req.StockKeeperName,
                        Type = req.Type,
                        Status = req.Status,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _stockContext.AddAsync(stockInfo);

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    await _stockActivityLog.LogBuilder(() => StockActivityLogMessage.Create)
                      .MessageResourceFormatDatas(req.StockCode)
                      .ObjectId(stockInfo.StockId)
                      .JsonData(req.JsonSerialize())
                      .CreateLog();

                    await _roleHelperService.GrantDataForAllRoles(EnumObjectType.Stock, stockInfo.StockId);

                    return stockInfo.StockId;
                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;

                }
            }
        }

        public async Task<StockOutput> StockInfo(int stockId)
        {
            var stockInfo = await _stockContext.Stock.FirstOrDefaultAsync(p => p.StockId == stockId);
            if (stockInfo == null)
            {
                throw new BadRequestException(StockErrorCode.StockNotFound);
            }
            return new StockOutput()
            {
                StockId = stockInfo.StockId,
                StockName = stockInfo.StockName,
                StockCode = stockInfo.StockCode,
                Description = stockInfo.Description,
                StockKeeperId = stockInfo.StockKeeperId,
                StockKeeperName = stockInfo.StockKeeperName,
                Type = stockInfo.Type,
                Status = stockInfo.Status
            };
        }

        public async Task<bool> UpdateStock(int stockId, StockModel req)
        {
            req.StockName = (req.StockName ?? "").Trim();

            var checkExistsName = await _stockContext.Stock.AnyAsync(p => p.StockName == req.StockName && p.StockId != stockId);
            if (checkExistsName)
            {
                throw new BadRequestException(StockErrorCode.StockNameAlreadyExisted);
            }

            var checkExistsCode = await _stockContext.Stock.AnyAsync(p => p.StockCode == req.StockCode && p.StockId != stockId);
            if (checkExistsCode)
            {
                throw new BadRequestException(StockErrorCode.StockCodeAlreadyExisted);
            }

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //Getdata
                    var stockInfo = await _stockContext.Stock.FirstOrDefaultAsync(p => p.StockId == stockId);
                    if (stockInfo == null)
                    {
                        throw new BadRequestException(StockErrorCode.StockNotFound);
                    }

                    //Update

                    //stockInfo.StockId = req.StockId;
                    stockInfo.StockName = req.StockName;
                    stockInfo.StockCode = req.StockCode;
                    stockInfo.Description = req.Description;
                    stockInfo.StockKeeperId = req.StockKeeperId;
                    stockInfo.StockKeeperName = req.StockKeeperName;
                    stockInfo.Type = req.Type;
                    stockInfo.Status = req.Status;
                    stockInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    
                    await _stockActivityLog.LogBuilder(() => StockActivityLogMessage.Update)
                      .MessageResourceFormatDatas(req.StockCode)
                      .ObjectId(stockInfo.StockId)
                      .JsonData(req.JsonSerialize())
                      .CreateLog();

                    return true;
                }
                catch (Exception)
                {

                    trans.TryRollbackTransaction();
                    throw;
                }
            }
        }

        public async Task<bool> DeleteStock(int stockId)
        {
            var stockInfo = await _stockContext.Stock.FirstOrDefaultAsync(p => p.StockId == stockId);

            if (stockInfo == null)
            {
                throw new BadRequestException(StockErrorCode.StockNotFound);
            }

            stockInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    stockInfo.IsDeleted = true;
                    stockInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();
                    

                    await _stockActivityLog.LogBuilder(() => StockActivityLogMessage.Delete)
                      .MessageResourceFormatDatas(stockInfo.StockCode)
                      .ObjectId(stockInfo.StockId)
                      .JsonData(stockInfo.JsonSerialize())
                      .CreateLog();

                    return true;
                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;

                }
            }
        }
        public async Task<PageData<StockOutput>> GetAll(string keyword, int page, int size, Clause filters)
        {
            keyword = (keyword ?? "").Trim();

            var query = from p in _stockContext.Stock
                        select p;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.StockName.Contains(keyword) || q.StockCode.Contains(keyword)
                        select q;
            }

            query = query.InternalFilter(filters);

            var total = await query.CountAsync();
            var lstData = size > 0 ? await query.Skip((page - 1) * size).Take(size).ToListAsync() : await query.ToListAsync();

            var pagedData = new List<StockOutput>();
            foreach (var item in lstData)
            {
                var stockInfo = new StockOutput()
                {
                    StockId = item.StockId,
                    StockName = item.StockName,
                    StockCode = item.StockCode,
                    Description = item.Description,
                    StockKeeperId = item.StockKeeperId,
                    StockKeeperName = item.StockKeeperName,
                    Type = item.Type,
                    Status = item.Status

                };
                pagedData.Add(stockInfo);
            }
            return (pagedData, total);
        }
        #endregion




        public async Task<PageData<StockOutput>> GetListByUserId(int userId, string keyword, int page, int size)
        {
            var query = from p in _stockContext.Stock
                        where p.StockKeeperId == userId
                        select p;


            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.StockName.Contains(keyword) || q.StockCode.Contains(keyword)
                        select q;
            }

            var total = await query.CountAsync();
            var lstData = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var pagedData = new List<StockOutput>();
            foreach (var item in lstData)
            {
                var stockInfo = new StockOutput()
                {
                    StockId = item.StockId,
                    StockName = item.StockName,
                    StockCode = item.StockCode,
                    Description = item.Description,
                    StockKeeperId = item.StockKeeperId,
                    StockKeeperName = item.StockKeeperName,
                    Type = item.Type,
                    Status = item.Status

                };
                pagedData.Add(stockInfo);
            }
            return (pagedData, total);
        }

        public async Task<IList<SimpleStockInfo>> GetSimpleList()
        {
            return await _stockContext.Stock.Select(s => new SimpleStockInfo()
            {
                StockId = s.StockId,
                StockName = s.StockName,
                StockCode = s.StockCode
            }).ToListAsync();
        }

    }
}
