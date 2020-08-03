//using NPOI.SS.UserModel;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using VErp.Commons.Enums.MasterEnum;
//using VErp.Commons.Enums.StandardEnum;
//using VErp.Commons.GlobalObject;
//using VErp.Commons.Library;
//using VErp.Commons.Library.Model;
//using VErp.Infrastructure.EF.StockDB;
//using VErp.Infrastructure.ServiceCore.Model;
//using VErp.Services.Stock.Model.Inventory;
//using VErp.Services.Stock.Model.Inventory.OpeningBalance;

//namespace VErp.Services.Stock.Service.Stock.Implement.InventoryFileData
//{
//    public class InventoryImportFacade
//    {
//        private StockDBContext _stockDbContext;
//        public InventoryImportFacade SetStockDBContext(StockDBContext stockDbContext)
//        {
//            _stockDbContext = stockDbContext;
//            return this;
//        }



//        public async Task<bool> ImportCustomerFromMapping(ImportExcelMapping mapping, Stream stream)
//        {
//            var reader = new ExcelReader(stream);

//            var fields = typeof(CustomerModel).GetProperties();

//            var data = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

//            var rowDatas = new List<List<ImportExcelRowData>>();

//            for (var rowIndx = 0; rowIndx < data.Rows.Length; rowIndx++)
//            {
//                var row = data.Rows[rowIndx];

//                var rowData = new List<ImportExcelRowData>();
//                bool isIgnoreRow = false;
//                for (int fieldIndx = 0; fieldIndx < mapping.MappingFields.Count && !isIgnoreRow; fieldIndx++)
//                {
//                    var mappingField = mapping.MappingFields[fieldIndx];

//                    string value = null;
//                    if (row.ContainsKey(mappingField.Column))
//                        value = row[mappingField.Column]?.ToString();

//                    if (string.IsNullOrWhiteSpace(value) && mappingField.IsRequire)
//                    {
//                        isIgnoreRow = true;
//                        continue;
//                    }

//                    var field = fields.FirstOrDefault(f => f.Name == mappingField.FieldName);

//                    if (field == null) throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy field {mappingField.FieldName}");



//                    rowData.Add(new ImportExcelRowData()
//                    {
//                        FieldMapping = mappingField,
//                        PropertyInfo = field,
//                        CellValue = value
//                    });
//                }

//                if (!isIgnoreRow)
//                    rowDatas.Add(rowData);
//            }


//            using (var trans = await _organizationContext.Database.BeginTransactionAsync())
//            {
//                try
//                {
//                    var insertedData = new Dictionary<int, CustomerModel>();

//                    // Insert data
//                    foreach (var rowData in rowDatas)
//                    {
//                        var rowInput = new Dictionary<string, string>();

//                        var customerInfo = new CustomerModel()
//                        {
//                            CustomerStatusId = EnumCustomerStatus.Actived
//                        };

//                        foreach (var cellData in rowData)
//                        {
//                            if (string.IsNullOrWhiteSpace(cellData.FieldMapping.FieldName) || string.IsNullOrWhiteSpace(cellData.CellValue)) continue;

//                            if (cellData.PropertyInfo.Name == nameof(CustomerModel.CustomerTypeId))
//                            {
//                                if (cellData.CellValue.NormalizeAsInternalName().Equals(EnumCustomerType.Personal.GetEnumDescription().NormalizeAsInternalName()))
//                                {
//                                    customerInfo.CustomerTypeId = EnumCustomerType.Personal;
//                                }
//                                else
//                                {
//                                    customerInfo.CustomerTypeId = EnumCustomerType.Organization;
//                                }
//                            }
//                            else
//                            {
//                                cellData.PropertyInfo.SetValue(customerInfo, cellData.CellValue.ConvertValueByType(cellData.PropertyInfo.PropertyType));
//                            }
//                        }

//                        var context = new ValidationContext(customerInfo);
//                        ICollection<ValidationResult> results = new List<ValidationResult>();
//                        bool isValid = Validator.TryValidateObject(customerInfo, context, results, true);
//                        if (!isValid)
//                        {
//                            throw new BadRequestException(GeneralCode.InvalidParams, string.Join(", ", results.FirstOrDefault()?.MemberNames) + ": " + results.FirstOrDefault()?.ErrorMessage);
//                        }

