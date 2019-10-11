using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductUnitConversionService : IProductUnitConversionService
    {
        private readonly StockDBContext _stockContext;
        private readonly ILogger _logger;

        public ProductUnitConversionService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductService> logger
            , IUnitService unitService
            , IActivityService activityService
        )
        {
            _stockContext = stockContext;
            _logger = logger;
        }

        public async Task<Enum> AddProductUnitConversion(ProductUnitConversionModel req)
        {
            var productByPk = await _stockContext.ProductUnitConversion.FirstOrDefaultAsync(p => p.ProductId == req.ProductId && p.SecondaryUnitId == req.SecondaryUnitId);
            if (productByPk != null)
            {
                return ProductUnitConversionErrorCode.ProductSecondaryUnitAlreadyExisted;
            }

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var productUnitConversionInfo = new ProductUnitConversion()
                    {
                        ProductId = req.ProductId,
                        SecondaryUnitId = req.SecondaryUnitId,
                        FactorExpression = req.FactorExpression,
                        ConversionDescription = req.ConversionDescription
                    };

                    await _stockContext.AddAsync(productUnitConversionInfo);

                    await _stockContext.SaveChangesAsync();

                    trans.Commit();


                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "AddProduct");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<ServiceResult<List<ProductUnitConversionOutput>>> ProductUnitConversionList(int productId)
        {
            var productUnitConversionList = await _stockContext.ProductUnitConversion.Where(p => p.ProductId == productId).ToListAsync();
            if (productUnitConversionList == null)
            {
                return new List<ProductUnitConversionOutput>();
            }

            var res = new List<ProductUnitConversionOutput>(productUnitConversionList.Count);
            foreach (var item in productUnitConversionList)
            {
                res.Add(new ProductUnitConversionOutput
                {
                    ProductId = item.ProductId,
                    SecondaryUnitId = item.SecondaryUnitId,
                    FactorExpression = item.FactorExpression,
                    ConversionDescription = item.ConversionDescription
                });
            }
            return res;
        }

        public async Task<Enum> UpdateProductUnitConversion(ProductUnitConversionModel req)
        {

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //Getdata
                    var info = await _stockContext.ProductUnitConversion.FirstOrDefaultAsync(p => p.ProductId == req.ProductId && p.SecondaryUnitId == req.SecondaryUnitId);
                    if (info == null)
                    {
                        return ProductErrorCode.ProductNotFound;
                    }

                    //Update
                    info.ConversionDescription = req.ConversionDescription;
                    info.FactorExpression = req.FactorExpression;

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();
                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "UpdateProductUnitConversion");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> DeleteProductUnitConversion(int productId, int secondaryUnitId)
        {
            var info = await _stockContext.ProductUnitConversion.FirstOrDefaultAsync(p => p.ProductId == productId && p.SecondaryUnitId == secondaryUnitId);

            if (info == null)
            {
                return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
            }

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    _stockContext.ProductUnitConversion.Remove(info);
                    await _stockContext.SaveChangesAsync();
                    trans.Commit();
                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "DeleteProductUnitConversion");
                    return GeneralCode.InternalError;
                }
            }
        }
    }
}
