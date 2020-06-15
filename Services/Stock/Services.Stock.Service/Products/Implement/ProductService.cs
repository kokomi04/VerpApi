using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.FileResources;
using VErp.Infrastructure.EF.EFExtensions;
using static VErp.Services.Stock.Model.Product.ProductModel;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductService : IProductService
    {
        private readonly StockDBContext _stockContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IActivityLogService _activityLogService;
        private readonly IFileService _fileService;
        private readonly IAsyncRunnerService _asyncRunner;

        public ProductService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductService> logger
            , IUnitService unitService
            , IActivityLogService activityLogService
            , IFileService fileService
            , IAsyncRunnerService asyncRunner
            )
        {
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _unitService = unitService;
            _activityLogService = activityLogService;
            _fileService = fileService;
            _asyncRunner = asyncRunner;
        }

        public async Task<ServiceResult<int>> AddProduct(ProductModel req)
        {
            req.ProductCode = (req.ProductCode ?? "").Trim();
            Enum validate;
            if (!(validate = ValidateProduct(req)).IsSuccess())
            {
                return validate;
            }

            var productExisted = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductCode == req.ProductCode || p.ProductName == req.ProductName);
            if (productExisted != null)
            {
                if (string.Compare(productExisted.ProductCode, req.ProductCode, StringComparison.OrdinalIgnoreCase) == 0)
                    return ProductErrorCode.ProductCodeAlreadyExisted;
                return ProductErrorCode.ProductNameAlreadyExisted;
            }

            if (!await _stockContext.ProductCate.AnyAsync(c => c.ProductCateId == req.ProductCateId))
            {
                return ProductErrorCode.ProductCateInvalid;
            }

            if (!await _stockContext.ProductType.AnyAsync(c => c.ProductTypeId == req.ProductTypeId))
            {
                return ProductErrorCode.ProductTypeInvalid;
            }

            int productId = 0;
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var productInfo = new Product()
                    {
                        ProductCode = req.ProductCode,
                        ProductName = req.ProductName,
                        IsCanBuy = req.IsCanBuy,
                        IsCanSell = req.IsCanSell,
                        MainImageFileId = req.MainImageFileId,
                        ProductTypeId = req.ProductTypeId,
                        ProductCateId = req.ProductCateId,
                        BarcodeConfigId = req.BarcodeConfigId,
                        BarcodeStandardId = (int?)req.BarcodeStandardId,
                        Barcode = req.Barcode,
                        UnitId = req.UnitId,
                        EstimatePrice = req.EstimatePrice,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _stockContext.AddAsync(productInfo);

                    await _stockContext.SaveChangesAsync();

                    var productExtra = new ProductExtraInfo()
                    {
                        ProductId = productInfo.ProductId,
                        Specification = req.Extra?.Specification,
                        Description = req.Extra?.Description,
                        IsDeleted = false
                    };

                    await _stockContext.AddAsync(productExtra);

                    var productStockInfo = new ProductStockInfo()
                    {
                        ProductId = productInfo.ProductId,
                        StockOutputRuleId = (int?)req.StockInfo?.StockOutputRuleId,
                        AmountWarningMin = req.StockInfo?.AmountWarningMin,
                        AmountWarningMax = req.StockInfo?.AmountWarningMax,
                        TimeWarningTimeTypeId = (int?)req.StockInfo?.TimeWarningTimeTypeId,
                        TimeWarningAmount = req.StockInfo?.TimeWarningAmount,
                        DescriptionToStock = req.StockInfo?.DescriptionToStock,
                        ExpireTimeTypeId = (int?)req.StockInfo?.ExpireTimeTypeId,
                        ExpireTimeAmount = req.StockInfo?.ExpireTimeAmount,
                        IsDeleted = false
                    };

                    await _stockContext.AddAsync(productStockInfo);

                    var lstStockValidations = req.StockInfo?.StockIds?.Select(s => new ProductStockValidation()
                    {
                        ProductId = productInfo.ProductId,
                        StockId = s
                    });

                    if (lstStockValidations != null)
                    {
                        await _stockContext.AddRangeAsync(lstStockValidations);
                    }

                    var unitInfo = await _unitService.GetUnitInfo(req.UnitId);
                    if (unitInfo == null)
                    {
                        return UnitErrorCode.UnitNotFound;
                    }

                    var lstUnitConverions = req.StockInfo?.UnitConversions?.Select(u => new Infrastructure.EF.StockDB.ProductUnitConversion()
                    {
                        ProductId = productInfo.ProductId,
                        ProductUnitConversionName = u.ProductUnitConversionName,
                        SecondaryUnitId = u.SecondaryUnitId,
                        FactorExpression = u.FactorExpression,
                        ConversionDescription = u.ConversionDescription,
                        IsDefault = false,
                    })
                    .ToList();

                    if (lstUnitConverions == null)
                    {
                        lstUnitConverions = new List<ProductUnitConversion>();
                    }

                    lstUnitConverions.Add(
                        new ProductUnitConversion()
                        {
                            ProductId = productInfo.ProductId,
                            ProductUnitConversionName = unitInfo.Data.UnitName,
                            SecondaryUnitId = req.UnitId,
                            FactorExpression = "1",
                            ConversionDescription = "Mặc định",
                            IsDefault = true
                        }
                    );

                    if (lstUnitConverions != null)
                    {
                        await _stockContext.AddRangeAsync(lstUnitConverions);
                    }

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();


                    await _activityLogService.CreateLog(EnumObjectType.Product, productInfo.ProductId, $"Thêm mới sản phẩm {productInfo.ProductName}", req.JsonSerialize());

                    productId = productInfo.ProductId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "AddProduct");
                    return GeneralCode.InternalError;
                }
            }

            if (req.MainImageFileId.HasValue)
            {
                _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.Product, productId, req.MainImageFileId.Value));
            }

            return productId;
        }


        public async Task<ServiceResult<ProductModel>> ProductInfo(int productId)
        {
            var productInfo = await _stockContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productInfo == null)
            {
                return ProductErrorCode.ProductNotFound;
            }
            var productExtra = await _stockContext.ProductExtraInfo.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            var productStockInfo = await _stockContext.ProductStockInfo.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            var stockValidations = await _stockContext.ProductStockValidation.AsNoTracking().Where(p => p.ProductId == productId).ToListAsync();
            var unitConverions = await _stockContext.ProductUnitConversion.AsNoTracking().Where(p => !p.IsDefault && p.ProductId == productId).ToListAsync();

            return new ProductModel()
            {
                ProductCode = productInfo.ProductCode,
                ProductName = productInfo.ProductName,
                IsCanBuy = productInfo.IsCanBuy,
                IsCanSell = productInfo.IsCanSell,
                MainImageFileId = productInfo.MainImageFileId,
                ProductTypeId = productInfo.ProductTypeId,
                ProductCateId = productInfo.ProductCateId,
                BarcodeConfigId = productInfo.BarcodeConfigId,
                BarcodeStandardId = (EnumBarcodeStandard?)productInfo.BarcodeStandardId,
                Barcode = productInfo.Barcode,
                UnitId = productInfo.UnitId,
                EstimatePrice = productInfo.EstimatePrice,

                Extra = productExtra != null ? new ProductModelExtra()
                {
                    Specification = productExtra.Specification,
                    Description = productExtra.Description
                } : null,
                StockInfo = productStockInfo != null ? new ProductModelStock()
                {
                    StockOutputRuleId = (EnumStockOutputRule?)productStockInfo.StockOutputRuleId,
                    AmountWarningMin = productStockInfo.AmountWarningMin,
                    AmountWarningMax = productStockInfo.AmountWarningMax,
                    TimeWarningTimeTypeId = (EnumTimeType?)productStockInfo.TimeWarningTimeTypeId,
                    TimeWarningAmount = productStockInfo.TimeWarningAmount,
                    ExpireTimeTypeId = (EnumTimeType?)productStockInfo.ExpireTimeTypeId,
                    ExpireTimeAmount = productStockInfo.ExpireTimeAmount,
                    DescriptionToStock = productStockInfo.DescriptionToStock,
                    StockIds = stockValidations?.Select(s => s.StockId).ToList(),
                    UnitConversions = unitConverions?.Select(c => new ProductModelUnitConversion()
                    {
                        ProductUnitConversionId = c.ProductUnitConversionId,
                        ProductUnitConversionName = c.ProductUnitConversionName,
                        SecondaryUnitId = c.SecondaryUnitId,
                        FactorExpression = c.FactorExpression,
                        ConversionDescription = c.ConversionDescription
                    }).ToList()
                } : null
            };
        }


      
        public async Task<Enum> UpdateProduct(int productId, ProductModel req)
        {
            Enum validate;
            if (!(validate = ValidateProduct(req)).IsSuccess())
            {
                return validate;
            }

            req.ProductCode = (req.ProductCode ?? "").Trim();

            var productExisted = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId != productId && (p.ProductCode == req.ProductCode || p.ProductName == req.ProductName));
            if (productExisted != null)
            {
                if (productExisted.ProductCode == req.ProductCode)
                    return ProductErrorCode.ProductCodeAlreadyExisted;
                return ProductErrorCode.ProductNameAlreadyExisted;
            }


            long? oldMainImageFileId = 0L;

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //Getdata
                    var productInfo = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                    if (productInfo == null)
                    {
                        return ProductErrorCode.ProductNotFound;
                    }



                    var unitConverions = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

                    var keepIds = req.StockInfo?.UnitConversions.Select(c => c.ProductUnitConversionId);
                    var toRemoveUnitConversions = unitConverions.Where(c => !keepIds.Contains(c.ProductUnitConversionId) && !c.IsDefault).ToList();
                    if (toRemoveUnitConversions.Count > 0)
                    {
                        var removeConversionIds = toRemoveUnitConversions.Select(c => (int?)c.ProductUnitConversionId).ToList();


                        var usedUnitConvertion = _stockContext.InventoryDetail.FirstOrDefaultAsync(d => removeConversionIds.Contains(d.ProductUnitConversionId));
                        if (usedUnitConvertion != null)
                        {
                            trans.Rollback();
                            return ProductErrorCode.SomeProductUnitConversionInUsed;
                        }

                        _stockContext.RemoveRange(toRemoveUnitConversions);

                    }

                    oldMainImageFileId = productInfo.MainImageFileId;

                    var productExtra = await _stockContext.ProductExtraInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                    var productStockInfo = await _stockContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                    var stockValidations = await _stockContext.ProductStockValidation.Where(p => p.ProductId == productId).ToListAsync();



                    //Update

                    //Productinfo
                    productInfo.ProductCode = req.ProductCode;
                    productInfo.ProductName = req.ProductName;
                    productInfo.IsCanBuy = req.IsCanBuy;
                    productInfo.IsCanSell = req.IsCanSell;
                    productInfo.MainImageFileId = req.MainImageFileId;
                    productInfo.ProductTypeId = req.ProductTypeId;
                    productInfo.ProductCateId = req.ProductCateId;
                    productInfo.BarcodeConfigId = req.BarcodeConfigId;
                    productInfo.BarcodeStandardId = (int?)req.BarcodeStandardId;
                    productInfo.Barcode = req.Barcode;
                    productInfo.UnitId = req.UnitId;
                    productInfo.EstimatePrice = req.EstimatePrice;
                    productInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    //Product extra info
                    productExtra.Specification = req.Extra?.Specification;
                    productExtra.Description = req.Extra?.Description;

                    //Product stock info
                    productStockInfo.StockOutputRuleId = (int?)req.StockInfo?.StockOutputRuleId;
                    productStockInfo.AmountWarningMin = req.StockInfo?.AmountWarningMin;
                    productStockInfo.AmountWarningMax = req.StockInfo?.AmountWarningMax;
                    productStockInfo.TimeWarningTimeTypeId = (int?)req.StockInfo?.TimeWarningTimeTypeId;
                    productStockInfo.TimeWarningAmount = req.StockInfo?.TimeWarningAmount;
                    productStockInfo.DescriptionToStock = req.StockInfo?.DescriptionToStock;
                    productStockInfo.ExpireTimeTypeId = (int?)req.StockInfo?.ExpireTimeTypeId;
                    productStockInfo.ExpireTimeAmount = req.StockInfo?.ExpireTimeAmount;

                    var lstStockValidations = req.StockInfo?.StockIds?.Select(s => new ProductStockValidation()
                    {
                        ProductId = productInfo.ProductId,
                        StockId = s
                    });

                    _stockContext.RemoveRange(stockValidations);
                    if (lstStockValidations != null)
                    {
                        await _stockContext.AddRangeAsync(lstStockValidations);
                    }


                    var unitInfo = await _unitService.GetUnitInfo(req.UnitId);
                    if (unitInfo == null)
                    {
                        return UnitErrorCode.UnitNotFound;
                    }

                    var lstNewUnitConverions = req.StockInfo?.UnitConversions?
                        .Where(c => c.ProductUnitConversionId <= 0)?
                        .Select(u => new Infrastructure.EF.StockDB.ProductUnitConversion()
                        {
                            ProductId = productInfo.ProductId,
                            ProductUnitConversionName = u.ProductUnitConversionName,
                            SecondaryUnitId = u.SecondaryUnitId,
                            FactorExpression = u.FactorExpression,
                            ConversionDescription = u.ConversionDescription
                        });


                    if (lstNewUnitConverions != null)
                    {
                        await _stockContext.AddRangeAsync(lstNewUnitConverions);
                    }

                    foreach (var productUnitConversionId in keepIds)
                    {
                        var db = unitConverions.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);
                        var u = req.StockInfo?.UnitConversions?.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);
                        if (db != null && u != null)
                        {
                            db.ProductUnitConversionName = u.ProductUnitConversionName;
                            db.SecondaryUnitId = u.SecondaryUnitId;
                            db.FactorExpression = u.FactorExpression;
                            db.ConversionDescription = u.ConversionDescription;
                        }
                    }
                    var defaultUnitConversion = unitConverions.FirstOrDefault(c => c.IsDefault);
                    if (defaultUnitConversion != null)
                    {
                        defaultUnitConversion.SecondaryUnitId = req.UnitId;
                        defaultUnitConversion.ProductUnitConversionName = unitInfo.Data.UnitName;
                    }

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    var lstUnitConverions = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

                    await _activityLogService.CreateLog(EnumObjectType.Product, productInfo.ProductId, $"Cập nhật sản phẩm {productInfo.ProductName}", req.JsonSerialize());
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "UpdateProduct");
                    return GeneralCode.InternalError;
                }
            }

            if (req.MainImageFileId.HasValue && oldMainImageFileId != req.MainImageFileId)
            {
                _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.Product, productId, req.MainImageFileId.Value));
                if (oldMainImageFileId.HasValue)
                {
                    _asyncRunner.RunAsync<IFileService>(f => f.DeleteFile(oldMainImageFileId.Value));
                }
            }
            return GeneralCode.Success;
        }


        public async Task<Enum> DeleteProduct(int productId)
        {
            var productInfo = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (productInfo == null)
            {
                return ProductErrorCode.ProductNotFound;
            }

            var productExtra = await _stockContext.ProductExtraInfo.FirstOrDefaultAsync(p => p.ProductId == productId);

            var productStockInfo = await _stockContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);

            var stockValidations = await _stockContext.ProductStockValidation.Where(p => p.ProductId == productId).ToListAsync();

            var unitConverions = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();


            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    productInfo.IsDeleted = true;
                    productInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    productExtra.IsDeleted = true;

                    productStockInfo.IsDeleted = true;

                    //_stockContext.ProductStockValidation.RemoveRange(stockValidations);

                    // _stockContext.ProductUnitConversion.RemoveRange(unitConverions);

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.Product, productInfo.ProductId, $"Xóa sản phẩm {productInfo.ProductName}", productInfo.JsonSerialize());

                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "DeleteProduct");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<bool> ValidateProductUnitConversions(Dictionary<int, int> productUnitConvertsionProduct)
        {
            var productIds = productUnitConvertsionProduct.Values;
            var productUnitConversionIds = productUnitConvertsionProduct.Keys;

            var productUnitConversions = (await _stockContext.ProductUnitConversion
                .Where(c => productIds.Contains(c.ProductId) && productUnitConversionIds.Contains(c.ProductUnitConversionId))
                .ToListAsync())
                .ToDictionary(c => c.ProductUnitConversionId, c => c.ProductId);

            foreach (var item in productUnitConvertsionProduct)
            {
                if (!productUnitConversions.ContainsKey(item.Key) || productUnitConversions[item.Key] != item.Value) return false;
            }

            return true;
        }



        public async Task<PageData<ProductListOutput>> GetList(string keyword, int[] productTypeIds, int[] productCateIds, int page, int size, Clause filters = null)
        {
            var products = _stockContext.Product.AsQueryable();
            products = products.InternalFilter(filters);
            var query = (
              from p in products
              join pe in _stockContext.ProductExtraInfo on p.ProductId equals pe.ProductId
              join pt in _stockContext.ProductType on p.ProductTypeId equals pt.ProductTypeId into pts
              from pt in pts.DefaultIfEmpty()
              join pc in _stockContext.ProductCate on p.ProductCateId equals pc.ProductCateId into pcs
              from pc in pcs.DefaultIfEmpty()
              select new
              {
                  p.ProductId,
                  p.ProductCode,
                  p.ProductName,
                  p.MainImageFileId,
                  p.ProductTypeId,
                  ProductTypeName = pt == null ? null : pt.ProductTypeName,
                  p.ProductCateId,
                  ProductCateName = pc == null ? null : pc.ProductCateName,
                  p.Barcode,
                  pe.Specification,
                  pe.Description,
                  p.UnitId,
                  p.EstimatePrice
              });

            if (productTypeIds != null && productTypeIds.Length > 0)
            {
                var types = productTypeIds.Select(t => (int?)t);
                query = from p in query
                        where types.Contains(p.ProductTypeId)
                        select p;
            }

            if (productCateIds != null && productCateIds.Length > 0)
            {
                query = from p in query
                        where productCateIds.Contains(p.ProductCateId)
                        select p;
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from c in query
                        where
                        c.ProductCode.Contains(keyword)
                        || c.Barcode.Contains(keyword)
                        || c.ProductName.Contains(keyword)
                        || c.ProductTypeName.Contains(keyword)
                        || c.ProductCateName.Contains(keyword)
                        || c.Specification.Contains(keyword)
                        || c.Description.Contains(keyword)
                        select c;
            }

            var total = await query.CountAsync();
            var lstData = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var unitIds = lstData.Select(p => p.UnitId).ToList();
            var unitInfos = await _unitService.GetListByIds(unitIds);


            var pageData = new List<ProductListOutput>();
            foreach (var item in lstData)
            {
                var product = new ProductListOutput()
                {
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    Barcode = item.Barcode,
                    MainImageFileId = item.MainImageFileId,
                    ProductCateId = item.ProductCateId,
                    ProductCateName = item.ProductCateName,
                    ProductTypeId = item.ProductTypeId,
                    ProductTypeName = item.ProductTypeName,
                    Specification = item.Specification,
                    UnitId = item.UnitId,
                    EstimatePrice = item.EstimatePrice
                };

                var unitInfo = unitInfos.FirstOrDefault(u => u.UnitId == item.UnitId);

                product.UnitName = unitInfo?.UnitName;

                pageData.Add(product);
            }


            return (pageData, total);
        }

        public async Task<IList<ProductListOutput>> GetListByIds(IList<int> productIds)
        {
            if (productIds == null || productIds.Count == 0) return new List<ProductListOutput>();

            var query = (
                from p in _stockContext.Product
                join pe in _stockContext.ProductExtraInfo on p.ProductId equals pe.ProductId
                join pt in _stockContext.ProductType on p.ProductTypeId equals pt.ProductTypeId into pts
                from pt in pts.DefaultIfEmpty()
                join pc in _stockContext.ProductCate on p.ProductCateId equals pc.ProductCateId into pcs
                from pc in pcs.DefaultIfEmpty()
                where productIds.Contains(p.ProductId)
                select new
                {
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    p.MainImageFileId,
                    p.ProductTypeId,
                    ProductTypeName = pt == null ? null : pt.ProductTypeName,
                    p.ProductCateId,
                    ProductCateName = pc == null ? null : pc.ProductCateName,
                    p.Barcode,
                    pe.Specification,
                    pe.Description,
                    p.UnitId,
                    p.EstimatePrice
                });

            var lstData = await query.ToListAsync();

            var unitIds = lstData.Select(p => p.UnitId).ToList();
            var unitInfos = await _unitService.GetListByIds(unitIds);

            var stockProductData = _stockContext.StockProduct.AsNoTracking().Where(q => productIds.Contains(q.ProductId)).ToList();


            var data = new List<ProductListOutput>();
            foreach (var item in lstData)
            {
                var product = new ProductListOutput()
                {
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    Barcode = item.Barcode,
                    MainImageFileId = item.MainImageFileId,
                    ProductCateId = item.ProductCateId,
                    ProductCateName = item.ProductCateName,
                    ProductTypeId = item.ProductTypeId,
                    ProductTypeName = item.ProductTypeName,
                    Specification = item.Specification,
                    UnitId = item.UnitId,
                    EstimatePrice = item.EstimatePrice,
                    StockProductModelList = stockProductData.Where(q => q.ProductId == item.ProductId).Select(q => new StockProductOutput
                    {
                        StockId = q.StockId,
                        ProductId = q.ProductId,
                        PrimaryUnitId = item.UnitId,
                        PrimaryQuantityRemaining = q.PrimaryQuantityRemaining,
                        ProductUnitConversionId = q.ProductUnitConversionId,
                        ProductUnitConversionRemaining = q.ProductUnitConversionRemaining
                    }).ToList()
                };

                var unitInfo = unitInfos.FirstOrDefault(u => u.UnitId == item.UnitId);

                product.UnitName = unitInfo?.UnitName;

                data.Add(product);
            }


            return data;
        }
        private Enum ValidateProduct(ProductModel req)
        {
            if (req.StockInfo.UnitConversions?.Count > 0)
            {
                foreach (var unitConversion in req.StockInfo.UnitConversions)
                {
                    try
                    {
                        var eval = Utils.EvalPrimaryQuantityFromProductUnitConversionQuantity(1, unitConversion.FactorExpression);
                        if (!(eval > 0))
                        {
                            return ProductErrorCode.InvalidUnitConversionExpression;
                        }
                    }
                    catch (Exception)
                    {

                        return ProductErrorCode.InvalidUnitConversionExpression;
                    }

                }
            }
            return GeneralCode.Success;
        }
    }
}
