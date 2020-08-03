using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
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

        private Dictionary<int, List<ProductUnitConversion>> _productUnits = null;


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


        public async Task<bool> ImportCustomerFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);


            var data = reader.ReadSheetEntity<OpeningBalanceModel>(mapping, null);

            var rowDatas = new List<List<ImportExcelRowData>>();

            for (var rowIndx = 0; rowIndx < data.Rows.Length; rowIndx++)
            {
                var row = data.Rows[rowIndx];

                var rowData = new List<ImportExcelRowData>();
                bool isIgnoreRow = false;
                for (int fieldIndx = 0; fieldIndx < mapping.MappingFields.Count && !isIgnoreRow; fieldIndx++)
                {
                    var mappingField = mapping.MappingFields[fieldIndx];

                    string value = null;
                    if (row.ContainsKey(mappingField.Column))
                        value = row[mappingField.Column]?.ToString();

                    if (string.IsNullOrWhiteSpace(value) && mappingField.IsRequire)
                    {
                        isIgnoreRow = true;
                        continue;
                    }

                    var field = fields.FirstOrDefault(f => f.Name == mappingField.FieldName);

                    if (field == null) throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy field {mappingField.FieldName}");



                    rowData.Add(new ImportExcelRowData()
                    {
                        FieldMapping = mappingField,
                        PropertyInfo = field,
                        CellValue = value
                    });
                }

                if (!isIgnoreRow)
                    rowDatas.Add(rowData);
            }


            using (var trans = await _organizationContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var insertedData = new Dictionary<int, CustomerModel>();

                    // Insert data
                    foreach (var rowData in rowDatas)
                    {
                        var rowInput = new Dictionary<string, string>();

                        var customerInfo = new CustomerModel()
                        {
                            CustomerStatusId = EnumCustomerStatus.Actived
                        };

                        foreach (var cellData in rowData)
                        {
                            if (string.IsNullOrWhiteSpace(cellData.FieldMapping.FieldName) || string.IsNullOrWhiteSpace(cellData.CellValue)) continue;

                            if (cellData.PropertyInfo.Name == nameof(CustomerModel.CustomerTypeId))
                            {
                                if (cellData.CellValue.NormalizeAsInternalName().Equals(EnumCustomerType.Personal.GetEnumDescription().NormalizeAsInternalName()))
                                {
                                    customerInfo.CustomerTypeId = EnumCustomerType.Personal;
                                }
                                else
                                {
                                    customerInfo.CustomerTypeId = EnumCustomerType.Organization;
                                }
                            }
                            else
                            {
                                cellData.PropertyInfo.SetValue(customerInfo, cellData.CellValue.ConvertValueByType(cellData.PropertyInfo.PropertyType));
                            }
                        }

                        var context = new ValidationContext(customerInfo);
                        ICollection<ValidationResult> results = new List<ValidationResult>();
                        bool isValid = Validator.TryValidateObject(customerInfo, context, results, true);
                        if (!isValid)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, string.Join(", ", results.FirstOrDefault()?.MemberNames) + ": " + results.FirstOrDefault()?.ErrorMessage);
                        }

                        var customerId = await AddCustomerToDb(customerInfo);

                        insertedData.Add(customerId, customerInfo);
                    }

                    trans.Commit();

                    foreach (var item in insertedData)
                    {
                        await _activityLogService.CreateLog(EnumObjectType.Customer, item.Key, $"Import đối tác {item.Value.CustomerName}", item.Value.JsonSerialize());
                    }

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "ImportCustomerFromMapping");
                    throw;
                }
            }

            return true;

        }

        private async Task<bool> ProcessInventoryInputExcelSheet(ImportExcelMapping mapping, Stream stream, InventoryOpeningBalanceModel model, int currentUserId)
        {
            var reader = new ExcelReader(stream);

            var currentCateName = string.Empty;
            var currentCatePrefixCode = string.Empty;

            var excelModel = reader.ReadSheetEntity<OpeningBalanceModel>(mapping, (entity, propertyName, value) =>
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

            await AddMissingProductCates(excelModel);
            await AddMissingProductTypes(excelModel);
            await AddMissingUnit(excelModel);

            await AddMissingProductCates(excelModel);

            var inventoryInputList = new List<InventoryInModel>();
            var inventoryInputModel = new InventoryInModel
            {
                InProducts = new List<InventoryInProductModel>(32)
            };
            var totalRowCount = excelModel.Count;

            var productDataList = new List<Product>(totalRowCount);
            var newInventoryInputModel = new List<InventoryInProductExtendModel>(totalRowCount);

            #region Cập nhật sản phẩm & các thông tin bổ sung
            foreach (var item in excelModel)
            {
                if (productDataList.Any(q => q.ProductCode == item.ProductCode))
                    continue;
                var productCateObj = productCateEntities.FirstOrDefault(q => q.ProductCateName == item.CateName);
                var productTypeObj = productTypeEntities.FirstOrDefault(q => q.IdentityCode == item.CatePrefixCode);
                var unitObj = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit1);
                var productEntity = new Product
                {
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    IsCanBuy = true,
                    IsCanSell = true,
                    MainImageFileId = null,
                    ProductTypeId = productTypeObj != null ? (int?)productTypeObj.ProductTypeId : null,
                    ProductCateId = productCateObj != null ? productCateObj.ProductCateId : 0,
                    BarcodeStandardId = null,
                    BarcodeConfigId = null,
                    Barcode = null,
                    UnitId = unitObj != null ? unitObj.UnitId : 0,
                    EstimatePrice = item.UnitPrice,
                    Long = item.Long,
                    Width = item.Width,
                    Height = item.Height,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false
                };
                productDataList.Add(productEntity);
            }

            var readBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(Product.ProductCode) } };
            _stockDbContext.BulkRead<Product>(productDataList, readBulkConfig);
            _stockDbContext.BulkInsertOrUpdate<Product>(productDataList, new BulkConfig { PreserveInsertOrder = false, SetOutputIdentity = true });

            // Cập nhật đơn vị chuyển đổi mặc định
            var defaultProductUnitConversionList = new List<ProductUnitConversion>(productDataList.Count);

            foreach (var p in productDataList)
            {
                if (p.ProductId > 0)
                {
                    var unitObj = unitDataList.FirstOrDefault(q => q.UnitId == p.UnitId);
                    if (unitObj != null)
                    {
                        var defaultProductUnitConversionEntity = new ProductUnitConversion()
                        {
                            ProductUnitConversionName = unitObj.UnitName,
                            ProductId = p.ProductId,
                            SecondaryUnitId = unitObj.UnitId,
                            FactorExpression = "1",
                            ConversionDescription = "Mặc định",
                            IsFreeStyle = false,
                            IsDefault = true
                        };
                        defaultProductUnitConversionList.Add(defaultProductUnitConversionEntity);
                    }
                }
            }
            var readDefaultProductUnitConversionBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductUnitConversion.ProductId), nameof(ProductUnitConversion.SecondaryUnitId), nameof(ProductUnitConversion.IsDefault) } };
            _stockDbContext.BulkRead<ProductUnitConversion>(defaultProductUnitConversionList, readDefaultProductUnitConversionBulkConfig);
            _stockDbContext.BulkInsert<ProductUnitConversion>(defaultProductUnitConversionList.Where(q => q.ProductUnitConversionId == 0).ToList(), new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true });

            #region Cập nhật mô tả sản phẩm & thông tin bổ sung
            var productExtraInfoList = new List<ProductExtraInfo>(productDataList.Count);
            var productExtraInfoModel = excelModel.Select(q => new { q.ProductCode, q.Specification }).GroupBy(g => g.ProductCode).Select(q => q.First()).ToList();
            foreach (var item in productExtraInfoModel)
            {
                var productObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);
                if (productObj != null)
                {
                    var productExtraInfoEntity = new ProductExtraInfo
                    {
                        ProductId = productObj.ProductId,
                        Specification = item.Specification,
                        Description = string.Empty,
                        IsDeleted = false
                    };
                    productExtraInfoList.Add(productExtraInfoEntity);
                }
            }
            var readProductExtraInfoBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductExtraInfo.ProductId) } };
            _stockDbContext.BulkRead<ProductExtraInfo>(productExtraInfoList, readProductExtraInfoBulkConfig);
            _stockDbContext.BulkInsertOrUpdate<ProductExtraInfo>(productExtraInfoList, new BulkConfig { PreserveInsertOrder = false, SetOutputIdentity = false });
            #endregion

            #region Cập nhật thông tin cảnh báo tồn kho của sản phẩm
            var productStockInfoList = new List<ProductStockInfo>(productDataList.Count);

            foreach (var p in productDataList)
            {
                if (p.ProductId > 0)
                {
                    var productStockInfo = new ProductStockInfo
                    {
                        ProductId = p.ProductId,
                        StockOutputRuleId = 0,
                        AmountWarningMin = 1,
                        AmountWarningMax = 1000000,
                        TimeWarningAmount = 0,
                        TimeWarningTimeTypeId = 4,
                        DescriptionToStock = string.Empty,
                        IsDeleted = false,
                        ExpireTimeTypeId = 4,
                        ExpireTimeAmount = 0,
                    };
                    productStockInfoList.Add(productStockInfo);
                }
            }
            var readProductStockInfoListBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductStockInfo.ProductId) } };
            _stockDbContext.BulkRead<ProductStockInfo>(productStockInfoList, readProductStockInfoListBulkConfig);
            _stockDbContext.BulkInsertOrUpdate<ProductStockInfo>(productStockInfoList, new BulkConfig { PreserveInsertOrder = false, SetOutputIdentity = false });
            #endregion

            #region Cập nhật đơn vị chuyển đổi - ProductUnitConversion
            var newProductUnitConversionList = new List<ProductUnitConversion>(productDataList.Count);
            foreach (var item in excelModel)
            {
                if (string.IsNullOrEmpty(item.ProductCode) || string.IsNullOrEmpty(item.Unit2) || item.Factor == 0)
                    continue;
                var unit1 = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit1);
                var unit2 = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit2);

                var productObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);

                if (item.Factor > 0 && productObj != null && unit1 != null && unit2 != null)
                {
                    var newProductUnitConversion = new ProductUnitConversion
                    {
                        ProductUnitConversionName = string.Format("{0}-{1}", unit2.UnitName, item.Factor).Replace(@",", ""),
                        ProductId = productObj.ProductId,
                        SecondaryUnitId = unit2.UnitId,
                        FactorExpression = item.Factor.ToString().Replace(@",", ""),
                        ConversionDescription = string.Format("{0} {1} {2}", unit1.UnitName, unit2.UnitName, item.Factor),
                        IsDefault = false
                    };

                    if (Utils.EvalPrimaryQuantityFromProductUnitConversionQuantity(0, newProductUnitConversion.FactorExpression) != 0
                        ||
                        Utils.EvalPrimaryQuantityFromProductUnitConversionQuantity(1, newProductUnitConversion.FactorExpression) <= 0
                        )
                    {
                        return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                    }

                    if (newProductUnitConversionList.Any(q => q.ProductUnitConversionName == newProductUnitConversion.ProductUnitConversionName && q.ProductId == newProductUnitConversion.ProductId))
                        continue;
                    else
                        newProductUnitConversionList.Add(newProductUnitConversion);
                }
            }
            var readProductUnitConversionBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductUnitConversion.ProductUnitConversionName), nameof(ProductUnitConversion.ProductId), nameof(ProductUnitConversion.IsDefault) } };
            _stockDbContext.BulkRead<ProductUnitConversion>(newProductUnitConversionList, readProductUnitConversionBulkConfig);
            _stockDbContext.BulkInsertOrUpdate<ProductUnitConversion>(newProductUnitConversionList, new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true });

            #endregion

            #endregion end db updating product & related data

            #region Tạo và xửa lý phiếu nhập kho

            newProductUnitConversionList = _stockDbContext.ProductUnitConversion.AsNoTracking().ToList();
            foreach (var item in excelModel)
            {
                if (string.IsNullOrEmpty(item.ProductCode))
                    continue;

                if (item.Qty1 == 0)
                    continue;
                ProductUnitConversion productUnitConversionObj = null;
                var productObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);

                if (!string.IsNullOrEmpty(item.Unit2) && item.Factor > 0)
                {
                    var unit2 = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit2);
                    if (unit2 != null && item.Factor > 0)
                    {
                        var factorExpression = item.Factor;
                        productUnitConversionObj = newProductUnitConversionList.FirstOrDefault(q => q.ProductId == productObj.ProductId && q.SecondaryUnitId == unit2.UnitId && q.FactorExpression == factorExpression.ToString() && !q.IsDefault);
                    }
                }
                else
                    productUnitConversionObj = newProductUnitConversionList.FirstOrDefault(q => q.ProductId == productObj.ProductId && q.IsDefault);

                newInventoryInputModel.Add(
                        new InventoryInProductExtendModel
                        {
                            ProductId = productObj != null ? productObj.ProductId : 0,
                            ProductCode = item.ProductCode,
                            ProductUnitConversionId = productUnitConversionObj.ProductUnitConversionId,
                            PrimaryQuantity = item.Qty1,
                            ProductUnitConversionQuantity = item.Qty2,
                            UnitPrice = item.UnitPrice,
                            RefObjectTypeId = null,
                            RefObjectId = null,
                            RefObjectCode = item.CatePrefixCode,
                            ToPackageId = null,
                            PackageOptionId = EnumPackageOption.NoPackageManager
                        }
                    ); ;
            }
            if (newInventoryInputModel.Count > 0)
            {
                var groupList = newInventoryInputModel.GroupBy(g => g.RefObjectCode).ToList();
                var index = 1;
                foreach (var g in groupList)
                {
                    var details = g.ToList();
                    var newInventory = new InventoryInModel
                    {
                        StockId = model.StockId,
                        InventoryCode = string.Format("PN_TonDau_{0}_{1}", index, DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
                        Date = model.IssuedDate,
                        Shipper = string.Empty,
                        Content = model.Description,
                        CustomerId = null,
                        Department = string.Empty,
                        StockKeeperUserId = null,
                        BillCode = string.Empty,
                        BillSerial = string.Empty,
                        BillDate = model.IssuedDate,
                        FileIdList = null,
                        InProducts = new List<InventoryInProductModel>(details.Count)
                    };
                    foreach (var item in details)
                    {
                        var currentProductObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);
                        newInventory.InProducts.Add(new InventoryInProductModel
                        {
                            ProductId = item.ProductId,
                            ProductUnitConversionId = item.ProductUnitConversionId,
                            PrimaryQuantity = item.PrimaryQuantity,
                            ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
                            UnitPrice = item.UnitPrice,
                            RefObjectTypeId = item.RefObjectTypeId,
                            RefObjectId = item.RefObjectId,
                            RefObjectCode = string.Format("PN_TonDau_{0}_{1}_{2}", index, DateTime.UtcNow.ToString("ddMMyyyyHHmmss"), item.RefObjectCode),
                            ToPackageId = null,
                            PackageOptionId = EnumPackageOption.NoPackageManager
                        });
                    }
                    inventoryInputList.Add(newInventory);
                    index++;
                }
            }

            if (inventoryInputList.Count > 0)
            {
                foreach (var item in inventoryInputList)
                {
                    var ret = await _inventoryService.AddInventoryInput(item);
                    if (ret > 0)
                    {
                        // Duyệt phiếu nhập kho
                        //await _inventoryService.ApproveInventoryInput(ret.Data, currentUserId); 
                        continue;
                    }
                    else
                    {
                        _logger.LogWarning(string.Format("ProcessInventoryInputExcelSheet not success, please recheck -> AddInventoryInput: {0}", item.InventoryCode));
                    }
                }
            }
            #endregion

            return GeneralCode.Success;

        }



        private async Task AddMissingProductCates(IList<OpeningBalanceModel> excelModel)
        {
            var productCates = await _stockDbContext.ProductCate.AsNoTracking().ToListAsync();

            var existedProductCateNormalizeNames = productCates.Select(c => c.ProductCateName.NormalizeAsInternalName()).Distinct().ToHashSet();

            var importProductCates = excelModel.Select(p => p.CateName).Distinct().ToList();

            var newProductCates = importProductCates
                .Where(c => !existedProductCateNormalizeNames.Contains(c.NormalizeAsInternalName()))
                .Select(c => new ProductCate()
                {
                    ProductCateName = c,
                    ParentProductCateId = null,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false
                });

            await _stockDbContext.ProductCate.AddRangeAsync(newProductCates);
            await _stockDbContext.SaveChangesAsync();

            productCates.AddRange(newProductCates);

            _productCates = new Dictionary<string, ProductCate>();
            foreach (var productCate in productCates)
            {
                var internalName = productCate.ProductCateName.NormalizeAsInternalName();
                if (!_productTypes.ContainsKey(internalName))
                {
                    _productCates.Add(internalName, productCate);
                }
            }
        }

        private async Task AddMissingProductTypes(IList<OpeningBalanceModel> excelModel)
        {
            var productTypes = await _stockDbContext.ProductType.AsNoTracking().ToListAsync();

            var existedProductTypeNormalizeNames = productTypes.Select(c => c.ProductTypeName.NormalizeAsInternalName()).Distinct().ToHashSet();

            var importProductTypes = excelModel.Select(p => p.CatePrefixCode).Distinct().ToList();

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
                });

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


        private async Task AddMissingProducts(IList<OpeningBalanceModel> excelModel)
        {
            var products = await _stockDbContext.Product.AsNoTracking().ToListAsync();

            _productUnits = (await _stockDbContext.ProductUnitConversion.AsNoTracking().ToListAsync()).GroupBy(pu => pu.ProductId).ToDictionary(pu => pu.Key, pu => pu.ToList());

            var existedProductNormalizeCodes = products.Select(c => c.ProductCode.NormalizeAsInternalName()).Distinct().ToHashSet();

            var existedProductNormalizeNames = products.Select(c => c.ProductName.NormalizeAsInternalName()).Distinct().ToHashSet();

            var importedProductsByCode = excelModel.GroupBy(p => p.ProductCode).ToDictionary(p => p.Key, p => p.ToList());

            var importedProductsByName = excelModel.GroupBy(p => p.ProductName).ToDictionary(p => p.Key, p => p.ToList());

            foreach (var productByCode in importedProductsByCode)
            {
                if (!existedProductNormalizeCodes.Contains(productByCode.Key.NormalizeAsInternalName()))
                {
                    var info = productByCode.Value.First();

                    var p = CreateProductModel(info);
                    _units.TryGetValue(info.Unit1, out var unitInfo);
                    p.UnitId = unitInfo.UnitId;

                    p.Extra.Specification = info.Specification;

                    var productId = await _productService.AddProductToDb(p);

                    var productInfo = await _stockDbContext.Product.AsNoTracking().FirstOrDefaultAsync(t => t.ProductId == productId);

                    existedProductNormalizeCodes.Add(productInfo.ProductCode.NormalizeAsInternalName());
                    existedProductNormalizeNames.Add(productInfo.ProductName.NormalizeAsInternalName());

                    products.Add(productInfo);
                }
            }

            foreach (var productByName in importedProductsByName)
            {
                if (!existedProductNormalizeNames.Contains(productByName.Key.NormalizeAsInternalName()))
                {
                    var info = productByName.Value.First();

                    var p = CreateProductModel(info);
                    _units.TryGetValue(info.Unit1, out var unitInfo);
                    p.UnitId = unitInfo.UnitId;

                    p.Extra.Specification = info.Specification;

                    var productId = await _productService.AddProductToDb(p);

                    var productInfo = await _stockDbContext.Product.AsNoTracking().FirstOrDefaultAsync(t => t.ProductId == productId);

                    existedProductNormalizeCodes.Add(productInfo.ProductCode.NormalizeAsInternalName());
                    existedProductNormalizeNames.Add(productInfo.ProductName.NormalizeAsInternalName());

                    products.Add(productInfo);
                }
            }

            var productByCodeNormalize = products.ToDictionary(p => p.ProductCode.NormalizeAsInternalName(), p => p);

            foreach (var productByCode in importedProductsByCode)
            {
                var productInfo = productByCodeNormalize[productByCode.Key.NormalizeAsInternalName()];

                var productUnits = productByCode.Value
                    .GroupBy(u => u.Unit2)
                    .Select(u => new
                    {
                        UnitName = u.Key,
                        u.First().Factor
                    }).ToList();

                var pus = _productUnits[productInfo.ProductId].Select(pu => pu.ProductUnitConversionName.NormalizeAsInternalName()).Distinct().ToHashSet();
                var newPus= productUnits.Where(pu => !pus.Contains(pu.UnitName.NormalizeAsInternalName())).Select(pu => {

                    _units.TryGetValue(pu.UnitName, out var secoundUnit);

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
                    });
                await _stockDbContext.ProductUnitConversion.AddRangeAsync(newPus);
                await _stockDbContext.SaveChangesAsync();
                _productUnits[productInfo.ProductId].AddRange(newPus);
            }


            _productsByCode = products.GroupBy(c => c.ProductCode).ToDictionary(c => c.Key.NormalizeAsInternalName(), c => c.First());

            _productsByName = products.GroupBy(c => c.ProductName).ToDictionary(c => c.Key.NormalizeAsInternalName(), c => c.First());

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

                Extra = new ProductModel.ProductModelExtra()
                {

                },
                StockInfo = new ProductModel.ProductModelStock() { },

            };
        }

        private async Task AddMissingUnit(IList<OpeningBalanceModel> excelModel)
        {
            var units = await _masterDBContext.Unit.AsNoTracking().ToListAsync();

            var existedUnitNames = units.Select(c => c.UnitName.NormalizeAsInternalName()).Distinct().ToHashSet();

            var importUnits = excelModel.SelectMany(p => new[] { p.Unit1, p.Unit2 }).Distinct().ToList();

            var newUnits = importUnits
                .Where(c => !existedUnitNames.Contains(c.NormalizeAsInternalName()))
                .Select(c => new Unit()
                {
                    UnitName = c,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    UnitStatusId = (int)EnumUnitStatus.Using
                });

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
