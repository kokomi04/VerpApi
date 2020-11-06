using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Inventory.OpeningBalance;
using VErp.Services.Stock.Service.Products;

namespace VErp.Services.Stock.Service.Stock.Implement.InventoryFileData
{
    public class InventoryImportFacade
    {
        private Dictionary<string, ProductCate> _productCates = null;
        private Dictionary<string, ProductType> _productTypes = null;
        private Dictionary<string, Unit> _units = null;

        private Dictionary<string, Product> _productsByCode = null;
        private Dictionary<string, Product> _productsByName = null;

        private IList<OpeningBalanceModel> _excelModel = null;
        private InventoryOpeningBalanceModel _model = null;

        private Dictionary<int, List<ProductUnitConversion>> _productUnitsByProduct = null;

        private StockDBContext _stockDbContext;
        private MasterDBContext _masterDBContext;
        private IProductService _productService;


        public InventoryImportFacade SetStockDBContext(StockDBContext stockDbContext)
        {
            _stockDbContext = stockDbContext;
            return this;
        }

        public InventoryImportFacade SetMasterDBContext(MasterDBContext masterDBContext)
        {
            _masterDBContext = masterDBContext;
            return this;
        }

        public InventoryImportFacade SetProductService(IProductService productService)
        {
            _productService = productService;
            return this;
        }




        public async Task ProcessExcelFile(ImportExcelMapping mapping, Stream stream, InventoryOpeningBalanceModel model)
        {
            _model = model;

            var stockInfo = await _stockDbContext.Stock.FirstOrDefaultAsync(s => s.StockId == model.StockId);
            if (stockInfo == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy thông tin kho trong hệ thống");
            }

            var reader = new ExcelReader(stream);

            var currentCateName = string.Empty;
            var currentCatePrefixCode = string.Empty;

            _excelModel = reader.ReadSheetEntity<OpeningBalanceModel>(mapping, (entity, propertyName, value) =>
            {
                if (propertyName == nameof(OpeningBalanceModel.CateName))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        currentCateName = value;
                    }

                    entity.CateName = currentCateName;

                    return true;
                }

                if (propertyName == nameof(OpeningBalanceModel.CatePrefixCode))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        currentCatePrefixCode = value;
                    }

                    entity.CatePrefixCode = currentCatePrefixCode;

                    return true;
                }

