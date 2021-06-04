using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
using VErp.Services.Stock.Model.Product;
using VErp.Commons.Library;


namespace VErp.Services.Stock.Service.Products.Implement.ProductFacade
{
    public class ProductImportFacade
    {
        const int DECIMAL_PLACE_DEFAULT = 11;

        private StockDBContext _stockContext;
        private MasterDBContext _masterDBContext;
        private IOrganizationHelperService _organizationHelperService;
        public ProductImportFacade(StockDBContext stockContext, MasterDBContext masterDBContext, IOrganizationHelperService organizationHelperService)
        {
            _stockContext = stockContext;
            _masterDBContext = masterDBContext;
            _organizationHelperService = organizationHelperService;
        }


        IDictionary<string, int?> customerByCodes = null;
        IDictionary<string, int?> customerByNames = null;

        IDictionary<string, ProductType> productTypes = null;
        IDictionary<string, ProductCate> productCates = null;
        int? defaultTypeId = null;
        int? defaultCateId = null;

        IDictionary<string, int> units = null;
        IDictionary<int, Unit> unitInfos = null;

        string unit01Text = nameof(ProductImportModel.SecondaryUnit01);
        string exp01Text = nameof(ProductImportModel.FactorExpression01);
        string decimalPlace01Text = nameof(ProductImportModel.DecimalPlace01);

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

            var data = reader.ReadSheetEntity<ProductImportModel>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;
                switch (propertyName)
                {
                    case nameof(ProductImportModel.BarcodeConfigId):
                        if (barcodeConfigs.ContainsKey(value)) entity.BarcodeConfigId = barcodeConfigs[value];
                        return true;
                    case nameof(ProductImportModel.StockOutputRuleId):
                        var rule = EnumExtensions.GetEnumMembers<EnumStockOutputRule>().FirstOrDefault(r => r.Description == value);
                        if (rule != null) entity.StockOutputRuleId = rule.Enum;
                        return true;
                    case nameof(ProductImportModel.ExpireTimeTypeId):
                        var timeType = EnumExtensions.GetEnumMembers<EnumTimeType>().FirstOrDefault(r => r.Description == value);
                        if (timeType != null) entity.ExpireTimeTypeId = timeType.Enum;
                        return true;
                    case nameof(ProductImportModel.StockIds):
                        var stockNames = value.Split(",");
                        var stockIds = stockNames.Where(s => stocks.ContainsKey(s)).Select(s => stocks[s]).ToList();
                        if (stockIds.Count != stockNames.Length) throw new BadRequestException(GeneralCode.InvalidParams, $"Danh sách kho {value} không đúng");
                        if (stockIds.Count > 0) entity.StockIds = stockIds;
                        return true;
                    case nameof(ProductImportModel.QuantitativeUnitTypeId):
                        var quantitativeUnitTypeId = EnumExtensions.GetEnumMembers<EnumQuantitativeUnitType>().FirstOrDefault(r => r.Description.NormalizeAsInternalName() == value.NormalizeAsInternalName());
                        if (quantitativeUnitTypeId != null) entity.QuantitativeUnitTypeId = quantitativeUnitTypeId.Enum;
                        return true;

                    case nameof(ProductImportModel.IsProduct):
                        if (value.NormalizeAsInternalName().Equals("Có".NormalizeAsInternalName()))
                        {
                            entity.IsProduct = true;
                        }
                        return true;
                    case nameof(ProductImportModel.IsProductSemi):
                        if (value.NormalizeAsInternalName().Equals("Có".NormalizeAsInternalName()))
                        {
                            entity.IsProductSemi = true;
                        }
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
                                throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy khách hàng có mã {value} trong hệ thống!");
                            }
                        }

                        return false;
                    case nameof(ProductImportModel.CustomerName):
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var code = value.NormalizeAsInternalName();

