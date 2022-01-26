﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Stock.Product;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Services.Stock.Model.Product;
using static Verp.Resources.Stock.Product.ProductValidationMessage;

namespace VErp.Services.Stock.Service.Products.Implement.ProductFacade
{
    public class ProductImportFacade
    {
        const int DECIMAL_PLACE_DEFAULT = 11;

        private StockDBContext _stockContext;
        private MasterDBContext _masterDBContext;
        private IOrganizationHelperService _organizationHelperService;
        private ObjectActivityLogFacade _productActivityLog;

        public ProductImportFacade(StockDBContext stockContext, MasterDBContext masterDBContext, IOrganizationHelperService organizationHelperService, ObjectActivityLogFacade productActivityLog)
        {
            _stockContext = stockContext;
            _masterDBContext = masterDBContext;
            _organizationHelperService = organizationHelperService;
            _productActivityLog = productActivityLog;
        }


        IDictionary<string, int?> customerByCodes = null;
        IDictionary<string, int?> customerByNames = null;

        IDictionary<string, ProductType> productTypes = null;
        IDictionary<string, ProductCate> productCates = null;
        int? defaultTypeId = null;
        int? defaultCateId = null;

        IDictionary<string, int> units = null;
        IDictionary<int, Unit> unitInfos = null;

        string productUnitConversionNamePropPrefix = nameof(ProductImportModel.SecondaryUnit02)[..^2];
        string productUnitExpressionPropPrefix = nameof(ProductImportModel.FactorExpression02)[..^2];
        string productUnitDecimalPlacePropPrefix = nameof(ProductImportModel.DecimalPlace02)[..^2];

        Type typeInfo = typeof(ProductImportModel);

        public async Task<bool> ImportProductFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            // Lấy thông tin field
            var fields = typeof(Product).GetProperties(BindingFlags.Public);

            productTypes = _stockContext.ProductType.ToList().Select(t => new { IdentityCode = t.IdentityCode.NormalizeAsInternalName(), ProductType = t }).GroupBy(t => t.IdentityCode).ToDictionary(t => t.Key, t => t.First().ProductType);
            productCates = _stockContext.ProductCate.ToList().Select(c => new { ProductCateName = c.ProductCateName.NormalizeAsInternalName(), ProductCate = c }).GroupBy(c => c.ProductCateName).ToDictionary(c => c.Key, c => c.First().ProductCate);
            var barcodeConfigs = _masterDBContext.BarcodeConfig.Where(c => c.IsActived).Select(c => new { c.BarcodeConfigId, c.Name }).ToDictionary(c => c.Name.NormalizeAsInternalName(), c => c.BarcodeConfigId);
            unitInfos = _masterDBContext.Unit.ToList().ToDictionary(u => u.UnitId, u => u);

            units = unitInfos.GroupBy(u => u.Value.UnitName.NormalizeAsInternalName()).ToDictionary(u => u.Key, u => u.First().Key);

            var stocks = _stockContext.Stock.ToDictionary(s => s.StockName, s => s.StockId);

            var customers = await _organizationHelperService.AllCustomers();

            customerByCodes = customers.Select(c => (new { CustomerId = c.CustomerId, Code = c.CustomerCode.NormalizeAsInternalName() }))
                .GroupBy(c => c.Code)
                .ToDictionary(c => c.Key, c => (int?)c.First().CustomerId);

            customerByNames = customers.Select(c => (new { CustomerId = c.CustomerId, Name = c.CustomerName.NormalizeAsInternalName() }))
               .GroupBy(c => c.Name)
               .ToDictionary(c => c.Key, c => (int?)c.First().CustomerId);

