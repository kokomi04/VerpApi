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
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Stock.Model.Location;
using VErp.Services.Stock.Model.Package;
using Verp.Resources.Stock.Stock;
using PackageModel = VErp.Infrastructure.EF.StockDB.Package;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class PackageService : IPackageService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ObjectActivityLogFacade _packageActivityLog;


        public PackageService(StockDBContext stockContext
           , IOptions<AppSetting> appSetting
           , ILogger<PackageService> logger
           , IActivityLogService activityLogService
           , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _customGenCodeHelperService = customGenCodeHelperService;
            _packageActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Package); ;
        }

        public async Task<bool> UpdatePackage(long packageId, PackageInputModel req)
        {
            var obj = _stockDbContext.Package.FirstOrDefault(q => q.PackageId == packageId);
            if (obj == null)
                throw new BadRequestException(PackageErrorCode.PackageNotFound);

            var expiredDate = req.ExpiryTime > 0 ? req.ExpiryTime.UnixToDateTime() : DateTime.MinValue;

            obj.PackageCode = req.PackageCode;
            obj.LocationId = req.LocationId;
            obj.ExpiryTime = expiredDate == DateTime.MinValue ? null : (DateTime?)expiredDate;
            obj.Description = req.Description;
            obj.OrderCode = req.OrderCode;
            obj.Pocode = req.POCode;
            obj.ProductionOrderCode = req.ProductionOrderCode;

            obj.CustomPropertyValue = req.CustomPropertyValue?.JsonSerialize();

            await _stockDbContext.SaveChangesAsync();


            await _packageActivityLog.LogBuilder(() => PackageActivityLogMessage.Update)
              .MessageResourceFormatDatas(obj.PackageCode)
              .ObjectId(obj.PackageId)
              .JsonData(req.JsonSerialize())
              .CreateLog();

            return true;
        }

        public async Task<bool> SplitPackage(long packageId, PackageSplitInput req)
        {
            if (req == null || req.ToPackages == null || req.ToPackages.Count == 0 || req.ToPackages.Any(p => p.ProductUnitConversionQuantity <= 0))
                throw new BadRequestException(GeneralCode.InvalidParams);

            var origin = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageId == packageId);
            if (origin == null)
                throw new BadRequestException(PackageErrorCode.PackageNotFound);

            if (origin.ProductUnitConversionWaitting > 0 || origin.PrimaryQuantityWaiting > 0)
                throw new BadRequestException(PackageErrorCode.HasSomeQualtityWaitingForApproved);

            ProductUnitConversion unitConversionInfo = await _stockDbContext.ProductUnitConversion.FirstOrDefaultAsync(c => c.ProductUnitConversionId == origin.ProductUnitConversionId);

            if (unitConversionInfo == null)
                throw new BadRequestException(ProductUnitConversionErrorCode.ProductUnitConversionNotFound);

            var defaulUnitConversionInfo = await _stockDbContext.ProductUnitConversion.FirstOrDefaultAsync(c => c.ProductId == origin.ProductId && c.IsDefault);


            var totalSecondaryInput = req.ToPackages.Sum(p => p.ProductUnitConversionQuantity);
            if (totalSecondaryInput > origin.ProductUnitConversionRemaining)
                throw new BadRequestException(PackageErrorCode.QualtityOfProductInPackageNotEnough);

            var newPackages = new List<PackageModel>();
            var baseValueChains = new Dictionary<string, int>();
            var genCodeContexts = new List<GenerateCodeContext>();
            foreach (var package in req.ToPackages)
            {
                genCodeContexts.Add(await GeneratePackageCode(package, baseValueChains));

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
                    var calcModel = new QuantityPairInputModel()
                    {
                        PrimaryQuantity = package.PrimaryQuantity,
                        PrimaryDecimalPlace = defaulUnitConversionInfo?.DecimalPlace ?? 12,

                        PuQuantity = package.ProductUnitConversionQuantity,
                        PuDecimalPlace = unitConversionInfo.DecimalPlace,

                        FactorExpression = unitConversionInfo.FactorExpression,

                        FactorExpressionRate = origin.ProductUnitConversionRemaining / origin.PrimaryQuantityRemaining
                    };

                    //var (isSuccess, priQuantity) = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(package.ProductUnitConversionQuantity, packageInfo.ProductUnitConversionRemaining / packageInfo.PrimaryQuantityRemaining, package.PrimaryQuantity, defaulUnitConversionInfo?.DecimalPlace ?? 11);

                    var (isSuccess, priQuantity, puQuantity) = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(calcModel);

                    if (isSuccess)
                    {
                        qualtityInPrimaryUnit = priQuantity;
                    }
                    else
                    {
                        _logger.LogWarning($"Wrong priQuantity input data: PrimaryQuantity={package.PrimaryQuantity}, FactorExpression={origin.ProductUnitConversionRemaining / origin.PrimaryQuantityRemaining}, ProductUnitConversionQuantity={package.ProductUnitConversionQuantity}, evalData={priQuantity}");
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
                    StockId = origin.StockId,
                    ProductId = origin.ProductId,
                    Date = origin.Date,
                    ExpiryTime = origin.ExpiryTime,
                    Description = origin.Description,
                    ProductUnitConversionId = origin.ProductUnitConversionId,
                    PrimaryQuantityWaiting = 0,
                    PrimaryQuantityRemaining = qualtityInPrimaryUnit,
                    ProductUnitConversionWaitting = 0,
                    ProductUnitConversionRemaining = package.ProductUnitConversionQuantity,
                    PackageTypeId = (int)EnumPackageType.Custom,
                    CustomPropertyValue = origin.CustomPropertyValue
                });

                origin.PrimaryQuantityRemaining = origin.PrimaryQuantityRemaining.SubDecimal(qualtityInPrimaryUnit);
                origin.ProductUnitConversionRemaining = origin.ProductUnitConversionRemaining.SubDecimal(package.ProductUnitConversionQuantity);

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
                        PackageOperationTypeId = (int)EnumPackageOperationType.Split
                    });
                }

                await _stockDbContext.PackageRef.AddRangeAsync(packageRefs);
                await _stockDbContext.SaveChangesAsync();

                trans.Commit();


                await _packageActivityLog.LogBuilder(() => PackageActivityLogMessage.Split)
                  .MessageResourceFormatDatas(origin.PackageCode, string.Join(", ", req.ToPackages.Select(p => p.PackageCode)))
                  .ObjectId(origin.PackageId)
                  .JsonData(new { req, packageRefs }.JsonSerialize())
                  .CreateLog();

                foreach (var newPackage in newPackages)
                {
                    await _packageActivityLog.LogBuilder(() => PackageActivityLogMessage.Split)
                      .MessageResourceFormatDatas(origin.PackageCode, string.Join(", ", req.ToPackages.Select(p => p.PackageCode)))
                      .ObjectId(newPackage.PackageId)
                      .JsonData(new { req, packageRefs, newPackage }.JsonSerialize())
                      .CreateLog();
                }
            }

            foreach (var item in genCodeContexts)
            {
                await item.ConfirmCode();
            }
            return true;
        }

        public async Task<long> JoinPackage(PackageJoinInput req)
        {
            if (req == null || req.FromPackageIds == null || req.FromPackageIds.Count == 0)
                throw new BadRequestException(GeneralCode.InvalidParams);

            var ctx = await GeneratePackageCode(req, null);

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

            var customValues = fromPackages[0].CustomPropertyValue?.JsonDeserialize<Dictionary<int, object>>();
            if (customValues == null) customValues = new Dictionary<int, object>();

            for (var i = 1; i < fromPackages.Count; i++)
            {
                var fromCustomValues = fromPackages[1].CustomPropertyValue?.JsonDeserialize<Dictionary<int, object>>();
                if (fromCustomValues != null)
                {
                    foreach (var v in fromCustomValues)
                    {
                        var propId = v.Key;
                        var existedValue = customValues.ContainsKey(propId) ? customValues[propId] : null;
                        if (v.Value != null && !v.Value.IsNullObject())
                        {
                            if (!customValues.ContainsKey(propId))
                            {
                                customValues.Add(propId, v.Value);
                            }
                            else
                            {
                                if (existedValue?.ToString() != v.Value?.ToString())
                                {
                                    customValues.Remove(propId);
                                }
                            }
                        }
                    }
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
                    PrimaryQuantityWaiting = 0,
                    PrimaryQuantityRemaining = fromPackages.Sum(p => p.PrimaryQuantityRemaining),
                    ProductUnitConversionWaitting = 0,
                    ProductUnitConversionRemaining = fromPackages.Sum(p => p.ProductUnitConversionRemaining),
                    PackageTypeId = (int)EnumPackageType.Custom,
                    CustomPropertyValue = customValues?.JsonSerialize()
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
                        PackageOperationTypeId = (int)EnumPackageOperationType.Join
                    });


                    package.PrimaryQuantityRemaining = 0;
                    package.ProductUnitConversionRemaining = 0;
                }

                await _stockDbContext.PackageRef.AddRangeAsync(packageRefs);
                await _stockDbContext.SaveChangesAsync();
                trans.Commit();


                await ctx.ConfirmCode();

                await _packageActivityLog.LogBuilder(() => PackageActivityLogMessage.Join)
                 .MessageResourceFormatDatas(string.Join(", ", fromPackages.Select(p => p.PackageCode)), newPackage.PackageCode)
                 .ObjectId(newPackage.PackageId)
                 .JsonData(new { req, packageRefs }.JsonSerialize())
                 .CreateLog();

                foreach (var oldPackage in fromPackages)
                {
                    await _packageActivityLog.LogBuilder(() => PackageActivityLogMessage.Join)
                      .MessageResourceFormatDatas(string.Join(", ", fromPackages.Select(p => p.PackageCode)), newPackage.PackageCode)
                      .ObjectId(oldPackage.PackageId)
                      .JsonData(new { req, packageRefs, oldPackage }.JsonSerialize())
                      .CreateLog();
                }

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
                OrderCode = obj.OrderCode,
                POCode = obj.Pocode,
                ProductionOrderCode = obj.ProductionOrderCode,
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
                CustomPropertyValue = obj.CustomPropertyValue?.JsonDeserialize<Dictionary<int, object>>(),
                LocationOutputModel = locationOutputModel
            };
            return packageOutputModel;

        }

        public async Task<PageData<PackageOutputModel>> GetList(int stockId = 0, string keyword = "", int page = 1, int size = 10)
        {
            keyword = (keyword ?? "").Trim();

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
                    PrimaryQuantityWaiting = item.Package.PrimaryQuantityWaiting,
                    PrimaryQuantityRemaining = item.Package.PrimaryQuantityRemaining,
                    ProductUnitConversionWaitting = item.Package.ProductUnitConversionWaitting,
                    ProductUnitConversionRemaining = item.Package.ProductUnitConversionRemaining,
                    LocationOutputModel = locationOutputModel,

                    POCode = item.Package.Pocode,
                    ProductionOrderCode = item.Package.ProductionOrderCode,
                    OrderCode = item.Package.OrderCode,

                    CustomPropertyValue = item.Package.CustomPropertyValue?.JsonDeserialize<Dictionary<int, object>>(),
                };
                resultList.Add(model);
            }

            return (resultList, totalRecord);

        }

        private async Task<GenerateCodeContext> GeneratePackageCode(INewPackageBase pageAge, Dictionary<string, int> baseValueChains)
        {
            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);

            var code = await ctx
                .SetConfig(EnumObjectType.Package)
                .SetConfigData(0)
                .TryValidateAndGenerateCode(_stockDbContext.Package, pageAge.PackageCode, (s, code) => s.PackageCode == code);

            pageAge.PackageCode = code;
            return ctx;
        }



        public async Task<PageData<ProductPackageOutputModel>> GetProductPackageListForExport(string keyword, bool? isTwoUnit, IList<int> productCateIds, IList<int> productIds, IList<long> productUnitConversionIds, IList<long> packageIds, IList<int> stockIds, int page = 1, int size = 20)
        {
            var packpages = _stockDbContext.Package.AsQueryable();
            if (packageIds?.Count > 0)
            {
                packpages = packpages.Where(p => packageIds.Contains(p.PackageId));
            }

            if (productIds?.Count > 0)
            {
                packpages = packpages.Where(p => productIds.Contains(p.ProductId));
            }



            if (stockIds?.Count > 0)
            {
                packpages = packpages.Where(p => stockIds.Contains(p.StockId));
            }

            if (productUnitConversionIds?.Count > 0)
            {
                packpages = packpages.Where(p => productUnitConversionIds.Contains(p.ProductUnitConversionId));
            }

            var productQuery = _stockDbContext.Product.AsQueryable();
            if (productCateIds?.Count > 0)
            {
                productQuery = productQuery.Where(p => productCateIds.Contains(p.ProductCateId));

            }
            var query = from pk in packpages
                        join l in _stockDbContext.Location on pk.LocationId equals l.LocationId into ls
                        from l in ls.DefaultIfEmpty()
                        join p in productQuery on pk.ProductId equals p.ProductId
                        join s in _stockDbContext.ProductExtraInfo on p.ProductId equals s.ProductId
                        join pu in _stockDbContext.ProductUnitConversion on pk.ProductUnitConversionId equals pu.ProductUnitConversionId
                        where //stockIds.Contains(pk.StockId) &&
                        pk.PrimaryQuantityRemaining > 0
                        select new
                        {
                            ProductId = p.ProductId,
                            ProductCode = p.ProductCode,
                            ProductName = p.ProductName,
                            Specification = s.Specification,
                            MainImageFileId = p.MainImageFileId,
                            UnitId = p.UnitId,
                            PackageId = pk.PackageId,

                            PackageTypeId = pk.PackageTypeId,

                            PackageCode = pk.PackageCode,
                            PackageDescription = pk.Description,

                            LocationId = pk.LocationId,
                            LocationName = l == null ? null : l.Name,

                            StockId = pk.StockId,

                            Date = pk.Date,
                            ExpiryTime = pk.ExpiryTime,

                            ProductUnitConversionIsDefault = pu.IsDefault,

                            ProductUnitConversionId = pu.ProductUnitConversionId,
                            ProductUnitConversionname = pu.ProductUnitConversionName,

                            PrimaryQuantityWaiting = pk.PrimaryQuantityWaiting,
                            PrimaryQuantityRemaining = pk.PrimaryQuantityRemaining,

                            ProductUnitConversionWaitting = pk.ProductUnitConversionWaitting,
                            ProductUnitConversionRemaining = pk.ProductUnitConversionRemaining,

                            POCode = pk.Pocode,
                            pk.OrderCode,
                            ProductionOrderCode = pk.ProductionOrderCode,

                            pk.CustomPropertyValue
                        };
            if (isTwoUnit.HasValue)
            {
                query = query.Where(p => p.ProductUnitConversionIsDefault == !isTwoUnit.Value);
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.PackageCode.Contains(keyword)
                 || p.ProductCode.Contains(keyword)
                 || p.ProductName.Contains(keyword)
                 || p.POCode.Contains(keyword)
                 || p.ProductionOrderCode.Contains(keyword)
                 || p.LocationName.Contains(keyword)
                );
            }
            var total = await query.CountAsync();

            var packageData = size > 0 ? await query.OrderByDescending(p => p.ProductUnitConversionRemaining).AsNoTracking().Skip((page - 1) * size).Take(size).ToListAsync() : await query.AsNoTracking().ToListAsync();


            var packageList = new List<ProductPackageOutputModel>(total);
            var dataProductIds = packageData.Select(d => d.ProductId).ToList();
            var pusDefaults = await _stockDbContext.ProductUnitConversion.Where(p => dataProductIds.Contains(p.ProductId) && p.IsDefault).ToListAsync();
            foreach (var item in packageData)
            {
                packageList.Add(new ProductPackageOutputModel()
                {
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    Specification = item.Specification,
                    MainImageFileId = item.MainImageFileId,
                    UnitId = item.UnitId,
                    UnitName = pusDefaults.FirstOrDefault(d => d.ProductId == item.ProductId)?.ProductUnitConversionName,

                    PackageId = item.PackageId,

                    PackageTypeId = item.PackageTypeId,

                    PackageCode = item.PackageCode,
                    PackageDescription = item.PackageDescription,

                    LocationId = item.LocationId,
                    LocationName = item.LocationName,
                    StockId = item.StockId,

                    Date = item.Date?.GetUnix(),
                    ExpiryTime = item.ExpiryTime?.GetUnix(),

                    ProductUnitConversionIsDefault = item.ProductUnitConversionIsDefault,

                    ProductUnitConversionId = item.ProductUnitConversionId,
                    ProductUnitConversionName = item.ProductUnitConversionname,

                    PrimaryQuantityWaiting = item.PrimaryQuantityWaiting,
                    PrimaryQuantityRemaining = item.PrimaryQuantityRemaining,

                    ProductUnitConversionWaitting = item.ProductUnitConversionWaitting,
                    ProductUnitConversionRemaining = item.ProductUnitConversionRemaining,

                    POCode = item.POCode,
                    OrderCode = item.OrderCode,
                    ProductionOrderCode = item.ProductionOrderCode,

                    CustomPropertyValue = item.CustomPropertyValue?.JsonDeserialize<Dictionary<int, object>>(),
                });

            }
            return (packageList, total);

        }


    }
}
