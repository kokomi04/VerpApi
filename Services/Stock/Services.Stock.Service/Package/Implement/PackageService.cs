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
using VErp.Services.Stock.Model.Package;
using Microsoft.EntityFrameworkCore.Internal;
using System.Globalization;

namespace VErp.Services.Stock.Service.Package.Implement
{
    public class PackageService : IPackageService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityService _activityService;

        public PackageService(StockDBContext stockContext
           , IOptions<AppSetting> appSetting
           , ILogger<PackageService> logger
           , IActivityService activityService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityService = activityService;
        }

        public async Task<ServiceResult<long>> AddPackage(PackageInputModel req)
        {
            try
            {
                if (req == null || _stockDbContext.Package.Any(q => q.PackageCode == req.PackageCode))
                    return GeneralCode.InvalidParams;

                DateTime issuedDate = DateTime.MinValue;
                DateTime expiredDate = DateTime.MinValue;
                
                if (!string.IsNullOrEmpty(req.Date))
                    DateTime.TryParseExact(req.Date, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out issuedDate);
                if (!string.IsNullOrEmpty(req.ExpiryTime))
                    DateTime.TryParseExact(req.ExpiryTime, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out expiredDate);

                var obj = new VErp.Infrastructure.EF.StockDB.Package
                {
                    PackageCode = req.PackageCode,
                    LocationId = req.LocationId,
                    StockId =  req.StockId,
                    ProductId = req.ProductId,
                    Date = issuedDate == DateTime.MinValue ? null : (DateTime?)issuedDate,
                    ExpiryTime = expiredDate == DateTime.MinValue ? null : (DateTime?)expiredDate,
                    PrimaryUnitId = req.PrimaryUnitId,
                    PrimaryQuantity = req.PrimaryQuantity,
                    ProductUnitConversionId = req.ProductUnitConversionId,
                    SecondaryUnitId = req.SecondaryUnitId,
                    SecondaryQuantity = req.SecondaryQuantity,
                    PrimaryQuantityWaiting = req.PrimaryQuantityWaiting,
                    PrimaryQuantityRemaining = req.PrimaryQuantityRemaining,
                    SecondaryQuantityWaitting = req.SecondaryQuantityWaitting,
                    SecondaryQuantityRemaining = req.SecondaryQuantityRemaining,
                    PackageType  = req.PackageType,

                    CreatedDatetimeUtc = DateTime.Now,
                    UpdatedDatetimeUtc = DateTime.Now,
                    IsDeleted = false
                };
                await _stockDbContext.Package.AddAsync(obj);
                _activityService.CreateActivityAsync(EnumObjectType.Package, obj.PackageId, $"Tạo mới thông tin kiện {obj.PackageCode} ", null, obj);
                await _stockDbContext.SaveChangesAsync();

                return obj.PackageId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddPackage");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> UpdatePackage(long packageId, PackageInputModel req)
        {
            try
            {
                var obj = _stockDbContext.Package.FirstOrDefault(q => q.PackageId == packageId);
                var oldPackageData = GetInfoForLog(obj);
                if (obj == null)
                {
                    return PackageErrorCode.PackageNotFound;
                }
                
                var expiredDate = DateTime.MinValue;

                if (!string.IsNullOrEmpty(req.ExpiryTime))
                    DateTime.TryParseExact(req.ExpiryTime, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out expiredDate);

                obj.PackageCode = req.PackageCode;
                obj.LocationId = req.LocationId;
                obj.ExpiryTime = expiredDate == DateTime.MinValue ? null : (DateTime?)expiredDate;
                obj.UpdatedDatetimeUtc = DateTime.Now;

                _activityService.CreateActivityAsync(EnumObjectType.Package, obj.PackageId, $"Cập nhật thông tin kiện {obj.PackageCode} ", oldPackageData.JsonSerialize(), obj);
                await _stockDbContext.SaveChangesAsync();

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePackage");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> DeletePackage(long packageId)
        {
            try
            {
                var obj = _stockDbContext.Package.FirstOrDefault(q => q.PackageId == packageId);
                var oldPackageData = GetInfoForLog(obj);
                if (obj == null)
                {
                    return PackageErrorCode.PackageNotFound;
                }
                obj.UpdatedDatetimeUtc = DateTime.Now;
                obj.IsDeleted = true;

                _activityService.CreateActivityAsync(EnumObjectType.Package, obj.PackageId, $"Xoá thông tin kiện {obj.PackageCode} ", oldPackageData.JsonSerialize(), obj);
                await _stockDbContext.SaveChangesAsync();

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePackage");
                return GeneralCode.InternalError;
            }
        }

        public async Task<ServiceResult<PackageOutputModel>> GetInfo(long packageId)
        {
            try
            {
                var obj = _stockDbContext.Package.FirstOrDefault(q => q.PackageId == packageId);

                if (obj == null)
                {
                    return PackageErrorCode.PackageNotFound;
                }
                var locationObj = _stockDbContext.Location.FirstOrDefault(q => q.LocationId == obj.LocationId);
                var locationOutputModel = locationObj == null ? null : new LocationOutput
                {
                    LocationId = locationObj.LocationId,
                    StockId = locationObj.StockId,
                    StockName = string.Empty,
                    Name = locationObj.Name,
                    Description = locationObj.Description,
                    Status = locationObj.Status,
                };
                var packageOutputModel = new PackageOutputModel()
                {
                    PackageId = obj.PackageId,
                    //InventoryDetailId = obj.InventoryDetailId,
                    PackageCode = obj.PackageCode,
                    LocationId = obj.LocationId ?? 0,
                    StockId = obj.StockId ?? 0,
                    ProductId = obj.ProductId ?? 0,
                    Date = obj.Date,
                    ExpiryTime = obj.ExpiryTime,
                    ProductUnitConversionId = obj.ProductUnitConversionId,
                    PrimaryUnitId = obj.PrimaryUnitId,
                    PrimaryQuantity = obj.PrimaryQuantity,
                    SecondaryUnitId = obj.SecondaryUnitId,
                    SecondaryQuantity = obj.SecondaryQuantity,
                    CreatedDatetimeUtc = obj.CreatedDatetimeUtc,
                    UpdatedDatetimeUtc = obj.UpdatedDatetimeUtc,
                    PrimaryQuantityWaiting = obj.PrimaryQuantityWaiting,
                    PrimaryQuantityRemaining = obj.PrimaryQuantityRemaining,
                    SecondaryQuantityWaitting = obj.SecondaryQuantityWaitting,
                    SecondaryQuantityRemaining = obj.SecondaryQuantityRemaining,
                    LocationOutputModel = locationOutputModel
                };
                return packageOutputModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetInfo");
                return GeneralCode.InternalError;
            }
        }

        public async Task<PageData<PackageOutputModel>> GetList(int stockId = 0, string keyword = "", int page = 1, int size = 10)
        {
            try
            {
                var query = from p in _stockDbContext.Package
                            join l in _stockDbContext.Location on p.LocationId equals l.LocationId into pl
                            from lo in pl.DefaultIfEmpty()
                            select new { p, lo };               

                if (stockId > 0)
                    query = query.Where(q => q.p.StockId == stockId);

                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q => q.p.PackageCode.Contains(keyword));

                var totalRecord = query.AsNoTracking().Count();
                var resultList = new List<PackageOutputModel>(totalRecord);

                if (page > 0 && size > 0)
                {
                    var dataFromDb = query.AsNoTracking().Skip((page - 1) * size).Take(size).Select(q => new { Package = q.p, Location = q.lo }).ToList();
                    foreach (var d in dataFromDb)
                    {
                        var item = d;
                        var locationOutputModel = item.Location == null ? null : new LocationOutput()
                        {
                            LocationId = item.Location.LocationId,
                            StockId = item.Location.StockId,
                            StockName = string.Empty,
                            Name = item.Location.Name,
                            Description = item.Location.Description,
                            Status = item.Location.Status,
                        };
                        var model = new PackageOutputModel
                        {
                            PackageId = item.Package.PackageId,
                            //InventoryDetailId = item.Package.InventoryDetailId,
                            PackageCode = item.Package.PackageCode,
                            LocationId = item.Package.LocationId ?? 0,
                            StockId = item.Package.StockId ?? 0,
                            ProductId = item.Package.ProductId ?? 0,
                            Date = item.Package.Date,
                            ExpiryTime = item.Package.ExpiryTime,
                            ProductUnitConversionId = item.Package.ProductUnitConversionId,
                            PrimaryUnitId = item.Package.PrimaryUnitId,
                            PrimaryQuantity = item.Package.PrimaryQuantity,
                            SecondaryUnitId = item.Package.SecondaryUnitId,
                            SecondaryQuantity = item.Package.SecondaryQuantity,
                            CreatedDatetimeUtc = item.Package.CreatedDatetimeUtc,
                            UpdatedDatetimeUtc = item.Package.UpdatedDatetimeUtc,
                            PrimaryQuantityWaiting = item.Package.PrimaryQuantityWaiting,
                            PrimaryQuantityRemaining = item.Package.PrimaryQuantityRemaining,
                            SecondaryQuantityWaitting = item.Package.SecondaryQuantityWaitting,
                            SecondaryQuantityRemaining = item.Package.SecondaryQuantityRemaining,
                            LocationOutputModel = locationOutputModel
                        };
                        resultList.Add(model);
                    }
                }
                else
                {
                    var dataFromDb = query.AsNoTracking().Select(q => new { Package = q.p, Location = q.lo }).ToList();
                    foreach (var d in dataFromDb)
                    {
                        var item = d;
                        var locationOutputModel = item.Location == null ? null : new LocationOutput
                        {
                            LocationId = item.Location.LocationId,
                            StockId = item.Location.StockId,
                            StockName = string.Empty,
                            Name = item.Location.Name,
                            Description = item.Location.Description,
                            Status = item.Location.Status,
                        };
                        var model = new PackageOutputModel
                        {
                            PackageId = item.Package.PackageId,
                            //InventoryDetailId = item.Package.InventoryDetailId,
                            PackageCode = item.Package.PackageCode,
                            LocationId = item.Package.LocationId ?? 0,
                            StockId = item.Package.StockId ?? 0,
                            ProductId = item.Package.ProductId ?? 0,
                            Date = item.Package.Date,
                            ExpiryTime = item.Package.ExpiryTime,
                            ProductUnitConversionId = item.Package.ProductUnitConversionId,
                            PrimaryUnitId = item.Package.PrimaryUnitId,
                            PrimaryQuantity = item.Package.PrimaryQuantity,
                            SecondaryUnitId = item.Package.SecondaryUnitId,
                            SecondaryQuantity = item.Package.SecondaryQuantity,
                            CreatedDatetimeUtc = item.Package.CreatedDatetimeUtc,
                            UpdatedDatetimeUtc = item.Package.UpdatedDatetimeUtc,
                            PrimaryQuantityWaiting = item.Package.PrimaryQuantityWaiting,
                            PrimaryQuantityRemaining = item.Package.PrimaryQuantityRemaining,
                            SecondaryQuantityWaitting = item.Package.SecondaryQuantityWaitting,
                            SecondaryQuantityRemaining = item.Package.SecondaryQuantityRemaining,
                            LocationOutputModel = locationOutputModel
                        };
                        resultList.Add(model);
                    }
                }

                return (resultList, totalRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetList");
                return (null, 0);
            }
        }

        private object GetInfoForLog(VErp.Infrastructure.EF.StockDB.Package obj)
        {
            return obj;
        }

    }
}
