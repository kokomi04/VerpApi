using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VErp.Services.Master.Service.Dictionay;
using VErp.Commons.Enums.StandardEnum;
using VErp.Services.Master.Service.Activity;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using static VErp.Services.Stock.Model.Product.ProductModel;
using VErp.Services.Stock.Service.FileResources;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductService : IProductService
    {
        private readonly StockDBContext _stockContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IActivityService _activityService;
        private readonly IFileService _fileService;

        public ProductService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductService> logger
            , IUnitService unitService
            , IActivityService activityService
            , IFileService fileService
            )
        {
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _unitService = unitService;
            _activityService = activityService;
            _fileService = fileService;
        }

        public async Task<ServiceResult<int>> AddProduct(ProductModel req)
        {
            req.ProductCode = (req.ProductCode ?? "").Trim();

            var productByCode = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductCode == req.ProductCode);
            if (productByCode != null)
            {
                return ProductErrorCode.ProductCodeAlreadyExisted;
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
                        DescriptionToStock = req.StockInfo?.DescriptionToStock,
                        TimeWarningAmount = req.StockInfo?.TimeWarningAmount,
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


                    var lstUnitConverions = req.StockInfo?.UnitConversions?.Select(u => new ProductUnitConversion()
                    {
                        ProductId = productInfo.ProductId,
                        ProductUnitConversionId = u.ProductUnitConversionId,
                        ProductUnitConversionName = u.ProductUnitConversionName,
                        SecondaryUnitId = u.SecondaryUnitId,
                        FactorExpression = u.FactorExpression,
                        ConversionDescription = u.ConversionDescription
                    });


                    if (lstUnitConverions != null)
                    {
                        await _stockContext.AddRangeAsync(lstUnitConverions);
                    }

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    var objLog = GetProductForLog(productInfo, productExtra, productStockInfo, lstStockValidations, lstUnitConverions);

                    await _activityService.CreateActivity(EnumObjectType.Product, productInfo.ProductId, $"Thêm mới sản phẩm {productInfo.ProductName}", null, objLog);

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
                _ = _fileService.FileAssignToObject(EnumObjectType.Product, productId, req.MainImageFileId.Value);
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
            var productExtra = await _stockContext.ProductExtraInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
            var productStockInfo = await _stockContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
            var stockValidations = await _stockContext.ProductStockValidation.Where(p => p.ProductId == productId).ToListAsync();
            var unitConverions = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

            return new ProductModel()
            {
                ProductCode = productInfo.ProductCode,
                ProductName = productInfo.ProductName,
                IsCanBuy = productInfo.IsCanBuy,
                IsCanSell = productInfo.IsCanSell,
                MainImageFileId = productInfo.MainImageFileId,
                ProductTypeId = productInfo.ProductTypeId,
                ProductCateId = productInfo.ProductCateId,
                BarcodeStandardId = (EnumBarcodeStandard?)productInfo.BarcodeStandardId,
                Barcode = productInfo.Barcode,
                UnitId = productInfo.UnitId,
                EstimatePrice = productInfo.EstimatePrice,

                Extra = new ProductModelExtra()
                {
                    Specification = productExtra.Specification,
                    Description = productExtra.Description
                },
                StockInfo = new ProductModelStock()
                {
                    StockOutputRuleId = (EnumStockOutputRule?)productStockInfo.StockOutputRuleId,
                    AmountWarningMin = productStockInfo.AmountWarningMin,
                    AmountWarningMax = productStockInfo.AmountWarningMax,
                    TimeWarningTimeTypeId = (EnumTimeType?)productStockInfo.TimeWarningTimeTypeId,
                    TimeWarningAmount = productStockInfo.TimeWarningAmount,
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
                }

            };
        }


        public async Task<Enum> UpdateProduct(int productId, ProductModel req)
        {
            req.ProductCode = (req.ProductCode ?? "").Trim();

            var productByCode = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductCode == req.ProductCode && p.ProductId != productId);
            if (productByCode != null)
            {
                return ProductErrorCode.ProductCodeAlreadyExisted;
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

                    oldMainImageFileId = productInfo.MainImageFileId;

                    var productExtra = await _stockContext.ProductExtraInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                    var productStockInfo = await _stockContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                    var stockValidations = await _stockContext.ProductStockValidation.Where(p => p.ProductId == productId).ToListAsync();
                    var unitConverions = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

                    var beforeData = GetProductForLog(productInfo, productExtra, productStockInfo, stockValidations, unitConverions);


                    //Update

                    //Productinfo
                    productInfo.ProductCode = req.ProductCode;
                    productInfo.ProductName = req.ProductName;
                    productInfo.IsCanBuy = req.IsCanBuy;
                    productInfo.IsCanSell = req.IsCanSell;
                    productInfo.MainImageFileId = req.MainImageFileId;
                    productInfo.ProductTypeId = req.ProductTypeId;
                    productInfo.ProductCateId = req.ProductCateId;
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
                    productStockInfo.DescriptionToStock = req.StockInfo?.DescriptionToStock;
                    productStockInfo.TimeWarningAmount = req.StockInfo?.TimeWarningAmount;

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


                    var lstUnitConverions = req.StockInfo?.UnitConversions?.Select(u => new ProductUnitConversion()
                    {
                        ProductId = productInfo.ProductId,
                        ProductUnitConversionId = u.ProductUnitConversionId,
                        ProductUnitConversionName = u.ProductUnitConversionName,
                        SecondaryUnitId = u.SecondaryUnitId,
                        FactorExpression = u.FactorExpression,
                        ConversionDescription = u.ConversionDescription
                    });

                    _stockContext.RemoveRange(unitConverions);
                    if (lstUnitConverions != null)
                    {
                        await _stockContext.AddRangeAsync(lstUnitConverions);
                    }

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    var objLog = GetProductForLog(productInfo, productExtra, productStockInfo, lstStockValidations, lstUnitConverions);

                    await _activityService.CreateActivity(EnumObjectType.Product, productInfo.ProductId, $"Cập nhật sản phẩm {productInfo.ProductName}", beforeData.JsonSerialize(), objLog);
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
                _ = _fileService.FileAssignToObject(EnumObjectType.Product, productId, req.MainImageFileId.Value);
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

            productInfo.IsDeleted = true;
            productInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

            var productExtra = await _stockContext.ProductExtraInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productExtra != null)
            {
                productExtra.IsDeleted = true;
            }

            var productStockInfo = await _stockContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);

            var stockValidations = await _stockContext.ProductStockValidation.Where(p => p.ProductId == productId).ToListAsync();

            var unitConverions = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

            var objLog = GetProductForLog(productInfo, productExtra, productStockInfo, stockValidations, unitConverions);
            var dataBefore = objLog.JsonSerialize();

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    productInfo.IsDeleted = true;
                    productInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    productExtra.IsDeleted = true;

                    productStockInfo.IsDeleted = true;

                    _stockContext.ProductStockValidation.RemoveRange(stockValidations);

                    _stockContext.ProductUnitConversion.RemoveRange(unitConverions);

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityService.CreateActivity(EnumObjectType.Product, productInfo.ProductId, $"Xóa sản phẩm {productInfo.ProductName}", dataBefore, null);

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

        public async Task<PageData<ProductListOutput>> GetList(string keyword, int page, int size)
        {
            var query = (
                from p in _stockContext.Product
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
                    p.UnitId
                });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from c in query
                        where c.ProductName.Contains(keyword)
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
                    UnitId = item.UnitId
                };

                var unitInfo = unitInfos.FirstOrDefault(u => u.UnitId == item.UnitId);

                product.UnitName = unitInfo?.UnitName;

                pageData.Add(product);
            }


            return (pageData, total);
        }

        private object GetProductForLog(Product productInfo, ProductExtraInfo extraInfo, ProductStockInfo stockInfo, IEnumerable<ProductStockValidation> stocks, IEnumerable<ProductUnitConversion> converts)
        {
            return new
            {
                ProductInfo = productInfo,
                ProductExtraInfo = extraInfo,
                ProductStockInfo = stockInfo,
                ProductStockValidations = stocks,
                ProductUnitConversions = converts
            };
        }
    }
}
