using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Stock.Inventory.InventoryFileData;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Formaters;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Inventory.OpeningBalance;
using static Verp.Resources.Stock.Inventory.InventoryFileData.InventoryImportFacadeMessage;

namespace VErp.Services.Stock.Service.Stock.Implement.InventoryFileData
{
    internal class InventoryOutImportFacade
    {
        private Dictionary<string, Product> _productsByCode = null;
        private Dictionary<string, List<Product>> _productsByName = null;

        private IList<ImportInvOutputModel> _excelRows = null;

        private Dictionary<int, List<ProductUnitConversion>> _productUnitsByProduct = null;

        private StockDBContext _stockDbContext;
        private IOrganizationHelperService _organizationHelperService;


        public InventoryOutImportFacade SetStockDBContext(StockDBContext stockDbContext)
        {
            _stockDbContext = stockDbContext;
            return this;
        }
        public InventoryOutImportFacade SetOrganizationHelper(IOrganizationHelperService organizationHelperService)
        {
            _organizationHelperService = organizationHelperService;
            return this;
        }



        private ImportExcelMapping _mapping;
        public async Task ProcessExcelFile(ImportExcelMapping mapping, Stream stream)
        {
            _mapping = mapping;

            var reader = new ExcelReader(stream);

            var currentCateName = string.Empty;
            var currentCatePrefixCode = string.Empty;

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


            _excelRows = reader.ReadSheetEntity<ImportInvOutputModel>(mapping, (entity, propertyName, value, refObj, refProperty) =>
            {
                if (propertyName == nameof(ImportInvOutputModel.InventoryActionId))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {

                        if (!int.TryParse(value, out var action) || !ImportInvOutputModel.InventoryActionIds.Contains((EnumInventoryAction)action))
                        {
                            var actions = ImportInvOutputModel.InventoryActionIds;
                            var des = $"Loại ({string.Join(",", actions.Select(a => $"{(int)a}: {a.GetEnumDescription()}"))})";

                            throw GeneralCode.InvalidParams.BadRequest("Loại phiếu không hợp lệ " + value + ", chỉ chấp nhận " + des);
                        }

                        entity.InventoryActionId = (EnumInventoryAction)action;
                    }

                    return true;
                }

                if (propertyName == nameof(ImportInvOutputModel.StockId))
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


                if (propertyName == nameof(ImportInvOutputModel.CateName))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        currentCateName = value;
                    }

                    entity.CateName = currentCateName;

                    return true;
                }

                if (propertyName == nameof(ImportInvOutputModel.CatePrefixCode))
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

            await LoadData();
        }


        private async Task LoadData()
        {
            var products = (await _stockDbContext.Product.AsNoTracking().ToListAsync()).GroupBy(p => p.ProductCode).Select(p => p.First()).ToList();


            var existedProductNormalizeCodes = products.Select(c => c.ProductCode.NormalizeAsInternalName()).Distinct().ToHashSet();

            var existedProductNormalizeNames = products.Select(c => c.ProductName.NormalizeAsInternalName()).Distinct().ToHashSet();



            _productsByCode = products.GroupBy(c => c.ProductCode.NormalizeAsInternalName()).ToDictionary(c => c.Key, c => c.First());

            var productIds = products.Select(p => p.ProductId).ToList();

            _productUnitsByProduct = (await _stockDbContext.ProductUnitConversion.AsNoTracking().Where(u => productIds.Contains(u.ProductId)).ToListAsync())
                .GroupBy(pu => pu.ProductId)
                .ToDictionary(pu => pu.Key, pu => pu.ToList());

            _productsByName = products.GroupBy(c => c.ProductName.NormalizeAsInternalName()).ToDictionary(c => c.Key, c => c.ToList());

        }


