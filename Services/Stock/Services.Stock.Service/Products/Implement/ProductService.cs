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
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;

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
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ICurrentContextService _currentContextService;

        public ProductService(
            StockDBContext stockContext
            , MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductService> logger
            , IUnitService unitService
            , IActivityLogService activityLogService
            , IFileService fileService
            , IAsyncRunnerService asyncRunner
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ICurrentContextService currentContextService
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
            _customGenCodeHelperService = customGenCodeHelperService;
            _currentContextService = currentContextService;
        }

        public async Task<int> AddProduct(ProductModel req)
        {
            var customGenCodeId = await GenerateProductCode(null, req);

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                var productId = await AddProductToDb(req);
                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.Product, productId, $"Thêm mới sản phẩm {req.ProductName}", req.JsonSerialize());

                await ConfirmProductCode(customGenCodeId);

                return productId;
            }


        }

        public async Task<int> AddProductDefault(ProductDefaultModel req)
        {
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                req.ProductCode = (req.ProductCode ?? "").Trim();
                var productExisted = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductCode == req.ProductCode || p.ProductName == req.ProductName);
                if (productExisted != null)
                {
                    if (string.Compare(productExisted.ProductCode, req.ProductCode, StringComparison.OrdinalIgnoreCase) == 0)
                        throw new BadRequestException(ProductErrorCode.ProductCodeAlreadyExisted, $"Mã mặt hàng \"{req.ProductCode}\" đã tồn tại");
                    throw new BadRequestException(ProductErrorCode.ProductNameAlreadyExisted, $"Tên mặt hàng \"{req.ProductName}\" đã tồn tại");
                }
                var defaultProductCate = _stockContext.ProductCate.FirstOrDefault(c => c.IsDefault);
                if (defaultProductCate == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Danh mục mặt hàng mặc định không tồn tại");
                var unitInfo = await _unitService.GetUnitInfo(req.UnitId);
                if (unitInfo == null)
                {
                    throw new BadRequestException(UnitErrorCode.UnitNotFound, $"Sản phẩm {req.ProductCode}, đơn vị tính không tìm thấy ");
                }
                var productInfo = new Product()
                {
                    ProductCode = req.ProductCode,
                    ProductName = req.ProductName ?? req.ProductCode,
                    ProductInternalName = req.ProductName.NormalizeAsInternalName(),
                    IsCanBuy = false,
                    IsCanSell = false,
                    ProductCateId = defaultProductCate.ProductCateId,
                    UnitId = req.UnitId
                };

                await _stockContext.AddAsync(productInfo);
                await _stockContext.SaveChangesAsync();

                var productStockInfo = new ProductStockInfo()
                {
                    ProductId = productInfo.ProductId,
                    StockOutputRuleId = (int)EnumStockOutputRule.None
                };

                await _stockContext.AddAsync(productStockInfo);

                var productExtra = new ProductExtraInfo()
                {
                    ProductId = productInfo.ProductId,
                    Specification = req.Specification
                };

                await _stockContext.AddAsync(productExtra);
                var unitConverion = new ProductUnitConversion()
                {
                    ProductId = productInfo.ProductId,
                    ProductUnitConversionName = unitInfo.UnitName,
                    SecondaryUnitId = req.UnitId,
                    FactorExpression = "1",
                    ConversionDescription = "Mặc định",
                    IsDefault = true,
                    IsFreeStyle = false
                };
                _stockContext.ProductUnitConversion.Add(unitConverion);

                await _stockContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.Product, productInfo.ProductId, $"Thêm mới sản phẩm {req.ProductName}", req.JsonSerialize());
                return productInfo.ProductId;
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
                throw new BadRequestException(ProductErrorCode.ProductNameAlreadyExisted, $"Tên mặt hàng \"{req.ProductName}\" đã tồn tại");
            }

            if (!await _stockContext.ProductCate.AnyAsync(c => c.ProductCateId == req.ProductCateId))
            {
                throw new BadRequestException(ProductErrorCode.ProductCateInvalid, $"Danh mục mặt hàng không đúng");
            }

            //if (!await _stockContext.ProductType.AnyAsync(c => c.ProductTypeId == req.ProductTypeId))
            //{
            //    throw new BadRequestException(ProductErrorCode.ProductTypeInvalid, $"Loại sinh mã mặt hàng không đúng");
            //}

            var productInfo = new Product()
            {
                ProductCode = req.ProductCode,
                ProductName = req.ProductName ?? req.ProductCode,
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
                IsDeleted = false,
                CustomerId = req.CustomerId,
                GrossWeight = req.GrossWeight,
                Height = req.Height,
                Long = req.Long,
                Width = req.Width,
                LoadAbility = req.LoadAbility,
                NetWeight = req.NetWeight,
                PackingMethod = req.PackingMethod,
                Measurement = req.Measurement,
                Quantitative = req.Quantitative,
                QuantitativeUnitTypeId = (int?)req.QuantitativeUnitTypeId,
                ProductDescription = req.ProductDescription,
                ProductNameEng = req.ProductNameEng
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
                    ProductUnitConversionName = unitInfo.UnitName,
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

        public async Task<ProductModel> ProductInfo(int productId)
        {
            var productInfo = await _stockContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }
            return (await EnrichToProductModel(new[] { productInfo })).FirstOrDefault();
        }



        public async Task<bool> UpdateProduct(int productId, ProductModel req)
        {
            Enum validate;
            if (!(validate = ValidateProduct(req)).IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            req.ProductCode = (req.ProductCode ?? "").Trim();

            var productExisted = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId != productId && p.ProductName == req.ProductName);
            if (productExisted != null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNameAlreadyExisted);
            }

            var customGenCodeId = await GenerateProductCode(productId, req);

            long? oldMainImageFileId = 0L;

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //Getdata
                    var productInfo = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                    if (productInfo == null)
                    {
                        throw new BadRequestException(ProductErrorCode.ProductNotFound);
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
                            throw new BadRequestException(ProductErrorCode.SomeProductUnitConversionInUsed);
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
                    productInfo.CustomerId = req.CustomerId;
                    productInfo.GrossWeight = req.GrossWeight;
                    productInfo.Height = req.Height;
                    productInfo.Long = req.Long;
                    productInfo.Width = req.Width;
                    productInfo.LoadAbility = req.LoadAbility;
                    productInfo.NetWeight = req.NetWeight;
                    productInfo.PackingMethod = req.PackingMethod;
                    productInfo.Measurement = req.Measurement;
                    productInfo.Quantitative = req.Quantitative;
                    productInfo.QuantitativeUnitTypeId = (int?)req.QuantitativeUnitTypeId;
                    productInfo.ProductDescription = req.ProductDescription;
                    productInfo.ProductNameEng = req.ProductNameEng;

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
                        throw new BadRequestException(UnitErrorCode.UnitNotFound);
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
                        defaultUnitConversion.ProductUnitConversionName = unitInfo.UnitName;
                    }

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    var lstUnitConverions = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

                    await _activityLogService.CreateLog(EnumObjectType.Product, productInfo.ProductId, $"Cập nhật sản phẩm {productInfo.ProductName}", req.JsonSerialize());

                    await ConfirmProductCode(customGenCodeId);
                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;
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
            return true;
        }


        private async Task<CustomGenCodeBaseValueModel> GenerateProductCode(int? productId, ProductModel model)
        {
            int customGenCodeId = 0;
            model.ProductCode = (model.ProductCode ?? "").Trim();

            Product existedItem = null;
            if (!string.IsNullOrWhiteSpace(model.ProductCode))
            {
                existedItem = await _stockContext.Product.FirstOrDefaultAsync(r => r.ProductCode == model.ProductCode && r.ProductId != productId);
                if (existedItem != null) throw new BadRequestException(ProductErrorCode.ProductCodeAlreadyExisted);
                return null;
            }
            else
            {
                var productTypeInfo = await _stockContext.ProductType.FirstOrDefaultAsync(t => t.ProductTypeId == model.ProductTypeId);

                var config = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.Product, EnumObjectType.ProductType, model.ProductTypeId ?? 0, productId, productTypeInfo?.IdentityCode, _currentContextService.GetNowUtc().GetUnix());
                if (config == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết lập cấu hình sinh mã cho loại " + (productTypeInfo?.ProductTypeName));

                customGenCodeId = config.CustomGenCodeId;
                int dem = 0;
                do
                {
                    model.ProductCode = (await _customGenCodeHelperService.GenerateCode(config.CustomGenCodeId, config.CurrentLastValue.LastValue, productId, productTypeInfo?.IdentityCode, _currentContextService.GetNowUtc().GetUnix()))?.CustomCode;
                    existedItem = await _stockContext.Product.FirstOrDefaultAsync(r => r.ProductCode == model.ProductCode && r.ProductId != productId);
                    dem++;
                } while (existedItem != null && dem < 10);
                return config.CurrentLastValue;
            }

        }

        private async Task<bool> ConfirmProductCode(CustomGenCodeBaseValueModel customGenCodeBaseValue)
        {
            if (customGenCodeBaseValue.IsNullObject()) return true;

            return await _customGenCodeHelperService.ConfirmCode(customGenCodeBaseValue);
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            var productInfo = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
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

                    return true;
                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;
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

            return await EnrichToProductModel(products);

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

            return await EnrichToProductModel(products);
        }

        private async Task<IList<ProductModel>> EnrichToProductModel(IList<Product> products)
        {
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

                result.Add(new ProductModel()
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
                    CustomerId = productInfo.CustomerId,
                    GrossWeight = productInfo.GrossWeight,
                    Height = productInfo.Height,
                    Long = productInfo.Long,
                    Width = productInfo.Width,
                    LoadAbility = productInfo.LoadAbility,
                    NetWeight = productInfo.NetWeight,
                    PackingMethod = productInfo.PackingMethod,
                    Measurement = productInfo.Measurement,
                    Quantitative = productInfo.Quantitative,
                    QuantitativeUnitTypeId = (EnumQuantitativeUnitType?)productInfo.QuantitativeUnitTypeId,
                    ProductDescription = productInfo.ProductDescription,
                    ProductNameEng = productInfo.ProductNameEng,

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
                });
            }

            return result;
        }

        public async Task<int> ImportProductFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            // Lấy thông tin field
            var fields = typeof(Product).GetProperties(BindingFlags.Public);

            var productTypes = _stockContext.ProductType.Select(t => new { t.ProductTypeId, t.IdentityCode }).ToList().Select(t => new { IdentityCode = t.IdentityCode.NormalizeAsInternalName(), t.ProductTypeId }).GroupBy(t => t.IdentityCode).ToDictionary(t => t.Key, t => t.First().ProductTypeId);
            var productCates = _stockContext.ProductCate.Select(c => new { c.ProductCateId, c.ProductCateName }).ToList().Select(c => new { ProductCateName = c.ProductCateName.NormalizeAsInternalName(), c.ProductCateId }).GroupBy(c => c.ProductCateName).ToDictionary(c => c.Key, c => c.First().ProductCateId);
            var barcodeConfigs = _masterDBContext.BarcodeConfig.Where(c => c.IsActived).Select(c => new { c.BarcodeConfigId, c.Name }).ToDictionary(c => c.Name.NormalizeAsInternalName(), c => c.BarcodeConfigId);
            var units = _masterDBContext.Unit.Select(u => new { u.UnitId, u.UnitName }).ToList().Select(u => new { UnitName = u.UnitName.NormalizeAsInternalName(), u.UnitId }).GroupBy(u => u.UnitName).ToDictionary(u => u.Key, u => u.First().UnitId);
            var stocks = _stockContext.Stock.ToDictionary(s => s.StockName, s => s.StockId);

            var data = reader.ReadSheetEntity<ProductImportModel>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;
                switch (propertyName)
                {
                    case nameof(ProductImportModel.BarcodeConfigId):
                        if (barcodeConfigs.ContainsKey(value)) entity.BarcodeConfigId = barcodeConfigs[value];
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
                    case nameof(ProductImportModel.QuantitativeUnitTypeId):
                        var quantitativeUnitTypeId = EnumExtensions.GetEnumMembers<EnumQuantitativeUnitType>().FirstOrDefault(r => r.Description.NormalizeAsInternalName() == value.NormalizeAsInternalName());
                        if (quantitativeUnitTypeId != null) entity.QuantitativeUnitTypeId = quantitativeUnitTypeId.Enum;
                        return true;
                        //case nameof(ProductImportModel.ProductTypeCode):
                        //case nameof(ProductImportModel.ProductTypeName):
                        //case nameof(ProductImportModel.ProductCate):
                        //case nameof(ProductImportModel.Unit):
                        //case nameof(ProductImportModel.SecondaryUnit01):
                        //case nameof(ProductImportModel.SecondaryUnit02):
                        //case nameof(ProductImportModel.SecondaryUnit03):
                        //case nameof(ProductImportModel.SecondaryUnit04):
                        //case nameof(ProductImportModel.SecondaryUnit05):                    
                        //    var type = entity.GetType();
                        //    var p = type.GetProperty(propertyName);
                        //    if (p != null)
                        //    {
                        //        p.SetValue(entity, value);
                        //    }
                        //    return true;
                }
                return false;
            });

            var includeProductCates = new List<ProductCate>();
            var includeProductTypes = new List<ProductType>();
            var includeUnits = new List<Unit>();

            var unit01Text = nameof(ProductImportModel.SecondaryUnit01);
            var exp01Text = nameof(ProductImportModel.FactorExpression01);
            Type typeInfo = typeof(ProductImportModel);

            foreach (var row in data)
            {
                if (!units.ContainsKey(row.Unit.NormalizeAsInternalName()) && !includeUnits.Any(u => u.UnitName.NormalizeAsInternalName() == row.Unit.NormalizeAsInternalName()))
                {
                    includeUnits.Add(new Unit
                    {
                        UnitName = row.Unit,
                        UnitStatusId = (int)EnumUnitStatus.Using
                    });
                }
                for (int suffix = 1; suffix <= 5; suffix++)
                {
                    var unitText = suffix > 1 ? new StringBuilder(unit01Text).Remove(unit01Text.Length - 2, 2).Append($"0{suffix}").ToString() : unit01Text;
                    var unit = typeInfo.GetProperty(unitText).GetValue(row) as string;
                    if (!string.IsNullOrEmpty(unit) && !units.ContainsKey(unit.NormalizeAsInternalName()) && !includeUnits.Any(u => u.UnitName.NormalizeAsInternalName() == unit.NormalizeAsInternalName()))
                    {
                        includeUnits.Add(new Unit
                        {
                            UnitName = unit,
                            UnitStatusId = (int)EnumUnitStatus.Using
                        });
                    }
                }
                if (!productCates.ContainsKey(row.ProductCate.NormalizeAsInternalName()) && !includeProductCates.Any(c => c.ProductCateName.NormalizeAsInternalName() == row.ProductCate.NormalizeAsInternalName()))
                {
                    includeProductCates.Add(new ProductCate
                    {
                        ProductCateName = row.ProductCate,
                        SortOrder = 9999
                    });
                }

                if (!productTypes.ContainsKey(row.ProductTypeCode.NormalizeAsInternalName()) && !includeProductTypes.Any(t => t.IdentityCode.NormalizeAsInternalName() == row.ProductTypeCode.NormalizeAsInternalName()))
                {
                    includeProductTypes.Add(new ProductType
                    {
                        IdentityCode = row.ProductTypeCode,
                        ProductTypeName = string.IsNullOrEmpty(row.ProductTypeName) ? row.ProductTypeCode : row.ProductTypeName
                    });
                }
            }

            _masterDBContext.Unit.AddRange(includeUnits);
            _stockContext.ProductType.AddRange(includeProductTypes);
            _stockContext.ProductCate.AddRange(includeProductCates);

            _masterDBContext.SaveChanges();
            _stockContext.SaveChanges();

            foreach (var unit in includeUnits)
            {
                units.Add(unit.UnitName.NormalizeAsInternalName(), unit.UnitId);
            }
            foreach (var productCate in includeProductCates)
            {
                productCates.Add(productCate.ProductCateName.NormalizeAsInternalName(), productCate.ProductCateId);
            }
            foreach (var productType in includeProductTypes)
            {
                productTypes.Add(productType.IdentityCode.NormalizeAsInternalName(), productType.ProductTypeId);
            }

            // Validate unique product code
            var productCodes = data.Select(p => p.ProductCode).ToList();

            var dupCodes = productCodes.GroupBy(c => c).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            dupCodes.AddRange(_stockContext.Product
                .Where(p => productCodes.Contains(p.ProductCode))
                .Select(p => p.ProductCode)
                .ToList());
            dupCodes = dupCodes.Distinct().ToList();
            if (dupCodes.Count > 0)
            {
                throw new BadRequestException(ProductErrorCode.ProductCodeAlreadyExisted, $"Mã mặt hàng {string.Join(",", dupCodes)} đã tồn tại");
            }

            // Validate required product name
            var emptyNameProducts = data.Where(p => string.IsNullOrEmpty(p.ProductName)).Select(p => p.ProductCode).ToList();
            if (emptyNameProducts.Count > 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, $"Vui lòng nhập tên sản phẩm có mã: {string.Join(",", emptyNameProducts)}");
            }

            var productNames = data.Select(r => r.ProductName.NormalizeAsInternalName()).ToList();
            // Validate unique product name
            var dupNames = productNames.GroupBy(n => n).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var dupNameCodes = data.Where(r => dupCodes.Contains(r.ProductName.NormalizeAsInternalName())).Select(r => r.ProductCode).ToList();
            var dupNameProducts = _stockContext.Product
                .Where(p => productNames.Contains(p.ProductInternalName)).ToList();

            dupNames.AddRange(dupNameProducts.Select(p => p.ProductInternalName).ToList());
            dupNameCodes.AddRange(dupNameProducts.Select(p => p.ProductCode).ToList());

            dupNames = dupNames.Distinct().ToList();
            dupNameCodes = dupNameCodes.Distinct().ToList();

            if (dupNameProducts.Count > 0)
            {
                throw new BadRequestException(ProductErrorCode.ProductNameAlreadyExisted, $"Tên mặt hàng {string.Join(",", dupNames)} của các mã {string.Join(",", dupNameCodes)} đã tồn tại");
            }

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
                        ProductTypeId = productTypes[row.ProductTypeCode.NormalizeAsInternalName()],
                        ProductCateId = productCates[row.ProductCate.NormalizeAsInternalName()],
                        BarcodeConfigId = row.BarcodeConfigId,
                        BarcodeStandardId = null,
                        Barcode = row.Barcode,
                        UnitId = units[row.Unit.NormalizeAsInternalName()],
                        EstimatePrice = row.EstimatePrice,
                        GrossWeight = row.GrossWeight,
                        Height = row.Height,
                        Long = row.Long,
                        Width = row.Width,
                        LoadAbility = row.LoadAbility,
                        NetWeight = row.NetWeight,
                        PackingMethod = row.PackingMethod,
                        Measurement = row.Measurement,
                        Quantitative = row.Quantitative,
                        QuantitativeUnitTypeId = (int?)row.QuantitativeUnitTypeId,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                        IsDeleted = false,
                        ProductExtraInfo = new ProductExtraInfo()
                        {
                            Specification = row.Specification,
                            IsDeleted = false
                        },
                        ProductStockInfo = new ProductStockInfo()
                        {
                            StockOutputRuleId = (int?)row.StockOutputRuleId,
                            AmountWarningMin = row.AmountWarningMin,
                            AmountWarningMax = row.AmountWarningMax,
                            TimeWarningTimeTypeId = null,
                            TimeWarningAmount = null,
                            ExpireTimeTypeId = (int?)row.ExpireTimeTypeId,
                            ExpireTimeAmount = row.ExpireTimeAmount,
                            IsDeleted = false
                        },
                        ProductStockValidation = row.StockIds.Select(s => new ProductStockValidation
                        {
                            StockId = s
                        }).ToList()
                    };

                    var lstUnitConverions = new List<ProductUnitConversion>(){
                            new ProductUnitConversion()
                            {
                                ProductUnitConversionName = row.Unit,
                                SecondaryUnitId = units[row.Unit.NormalizeAsInternalName()],
                                FactorExpression = "1",
                                ConversionDescription = "Mặc định",
                                IsDefault = true,
                                IsFreeStyle = false
                            }
                        };

                    for (int suffix = 1; suffix <= 5; suffix++)
                    {
                        var unitText = suffix > 1 ? new StringBuilder(unit01Text).Remove(unit01Text.Length - 2, 2).Append($"0{suffix}").ToString() : unit01Text;
                        var expText = suffix > 1 ? new StringBuilder(exp01Text).Remove(exp01Text.Length - 2, 2).Append($"0{suffix}").ToString() : exp01Text;
                        var unit = typeInfo.GetProperty(unitText).GetValue(row) as string;
                        var exp = typeInfo.GetProperty(expText).GetValue(row) as string;
                        if (!string.IsNullOrEmpty(unit))
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
                                ProductUnitConversionName = unitText,
                                SecondaryUnitId = units[unit.NormalizeAsInternalName()],
                                FactorExpression = typeInfo.GetProperty(expText).GetValue(row) as string,
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
                trans.TryRollbackTransaction();
                throw ex;
            }

        }

        public List<EntityField> GetFields(Type type)
        {
            var fields = new List<EntityField>();

            foreach (var prop in type.GetProperties())
            {
                EntityField field = new EntityField
                {
                    FieldName = prop.Name,
                    Title = prop.GetCustomAttributes<System.ComponentModel.DataAnnotations.DisplayAttribute>().FirstOrDefault()?.Name ?? prop.Name,
                    Group = prop.GetCustomAttributes<System.ComponentModel.DataAnnotations.DisplayAttribute>().FirstOrDefault()?.GroupName
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