//                        var customerId = await AddCustomerToDb(customerInfo);

//                        insertedData.Add(customerId, customerInfo);
//                    }

//                    trans.Commit();

//                    foreach (var item in insertedData)
//                    {
//                        await _activityLogService.CreateLog(EnumObjectType.Customer, item.Key, $"Import đối tác {item.Value.CustomerName}", item.Value.JsonSerialize());
//                    }

//                }
//                catch (Exception ex)
//                {
//                    trans.Rollback();
//                    _logger.LogError(ex, "ImportCustomerFromMapping");
//                    throw;
//                }
//            }

//            return true;

//        }

//        private async Task<bool> ProcessInventoryInputExcelSheet(ImportExcelMapping mapping, Stream stream, InventoryOpeningBalanceModel model, int currentUserId)
//        {
//            var reader = new ExcelReader(stream);


//            var rowDatas = reader.ReadSheetData<OpeningBalanceModel>(mapping);


//            var inventoryInputList = new List<InventoryInModel>();
//            InventoryInModel inventoryInputModel = new InventoryInModel
//            {
//                InProducts = new List<InventoryInProductModel>(32)
//            };
//            var totalRowCount = rowDatas.Count;

//            var excelModel = new List<OpeningBalanceModel>(totalRowCount);

//            var productDataList = new List<Product>(totalRowCount);
//            var newInventoryInputModel = new List<InventoryInProductExtendModel>(totalRowCount);

//            var currentCateName = string.Empty;
//            var currentCatePrefixCode = string.Empty;
//            var cateName = string.Empty;
//            var catePrefixCode = string.Empty;


//            foreach (var row in rowDatas)
//            {
//                if (row == null) continue;

//                var cellCateName = row. row.GetCell(0);
//                var cellCatePreifxCode = row.GetCell(1);
//                cateName = cellCateName != null ? HelperCellGetStringValue(cellCateName) : string.Empty;
//                catePrefixCode = cellCatePreifxCode != null ? HelperCellGetStringValue(cellCatePreifxCode) : string.Empty;
//                if (!string.IsNullOrEmpty(cateName))
//                {
//                    currentCateName = cateName;
//                }
//                if (!string.IsNullOrEmpty(catePrefixCode))
//                {
//                    currentCatePrefixCode = catePrefixCode;
//                }
//                var cellProductCode = row.GetCell(2);
//                if (cellProductCode == null)
//                    continue;
//                var productCode = cellProductCode != null ? HelperCellGetStringValue(cellProductCode) : string.Empty;
//                if (string.IsNullOrEmpty(productCode))
//                    continue;
//                #region Get All Cell value
//                var productName = row.GetCell(3) != null ? HelperCellGetStringValue(row.GetCell(3)) : string.Empty;
//                var cellUnit = row.GetCell(4);

//                var unitName = cellUnit != null ? HelperCellGetStringValue(cellUnit) : string.Empty;
//                if (string.IsNullOrEmpty(unitName))
//                    continue;

//                var cellUnitAlt = row.GetCell(9);
//                var unitAltName = cellUnitAlt != null ? HelperCellGetStringValue(cellUnitAlt) : string.Empty;
//                var qTy = row.GetCell(5) != null ? HelperCellGetNumericValue(row.GetCell(5)) : 0;
//                var unitPrice = row.GetCell(6) != null ? (decimal)HelperCellGetNumericValue(row.GetCell(6)) : 0;
//                var qTy2 = row.GetCell(11) != null ? HelperCellGetNumericValue(row.GetCell(11)) : 0;
//                var factor = row.GetCell(10) != null ? HelperCellGetNumericValue(row.GetCell(10)) : 0;
//                var specification = row.GetCell(8) != null ? HelperCellGetStringValue(row.GetCell(8)) : string.Empty;
//                var heightSize = row.GetCell(13) != null ? HelperCellGetNumericValue(row.GetCell(13)) : 0;
//                var widthSize = row.GetCell(14) != null ? HelperCellGetNumericValue(row.GetCell(14)) : 0;
//                var longSize = row.GetCell(15) != null ? HelperCellGetNumericValue(row.GetCell(15)) : 0;

