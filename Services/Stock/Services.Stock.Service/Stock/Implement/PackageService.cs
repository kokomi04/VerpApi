﻿using Microsoft.EntityFrameworkCore;
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
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
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
        private readonly IActivityLogService _activityLogService;

        public PackageService(StockDBContext stockContext
           , IOptions<AppSetting> appSetting
           , ILogger<PackageService> logger
           , IActivityLogService activityLogService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<bool> UpdatePackage(long packageId, PackageInputModel req)
        {

            var obj = _stockDbContext.Package.FirstOrDefault(q => q.PackageId == packageId);

            if (obj == null)
            {
                throw new BadRequestException(PackageErrorCode.PackageNotFound);
            }

            //var expiredDate = DateTime.MinValue;

            //if (!string.IsNullOrEmpty(req.ExpiryTime))
            //    DateTime.TryParseExact(req.ExpiryTime, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out expiredDate);
            var expiredDate = req.ExpiryTime > 0 ? req.ExpiryTime.UnixToDateTime() : DateTime.MinValue;


            obj.PackageCode = req.PackageCode;
            obj.LocationId = req.LocationId;
            obj.ExpiryTime = expiredDate == DateTime.MinValue ? null : (DateTime?)expiredDate;
            obj.UpdatedDatetimeUtc = DateTime.UtcNow;
            obj.Description = req.Description;

            await _stockDbContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Package, obj.PackageId, $"Cập nhật thông tin kiện {obj.PackageCode} ", req.JsonSerialize());

            return true;

        }



        public async Task<bool> SplitPackage(long packageId, PackageSplitInput req)
        {
            if (req == null || req.ToPackages == null || req.ToPackages.Count == 0 || req.ToPackages.Any(p => p.ProductUnitConversionQuantity <= 0))
                throw new BadRequestException(GeneralCode.InvalidParams);

            var packageInfo = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageId == packageId);
            if (packageInfo == null) throw new BadRequestException(PackageErrorCode.PackageNotFound);

            if (packageInfo.ProductUnitConversionWaitting > 0 || packageInfo.PrimaryQuantityWaiting > 0)
            {
                throw new BadRequestException(PackageErrorCode.HasSomeQualtityWaitingForApproved);
            }

            ProductUnitConversion unitConversionInfo = null;

            unitConversionInfo = await _stockDbContext.ProductUnitConversion.FirstOrDefaultAsync(c => c.ProductUnitConversionId == packageInfo.ProductUnitConversionId);

            if (unitConversionInfo == null) throw new BadRequestException(ProductUnitConversionErrorCode.ProductUnitConversionNotFound);


            var totalSecondaryInput = req.ToPackages.Sum(p => p.ProductUnitConversionQuantity);
            if (totalSecondaryInput > packageInfo.ProductUnitConversionRemaining) throw new BadRequestException(PackageErrorCode.QualtityOfProductInPackageNotEnough);

            var newPackages = new List<PackageModel>();


            foreach (var package in req.ToPackages)
            {
                if (string.IsNullOrWhiteSpace(package.PackageCode))
                {
                    throw new BadRequestException(PackageErrorCode.PackageCodeEmpty);
                }

                var packageExisted = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageCode == package.PackageCode);
                if (packageExisted != null)
                {
                    throw new BadRequestException(PackageErrorCode.PackageAlreadyExisted);
                }

                decimal qualtityInPrimaryUnit = package.PrimaryQuantity;
                if (unitConversionInfo.IsFreeStyle == false)
                {
                    //qualtityInPrimaryUnit = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(package.ProductUnitConversionQuantity, unitConversionInfo.FactorExpression);

                    var (isSuccess, priQuantity) = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(package.ProductUnitConversionQuantity, packageInfo.ProductUnitConversionRemaining / packageInfo.PrimaryQuantityRemaining, package.PrimaryQuantity);
                    if (isSuccess)
                    {
                        qualtityInPrimaryUnit = priQuantity;
                    }
                    else
                    {
                        _logger.LogWarning($"Wrong priQuantity input data: PrimaryQuantity={package.PrimaryQuantity}, FactorExpression={packageInfo.ProductUnitConversionRemaining / packageInfo.PrimaryQuantityRemaining}, ProductUnitConversionQuantity={package.ProductUnitConversionQuantity}, evalData={priQuantity}");
                        throw new BadRequestException(ProductUnitConversionErrorCode.SecondaryUnitConversionError);
                    }

                    if (!(qualtityInPrimaryUnit > 0))
                    {
                        throw new BadRequestException(ProductUnitConversionErrorCode.SecondaryUnitConversionError);
                    }
                }

                if (qualtityInPrimaryUnit <= 0 || package.ProductUnitConversionQuantity <= 0)
                    throw new BadRequestException(GeneralCode.InvalidParams);


                newPackages.Add(new PackageModel()
                {
                    PackageCode = package.PackageCode,
                    LocationId = package.LocationId,
                    StockId = packageInfo.StockId,
                    ProductId = packageInfo.ProductId,
                    Date = packageInfo.Date,
                    ExpiryTime = packageInfo.ExpiryTime,
                    Description = packageInfo.Description,
                    ProductUnitConversionId = packageInfo.ProductUnitConversionId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    PrimaryQuantityWaiting = 0,
                    PrimaryQuantityRemaining = qualtityInPrimaryUnit,
                    ProductUnitConversionWaitting = 0,
                    ProductUnitConversionRemaining = package.ProductUnitConversionQuantity,
                    PackageTypeId = (int)EnumPackageType.Custom
                });

                packageInfo.PrimaryQuantityRemaining = packageInfo.PrimaryQuantityRemaining.SubDecimal(qualtityInPrimaryUnit);
                packageInfo.ProductUnitConversionRemaining = packageInfo.ProductUnitConversionRemaining.SubDecimal(package.ProductUnitConversionQuantity);

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
            return true;
        }

        public async Task<long> JoinPackage(PackageJoinInput req)
        {
            if (req == null || req.FromPackageIds == null || req.FromPackageIds.Count == 0)
                throw new BadRequestException(GeneralCode.InvalidParams);

            if (string.IsNullOrWhiteSpace(req.PackageCode))
            {
                throw new BadRequestException(PackageErrorCode.PackageCodeEmpty);
            }

            var packageExisted = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageCode == req.PackageCode);
            if (packageExisted != null)
            {
                throw new BadRequestException(PackageErrorCode.PackageAlreadyExisted);
            }

            var fromPackages = await _stockDbContext.Package.Where(p => req.FromPackageIds.Contains(p.PackageId)).ToListAsync();

            if (fromPackages
                .GroupBy(p => new { p.StockId, p.ProductId, p.ProductUnitConversionId })
                .Count() > 1)
            {
                throw new BadRequestException(PackageErrorCode.PackagesToJoinMustBeSameProductAndUnit);
            }

            var defaultPackage = fromPackages.FirstOrDefault(p => p.PackageTypeId == (int)EnumPackageType.Default);
            if (defaultPackage != null)
            {
                throw new BadRequestException(PackageErrorCode.CanNotJoinDefaultPackage);
            }

            foreach (var packageId in req.FromPackageIds)
            {
                var packageInfo = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageId == packageId);
                if (packageInfo == null) throw new BadRequestException(PackageErrorCode.PackageNotFound);

                if (packageInfo.ProductUnitConversionWaitting > 0 || packageInfo.PrimaryQuantityWaiting > 0)
                {
                    throw new BadRequestException(PackageErrorCode.HasSomeQualtityWaitingForApproved);
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
                    Description = string.Join(", ", fromPackages.Select(f => f.Description)),
                    ProductUnitConversionId = fromPackages[0].ProductUnitConversionId,
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
                        PrimaryQuantity = package.PrimaryQuantityRemaining,
                        ProductUnitConversionId = package.ProductUnitConversionId,
                        ProductUnitConversionQuantity = package.ProductUnitConversionRemaining,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        PackageOperationTypeId = (int)EnumPackageOperationType.Join
                    });


                    package.PrimaryQuantityRemaining = 0;
                    package.ProductUnitConversionRemaining = 0;
                }

                await _stockDbContext.PackageRef.AddRangeAsync(packageRefs);
                await _stockDbContext.SaveChangesAsync();
                trans.Commit();
                return newPackage.PackageId;
            }
        }

        public async Task<PackageOutputModel> GetInfo(long packageId)
        {

            var obj = await _stockDbContext.Package.FirstOrDefaultAsync(q => q.PackageId == packageId);

            if (obj == null)
            {
                throw new BadRequestException(PackageErrorCode.PackageNotFound);
            }

            var productInfo = await _stockDbContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == obj.ProductId);

            var locationObj = await _stockDbContext.Location.FirstOrDefaultAsync(q => q.LocationId == obj.LocationId);
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
                Date = obj.Date != null ? ((DateTime)obj.Date).GetUnix() : 0,
                ExpiryTime = obj.ExpiryTime != null ? ((DateTime)obj.ExpiryTime).GetUnix() : 0,
                Description = obj.Description,
                PrimaryUnitId = productInfo.UnitId,
                ProductUnitConversionId = obj.ProductUnitConversionId,
                PrimaryQuantityWaiting = obj.PrimaryQuantityWaiting,
                PrimaryQuantityRemaining = obj.PrimaryQuantityRemaining,
                ProductUnitConversionWaitting = obj.ProductUnitConversionWaitting,
                ProductUnitConversionRemaining = obj.ProductUnitConversionRemaining,

                CreatedDatetimeUtc = obj.CreatedDatetimeUtc != null ? ((DateTime)obj.CreatedDatetimeUtc).GetUnix() : 0,
                UpdatedDatetimeUtc = obj.UpdatedDatetimeUtc != null ? ((DateTime)obj.UpdatedDatetimeUtc).GetUnix() : 0,

                LocationOutputModel = locationOutputModel
            };
            return packageOutputModel;

        }

        public async Task<PageData<PackageOutputModel>> GetList(int stockId = 0, string keyword = "", int page = 1, int size = 10)
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


            var dataFromDb = page > 0 && size > 0 ?
                query.AsNoTracking().Skip((page - 1) * size).Take(size).Select(q => new { Package = q.p, Location = q.lo }).ToList()
                : query.AsNoTracking().Select(q => new { Package = q.p, Location = q.lo }).ToList();

            var productIds = dataFromDb.Select(d => d.Package.ProductId).Distinct().ToList();

            var productUnitInfos = (
                await _stockDbContext.Product
                .Where(p => productIds.Contains(p.ProductId))
                .AsNoTracking()
                .Select(p => new { p.ProductId, p.UnitId })
                .ToListAsync()
                ).ToDictionary(p => p.ProductId, p => p);


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
                    Date = item.Package.Date != null ? ((DateTime)item.Package.Date).GetUnix() : 0,
                    ExpiryTime = item.Package.ExpiryTime != null ? ((DateTime)item.Package.ExpiryTime).GetUnix() : 0,
                    Description = item.Package.Description,

                    PrimaryUnitId = productUnitInfos[item.Package.ProductId].UnitId,
                    ProductUnitConversionId = item.Package.ProductUnitConversionId,
                    CreatedDatetimeUtc = item.Package.CreatedDatetimeUtc != null ? ((DateTime)item.Package.CreatedDatetimeUtc).GetUnix() : 0,
                    UpdatedDatetimeUtc = item.Package.UpdatedDatetimeUtc != null ? ((DateTime)item.Package.UpdatedDatetimeUtc).GetUnix() : 0,
                    PrimaryQuantityWaiting = item.Package.PrimaryQuantityWaiting,
                    PrimaryQuantityRemaining = item.Package.PrimaryQuantityRemaining,
                    ProductUnitConversionWaitting = item.Package.ProductUnitConversionWaitting,
                    ProductUnitConversionRemaining = item.Package.ProductUnitConversionRemaining,
                    LocationOutputModel = locationOutputModel
                };
                resultList.Add(model);
            }

            return (resultList, totalRecord);

        }

    }
}
