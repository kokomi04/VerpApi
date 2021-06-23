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
    public class ProductPartialService : IProductPartialService
    {
        public const int DECIMAL_PLACE_DEFAULT = 11;

        private readonly StockDBContext _stockContext;
        private readonly IUnitService _unitService;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        public ProductPartialService(
            StockDBContext stockContext
            , IUnitService unitService
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _stockContext = stockContext;
            _unitService = unitService;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }



        public async Task<ProductPartialGeneralModel> GeneralInfo(int productId)
        {
            var productInfo = await _stockContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }

            var stockInfo = await _stockContext.ProductStockInfo.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            var extraInfo = await _stockContext.ProductExtraInfo.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            return new ProductPartialGeneralModel()
            {
                ProductTypeId = productInfo.ProductTypeId,

                ProductCode = productInfo.ProductCode,

                ProductName = productInfo.ProductName,

                ProductNameEng = productInfo.ProductName,

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
                BarcodeStandardId = (EnumBarcodeStandard)productInfo.BarcodeStandardId,
                Barcode = productInfo.Barcode,

                Quantitative = productInfo.Quantitative,
                QuantitativeUnitTypeId = (EnumQuantitativeUnitType?)productInfo.QuantitativeUnitTypeId,

                Specification = extraInfo.Specification,

                EstimatePrice = productInfo.EstimatePrice,

                IsProductSemi = productInfo.IsProductSemi,
                IsProduct = productInfo.IsProduct
            };
        }

        public async Task<bool> UpdateGeneralInfo(int productId, ProductPartialGeneralModel model)
        {
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                var productInfo = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (productInfo == null)
                {
                    throw new BadRequestException(ProductErrorCode.ProductNotFound);
                }

                var stockInfo = await _stockContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                var extraInfo = await _stockContext.ProductExtraInfo.FirstOrDefaultAsync(p => p.ProductId == productId);

                productInfo.ProductTypeId = model.ProductTypeId;

                productInfo.ProductCode = model.ProductCode;

                productInfo.ProductName = model.ProductName;

                productInfo.ProductNameEng = model.ProductName;

                productInfo.MainImageFileId = model.MainImageFileId;

                productInfo.Long = model.Long;
                productInfo.Width = model.Width;
                productInfo.Height = model.Height;

                productInfo.Color = model.Color;

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

                extraInfo.Specification = model.Specification;

                productInfo.EstimatePrice = model.EstimatePrice;

                productInfo.IsProductSemi = model.IsProductSemi;
                productInfo.IsProduct = model.IsProduct;
                await _stockContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.Product, productInfo.ProductId, $"Cập nhật TT chung mặt hàng {productInfo.ProductName}", model.JsonSerialize());

                return true;
            }
        }


        public async Task<ProductPartialStockModel> StockInfo(int productId)
        {
            var productInfo = await _stockContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }

            var stockInfo = await _stockContext.ProductStockInfo.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            var stockValidation = await _stockContext.ProductStockValidation.AsNoTracking().Where(p => p.ProductId == productId).ToListAsync();
            var pus = await _stockContext.ProductUnitConversion.AsNoTracking().Where(p => p.ProductId == productId).ToListAsync();
            return new ProductPartialStockModel()
            {
                StockIds = stockValidation.Select(s => s.StockId).ToList(),

                AmountWarningMin = stockInfo.AmountWarningMin,
                AmountWarningMax = stockInfo.AmountWarningMax,
                DescriptionToStock = stockInfo.DescriptionToStock,

                StockOutputRuleId = (EnumStockOutputRule?)stockInfo.StockOutputRuleId,

                UnitConversions = _mapper.Map<List<ProductModelUnitConversion>>(pus)
            };
        }

        public async Task<bool> UpdateStockInfo(int productId, ProductPartialStockModel model)
        {
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                var productInfo = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (productInfo == null)
                {
                    throw new BadRequestException(ProductErrorCode.ProductNotFound);
                }

                var stockInfo = await _stockContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
                var stockValidation = await _stockContext.ProductStockValidation.Where(p => p.ProductId == productId).ToListAsync();
                var pus = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

                var lstStockValidations = model?.StockIds?
                        .Select(s => new ProductStockValidation()
                        {
                            ProductId = productInfo.ProductId,
                            StockId = s
                        });

                _stockContext.RemoveRange(stockValidation);
                if (lstStockValidations != null)
                {
                    await _stockContext.ProductStockValidation.AddRangeAsync(lstStockValidations);
                }


                stockInfo.AmountWarningMin = model.AmountWarningMin;
                stockInfo.AmountWarningMax = model.AmountWarningMax;
                stockInfo.DescriptionToStock = model.DescriptionToStock;

                stockInfo.StockOutputRuleId = (int?)model.StockOutputRuleId;

                var unitConverions = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();

                var keepPuIds = model.UnitConversions?.Select(c => c.ProductUnitConversionId);
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


                var unitInfo = await _unitService.GetUnitInfo(productInfo.UnitId);
                if (unitInfo == null)
                {
                    throw new BadRequestException(UnitErrorCode.UnitNotFound);
                }

                var lstNewUnitConverions = model?.UnitConversions?
                    .Where(c => c.ProductUnitConversionId <= 0)?
                    .Select(u => _mapper.Map<ProductUnitConversion>(u));

                if (lstNewUnitConverions != null)
                {
                    foreach(var c in lstNewUnitConverions)
                    {
                        c.ProductId = productId;
                        c.ProductUnitConversionId = 0;
                    }
                    await _stockContext.ProductUnitConversion.AddRangeAsync(lstNewUnitConverions);
                }

                foreach (var productUnitConversionId in keepPuIds)
                {
                    var db = unitConverions.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);
                    var u = model?.UnitConversions?.FirstOrDefault(c => c.ProductUnitConversionId == productUnitConversionId);
                    if (db != null && u != null)
                    {
                        _mapper.Map(u, db);
                    }
                }
                var defaultUnitConversion = unitConverions.FirstOrDefault(c => c.IsDefault);
                if (defaultUnitConversion != null)
                {
                    defaultUnitConversion.SecondaryUnitId = productInfo.UnitId;
                    defaultUnitConversion.IsDefault = true;
                    defaultUnitConversion.IsFreeStyle = false;
                    defaultUnitConversion.ProductUnitConversionName = unitInfo.UnitName;
                    defaultUnitConversion.DecimalPlace = model?.UnitConversions?.FirstOrDefault(u => u.ProductUnitConversionId == defaultUnitConversion.ProductUnitConversionId || u.IsDefault)?.DecimalPlace ?? DECIMAL_PLACE_DEFAULT;
                }

                await _stockContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.Product, productInfo.ProductId, $"Cập nhật TT lưu kho mặt hàng {productInfo.ProductName}", model.JsonSerialize());

                return true;
            }
        }



        public async Task<ProductPartialSellModel> SellInfo(int productId)
        {
            var productInfo = await _stockContext.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productInfo == null)
            {
                throw new BadRequestException(ProductErrorCode.ProductNotFound);
            }

            var productCustomers = await _stockContext.ProductCustomer.AsNoTracking().Where(p => p.ProductId == productId).ToListAsync();

            return new ProductPartialSellModel()
            {
                CustomerId = productInfo.CustomerId,
                Measurement = productInfo.Measurement,
                PackingMethod = productInfo.PackingMethod,
                GrossWeight = productInfo.GrossWeight,
                NetWeight = productInfo.NetWeight,
                LoadAbility = productInfo.NetWeight,
                ProductDescription = productInfo.ProductDescription,

                ProductCustomers = _mapper.Map<List<ProductModelCustomer>>(productCustomers)
            };
        }

        public async Task<bool> UpdateSellInfo(int productId, ProductPartialSellModel model)
        {
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                var productInfo = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (productInfo == null)
                {
                    throw new BadRequestException(ProductErrorCode.ProductNotFound);
                }

                var productCustomers = await _stockContext.ProductCustomer.Where(p => p.ProductId == productId).ToListAsync();

                productInfo.CustomerId = model.CustomerId;
                productInfo.Measurement = model.Measurement;
                productInfo.PackingMethod = model.PackingMethod;
                productInfo.GrossWeight = model.GrossWeight;
                productInfo.NetWeight = model.NetWeight;
                productInfo.LoadAbility = model.NetWeight;
                productInfo.ProductDescription = model.ProductDescription;

                if (model.ProductCustomers == null)
                {
                    model.ProductCustomers = new List<ProductModelCustomer>();
                }


                if (model.ProductCustomers.GroupBy(c => c.CustomerId).Any(g => g.Count() > 1))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Tồn tại nhiều hơn 1 thiết lập cho 1 khách hàng!");
                }

                var removeProductCustomers = productCustomers.Where(c => !model.ProductCustomers.Select(c1 => c.CustomerId).Contains(c.CustomerId));
                _stockContext.ProductCustomer.RemoveRange(removeProductCustomers);

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
                        await _stockContext.ProductCustomer.AddAsync(entity);
                    }
                }


                await _stockContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.Product, productInfo.ProductId, $"Cập nhật TT bán mặt hàng {productInfo.ProductName}", model.JsonSerialize());

                return true;
            }
        }
    }
}