//                var cellItem = new OpeningBalanceModel
//                {
//                    CateName = currentCateName,
//                    CatePrefixCode = currentCatePrefixCode,
//                    ProductCode = productCode,
//                    ProductName = productName,
//                    Unit1 = unitName.ToLower(),
//                    Qty1 = qTy,
//                    UnitPrice = unitPrice,
//                    Specification = specification,
//                    Unit2 = unitAltName.ToLower(),
//                    Qty2 = qTy2,
//                    Factor = factor,
//                    Height = heightSize,
//                    Width = widthSize,
//                    Long = longSize
//                };
//                excelModel.Add(cellItem);
//                #endregion
//            } // end for loop


//            #region Cập nhật ProductCate && ProductType
//            var productCateNameModelList = excelModel.GroupBy(g => g.CateName).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.CateName)).Select(q => q.CateName).ToList();
//            var productCateEntities = new List<ProductCate>(productCateNameModelList.Count);
//            foreach (var item in productCateNameModelList)
//            {
//                var exists = _stockDbContext.ProductCate.Any(q => q.ProductCateName == item);
//                if (!exists)
//                {
//                    var newCate = new ProductCate
//                    {
//                        ProductCateName = item,
//                        ParentProductCateId = null,
//                        CreatedDatetimeUtc = DateTime.UtcNow,
//                        UpdatedDatetimeUtc = DateTime.UtcNow,
//                        IsDeleted = false
//                    };
//                    _stockDbContext.ProductCate.Add(newCate);
//                }
//            }
//            _stockDbContext.SaveChanges();
//            productCateEntities = _stockDbContext.ProductCate.AsNoTracking().ToList();

//            // Thêm Cate prefix ProductType
//            var productTypeModelList = excelModel.GroupBy(g => g.CatePrefixCode).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.CatePrefixCode)).Select(q => q.CatePrefixCode).ToList();
//            var productTypeEntities = new List<ProductType>(productTypeModelList.Count);

//            foreach (var item in productTypeModelList)
//            {
//                var exists = _stockDbContext.ProductType.Any(q => q.ProductTypeName == item);
//                if (!exists)
//                {
//                    var newProductType = new ProductType
//                    {
//                        ProductTypeName = item,
//                        ParentProductTypeId = null,
//                        IdentityCode = item,
//                        CreatedDatetimeUtc = DateTime.UtcNow,
//                        UpdatedDatetimeUtc = DateTime.UtcNow,
//                        IsDeleted = false
//                    };
//                    _stockDbContext.ProductType.Add(newProductType);
//                }
//            }
//            _stockDbContext.SaveChanges();
//            productTypeEntities = _stockDbContext.ProductType.AsNoTracking().ToList();

//            #endregion

//            #region Cập nhật đơn vị tính chính & phụ
//            var unit1ModelList = excelModel.GroupBy(g => g.Unit1).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.Unit1)).Select(q => q.Unit1).ToList();
//            var unit2ModelList = excelModel.GroupBy(g => g.Unit2).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.Unit2)).Select(q => q.Unit2).ToList();
//            var unitModelList = unit1ModelList.Union(unit2ModelList).GroupBy(g => g.ToLower()).Select(q => q.First());
//            foreach (var u in unitModelList)
//            {
//                var exists = _masterDBContext.Unit.Any(q => q.UnitName == u);
//                if (!exists)
//                {
//                    var newUnit = new Unit
//                    {
//                        UnitName = u,
//                        IsDeleted = false,
//                        CreatedDatetimeUtc = DateTime.UtcNow,
//                        UpdatedDatetimeUtc = DateTime.UtcNow
//                    };
//                    _masterDBContext.Unit.Add(newUnit);
//                }
//            }
//            _masterDBContext.SaveChanges();
//            var unitDataList = _masterDBContext.Unit.AsNoTracking().ToList();
//            #endregion

