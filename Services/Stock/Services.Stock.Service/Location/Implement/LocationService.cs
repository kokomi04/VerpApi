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

namespace VErp.Services.Stock.Service.Location.Implement
{
    public class LocationService : ILocationService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IActivityService _activityService;

        public LocationService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<LocationService> logger
            , IUnitService unitService
            , IActivityService activityService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _unitService = unitService;
            _activityService = activityService;
        }


        public async Task<ServiceResult<int>> AddLocation(LocationInput req)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var locationInfo = new VErp.Infrastructure.EF.StockDB.Location()
                    {
                        StockId = req.StockId,
                        Name = req.Name,
                        Description = req.Description,                        
                        Status = req.Status,
                        CreatedDatetimeUtc = DateTime.Now,
                        UpdatedDatetimeUtc = DateTime.Now,
                        IsDeleted = false
                    };

                    await _stockDbContext.AddAsync(locationInfo);

                    await _stockDbContext.SaveChangesAsync();
                    trans.Commit();

                    var objLog = GetLocationInfoForLog(locationInfo);

                    await _activityService.CreateActivity(EnumObjectType.Location, locationInfo.StockId, $"Thêm mới vị trí {locationInfo.Name} kho {locationInfo.StockId}", null, objLog);

                    return locationInfo.StockId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "AddLocation");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> DeleteLocation(int locationId)
        {
            var locationInfo = await _stockDbContext.Location.FirstOrDefaultAsync(p => p.LocationId == locationId);

            if (locationInfo == null)
            {
                return LocationErrorCode.LocationNotFound;
            }

            locationInfo.IsDeleted = true;
            locationInfo.UpdatedDatetimeUtc = DateTime.UtcNow;


            var objLog = GetLocationInfoForLog(locationInfo);
            var dataBefore = objLog.JsonSerialize();

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    locationInfo.IsDeleted = true;
                    locationInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    await _stockDbContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityService.CreateActivity(EnumObjectType.Location, locationInfo.StockId, $"Xóa vị trí {locationInfo.Name} kho: {locationInfo.StockId}", dataBefore, null);

                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "DeleteLocation");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<PageData<LocationOutput>> GetList(int stockId, string keyword, int page, int size)
        {
            var query = from p in _stockDbContext.Location
                        select p;
            if(stockId > 0)
            {
                query = query.Where(q => q.StockId == stockId);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.Name.Contains(keyword) || q.Description.Contains(keyword)
                        select q;
            }

            var total = await query.CountAsync();
            var locationList = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var pageData = new List<LocationOutput>();
            foreach (var item in locationList)
            {
                var locationInfo = new LocationOutput()
                {
                    LocationId = item.LocationId,
                    StockId = item.StockId,
                    Name = item.Name,
                    Description = item.Description,                    
                    Status = item.Status

                };
                pageData.Add(locationInfo);
            }
            return (pageData, total);
        }

        public async Task<ServiceResult<LocationOutput>> GetLocationInfo(int locationId)
        {
            var locationInfo = await _stockDbContext.Location.FirstOrDefaultAsync(p => p.LocationId == locationId);
            if (locationInfo == null)
            {
                return LocationErrorCode.LocationNotFound;
            }
            return new LocationOutput()
            {
                LocationId = locationInfo.LocationId,
                StockId = locationInfo.StockId,
                Name = locationInfo.Name,
                Description = locationInfo.Description,
                Status = locationInfo.Status
            };
        }

        public async Task<Enum> UpdateLocation(int locationId, LocationInput req)
        {
            req.Name = (req.Name ?? "").Trim();

            var checkExistsName = await _stockDbContext.Location.AnyAsync(p => p.Name == req.Name && p.StockId != req.StockId);
            if (checkExistsName)
            {
                return LocationErrorCode.LocationAlreadyExisted;
            }

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //Getdata
                    var locationInfo = await _stockDbContext.Location.FirstOrDefaultAsync(p => p.LocationId == locationId);
                    if (locationInfo == null)
                    {
                        return LocationErrorCode.LocationNotFound;
                    }
                    var originalObj = GetLocationInfoForLog(locationInfo);

                    //Update

                    locationInfo.StockId = req.StockId;
                    locationInfo.Name = req.Name;
                    locationInfo.Description = req.Description;                                       
                    locationInfo.Status = req.Status;
                    locationInfo.UpdatedDatetimeUtc = DateTime.Now;

                    await _stockDbContext.SaveChangesAsync();
                    trans.Commit();

                    var objLog = GetLocationInfoForLog(locationInfo);

                    await _activityService.CreateActivity(EnumObjectType.Location, locationInfo.LocationId, $"Cập nhật thông tin vị trí {locationInfo.Name} kho hàng Id: {locationInfo.StockId}", originalObj.JsonSerialize(), objLog);

                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "UpdateLocation");
                    return GeneralCode.InternalError;
                }
            }
        }

        private object GetLocationInfoForLog(VErp.Infrastructure.EF.StockDB.Location locationInfo)
        {
            return locationInfo;
        }
    }
}
