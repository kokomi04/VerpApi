using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Stock.Product;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Product.Partial;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Inventory.Implement.Abstract;
using VErp.Services.Stock.Service.Products.Implement.ProductFacade;
using static Verp.Resources.Stock.Product.ProductValidationMessage;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using static VErp.Commons.GlobalObject.InternalDataInterface.Stock.ProductModel;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductService : PuConversionValidateAbstract, IProductService
    {
        public const int DECIMAL_PLACE_DEFAULT = 11;

        private readonly MasterDBContext _masterDBContext;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IAsyncRunnerService _asyncRunner;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IManufacturingHelperService _manufacturingHelperService;
        private readonly IMapper _mapper;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IBarcodeConfigHelperService _barcodeConfigHelperService;
        private readonly ILongTaskResourceLockService longTaskResourceLockService;
        private readonly ObjectActivityLogFacade _productActivityLog;

        public ProductService(
            StockDBContext stockContext
            , MasterDBContext masterDBContext
            , ILogger<ProductService> logger
            , IUnitService unitService
            , IActivityLogService activityLogService
            , IAsyncRunnerService asyncRunner
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IManufacturingHelperService manufacturingHelperService
            , IMapper mapper
            , IOrganizationHelperService organizationHelperService
            , IBarcodeConfigHelperService barcodeConfigHelperService
            , ILongTaskResourceLockService longTaskResourceLockService
            ) : base(stockContext)
        {
            _masterDBContext = masterDBContext;
            _logger = logger;
            _unitService = unitService;
            _asyncRunner = asyncRunner;
            _customGenCodeHelperService = customGenCodeHelperService;
            _manufacturingHelperService = manufacturingHelperService;
            _mapper = mapper;
            _organizationHelperService = organizationHelperService;
            _barcodeConfigHelperService = barcodeConfigHelperService;
            this.longTaskResourceLockService = longTaskResourceLockService;
            _productActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Product);
        }

        public async Task<bool> UpdateProductionProcessVersion(int productId)
        {
            var productInfo = await _stockDbContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }

            if (!productInfo.ProductionProcessVersion.HasValue)
                productInfo.ProductionProcessVersion = 1;
            else productInfo.ProductionProcessVersion += 1;

            await _stockDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<long> GetProductionProcessVersion(int productId)
        {
            var productInfo = await _stockDbContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }

            return productInfo.ProductionProcessVersion.GetValueOrDefault();
        }

        public async Task<int> AddProduct(ProductModel req)
        {
            //var customGenCode = await GenerateProductCode(null, req);
            var ctx = await GenerateProductCode(null, req);

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var productId = await AddProductToDb(req);
                await trans.CommitAsync();

                //await ConfirmProductCode(customGenCode);
                await ctx.ConfirmCode();

                await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.Create)
                      .MessageResourceFormatDatas(req.ProductCode)
                      .ObjectId(productId)
                      .JsonData(req.JsonSerialize())
                      .CreateLog();

                return productId;
            }
        }

        public async Task<ProductDefaultModel> ProductAddProductSemi(int parentProductId, ProductDefaultModel req)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var defaultProductType = await _stockDbContext.ProductType.FirstOrDefaultAsync(t => t.IsDefault);
                if (req.ProductTypeId == null)
                {
                    req.ProductTypeId = defaultProductType?.ProductTypeId;
                }
                //var customGenCode = await ProductGenerateProductSemiCode(parentProductId, null, req);
                var ctx = await ProductGenerateProductSemiCode(parentProductId, null, req);

                var defaultProductCate = _stockDbContext.ProductCate.FirstOrDefault(c => c.IsDefault);
                if (defaultProductCate == null)
                    throw DefaultProductCateNotFound.BadRequest();

                var unitInfo = await _unitService.GetUnitInfo(req.UnitId);
                if (unitInfo == null)
                {
                    throw UnitOfProductNotFound.BadRequestFormat(req.ProductCode);
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
                    IsProduct = false,
                    IsMaterials = false
                };

                await _stockDbContext.Product.AddAsync(productInfo);
                await _stockDbContext.SaveChangesAsync();

                var productStockInfo = new ProductStockInfo()
                {
                    ProductId = productInfo.ProductId,
                    StockOutputRuleId = (int)EnumStockOutputRule.None
                };

                await _stockDbContext.ProductStockInfo.AddAsync(productStockInfo);

                var productExtra = new ProductExtraInfo()
                {
                    ProductId = productInfo.ProductId,
                    Specification = req.Specification
                };

                await _stockDbContext.ProductExtraInfo.AddAsync(productExtra);
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
                _stockDbContext.ProductUnitConversion.Add(unitConverion);

                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();

                await ctx.ConfirmCode();// ConfirmProductCode(customGenCode);
                req.ProductCode = productInfo.ProductCode;
                req.ProductId = productInfo.ProductId;


                await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.CreateProductPart)
                      .MessageResourceFormatDatas(req.ProductCode)
                      .ObjectId(productInfo.ProductId)
                      .JsonData(req.JsonSerialize())
                      .CreateLog();
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

            var productExisted = await _stockDbContext.Product.FirstOrDefaultAsync(p => p.ProductCode == req.ProductCode);//|| p.ProductName == req.ProductName
            if (productExisted != null)
            {
                //if (string.Compare(productExisted.ProductCode, req.ProductCode, StringComparison.OrdinalIgnoreCase) == 0)                
                //throw new BadRequestException(ProductErrorCode.ProductNameAlreadyExisted, $"Tên mặt hàng \"{req.ProductName}\" đã tồn tại");
                throw ProductCodeAlreadyExisted.BadRequestFormat(req.ProductCode);
            }

            if (!await _stockDbContext.ProductCate.AnyAsync(c => c.ProductCateId == req.ProductCateId))
            {
                throw ProductCateNotFound.BadRequest();
            }

            //if (!await _stockContext.ProductType.AnyAsync(c => c.ProductTypeId == req.ProductTypeId))
            //{
            //    throw new BadRequestException(ProductErrorCode.ProductTypeInvalid, $"Loại sinh mã mặt hàng không đúng");
            //}

            if (req.IsMaterials == false && req.IsProduct == false && req.IsProductSemi == false)
            {
                req.IsProduct = true;
            }

            var productInfo = _mapper.Map<Product>(req);
            productInfo.ProductInternalName = req.ProductName.NormalizeAsInternalName();
            productInfo.ProductionProcessStatusId = (int)EnumProductionProcessStatus.NotCreatedYet;

            await _stockDbContext.Product.AddAsync(productInfo);

            await _stockDbContext.SaveChangesAsync();

            var productExtra = _mapper.Map<ProductExtraInfo>(req.Extra ?? new ProductModelExtra());
            productExtra.ProductId = productInfo.ProductId;

            await _stockDbContext.ProductExtraInfo.AddAsync(productExtra);


            var productStockInfo = _mapper.Map<ProductStockInfo>(req.StockInfo);
            productStockInfo.ProductId = productInfo.ProductId;

            await _stockDbContext.ProductStockInfo.AddAsync(productStockInfo);

            var lstStockValidations = req.StockInfo?.StockIds?.Select(s => new ProductStockValidation()
            {
                ProductId = productInfo.ProductId,
                StockId = s
            });

            if (lstStockValidations != null)
            {
                await _stockDbContext.ProductStockValidation.AddRangeAsync(lstStockValidations);
            }

            var unitInfo = await _unitService.GetUnitInfo(req.UnitId);
            if (unitInfo == null)
            {
                throw UnitOfProductNotFound.BadRequestFormat(req.ProductCode);
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
                u.ProductId = productInfo.ProductId;
                u.ProductUnitConversionId = 0;
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

            var duplicateUnit = lstUnitConverions.GroupBy(u => u.ProductUnitConversionName?.NormalizeAsInternalName())
                .Where(g => g.Count() > 1)
                .FirstOrDefault();
            if (duplicateUnit != null)
            {
                throw PuConversionDuplicated.BadRequestFormat(duplicateUnit.First()?.ProductUnitConversionName, req.ProductCode);
            }

            await _stockDbContext.ProductUnitConversion.AddRangeAsync(lstUnitConverions);

            var productCustomers = _mapper.Map<List<ProductCustomer>>(req.ProductCustomers);
            if (productCustomers == null)
            {
                productCustomers = new List<ProductCustomer>();
            }
            foreach (var c in productCustomers)
            {
                c.ProductId = productInfo.ProductId;
            }
            await _stockDbContext.ProductCustomer.AddRangeAsync(productCustomers);

            await _stockDbContext.SaveChangesAsync();

            if (req.MainImageFileId.HasValue)
            {
                _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.Product, productInfo.ProductId, req.MainImageFileId.Value));
            }

            return productInfo.ProductId;
        }

        public async Task<ProductModel> ProductInfo(int productId)
        {
            var productInfo = await _stockDbContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
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

            if (req.IsMaterials == false && req.IsProduct == false && req.IsProductSemi == false)
            {
                req.IsProduct = true;
            }

            req.ProductCode = (req.ProductCode ?? "").Trim();

            //var productExisted = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId != productId && p.ProductName == req.ProductName);
            //if (productExisted != null)
            //{
            //    throw new BadRequestException(ProductErrorCode.ProductNameAlreadyExisted);
            //}


            long? oldMainImageFileId = 0L;

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //var customGenCode = await GenerateProductCode(productId, req);
                    var ctx = await GenerateProductCode(productId, req);

                    //Getdata
                    var productInfo = await _stockDbContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                    if (productInfo == null)
                    {
                        throw new BadRequestException(ProductErrorCode.ProductNotFound);
                    }


                    /*
                    if (productInfo.UnitId != req.UnitId)
                    {
                        var isInUsed = new SqlParameter("@IsUsed", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var checkParams = new[]
                        {
                            new SqlParameter("@ProductId",productId),
                            isInUsed
                        };

                        await _stockDbContext.ExecuteStoreProcedure("asp_Product_CheckUsed", checkParams);

                        if (isInUsed.Value as bool? == true)
                        {
                            throw CanNotUpdateUnitProductWhichInUsed.BadRequestFormat(req.ProductCode);
                        }
                    }*/



                    var unitConverions = await _stockDbContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

                    var keepPuIds = req.StockInfo?.UnitConversions?.Select(c => c.ProductUnitConversionId).Where(productUnitConversionId => productUnitConversionId > 0)?.ToList();
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

                        await _stockDbContext.ExecuteStoreProcedure("asp_ProductUnitConversion_CheckUsed", checkParams);

                        if (isInUsed.Value as bool? == true)
                        {
                            trans.Rollback();
                            throw new BadRequestException(ProductErrorCode.SomeProductUnitConversionInUsed);
                        }

                        _stockDbContext.ProductUnitConversion.RemoveRange(toRemovePus);

                    }

                    oldMainImageFileId = productInfo.MainImageFileId;

                    var productExtra = await _stockDbContext.ProductExtraInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                    var productStockInfo = await _stockDbContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                    var stockValidations = await _stockDbContext.ProductStockValidation.Where(p => p.ProductId == productId).ToListAsync();
                    var productCustomers = await _stockDbContext.ProductCustomer.Where(p => p.ProductId == productId).ToListAsync();

                    //Update

                    //Productinfo
                    req.ProductionProcessVersion = productInfo.ProductionProcessVersion;

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

                    _stockDbContext.RemoveRange(stockValidations);
                    if (lstStockValidations != null)
                    {
                        await _stockDbContext.ProductStockValidation.AddRangeAsync(lstStockValidations);
                    }


                    var unitInfo = await _unitService.GetUnitInfo(req.UnitId);
                    if (unitInfo == null)
                    {
                        throw new BadRequestException(UnitErrorCode.UnitNotFound);
                    }

                    var lstNewUnitConverions = req.StockInfo?.UnitConversions?
                        .Where(c => c.ProductUnitConversionId <= 0)?
                        .Select(u => _mapper.Map<ProductUnitConversion>(u))
                        .ToList();

                    var newUnitConversionList = new List<ProductUnitConversion>();

                    if (lstNewUnitConverions != null)
                    {
                        foreach (var u in lstNewUnitConverions)
                        {
                            u.ProductId = productId;
                            u.ProductUnitConversionId = 0;
                        }

                        newUnitConversionList.AddRange(lstNewUnitConverions);

                        await _stockDbContext.ProductUnitConversion.AddRangeAsync(lstNewUnitConverions);
                    }

                    var changingPuRateIds = new List<long>();
                    foreach (var productUnitConversionId in keepPuIds)
                    {
                        var db = unitConverions.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);
                        var u = req.StockInfo?.UnitConversions?.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);
                        if (db != null && u != null)
                        {
                            if (u.FactorExpression?.Trim() != db.FactorExpression?.Trim())
                            {
                                changingPuRateIds.Add(db.ProductUnitConversionId);
                            }
                            _mapper.Map(u, db);
                        }

                        newUnitConversionList.Add(db);
                    }

                    await PuRateChangeValidateExistingInventoryData(changingPuRateIds);

                    var defaultUnitConversion = unitConverions.FirstOrDefault(c => c.IsDefault);
                    if (defaultUnitConversion != null)
                    {
                        defaultUnitConversion.SecondaryUnitId = req.UnitId;
                        defaultUnitConversion.IsDefault = true;
                        defaultUnitConversion.IsFreeStyle = false;
                        defaultUnitConversion.ProductUnitConversionName = unitInfo.UnitName;
                        defaultUnitConversion.DecimalPlace = req.StockInfo?.UnitConversions?.FirstOrDefault(u => u.ProductUnitConversionId == defaultUnitConversion.ProductUnitConversionId || u.IsDefault)?.DecimalPlace ?? DECIMAL_PLACE_DEFAULT;

                        if (!newUnitConversionList.Contains(defaultUnitConversion))
                            newUnitConversionList.Add(defaultUnitConversion);
                    }

                    var duplicateUnit = newUnitConversionList.GroupBy(u => u.ProductUnitConversionName?.NormalizeAsInternalName())
                   .Where(g => g.Count() > 1)
                   .FirstOrDefault();
                    if (duplicateUnit != null)
                    {
                        await trans.RollbackAsync();
                        throw PuConversionDuplicated.BadRequestFormat(duplicateUnit.First()?.ProductUnitConversionName, req.ProductCode);
                    }



                    if (req.ProductCustomers == null)
                    {
                        req.ProductCustomers = new List<ProductModelCustomer>();
                    }


                    if (req.ProductCustomers.GroupBy(c => c.CustomerId).Any(g => g.Count() > 1))
                    {
                        throw ExistMoreSameCustomerProduct.BadRequest();
                    }

                    var removeProductCustomers = productCustomers.Where(c => !req.ProductCustomers.Select(c1 => c.CustomerId).Contains(c.CustomerId));
                    _stockDbContext.ProductCustomer.RemoveRange(removeProductCustomers);

                    foreach (var c in req.ProductCustomers)
                    {
                        var existed = productCustomers.FirstOrDefault(c1 => c1.CustomerId == c.CustomerId);
                        if (existed != null)
                        {
                            _mapper.Map(c, existed);
                        }
                        else
                        {
                            var pCustomerentity = _mapper.Map<ProductCustomer>(c);
                            pCustomerentity.ProductId = productId;
                            await _stockDbContext.ProductCustomer.AddAsync(pCustomerentity);
                        }
                    }

                    await _stockDbContext.SaveChangesAsync();
                    trans.Commit();

                    await ctx.ConfirmCode();// ConfirmProductCode(customGenCode);

                    await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.Update)
                          .MessageResourceFormatDatas(req.ProductCode)
                          .ObjectId(productInfo.ProductId)
                          .JsonData(req.JsonSerialize())
                          .CreateLog();
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


        private async Task<IGenerateCodeContext> GenerateProductCode(int? productId, ProductGenCodeModel model)
        {
            model.ProductCode = (model.ProductCode ?? "").Trim();

            var productTypeInfo = await _stockDbContext.ProductType.FirstOrDefaultAsync(t => t.ProductTypeId == model.ProductTypeId);

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.Product, EnumObjectType.ProductType, model.ProductTypeId ?? 0, productTypeInfo?.ProductTypeName)
                .SetConfigData(productId ?? 0, null, productTypeInfo?.IdentityCode)
                .TryValidateAndGenerateCode(_stockDbContext.Product, model.ProductCode, (s, code) => s.ProductId != productId && s.ProductCode == code);

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


        private async Task<IGenerateCodeContext> ProductGenerateProductSemiCode(int parentProductId, int? productId, ProductGenCodeModel model)
        {
            model.ProductCode = (model.ProductCode ?? "").Trim();

            var parentProductInfo = await _stockDbContext.Product.FirstOrDefaultAsync(t => t.ProductId == parentProductId);


            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.Product)
                .SetConfigData(productId ?? 0, null, parentProductInfo?.ProductCode)
                .TryValidateAndGenerateCode(_stockDbContext.Product, model.ProductCode, (s, code) => s.ProductId != productId && s.ProductCode == code);

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
            var productInfo = await _stockDbContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }

            //var isInUsed = new SqlParameter("@IsUsed", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            //var checkParams = new[]
            //{
            //    new SqlParameter("@ProductId",productId),
            //    isInUsed
            //};

            //await _stockDbContext.ExecuteStoreProcedure("asp_Product_CheckUsed", checkParams);

            //var (usedProductId, msg) = await CheckProductIdsIsUsed(new List<int>() { productId });
            ////if (isInUsed.Value as bool? == true)
            //if (usedProductId.HasValue)
            //{
            //    throw ProductErrorCode.ProductInUsed.BadRequestFormat(CanNotDeleteProductWhichInUsed, msg);
            //}

            var productTopUsed = await GetProductTopInUsed(new List<int>() { productId }, false);
            if (productTopUsed.Count > 0)
            {
                throw GeneralCode.ItemInUsed.BadRequestFormatWithData(productTopUsed, CanNotDeleteProductWhichInUsed, productTopUsed.First().Description);
            }

            var productExtra = await _stockDbContext.ProductExtraInfo.FirstOrDefaultAsync(p => p.ProductId == productId);

            var productStockInfo = await _stockDbContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);

            var stockValidations = await _stockDbContext.ProductStockValidation.Where(p => p.ProductId == productId).ToListAsync();

            var unitConverions = await _stockDbContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

            var productBoms = await _stockDbContext.ProductBom.Where(b => b.ProductId == productId).ToListAsync();

            var productConsum = await _stockDbContext.ProductMaterialsConsumption.Where(b => b.ProductId == productId).ToListAsync();

            var materials = await _stockDbContext.ProductMaterial.Where(b => b.RootProductId == productId).ToListAsync();

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    productInfo.IsDeleted = true;
                    productInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    productExtra.IsDeleted = true;

                    productStockInfo.IsDeleted = true;

                    foreach (var p in productBoms)
                    {
                        p.IsDeleted = true;
                    }

                    foreach (var p in productConsum)
                    {
                        p.IsDeleted = true;
                    }

                    _stockDbContext.ProductMaterial.RemoveRange(materials);

                    //_stockContext.ProductStockValidation.RemoveRange(stockValidations);

                    // _stockContext.ProductUnitConversion.RemoveRange(unitConverions);

                    await _stockDbContext.SaveChangesAsync();
                    trans.Commit();


                    await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.Delete)
                          .MessageResourceFormatDatas(productInfo.ProductCode)
                          .ObjectId(productInfo.ProductId)
                          .JsonData(productInfo.JsonSerialize())
                          .CreateLog();
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

            var productUnitConversions = (await _stockDbContext.ProductUnitConversion
                .Where(c => productIds.Contains(c.ProductId) && productUnitConversionIds.Contains(c.ProductUnitConversionId))
                .ToListAsync())
                .ToDictionary(c => c.ProductUnitConversionId, c => c.ProductId);

            foreach (var item in productUnitConvertsionProduct)
            {
                if (!productUnitConversions.ContainsKey(item.Key) || productUnitConversions[item.Key] != item.Value) return false;
            }

            return true;
        }

        public async Task<PageData<ProductListOutput>> GetList(ProductFilterRequestModel req, int page, int size)
        {
            var keyword = (req.Keyword ?? "").Trim();
            var productName = (req.ProductName ?? "").Trim();

            var productInternalName = productName.NormalizeAsInternalName();

            var products = _stockDbContext.Product.AsQueryable();

            if (req.ProductIds != null && req.ProductIds.Count > 0)
            {
                products = products.Where(x => req.ProductIds.Contains(x.ProductId));
            }

            if (req.IsProductSemi.HasValue)
            {
                products = products.Where(x => x.IsProductSemi == req.IsProductSemi.Value);
            }

            if (req.IsProduct.HasValue)
            {
                products = products.Where(x => x.IsProduct == req.IsProduct.Value);
            }

            if (req.IsMaterials.HasValue)
            {
                products = products.Where(x => x.IsMaterials == req.IsMaterials.Value);
            }

            if (!string.IsNullOrWhiteSpace(productName))
            {
                products = products.Where(p => p.ProductInternalName == productInternalName);
            }

            if (req.StockIds?.Count > 0)
            {
                var productIdsInStocks = _stockDbContext.StockProduct.Where(s => req.StockIds.Contains(s.StockId)).Select(s => s.ProductId);
                products = products.Where(s => productIdsInStocks.Contains(s.ProductId));
            }
            var query = (
              from p in products
              join pe in _stockDbContext.ProductExtraInfo on p.ProductId equals pe.ProductId
              join s in _stockDbContext.ProductStockInfo on p.ProductId equals s.ProductId
              join pt in _stockDbContext.ProductType on p.ProductTypeId equals pt.ProductTypeId into pts
              from pt in pts.DefaultIfEmpty()
              join pc in _stockDbContext.ProductCate on p.ProductCateId equals pc.ProductCateId into pcs
              from pc in pcs.DefaultIfEmpty()
              select new
              {
                  p.ProductId,
                  p.ProductCode,
                  p.ProductName,
                  p.ProductNameEng,
                  p.MainImageFileId,
                  p.ProductTypeId,
                  ProductTypeCode = pt == null ? null : pt.IdentityCode,
                  ProductTypeName = pt == null ? null : pt.ProductTypeName,
                  p.ProductCateId,
                  ProductCateName = pc == null ? null : pc.ProductCateName,
                  p.BarcodeConfigId,
                  p.Barcode,
                  pe.Specification,
                  pe.Description,
                  p.UnitId,
                  p.EstimatePrice,
                  p.IsProductSemi,
                  p.Coefficient,
                  p.IsProduct,
                  p.IsMaterials,
                  p.Height,
                  p.Long,
                  p.Width,
                  p.CreatedDatetimeUtc,
                  p.ProductionProcessStatusId,
                  //p.CustomerId,

                  p.PackingMethod,
                  p.Quantitative,
                  p.QuantitativeUnitTypeId,

                  p.ProductPurity,

                  p.Measurement,
                  p.NetWeight,
                  p.GrossWeight,
                  p.LoadAbility,

                  p.PackingQuantitative,
                  p.PackingWidth,
                  p.PackingHeight,
                  p.PackingLong,

                  s.StockOutputRuleId,
                  s.AmountWarningMin,
                  s.AmountWarningMax,
                  s.ExpireTimeAmount,
                  s.ExpireTimeTypeId,

                  s.DescriptionToStock,

                  p.Color,
                  p.TargetProductivityId
              });


            if (req.ProductTypeIds != null && req.ProductTypeIds.Length > 0)
            {
                var types = req.ProductTypeIds.Select(t => (int?)t);
                query = from p in query
                        where types.Contains(p.ProductTypeId)
                        select p;
            }

            if (req.ProductCateIds != null && req.ProductCateIds.Count() > 0)
            {
                query = from p in query
                        where req.ProductCateIds.Contains(p.ProductCateId)
                        select p;
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from c in query
                        where
                        c.ProductCode.Contains(keyword)
                        || c.Barcode.Contains(keyword)
                        || c.ProductName.Contains(keyword)
                        || c.ProductNameEng.Contains(keyword)
                        || c.ProductTypeName.Contains(keyword)
                        || c.ProductCateName.Contains(keyword)
                        || c.Specification.Contains(keyword)
                        || c.Description.Contains(keyword)
                        || c.DescriptionToStock.Contains(keyword)
                        select c;
            }
            query = query.InternalFilter(req.Filters);



            var total = await query.CountAsync();
            var pagedQuery = query.OrderByDescending(p => p.CreatedDatetimeUtc).Skip((page - 1) * size).Take(size);
            var lstData = await pagedQuery.ToListAsync();


            var unitIds = lstData.Select(p => p.UnitId).ToList();
            var unitInfos = await _unitService.GetListByIds(unitIds);
            var barCodeConfigs = (await _barcodeConfigHelperService.GetList("")).List.ToDictionary(b => b.BarcodeConfigId, b => b);

            var dataProductIds = lstData.Select(p => p.ProductId).ToList();
            var productUnitConverions = (await _stockDbContext.ProductUnitConversion.Where(p => dataProductIds.Contains(p.ProductId)).ToListAsync())
                .GroupBy(p => p.ProductId)
                .ToDictionary(p => p.Key, p => p.ToList()); ;

            var pageData = new List<ProductListOutput>();

            IList<StockProduct> stockProductData = new List<StockProduct>();
            if (req.QuantityStockIds?.Count > 0)
            {
                var productIdsQuery = pagedQuery.Select(p => p.ProductId);

                stockProductData = await _stockDbContext.StockProduct.AsNoTracking()
                    .Where(q => req.QuantityStockIds.Contains(q.StockId) && productIdsQuery.Contains(q.ProductId))
                    .ToListAsync();
            }

            foreach (var item in lstData)
            {
                productUnitConverions.TryGetValue(item.ProductId, out var pus);
                var puDefault = pus.FirstOrDefault(p => p.IsDefault);

                var barcodeConfigId = item.BarcodeConfigId ?? 0;
                barCodeConfigs.TryGetValue(barcodeConfigId, out var barcodeConfig);
                var product = new ProductListOutput()
                {
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    ProductNameEng = item.ProductNameEng,
                    BarcodeConfigName = barcodeConfig?.Name,
                    Barcode = item.Barcode,
                    MainImageFileId = item.MainImageFileId,
                    ProductCateId = item.ProductCateId,
                    ProductCateName = item.ProductCateName,
                    ProductTypeId = item.ProductTypeId,
                    ProductTypeCode = item.ProductTypeCode,
                    ProductTypeName = item.ProductTypeName,
                    Specification = item.Specification,
                    UnitId = item.UnitId,
                    EstimatePrice = item.EstimatePrice,
                    IsProductSemi = item.IsProductSemi,
                    Coefficient = item.Coefficient,
                    IsProduct = item.IsProduct ?? false,
                    IsMaterials = item.IsMaterials ?? false,
                    ProductionProcessStatusId = (EnumProductionProcessStatus)item.ProductionProcessStatusId,
                    Long = item.Long,
                    Width = item.Width,
                    Height = item.Height,
                    //CustomerId = item.CustomerId,
                    PackingMethod = item.PackingMethod,
                    Quantitative = item.Quantitative,
                    QuantitativeUnitTypeId = (EnumQuantitativeUnitType?)item.QuantitativeUnitTypeId,
                    ProductPurity = item.ProductPurity,

                    PackingQuantitative = item.PackingQuantitative,
                    PackingHeight = item.PackingHeight,
                    PackingLong = item.PackingLong,
                    PackingWidth = item.PackingWidth,

                    Measurement = item.Measurement,
                    NetWeight = item.NetWeight,
                    GrossWeight = item.GrossWeight,
                    LoadAbility = item.LoadAbility,
                    StockOutputRuleId = (EnumStockOutputRule?)item.StockOutputRuleId,
                    AmountWarningMin = item.AmountWarningMin,
                    AmountWarningMax = item.AmountWarningMax,
                    ExpireTimeAmount = item.ExpireTimeAmount,
                    ExpireTimeTypeId = (EnumTimeType?)item.ExpireTimeTypeId,
                    DescriptionToStock = item.DescriptionToStock,
                    DecimalPlace = puDefault?.DecimalPlace ?? DECIMAL_PLACE_DEFAULT,
                    UnitName = puDefault?.ProductUnitConversionName,
                    ProductUnitConversions = _mapper.Map<List<ProductModelUnitConversion>>(pus),
                    Description = item.Description,
                    Color = item.Color,
                    TargetProductivityId = item.TargetProductivityId,

                    StockRemainings = stockProductData.Where(q => q.ProductId == item.ProductId).Select(q => new StockProductOutput
                    {
                        StockId = q.StockId,
                        ProductId = q.ProductId,
                        PrimaryUnitId = item.UnitId,
                        PrimaryQuantityRemaining = q.PrimaryQuantityRemaining.RoundBy(),
                        ProductUnitConversionId = q.ProductUnitConversionId,
                        ProductUnitConversionRemaining = q.ProductUnitConversionRemaining.RoundBy()
                    }).ToList()
                };

                var unitInfo = unitInfos.FirstOrDefault(u => u.UnitId == item.UnitId);

                product.UnitName = unitInfo?.UnitName;

                pageData.Add(product);
            }


            return (pageData, total);
        }

        public async Task<(Stream stream, string fileName, string contentType)> ExportList(ProductExportRequestModel req)
        {
            var lst = await GetList(req, 1, int.MaxValue);
            var bomExport = new ProductExportFacade(_stockDbContext, req.FieldNames);
            return await bomExport.Export(lst.List);
        }

        public async Task<IList<ProductListOutput>> GetListByIds(IList<int> productIds)
        {
            if (productIds == null || productIds.Count == 0) return new List<ProductListOutput>();
            var req = new ProductFilterRequestModel("", productIds, "", new int[0], new int[0], null, null, null, null);
            //var productList = await GetList("", productIds, "", new int[0], new int[0], 1, int.MaxValue, null, null, null, null);
            var productList = await GetList(req, 1, int.MaxValue);

            var pagedData = productList.List;

            var productIdList = pagedData.Select(p => p.ProductId).ToList();

            var stockProductData = await _stockDbContext.StockProduct.AsNoTracking().Where(q => productIdList.Contains(q.ProductId)).ToListAsync();

            foreach (var item in pagedData)
            {
                item.StockRemainings =
                    stockProductData.Where(q => q.ProductId == item.ProductId).Select(q => new StockProductOutput
                    {
                        StockId = q.StockId,
                        ProductId = q.ProductId,
                        PrimaryUnitId = item.UnitId,
                        PrimaryQuantityRemaining = q.PrimaryQuantityRemaining.RoundBy(),
                        ProductUnitConversionId = q.ProductUnitConversionId,
                        ProductUnitConversionRemaining = q.ProductUnitConversionRemaining.RoundBy()
                    }).ToList();
            }

            return pagedData;
        }

        public async Task<IList<ProductModel>> GetListProductsByIds(IList<int> productIds)
        {

            if (!(productIds?.Count > 0)) return new List<ProductModel>();

            var products = await _stockDbContext.Product.Where(p => productIds.Contains(p.ProductId)).ToListAsync();

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


            var products = await _stockDbContext.Product.AsNoTracking().Where(p => productCodes.Contains(p.ProductCode) || productInternalNames.Contains(p.ProductInternalName)).ToListAsync();

            return await EnrichToProductModel(products);
        }

        private async Task<IList<ProductModel>> EnrichToProductModel(IList<Product> products)
        {
            var productIds = products.Select(d => d.ProductId).ToList();

            var productExtraData = await _stockDbContext.ProductExtraInfo.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();
            var productStockInfoData = await _stockDbContext.ProductStockInfo.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();
            var stockValidationData = await _stockDbContext.ProductStockValidation.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();
            var unitConverionData = await _stockDbContext.ProductUnitConversion.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();
            var productCustomersData = await _stockDbContext.ProductCustomer.AsNoTracking().Where(p => productIds.Contains(p.ProductId)).ToListAsync();

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

                productModel.ProductionProcessStatusId = (EnumProductionProcessStatus)productInfo.ProductionProcessStatusId;
                result.Add(productModel);
            }

            return result;
        }

        public Task<bool> ImportProductFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            return new ProductImportFacade(_stockDbContext, _masterDBContext, _organizationHelperService, _productActivityLog, this)
                   .ImportProductFromMapping(longTaskResourceLockService, mapping, stream);

        }

        public CategoryNameModel GetFieldMappings()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "Product",
                CategoryTitle = ProductImportAsCateTitle,
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = ExcelUtils.GetFieldNameModels<ProductImportModel>();
            result.Fields = fields;
            return result;
        }

        private Enum ValidateProduct(ProductModel req)
        {
            if (string.IsNullOrWhiteSpace(req?.ProductCode))
            {
                return ProductErrorCode.ProductCodeEmpty;
            }

            if (string.IsNullOrWhiteSpace(req?.ProductName))
            {
                return ProductErrorCode.ProductNameEmpty;
            }

            if (req.StockInfo.UnitConversions?.Count > 0)
            {
                foreach (var unitConversion in req.StockInfo.UnitConversions)
                {
                    try
                    {
                        if (!unitConversion.IsDefault)
                        {
                            var eval = EvalUtils.EvalPrimaryQuantityFromProductUnitConversionQuantity(1, unitConversion.FactorExpression);
                            if (!(eval > 0))
                            {
                                return ProductErrorCode.InvalidUnitConversionExpression;
                            }
                        }
                        else
                        {
                            unitConversion.FactorExpression = "1";
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

        public async Task<bool> UpdateProductCoefficientManual(int productId, decimal coefficient)
        {
            var product = await _stockDbContext.Product.FirstOrDefaultAsync(x => x.ProductId == productId);
            if (product == null)
                throw new BadRequestException(ProductErrorCode.ProductNotFound);

            product.Coefficient = coefficient < 1 ? 1 : coefficient;

            await _stockDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> CopyProduct(ProductModel req, int sourceProductId)
        {
            var ctx = await GenerateProductCode(null, req);

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var productId = await AddProductToDb(req);

                    var parammeters = new[]
                    {
                        new SqlParameter("@SourceProductId", sourceProductId),
                        new SqlParameter("@DestProductId", productId),
                    };


                    await _stockDbContext.ExecuteStoreProcedure("asp_CopySourceProductIntoDestinationProduct", parammeters);


                    await trans.CommitAsync();

                    await ctx.ConfirmCode();


                    await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.Create)
                          .MessageResourceFormatDatas(req.ProductCode)
                          .ObjectId(productId)
                          .JsonData(req.JsonSerialize())
                          .CreateLog();

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

        public async Task<int> CopyProductBom(int sourceProductId, int destProductId)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var sourceProduct = await _stockDbContext.Product.Where(p => p.ProductId == sourceProductId).FirstOrDefaultAsync();

                    var desProduct = await _stockDbContext.Product.Where(p => p.ProductId == destProductId).FirstOrDefaultAsync();

                    if (sourceProduct == null) throw GeneralCode.ItemNotFound.BadRequest();

                    if (desProduct == null) throw GeneralCode.ItemNotFound.BadRequest();

                    var parammeters = new[]
                    {
                        new SqlParameter("@SourceProductId", sourceProductId),
                        new SqlParameter("@DestProductId", destProductId),
                    };

                    await _stockDbContext.ExecuteStoreProcedure("asp_CopyProductBom", parammeters);

                    //await _activityLogService.CreateLog(EnumObjectType.Product, destProductId, $"Sao chép BOM từ MH {sourceProductId} sang MH {destProductId}", destProductId.JsonSerialize());

                    await trans.CommitAsync();

                    var bom = await _stockDbContext.ProductBom.Where(b => b.ProductId == destProductId).ToListAsync();

                    await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.CopyBom)
                        .MessageResourceFormatDatas(sourceProduct.ProductCode, desProduct.ProductCode)
                        .ObjectId(destProductId)
                        .JsonData(bom.JsonSerialize())
                        .CreateLog();

                    return destProductId;
                }
                catch (Exception ex)
                {
                    await trans.TryRollbackTransactionAsync();
                    _logger.LogError("CopyProductBom", ex);
                    throw;
                }

            }
        }

        public async Task<int> CopyProductMaterialConsumption(int sourceProductId, int destProductId)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var sourceProduct = await _stockDbContext.Product.Where(p => p.ProductId == sourceProductId).FirstOrDefaultAsync();

                    var desProduct = await _stockDbContext.Product.Where(p => p.ProductId == destProductId).FirstOrDefaultAsync();

                    if (sourceProduct == null) throw GeneralCode.ItemNotFound.BadRequest();

                    if (desProduct == null) throw GeneralCode.ItemNotFound.BadRequest();

                    var parammeters = new[]
                    {
                        new SqlParameter("@SourceProductId", sourceProductId),
                        new SqlParameter("@DestProductId", destProductId),
                    };

                    await _stockDbContext.ExecuteStoreProcedure("asp_CopyProductMaterialConsumption", parammeters);

                    //await _activityLogService.CreateLog(EnumObjectType.Product, destProductId, $"Sao chép vật tư tiêu hao từ MH {sourceProductId} sang MH {destProductId}", destProductId.JsonSerialize());

                    await trans.CommitAsync();

                    var consum = await _stockDbContext.ProductMaterialsConsumption.Where(b => b.ProductId == destProductId).ToListAsync();

                    await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.CopyConsumption)
                        .MessageResourceFormatDatas(sourceProduct.ProductCode, desProduct.ProductCode)
                        .ObjectId(destProductId)
                        .JsonData(consum.JsonSerialize())
                        .CreateLog();

                    return destProductId;
                }
                catch (Exception ex)
                {
                    await trans.TryRollbackTransactionAsync();
                    _logger.LogError("CopyProductMaterialConsumption", ex);
                    throw;
                }

            }
        }

        /*
        public async Task<(int? productId, string msg)> CheckProductIdsIsUsed(List<int> productIds)
        {
            var outProductId = new SqlParameter("@OutProductId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var outMessage = new SqlParameter("@OutMessage", SqlDbType.NVarChar, 512) { Direction = ParameterDirection.Output };
            var checkParams = new[]
            {
                listProduct.ToSqlParameter("@ProductIds"),
                outProductId,
                outMessage
            };
            await _stockDbContext.ExecuteStoreProcedure("asp_Product_CheckUsed_ByList", checkParams);
            var msg = outMessage?.Value?.ToString();
            return (outProductId.Value as int?, msg);

            //var lst = await GetProductTopUsed(productIds, true);
            //if (lst.Any())
            //{
            //    return (lst[0].ProductId as int?, lst[0].Description);
            //}
            //return (null, null);
        }*/

        public async Task<IList<ObjectBillInUsedInfo>> GetProductTopInUsed(IList<int> productIds, bool isCheckExistOnly)
        {
            var checkParams = new[]
            {
                productIds.ToSqlParameter("@ProductIds"),
                new SqlParameter("@IsCheckExistOnly", SqlDbType.Bit){ Value  = isCheckExistOnly }
            };
            return await _stockDbContext.QueryListProc<ObjectBillInUsedInfo>("asp_Product_GetTopUsed_ByList", checkParams);
        }

        public async Task<bool> UpdateProductProcessStatus(int productId, EnumProductionProcessStatus enumProductionProcessStatus, bool isSaveLog =false)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var productInfo = await _stockDbContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (productInfo == null)
                {
                    throw new BadRequestException(ProductErrorCode.ProductNotFound);
                }

                productInfo.ProductionProcessStatusId = (int)enumProductionProcessStatus;

                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();
                if (isSaveLog)
                    await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.UpdateProcessInfo)
                  .MessageResourceFormatDatas(productInfo.ProductCode)
                  .ObjectId(productId)
                  .JsonData(productInfo.JsonSerialize())
                  .CreateLog();

                return true;
            }
        }

    }
}