//            #region Cập nhật sản phẩm & các thông tin bổ sung
//            foreach (var item in excelModel)
//            {
//                if (productDataList.Any(q => q.ProductCode == item.ProductCode))
//                    continue;
//                var productCateObj = productCateEntities.FirstOrDefault(q => q.ProductCateName == item.CateName);
//                var productTypeObj = productTypeEntities.FirstOrDefault(q => q.IdentityCode == item.CatePrefixCode);
//                var unitObj = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit1);
//                var productEntity = new Product
//                {
//                    ProductCode = item.ProductCode,
//                    ProductName = item.ProductName,
//                    IsCanBuy = true,
//                    IsCanSell = true,
//                    MainImageFileId = null,
//                    ProductTypeId = productTypeObj != null ? (int?)productTypeObj.ProductTypeId : null,
//                    ProductCateId = productCateObj != null ? productCateObj.ProductCateId : 0,
//                    BarcodeStandardId = null,
//                    BarcodeConfigId = null,
//                    Barcode = null,
//                    UnitId = unitObj != null ? unitObj.UnitId : 0,
//                    EstimatePrice = item.UnitPrice,
//                    Long = item.Long,
//                    Width = item.Width,
//                    Height = item.Height,
//                    CreatedDatetimeUtc = DateTime.UtcNow,
//                    UpdatedDatetimeUtc = DateTime.UtcNow,
//                    IsDeleted = false
//                };
//                productDataList.Add(productEntity);
//            }

//            var readBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(Product.ProductCode) } };
//            _stockDbContext.BulkRead<Product>(productDataList, readBulkConfig);
//            _stockDbContext.BulkInsertOrUpdate<Product>(productDataList, new BulkConfig { PreserveInsertOrder = false, SetOutputIdentity = true });

//            // Cập nhật đơn vị chuyển đổi mặc định
//            var defaultProductUnitConversionList = new List<ProductUnitConversion>(productDataList.Count);

//            foreach (var p in productDataList)
//            {
//                if (p.ProductId > 0)
//                {
//                    var unitObj = unitDataList.FirstOrDefault(q => q.UnitId == p.UnitId);
//                    if (unitObj != null)
//                    {
//                        var defaultProductUnitConversionEntity = new ProductUnitConversion()
//                        {
//                            ProductUnitConversionName = unitObj.UnitName,
//                            ProductId = p.ProductId,
//                            SecondaryUnitId = unitObj.UnitId,
//                            FactorExpression = "1",
//                            ConversionDescription = "Mặc định",
//                            IsFreeStyle = false,
//                            IsDefault = true
//                        };
//                        defaultProductUnitConversionList.Add(defaultProductUnitConversionEntity);
//                    }
//                }
//            }
//            var readDefaultProductUnitConversionBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductUnitConversion.ProductId), nameof(ProductUnitConversion.SecondaryUnitId), nameof(ProductUnitConversion.IsDefault) } };
//            _stockDbContext.BulkRead<ProductUnitConversion>(defaultProductUnitConversionList, readDefaultProductUnitConversionBulkConfig);
//            _stockDbContext.BulkInsert<ProductUnitConversion>(defaultProductUnitConversionList.Where(q => q.ProductUnitConversionId == 0).ToList(), new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true });

//            #region Cập nhật mô tả sản phẩm & thông tin bổ sung
//            var productExtraInfoList = new List<ProductExtraInfo>(productDataList.Count);
//            var productExtraInfoModel = excelModel.Select(q => new { q.ProductCode, q.Specification }).GroupBy(g => g.ProductCode).Select(q => q.First()).ToList();
//            foreach (var item in productExtraInfoModel)
//            {
//                var productObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);
//                if (productObj != null)
//                {
//                    var productExtraInfoEntity = new ProductExtraInfo
//                    {
//                        ProductId = productObj.ProductId,
//                        Specification = item.Specification,
//                        Description = string.Empty,
//                        IsDeleted = false
//                    };
//                    productExtraInfoList.Add(productExtraInfoEntity);
//                }
//            }
//            var readProductExtraInfoBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductExtraInfo.ProductId) } };
//            _stockDbContext.BulkRead<ProductExtraInfo>(productExtraInfoList, readProductExtraInfoBulkConfig);
//            _stockDbContext.BulkInsertOrUpdate<ProductExtraInfo>(productExtraInfoList, new BulkConfig { PreserveInsertOrder = false, SetOutputIdentity = false });
//            #endregion