        public async Task<IList<InventoryOutModel>> GetOutputInventoryModel()
        {

            var lst = new List<InventoryOutModel>();

            var groups = _excelRows.GroupBy(r => r.InventoryCode);

            var codes = groups.Select(g => g.Key).ToList();

            var existedInventoryByCodes = (await _stockDbContext.Inventory
                .Where(iv => iv.InventoryTypeId == (int)EnumInventoryType.Output && codes.Contains(iv.InventoryCode))
                .Select(iv => iv.InventoryCode)
                .ToListAsync())
                .Select(code => code.ToLower());

            foreach (var g in groups)
            {
                if (existedInventoryByCodes.Contains(g.Key?.ToLower()))
                {

                    switch (_mapping.ImportDuplicateOptionId)
                    {
                        case EnumImportDuplicateOption.Ignore:
                            continue;

                        case EnumImportDuplicateOption.Denied:
                            throw InventoryErrorCode.InventoryCodeAlreadyExisted.BadRequestDescriptionFormat(g.Key);

                        default:
                            throw GeneralCode.NotYetSupported.BadRequest("Update option is not support yet!");
                    }
                }

                var totalRowCount = g.Count();

                var newInventoryOutProductModel = new List<InventoryOutProductModel>(totalRowCount);

                var productUnitConversionIds = new List<int>();
                var productIds = new List<int>();

                var productInfoByIds = new Dictionary<int, Product>();

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
                            throw ProductUniConversionNameNotFound.BadRequestFormat(item.Unit2, item.ProductCode);
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


                    productUnitConversionIds.Add(productUnitConversionObj.ProductUnitConversionId);
                    productIds.Add(productObj.ProductId);
                    if (!productInfoByIds.ContainsKey(productObj.ProductId))
                    {
                        productInfoByIds.Add(productObj.ProductId, productObj);
                    }

                    long fromPackageId = 0;
                    Package packageInfo;
                    if (!string.IsNullOrWhiteSpace(item.FromPackageCode))
                    {
                        packageInfo = await _stockDbContext.Package.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productObj.ProductId && p.PackageCode == item.FromPackageCode && p.ProductUnitConversionId == productUnitConversionObj.ProductUnitConversionId);

                        if (packageInfo == null) throw ProductPackageWithCodeNotFound.BadRequestFormat(item.FromPackageCode, productObj.ProductCode);

                        fromPackageId = packageInfo.PackageId;
                    }
                    else
                    {
                        packageInfo = await _stockDbContext.Package.AsNoTracking()
                          .Where(p => p.ProductId == productObj.ProductId && p.ProductUnitConversionId == productUnitConversionObj.ProductUnitConversionId)
                          .OrderByDescending(p => p.PackageTypeId == (int)EnumPackageType.Default)
                          .FirstOrDefaultAsync();

                        if (packageInfo == null) throw PuProductPackageNotFound.BadRequestFormat(productObj.ProductCode, productUnitConversionObj?.ProductUnitConversionName);

                        fromPackageId = packageInfo.PackageId;
                    }


                    var puDefault = _productUnitsByProduct[productObj.ProductId].FirstOrDefault(u => u.IsDefault);

                    var calcModel = new QuantityPairInputModel()
                    {
                        PrimaryQuantity = item.Qty1 ?? 0,
                        PrimaryDecimalPlace = puDefault?.DecimalPlace ?? 12,

                        PuQuantity = item.Qty2 ?? 0,
                        PuDecimalPlace = productUnitConversionObj.DecimalPlace,

                        FactorExpression = productUnitConversionObj.FactorExpression,

                        FactorExpressionRate = packageInfo.ProductUnitConversionRemaining / packageInfo.PrimaryQuantityRemaining
                    };


                    var (isSuccess, primaryQuantity, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(calcModel);

                    if (isSuccess)
                    {
                        item.Qty1 = primaryQuantity;
                        item.Qty2 = pucQuantity;
                    }
                    else
                    {
                        throw new BadRequestException(ProductUnitConversionErrorCode.SecondaryUnitConversionError, $"{item.ProductCode} không thể tính giá trị ĐVCĐ, tính theo tỷ lệ: {pucQuantity.Format()}, nhập vào {item.Qty2?.Format()}, kiểm tra lại độ sai số đơn vị");
                    }

                    CalcMoney(item);

                    CalcPrice(item);

                    CalcMoney(item);

                    newInventoryOutProductModel.Add(new InventoryOutProductModel
                    {
                        SortOrder = sortOrder++,
                        ProductId = productObj.ProductId,
                        ProductUnitConversionId = productUnitConversionObj.ProductUnitConversionId,
                        PrimaryQuantity = item.Qty1 ?? 0,
                        ProductUnitConversionQuantity = item.Qty2 ?? 0,
                        UnitPrice = item.UnitPrice,
                        ProductUnitConversionPrice = item.Unit2Price,
                        Money = item.Money,
                        RefObjectTypeId = null,
                        RefObjectId = null,
                        RefObjectCode = item.CatePrefixCode,
                        FromPackageId = fromPackageId
                        //AccountancyAccountNumberDu = item.AccountancyAccountNumberDu
                    });
                }

                var firstRow = g.First();

                if (!firstRow.StockId.HasValue)
                {
                    throw InventoryImportFacadeMessage.StockInfoNotFound.BadRequest();
                }

                var newInventory = new InventoryOutModel
                {
                    StockId = firstRow.StockId.Value,
                    InventoryActionId = firstRow.InventoryActionId ?? EnumInventoryAction.Normal,
                    InventoryCode = g.Key,
                    //InventoryCode = string.Format("PX_TonDau_{0}", DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
                    Date = firstRow.Date.GetUnix(),

                    Shipper = firstRow.Shipper,
                    Content = firstRow.Description,
                    CustomerId = firstRow.Customer?.CustomerId,
                    Department = string.Empty,
                    DepartmentId = firstRow.Department?.DepartmentId,
                    StockKeeperUserId = null,
                    BillCode = firstRow.BillCode,
                    BillSerial = firstRow.BillSerial,
                    BillDate = firstRow.BillDate?.GetUnix(),
                    FileIdList = null,
                    OutProducts = newInventoryOutProductModel,
                    // AccountancyAccountNumber = _model.AccountancyAccountNumber
                };

                lst.Add(newInventory);
            }

            return lst;
        }

        private void CalcPrice(ImportInvOutputModel item)
        {
            if (!item.UnitPrice.HasValue && item.Qty1 > 0)
            {
                item.UnitPrice = item.Money / item.Qty1;
            }

            if (!item.Unit2Price.HasValue && item.Qty2 > 0)
            {
                item.Unit2Price = item.Money / item.Qty2;
            }

        }

        private void CalcMoney(ImportInvOutputModel item)
        {

            if (!item.Money.HasValue)
            {
                item.Money = item.Qty1 * item.UnitPrice;
            }

            if (!item.Money.HasValue)
            {
                item.Money = item.Qty2 * item.Unit2Price;
            }

        }

        private Product GetProduct(OpeningBalanceModel item)
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


    }

}
