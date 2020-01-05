using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Stock.Model.Location;
using VErp.Services.Stock.Model.Package;
using PackageModel = VErp.Infrastructure.EF.StockDB.Package;

namespace VErp.Services.Stock.Service.Stock.Implement
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

        //public async Task<ServiceResult<long>> AddPackage(PackageInputModel req)
        //{
        //    try
        //    {
        //        if (req == null || _stockDbContext.Package.Any(q => q.PackageCode == req.PackageCode))
        //            return GeneralCode.InvalidParams;

        //        DateTime issuedDate = DateTime.MinValue;
        //        DateTime expiredDate = DateTime.MinValue;

        //        if (!string.IsNullOrEmpty(req.Date))
        //            DateTime.TryParseExact(req.Date, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out issuedDate);
        //        if (!string.IsNullOrEmpty(req.ExpiryTime))
        //            DateTime.TryParseExact(req.ExpiryTime, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out expiredDate);

        //        var obj = new VErp.Infrastructure.EF.StockDB.Package
        //        {
        //            PackageTypeId = req.PackageTypeId,
        //            PackageCode = req.PackageCode,
        //            LocationId = req.LocationId,
        //            StockId = req.StockId,
        //            ProductId = req.ProductId,
        //            Date = issuedDate == DateTime.MinValue ? null : (DateTime?)issuedDate,
        //            ExpiryTime = expiredDate == DateTime.MinValue ? null : (DateTime?)expiredDate,
        //            PrimaryUnitId = req.PrimaryUnitId,
        //            PrimaryQuantity = req.PrimaryQuantity,
        //            ProductUnitConversionId = req.ProductUnitConversionId,
        //            ProductUnitConversionQuantity = req.ProductUnitConversionQuantity,
        //            PrimaryQuantityWaiting = req.PrimaryQuantityWaiting,
        //            PrimaryQuantityRemaining = req.PrimaryQuantityRemaining,
        //            ProductUnitConversionWaitting = req.ProductUnitConversionWaitting,
        //            ProductUnitConversionRemaining = req.ProductUnitConversionRemaining,

        //            CreatedDatetimeUtc = DateTime.Now,
        //            UpdatedDatetimeUtc = DateTime.Now,
        //            IsDeleted = false
        //        };
        //        await _stockDbContext.Package.AddAsync(obj);
        //        _activityService.CreateActivityAsync(EnumObjectType.Package, obj.PackageId, $"Tạo mới thông tin kiện {obj.PackageCode} ", null, obj);
        //        await _stockDbContext.SaveChangesAsync();

        //        return obj.PackageId;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "AddPackage");
        //        return GeneralCode.InternalError;
        //    }
        //}

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

        //public async Task<Enum> DeletePackage(long packageId)
        //{
        //    try
        //    {
        //        var obj = _stockDbContext.Package.FirstOrDefault(q => q.PackageId == packageId);
        //        var oldPackageData = GetInfoForLog(obj);
        //        if (obj == null)
        //        {
        //            return PackageErrorCode.PackageNotFound;
        //        }
        //        obj.UpdatedDatetimeUtc = DateTime.Now;
        //        obj.IsDeleted = true;

        //        _activityService.CreateActivityAsync(EnumObjectType.Package, obj.PackageId, $"Xoá thông tin kiện {obj.PackageCode} ", oldPackageData.JsonSerialize(), obj);
        //        await _stockDbContext.SaveChangesAsync();

        //        return GeneralCode.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "UpdatePackage");
        //        return GeneralCode.InternalError;
        //    }
        //}


        public async Task<Enum> SplitPackage(long packageId, PackageSplitInput req)
        {
            if (req == null || req.ToPackages == null || req.ToPackages.Count == 0 || req.ToPackages.Any(p => p.ProductUnitConversionQuantity <= 0))
                return GeneralCode.InvalidParams;

            var packageInfo = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageId == packageId);
            if (packageInfo == null) return PackageErrorCode.PackageNotFound;

            if (packageInfo.ProductUnitConversionWaitting > 0)
            {
                return PackageErrorCode.HasSomeQualtityWaitingForApproved;
            }

            Infrastructure.EF.StockDB.ProductUnitConversion unitConversionInfo = null;

            unitConversionInfo = await _stockDbContext.ProductUnitConversion.FirstOrDefaultAsync(c => c.ProductUnitConversionId == packageInfo.ProductUnitConversionId);

            if (unitConversionInfo == null) return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;


            var totalSecondaryInput = req.ToPackages.Sum(p => p.ProductUnitConversionQuantity);
            if (totalSecondaryInput > packageInfo.ProductUnitConversionRemaining) return PackageErrorCode.QualtityOfProductInPackageNotEnough;

            var newPackages = new List<PackageModel>();


            foreach (var package in req.ToPackages)
            {
                if (string.IsNullOrWhiteSpace(package.PackageCode))
                {
                    return PackageErrorCode.PackageCodeEmpty;
                }

                var packageExisted = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageCode == package.PackageCode);
                if (packageExisted != null)
                {
                    return PackageErrorCode.PackageAlreadyExisted;
                }

                decimal qualtityInPrimaryUnit = package.PrimaryQuantity;
                if (unitConversionInfo.IsFreeStyle == false)
                {
                    qualtityInPrimaryUnit = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(package.ProductUnitConversionQuantity, unitConversionInfo.FactorExpression);
                    if (!(qualtityInPrimaryUnit > 0))
                    {
                        return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                    }
                }

                if (qualtityInPrimaryUnit <= 0 || package.ProductUnitConversionQuantity <= 0)
                    return GeneralCode.InvalidParams;


                newPackages.Add(new PackageModel()
                {
                    PackageCode = package.PackageCode,
                    LocationId = package.LocationId,
                    StockId = packageInfo.StockId,
                    ProductId = packageInfo.ProductId,
                    Date = packageInfo.Date,
                    ExpiryTime = packageInfo.ExpiryTime,
                    PrimaryUnitId = packageInfo.PrimaryUnitId,
                    PrimaryQuantity = qualtityInPrimaryUnit,
                    ProductUnitConversionId = packageInfo.ProductUnitConversionId,
                    ProductUnitConversionQuantity = package.ProductUnitConversionQuantity,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    PrimaryQuantityWaiting = 0,
                    PrimaryQuantityRemaining = qualtityInPrimaryUnit,
                    ProductUnitConversionWaitting = 0,
                    ProductUnitConversionRemaining = package.ProductUnitConversionQuantity,
                    PackageTypeId = (int)EnumPackageType.Custom
                });

                packageInfo.PrimaryQuantityRemaining -= qualtityInPrimaryUnit;
                packageInfo.ProductUnitConversionRemaining -= package.ProductUnitConversionQuantity;

            }

            var packageRefs = new List<PackageRef>();
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                foreach (var newPackage in newPackages)
                {
                    await _stockDbContext.Package.AddAsync(newPackage);
                    await _stockDbContext.SaveChangesAsync();

                    packageRefs.Add(new PackageRef()
                    {
                        PackageId = newPackage.PackageId,
                        RefPackageId = packageId,
                        PrimaryUnitId = newPackage.PrimaryUnitId,
                        PrimaryQuantity = newPackage.PrimaryQuantityRemaining,
                        ProductUnitConversionId = newPackage.ProductUnitConversionId,
                        ProductUnitConversionQuantity = newPackage.ProductUnitConversionRemaining,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        PackageOperationTypeId = (int)EnumPackageOperationType.Split
                    });
                }

                await _stockDbContext.PackageRef.AddRangeAsync(packageRefs);
                await _stockDbContext.SaveChangesAsync();
                trans.Commit();
            }
            return GeneralCode.Success;
        }

        public async Task<ServiceResult<long>> JoinPackage(PackageJoinInput req)
        {
            if (req == null || req.FromPackageIds == null || req.FromPackageIds.Count == 0)
                return GeneralCode.InvalidParams;

            if (string.IsNullOrWhiteSpace(req.PackageCode))
            {
                return PackageErrorCode.PackageCodeEmpty;
            }

            var packageExisted = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageCode == req.PackageCode);
            if (packageExisted != null)
            {
                return PackageErrorCode.PackageAlreadyExisted;
            }

            var fromPackages = await _stockDbContext.Package.Where(p => req.FromPackageIds.Contains(p.PackageId)).ToListAsync();

            if (fromPackages
                .GroupBy(p => new { p.StockId, p.ProductId, p.PrimaryUnitId, p.ProductUnitConversionId })
                .Count() > 1)
            {
                return PackageErrorCode.PackagesToJoinMustBeSameProductAndUnit;
            }
            var defaultPackage = fromPackages.FirstOrDefault(p => p.PackageTypeId == (int)EnumPackageType.Default);
            if (defaultPackage != null)
            {
                return PackageErrorCode.CanNotJoinDefaultPackage;
            }

            foreach (var packageId in req.FromPackageIds)
            {
                var packageInfo = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageId == packageId);
                if (packageInfo == null) return PackageErrorCode.PackageNotFound;

                if (packageInfo.ProductUnitConversionWaitting > 0)
                {
                    return PackageErrorCode.HasSomeQualtityWaitingForApproved;
                }
            }

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var newPackage = new PackageModel()
                {
                    PackageCode = req.PackageCode,
                    LocationId = req.LocationId,
                    StockId = fromPackages[0].StockId,
                    ProductId = fromPackages[0].ProductId,
                    Date = fromPackages.Max(p => p.Date),
                    ExpiryTime = fromPackages.Min(p => p.ExpiryTime),
                    PrimaryUnitId = fromPackages[0].PrimaryUnitId,
                    PrimaryQuantity = fromPackages.Sum(p => p.PrimaryQuantityRemaining),
                    ProductUnitConversionId = fromPackages[0].ProductUnitConversionId,
                    ProductUnitConversionQuantity = fromPackages.Sum(p => p.ProductUnitConversionRemaining),
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    PrimaryQuantityWaiting = 0,
                    PrimaryQuantityRemaining = fromPackages.Sum(p => p.PrimaryQuantityRemaining),
                    ProductUnitConversionWaitting = 0,
                    ProductUnitConversionRemaining = fromPackages.Sum(p => p.ProductUnitConversionRemaining),
                    PackageTypeId = (int)EnumPackageType.Custom
                };
                await _stockDbContext.AddAsync(newPackage);
                await _stockDbContext.SaveChangesAsync();

                var packageRefs = new List<PackageRef>();
                foreach (var package in fromPackages)
                {
                    packageRefs.Add(new PackageRef()
                    {
                        PackageId = newPackage.PackageId,
                        RefPackageId = package.PackageId,
                        PrimaryUnitId = package.PrimaryUnitId,
                        PrimaryQuantity = package.PrimaryQuantityRemaining,
                        ProductUnitConversionId = package.ProductUnitConversionId,
                        ProductUnitConversionQuantity = package.ProductUnitConversionRemaining,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        PackageOperationTypeId = (int)EnumPackageOperationType.Join
                    });


                    package.PrimaryQuantity = 0;
                    package.PrimaryQuantityRemaining = 0;
                    package.ProductUnitConversionQuantity = 0;
                    package.ProductUnitConversionRemaining = 0;
                }

                await _stockDbContext.PackageRef.AddRangeAsync(packageRefs);
                await _stockDbContext.SaveChangesAsync();
                trans.Commit();
            }
            return GeneralCode.Success;
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
                    PackageTypeId = obj.PackageTypeId,
                    PackageCode = obj.PackageCode,
                    LocationId = obj.LocationId ?? 0,
                    StockId = obj.StockId,
                    ProductId = obj.ProductId,
                    Date = obj.Date,
                    ExpiryTime = obj.ExpiryTime,
                    PrimaryUnitId = obj.PrimaryUnitId,
                    PrimaryQuantity = obj.PrimaryQuantity,
                    ProductUnitConversionId = obj.ProductUnitConversionId,
                    ProductUnitConversionQuantity = obj.ProductUnitConversionQuantity,
                    PrimaryQuantityWaiting = obj.PrimaryQuantityWaiting,
                    PrimaryQuantityRemaining = obj.PrimaryQuantityRemaining,
                    ProductUnitConversionWaitting = obj.ProductUnitConversionWaitting,
                    ProductUnitConversionRemaining = obj.ProductUnitConversionRemaining,

                    CreatedDatetimeUtc = obj.CreatedDatetimeUtc,
                    UpdatedDatetimeUtc = obj.UpdatedDatetimeUtc,

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
                            PackageTypeId = item.Package.PackageTypeId,
                            PackageCode = item.Package.PackageCode,
                            LocationId = item.Package.LocationId ?? 0,
                            StockId = item.Package.StockId,
                            ProductId = item.Package.ProductId,
                            Date = item.Package.Date,
                            ExpiryTime = item.Package.ExpiryTime,

                            PrimaryUnitId = item.Package.PrimaryUnitId,
                            PrimaryQuantity = item.Package.PrimaryQuantity,
                            ProductUnitConversionId = item.Package.ProductUnitConversionId,
                            ProductUnitConversionQuantity = item.Package.ProductUnitConversionQuantity,
                            CreatedDatetimeUtc = item.Package.CreatedDatetimeUtc,
                            UpdatedDatetimeUtc = item.Package.UpdatedDatetimeUtc,
                            PrimaryQuantityWaiting = item.Package.PrimaryQuantityWaiting,
                            PrimaryQuantityRemaining = item.Package.PrimaryQuantityRemaining,
                            ProductUnitConversionWaitting = item.Package.ProductUnitConversionWaitting,
                            ProductUnitConversionRemaining = item.Package.ProductUnitConversionRemaining,
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
                            PackageTypeId = item.Package.PackageTypeId,
                            PackageCode = item.Package.PackageCode,
                            LocationId = item.Package.LocationId ?? 0,
                            StockId = item.Package.StockId,
                            ProductId = item.Package.ProductId,
                            Date = item.Package.Date,
                            ExpiryTime = item.Package.ExpiryTime,
                            ProductUnitConversionId = item.Package.ProductUnitConversionId,
                            PrimaryUnitId = item.Package.PrimaryUnitId,
                            PrimaryQuantity = item.Package.PrimaryQuantity,
                            ProductUnitConversionQuantity = item.Package.ProductUnitConversionQuantity,
                            CreatedDatetimeUtc = item.Package.CreatedDatetimeUtc,
                            UpdatedDatetimeUtc = item.Package.UpdatedDatetimeUtc,
                            PrimaryQuantityWaiting = item.Package.PrimaryQuantityWaiting,
                            PrimaryQuantityRemaining = item.Package.PrimaryQuantityRemaining,
                            ProductUnitConversionWaitting = item.Package.ProductUnitConversionWaitting,
                            ProductUnitConversionRemaining = item.Package.ProductUnitConversionRemaining,
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