//            #region Cập nhật thông tin cảnh báo tồn kho của sản phẩm
//            var productStockInfoList = new List<ProductStockInfo>(productDataList.Count);

//            foreach (var p in productDataList)
//            {
//                if (p.ProductId > 0)
//                {
//                    var productStockInfo = new ProductStockInfo
//                    {
//                        ProductId = p.ProductId,
//                        StockOutputRuleId = 0,
//                        AmountWarningMin = 1,
//                        AmountWarningMax = 1000000,
//                        TimeWarningAmount = 0,
//                        TimeWarningTimeTypeId = 4,
//                        DescriptionToStock = string.Empty,
//                        IsDeleted = false,
//                        ExpireTimeTypeId = 4,
//                        ExpireTimeAmount = 0,
//                    };
//                    productStockInfoList.Add(productStockInfo);
//                }
//            }
//            var readProductStockInfoListBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductStockInfo.ProductId) } };
//            _stockDbContext.BulkRead<ProductStockInfo>(productStockInfoList, readProductStockInfoListBulkConfig);
//            _stockDbContext.BulkInsertOrUpdate<ProductStockInfo>(productStockInfoList, new BulkConfig { PreserveInsertOrder = false, SetOutputIdentity = false });
//            #endregion

//            #region Cập nhật đơn vị chuyển đổi - ProductUnitConversion
//            var newProductUnitConversionList = new List<ProductUnitConversion>(productDataList.Count);
//            foreach (var item in excelModel)
//            {
//                if (string.IsNullOrEmpty(item.ProductCode) || string.IsNullOrEmpty(item.Unit2) || item.Factor == 0)
//                    continue;
//                var unit1 = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit1);
//                var unit2 = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit2);

//                var productObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);

//                if (item.Factor > 0 && productObj != null && unit1 != null && unit2 != null)
//                {
//                    var newProductUnitConversion = new ProductUnitConversion
//                    {
//                        ProductUnitConversionName = string.Format("{0}-{1}", unit2.UnitName, item.Factor).Replace(@",", ""),
//                        ProductId = productObj.ProductId,
//                        SecondaryUnitId = unit2.UnitId,
//                        FactorExpression = item.Factor.ToString().Replace(@",", ""),
//                        ConversionDescription = string.Format("{0} {1} {2}", unit1.UnitName, unit2.UnitName, item.Factor),
//                        IsDefault = false
//                    };

//                    if (Utils.EvalPrimaryQuantityFromProductUnitConversionQuantity(0, newProductUnitConversion.FactorExpression) != 0
//                        ||
//                        Utils.EvalPrimaryQuantityFromProductUnitConversionQuantity(1, newProductUnitConversion.FactorExpression) <= 0
//                        )
//                    {
//                        return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
//                    }

//                    if (newProductUnitConversionList.Any(q => q.ProductUnitConversionName == newProductUnitConversion.ProductUnitConversionName && q.ProductId == newProductUnitConversion.ProductId))
//                        continue;
//                    else
//                        newProductUnitConversionList.Add(newProductUnitConversion);
//                }
//            }
//            var readProductUnitConversionBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductUnitConversion.ProductUnitConversionName), nameof(ProductUnitConversion.ProductId), nameof(ProductUnitConversion.IsDefault) } };
//            _stockDbContext.BulkRead<ProductUnitConversion>(newProductUnitConversionList, readProductUnitConversionBulkConfig);
//            _stockDbContext.BulkInsertOrUpdate<ProductUnitConversion>(newProductUnitConversionList, new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true });

//            #endregion

//            #endregion end db updating product & related data

//            #region Tạo và xửa lý phiếu nhập kho

//            newProductUnitConversionList = _stockDbContext.ProductUnitConversion.AsNoTracking().ToList();
//            foreach (var item in excelModel)
//            {
//                if (string.IsNullOrEmpty(item.ProductCode))
//                    continue;