                return false;
            });

            await AddMissingProductCates();
            await AddMissingProductTypes();
            await AddMissingUnit();
            await AddMissingProducts();
        }

        public IList<InventoryInModel> GetInputInventoryModels()
        {
            var inventoryInputList = new List<InventoryInModel>();

            var totalRowCount = _excelModel.Count;

            var newInventoryInputModel = new List<InventoryInProductModel>(totalRowCount);

            foreach (var item in _excelModel)
            {
                if (string.IsNullOrWhiteSpace(item.ProductCode)) continue;

                if (item.Qty1 <= 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Số lượng ở mặt hàng {item.ProductCode} {item.ProductName} không đúng!");

                var productObj = _productsByCode[item.ProductCode.NormalizeAsInternalName()];
                if (productObj == null)
                {
                    productObj = _productsByName[item.ProductName.NormalizeAsInternalName()];
                }

                ProductUnitConversion productUnitConversionObj = null;

                if (!string.IsNullOrWhiteSpace(item.Unit2))
                {
                    productUnitConversionObj = _productUnitsByProduct[productObj.ProductId].FirstOrDefault(u => u.ProductUnitConversionName.NormalizeAsInternalName().Equals(item.Unit2.NormalizeAsInternalName()));
                    if (productUnitConversionObj == null)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy đơn vị chuyển đổi {item.Unit2} sản phẩm {item.ProductCode}");
                    }
                }
                else
                {
                    productUnitConversionObj = _productUnitsByProduct[productObj.ProductId].FirstOrDefault(u => u.IsDefault);
                    if (productUnitConversionObj == null)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy đơn vị tính {item.Unit2} sản phẩm {item.ProductCode}");
                    }
                }

                newInventoryInputModel.Add(new InventoryInProductModel
                {
                    ProductId = productObj != null ? productObj.ProductId : 0,
                    ProductUnitConversionId = productUnitConversionObj.ProductUnitConversionId,
                    PrimaryQuantity = item.Qty1,
                    ProductUnitConversionQuantity = item.Qty2,
                    UnitPrice = item.UnitPrice,
                    RefObjectTypeId = null,
                    RefObjectId = null,
                    RefObjectCode = item.CatePrefixCode,
                    ToPackageId = null,
                    PackageOptionId = EnumPackageOption.NoPackageManager,
                    AccountancyAccountNumberDu = item.AccountancyAccountNumberDu
                });
            }


            if (newInventoryInputModel.Count > 0)
            {
                //var groupList = newInventoryInputModel.GroupBy(g => g.RefObjectCode).ToList();
                //var index = 1;

                //foreach (var g in groupList)
                //{
                //    var details = g.ToList();
                //    foreach(var d in details)
                //    {
                //        d.PackageOptionId = EnumPackageOption.NoPackageManager;
                //        d.ToPackageId = null;
                //        d.RefObjectCode = string.Format("PN_TonDau_{0}_{1}_{2}", index, DateTime.UtcNow.ToString("ddMMyyyyHHmmss"), d.RefObjectCode);
                //    }

                //    var newInventory = new InventoryInModel
                //    {
                //        StockId = _model.StockId,
                //        InventoryCode = string.Format("PN_TonDau_{0}_{1}", index, DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
                //        Date = _model.IssuedDate,
                //        Shipper = string.Empty,
                //        Content = "Nhập tồn kho ban đầu từ excel",
                //        CustomerId = null,
                //        Department = string.Empty,
                //        StockKeeperUserId = null,
                //        BillCode = string.Empty,
                //        BillSerial = string.Empty,
                //        BillDate = _model.IssuedDate,
                //        FileIdList = null,
                //        InProducts = details
                //    };

                //    inventoryInputList.Add(newInventory);

                //    index++;
                //}

                foreach (var d in newInventoryInputModel)
                {
                    d.PackageOptionId = EnumPackageOption.NoPackageManager;
                    d.ToPackageId = null;
                    d.RefObjectCode = string.Format("PN_TonDau_{0}_{1}", DateTime.UtcNow.ToString("ddMMyyyyHHmmss"), d.RefObjectCode);
                }

                var newInventory = new InventoryInModel
                {
                    StockId = _model.StockId,
                    InventoryCode = string.Format("PN_TonDau_{0}", DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
                    Date = _model.IssuedDate,

                    Shipper = string.Empty,
                    Content = "Nhập tồn kho ban đầu từ excel",
                    CustomerId = null,
                    Department = string.Empty,
                    StockKeeperUserId = null,
                    BillCode = string.Empty,
                    BillSerial = string.Empty,
                    BillDate = _model.IssuedDate,
                    FileIdList = null,
                    InProducts = newInventoryInputModel,
                    AccountancyAccountNumber = _model.AccountancyAccountNumber
                };

                inventoryInputList.Add(newInventory);

            }

            return inventoryInputList;
        }


        public async Task<IList<InventoryOutModel>> GetOutputInventoryModels()
        {
            var inventoryOutList = new List<InventoryOutModel>();

            var totalRowCount = _excelModel.Count;

            var newInventoryOutProductModel = new List<InventoryOutProductModel>(totalRowCount);

            var productUnitConversionIds = new List<int>();
            var productIds = new List<int>();

            var productInfoByIds = new Dictionary<int, Product>();

            foreach (var item in _excelModel)
            {
                if (string.IsNullOrWhiteSpace(item.ProductCode)) continue;

                if (item.Qty1 <= 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Số lượng ở mặt hàng {item.ProductCode} {item.ProductName} không đúng!");

                var productObj = _productsByCode[item.ProductCode.NormalizeAsInternalName()];
                if (productObj == null)
                {
                    productObj = _productsByName[item.ProductName.NormalizeAsInternalName()];
                }

                ProductUnitConversion productUnitConversionObj = null;

                if (!string.IsNullOrWhiteSpace(item.Unit2))
                {
                    productUnitConversionObj = _productUnitsByProduct[productObj.ProductId].FirstOrDefault(u => u.ProductUnitConversionName.NormalizeAsInternalName().Equals(item.Unit2.NormalizeAsInternalName()));
                    if (productUnitConversionObj == null)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy đơn vị chuyển đổi {item.Unit2} sản phẩm {item.ProductCode}");
                    }
                }
                else
                {
                    productUnitConversionObj = _productUnitsByProduct[productObj.ProductId].FirstOrDefault(u => u.IsDefault);
                    if (productUnitConversionObj == null)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy đơn vị tính {item.Unit2} sản phẩm {item.ProductCode}");
                    }
                }


                productUnitConversionIds.Add(productUnitConversionObj.ProductUnitConversionId);
                productIds.Add(productObj.ProductId);
                if (!productInfoByIds.ContainsKey(productObj.ProductId))
                {
                    productInfoByIds.Add(productObj.ProductId, productObj);
                }

                newInventoryOutProductModel.Add(new InventoryOutProductModel
                {
                    ProductId = productObj.ProductId,
                    ProductUnitConversionId = productUnitConversionObj.ProductUnitConversionId,
                    PrimaryQuantity = item.Qty1,
                    ProductUnitConversionQuantity = item.Qty2,
                    UnitPrice = item.UnitPrice,
                    RefObjectTypeId = null,
                    RefObjectId = null,
                    RefObjectCode = item.CatePrefixCode,
                    AccountancyAccountNumberDu = item.AccountancyAccountNumberDu
                });
            }

            var packages = (await _stockDbContext.Package.AsNoTracking().Where(p => productIds.Contains(p.ProductId) && productUnitConversionIds.Contains(p.ProductUnitConversionId)).ToListAsync())
                .GroupBy(p => p.ProductId)
                .ToDictionary(p => p.Key, p => p.ToList());

            foreach (var item in newInventoryOutProductModel)
            {
                var packageInfo = packages[item.ProductId]
                    .Where(p => p.ProductUnitConversionId == item.ProductUnitConversionId)
                    .OrderByDescending(p => p.PackageTypeId == (int)EnumPackageType.Default)
                    .FirstOrDefault();

                var productInfo = productInfoByIds[item.ProductId];
                var puInfo = _productUnitsByProduct[item.ProductId].FirstOrDefault(u => u.ProductUnitConversionId == item.ProductUnitConversionId);

                if (packageInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy kiện nào chứa sản phẩm {productInfo.ProductCode} đơn vị {puInfo?.ProductUnitConversionName}");

                item.FromPackageId = packageInfo.PackageId;
            }

            if (newInventoryOutProductModel.Count > 0)
            {
                //var groupList = newInventoryOutProductModel.GroupBy(g => g.RefObjectCode).ToList();
                //var index = 1;

                //foreach (var g in groupList)
                //{
                //    var details = g.ToList();
                //    foreach (var d in details)
                //    {
                //        d.RefObjectCode = string.Format("PX_{0}", DateTime.UtcNow.ToString("ddMMyyyyHHmmss"));
                //    }

                //    var newInventory = new InventoryOutModel
                //    {
                //        StockId = _model.StockId,
                //        InventoryCode = string.Format("PX_TonDau_{0}_{1}", index, DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
                //        Date = _model.IssuedDate,
                //        Shipper = string.Empty,
                //        Content = "Xuất kho ban đầu từ excel",
                //        CustomerId = null,
                //        Department = string.Empty,
                //        StockKeeperUserId = null,
                //        BillCode = string.Empty,
                //        BillSerial = string.Empty,
                //        BillDate = _model.IssuedDate,
                //        FileIdList = null,
                //        OutProducts = details
                //    };

                //    inventoryOutList.Add(newInventory);

                //    index++;
                //}


                foreach (var d in newInventoryOutProductModel)
                {
                    d.RefObjectCode = string.Format("PX_{0}", DateTime.UtcNow.ToString("ddMMyyyyHHmmss"));
                }

                var newInventory = new InventoryOutModel
                {
                    StockId = _model.StockId,
                    InventoryCode = string.Format("PX_TonDau_{0}", DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
                    Date = _model.IssuedDate,
                    Shipper = string.Empty,
                    Content = "Xuất kho ban đầu từ excel",
                    CustomerId = null,
                    Department = string.Empty,
                    StockKeeperUserId = null,
                    BillCode = string.Empty,
                    BillSerial = string.Empty,
                    BillDate = _model.IssuedDate,
                    FileIdList = null,
                    OutProducts = newInventoryOutProductModel,
                    AccountancyAccountNumber = _model.AccountancyAccountNumber
                };

                inventoryOutList.Add(newInventory);


            }

            return inventoryOutList;
        }

        private async Task AddMissingProductCates()
        {
            var productCates = await _stockDbContext.ProductCate.AsNoTracking().ToListAsync();

            var existedProductCateNormalizeNames = productCates.Select(c => c.ProductCateName.NormalizeAsInternalName()).Distinct().ToHashSet();

            var importProductCates = _excelModel.Select(p => p.CateName).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();

            var newProductCates = importProductCates
                .Where(c => !existedProductCateNormalizeNames.Contains(c.NormalizeAsInternalName()))
                .Select(c => new ProductCate()
                {
                    ProductCateName = c,
                    ParentProductCateId = null,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false
                })
                .ToList();

            await _stockDbContext.ProductCate.AddRangeAsync(newProductCates);
            await _stockDbContext.SaveChangesAsync();

            productCates.AddRange(newProductCates);

            _productCates = new Dictionary<string, ProductCate>();
            foreach (var productCate in productCates)
            {
                var internalName = productCate.ProductCateName.NormalizeAsInternalName();
                if (!_productCates.ContainsKey(internalName))
                {
                    _productCates.Add(internalName, productCate);
                }
            }
        }

        private async Task AddMissingProductTypes()
        {
            var productTypes = await _stockDbContext.ProductType.AsNoTracking().ToListAsync();

            var existedProductTypeNormalizeNames = productTypes.Select(c => c.ProductTypeName.NormalizeAsInternalName()).Distinct().ToHashSet();

            var importProductTypes = _excelModel.Select(p => p.CatePrefixCode).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();

            var newProductTypes = importProductTypes
                .Where(c => !existedProductTypeNormalizeNames.Contains(c.NormalizeAsInternalName()))
                .Select(c => new ProductType()
                {
                    ProductTypeName = c,
                    IdentityCode = c,
                    ParentProductTypeId = null,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false
                })
                .ToList();

            await _stockDbContext.ProductType.AddRangeAsync(newProductTypes);
            await _stockDbContext.SaveChangesAsync();

            productTypes.AddRange(newProductTypes);

            _productTypes = new Dictionary<string, ProductType>();
            foreach (var productType in productTypes)
            {
                var internalName = productType.ProductTypeName.NormalizeAsInternalName();
                if (!_productTypes.ContainsKey(internalName))
                {
                    _productTypes.Add(internalName, productType);
                }
            }
        }


        private async Task AddMissingProducts()
        {
            var products = (await _stockDbContext.Product.AsNoTracking().ToListAsync()).GroupBy(p => p.ProductCode).Select(p => p.First()).ToList();


            var existedProductNormalizeCodes = products.Select(c => c.ProductCode.NormalizeAsInternalName()).Distinct().ToHashSet();

            var existedProductNormalizeNames = products.Select(c => c.ProductName.NormalizeAsInternalName()).Distinct().ToHashSet();


            var importedProductsByCode = _excelModel.Where(m => !string.IsNullOrWhiteSpace(m.ProductCode)).GroupBy(p => p.ProductCode).ToDictionary(p => p.Key, p => p.ToList());

            foreach (var productByCode in importedProductsByCode)
            {
                if (!existedProductNormalizeCodes.Contains(productByCode.Key.NormalizeAsInternalName()))
                {
                    var info = productByCode.Value.First();

                    if (existedProductNormalizeNames.Contains(info.ProductName.NormalizeAsInternalName()))
                    {
                        info.ProductName += " " + info.ProductCode;
                    }
                    await AddProduct(info, products, existedProductNormalizeCodes, existedProductNormalizeNames);
                }
            }

            _productsByCode = products.GroupBy(c => c.ProductCode.NormalizeAsInternalName()).ToDictionary(c => c.Key, c => c.First());

            _productUnitsByProduct = (await _stockDbContext.ProductUnitConversion.AsNoTracking().ToListAsync()).GroupBy(pu => pu.ProductId).ToDictionary(pu => pu.Key, pu => pu.ToList());

            foreach (var productByCode in importedProductsByCode)
            {
                var productInfo = _productsByCode[productByCode.Key.NormalizeAsInternalName()];

                var productUnits = productByCode.Value
                    .Where(u => !string.IsNullOrWhiteSpace(u.Unit2) && u.Qty1 > 0)
                    .Where(u => u.Qty2 > 0 || u.Factor > 0)
                    .GroupBy(u => u.Unit2)
                    .Select(u =>
                    {
                        var item = u.First();
                        var factor = item.Factor;
                        if (!(factor > 0))
                        {
                            factor = item.Qty2 / item.Qty1;
                        }
                        return new
                        {
                            UnitName = u.Key,
                            Factor = factor
                        };
                    }).ToList();

                var dbPus = _productUnitsByProduct[productInfo.ProductId].Select(pu => pu.ProductUnitConversionName.NormalizeAsInternalName()).Distinct().ToHashSet();

                var newPus = productUnits.Where(pu => !dbPus.Contains(pu.UnitName.NormalizeAsInternalName()))
                    .Select(pu =>
                    {
                        _units.TryGetValue(pu.UnitName.NormalizeAsInternalName(), out var secoundUnit);

                        return new ProductUnitConversion()
                        {
                            ProductUnitConversionName = pu.UnitName,
                            ProductId = productInfo.ProductId,
                            SecondaryUnitId = secoundUnit.UnitId,
                            FactorExpression = pu.Factor.ToString(),
                            ConversionDescription = pu.UnitName,
                            IsFreeStyle = false,
                            IsDefault = false
                        };
                    })
                    .ToList();

                await _stockDbContext.ProductUnitConversion.AddRangeAsync(newPus);
                await _stockDbContext.SaveChangesAsync();
                _productUnitsByProduct[productInfo.ProductId].AddRange(newPus);
            }

            _productsByName = products.GroupBy(c => c.ProductName.NormalizeAsInternalName()).ToDictionary(c => c.Key, c => c.First());

        }

        private async Task AddProduct(OpeningBalanceModel info, IList<Product> products, HashSet<string> existedProductNormalizeCodes, HashSet<string> existedProductNormalizeNames)
        {
            var p = CreateProductModel(info);
            _units.TryGetValue(info.Unit1.NormalizeAsInternalName(), out var unitInfo);
            p.UnitId = unitInfo.UnitId;

            p.Extra.Specification = info.Specification;

            var productId = await _productService.AddProductToDb(p);

            var productInfo = await _stockDbContext.Product.AsNoTracking().FirstOrDefaultAsync(t => t.ProductId == productId);

            existedProductNormalizeCodes.Add(productInfo.ProductCode.NormalizeAsInternalName());
            existedProductNormalizeNames.Add(productInfo.ProductName.NormalizeAsInternalName());

            products.Add(productInfo);
        }

        private ProductModel CreateProductModel(OpeningBalanceModel p)
        {
            _productTypes.TryGetValue(p.CatePrefixCode.NormalizeAsInternalName(), out var productType);

            _productCates.TryGetValue(p.CateName.NormalizeAsInternalName(), out var productCate);

            return new ProductModel()
            {

                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                IsCanBuy = true,
                IsCanSell = true,
                MainImageFileId = null,
                ProductTypeId = productType.ProductTypeId,
                ProductCateId = productCate.ProductCateId,
                BarcodeConfigId = null,
                BarcodeStandardId = null,
                Barcode = null,
                UnitId = 0,
                EstimatePrice = null,

                
                Height = p.Height,
                Long = p.Long,
                Width = p.Width,

                //GrossWeight = p.GrossWeight,
                //LoadAbility = p.LoadAbility,
                //NetWeight = p.NetWeight,
                //PackingMethod = p.PackingMethod,
                //Measurement = p.Measurement,
                //Quantitative = p.Quantitative,
                //QuantitativeUnitTypeId = (int?)p.QuantitativeUnitTypeId,

                Extra = new ProductModel.ProductModelExtra()
                {

                },
                StockInfo = new ProductModel.ProductModelStock() { },

            };
        }

        private async Task AddMissingUnit()
        {
            var units = await _masterDBContext.Unit.AsNoTracking().ToListAsync();

            var existedUnitNames = units.Select(c => c.UnitName.NormalizeAsInternalName()).Distinct().ToHashSet();

            var importUnits = _excelModel.SelectMany(p => new[] { p.Unit1, p.Unit2 }).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();

            var newUnits = importUnits
                .Where(c => !existedUnitNames.Contains(c.NormalizeAsInternalName()))
                .Select(c => new Unit()
                {
                    UnitName = c,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    UnitStatusId = (int)EnumUnitStatus.Using
                })
                .ToList();

            await _masterDBContext.Unit.AddRangeAsync(newUnits);
            await _masterDBContext.SaveChangesAsync();

            units.AddRange(newUnits);

            _units = new Dictionary<string, Unit>();
            foreach (var unit in units)
            {
                var internalName = unit.UnitName.NormalizeAsInternalName();
                if (!_units.ContainsKey(internalName))
                {
                    _units.Add(internalName, unit);
                }
            }
        }



    }

}
