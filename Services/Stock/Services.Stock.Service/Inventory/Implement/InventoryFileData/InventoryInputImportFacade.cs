using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Inventory.OpeningBalance;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Service.Products;
using static Verp.Resources.Stock.Inventory.InventoryFileData.InventoryImportFacadeMessage;
using LocationEntity = VErp.Infrastructure.EF.StockDB.Location;

namespace VErp.Services.Stock.Service.Stock.Implement.InventoryFileData
{
    internal class InventoryInputImportFacade
    {
        private Dictionary<string, ProductCate> _productCates = null;
        private Dictionary<string, ProductType> _productTypes = null;
        private Dictionary<string, Unit> _units = null;

        private Dictionary<string, Product> _productsByCode = null;
        private Dictionary<string, List<Product>> _productsByName = null;

        private IList<ImportInvInputModel> _excelRows = null;

        private Dictionary<int, List<ProductUnitConversion>> _productUnitsByProduct = null;

        private StockDBContext _stockDbContext;
        private MasterDBContext _masterDBContext;
        private IProductService _productService;
        private IOrganizationHelperService _organizationHelperService;


        public InventoryInputImportFacade SetStockDBContext(StockDBContext stockDbContext)
        {
            _stockDbContext = stockDbContext;
            return this;
        }

        public InventoryInputImportFacade SetMasterDBContext(MasterDBContext masterDBContext)
        {
            _masterDBContext = masterDBContext;
            return this;
        }

        public InventoryInputImportFacade SetProductService(IProductService productService)
        {
            _productService = productService;
            return this;
        }

        public InventoryInputImportFacade SetOrganizationHelper(IOrganizationHelperService organizationHelperService)
        {
            _organizationHelperService = organizationHelperService;
            return this;
        }


        public async Task ProcessExcelFile(ImportExcelMapping mapping, Stream stream)
        {

            var reader = new ExcelReader(stream);

            var currentCateName = string.Empty;
            var currentCatePrefixCode = string.Empty;

            var customProps = await _stockDbContext.PackageCustomProperty.ToListAsync();

            var locations = (await _stockDbContext.Location.ToListAsync())
                .GroupBy(l => l.Name?.NormalizeAsInternalName())
                .ToDictionary(l => l.Key, l => l.ToList());

            var stocks = await _stockDbContext.Stock.ToListAsync();

            var stockByCodes = stocks
               .GroupBy(l => l.StockCode?.NormalizeAsInternalName())
               .ToDictionary(l => l.Key, l => l.ToList());

            var stockByNames = stocks
               .GroupBy(l => l.StockName?.NormalizeAsInternalName())
               .ToDictionary(l => l.Key, l => l.ToList());

            var customers = await _organizationHelperService.AllCustomers();

            var customerByCodes = customers
                .GroupBy(l => l.CustomerCode?.NormalizeAsInternalName())
                .ToDictionary(l => l.Key, l => l.ToList());

            var customerByNames = customers
               .GroupBy(l => l.CustomerName?.NormalizeAsInternalName())
               .ToDictionary(l => l.Key, l => l.ToList());

            var departments = await _organizationHelperService.GetAllDepartmentSimples();

            var departmentsByCodes = departments
                 .GroupBy(l => l.DepartmentCode?.NormalizeAsInternalName())
                .ToDictionary(l => l.Key, l => l.ToList());

            var departmentByNames = departments
                 .GroupBy(l => l.DepartmentName?.NormalizeAsInternalName())
                .ToDictionary(l => l.Key, l => l.ToList());

            _excelRows = reader.ReadSheetEntity<ImportInvInputModel>(mapping, (entity, propertyName, value, refObj, refProperty) =>
            {
                if (propertyName == nameof(ImportInvInputModel.InventoryActionId))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {

                        if (!int.TryParse(value, out var action) || !ImportInvInputModel.InventoryActionIds.Contains((EnumInventoryAction)action))
                        {
                            var actions = ImportInvOutputModel.InventoryActionIds;
                            var des = $"Loại ({string.Join(",", actions.Select(a => $"{(int)a}: {a.GetEnumDescription()}"))})";

                            throw GeneralCode.InvalidParams.BadRequest("Loại phiếu không hợp lệ " + value + ", chỉ chấp nhận " + des);
                        }

                        entity.InventoryActionId = (EnumInventoryAction)action;
                    }

                    return true;
                }

                if (propertyName == nameof(ImportInvInputModel.StockId))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        var stockNameNormalize = value?.NormalizeAsInternalName();
                        if (!stockByNames.ContainsKey(stockNameNormalize))
                        {
                            throw GeneralCode.ItemNotFound.BadRequest("Không tìm thấy kho hoặc bạn không có quyền thực hiện trên kho " + value);
                        }

                        var stockInfo = stockByNames[stockNameNormalize].OrderByDescending(s => s.StockName == value).First();
                        entity.StockId = stockInfo.StockId;
                    }

                    return true;
                }