//                if (item.Qty1 == 0)
//                    continue;
//                ProductUnitConversion productUnitConversionObj = null;
//                var productObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);

//                if (!string.IsNullOrEmpty(item.Unit2) && item.Factor > 0)
//                {
//                    var unit2 = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit2);
//                    if (unit2 != null && item.Factor > 0)
//                    {
//                        var factorExpression = item.Factor;
//                        productUnitConversionObj = newProductUnitConversionList.FirstOrDefault(q => q.ProductId == productObj.ProductId && q.SecondaryUnitId == unit2.UnitId && q.FactorExpression == factorExpression.ToString() && !q.IsDefault);
//                    }
//                }
//                else
//                    productUnitConversionObj = newProductUnitConversionList.FirstOrDefault(q => q.ProductId == productObj.ProductId && q.IsDefault);

//                newInventoryInputModel.Add(
//                        new InventoryInProductExtendModel
//                        {
//                            ProductId = productObj != null ? productObj.ProductId : 0,
//                            ProductCode = item.ProductCode,
//                            ProductUnitConversionId = productUnitConversionObj.ProductUnitConversionId,
//                            PrimaryQuantity = item.Qty1,
//                            ProductUnitConversionQuantity = item.Qty2,
//                            UnitPrice = item.UnitPrice,
//                            RefObjectTypeId = null,
//                            RefObjectId = null,
//                            RefObjectCode = item.CatePrefixCode,
//                            ToPackageId = null,
//                            PackageOptionId = EnumPackageOption.NoPackageManager
//                        }
//                    ); ;
//            }
//            if (newInventoryInputModel.Count > 0)
//            {
//                var groupList = newInventoryInputModel.GroupBy(g => g.RefObjectCode).ToList();
//                var index = 1;
//                foreach (var g in groupList)
//                {
//                    var details = g.ToList();
//                    var newInventory = new InventoryInModel
//                    {
//                        StockId = model.StockId,
//                        InventoryCode = string.Format("PN_TonDau_{0}_{1}", index, DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
//                        Date = model.IssuedDate,
//                        Shipper = string.Empty,
//                        Content = model.Description,
//                        CustomerId = null,
//                        Department = string.Empty,
//                        StockKeeperUserId = null,
//                        BillCode = string.Empty,
//                        BillSerial = string.Empty,
//                        BillDate = model.IssuedDate,
//                        FileIdList = null,
//                        InProducts = new List<InventoryInProductModel>(details.Count)
//                    };
//                    foreach (var item in details)
//                    {
//                        var currentProductObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);
//                        newInventory.InProducts.Add(new InventoryInProductModel
//                        {
//                            ProductId = item.ProductId,
//                            ProductUnitConversionId = item.ProductUnitConversionId,
//                            PrimaryQuantity = item.PrimaryQuantity,
//                            ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
//                            UnitPrice = item.UnitPrice,
//                            RefObjectTypeId = item.RefObjectTypeId,
//                            RefObjectId = item.RefObjectId,
//                            RefObjectCode = string.Format("PN_TonDau_{0}_{1}_{2}", index, DateTime.UtcNow.ToString("ddMMyyyyHHmmss"), item.RefObjectCode),
//                            ToPackageId = null,
//                            PackageOptionId = EnumPackageOption.NoPackageManager
//                        });
//                    }
//                    inventoryInputList.Add(newInventory);
//                    index++;
//                }
//            }

//            if (inventoryInputList.Count > 0)
//            {
//                foreach (var item in inventoryInputList)
//                {
//                    var ret = await _inventoryService.AddInventoryInput(item);
//                    if (ret > 0)
//                    {
//                        // Duyệt phiếu nhập kho
//                        //await _inventoryService.ApproveInventoryInput(ret.Data, currentUserId); 
//                        continue;
//                    }
//                    else
//                    {
//                        _logger.LogWarning(string.Format("ProcessInventoryInputExcelSheet not success, please recheck -> AddInventoryInput: {0}", item.InventoryCode));
//                    }
//                }
//            }
//            #endregion

//            return GeneralCode.Success;

//        }

//    }

//}
