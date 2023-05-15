using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Stock.Product;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product.Partial;
using VErp.Services.Stock.Service.Inventory.Implement.Abstract;
using static Verp.Resources.Stock.Product.ProductValidationMessage;
using static VErp.Commons.GlobalObject.InternalDataInterface.ProductModel;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductPartialService : PuConversionValidateAbstract, IProductPartialService
    {
        public const int DECIMAL_PLACE_DEFAULT = 11;

        private readonly IUnitService _unitService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _productActivityLog;

        public ProductPartialService(
            StockDBContext stockContext
            , IUnitService unitService
            , IProductService productService
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(stockContext)
        {
            _unitService = unitService;
            _productService = productService;
            _mapper = mapper;
            _productActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Product);
        }



        public async Task<ProductPartialGeneralModel> GeneralInfo(int productId)
        {
            var productInfo = await _stockDbContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }

            var stockInfo = await _stockDbContext.ProductStockInfo.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            var extraInfo = await _stockDbContext.ProductExtraInfo.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            return new ProductPartialGeneralModel()
            {
                ProductTypeId = productInfo.ProductTypeId,

                ProductCode = productInfo.ProductCode,

                ProductName = productInfo.ProductName,

                ProductNameEng = productInfo.ProductNameEng,

                MainImageFileId = productInfo.MainImageFileId,

                Long = productInfo.Long,
                Width = productInfo.Width,
                Height = productInfo.Height,

                Color = productInfo.Color,

                UnitId = productInfo.UnitId,

                ProductCateId = productInfo.ProductCateId,

                ExpireTimeAmount = stockInfo.ExpireTimeAmount,
                ExpireTimeTypeId = (EnumTimeType?)stockInfo.ExpireTimeTypeId,

                Description = extraInfo.Description,

                BarcodeConfigId = productInfo.BarcodeConfigId,
                BarcodeStandardId = (EnumBarcodeStandard?)productInfo.BarcodeStandardId,
                Barcode = productInfo.Barcode,

                Quantitative = productInfo.Quantitative,
                QuantitativeUnitTypeId = (EnumQuantitativeUnitType?)productInfo.QuantitativeUnitTypeId,

                ProductPurity = productInfo.ProductPurity,

                Specification = extraInfo.Specification,

                EstimatePrice = productInfo.EstimatePrice,

                IsProductSemi = productInfo.IsProductSemi,
                IsProduct = productInfo.IsProduct,

                IsMaterials = productInfo.IsMaterials,
                TargetProductivityId = productInfo.TargetProductivityId,
                UpdatedDatetimeUtc = productInfo.UpdatedDatetimeUtc.GetUnix(),
            };
        }

        public async Task<bool> UpdateGeneralInfo(int productId, ProductPartialGeneralUpdateWithExtraModel model)
        {
            model.ProductCode = (model.ProductCode ?? "").Trim();

            if (await _stockDbContext.Product.AnyAsync(p => p.ProductId != productId && p.ProductCode == model.ProductCode))
            {
                throw ProductCodeAlreadyExisted.BadRequestFormat(model.ProductCode);
            }

            var unitInfo = await _unitService.GetUnitInfo(model.UnitId);
            if (unitInfo == null)
            {
                throw UnitOfProductNotFound.BadRequestFormat(model.ProductCode);
            }
            using (var batchLog = _productActivityLog.BeginBatchLog())
            {
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {

                    var productInfo = await _stockDbContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                    if (productInfo == null)
                    {
                        throw new BadRequestException(ProductErrorCode.ProductNotFound);
                    }

                    if (model.UpdatedDatetimeUtc != productInfo.UpdatedDatetimeUtc.GetUnix())
                    {
                        throw GeneralCode.DataIsOld.BadRequest();
                    }



                    if (model.ConfirmFlag != true && productInfo.UnitId != model.UnitId)
                    {
                        var productTopUsed = await _productService.GetProductTopInUsed(new List<int>() { productId }, false);
                        if (productTopUsed.Count > 0)
                        {
                            throw ProductErrorCode.ProductInUsed.BadRequestFormatWithData(productTopUsed, CanNotUpdateUnitProductWhichInUsed, productInfo.ProductCode + " " + productTopUsed.First().Description);
                        }
                    }

                    /*
                    if (productInfo.UnitId != model.UnitId)
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
                            throw CanNotUpdateUnitProductWhichInUsed.BadRequestFormat(model.ProductCode);
                        }
                    }
                    */

                    if (model.IsMaterials == false && model.IsProduct == false && model.IsProductSemi == false)
                    {
                        model.IsProduct = true;
                    }

                    var defaultPuConversion = await _stockDbContext.ProductUnitConversion.FirstOrDefaultAsync(pu => pu.IsDefault && pu.ProductId == productId);

                    if (defaultPuConversion == null)
                    {
                        throw DefaultProductUnitNotFound.BadRequest();
                    }

                    var stockInfo = await _stockDbContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                    var extraInfo = await _stockDbContext.ProductExtraInfo.FirstOrDefaultAsync(p => p.ProductId == productId);

                    productInfo.ProductTypeId = model.ProductTypeId;

                    productInfo.ProductCode = model.ProductCode;

                    productInfo.ProductName = model.ProductName;

                    productInfo.ProductNameEng = model.ProductNameEng;

                    productInfo.MainImageFileId = model.MainImageFileId;

                    productInfo.Long = model.Long;
                    productInfo.Width = model.Width;
                    productInfo.Height = model.Height;

                    productInfo.Color = model.Color;


                    defaultPuConversion.SecondaryUnitId = model.UnitId;
                    defaultPuConversion.ProductUnitConversionName = unitInfo.UnitName;
                    defaultPuConversion.DecimalPlace = productInfo.UnitId != model.UnitId ? unitInfo.DecimalPlace : defaultPuConversion.DecimalPlace;

                    productInfo.UnitId = model.UnitId;

                    productInfo.ProductCateId = model.ProductCateId;

                    stockInfo.ExpireTimeAmount = model.ExpireTimeAmount;
                    stockInfo.ExpireTimeTypeId = (int?)model.ExpireTimeTypeId;

                    extraInfo.Description = model.Description;

                    productInfo.BarcodeConfigId = model.BarcodeConfigId;
                    productInfo.BarcodeStandardId = (int?)model.BarcodeStandardId;
                    productInfo.Barcode = model.Barcode;

                    productInfo.Quantitative = model.Quantitative;
                    productInfo.QuantitativeUnitTypeId = (int?)model.QuantitativeUnitTypeId;

                    productInfo.ProductPurity = model.ProductPurity;

                    extraInfo.Specification = model.Specification;

                    productInfo.EstimatePrice = model.EstimatePrice;

                    productInfo.IsProductSemi = model.IsProductSemi;

                    productInfo.IsProduct = model.IsProduct;

                    productInfo.IsMaterials = model.IsMaterials;
                    productInfo.TargetProductivityId = model.TargetProductivityId;

                    if (model.ProductTargetProductivities != null)
                    {
                        var productInfos = await _stockDbContext.Product.Where(p => model.ProductTargetProductivities.Select(t => t.ProductId).Distinct().Contains(p.ProductId)).ToListAsync();
                        foreach (var p in model.ProductTargetProductivities)
                        {
                            var pInfo = productInfos.FirstOrDefault(inf => inf.ProductId == p.ProductId);
                            if (pInfo != null && pInfo.TargetProductivityId != p.TargetProductivityId)
                            {
                                pInfo.TargetProductivityId = p.TargetProductivityId;

                                await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.UpdateProductProducitity)
                                  .MessageResourceFormatDatas(pInfo.ProductCode, productInfo.ProductCode)
                                  .ObjectId(p.ProductId)
                                  .JsonData(p.JsonSerialize())
                                  .CreateLog();

                            }
                        }
                    }

                    if (_stockDbContext.HasChanges())
                        productInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    await _stockDbContext.SaveChangesAsync();

                    await trans.CommitAsync();


                    await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.UpdateGeneralInfo)
                          .MessageResourceFormatDatas(productInfo.ProductCode)
                          .ObjectId(productId)
                          .JsonData(model.JsonSerialize())
                          .CreateLog();

                }

                await batchLog.CommitAsync();

                return true;
            }
        }


        public async Task<ProductPartialStockModel> StockInfo(int productId)
        {
            var productInfo = await _stockDbContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }

            var stockInfo = await _stockDbContext.ProductStockInfo.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            var stockValidation = await _stockDbContext.ProductStockValidation.AsNoTracking().Where(p => p.ProductId == productId).ToListAsync();
            var pus = await _stockDbContext.ProductUnitConversion.AsNoTracking().Where(p => p.ProductId == productId).ToListAsync();
            return new ProductPartialStockModel()
            {
                StockIds = stockValidation.Select(s => s.StockId).ToList(),

                AmountWarningMin = stockInfo.AmountWarningMin,
                AmountWarningMax = stockInfo.AmountWarningMax,
                DescriptionToStock = stockInfo.DescriptionToStock,

                StockOutputRuleId = (EnumStockOutputRule?)stockInfo.StockOutputRuleId,

                UnitConversions = _mapper.Map<List<ProductModelUnitConversion>>(pus),

                UpdatedDatetimeUtc = productInfo.UpdatedDatetimeUtc.GetUnix()
            };
        }

        public async Task<bool> UpdateStockInfo(int productId, ProductPartialStockModel model)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var productInfo = await _stockDbContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (productInfo == null)
                {
                    throw new BadRequestException(ProductErrorCode.ProductNotFound);
                }

                if (model.UpdatedDatetimeUtc != productInfo.UpdatedDatetimeUtc.GetUnix())
                {
                    throw GeneralCode.DataIsOld.BadRequest();
                }

                var stockInfo = await _stockDbContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                var stockValidation = await _stockDbContext.ProductStockValidation.Where(p => p.ProductId == productId).ToListAsync();
                var pus = await _stockDbContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

                var lstStockValidations = model?.StockIds?
                        .Select(s => new ProductStockValidation()
                        {
                            ProductId = productInfo.ProductId,
                            StockId = s
                        });

                _stockDbContext.RemoveRange(stockValidation);
                if (lstStockValidations != null)
                {
                    await _stockDbContext.ProductStockValidation.AddRangeAsync(lstStockValidations);
                }


                stockInfo.AmountWarningMin = model.AmountWarningMin;
                stockInfo.AmountWarningMax = model.AmountWarningMax;
                stockInfo.DescriptionToStock = model.DescriptionToStock;

                stockInfo.StockOutputRuleId = (int?)model.StockOutputRuleId;

                var unitConverions = await _stockDbContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

                var keepPuIds = model.UnitConversions?.Select(c => c.ProductUnitConversionId)?.Where(productUnitConversionId => productUnitConversionId > 0)?.ToList();
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


                var unitInfo = await _unitService.GetUnitInfo(productInfo.UnitId);
                if (unitInfo == null)
                {
                    throw new BadRequestException(UnitErrorCode.UnitNotFound);
                }

                var lstNewUnitConverions = model?.UnitConversions?
                    .Where(c => c.ProductUnitConversionId <= 0)?
                    .Select(u => _mapper.Map<ProductUnitConversion>(u))
                    .ToList();

                var newUnitConversionList = new List<ProductUnitConversion>();
                if (lstNewUnitConverions != null)
                {
                    foreach (var c in lstNewUnitConverions)
                    {
                        c.ProductId = productId;
                        c.ProductUnitConversionId = 0;

                        ValidatePu(c);
                    }
                    newUnitConversionList.AddRange(lstNewUnitConverions);
                    await _stockDbContext.ProductUnitConversion.AddRangeAsync(lstNewUnitConverions);
                }

                var changingPuRateIds = new List<long>();

                foreach (var productUnitConversionId in keepPuIds)
                {
                    var db = unitConverions.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);
                    var u = model?.UnitConversions?.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);
                    if (db != null && u != null)
                    {
                        if (u.FactorExpression?.Trim() != db.FactorExpression?.Trim())
                        {
                            changingPuRateIds.Add(db.ProductUnitConversionId);
                        }

                        _mapper.Map(u, db);
                        ValidatePu(db);
                    }
                    newUnitConversionList.Add(db);
                }

                await PuRateChangeValidateExistingInventoryData(changingPuRateIds);

                var defaultUnitConversion = unitConverions.FirstOrDefault(c => c.IsDefault);
                if (defaultUnitConversion != null)
                {
                    defaultUnitConversion.SecondaryUnitId = productInfo.UnitId;
                    defaultUnitConversion.IsDefault = true;
                    defaultUnitConversion.IsFreeStyle = false;
                    defaultUnitConversion.ProductUnitConversionName = unitInfo.UnitName;
                    defaultUnitConversion.DecimalPlace = model?.UnitConversions?.FirstOrDefault(u => u.ProductUnitConversionId == defaultUnitConversion.ProductUnitConversionId || u.IsDefault)?.DecimalPlace ?? DECIMAL_PLACE_DEFAULT;

                    if (!newUnitConversionList.Contains(defaultUnitConversion))
                        newUnitConversionList.Add(defaultUnitConversion);
                }
                var duplicateUnit = newUnitConversionList.GroupBy(u => u.ProductUnitConversionName?.NormalizeAsInternalName())
                   .Where(g => g.Count() > 1)
                   .FirstOrDefault();

                if (duplicateUnit != null)
                {
                    await trans.RollbackAsync();
                    throw PuConversionDuplicated.BadRequestFormat(duplicateUnit.First()?.ProductUnitConversionName, productInfo.ProductCode);
                }

                if (_stockDbContext.HasChanges())
                    productInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.UpdateStockInfo)
                  .MessageResourceFormatDatas(productInfo.ProductCode)
                  .ObjectId(productId)
                  .JsonData(model.JsonSerialize())
                  .CreateLog();

                return true;
            }
        }


        private void ValidatePu(ProductUnitConversion pu)
        {
            try
            {
                if (!pu.IsDefault)
                {
                    var eval = EvalUtils.EvalPrimaryQuantityFromProductUnitConversionQuantity(1, pu.FactorExpression);
                    if (!(eval > 0))
                    {
                        throw ProductErrorCode.InvalidUnitConversionExpression.BadRequest();
                    }
                }
                else
                {
                    pu.FactorExpression = "1";
                }
            }
            catch (Exception)
            {
                throw ProductErrorCode.InvalidUnitConversionExpression.BadRequest();
            }
        }


        public async Task<ProductPartialSellModel> SellInfo(int productId)
        {
            var productInfo = await _stockDbContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }

            var productCustomers = await _stockDbContext.ProductCustomer.AsNoTracking().Where(p => p.ProductId == productId).ToListAsync();

            return new ProductPartialSellModel()
            {
                //CustomerId = productInfo.CustomerId,
                Measurement = productInfo.Measurement,
                PackingMethod = productInfo.PackingMethod,
                GrossWeight = productInfo.GrossWeight,
                NetWeight = productInfo.NetWeight,
                LoadAbility = productInfo.LoadAbility,
                SellDescription = productInfo.SellDescription,
                PackingQuantitative = productInfo.PackingQuantitative,
                PackingWidth = productInfo.PackingWidth,
                PackingLong = productInfo.PackingLong,
                PackingHeight = productInfo.PackingHeight,
                ProductCustomers = _mapper.Map<List<ProductModelCustomer>>(productCustomers),
                UpdatedDatetimeUtc = productInfo.UpdatedDatetimeUtc.GetUnix()
            };
        }

        public async Task<bool> UpdateSellInfo(int productId, ProductPartialSellModel model)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var productInfo = await _stockDbContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (productInfo == null)
                {
                    throw new BadRequestException(ProductErrorCode.ProductNotFound);
                }

                if (model.UpdatedDatetimeUtc != productInfo.UpdatedDatetimeUtc.GetUnix())
                {
                    throw GeneralCode.DataIsOld.BadRequest();
                }

                var productCustomers = await _stockDbContext.ProductCustomer.Where(p => p.ProductId == productId).ToListAsync();

                //productInfo.CustomerId = model.CustomerId;
                productInfo.Measurement = model.Measurement;
                productInfo.PackingMethod = model.PackingMethod;
                productInfo.GrossWeight = model.GrossWeight;
                productInfo.NetWeight = model.NetWeight;
                productInfo.LoadAbility = model.LoadAbility;
                productInfo.SellDescription = model.SellDescription;
                productInfo.PackingQuantitative = model.PackingQuantitative;
                productInfo.PackingWidth = model.PackingWidth;
                productInfo.PackingLong = model.PackingLong;
                productInfo.PackingHeight = model.PackingHeight;

                if (model.ProductCustomers == null)
                {
                    model.ProductCustomers = new List<ProductModelCustomer>();
                }


                if (model.ProductCustomers.GroupBy(c => c.CustomerId).Any(g => g.Count() > 1))
                {
                    throw ExistMoreSameCustomerProduct.BadRequest();
                }

                var removeProductCustomers = productCustomers.Where(c => !model.ProductCustomers.Select(c1 => c1.CustomerId).Contains(c.CustomerId));
                _stockDbContext.ProductCustomer.RemoveRange(removeProductCustomers);

                foreach (var c in model.ProductCustomers)
                {
                    var existed = productCustomers.FirstOrDefault(c1 => c1.CustomerId == c.CustomerId);
                    if (existed != null)
                    {
                        _mapper.Map(c, existed);
                    }
                    else
                    {
                        var entity = _mapper.Map<ProductCustomer>(c);
                        entity.ProductId = productId;
                        await _stockDbContext.ProductCustomer.AddAsync(entity);
                    }
                }

                if (_stockDbContext.HasChanges())
                    productInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.UpdateSellInfo)
                   .MessageResourceFormatDatas(productInfo.ProductCode)
                   .ObjectId(productId)
                   .JsonData(model.JsonSerialize())
                   .CreateLog();
                return true;
            }
        }



        public async Task<ProductProcessModel> ProcessInfo(int productId)
        {
            var productInfo = await _stockDbContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }

            var productCustomers = await _stockDbContext.ProductCustomer.AsNoTracking().Where(p => p.ProductId == productId).ToListAsync();

            return new ProductProcessModel()
            {
                Coefficient = productInfo.Coefficient,
                ProductionProcessStatusId = (EnumProductionProcessStatus)productInfo.ProductionProcessStatusId
            };
        }

        public async Task<bool> UpdateProcessInfo(int productId, ProductProcessModel model)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var productInfo = await _stockDbContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (productInfo == null)
                {
                    throw new BadRequestException(ProductErrorCode.ProductNotFound);
                }

                productInfo.Coefficient = model.Coefficient;
                productInfo.ProductionProcessStatusId = (int)EnumProductionProcessStatus.Created;

                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.UpdateProcessInfo)
                   .MessageResourceFormatDatas(productInfo.ProductCode)
                   .ObjectId(productId)
                   .JsonData(model.JsonSerialize())
                   .CreateLog();
                return true;
            }
        }
    }
}
