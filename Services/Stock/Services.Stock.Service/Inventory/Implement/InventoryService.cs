using System;
using System.Collections.Generic;
using System.Text;
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
using VErp.Commons.Library;

namespace VErp.Services.Stock.Service.Invetory.Implement
{
    public class InventoryService : IInventoryService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IActivityService _activityService;

        public InventoryService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<InventoryService> logger
            , IUnitService unitService
            , IActivityService activityService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _unitService = unitService;
            _activityService = activityService;
        }

        /// <summary>
        /// Tìm kiếm danh sách phiếu nhập xuất kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockId"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<PageData<InventoryOutput>> GetList(string keyword, int stockId = 0, DateTime? beginTime = null, DateTime? endTime = null, int page = 1, int size = 10)
        {
            var query = from i in _stockDbContext.Inventory
                        join id in _stockDbContext.InventoryDetail on i.InventoryId equals id.InventoryId
                        join p in _stockDbContext.Product on id.ProductId equals p.ProductId
                        join s in _stockDbContext.Stock on i.StockId equals s.StockId
                        select new { i, id, p, s };
            if (stockId > 0)
            {
                query = query.Where(q => q.i.StockId == stockId);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.i.InventoryCode.Contains(keyword) || q.p.ProductCode.Contains(keyword) || q.p.ProductName.Contains(keyword) || q.i.Shipper.Contains(keyword) || q.id.RefObjectCode.Contains(keyword)
                        select q;
            }

            if (beginTime.HasValue && endTime.HasValue)
            {
                query = query.Where(q => q.i.DateUtc >= beginTime && q.i.DateUtc <= endTime);
            }
            else
            {
                if (beginTime.HasValue)
                {
                    query = query.Where(q => q.i.DateUtc >= beginTime);
                }
                if (endTime.HasValue)
                {
                    query = query.Where(q => q.i.DateUtc <= endTime);
                }
            }


            var total = await query.CountAsync();
            var dataList = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            var inventoryList = dataList.Select(q => q.i).ToList();
            var pagedData = new List<InventoryOutput>();
            foreach (var item in inventoryList)
            {
                var listInventoryDetails = dataList.Where(q => q.id.InventoryId == item.InventoryId).Select(q => q.id).ToList();
                var listInventoryDetailsOutput = listInventoryDetails.Select(q => new InventoryDetailOutput
                {
                    InventoryId = q.InventoryId,
                    InventoryDetailId = q.InventoryDetailId,
                    ProductId = q.ProductId,
                    IsDeleted = q.IsDeleted,
                    PrimaryUnitId = q.PrimaryUnitId,
                    PrimaryQuantity = q.PrimaryQuantity,
                    SecondaryUnitId = q.SecondaryUnitId,
                    SecondaryQuantity = q.SecondaryQuantity,
                    ManufactureDatetimeUtc = q.ManufactureDatetimeUtc,
                    PackageId = q.PackageId,
                    RefObjectTypeId = q.RefObjectTypeId,
                    RefObjectId = q.RefObjectId,
                    RefObjectCode = q.RefObjectCode
                }).ToList();
                var stockInfo = dataList.Select(q => q.s).FirstOrDefault(q => q.StockId == item.StockId);
                var inventoryOutput = new InventoryOutput()
                {
                    InventoryId = item.InventoryId,
                    StockId = item.StockId,
                    InventoryCode = item.InventoryCode,
                    InventoryTypeId = item.InventoryTypeId,
                    Shipper = item.Shipper,
                    Content = item.Content,
                    DateUtc = item.DateUtc,
                    CustomerId = item.CustomerId,
                    Department = item.Department,
                    UserId = item.UserId,
                    InvoiceFileId = item.InvoiceFileId,
                    CreatedByUserId = item.CreatedByUserId,
                    UpdatedByUserId = item.UpdatedByUserId,

                    StockOutput = new Model.Stock.StockOutput()
                    {
                        StockId = stockInfo.StockId,
                        StockName = stockInfo.StockName,
                        StockKeeperName = stockInfo.StockKeeperName,
                        StockKeeperId = stockInfo.StockKeeperId
                    },
                    InventoryDetailOutputList = listInventoryDetailsOutput
                };
                pagedData.Add(inventoryOutput);
            }
            return (pagedData, total);
        }

        public async Task<ServiceResult<long>> AddInventory(InventoryInput req)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var inventoryObj = new VErp.Infrastructure.EF.StockDB.Inventory()
                    {
                        StockId = req.StockId,
                        InventoryCode = req.InventoryCode,
                        InventoryTypeId = req.InventoryTypeId,
                        Shipper = req.Shipper,
                        Content = req.Content,
                        DateUtc = req.DateUtc,
                        CustomerId = req.CustomerId,
                        Department = req.Department,
                        UserId = req.UserId,
                        InvoiceFileId = req.InvoiceFileId,
                        CreatedByUserId = req.CreatedByUserId,
                        UpdatedByUserId = 0,
                        CreatedDatetimeUtc = DateTime.Now,
                        UpdatedDatetimeUtc = DateTime.Now,
                        IsDeleted = false
                    };
                    await _stockDbContext.AddAsync(inventoryObj);
                    await _stockDbContext.SaveChangesAsync();

                    if (req.InventoryDetailInputList.Count > 0)
                    {
                        var inventoryDetailList = new List<InventoryDetail>(req.InventoryDetailInputList.Count);
                        foreach (var details in req.InventoryDetailInputList)
                        {
                            var package = new Package()
                            {
                                PackageCode = string.Empty,
                                LocationId = null
                            };
                            await _stockDbContext.AddAsync(package);
                            await _stockDbContext.SaveChangesAsync();

                            inventoryDetailList.Add(new InventoryDetail
                            {
                                InventoryId = inventoryObj.InventoryId,
                                ProductId = details.ProductId,
                                CreatedDatetimeUtc = DateTime.Now,
                                UpdatedDatetimeUtc = DateTime.Now,
                                IsDeleted = false,
                                PrimaryUnitId = details.PrimaryUnitId,
                                PrimaryQuantity = details.PrimaryQuantity,
                                SecondaryUnitId = details.SecondaryUnitId,
                                SecondaryQuantity = details.SecondaryUnitId,
                                ManufactureDatetimeUtc = DateTime.Now,
                                PackageId = package.PackageId,
                                RefObjectTypeId = details.RefObjectTypeId,
                                RefObjectId = details.RefObjectId,
                                RefObjectCode = details.RefObjectCode
                            });
                        }
                        await _stockDbContext.AddRangeAsync(inventoryDetailList);
                        await _stockDbContext.SaveChangesAsync();
                    }

                    trans.Commit();

                    var objLog = GetInventoryInfoForLog(inventoryObj);

                    _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, $"Thêm mới phiếu nhập/xuất kho, mã: {inventoryObj.InventoryCode} ", null, objLog);

                    return inventoryObj.InventoryId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "AddInventory");
                    return GeneralCode.InternalError;
                }
            }
        }

        private object GetInventoryInfoForLog(VErp.Infrastructure.EF.StockDB.Inventory inventoryObj)
        {
            return inventoryObj;
        }
    }
}
