using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Grpc.Protos;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Grpc.Service
{
    public class InternalProductService: VErp.Grpc.Protos.Product.ProductBase
    {
        private readonly StockDBContext _stockContext;
        private readonly ILogger _logger;

        public InternalProductService(
            StockDBContext stockContext
            , ILogger<InternalProductService> logger
            )
        {
            _stockContext = stockContext;
            _logger = logger;
        }

        public override async Task GetListProducts(IAsyncStreamReader<GetListProductRequest> requestStream, IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            while( await requestStream.MoveNext())
            {
                var current = requestStream.Current;

                var product = await _stockContext.Product.FirstOrDefaultAsync(p => p.ProductId.Equals(current.ProductId));
                
                if (product == null) 
                    await responseStream.WriteAsync(new ProductModel());
                else
                {
                    var productExtra = await _stockContext.ProductExtraInfo.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
                    var productStockInfo = await _stockContext.ProductStockInfo.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
                    var stockValidations = await _stockContext.ProductStockValidation.AsNoTracking().Where(p => p.ProductId == product.ProductId).ToListAsync();
                    var unitConverions = await _stockContext.ProductUnitConversion.AsNoTracking().Where(p => p.ProductId == product.ProductId).ToListAsync();

                    var productData = new VErp.Grpc.Protos.ProductModel()
                    {
                        ProductId = product.ProductId,
                        ProductCode = product.ProductCode,
                        ProductName = product.ProductName,
                        IsCanBuy = product.IsCanBuy,
                        IsCanSell = product.IsCanSell,
                        MainImageFileId = (long)product.MainImageFileId,
                        ProductTypeId = (int)product.ProductTypeId,
                        ProductCateId = product.ProductCateId,
                        BarcodeConfigId = (int)product.BarcodeConfigId,
                        BarcodeStandardId = (VErp.Grpc.Protos.Enums.EnumBarcodeStandard)product.BarcodeStandardId,
                        Barcode = product.Barcode,
                        UnitId = product.UnitId,
                        EstimatePrice = product.EstimatePrice,

                        Extra = productExtra != null ? new ProductModelExtra()
                        {
                            Specification = productExtra.Specification,
                            Description = productExtra.Description
                        } : null,
                        StockInfo = productStockInfo != null ? new ProductModelStock()
                        {
                            StockOutputRuleId = (VErp.Grpc.Protos.Enums.EnumStockOutputRule)productStockInfo.StockOutputRuleId,
                            AmountWarningMin = (long)productStockInfo.AmountWarningMin,
                            AmountWarningMax = (long)productStockInfo.AmountWarningMax,
                            TimeWarningTimeTypeId = (VErp.Grpc.Protos.Enums.EnumTimeType)productStockInfo.TimeWarningTimeTypeId,
                            TimeWarningAmount = (double)productStockInfo.TimeWarningAmount,
                            ExpireTimeTypeId = (VErp.Grpc.Protos.Enums.EnumTimeType)productStockInfo.ExpireTimeTypeId,
                            ExpireTimeAmount = (double)productStockInfo.ExpireTimeAmount,
                            DescriptionToStock = productStockInfo.DescriptionToStock,

                        } : null
                    };

                    productData.StockInfo.StockIds.AddRange(stockValidations?.Select(s => s.StockId).ToList());
                    productData.StockInfo.UnitConversions.AddRange(unitConverions?.Select(c => new ProductModelUnitConversion()
                    {
                        ProductUnitConversionId = c.ProductUnitConversionId,
                        ProductUnitConversionName = c.ProductUnitConversionName,
                        SecondaryUnitId = c.SecondaryUnitId,
                        IsDefault = c.IsDefault,
                        IsFreeStyle = c.IsFreeStyle ?? false,
                        FactorExpression = c.FactorExpression,
                        ConversionDescription = c.ConversionDescription
                    }).ToList());

                    await responseStream.WriteAsync(productData);
                }
                
            }
        }
    }
}