            var stockRules = Commons.Enums.StandardEnum.EnumExtensions.GetEnumMembers<EnumStockOutputRule>();
            var timeTypes = Commons.Enums.StandardEnum.EnumExtensions.GetEnumMembers<EnumTimeType>();
            var quantitativeUnitTypes = Commons.Enums.StandardEnum.EnumExtensions.GetEnumMembers<EnumQuantitativeUnitType>();
            var data = reader.ReadSheetEntity<ProductImportModel>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;
                switch (propertyName)
                {
                    case nameof(ProductImportModel.BarcodeConfigId):
                        if (barcodeConfigs.ContainsKey(value)) entity.BarcodeConfigId = barcodeConfigs[value];
                        return true;
                    case nameof(ProductImportModel.StockOutputRuleId):
                        var rule = stockRules.FirstOrDefault(r => r.Description.NormalizeAsInternalName() == value.NormalizeAsInternalName());
                        if (rule != null) entity.StockOutputRuleId = rule.Enum;
                        return true;
                    case nameof(ProductImportModel.ExpireTimeTypeId):
                        var timeType = timeTypes.FirstOrDefault(r => r.Description.NormalizeAsInternalName() == value.NormalizeAsInternalName());
                        if (timeType != null) entity.ExpireTimeTypeId = timeType.Enum;
                        return true;
                    case nameof(ProductImportModel.StockIds):
                        var stockNames = value.Split(",");
                        var stockIds = stockNames.Where(s => stocks.ContainsKey(s)).Select(s => stocks[s]).ToList();
                        if (stockIds.Count != stockNames.Length) throw StockNameNotFound.BadRequestFormat(value);
                        if (stockIds.Count > 0) entity.StockIds = stockIds;
                        return true;
                    case nameof(ProductImportModel.QuantitativeUnitTypeId):
                        var quantitativeUnitTypeId = quantitativeUnitTypes.FirstOrDefault(r => r.Description.NormalizeAsInternalName() == value.NormalizeAsInternalName());
                        if (quantitativeUnitTypeId != null) entity.QuantitativeUnitTypeId = quantitativeUnitTypeId.Enum;
                        return true;

                    case nameof(ProductImportModel.IsProduct):
                        if (value.IsRangeOfAllowValueForBooleanTrueValue())
                        {
                            entity.IsProduct = true;
                        }
                        else entity.IsProduct = false;
                        return true;
                    case nameof(ProductImportModel.IsProductSemi):
                        if (value.IsRangeOfAllowValueForBooleanTrueValue())
                        {
                            entity.IsProductSemi = true;
                        }
                        else entity.IsProductSemi = false;
                        return true;
                    case nameof(ProductImportModel.IsMaterials):
                        if (value.IsRangeOfAllowValueForBooleanTrueValue())
                        {
                            entity.IsMaterials = true;
                        }
                        else entity.IsMaterials = false;
                        return true;

                    //case nameof(ProductImportModel.ProductTypeCode):
                    //case nameof(ProductImportModel.ProductTypeName):
                    //case nameof(ProductImportModel.ProductCate):
                    //case nameof(ProductImportModel.Unit):
                    //case nameof(ProductImportModel.SecondaryUnit01):
                    //case nameof(ProductImportModel.SecondaryUnit02):
                    //case nameof(ProductImportModel.SecondaryUnit03):
                    //case nameof(ProductImportModel.SecondaryUnit04):
                    //case nameof(ProductImportModel.SecondaryUnit05):                    
                    //    var type = entity.GetType();
                    //    var p = type.GetProperty(propertyName);
                    //    if (p != null)
                    //    {
                    //        p.SetValue(entity, value);
                    //    }
                    //    return true;

                    case nameof(ProductImportModel.CustomerCode):
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var code = value.NormalizeAsInternalName();

                            if (!customerByCodes.ContainsKey(code))
                            {
                                throw CustomerWithCodeNotFound.BadRequestFormat(value);
                            }
                            entity.CustomerId = customerByCodes[code].Value;
                        }

                        return false;
                    case nameof(ProductImportModel.CustomerName):
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var code = value.NormalizeAsInternalName();

                            if (!customerByNames.ContainsKey(code))
                            {
                                throw CustomerWithNameNotFound.BadRequestFormat(value);
                            }

                            entity.CustomerId = customerByNames[code].Value;
                        }

