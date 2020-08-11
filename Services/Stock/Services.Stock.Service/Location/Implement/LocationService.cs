using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Location;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VErp.Services.Master.Service.Dictionay;
using VErp.Commons.Enums.StandardEnum;
using VErp.Services.Master.Service.Activity;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Stock.Service.Location.Implement
{
    public class LocationService : ILocationService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IActivityLogService _activityLogService;

        public LocationService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<LocationService> logger
            , IUnitService unitService
            , IActivityLogService activityLogService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _unitService = unitService;
            _activityLogService = activityLogService;
        }


        public async Task<int> AddLocation(LocationInput req)
        {
            var checkExisted = _stockDbContext.Location.Any(q =>
                q.StockId == req.StockId && q.Name == req.Name);

            if (checkExisted)
                throw new BadRequestException(LocationErrorCode.LocationAlreadyExisted);

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var locationInfo = new VErp.Infrastructure.EF.StockDB.Location
                    {
                        StockId = req.StockId,
                        Name = req.Name,
                        Description = req.Description,
                        Status = req.Status,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _stockDbContext.AddAsync(locationInfo);
                    await _stockDbContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.Location, locationInfo.LocationId, $"Thêm mới vị trí {locationInfo.Name} kho {locationInfo.StockId}", req.JsonSerialize());

                    return locationInfo.StockId;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public async Task<bool> DeleteLocation(int locationId)
        {
            var locationInfo = await _stockDbContext.Location.FirstOrDefaultAsync(p => p.LocationId == locationId);

            if (locationInfo == null)
            {
                throw new BadRequestException(LocationErrorCode.LocationNotFound);
            }

            locationInfo.IsDeleted = true;
            locationInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    locationInfo.IsDeleted = true;
                    locationInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    await _stockDbContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.Location, locationInfo.LocationId, $"Xóa vị trí {locationInfo.Name} kho: {locationInfo.StockId}", locationInfo.JsonSerialize());

                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public async Task<PageData<LocationOutput>> GetList(int stockId, string keyword, int page, int size)
        {
            var query = from l in _stockDbContext.Location
                        join s in _stockDbContext.Stock on l.StockId equals s.StockId
                        select new LocationOutput
                        {
                            LocationId = l.LocationId,
                            StockId = l.StockId,
                            StockName = s.StockName,
                            Name = l.Name,
                            Description = l.Description,
                            Status = l.Status
                        };

            if (stockId > 0)
            {
                query = query.Where(q => q.StockId == stockId);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.Name.Contains(keyword) || q.Description.Contains(keyword)
                        select q;
            }

            query = query.OrderBy(q => q.StockId);

            var total = await query.CountAsync();
            var locationList = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var pageData = new List<LocationOutput>();
            foreach (var item in locationList)
            {
                var locationInfo = new LocationOutput()
                {
                    LocationId = item.LocationId,
                    StockId = item.StockId,
                    StockName = item.StockName,
                    Name = item.Name,
                    Description = item.Description,
                    Status = item.Status

                };
                pageData.Add(locationInfo);
            }
            return (pageData, total);
        }

        public async Task<LocationOutput> GetLocationInfo(int locationId)
        {
            var locationInfo = await _stockDbContext.Location.FirstOrDefaultAsync(p => p.LocationId == locationId);

            if (locationInfo == null)
            {
                throw new BadRequestException(LocationErrorCode.LocationNotFound);
            }
            var stockInfo = await _stockDbContext.Stock.FirstOrDefaultAsync(p => p.StockId == locationInfo.StockId);
            return new LocationOutput()
            {
                LocationId = locationInfo.LocationId,
                StockId = locationInfo.StockId,
                StockName = stockInfo.StockName ?? string.Empty,
                Name = locationInfo.Name,
                Description = locationInfo.Description,
                Status = locationInfo.Status
            };
        }

        public async Task<bool> UpdateLocation(int locationId, LocationInput req)
        {
            req.Name = (req.Name ?? "").Trim();

            var checkExistsName = await _stockDbContext.Location.AnyAsync(p => locationId != p.LocationId && p.Name == req.Name && p.StockId == req.StockId);
            if (checkExistsName)
            {
                throw new BadRequestException(LocationErrorCode.LocationAlreadyExisted);
            }

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //Getdata
                    var locationInfo = await _stockDbContext.Location.FirstOrDefaultAsync(p => p.LocationId == locationId);
                    if (locationInfo == null)
                    {
                        throw new BadRequestException(LocationErrorCode.LocationNotFound);
                    }

                    //Update

                    locationInfo.StockId = req.StockId;
                    locationInfo.Name = req.Name;
                    locationInfo.Description = req.Description;
                    locationInfo.Status = req.Status;
                    locationInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    await _stockDbContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.Location, locationInfo.LocationId, $"Cập nhật thông tin vị trí {locationInfo.Name} kho hàng Id: {locationInfo.StockId}", req.JsonSerialize());

                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
    }
}
