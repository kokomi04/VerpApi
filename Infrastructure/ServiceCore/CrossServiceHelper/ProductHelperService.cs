using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IProductHelperService
    {
        Task<bool> ValidateProductUnitConversions(Dictionary<int, int> productUnitConvertsionProduct);
        Task<IList<ProductModel>> GetListByCodeAndInternalNames(IList<string> productCodes, IList<string> productInternalNames);
        Task<IList<ProductModel>> GetListProducts(IList<int> productIds);
    }


    public class ProductHelperService : IProductHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly VErp.Grpc.Protos.Product.ProductClient _productClient;

        public ProductHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<ProductHelperService> logger, VErp.Grpc.Protos.Product.ProductClient productClient)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
            _productClient = productClient;
        }
        public async Task<bool> ValidateProductUnitConversions(Dictionary<int, int> productUnitConvertsionProduct)
        {
            return await _httpCrossService.Post<bool>("api/internal/InternalProduct/ValidateProductUnitConversion", productUnitConvertsionProduct);
        }

        public async Task<IList<ProductModel>> GetListByCodeAndInternalNames(IList<string> productCodes, IList<string> productInternalNames)
        {
            return await _httpCrossService.Post<IList<ProductModel>>("api/internal/InternalProduct/GetListByCodeAndInternalNames", new
            {
                productCodes,
                productInternalNames,
            });
        }

        public async Task<IList<ProductModel>> GetListProducts(IList<int> productIds)
        {
            if (_appSetting.GrpcInternal?.Address?.Contains("https") == true)
            {
                IList<ProductModel> productModels = new List<ProductModel>();

                using(var call = _productClient.GetListProducts())
                {
                    foreach(var productId in productIds)
                    {
                        await call.RequestStream.WriteAsync(new VErp.Grpc.Protos.GetListProductRequest { ProductId = productId });
                    }

                    await call.RequestStream.CompleteAsync();

                    while (await call.ResponseStream.MoveNext())
                    {
                        var current = call.ResponseStream.Current;
                        if(current.ProductId != 0)
                        {
                            productModels.Add(new ProductModel
                            {
                                Barcode = current.Barcode,
                                BarcodeConfigId = current.BarcodeConfigId,
                                BarcodeStandardId = (Commons.Enums.MasterEnum.EnumBarcodeStandard?)current.BarcodeStandardId,
                                EstimatePrice = current.EstimatePrice,
                                IsCanBuy = current.IsCanBuy,
                                IsCanSell = current.IsCanSell,
                                MainImageFileId = current.MainImageFileId,
                                ProductCateId = current.ProductCateId,
                                ProductCode = current.ProductCode,
                                ProductId = current.ProductId,
                                ProductName = current.ProductName,
                                ProductTypeId = current.ProductTypeId,
                                Extra = new ProductModel.ProductModelExtra
                                {
                                    Description = current.Extra.Description,
                                    Specification = current.Extra.Specification
                                },
                                StockInfo = new ProductModel.ProductModelStock
                                {
                                    StockOutputRuleId = (Commons.Enums.MasterEnum.EnumStockOutputRule?)current.StockInfo.StockOutputRuleId,
                                    AmountWarningMin = current.StockInfo.AmountWarningMin,
                                    AmountWarningMax = current.StockInfo.AmountWarningMax,
                                    TimeWarningTimeTypeId = (Commons.Enums.MasterEnum.EnumTimeType?)current.StockInfo.TimeWarningTimeTypeId,
                                    TimeWarningAmount = current.StockInfo.TimeWarningAmount,
                                    ExpireTimeTypeId = (Commons.Enums.MasterEnum.EnumTimeType?)current.StockInfo.ExpireTimeTypeId,
                                    ExpireTimeAmount = current.StockInfo.ExpireTimeAmount,
                                    DescriptionToStock = current.StockInfo.DescriptionToStock,
                                },
                                UnitId = current.UnitId
                            });
                        }
                        
                    }
                    return productModels;
                }
            }
            return await _httpCrossService.Post<IList<ProductModel>>("api/internal/InternalProduct/GetListProductsByIds", productIds);
        }
    }
}