                            if (!customerByNames.ContainsKey(code))
                            {
                                throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy khách hàng có tên {value} trong hệ thống!");
                            }
                        }

                        return false;


                }

                var decimalPlacePropPrefix = nameof(ProductImportModel.DecimalPlace01)[..^2];

                if (propertyName == nameof(ProductImportModel.DecimalPlaceDefault) || propertyName.StartsWith(decimalPlacePropPrefix))
                {
                    if (int.TryParse(value, out var v) && v < 0 || v > 12)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Độ chính xác {value} không đúng, độ chính xác phải >=0 và <=12");
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
                if (!units.ContainsKey(row.Unit.NormalizeAsInternalName()) && !includeUnits.Any(u => u.UnitName.NormalizeAsInternalName() == row.Unit.NormalizeAsInternalName()))
                {
                    includeUnits.Add(new Unit
                    {
                        UnitName = row.Unit,
                        UnitStatusId = (int)EnumUnitStatus.Using
                    });
                }
                for (int suffix = 1; suffix <= 5; suffix++)
                {
                    var unitText = suffix > 1 ? new StringBuilder(unit01Text).Remove(unit01Text.Length - 2, 2).Append($"0{suffix}").ToString() : unit01Text;
                    var unit = typeInfo.GetProperty(unitText).GetValue(row) as string;
                    if (!string.IsNullOrEmpty(unit) && !units.ContainsKey(unit.NormalizeAsInternalName()) && !includeUnits.Any(u => u.UnitName.NormalizeAsInternalName() == unit.NormalizeAsInternalName()))
                    {
                        includeUnits.Add(new Unit
                        {
                            UnitName = unit,
                            UnitStatusId = (int)EnumUnitStatus.Using
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
                .ToListAsync();

            var existsProductCodes = existsProduct.Select(p => p.ProductCode).Distinct().ToHashSet();

            var dupCodes = productCodes.GroupBy(c => c).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            if (mapping.ImportDuplicateOptionId == null || mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
            {
                if (dupCodes.Count > 0)
                    throw new BadRequestException(ProductErrorCode.ProductCodeAlreadyExisted, $"Tồn tại nhiều mặt hàng {string.Join(",", dupCodes)} trong file import");
                if (existsProductCodes.Count > 0)
                    throw new BadRequestException(ProductErrorCode.ProductCodeAlreadyExisted, $"Mã mặt hàng {string.Join(",", existsProductCodes)} đã tồn tại trong hệ thống");
            }
            //else
            //{
            //    data = data.Where(x => !existsProductCodes.Contains(x.ProductCode)).GroupBy(x => x.ProductCode).Select(y => y.FirstOrDefault()).ToList();
            //}

            // Validate required product name
            var emptyNameProducts = data.Where(p => string.IsNullOrEmpty(p.ProductName)).Select(p => p.ProductCode).ToList();
            if (emptyNameProducts.Count > 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, $"Vui lòng nhập tên mặt hàng có mã: {string.Join(",", emptyNameProducts)}");
            }


            existsProductCodes = existsProductCodes.Select(c => c.ToLower()).Distinct().ToHashSet();

            var newProducts = data.Where(x => !existsProductCodes.Contains(x.ProductCode?.ToLower()))
                .GroupBy(x => x.ProductCode)
                .Select(y => y.ToList().MergeData())
                .ToList();

            newProducts.ForEach(p =>
            {
                if (defaultTypeId == null && string.IsNullOrWhiteSpace(p.ProductTypeCode) && string.IsNullOrWhiteSpace(p.ProductTypeName))
                {
                    throw new BadRequestException(ProductErrorCode.ProductTypeInvalid, $"Cần chọn loại mã mặt hàng cho mặt hàng {p.ProductCode} ");
                }

                if (defaultCateId == null && string.IsNullOrWhiteSpace(p.ProductCate))
                {
                    throw new BadRequestException(ProductErrorCode.ProductTypeInvalid, $"Cần chọn loại mã mặt hàng cho mặt hàng {p.ProductCode} ");
                }
            });
            var updateProducts = data.Where(x => existsProductCodes.Contains(x.ProductCode?.ToLower()))
                .GroupBy(x => x.ProductCode)
                .Select(y => y.ToList().MergeData())
                .ToList();

            using var trans = await _stockContext.Database.BeginTransactionAsync();
            try
            {

                foreach (var row in newProducts)
                {
                    var newProduct = new Product();
                    ParseProductInfoEntity(newProduct, row);

                    _stockContext.Product.Add(newProduct);
                }

                if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                {
                    if (updateProducts.Count > 0)
                    {
                        UpdateProduct(updateProducts, existsProduct);
                    }
                }

                _stockContext.SaveChanges();
                trans.Commit();
                return data.Count > 0;
            }
            catch (Exception)
            {
                trans.TryRollbackTransaction();
                throw;
            }

        }

        private void UpdateProduct(IList<ProductImportModel> updateProducts, IList<Product> existsProduct)
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

                ParseProductInfoEntity(existedProduct, row);
            }

        }

        private void ParseProductInfoEntity(Product product, ProductImportModel row)
        {

            var typeCode = row.ProductTypeCode.NormalizeAsInternalName();
            var cateName = row.ProductCate.NormalizeAsInternalName();

            product.UpdateIfAvaiable(p => p.CustomerId, customerByCodes, row.CustomerCode.NormalizeAsInternalName());
            product.UpdateIfAvaiable(p => p.CustomerId, customerByNames, row.CustomerName.NormalizeAsInternalName());


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

            product.UpdateIfAvaiable(p => p.UnitId, units, row.Unit.NormalizeAsInternalName());

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
                    Specification = row.Specification
                };
            }
            product.UpdateIfAvaiable(p => p.ProductExtraInfo.Specification, row.Specification);

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
            var productCustomers = ParseProductCustomers(row, product.ProductId, product.CustomerId);
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
                }
            }

            var newProductUnitConversions = ParsePuConverions(row, product.ProductId);

            var existedPus = product.ProductUnitConversion.GroupBy(p => p.ProductUnitConversionName.ToLower())
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
                        existedItem.UpdateIfAvaiable(v => v.DecimalPlace, pu.UploadDecimalPlace);
                    }

                }
                else
                {
                    var puKey = pu.ProductUnitConversionName.ToLower();
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

        private IList<ProductCustomer> ParseProductCustomers(ProductImportModel row, int productId, int? customerId)
        {
            return string.IsNullOrWhiteSpace(row.CustomerProductCode) ? new List<ProductCustomer>() :
                new List<ProductCustomer>()
                {
                           new ProductCustomer()
                           {
                               ProductId = productId,
                               CustomerId = customerId,
                               CustomerProductCode = row.CustomerProductCode
                           }
                };

        }

        private IList<ProductUnitConversionUpdate> ParsePuConverions(ProductImportModel row, int productId)
        {
            var lstUnitConverions = new List<ProductUnitConversionUpdate>(){
                            new ProductUnitConversionUpdate()
                            {
                                ProductId = productId,
                                ProductUnitConversionName = row.Unit,
                                SecondaryUnitId = units[row.Unit.NormalizeAsInternalName()],
                                FactorExpression = "1",
                                ConversionDescription = "Mặc định",
                                IsDefault = true,
                                IsFreeStyle = false,
                                DecimalPlace = row.DecimalPlaceDefault >= 0 ? row.DecimalPlaceDefault??DECIMAL_PLACE_DEFAULT : DECIMAL_PLACE_DEFAULT,
                                UploadDecimalPlace=null
                            }
                        };

            for (int suffix = 1; suffix <= 5; suffix++)
            {
                var unitText = suffix > 1 ? new StringBuilder(unit01Text).Remove(unit01Text.Length - 2, 2).Append($"0{suffix}").ToString() : unit01Text;
                var expText = suffix > 1 ? new StringBuilder(exp01Text).Remove(exp01Text.Length - 2, 2).Append($"0{suffix}").ToString() : exp01Text;
                var decimalPlaceText = suffix > 1 ? new StringBuilder(decimalPlace01Text).Remove(decimalPlace01Text.Length - 2, 2).Append($"0{suffix}").ToString() : decimalPlace01Text;
                var unit = typeInfo.GetProperty(unitText).GetValue(row) as string;
                var exp = typeInfo.GetProperty(expText).GetValue(row) as string;
                int? decimalPlace = null;
                var strDecimalPlace = typeInfo.GetProperty(decimalPlaceText).GetValue(row) as string;
                if (!strDecimalPlace.IsNullObject() && int.TryParse(strDecimalPlace, out var d))
                {
                    decimalPlace = d;

                }
                if (!string.IsNullOrEmpty(unit))
                {
                    try
                    {
                        var eval = Utils.EvalPrimaryQuantityFromProductUnitConversionQuantity(1, exp);
                        if (!(eval > 0))
                        {
                            throw new BadRequestException(ProductErrorCode.InvalidUnitConversionExpression, $"Biểu thức chuyển đổi {exp} của mặt hàng {row.ProductCode} không đúng");
                        }
                    }
                    catch (Exception)
                    {

                        throw new BadRequestException(ProductErrorCode.InvalidUnitConversionExpression, $"Lỗi không thể tính toán biểu thức đơn vị chuyển đổi {exp}  của mặt hàng {row.ProductCode}");
                    }

                    lstUnitConverions.Add(new ProductUnitConversionUpdate()
                    {
                        ProductId = productId,
                        ProductUnitConversionName = unitInfos[units[unit.NormalizeAsInternalName()]].UnitName,
                        SecondaryUnitId = units[unit.NormalizeAsInternalName()],
                        FactorExpression = typeInfo.GetProperty(expText).GetValue(row) as string,
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
