using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.Products;
using VErp.Services.Stock.Service.Stock;
using Xunit;
using static VErp.Commons.GlobalObject.InternalDataInterface.ProductModel;

namespace MasterTests
{
    public class InventoryTests : BaseDevelopmentUnitStartup
    {
        private readonly IProductCateService productCateService;
        private readonly IProductTypeService productTypeService;
        private readonly IProductService productService;
        private readonly IUnitService unitService;
        private readonly IInventoryService inventoryService;
        private readonly IStockService stockService;

        public InventoryTests()
        {
            productCateService = webHost.Services.GetService<IProductCateService>();
            productTypeService = webHost.Services.GetService<IProductTypeService>();
            productService = webHost.Services.GetService<IProductService>();
            unitService = webHost.Services.GetService<IUnitService>();
            inventoryService = webHost.Services.GetService<IInventoryService>();
            stockService = webHost.Services.GetService<IStockService>();
        }

        [Fact]
        public async Task TestInventory()
        {

            try
            {
                var productTestCode1 = "PROD_001";
                var productTestCode2 = "PROD_002";

                var product1 = await EnsureProduct(productTestCode1);
                var product2 = await EnsureProduct(productTestCode2);

                await ClearStock(product1.ProductId);
                await ClearStock(product2.ProductId);

                var stockInfo = await EnsureStock("STOCK_TEST01");

                await ClearInventory("TEST-INV-1-1");
                await ClearInventory("TEST-OUV-1-1");
                await ClearInventory("TEST-INV-1-2");
                await ClearInventory("TEST-OUV-1-2");
                await ClearInventory("TEST-INV-1-3");
                await ClearInventory("TEST-INV-2-1");
                await ClearInventory("TEST-INV-2-2");
                await ClearInventory("TEST-OUV-2-1");
                await ClearInventory("TEST-INV-2-3");
                await ClearInventory("TEST-OUV-2-2");
                await ClearInventory("TEST-OUV-4-1");
                await ClearInventory("TEST-INV-4-1");
                await ClearInventory("TEST-OUV-4-2");

                await InitInventories(stockInfo.StockId, product1, product2);
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        private async Task InitInventories(int stockId, ProductData productId1, ProductData productId2)
        {
            /*
             * TEST-INV-1-1 1/1/2020 +20        20
             * TEST-OUV-1-1 1/1/2020                -15     25  
             * TEST-INV-1-2 1/1/2020 +10        30
             * TEST-OUV-1-2 1/1/2020                -5      20
             * TEST-OUV-1-2 1/1/2020                -10     10
             * TEST-INV-1-3 1/1/2020 +10        40
             * 
             * TEST-INV-2-1 2/1/2020 +10        20
             * TEST-INV-2-1 2/1/2020 +2         22
             * TEST-INV-2-2 2/1/2020 +5 (f)     *
             * TEST-OUV-2-1 2/1/2020                -5      25
             * TEST-OUV-2-1 2/1/2020                -2      23
             * TEST-INV-2-3 2/1/2020 +2         24
             * TEST-INV-2-3 2/1/2020 +6         30
             * TEST-OUV-2-2 2/1/2020                -10 (f) *
             * 
             * TEST-OUV-4-1 4/1/2020                -5      28
             * TEST-OUV-4-1 4/1/2020                -5      23
             * TEST-OUV-4-1 4/1/2020                -5      18
             * TEST-INV-4-1 4/1/2020 +10        33
             * TEST-OUV-4-2 4/1/2020                -5      13
             */



            /*Day1*/
            //TEST-INV-1-1 
            var inv11 = await InputInventory(stockId, "TEST-INV-1-1", new DateTime(2020, 1, 1), P(productId1, 20));

            Assert.True((await inventoryService.ApproveInventoryInput(inv11.Data, UserId)).IsSuccess());



            //TEST-OUV-1-1
            var outv11 = await OutputInventory(stockId, "TEST-OUV-1-1", new DateTime(2020, 1, 1), P(productId1, 15));
            Assert.True((await inventoryService.ApproveInventoryOutput(outv11.Data, UserId)).Code.IsSuccess());

            //TEST-INV-1-2 
            var inv12 = await InputInventory(stockId, "TEST-INV-1-2", new DateTime(2020, 1, 1), P(productId1, 10));
            Assert.True((await inventoryService.ApproveInventoryInput(inv12.Data, UserId)).IsSuccess());

            //TEST-OUV-1-2
            var outv12 = await OutputInventory(stockId, "TEST-OUV-1-2", new DateTime(2020, 1, 1), P(productId1, 5), P(productId1, 10));
            Assert.True((await inventoryService.ApproveInventoryOutput(outv12.Data, UserId)).Code.IsSuccess());

            //TEST-INV-1-3
            var inv13 = await InputInventory(stockId, "TEST-INV-1-3", new DateTime(2020, 1, 1), P(productId1, 10));
            Assert.True((await inventoryService.ApproveInventoryInput(inv13.Data, UserId)).IsSuccess());


            /*Day2*/
            //TEST-INV-2-1
            var inv21 = await InputInventory(stockId, "TEST-INV-2-1", new DateTime(2020, 1, 2), P(productId1, 10), P(productId1, 2));
            Assert.True((await inventoryService.ApproveInventoryInput(inv21.Data, UserId)).IsSuccess());

            //TEST-INV-2-2 2/1/2020
            await InputInventory(stockId, "TEST-INV-2-2", new DateTime(2020, 1, 2), P(productId1, 5));


            //TEST-OUV-2-1
            var outv21 = await OutputInventory(stockId, "TEST-OUV-2-1", new DateTime(2020, 1, 2), P(productId1, 5), P(productId1, 2));
            Assert.True((await inventoryService.ApproveInventoryOutput(outv21.Data, UserId)).IsSuccessCode());

            //TEST-INV-2-3
            var inv23 = await InputInventory(stockId, "TEST-INV-2-3", new DateTime(2020, 1, 2), P(productId1, 2), P(productId1, 6));
            Assert.True((await inventoryService.ApproveInventoryInput(inv23.Data, UserId)).IsSuccess());

            //TEST-OUV-2-2
            await OutputInventory(stockId, "TEST-OUV-2-2", new DateTime(2020, 1, 2), P(productId1, 10));


            /*Day3*/
            //TEST-OUV-4-1
            var outv41 = await OutputInventory(stockId, "TEST-OUV-4-1", new DateTime(2020, 1, 4), P(productId1, 5), P(productId1, 5), P(productId1, 5));
            Assert.True((await inventoryService.ApproveInventoryOutput(outv41.Data, UserId)).IsSuccessCode());

            //TEST-INV-4-1
            var inv41 = await InputInventory(stockId, "TEST-INV-4-1", new DateTime(2020, 1, 4), P(productId1, 10));
            Assert.True((await inventoryService.ApproveInventoryInput(inv41.Data, UserId)).IsSuccess());


            //TEST-OUV-4-2
            var outv42 = await OutputInventory(stockId, "TEST-OUV-4-2", new DateTime(2020, 1, 4), P(productId1, 5));
            Assert.True((await inventoryService.ApproveInventoryOutput(outv42.Data, UserId)).IsSuccessCode());


            Assert.Equal(10, await GetBalanceByDate(new DateTime(2020, 1, 1), stockId, productId1.ProductId));

            Assert.Equal(23, await GetBalanceByDate(new DateTime(2020, 1, 2), stockId, productId1.ProductId));

            Assert.Equal(13, await GetBalanceByDate(new DateTime(2020, 1, 4), stockId, productId1.ProductId));

            //Test change date and quantity input
            var inventory23Id = inv23.Data;
            var inventory23Code = "TEST-INV-2-3";
            var inventory23Info = await inventoryService.GetInventory(inventory23Id);
            var affect23From = new DateTime(2011, 1, 1).GetUnix();
            var affect23To = new DateTime(2022, 1, 1).GetUnix();
            var oldP1Quantity = 2;
            var newP1Quantity = 4;

            var oldP2Quantity = 6;
            var newP2Quantity = 12;

            var oldP1Detail = await _stockDBContext.InventoryDetail.Where(d => d.InventoryId == inventory23Id && d.ProductId == productId1.ProductId && d.PrimaryQuantity == oldP1Quantity).FirstOrDefaultAsync();


            //--Increase date
            var update23Model = InputInventoryCreationModel(stockId, inventory23Code, new DateTime(2020, 1, 4), P(productId1, newP1Quantity, oldP1Detail.InventoryDetailId), P(productId1, newP2Quantity));

            var affectDetails = await inventoryService.InputUpdateGetAffectedPackages(inventory23Id, affect23From, affect23To, update23Model);
            Assert.True((await inventoryService.ApprovedInputDataUpdate(UserId, inventory23Id, affect23From, affect23To, new ApprovedInputDataSubmitModel()
            {
                Inventory = update23Model,
                AffectDetails = affectDetails.Data
            })).IsSuccessCode());


            //--Decrease date
            update23Model = InputInventoryCreationModel(stockId, inventory23Code, new DateTime(2020, 1, 2), P(productId1, oldP1Quantity, oldP1Detail.InventoryDetailId), P(productId1, oldP2Quantity));

            affectDetails = await inventoryService.InputUpdateGetAffectedPackages(inventory23Id, affect23From, affect23To, update23Model);
            Assert.True((await inventoryService.ApprovedInputDataUpdate(UserId, inventory23Id, affect23From, affect23To, new ApprovedInputDataSubmitModel()
            {
                Inventory = update23Model,
                AffectDetails = affectDetails.Data
            })).IsSuccessCode());


            //Test change date and quantity output
            //TEST-OUV-2-1
            var inventory21Id = outv21.Data;
            var inventory21Code = "TEST-OUV-2-1";
            var inventory21Info = await inventoryService.GetInventory(inventory23Id);
            var out21_oldP1Quantity = 5;
            var out21_newP1Quantity = 6;

            var out21_oldP2Quantity = 2;
            var out21_newP2Quantity = 3;

            var updatedOut21Model = OutputInventoryCreationModel(stockId, inventory21Code, new DateTime(2020, 1, 4), P(productId1, out21_newP1Quantity), P(productId1, out21_newP2Quantity));
            Assert.True((await inventoryService.UpdateInventoryOutput(inventory21Id, UserId, updatedOut21Model)).IsSuccess());
            Assert.True((await inventoryService.ApproveInventoryOutput(inventory21Id, UserId)).IsSuccessCode());

            updatedOut21Model = OutputInventoryCreationModel(stockId, inventory21Code, new DateTime(2020, 1, 2), P(productId1, out21_oldP1Quantity), P(productId1, out21_oldP2Quantity));
            Assert.True((await inventoryService.UpdateInventoryOutput(inventory21Id, UserId, updatedOut21Model)).IsSuccess());
            Assert.True((await inventoryService.ApproveInventoryOutput(inventory21Id, UserId)).IsSuccessCode());


            //Test delete inventory input

            update23Model = InputInventoryCreationModel(stockId, inventory23Code, new DateTime(2020, 1, 2));

            affectDetails = await inventoryService.InputUpdateGetAffectedPackages(inventory23Id, affect23From, affect23To, update23Model);
            Assert.True((await inventoryService.ApprovedInputDataUpdate(UserId, inventory23Id, affect23From, affect23To, new ApprovedInputDataSubmitModel()
            {
                Inventory = update23Model,
                AffectDetails = affectDetails.Data
            })).IsSuccessCode());

            inv23 = await InputInventory(stockId, inventory23Code, new DateTime(2020, 1, 2), P(productId1, 2), P(productId1, 6));
            Assert.True((await inventoryService.ApproveInventoryInput(inv23.Data, UserId)).IsSuccess());


            Assert.Equal(10, await GetBalanceByDate(new DateTime(2020, 1, 1), stockId, productId1.ProductId));

            Assert.Equal(23, await GetBalanceByDate(new DateTime(2020, 1, 2), stockId, productId1.ProductId));

            Assert.Equal(13, await GetBalanceByDate(new DateTime(2020, 1, 4), stockId, productId1.ProductId));

            //Test delete inventory output
            await inventoryService.DeleteInventoryOutput(inventory21Id, UserId);

            outv21 = await OutputInventory(stockId, inventory21Code, new DateTime(2020, 1, 2), P(productId1, out21_oldP1Quantity), P(productId1, out21_oldP2Quantity));
            await inventoryService.ApproveInventoryOutput(outv21.Data, UserId);

            Assert.Equal(10, await GetBalanceByDate(new DateTime(2020, 1, 1), stockId, productId1.ProductId));

            Assert.Equal(23, await GetBalanceByDate(new DateTime(2020, 1, 2), stockId, productId1.ProductId));

            Assert.Equal(13, await GetBalanceByDate(new DateTime(2020, 1, 4), stockId, productId1.ProductId));
        }


        private async Task<decimal?> GetBalanceByDate(DateTime date, int stockId, int productId)
        {
            var balance = await (
                from d in _stockDBContext.InventoryDetail
                join iv in _stockDBContext.Inventory on d.InventoryId equals iv.InventoryId
                where iv.IsApproved && iv.Date == date && iv.StockId == stockId && d.ProductId == productId
                orderby iv.InventoryTypeId descending, iv.InventoryId descending, d.InventoryDetailId descending
                select d.PrimaryQuantityRemaning
                                ).FirstOrDefaultAsync();
            return balance;
        }


        private async Task<ServiceResult<long>> InputInventory(int stockId, string inventoryCode, DateTime date, params InventoryProductCreationModel[] creationModels)
        {
            var result = await inventoryService.AddInventoryInput(UserId, InputInventoryCreationModel(stockId, inventoryCode, date, creationModels));

            if (!result.Code.IsSuccess())
            {
                throw new Exception(result.Message);
            }
            return result;
        }

        private InventoryInModel InputInventoryCreationModel(int stockId, string inventoryCode, DateTime date, params InventoryProductCreationModel[] creationModels)
        {
            return new InventoryInModel()
            {
                StockId = stockId,

                InventoryCode = inventoryCode,

                Shipper = "",
                Content = "",
                Date = date.GetUnix(),
                CustomerId = null,
                Department = "",
                StockKeeperUserId = null,

                BillCode = "",

                BillSerial = "",

                BillDate = 0,

                FileIdList = null,
                InProducts = creationModels.Select(c => CreateInventoryInputProductModel(c)).ToList()
            };
        }

        private async Task<ServiceResult<long>> OutputInventory(int stockId, string inventoryCode, DateTime date, params InventoryProductCreationModel[] creationModels)
        {
            var result = await inventoryService.AddInventoryOutput(UserId, OutputInventoryCreationModel(stockId, inventoryCode, date, creationModels));

            if (!result.Code.IsSuccess())
            {
                throw new Exception(result.Message);
            }
            return result;
        }

        private InventoryOutModel OutputInventoryCreationModel(int stockId, string inventoryCode, DateTime date, params InventoryProductCreationModel[] creationModels)
        {
            return new InventoryOutModel()
            {
                StockId = stockId,

                InventoryCode = inventoryCode,

                Shipper = "",
                Content = "",
                Date = date.GetUnix(),
                CustomerId = null,
                Department = "",
                StockKeeperUserId = null,

                FileIdList = null,

                OutProducts = creationModels.Select(c => CreateInventoryOutputProductModel(c)).ToList()

            };
        }

        private InventoryInProductModel CreateInventoryInputProductModel(InventoryProductCreationModel inventoryProduct)
        {
            return new InventoryInProductModel
            {
                InventoryDetailId = inventoryProduct.InventoryDetailId,
                ProductId = inventoryProduct.ProductId,
                ProductUnitConversionId = inventoryProduct.ProductUnitConversionId,

                PrimaryQuantity = inventoryProduct.Quantity,

                ProductUnitConversionQuantity = inventoryProduct.Quantity,

                UnitPrice = 0,

                RefObjectTypeId = null,
                RefObjectId = null,
                RefObjectCode = null,

                OrderCode = null,

                POCode = null,

                ProductionOrderCode = null,

                ToPackageId = null,
                PackageOptionId = EnumPackageOption.NoPackageManager,
                SortOrder = 0
            };
        }


        private InventoryOutProductModel CreateInventoryOutputProductModel(InventoryProductCreationModel inventoryProduct)
        {
            var package = _stockDBContext.Package.Where(p => p.ProductId == inventoryProduct.ProductId && p.ProductUnitConversionId == inventoryProduct.ProductUnitConversionId).First();

            return new InventoryOutProductModel
            {
                ProductId = inventoryProduct.ProductId,
                ProductUnitConversionId = inventoryProduct.ProductUnitConversionId,

                PrimaryQuantity = inventoryProduct.Quantity,

                ProductUnitConversionQuantity = inventoryProduct.Quantity,

                UnitPrice = 0,

                RefObjectTypeId = null,
                RefObjectId = null,
                RefObjectCode = null,

                OrderCode = null,

                POCode = null,

                ProductionOrderCode = null,
                FromPackageId = package.PackageId,
                SortOrder = 0
            };
        }


        [Fact]
        public async void CreateTestProduct()
        {
            var result = await CreateProduct("Test");

            Assert.True(result.Code.IsSuccess());
            Assert.True(result.Data > 0);
        }


        private async Task ClearInventory(string inventoryCode)
        {
            var inventoryInfo = await _stockDBContext.Inventory.FirstOrDefaultAsync(iv => iv.InventoryCode == inventoryCode);
            if (inventoryInfo != null)
            {
                var productIds = await _stockDBContext.InventoryDetail.Where(p => p.InventoryId == inventoryInfo.InventoryId).Select(p => p.ProductId).ToListAsync();
                foreach (var productId in productIds)
                {
                    await ClearStock(productId);
                }

                _stockDBContext.Inventory.Remove(inventoryInfo);

                var inventoryChange = await _stockDBContext.InventoryChange.FirstOrDefaultAsync(iv => iv.InventoryId == inventoryInfo.InventoryId);
                if (inventoryChange != null)
                {
                    _stockDBContext.InventoryChange.Remove(inventoryChange);
                }

                await _stockDBContext.SaveChangesAsync();
            }

        }

        private async Task ClearStock(int productId)
        {
            using (var trans = await _stockDBContext.Database.BeginTransactionAsync())
            {
                var packages = await _stockDBContext.Package.IgnoreQueryFilters().Where(p => p.ProductId == productId).ToListAsync();
                var packageIds = packages.Select(p => p.PackageId).ToList();
                var packageRes = await _stockDBContext.PackageRef.IgnoreQueryFilters().Where(p => packageIds.Contains(p.PackageId) || packageIds.Contains(p.RefPackageId)).ToListAsync();
                var todetails = await _stockDBContext.InventoryDetailToPackage.IgnoreQueryFilters().Where(p => packageIds.Contains(p.ToPackageId)).ToListAsync();

                var inventoryDetails = await _stockDBContext.InventoryDetail.IgnoreQueryFilters().Where(p => p.ProductId == productId).ToListAsync();

                var stockProducts = await _stockDBContext.StockProduct.IgnoreQueryFilters().Where(p => p.ProductId == productId).ToListAsync();

                var inventoryDetailChanges = await _stockDBContext.InventoryDetailChange.IgnoreQueryFilters().Where(p => p.ProductId == productId).ToListAsync();

                _stockDBContext.StockProduct.RemoveRange(stockProducts);
                _stockDBContext.InventoryDetailToPackage.RemoveRange(todetails);
                _stockDBContext.PackageRef.RemoveRange(packageRes);

                _stockDBContext.InventoryDetail.RemoveRange(inventoryDetails);
                _stockDBContext.InventoryDetailChange.RemoveRange(inventoryDetailChanges);

                _stockDBContext.Package.RemoveRange(packages);


                await _stockDBContext.SaveChangesAsync();
                trans.Commit();
            }
        }

        private async Task<ProductData> EnsureProduct(string productCode)
        {
            var productInfo = await _stockDBContext.Product.FirstOrDefaultAsync(p => p.ProductCode == productCode);
            if (productInfo == null)
            {
                var result = await CreateProduct(productCode);

                if (!result.Code.IsSuccess())
                {
                    throw new Exception(result.Message);
                }

                productInfo = await _stockDBContext.Product.FirstOrDefaultAsync(p => p.ProductCode == productCode);
            }

            var productUnitConversion = await _stockDBContext.ProductUnitConversion.FirstOrDefaultAsync(p => p.ProductId == productInfo.ProductId && !p.IsDefault);


            return new ProductData()
            {
                ProductId = productInfo.ProductId,
                ProductUnitConversionId = productUnitConversion.ProductUnitConversionId
            };

        }

        private InventoryProductCreationModel P(ProductData product, decimal quantity, long? inventoryDetailId = null)
        {
            return new InventoryProductCreationModel(product, quantity, inventoryDetailId);
        }

        class ProductData
        {
            public int ProductId { get; set; }
            public int ProductUnitConversionId { get; set; }
        }

        class InventoryProductCreationModel : ProductData
        {
            public InventoryProductCreationModel(ProductData product, decimal quantity, long? inventoryDetailId = null)
            {
                ProductId = product.ProductId;
                ProductUnitConversionId = product.ProductUnitConversionId;
                Quantity = quantity;
                InventoryDetailId = inventoryDetailId;
            }



            public long? InventoryDetailId { get; set; }
            public decimal Quantity { get; set; }
        }

        private async Task<Stock> EnsureStock(string stockName)
        {
            var stockInfo = await _stockDBContext.Stock.FirstOrDefaultAsync(p => p.StockName == stockName);
            if (stockInfo == null)
            {
                var result = await stockService.AddStock(new StockModel()
                {
                    Status = 1,
                    Description = "",
                    StockName = stockName
                });

                if (!result.Code.IsSuccess())
                {
                    throw new Exception(result.Message);
                }

                stockInfo = await _stockDBContext.Stock.FirstOrDefaultAsync(p => p.StockName == stockName);

            }

            return stockInfo;

        }

        private async Task<ServiceResult<int>> CreateProduct(string productCode)
        {


            var productCateInfo = productCateService.GetList("", 1, 1).GetAwaiter().GetResult().List[0];
            var productTypeInfo = productTypeService.GetList("", 1, 1).GetAwaiter().GetResult().List[0];

            var unitInfo = unitService.GetList("", EnumUnitStatus.Using, 1, 1).GetAwaiter().GetResult().List[0];

            var result = await productService.AddProduct(new ProductModel()
            {
                ProductCode = productCode,
                ProductName = productCode,
                IsCanBuy = true,
                IsCanSell = true,
                MainImageFileId = null,
                ProductTypeId = productTypeInfo.ProductTypeId,
                ProductCateId = productCateInfo.ProductCateId,
                BarcodeConfigId = null,
                BarcodeStandardId = null,
                Barcode = "",
                UnitId = unitInfo.UnitId,
                EstimatePrice = 0,

                Extra = new ProductModelExtra()
                {
                    Specification = "",
                    Description = "",
                },
                StockInfo = new ProductModelStock()
                {
                    StockOutputRuleId = EnumStockOutputRule.Fifo,
                    AmountWarningMin = null,
                    AmountWarningMax = null,
                    TimeWarningAmount = null,
                    TimeWarningTimeTypeId = null,
                    ExpireTimeAmount = null,
                    ExpireTimeTypeId = null,
                    DescriptionToStock = "",

                    StockIds = null,

                    UnitConversions = new List<ProductModelUnitConversion>()
                    {
                        new ProductModelUnitConversion()
                        {
                            SecondaryUnitId = unitInfo.UnitId,
                            ProductUnitConversionName = "x2 " + unitInfo.UnitName,
                            FactorExpression = "2",
                            ConversionDescription = "",
                            IsDefault = false,
                        }
                    }
                }
            });

            return result;
        }
    }
}
