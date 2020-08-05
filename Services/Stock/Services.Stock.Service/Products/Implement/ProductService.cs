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
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.FileResources;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.GlobalObject.InternalDataInterface;
using static VErp.Commons.GlobalObject.InternalDataInterface.ProductModel;
using System.IO;
using System.Reflection;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using System.Text;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductService : IProductService
    {
        private readonly StockDBContext _stockContext;
        private readonly MasterDBContext _masterDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IActivityLogService _activityLogService;
        private readonly IFileService _fileService;
        private readonly IAsyncRunnerService _asyncRunner;

        public ProductService(
            StockDBContext stockContext
            , MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductService> logger
            , IUnitService unitService
            , IActivityLogService activityLogService
            , IFileService fileService
            , IAsyncRunnerService asyncRunner
            )
        {
            _masterDBContext = masterDBContext;
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
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                var productId = await AddProductToDb(req);
                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.Product, productId, $"Thêm mới sản phẩm {req.ProductName}", req.JsonSerialize());
                return productId;
            }
        }

        public async Task<int> AddProductToDb(ProductModel req)
        {
            req.ProductCode = (req.ProductCode ?? "").Trim();
            Enum validate;
            if (!(validate = ValidateProduct(req)).IsSuccess())
            {
                throw new BadRequestException(validate, req.ProductCode + " " + req.ProductName + " " + validate.GetEnumDescription());
            }

            var productExisted = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductCode == req.ProductCode || p.ProductName == req.ProductName);
            if (productExisted != null)
            {
                if (string.Compare(productExisted.ProductCode, req.ProductCode, StringComparison.OrdinalIgnoreCase) == 0)
                    throw new BadRequestException(ProductErrorCode.ProductCodeAlreadyExisted, $"Mã mặt hàng \"{req.ProductCode}\" đã tồn tại");
                throw new BadRequestException(ProductErrorCode.ProductNameAlreadyExisted, $"Tên mặt hàng \"{req.ProductCode}\" đã tồn tại");
            }

            if (!await _stockContext.ProductCate.AnyAsync(c => c.ProductCateId == req.ProductCateId))
            {
                throw new BadRequestException(ProductErrorCode.ProductCateInvalid, $"Danh mục mặt hàng không đúng");
            }

            if (!await _stockContext.ProductType.AnyAsync(c => c.ProductTypeId == req.ProductTypeId))
            {
                throw new BadRequestException(ProductErrorCode.ProductTypeInvalid, $"Loại sinh mã mặt hàng không đúng");
            }

            var productInfo = new Product()
            {
                ProductCode = req.ProductCode,
                ProductName = req.ProductName,
                ProductInternalName = req.ProductName.NormalizeAsInternalName(),
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
                throw new BadRequestException(UnitErrorCode.UnitNotFound, $"Sản phẩm {req.ProductCode}, đơn vị tính không tìm thấy ");
            }

            var lstUnitConverions = req.StockInfo?.UnitConversions?
                .Where(u => !u.IsDefault)?
                .Select(u => new ProductUnitConversion()
                {
                    ProductId = productInfo.ProductId,
                    ProductUnitConversionName = u.ProductUnitConversionName,
                    SecondaryUnitId = u.SecondaryUnitId,
                    FactorExpression = u.FactorExpression,
                    ConversionDescription = u.ConversionDescription,
                    IsDefault = false,
                    IsFreeStyle = u.IsFreeStyle
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
                    IsDefault = true,
                    IsFreeStyle = false
                }
            );

            if (lstUnitConverions != null)
            {
                await _stockContext.ProductUnitConversion.AddRangeAsync(lstUnitConverions);
            }

            await _stockContext.SaveChangesAsync();

            if (req.MainImageFileId.HasValue)
            {
                _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.Product, productInfo.ProductId, req.MainImageFileId.Value));
            }

            return productInfo.ProductId;
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
            var unitConverions = await _stockContext.ProductUnitConversion.AsNoTracking().Where(p => p.ProductId == productId).ToListAsync();

            return new ProductModel()
            {
                ProductId = productInfo.ProductId,
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
                        IsDefault = c.IsDefault,
                        IsFreeStyle = c.IsFreeStyle ?? false,
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

                    var keepIds = req.StockInfo?.UnitConversions?.Select(c => c.ProductUnitConversionId);
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
                    productInfo.ProductInternalName = req.ProductName.NormalizeAsInternalName();
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
                        .Select(u => new ProductUnitConversion()
                        {
                            ProductId = productInfo.ProductId,
                            ProductUnitConversionName = u.ProductUnitConversionName,
                            SecondaryUnitId = u.SecondaryUnitId,
                            FactorExpression = u.FactorExpression,
                            ConversionDescription = u.ConversionDescription,
                            IsDefault = false,
                            IsFreeStyle = u.IsFreeStyle
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
                            db.IsFreeStyle = u.IsFreeStyle;
                        }
                    }
                    var defaultUnitConversion = unitConverions.FirstOrDefault(c => c.IsDefault);
                    if (defaultUnitConversion != null)
                    {
                        defaultUnitConversion.SecondaryUnitId = req.UnitId;
                        defaultUnitConversion.IsDefault = true;
                        defaultUnitConversion.IsFreeStyle = false;
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

        public async Task<IList<ProductModel>> GetListProductsByIds(IList<int> productIds)
        {

            if (!(productIds?.Count > 0)) return new List<ProductModel>();

            var products = await _stockContext.Product.Where(p => productIds.Contains(p.ProductId)).ToListAsync();

            var productExtraData = await _stockContext.ProductExtraInfo.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();
            var productStockInfoData = await _stockContext.ProductStockInfo.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();
            var stockValidationData = await _stockContext.ProductStockValidation.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();
            var unitConverionData = await _stockContext.ProductUnitConversion.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();

            var result = new List<ProductModel>();
            foreach (var productInfo in products)
            {
                var productExtra = productExtraData.FirstOrDefault(p => p.ProductId == productInfo.ProductId);
                var productStockInfo = productStockInfoData.FirstOrDefault(p => p.ProductId == productInfo.ProductId);
                var stockValidations = stockValidationData.Where(p => p.ProductId == productInfo.ProductId);
                var unitConverions = unitConverionData.Where(p => p.ProductId == productInfo.ProductId);

                var productData = new ProductModel()
                {
                    ProductId = productInfo.ProductId,
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
                            IsDefault = c.IsDefault,
                            IsFreeStyle = c.IsFreeStyle ?? false,
                            FactorExpression = c.FactorExpression,
                            ConversionDescription = c.ConversionDescription
                        }).ToList()
                    } : null
                };
                result.Add(productData);
            }

            return result;
        }

        public async Task<IList<ProductModel>> GetListByCodeAndInternalNames(ProductQueryByProductCodeOrInternalNameRequest req)
        {
            var productCodes = req.ProductCodes;
            var productInternalNames = req.ProductInternalNames;

            if (!(productCodes?.Count > 0) && !(productInternalNames?.Count > 0)) return new List<ProductModel>();


            if (productCodes == null)
            {
                productCodes = new List<string>();
            }

            if (productInternalNames == null)
            {
                productInternalNames = new List<string>();
            }


            var products = await _stockContext.Product.AsNoTracking().Where(p => productCodes.Contains(p.ProductCode) || productInternalNames.Contains(p.ProductInternalName)).ToListAsync();

            var productIds = products.Select(d => d.ProductId).ToList();

            var productExtraData = await _stockContext.ProductExtraInfo.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();
            var productStockInfoData = await _stockContext.ProductStockInfo.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();
            var stockValidationData = await _stockContext.ProductStockValidation.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();
            var unitConverionData = await _stockContext.ProductUnitConversion.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();

            var result = new List<ProductModel>();
            foreach (var productInfo in products)
            {
                var productExtra = productExtraData.FirstOrDefault(p => p.ProductId == productInfo.ProductId);
                var productStockInfo = productStockInfoData.FirstOrDefault(p => p.ProductId == productInfo.ProductId);
                var stockValidations = stockValidationData.Where(p => p.ProductId == productInfo.ProductId);
                var unitConverions = unitConverionData.Where(p => p.ProductId == productInfo.ProductId);

                var productData = new ProductModel()
                {
                    ProductId = productInfo.ProductId,
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
                            IsDefault = c.IsDefault,
                            IsFreeStyle = c.IsFreeStyle ?? false,
                            FactorExpression = c.FactorExpression,
                            ConversionDescription = c.ConversionDescription
                        }).ToList()
                    } : null
                };
                result.Add(productData);
            }

            return result;
        }

        public async Task<int> ImportProductFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            // Lấy thông tin field
            var fields = typeof(Product).GetProperties(BindingFlags.Public);

            var productTypes = _stockContext.ProductType.Select(t => new { t.ProductTypeId, t.ProductTypeName }).ToDictionary(t => t.ProductTypeName, t => t.ProductTypeId);
            var productCates = _stockContext.ProductCate.Select(c => new { c.ProductCateId, c.ProductCateName }).ToDictionary(c => c.ProductCateName, c => c.ProductCateId);
            var barcodeConfigs = _masterDBContext.BarcodeConfig.Where(c => c.IsActived).Select(c => new { c.BarcodeConfigId, c.Name }).ToDictionary(c => c.Name, c => c.BarcodeConfigId);
            var units = _masterDBContext.Unit.Select(u => new { u.UnitId, u.UnitName }).ToDictionary(u => u.UnitName, u => u.UnitId);
            var stocks = _stockContext.Stock.ToDictionary(s => s.StockName, s => s.StockId);
            var data = reader.ReadSheetEntity<ProductImportModel>(mapping, (entity, propertyName, value) =>
            {
                switch (propertyName)
                {
                    case nameof(ProductImportModel.ProductTypeId):
                        if (!productTypes.ContainsKey(value)) throw new BadRequestException(ProductErrorCode.ProductTypeInvalid, $"Loại mặt hàng {value} không đúng");
                        entity.ProductTypeId = productTypes[value];
                        return true;
                    case nameof(ProductImportModel.ProductCateId):
                        if (!productCates.ContainsKey(value)) throw new BadRequestException(ProductErrorCode.ProductCateInvalid, $"Danh mục mặt hàng {value} không đúng");
                        entity.ProductCateId = productCates[value];
                        return true;
                    case nameof(ProductImportModel.BarcodeConfigId):
                        if (barcodeConfigs.ContainsKey(value)) entity.BarcodeConfigId = barcodeConfigs[value];
                        return true;
                    case nameof(ProductImportModel.UnitId):
                        if (!units.ContainsKey(value)) throw new BadRequestException(GeneralCode.InvalidParams, $"Đơn vị chính {value} không đúng");
                        entity.UnitId = units[value];
                        return true;
                    case nameof(ProductImportModel.StockOutputRuleId):
                        var rule = EnumExtensions.GetEnumMembers<EnumStockOutputRule>().FirstOrDefault(r => r.Description == value);
                        if (rule != null) entity.StockOutputRuleId = rule.Enum;
                        return true;
                    case nameof(ProductImportModel.ExpireTimeTypeId):
                        var timeType = EnumExtensions.GetEnumMembers<EnumTimeType>().FirstOrDefault(r => r.Description == value);
                        if (timeType != null) entity.ExpireTimeTypeId = timeType.Enum;
                        return true;
                    case nameof(ProductImportModel.StockIds):
                        var stockNames = value.Split(",");
                        var stockIds = stockNames.Where(s => stocks.ContainsKey(s)).Select(s => stocks[s]).ToList();
                        if (stockIds.Count != stockNames.Length) throw new BadRequestException(GeneralCode.InvalidParams, $"Danh sách kho {value} không đúng");
                        if (stockIds.Count > 0) entity.StockIds = stockIds;
                        return true;
                    case nameof(ProductImportModel.SecondaryUnitId01):
                        if (units.ContainsKey(value)) entity.SecondaryUnitId01 = units[value];
                        return true;
                    case nameof(ProductImportModel.SecondaryUnitId02):
                        if (units.ContainsKey(value)) entity.SecondaryUnitId02 = units[value];
                        return true;
                    case nameof(ProductImportModel.SecondaryUnitId03):
                        if (units.ContainsKey(value)) entity.SecondaryUnitId03 = units[value];
                        return true;
                    case nameof(ProductImportModel.SecondaryUnitId04):
                        if (units.ContainsKey(value)) entity.SecondaryUnitId04 = units[value];
                        return true;
                    case nameof(ProductImportModel.SecondaryUnitId05):
                        if (units.ContainsKey(value)) entity.SecondaryUnitId05 = units[value];
                        return true;
                    default:
                        return false;
                }
            });

            // Validate unique product code
            var productCodes = data.Select(p => p.ProductCode).ToList();
            if (productCodes.Count != productCodes.Distinct().Count() || _stockContext.Product.Any(p => productCodes.Contains(p.ProductCode)))
            {
                throw new BadRequestException(ProductErrorCode.ProductCodeAlreadyExisted);
            }

            // Validate required product name
            if (data.Any(p => string.IsNullOrEmpty(p.ProductName)))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng nhập tên sản phẩm");
            }
            var productNames = data.Select(r => r.ProductName).ToList();

            // Validate unique product name
            if (productNames.Count != productNames.Distinct().Count() || _stockContext.Product.Any(p => productNames.Contains(p.ProductName)))
            {
                throw new BadRequestException(ProductErrorCode.ProductNameAlreadyExisted);
            }

            var name01Text = nameof(ProductImportModel.ProductUnitConversionName01);
            var unit01Text = nameof(ProductImportModel.SecondaryUnitId01);
            var exp01Text = nameof(ProductImportModel.FactorExpression01);
            var desc01Text = nameof(ProductImportModel.ConversionDescription01);
            Type typeInfo = typeof(ProductImportModel);

            using var trans = await _stockContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var row in data)
                {
                    var productInfo = new Product()
                    {
                        ProductCode = row.ProductCode,
                        ProductName = row.ProductName,
                        ProductInternalName = row.ProductName.NormalizeAsInternalName(),
                        IsCanBuy = row.IsCanBuy ?? true,
                        IsCanSell = row.IsCanSell ?? true,
                        MainImageFileId = null,
                        ProductTypeId = row.ProductTypeId,
                        ProductCateId = row.ProductCateId,
                        BarcodeConfigId = row.BarcodeConfigId,
                        BarcodeStandardId = null,
                        Barcode = row.Barcode,
                        UnitId = row.UnitId,
                        EstimatePrice = row.EstimatePrice,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                        IsDeleted = false,
                        ProductExtraInfo = new ProductExtraInfo()
                        {
                            Specification = row.Specification,
                            Description = row.Description,
                            IsDeleted = false
                        },
                        ProductStockInfo = new ProductStockInfo()
                        {
                            StockOutputRuleId = (int?)row.StockOutputRuleId,
                            AmountWarningMin = row.AmountWarningMin,
                            AmountWarningMax = row.AmountWarningMax,
                            TimeWarningTimeTypeId = null,
                            TimeWarningAmount = null,
                            DescriptionToStock = row.DescriptionToStock,
                            ExpireTimeTypeId = (int?)row.ExpireTimeTypeId,
                            ExpireTimeAmount = row.ExpireTimeAmount,
                            IsDeleted = false
                        },
                        ProductStockValidation = row.StockIds.Select(s => new ProductStockValidation
                        {
                            StockId = s
                        }).ToList()
                    };

                    var unitByIds = units.ToDictionary(u => u.Value, u => u.Key);
                    var lstUnitConverions = new List<ProductUnitConversion>(){
                            new ProductUnitConversion()
                            {
                                ProductUnitConversionName = unitByIds[row.UnitId],
                                SecondaryUnitId = row.UnitId,
                                FactorExpression = "1",
                                ConversionDescription = "Mặc định",
                                IsDefault = true,
                                IsFreeStyle = false
                            }
                        };

                    for (int suffix = 1; suffix <= 5; suffix++)
                    {
                        var nameText = suffix > 1 ? new StringBuilder(name01Text).Remove(name01Text.Length - 2, 2).Append($"0{suffix}").ToString() : name01Text;
                        var unitText = suffix > 1 ? new StringBuilder(unit01Text).Remove(unit01Text.Length - 2, 2).Append($"0{suffix}").ToString() : unit01Text;
                        var expText = suffix > 1 ? new StringBuilder(exp01Text).Remove(exp01Text.Length - 2, 2).Append($"0{suffix}").ToString() : exp01Text;
                        var descText = suffix > 1 ? new StringBuilder(desc01Text).Remove(desc01Text.Length - 2, 2).Append($"0{suffix}").ToString() : desc01Text;

                        var name = typeInfo.GetProperty(nameText).GetValue(row) as string;
                        var unitId = typeInfo.GetProperty(unitText).GetValue(row) as int?;
                        var exp = typeInfo.GetProperty(expText).GetValue(row) as string;
                        var desc = typeInfo.GetProperty(descText).GetValue(row) as string;
                        if (name != null && unitId != null)
                        {
                            try
                            {
                                var eval = Utils.EvalPrimaryQuantityFromProductUnitConversionQuantity(1, exp);
                                if (!(eval > 0))
                                {
                                    throw new BadRequestException(ProductErrorCode.InvalidUnitConversionExpression, $"Biểu thức chuyển đổi {exp} của sản phẩm {row.ProductCode} không đúng");
                                }
                            }
                            catch (Exception)
                            {

                                throw new BadRequestException(ProductErrorCode.InvalidUnitConversionExpression, $"Lỗi không thể tính toán biểu thức đơn vị chuyển đổi {exp}  của sản phẩm {row.ProductCode}");
                            }

                            lstUnitConverions.Add(new ProductUnitConversion()
                            {
                                ProductUnitConversionName = name,
                                SecondaryUnitId = unitId.Value,
                                FactorExpression = typeInfo.GetProperty(expText).GetValue(row) as string,
                                ConversionDescription = typeInfo.GetProperty(descText).GetValue(row) as string,
                                IsDefault = false,
                                IsFreeStyle = false
                            });
                        }
                    }

                    productInfo.ProductUnitConversion = lstUnitConverions;

                    _stockContext.Product.Add(productInfo);
                }

                _stockContext.SaveChanges();
                trans.Commit();
                return data.Count;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }

        }

        public async Task<List<EntityField>> GetFields(Type type)
        {
            var fields = new List<EntityField>();

            foreach (var prop in type.GetProperties())
            {
                EntityField field = new EntityField
                {
                    FieldName = prop.Name,
                    Title = prop.GetCustomAttributes<System.ComponentModel.DataAnnotations.DisplayAttribute>().FirstOrDefault()?.Name ?? prop.Name
                };
                fields.Add(field);
            }

            return fields;
        }

        private Enum ValidateProduct(ProductModel req)
        {
            if (string.IsNullOrWhiteSpace(req?.ProductCode))
            {
                return ProductErrorCode.ProductCodeEmpty;
            }

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