                        return false;


                }


                if (propertyName == nameof(ProductImportModel.DecimalPlaceDefault) || propertyName.StartsWith(productUnitDecimalPlacePropPrefix))
                {
                    if (int.TryParse(value, out var v) && v < 0 || v > 12)
                    {
                        throw PuDecimalPlaceNotValid.BadRequestFormat(value, 0, 12);
                    }
                }


                return false;
            });

            var includeProductCates = new List<ProductCate>();
            var includeProductTypes = new List<ProductType>();
            var includeUnits = new List<Unit>();


            defaultTypeId = productTypes.FirstOrDefault(t => t.Value.IsDefault).Value?.ProductTypeId;
            defaultCateId = productCates.FirstOrDefault(t => t.Value.IsDefault).Value?.ProductCateId;

            foreach (var row in data)
            {
                if (!mapping.MappingFields.Any(f => f.FieldName == nameof(ProductImportModel.IsProduct)))
                {
                    row.IsProduct = true;
                }

                if (!string.IsNullOrWhiteSpace(row.Unit) && !units.ContainsKey(row.Unit.NormalizeAsInternalName()) && !includeUnits.Any(u => u.UnitName.NormalizeAsInternalName() == row.Unit.NormalizeAsInternalName()))
                {
                    includeUnits.Add(new Unit
                    {
                        UnitName = row.Unit,
                        UnitStatusId = (int)EnumUnitStatus.Using,
                        DecimalPlace = row.DecimalPlaceDefault.GetValueOrDefault()

                    });
                }
                for (int suffix = 2; suffix <= 5; suffix++)
                {
                    var unitText = $"{productUnitConversionNamePropPrefix}0{suffix}";
                    var decimalPlaceText = $"{productUnitDecimalPlacePropPrefix}0{suffix}";
                    var unit = typeInfo.GetProperty(unitText).GetValue(row) as string;
                    var decimalPlace = (int?)typeInfo.GetProperty(decimalPlaceText).GetValue(row);
                    if (!string.IsNullOrEmpty(unit) && !units.ContainsKey(unit.NormalizeAsInternalName()) && !includeUnits.Any(u => u.UnitName.NormalizeAsInternalName() == unit.NormalizeAsInternalName()))
                    {
                        includeUnits.Add(new Unit
                        {
                            UnitName = unit,
                            UnitStatusId = (int)EnumUnitStatus.Using,
                            DecimalPlace = decimalPlace.GetValueOrDefault()
                        });
                    }
                }

                if (!string.IsNullOrWhiteSpace(row.ProductCate) && !productCates.ContainsKey(row.ProductCate.NormalizeAsInternalName()) && !includeProductCates.Any(c => c.ProductCateName.NormalizeAsInternalName() == row.ProductCate.NormalizeAsInternalName()))
                {
                    includeProductCates.Add(new ProductCate
                    {
                        ProductCateName = row.ProductCate,
                        SortOrder = 9999
                    });
                }

                if (!string.IsNullOrWhiteSpace(row.ProductTypeCode) && !productTypes.ContainsKey(row.ProductTypeCode.NormalizeAsInternalName()) && !includeProductTypes.Any(t => t.IdentityCode.NormalizeAsInternalName() == row.ProductTypeCode.NormalizeAsInternalName()))
                {
                    includeProductTypes.Add(new ProductType
                    {
                        IdentityCode = row.ProductTypeCode,
                        ProductTypeName = string.IsNullOrEmpty(row.ProductTypeName) ? row.ProductTypeCode : row.ProductTypeName
                    });
                }
            }

            _masterDBContext.Unit.AddRange(includeUnits);
            _stockContext.ProductType.AddRange(includeProductTypes);
            _stockContext.ProductCate.AddRange(includeProductCates);

            _masterDBContext.SaveChanges();
            _stockContext.SaveChanges();

            foreach (var unit in includeUnits)
            {
                unitInfos.Add(unit.UnitId, unit);
                units.Add(unit.UnitName.NormalizeAsInternalName(), unit.UnitId);
            }
            foreach (var productCate in includeProductCates)
            {
                productCates.Add(productCate.ProductCateName.NormalizeAsInternalName(), productCate);
            }
            foreach (var productType in includeProductTypes)
            {
                productTypes.Add(productType.IdentityCode.NormalizeAsInternalName(), productType);
            }




            // Validate unique product code
            var productCodes = data.Select(p => p.ProductCode).ToList();

            var existsProduct = await _stockContext.Product.Where(p => productCodes.Contains(p.ProductCode))
                .Include(p => p.ProductExtraInfo)
                .Include(p => p.ProductStockInfo)
                .Include(p => p.ProductCustomer)
                .Include(p => p.ProductUnitConversion)
                .Include(p => p.ProductStockValidation)
                .ToListAsync();

            var existsProductCodes = existsProduct.Select(p => p.ProductCode).Distinct().ToHashSet();

            var dupCodes = productCodes.GroupBy(c => c).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            if (mapping.ImportDuplicateOptionId == null || mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
            {
                if (dupCodes.Count > 0)
                    throw ImportMultipleProductsFound.BadRequestFormat(string.Join(",", dupCodes));
                if (existsProductCodes.Count > 0)
                    throw ProductCodeAlreadyExisted.BadRequestFormat(string.Join(",", existsProductCodes));
            }
            //else
            //{
            //    data = data.Where(x => !existsProductCodes.Contains(x.ProductCode)).GroupBy(x => x.ProductCode).Select(y => y.FirstOrDefault()).ToList();
            //}




            existsProductCodes = existsProductCodes.Select(c => c.ToLower()).Distinct().ToHashSet();

            var newProducts = data.Where(x => !existsProductCodes.Contains(x.ProductCode?.ToLower()))
                .GroupBy(x => x.ProductCode)
                .Select(y => y.ToList().MergeData())
                .ToList();


            newProducts.ForEach(p =>
            {
                var rowNumber = "";
                if (p is MappingDataRowAbstract entity)
                {
                    rowNumber = ",  " + ImportRowTitle + entity.RowNumber;
                }

                if (p.IsMaterials == false && p.IsProduct == false && p.IsProductSemi == false)
                {
                    p.IsProduct = true;
                }

                //if (defaultTypeId == null && string.IsNullOrWhiteSpace(p.ProductTypeCode) && string.IsNullOrWhiteSpace(p.ProductTypeName))
                //{
                //    throw new BadRequestException(ProductErrorCode.ProductTypeInvalid, $"Cần chọn loại mã mặt hàng cho mặt hàng {p.ProductCode} {rowNumber}");
                //}

                var productCodeRowTitle = p.ProductCode + " " + rowNumber;
                if (defaultCateId == null && string.IsNullOrWhiteSpace(p.ProductCate))
                {
                    throw ImportNeedProductCateForProduct.BadRequestFormat(productCodeRowTitle);
                }

                if (string.IsNullOrWhiteSpace(p.Unit))
                {
                    throw UnitOfProductNotFound.BadRequestFormat(productCodeRowTitle);
                }

                if (string.IsNullOrWhiteSpace(p.ProductName))
                {
                    throw ProductNameOfCodeEmpty.BadRequestFormat(productCodeRowTitle);
                }
            });
            var updateProducts = data.Where(x => existsProductCodes.Contains(x.ProductCode?.ToLower()))
                .GroupBy(x => x.ProductCode)
                .Select(y => y.ToList().MergeData())
                .ToList();

            using var trans = await _stockContext.Database.BeginTransactionAsync();
            try
            {
                using (var logBatch = _productActivityLog.BeginBatchLog())
                {
                    var productsMap = new Dictionary<ProductImportModel, Product>();
                    foreach (var row in newProducts)
                    {
                        var newProduct = new Product();
                        await ParseProductInfoEntity(newProduct, row);

                        _stockContext.Product.Add(newProduct);
                        productsMap.Add(row, newProduct);
                    }

                    if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                    {
                        if (updateProducts.Count > 0)
                        {
                            await UpdateProduct(productsMap, updateProducts, existsProduct);
                        }
                    }

                    _stockContext.SaveChanges();

                    foreach (var row in newProducts)
                    {

                        await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.ImportNew)
                              .MessageResourceFormatDatas(row.ProductCode)
                              .ObjectId(productsMap[row].ProductId)
                              .JsonData(row.JsonSerialize())
                              .CreateLog();

                    }

                    if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                    {
                        foreach (var row in updateProducts)
                        {

                            await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.ImportUpdate)
                                  .MessageResourceFormatDatas(row.ProductCode)
                                  .ObjectId(productsMap[row].ProductId)
                                  .JsonData(row.JsonSerialize())
                                  .CreateLog();

                        }
                    }

                    await trans.CommitAsync();
                    await logBatch.CommitAsync();
                    return data.Count > 0;
                }
            }
            catch (Exception)
            {
                trans.TryRollbackTransaction();
                throw;
            }

        }

        private async Task UpdateProduct(Dictionary<ProductImportModel, Product> productsMap, IList<ProductImportModel> updateProducts, IList<Product> existsProduct)
        {

            var existsProductInLowerCase = existsProduct.GroupBy(g => g.ProductCode.ToLower())
                                            .ToDictionary(g => g.Key, g => g.ToList());


            var existedProductIds = existsProduct.Select(p => p.ProductId).ToList();


            foreach (var row in updateProducts)
            {
                var productCodeKey = row.ProductCode.ToLower();
                if (!existsProductInLowerCase.ContainsKey(productCodeKey))
                {
                    throw new BadRequestException(GeneralCode.InternalError, "Existed product not found!");
                }
                var existedProduct = existsProductInLowerCase[productCodeKey].First();

                await ParseProductInfoEntity(existedProduct, row);

                productsMap.Add(row, existedProduct);

            }

        }

        private async Task ParseProductInfoEntity(Product product, ProductImportModel row)
        {

            var typeCode = row.ProductTypeCode.NormalizeAsInternalName();
            var cateName = row.ProductCate.NormalizeAsInternalName();

            //product.UpdateIfAvaiable(p => p.CustomerId, customerByCodes, row.CustomerCode.NormalizeAsInternalName());
            //product.UpdateIfAvaiable(p => p.CustomerId, customerByNames, row.CustomerName.NormalizeAsInternalName());

            var oldUnitId = product.UnitId;

            product.UpdateIfAvaiable(p => p.UnitId, units, row.Unit.NormalizeAsInternalName());

            /*
            if (product.ProductId > 0 && product.UnitId != oldUnitId)
            {
                var isInUsed = new SqlParameter("@IsUsed", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var checkParams = new[]
                {
                            new SqlParameter("@ProductId",product.ProductId),
                            isInUsed
                        };

                await _stockContext.ExecuteStoreProcedure("asp_Product_CheckUsed", checkParams);

                if (isInUsed.Value as bool? == true)
                {
                    throw CanNotUpdateUnitProductWhichInUsed.BadRequestFormat(product.ProductCode);
                }
            }
            */

            product.UpdateIfAvaiable(p => p.ProductCode, row.ProductCode);

            if (!row.ProductName.IsNullObject())
            {
                product.ProductName = row.ProductName;
                product.ProductInternalName = row.ProductName.NormalizeAsInternalName();
            }

            //product.IsCanBuy = row.IsCanBuy ?? true;
            //product.IsCanSell = row.IsCanSell ?? true;
            //product.MainImageFileId = null;
            if (product.ProductId == 0)
            {
                product.ProductTypeId = defaultTypeId;
                product.ProductCateId = (defaultCateId ?? 0);
                product.Coefficient = 1;
            }

            product.UpdateIfAvaiable(p => p.ProductTypeId, productTypes, typeCode, t => t.ProductTypeId);

            product.UpdateIfAvaiable(p => p.ProductCateId, productCates, cateName, t => t.ProductCateId);

            product.UpdateIfAvaiable(p => p.BarcodeConfigId, row.BarcodeConfigId);


            //product.BarcodeStandardId = null;
            product.UpdateIfAvaiable(p => p.Barcode, row.Barcode);



            product.UpdateIfAvaiable(p => p.EstimatePrice, row.EstimatePrice);

            product.UpdateIfAvaiable(p => p.GrossWeight, row.GrossWeight);

            product.UpdateIfAvaiable(p => p.Height, row.Height);

            product.UpdateIfAvaiable(p => p.Long, row.Long);

            product.UpdateIfAvaiable(p => p.Width, row.Width);

            product.UpdateIfAvaiable(p => p.LoadAbility, row.LoadAbility);

            product.UpdateIfAvaiable(p => p.NetWeight, row.NetWeight);

            product.UpdateIfAvaiable(p => p.PackingMethod, row.PackingMethod);

            product.UpdateIfAvaiable(p => p.Measurement, row.Measurement);

            product.UpdateIfAvaiable(p => p.Quantitative, row.Quantitative);

            product.UpdateIfAvaiable(p => p.QuantitativeUnitTypeId, (int?)row.QuantitativeUnitTypeId);

            product.UpdateIfAvaiable(p => p.IsProductSemi, row.IsProductSemi);

            product.UpdateIfAvaiable(p => p.IsProduct, row.IsProduct);

            if (product.ProductId == 0)
            {
                product.Coefficient = 1;
            }
            product.UpdateIfAvaiable(p => p.Coefficient, row.Coefficient);

            if (product.ProductId == 0)
            {
                product.ProductExtraInfo = new ProductExtraInfo()
                {
                    ProductId = product.ProductId,
                    Specification = row.Specification,
                    Description = row.Description
                };
            }
            product.UpdateIfAvaiable(p => p.ProductExtraInfo.Specification, row.Specification);
            product.UpdateIfAvaiable(p => p.ProductExtraInfo.Description, row.Description);

            if (product.ProductId == 0)
            {
                product.ProductStockInfo = new ProductStockInfo()
                {
                    ProductId = product.ProductId,
                    StockOutputRuleId = (int?)row.StockOutputRuleId,
                    AmountWarningMin = row.AmountWarningMin,
                    AmountWarningMax = row.AmountWarningMax,
                    TimeWarningTimeTypeId = null,
                    TimeWarningAmount = null,
                    ExpireTimeTypeId = (int?)row.ExpireTimeTypeId,
                    ExpireTimeAmount = row.ExpireTimeAmount
                };
            }
            product.UpdateIfAvaiable(p => p.ProductStockInfo.StockOutputRuleId, (int?)row.StockOutputRuleId);
            product.UpdateIfAvaiable(p => p.ProductStockInfo.AmountWarningMin, row.AmountWarningMin);
            product.UpdateIfAvaiable(p => p.ProductStockInfo.AmountWarningMax, row.AmountWarningMax);
            product.UpdateIfAvaiable(p => p.ProductStockInfo.ExpireTimeTypeId, (int?)row.ExpireTimeTypeId);
            product.UpdateIfAvaiable(p => p.ProductStockInfo.ExpireTimeAmount, row.ExpireTimeAmount);

            var stockValidations = ParseProductStockValidations(row, product.ProductId);
            foreach (var newStock in stockValidations)
            {
                var existedItem = product.ProductStockValidation.FirstOrDefault(s => s.StockId == newStock.StockId);
                if (existedItem == null)
                {
                    product.ProductStockValidation.Add(newStock);
                }
                else
                {
                    //existedItem.UpdateIfAvaiable(v=>v.StockId)
                }
            }
            var productCustomers = ParseProductCustomers(row, product.ProductId);
            foreach (var productCustomer in productCustomers)
            {
                var existedItem = product.ProductCustomer.FirstOrDefault(s => s.CustomerId == productCustomer.CustomerId);
                if (existedItem == null)
                {
                    product.ProductCustomer.Add(productCustomer);
                }
                else
                {
                    existedItem.UpdateIfAvaiable(v => v.CustomerProductCode, productCustomer.CustomerProductCode);
                    existedItem.UpdateIfAvaiable(v => v.CustomerProductName, productCustomer.CustomerProductName);
                }
            }

            var newProductUnitConversions = ParsePuConverions(row, product.ProductId);

            var existedPus = product.ProductUnitConversion.GroupBy(p => p.ProductUnitConversionName.NormalizeAsInternalName())
                .ToDictionary(p => p.Key, p => p.First());

            foreach (var pu in newProductUnitConversions)
            {
                if (pu.IsDefault)
                {
                    var existedItem = product.ProductUnitConversion.FirstOrDefault(s => s.IsDefault);
                    if (existedItem == null)
                    {
                        product.ProductUnitConversion.Add(pu);
                    }
                    else
                    {
                        existedItem.UpdateIfAvaiable(v => v.ProductUnitConversionName, pu.ProductUnitConversionName);
                        existedItem.UpdateIfAvaiable(v => v.DecimalPlace, pu.UploadDecimalPlace);
                    }

                }
                else
                {
                    var puKey = pu.ProductUnitConversionName.NormalizeAsInternalName();
                    if (existedPus.TryGetValue(puKey, out var existedItem))
                    {
                        existedItem.UpdateIfAvaiable(v => v.DecimalPlace, pu.UploadDecimalPlace);
                        existedItem.UpdateIfAvaiable(v => v.FactorExpression, pu.FactorExpression);
                        existedItem.UpdateIfAvaiable(v => v.ConversionDescription, pu.ConversionDescription);
                    }
                    else
                    {
                        product.ProductUnitConversion.Add(pu);
                    }

                }
            }
        }



        private IList<ProductStockValidation> ParseProductStockValidations(ProductImportModel row, int productId)
        {
            return row.StockIds.Select(s => new ProductStockValidation
            {
                ProductId = productId,
                StockId = s
            }).ToList();

        }

        private IList<ProductCustomer> ParseProductCustomers(ProductImportModel row, int productId)
        {
            return string.IsNullOrWhiteSpace(row.CustomerProductCode) ? new List<ProductCustomer>() :
                new List<ProductCustomer>()
                {
                           new ProductCustomer()
                           {
                               ProductId = productId,
                               CustomerId = row.CustomerId,
                               CustomerProductCode = row.CustomerProductCode,
                               CustomerProductName = row.CustomerProductName
                           }
                };

        }

        private IList<ProductUnitConversionUpdate> ParsePuConverions(ProductImportModel row, int productId)
        {
            var defaultDecPlace = row.DecimalPlaceDefault >= 0 ? row.DecimalPlaceDefault : null;

            var lstUnitConverions = new List<ProductUnitConversionUpdate>(){
                            new ProductUnitConversionUpdate()
                            {
                                ProductId = productId,
                                ProductUnitConversionName = row.Unit,
                                SecondaryUnitId =row.Unit.IsNullObject()?0: units[row.Unit.NormalizeAsInternalName()],
                                FactorExpression = "1",
                                ConversionDescription = "Default",
                                IsDefault = true,
                                IsFreeStyle = false,
                                DecimalPlace = defaultDecPlace??DECIMAL_PLACE_DEFAULT,
                                UploadDecimalPlace= defaultDecPlace
                            }
                        };

            for (int suffix = 2; suffix <= 5; suffix++)
            {
                var unitNamePropName = $"{productUnitConversionNamePropPrefix}0{suffix}";
                var unitExpPropName = $"{productUnitExpressionPropPrefix}0{suffix}";
                var unitDecimalPropName = $"{productUnitDecimalPlacePropPrefix}0{suffix}";

                var unit = typeInfo.GetProperty(unitNamePropName).GetValue(row) as string;
                var exp = typeInfo.GetProperty(unitExpPropName).GetValue(row) as string;
                var decimalPlace = (int?)typeInfo.GetProperty(unitDecimalPropName).GetValue(row);
                if (!string.IsNullOrEmpty(unit))
                {
                    try
                    {
                        var eval = Utils.EvalPrimaryQuantityFromProductUnitConversionQuantity(1, exp);
                        if (!(eval > 0))
                        {
                            throw PuConversionExpressionInvalid.BadRequestFormat(exp, row.ProductCode);
                        }
                    }
                    catch (Exception)
                    {
                        throw PuConversionExpressionError.BadRequestFormat(exp, row.ProductCode);
                    }

                    lstUnitConverions.Add(new ProductUnitConversionUpdate()
                    {
                        ProductId = productId,
                        ProductUnitConversionName = unit,
                        SecondaryUnitId = units[unit.NormalizeAsInternalName()],
                        FactorExpression = exp,
                        IsDefault = false,
                        IsFreeStyle = false,
                        DecimalPlace = decimalPlace >= 0 ? decimalPlace.Value : DECIMAL_PLACE_DEFAULT,
                        UploadDecimalPlace = decimalPlace
                    });
                }
            }
            return lstUnitConverions;

        }

    }
    public class ProductUnitConversionUpdate : ProductUnitConversion
    {
        public int? UploadDecimalPlace { get; set; }
    }
}
