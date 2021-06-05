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
using Microsoft.Data.SqlClient;
using System.Data;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using AutoMapper;
using VErp.Services.Stock.Service.Products.Implement.ProductFacade;
using VErp.Services.Stock.Model.Product.Partial;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductService : IProductService
    {
        public const int DECIMAL_PLACE_DEFAULT = 11;

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
        private readonly IManufacturingHelperService _manufacturingHelperService;
        private readonly IMapper _mapper;
        private readonly IOrganizationHelperService _organizationHelperService;

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
            , IManufacturingHelperService manufacturingHelperService
            , IMapper mapper
            , IOrganizationHelperService organizationHelperService)
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
            _manufacturingHelperService = manufacturingHelperService;
            _mapper = mapper;
            _organizationHelperService = organizationHelperService;
        }

        public async Task<int> AddProduct(ProductModel req)
        {
            //var customGenCode = await GenerateProductCode(null, req);
            var ctx = await GenerateProductCode(null, req);

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                var productId = await AddProductToDb(req);
                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.Product, productId, $"Thêm mới mặt hàng {req.ProductName}", req.JsonSerialize());

                //await ConfirmProductCode(customGenCode);
                await ctx.ConfirmCode();

                return productId;
            }
        }

        public async Task<ProductDefaultModel> ProductAddProductSemi(int parentProductId, ProductDefaultModel req)
        {
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                var defaultProductType = await _stockContext.ProductType.FirstOrDefaultAsync(t => t.IsDefault);
                if (req.ProductTypeId == null)
                {
                    req.ProductTypeId = defaultProductType?.ProductTypeId;
                }
                //var customGenCode = await ProductGenerateProductSemiCode(parentProductId, null, req);
                var ctx = await ProductGenerateProductSemiCode(parentProductId, null, req);

                var defaultProductCate = _stockContext.ProductCate.FirstOrDefault(c => c.IsDefault);
                if (defaultProductCate == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Danh mục mặt hàng mặc định không tồn tại");
                var unitInfo = await _unitService.GetUnitInfo(req.UnitId);
                if (unitInfo == null)
                {
                    throw new BadRequestException(UnitErrorCode.UnitNotFound, $"Mặt hàng {req.ProductCode}, đơn vị tính không tìm thấy ");
                }

                var productInfo = new Product()
                {
                    ProductCode = req.ProductCode,
                    ProductName = req.ProductName ?? req.ProductCode,
                    ProductTypeId = req.ProductTypeId,
                    ProductInternalName = req.ProductName.NormalizeAsInternalName(),
                    IsCanBuy = false,
                    IsCanSell = false,
                    ProductCateId = defaultProductCate.ProductCateId,
                    UnitId = req.UnitId,
                    IsProductSemi = true,
                    Coefficient = 1,
                    IsProduct = false
                };

                await _stockContext.Product.AddAsync(productInfo);
                await _stockContext.SaveChangesAsync();

                var productStockInfo = new ProductStockInfo()
                {
                    ProductId = productInfo.ProductId,
                    StockOutputRuleId = (int)EnumStockOutputRule.None
                };

                await _stockContext.ProductStockInfo.AddAsync(productStockInfo);

                var productExtra = new ProductExtraInfo()
                {
                    ProductId = productInfo.ProductId,
                    Specification = req.Specification
                };

                await _stockContext.ProductExtraInfo.AddAsync(productExtra);
                var unitConverion = new ProductUnitConversion()
                {
                    ProductId = productInfo.ProductId,
                    ProductUnitConversionName = unitInfo.UnitName,
                    SecondaryUnitId = req.UnitId,
                    FactorExpression = "1",
                    ConversionDescription = "Mặc định",
                    IsDefault = true,
                    IsFreeStyle = false,
                    DecimalPlace = DECIMAL_PLACE_DEFAULT
                };
                _stockContext.ProductUnitConversion.Add(unitConverion);

                await _stockContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.Product, productInfo.ProductId, $"Thêm mới chi tiết mặt hàng {req.ProductName}", req.JsonSerialize());
                await ctx.ConfirmCode();// ConfirmProductCode(customGenCode);
                req.ProductCode = productInfo.ProductCode;
                req.ProductId = productInfo.ProductId;
                return req;
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

            var productExisted = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductCode == req.ProductCode);//|| p.ProductName == req.ProductName
            if (productExisted != null)
            {
                //if (string.Compare(productExisted.ProductCode, req.ProductCode, StringComparison.OrdinalIgnoreCase) == 0)
                throw new BadRequestException(ProductErrorCode.ProductCodeAlreadyExisted, $"Mã mặt hàng \"{req.ProductCode}\" đã tồn tại");
                //throw new BadRequestException(ProductErrorCode.ProductNameAlreadyExisted, $"Tên mặt hàng \"{req.ProductName}\" đã tồn tại");
            }

            if (!await _stockContext.ProductCate.AnyAsync(c => c.ProductCateId == req.ProductCateId))
            {
                throw new BadRequestException(ProductErrorCode.ProductCateInvalid, $"Danh mục mặt hàng không đúng");
            }

            //if (!await _stockContext.ProductType.AnyAsync(c => c.ProductTypeId == req.ProductTypeId))
            //{
            //    throw new BadRequestException(ProductErrorCode.ProductTypeInvalid, $"Loại sinh mã mặt hàng không đúng");
            //}

            var productInfo = _mapper.Map<Product>(req);
            productInfo.ProductInternalName = req.ProductName.NormalizeAsInternalName();

            await _stockContext.Product.AddAsync(productInfo);

            await _stockContext.SaveChangesAsync();

            var productExtra = _mapper.Map<ProductExtraInfo>(req.Extra ?? new ProductModelExtra());
            productExtra.ProductId = productInfo.ProductId;

            await _stockContext.ProductExtraInfo.AddAsync(productExtra);


            var productStockInfo = _mapper.Map<ProductStockInfo>(req.StockInfo);
            productStockInfo.ProductId = productInfo.ProductId;

            await _stockContext.ProductStockInfo.AddAsync(productStockInfo);

            var lstStockValidations = req.StockInfo?.StockIds?.Select(s => new ProductStockValidation()
            {
                ProductId = productInfo.ProductId,
                StockId = s
            });

            if (lstStockValidations != null)
            {
                await _stockContext.ProductStockValidation.AddRangeAsync(lstStockValidations);
            }

            var unitInfo = await _unitService.GetUnitInfo(req.UnitId);
            if (unitInfo == null)
            {
                throw new BadRequestException(UnitErrorCode.UnitNotFound, $"Mặt hàng {req.ProductCode}, đơn vị tính không tìm thấy ");
            }

            var lstUnitConverions = req.StockInfo?.UnitConversions?
                .Where(u => !u.IsDefault)?
                .Select(u => _mapper.Map<ProductUnitConversion>(u))
                .ToList();
            if (lstUnitConverions == null)
            {
                lstUnitConverions = new List<ProductUnitConversion>();
            }
            foreach (var u in lstUnitConverions)
            {
                u.DecimalPlace = u.DecimalPlace < 0 ? DECIMAL_PLACE_DEFAULT : u.DecimalPlace;
            }

            lstUnitConverions.Add(new ProductUnitConversion()
            {
                ProductId = productInfo.ProductId,
                ProductUnitConversionName = unitInfo.UnitName,
                SecondaryUnitId = req.UnitId,
                FactorExpression = "1",
                ConversionDescription = "Mặc định",
                IsDefault = true,
                IsFreeStyle = false,
                DecimalPlace = req.StockInfo?.UnitConversions?.FirstOrDefault(u => u.IsDefault)?.DecimalPlace ?? DECIMAL_PLACE_DEFAULT
            });

            await _stockContext.ProductUnitConversion.AddRangeAsync(lstUnitConverions);

            var productCustomers = _mapper.Map<List<ProductCustomer>>(req.ProductCustomers);
            if (productCustomers == null)
            {
                productCustomers = new List<ProductCustomer>();
            }
            foreach (var c in productCustomers)
            {
                c.ProductId = productInfo.ProductId;
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

            //var productExisted = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId != productId && p.ProductName == req.ProductName);
            //if (productExisted != null)
            //{
            //    throw new BadRequestException(ProductErrorCode.ProductNameAlreadyExisted);
            //}

            long? oldMainImageFileId = 0L;

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //var customGenCode = await GenerateProductCode(productId, req);
                    var ctx = await GenerateProductCode(productId, req);

                    //Getdata
                    var productInfo = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                    if (productInfo == null)
                    {
                        throw new BadRequestException(ProductErrorCode.ProductNotFound);
                    }



                    var unitConverions = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

                    var keepPuIds = req.StockInfo?.UnitConversions?.Select(c => c.ProductUnitConversionId);
                    var toRemovePus = unitConverions.Where(c => !keepPuIds.Contains(c.ProductUnitConversionId) && !c.IsDefault).ToList();
                    if (toRemovePus.Count > 0)
                    {
                        var removeConversionIds = toRemovePus.Select(c => c.ProductUnitConversionId).ToList();

                        var isInUsed = new SqlParameter("@IsUsed", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var checkParams = new[]
                        {
                            removeConversionIds.ToSqlParameter("@ProductUnitConverionIds"),
                            isInUsed
                        };

                        await _stockContext.ExecuteStoreProcedure("asp_ProductUnitConversion_CheckUsed", checkParams);

                        if (isInUsed.Value as bool? == true)
                        {
                            trans.Rollback();
                            throw new BadRequestException(ProductErrorCode.SomeProductUnitConversionInUsed);
                        }

                        _stockContext.ProductUnitConversion.RemoveRange(toRemovePus);

                    }

                    oldMainImageFileId = productInfo.MainImageFileId;

                    var productExtra = await _stockContext.ProductExtraInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                    var productStockInfo = await _stockContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                    var stockValidations = await _stockContext.ProductStockValidation.Where(p => p.ProductId == productId).ToListAsync();
                    var productCustomers = await _stockContext.ProductCustomer.Where(p => p.ProductId == productId).ToListAsync();

                    //Update

                    //Productinfo
                    _mapper.Map(req, productInfo);

                    productInfo.ProductInternalName = req.ProductName.NormalizeAsInternalName();
                    productInfo.Coefficient = req.Coefficient < 1 ? 1 : req.Coefficient;

                    //Product extra info
                    _mapper.Map(req.Extra, productExtra);


                    //Product stock info
                    _mapper.Map(req.StockInfo, productStockInfo);

                    var lstStockValidations = req.StockInfo?.StockIds?
                        .Select(s => new ProductStockValidation()
                        {
                            ProductId = productInfo.ProductId,
                            StockId = s
                        });

                    _stockContext.RemoveRange(stockValidations);
                    if (lstStockValidations != null)
                    {
                        await _stockContext.ProductStockValidation.AddRangeAsync(lstStockValidations);
                    }


                    var unitInfo = await _unitService.GetUnitInfo(req.UnitId);
                    if (unitInfo == null)
                    {
                        throw new BadRequestException(UnitErrorCode.UnitNotFound);
                    }

                    var lstNewUnitConverions = req.StockInfo?.UnitConversions?
                        .Where(c => c.ProductUnitConversionId <= 0)?
                        .Select(u => _mapper.Map<ProductUnitConversion>(u));

                    if (lstNewUnitConverions != null)
                    {
                        await _stockContext.ProductUnitConversion.AddRangeAsync(lstNewUnitConverions);
                    }

                    foreach (var productUnitConversionId in keepPuIds)
                    {
                        var db = unitConverions.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);
                        var u = req.StockInfo?.UnitConversions?.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);
                        if (db != null && u != null)
                        {
                            _mapper.Map(u, db);
                        }
                    }
                    var defaultUnitConversion = unitConverions.FirstOrDefault(c => c.IsDefault);
                    if (defaultUnitConversion != null)
                    {
                        defaultUnitConversion.SecondaryUnitId = req.UnitId;
                        defaultUnitConversion.IsDefault = true;
                        defaultUnitConversion.IsFreeStyle = false;
                        defaultUnitConversion.ProductUnitConversionName = unitInfo.UnitName;
                        defaultUnitConversion.DecimalPlace = req.StockInfo?.UnitConversions?.FirstOrDefault(u => u.ProductUnitConversionId == defaultUnitConversion.ProductUnitConversionId || u.IsDefault)?.DecimalPlace ?? DECIMAL_PLACE_DEFAULT;
                    }
                    if (req.ProductCustomers == null)
                    {
                        req.ProductCustomers = new List<ProductModelCustomer>();
                    }


                    if (req.ProductCustomers.GroupBy(c => c.CustomerId).Any(g => g.Count() > 1))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Tồn tại nhiều hơn 1 thiết lập cho 1 khách hàng!");
                    }

                    var removeProductCustomers = productCustomers.Where(c => !req.ProductCustomers.Select(c1 => c.CustomerId).Contains(c.CustomerId));
                    _stockContext.ProductCustomer.RemoveRange(removeProductCustomers);

                    foreach (var c in req.ProductCustomers)
                    {
                        var existed = productCustomers.FirstOrDefault(c1 => c1.CustomerId == c.CustomerId);
                        if (existed != null)
                        {
                            _mapper.Map(c, existed);
                        }
                        else
                        {
                            await _stockContext.ProductCustomer.AddAsync(_mapper.Map<ProductCustomer>(c));
                        }
                    }

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();


                    await _activityLogService.CreateLog(EnumObjectType.Product, productInfo.ProductId, $"Cập nhật mặt hàng {productInfo.ProductName}", req.JsonSerialize());

                    await ctx.ConfirmCode();// ConfirmProductCode(customGenCode);
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


        private async Task<GenerateCodeContext> GenerateProductCode(int? productId, ProductGenCodeModel model)
        {
            model.ProductCode = (model.ProductCode ?? "").Trim();

            var productTypeInfo = await _stockContext.ProductType.FirstOrDefaultAsync(t => t.ProductTypeId == model.ProductTypeId);

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.Product, EnumObjectType.ProductType, model.ProductTypeId ?? 0)
                .SetConfigData(productId ?? 0, null, productTypeInfo?.IdentityCode)
                .TryValidateAndGenerateCode(_stockContext.Product, model.ProductCode, (s, code) => s.ProductId != productId && s.ProductCode == code);

            model.ProductCode = code;

            return ctx;

            /*
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
            }*/
        }


        private async Task<GenerateCodeContext> ProductGenerateProductSemiCode(int parentProductId, int? productId, ProductGenCodeModel model)
        {
            model.ProductCode = (model.ProductCode ?? "").Trim();

            var parentProductInfo = await _stockContext.Product.FirstOrDefaultAsync(t => t.ProductId == parentProductId);


            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.Product, EnumObjectType.Product, 0)
                .SetConfigData(productId ?? 0, null, parentProductInfo?.ProductCode)
                .TryValidateAndGenerateCode(_stockContext.Product, model.ProductCode, (s, code) => s.ProductId != productId && s.ProductCode == code);

            model.ProductCode = code;

            return ctx;

            /*
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
                var parentProductInfo = await _stockContext.Product.FirstOrDefaultAsync(t => t.ProductId == parentProductId);

                var config = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.Product, EnumObjectType.Product, 0, productId, parentProductInfo?.ProductCode, _currentContextService.GetNowUtc().GetUnix());
                if (config == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết lập cấu hình sinh mã cho chi tiết mặt hàng");

                customGenCodeId = config.CustomGenCodeId;
                int dem = 0;
                do
                {
                    model.ProductCode = (await _customGenCodeHelperService.GenerateCode(config.CustomGenCodeId, config.CurrentLastValue.LastValue, productId, parentProductInfo?.ProductCode, _currentContextService.GetNowUtc().GetUnix()))?.CustomCode;
                    existedItem = await _stockContext.Product.FirstOrDefaultAsync(r => r.ProductCode == model.ProductCode && r.ProductId != productId);
                    dem++;
                } while (existedItem != null && dem < 10);
                return config.CurrentLastValue;
            }*/
        }

        //private async Task<bool> ConfirmProductCode(CustomGenCodeBaseValueModel customGenCodeBaseValue)
        //{
        //    if (customGenCodeBaseValue.IsNullObject()) return true;

        //    return await _customGenCodeHelperService.ConfirmCode(customGenCodeBaseValue);
        //}

        public async Task<bool> DeleteProduct(int productId)
        {
            var productInfo = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }

            var isInUsed = new SqlParameter("@IsUsed", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var checkParams = new[]
            {
                new SqlParameter("@ProductId",productId),
                isInUsed
            };

            await _stockContext.ExecuteStoreProcedure("asp_Product_CheckUsed", checkParams);

            if (isInUsed.Value as bool? == true)
            {
                throw new BadRequestException(ProductErrorCode.ProductInUsed, "Không thể xóa mặt hàng do mặt hàng đang được sử dụng");
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

                    await _activityLogService.CreateLog(EnumObjectType.Product, productInfo.ProductId, $"Xóa mặt hàng {productInfo.ProductName}", productInfo.JsonSerialize());

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



        public async Task<PageData<ProductListOutput>> GetList(string keyword, IList<int> productIds, string productName, int[] productTypeIds, int[] productCateIds, int page, int size, bool? isProductSemi, bool? isProduct, Clause filters = null)
        {
            var productInternalName = productName.NormalizeAsInternalName();

            var products = _stockContext.Product.AsQueryable();

            if (productIds != null && productIds.Count > 0)
            {
                products = products.Where(x => productIds.Contains(x.ProductId));
            }

            if (isProductSemi.HasValue && isProduct.HasValue)
            {
                products = products.Where(x => x.IsProductSemi == isProductSemi || x.IsProduct == isProduct);
            }
            else
            {

                if (isProductSemi.HasValue)
                {
                    products = products.Where(x => x.IsProductSemi == isProductSemi);
                }
                if (isProduct.HasValue)
                {
                    products = products.Where(x => x.IsProduct == isProduct);
                }
            }


            products = products.InternalFilter(filters);
            if (!string.IsNullOrWhiteSpace(productName))
            {
                products = products.Where(p => p.ProductInternalName == productInternalName);
            }

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
                  p.EstimatePrice,
                  p.IsProductSemi,
                  p.Coefficient,
                  p.IsProduct,
                  p.Height,
                  p.Long,
                  p.Width
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
                    EstimatePrice = item.EstimatePrice,
                    IsProductSemi = item.IsProductSemi,
                    Coefficient = item.Coefficient,
                    IsProduct = item.IsProduct ?? false,
                    Long = item.Long,
                    Width = item.Width,
                    Height = item.Height,
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
                join ucs in _stockContext.ProductUnitConversion on new { p.ProductId, p.UnitId } equals new { ucs.ProductId, UnitId = ucs.SecondaryUnitId } into gucs
                from ucs in gucs.DefaultIfEmpty()
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
                    p.EstimatePrice,
                    p.IsProductSemi,
                    p.Coefficient,
                    p.IsProduct,
                    p.Height,
                    p.Long,
                    p.Width,
                    ucs.ProductUnitConversionId,
                    ucs.DecimalPlace
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
                    IsProductSemi = item.IsProductSemi,
                    IsProduct = item.IsProduct ?? false,
                    Coefficient = item.Coefficient,
                    StockProductModelList = stockProductData.Where(q => q.ProductId == item.ProductId).Select(q => new StockProductOutput
                    {
                        StockId = q.StockId,
                        ProductId = q.ProductId,
                        PrimaryUnitId = item.UnitId,
                        PrimaryQuantityRemaining = q.PrimaryQuantityRemaining,
                        ProductUnitConversionId = q.ProductUnitConversionId,
                        ProductUnitConversionRemaining = q.ProductUnitConversionRemaining
                    }).ToList(),
                    Long = item.Long,
                    Width = item.Width,
                    Height = item.Height,
                    DecimalPlace = item.DecimalPlace
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
            var productCustomersData = await _stockContext.ProductCustomer.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();

            var result = new List<ProductModel>();
            foreach (var productInfo in products)
            {
                var productExtra = productExtraData.FirstOrDefault(p => p.ProductId == productInfo.ProductId);
                var productStockInfo = productStockInfoData.FirstOrDefault(p => p.ProductId == productInfo.ProductId);
                var stockValidations = stockValidationData.Where(p => p.ProductId == productInfo.ProductId);
                var unitConverions = unitConverionData.Where(p => p.ProductId == productInfo.ProductId);
                var productCustomers = productCustomersData.Where(p => p.ProductId == productInfo.ProductId);
                var productModel = _mapper.Map<ProductModel>(productInfo);
                productModel.IsProduct = productInfo.IsProduct ?? false;

                productModel.Extra = _mapper.Map<ProductModelExtra>(productExtra);

                productModel.StockInfo = _mapper.Map<ProductModelStock>(productStockInfo);
                if (productModel.StockInfo == null)
                {
                    productModel.StockInfo = new ProductModelStock();
                }

                productModel.StockInfo.StockIds = stockValidations?.Select(s => s.StockId).ToList();
                productModel.StockInfo.UnitConversions = _mapper.Map<List<ProductModelUnitConversion>>(unitConverions);

                productModel.ProductCustomers = _mapper.Map<List<ProductModelCustomer>>(productCustomers);

                result.Add(productModel);
            }

            return result;
        }

        public Task<bool> ImportProductFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            return new ProductImportFacade(_stockContext, _masterDBContext, _organizationHelperService)
                   .ImportProductFromMapping(mapping, stream);

        }

        public CategoryNameModel GetFieldMappings()
        {
            var result = new CategoryNameModel()
            {
                CategoryId = 1,
                CategoryCode = "Product",
                CategoryTitle = "Mặt hàng",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = Utils.GetFieldNameModels<ProductImportModel>();
            result.Fields = fields;
            return result;
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

        public async Task<bool> UpdateProductCoefficientManual(int productId, int coefficient)
        {
            var product = await _stockContext.Product.FirstOrDefaultAsync(x => x.ProductId == productId);
            if (product == null)
                throw new BadRequestException(ProductErrorCode.ProductNotFound);

            product.Coefficient = coefficient < 1 ? 1 : coefficient;

            await _stockContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> CopyProduct(ProductModel req, int sourceProductId)
        {
            var ctx = await GenerateProductCode(null, req);

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var productId = await AddProductToDb(req);

                    var parammeters = new[]
                    {
                        new SqlParameter("@SourceProductId", sourceProductId),
                        new SqlParameter("@DestProductId", productId),
                    };

                    await _stockContext.ExecuteStoreProcedure("asp_CopySourceProductInfoDestinationProduct", parammeters);

                    await _activityLogService.CreateLog(EnumObjectType.Product, productId, $"Thêm mới mặt hàng {req.ProductName}", req.JsonSerialize());
                    await ctx.ConfirmCode();

                    await trans.CommitAsync();

                    await _manufacturingHelperService.CopyProductionProcess(EnumContainerType.Product, sourceProductId, productId);
                    return productId;
                }
                catch (Exception ex)
                {
                    await trans.TryRollbackTransactionAsync();
                    _logger.LogError("CopyProduct", ex);
                    throw;
                }

            }
        }
    }
}