                if (propertyName == nameof(ImportInvInputModel.Customer))
                {
                    var customerInfo = (InvCustomerInfo)refObj;

                    if (refProperty == nameof(InvCustomerInfo.CustomerCode))
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var codeNormalize = value?.NormalizeAsInternalName();
                            if (!customerByCodes.ContainsKey(codeNormalize))
                            {
                                throw GeneralCode.ItemNotFound.BadRequest("Không tìm thấy mã khách " + value);
                            }

                            var info = customerByCodes[codeNormalize].OrderByDescending(s => s.CustomerCode == value).First();
                            customerInfo.CustomerId = info.CustomerId;
                        }
                    }

                    if (refProperty == nameof(InvCustomerInfo.CustomerName))
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var nameNormalize = value?.NormalizeAsInternalName();
                            if (!customerByNames.ContainsKey(nameNormalize))
                            {
                                throw GeneralCode.ItemNotFound.BadRequest("Không tìm thấy tên khách " + value);
                            }

                            var info = customerByNames[nameNormalize].OrderByDescending(s => s.CustomerName == value).First();
                            customerInfo.CustomerId = info.CustomerId;
                        }
                    }

                    return true;
                }


                if (propertyName == nameof(ImportInvInputModel.Department))
                {
                    var departmentInfo = (InvDepartmentInfo)refObj;

                    if (refProperty == nameof(InvDepartmentInfo.DepartmentCode))
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var codeNormalize = value?.NormalizeAsInternalName();
                            if (!departmentsByCodes.ContainsKey(codeNormalize))
                            {
                                throw GeneralCode.ItemNotFound.BadRequest("Không tìm thấy mã bộ phận " + value);
                            }

                            var info = departmentsByCodes[codeNormalize].OrderByDescending(s => s.DepartmentCode == value).First();
                            departmentInfo.DepartmentId = info.DepartmentId;
                        }
                    }

                    if (refProperty == nameof(InvDepartmentInfo.DepartmentName))
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var nameNormalize = value?.NormalizeAsInternalName();
                            if (!departmentByNames.ContainsKey(nameNormalize))
                            {
                                throw GeneralCode.ItemNotFound.BadRequest("Không tìm thấy tên bộ phận " + value);
                            }

                            var info = departmentByNames[nameNormalize].OrderByDescending(s => s.DepartmentName == value).First();
                            departmentInfo.DepartmentId = info.DepartmentId;
                        }
                    }

                    return true;
                }


                if (propertyName == nameof(ImportInvInputModel.CateName))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        currentCateName = value;
                    }

                    entity.CateName = currentCateName;

                    return true;
                }

                if (propertyName == nameof(ImportInvInputModel.CatePrefixCode))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        currentCatePrefixCode = value;
                    }

                    entity.CatePrefixCode = currentCatePrefixCode;

                    return true;
                }

                if (propertyName == nameof(ImportInvInputModel.ToPackgeInfo))
                {
                    var packageInfo = (PackageInputModel)refObj;

                    if (refProperty?.StartsWith(nameof(PackageInputModel.CustomPropertyValue)) == true)
                    {
                        var customPropertyId = Convert.ToInt32(refProperty.Substring(nameof(PackageInputModel.CustomPropertyValue).Length));

                        var propertyInfo = customProps.FirstOrDefault(p => p.PackageCustomPropertyId == customPropertyId);

                        if (propertyInfo == null) throw GeneralCode.ItemNotFound.BadRequest("Property " + customPropertyId + " was not found!");

                        if (packageInfo.CustomPropertyValue == null)
                        {
                            packageInfo.CustomPropertyValue = new Dictionary<int, object>();
                        }

                        if (!packageInfo.CustomPropertyValue.ContainsKey(customPropertyId))
                        {
                            var customValue = value.ConvertValueByType((EnumDataType)propertyInfo.DataTypeId);

                            packageInfo.CustomPropertyValue.Add(customPropertyId, customValue);
                        }

                        return true;
                    }

                    if (refProperty?.Equals(nameof(PackageInputModel.LocationId)) == true)
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            LocationEntity locationInfo = null;

                            var valueKey = value.NormalizeAsInternalName();
                            if (locations.ContainsKey(valueKey))
                            {

                                if (locations[valueKey].Count > 1)
                                {
                                    locationInfo = locations[valueKey].FirstOrDefault(l => l.Name.ToLower() == value.ToLower());
                                }
                                else
                                {
                                    locationInfo = locations[valueKey].FirstOrDefault();
                                }
                            }

                            if (locationInfo == null)
                            {
                                locationInfo = new LocationEntity()
                                {
                                    StockId = entity.StockId,
                                    Name = value,
                                    Description = "",
                                    Status = 1
                                };
                                _stockDbContext.Location.Add(locationInfo);
                                _stockDbContext.SaveChanges();
                            }

                            packageInfo.LocationId = locationInfo.LocationId;

                        }

                        return true;

                    }
                }


                return false;
            });

            await AddMissingProductCates();
            await AddMissingProductTypes();
            await AddMissingUnit();
            await AddMissingProducts();
        }

        public async Task<IList<InventoryInModel>> GetInputInventoryModel()
        {
            var lst = new List<InventoryInModel>();
            var groups = _excelRows.GroupBy(g => g.InventoryCode);
            foreach (var g in groups)
            {
                var inventoryInputList = new List<InventoryInModel>();

                var totalRowCount = g.Count();

                var newInventoryInputModel = new List<InventoryInProductModel>(totalRowCount);

                var sortOrder = 1;
                foreach (var item in g)
                {
                    if (string.IsNullOrWhiteSpace(item.ProductCode)) continue;

                    if (item.Qty1 <= 0)
                        throw ProductQuantityInvalid.BadRequestFormat($"{item.ProductCode} {item.ProductName}");

                    var productObj = GetProduct(item);

                    ProductUnitConversion productUnitConversionObj = null;

                    if (!string.IsNullOrWhiteSpace(item.Unit2))
                    {
                        productUnitConversionObj = _productUnitsByProduct[productObj.ProductId].FirstOrDefault(u => u.ProductUnitConversionName.NormalizeAsInternalName().Equals(item.Unit2.NormalizeAsInternalName()));
                        if (productUnitConversionObj == null)
                        {
                            //if (_model.CreateNewPuIfNotExists)
                            //{
                            //    var factor = item.Factor.ToString();

                            //    var eval = EvalUtils.EvalPrimaryQuantityFromProductUnitConversionQuantity(1, factor);
                            //    if (!(eval > 0))
                            //    {
                            //        throw ProductErrorCode.InvalidUnitConversionExpression.BadRequest();
                            //    }

                            //    var puInfo = await CreateNewPu(productObj.ProductId, item.Unit2, factor);
                            //    _productUnitsByProduct[productObj.ProductId].Add(puInfo);
                            //}
                            //else
                            //{

                            throw ProductUniConversionNameNotFound.BadRequestFormat(item.Unit2, item.ProductCode);
                            //}
                        }
                    }
                    else
                    {
                        productUnitConversionObj = _productUnitsByProduct[productObj.ProductId].FirstOrDefault(u => u.IsDefault);
                        if (productUnitConversionObj == null)
                        {
                            throw PuConversionDefaultError.BadRequestFormat(item.ProductCode);
                        }
                    }

                    var option = EnumPackageOption.NoPackageManager;

                    Package packageInfo = null;
                    if (!string.IsNullOrWhiteSpace(item.ToPackgeInfo?.PackageCode))
                    {
                        packageInfo = await _stockDbContext.Package.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productObj.ProductId && p.PackageCode == item.ToPackgeInfo.PackageCode && p.ProductUnitConversionId == productUnitConversionObj.ProductUnitConversionId);


                        if (packageInfo != null)
                        {
                            option = EnumPackageOption.Append;
                        }
                        else
                        {
                            if (g.Where(e => e.ToPackgeInfo?.PackageCode?.ToLower() == item.ToPackgeInfo?.PackageCode?.ToLower()).Count() > 1)
                            {
                                option = EnumPackageOption.CreateMerge;
                            }
                            else
                            {
                                option = EnumPackageOption.Create;
                            }
                        }
                    }



                    newInventoryInputModel.Add(new InventoryInProductModel
                    {
                        SortOrder = sortOrder++,
                        ProductId = productObj != null ? productObj.ProductId : 0,
                        ProductUnitConversionId = productUnitConversionObj.ProductUnitConversionId,
                        PrimaryQuantity = item.Qty1,
                        ProductUnitConversionQuantity = item.Qty2,
                        UnitPrice = item.UnitPrice,
                        RefObjectTypeId = null,
                        RefObjectId = null,
                        RefObjectCode = item.CatePrefixCode,
                        ToPackageId = packageInfo?.PackageId,
                        ToPackageInfo = item.ToPackgeInfo,
                        PackageOptionId = option,

                        //AccountancyAccountNumberDu = item.AccountancyAccountNumberDu
                    });
                }




                //foreach (var d in newInventoryInputModel)
                //{
                //    d.PackageOptionId = EnumPackageOption.NoPackageManager;
                //    d.ToPackageId = null;
                //d.RefObjectCode = string.Format("PN_TonDau_{0}_{1}", DateTime.UtcNow.ToString("ddMMyyyyHHmmss"), d.RefObjectCode);
                //}

                var firstRow = g.First();
                var newInventory = new InventoryInModel
                {
                    StockId = firstRow.StockId,
                    InventoryActionId = (EnumInventoryAction?)firstRow.InventoryActionId ?? EnumInventoryAction.Normal,
                    InventoryCode = g.Key,
                    //InventoryCode = string.Format("PN_TonDau_{0}", DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
                    Date = firstRow.Date,

                    Shipper = firstRow.Shipper,
                    Content = firstRow.Description,
                    CustomerId = firstRow.Customer?.CustomerId,
                    Department = string.Empty,
                    DepartmentId = firstRow.Department?.DepartmentId,
                    StockKeeperUserId = null,
                    BillCode = firstRow.BillCode,
                    BillSerial = firstRow.BillSerial,
                    BillDate = firstRow.BillDate,
                    FileIdList = null,
                    InProducts = newInventoryInputModel,
                    //AccountancyAccountNumber = _model.AccountancyAccountNumber
                };

                lst.Add(newInventory);
            }

            return lst;
        }

        /*
        private async Task<ProductUnitConversion> CreateNewPu(int productId, string name, string expression)
        {
            var puInfo = new ProductUnitConversion()
            {
                ConversionDescription = "Auto created from import",
                DecimalPlace = DECIMAL_PLACE_DEFAULT,
                FactorExpression = expression,
                IsDefault = false,
                IsFreeStyle = false,
                ProductId = productId,
                ProductUnitConversionName = name
            };

            await _stockDbContext.ProductUnitConversion.AddAsync(puInfo);
            await _stockDbContext.SaveChangesAsync();
            return puInfo;
        }
        */
        private Product GetProduct(ImportInvInputModel item)
        {
            var pCodeKey = item.ProductCode.NormalizeAsInternalName();
            Product productObj = null;
            if (_productsByCode.ContainsKey(pCodeKey))
                productObj = _productsByCode[pCodeKey];

            if (productObj == null)
            {
                var productbyNames = _productsByName[item.ProductName.NormalizeAsInternalName()];

                if (productbyNames.Count > 1)
                {
                    productbyNames = productbyNames.Where(p => p.ProductName == item.ProductName).ToList();
                    if (productbyNames.Count != 1)
                    {
                        throw ProductFoundMoreThanOne.BadRequestFormat(productbyNames.Count, $"{item.ProductCode} {item.ProductName}");
                    }
                    else
                    {
                        productObj = productbyNames.First();
                    }
                }
            }

            if (productObj == null)
                throw ProductNotFound.BadRequestFormat($"{item.ProductCode} {item.ProductName}");

            return productObj;
        }

        private async Task AddMissingProductCates()
        {
            var productCates = await _stockDbContext.ProductCate.AsNoTracking().ToListAsync();

            var existedProductCateNormalizeNames = productCates.Select(c => c.ProductCateName.NormalizeAsInternalName()).Distinct().ToHashSet();

            var importProductCates = _excelRows.Select(p => p.CateName).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();

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

            var importProductTypes = _excelRows.Select(p => p.CatePrefixCode).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();

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


            var importedProductsByCode = _excelRows.Where(m => !string.IsNullOrWhiteSpace(m.ProductCode)).GroupBy(p => p.ProductCode).ToDictionary(p => p.Key, p => p.ToList());

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

                foreach (var pu in newPus)
                {
                    var eval = EvalUtils.EvalPrimaryQuantityFromProductUnitConversionQuantity(1, pu.FactorExpression);
                    if (!(eval > 0))
                    {
                        throw ProductErrorCode.InvalidUnitConversionExpression.BadRequest();
                    }
                }

                await _stockDbContext.ProductUnitConversion.AddRangeAsync(newPus);
                await _stockDbContext.SaveChangesAsync();
                _productUnitsByProduct[productInfo.ProductId].AddRange(newPus);
            }

            _productsByName = products.GroupBy(c => c.ProductName.NormalizeAsInternalName()).ToDictionary(c => c.Key, c => c.ToList());

        }

        private async Task AddProduct(ImportInvInputModel info, IList<Product> products, HashSet<string> existedProductNormalizeCodes, HashSet<string> existedProductNormalizeNames)
        {
            if (string.IsNullOrWhiteSpace(info.Unit1))
            {
                throw PuConversionDefaultError.BadRequestFormat(info.ProductCode);
            }

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

        private ProductModel CreateProductModel(ImportInvInputModel p)
        {
            _productTypes.TryGetValue(p.CatePrefixCode.NormalizeAsInternalName(), out var productType);

            _productCates.TryGetValue(p.CateName.NormalizeAsInternalName(), out var productCate);
            if (productCate == null) throw ProductCateEmpty.BadRequest();

            return new ProductModel()
            {

                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                IsCanBuy = true,
                IsCanSell = true,
                MainImageFileId = null,
                ProductTypeId = productType?.ProductTypeId,
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

            var importUnits = _excelRows.SelectMany(p => new[] { p.Unit1, p.Unit2 }).Where(u => !string.IsNullOrWhiteSpace(u))
                .GroupBy(u => u.NormalizeAsInternalName())
                .Select(u => u.First())
                .Distinct()
                .ToList();

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
