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
using static Verp.Resources.Stock.Inventory.InventoryFileData.InventoryImportFacadeMessage;

namespace VErp.Services.Stock.Service.Stock.Implement.InventoryFileData
{
    internal class InventoryOutImportFacade
    {
        private Dictionary<string, Product> _productsByCode = null;
        private Dictionary<string, List<Product>> _productsByName = null;

        private IList<ImportInvOutputModel> _excelModel = null;
        private InventoryOutImportyExtraModel _model = null;

        private Dictionary<int, List<ProductUnitConversion>> _productUnitsByProduct = null;

        private StockDBContext _stockDbContext;

        public InventoryOutImportFacade SetStockDBContext(StockDBContext stockDbContext)
        {
            _stockDbContext = stockDbContext;
            return this;
        }



        public async Task ProcessExcelFile(ImportExcelMapping mapping, Stream stream, InventoryOutImportyExtraModel model)
        {
            _model = model;

            var stockInfo = await _stockDbContext.Stock.FirstOrDefaultAsync(s => s.StockId == model.StockId);
            if (stockInfo == null)
            {
                throw StockInfoNotFound.BadRequest(GeneralCode.ItemNotFound);
            }

            var reader = new ExcelReader(stream);

            var currentCateName = string.Empty;
            var currentCatePrefixCode = string.Empty;

            _excelModel = reader.ReadSheetEntity<ImportInvOutputModel>(mapping, (entity, propertyName, value) =>
            {
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


        public async Task<InventoryOutModel> GetOutputInventoryModel()
        {
            var inventoryOutList = new List<InventoryOutModel>();

            var totalRowCount = _excelModel.Count;

            var newInventoryOutProductModel = new List<InventoryOutProductModel>(totalRowCount);

            var productUnitConversionIds = new List<int>();
            var productIds = new List<int>();

            var productInfoByIds = new Dictionary<int, Product>();

            var sortOrder = 1;
            foreach (var item in _excelModel)
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
                if (!string.IsNullOrWhiteSpace(item.FromPackageCode))
                {
                    var packageInfo = await _stockDbContext.Package.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productObj.ProductId && p.PackageCode == item.FromPackageCode && p.ProductUnitConversionId == productUnitConversionObj.ProductUnitConversionId);

                    if (packageInfo == null) throw ProductPackageWithCodeNotFound.BadRequestFormat(item.FromPackageCode, productObj.ProductCode);

                    fromPackageId = packageInfo.PackageId;
                }
                else
                {
                    var packageInfo = await _stockDbContext.Package.AsNoTracking()
                       .Where(p => p.ProductId == productObj.ProductId && p.ProductUnitConversionId == productUnitConversionObj.ProductUnitConversionId)
                       .OrderByDescending(p => p.PackageTypeId == (int)EnumPackageType.Default)
                       .FirstOrDefaultAsync();

                    if (packageInfo == null) throw PuProductPackageNotFound.BadRequestFormat(productObj.ProductCode, productUnitConversionObj?.ProductUnitConversionName);

                    fromPackageId = packageInfo.PackageId;
                }

                newInventoryOutProductModel.Add(new InventoryOutProductModel
                {
                    SortOrder = sortOrder++,
                    ProductId = productObj.ProductId,
                    ProductUnitConversionId = productUnitConversionObj.ProductUnitConversionId,
                    PrimaryQuantity = item.Qty1,
                    ProductUnitConversionQuantity = item.Qty2,
                    UnitPrice = item.UnitPrice,
                    RefObjectTypeId = null,
                    RefObjectId = null,
                    RefObjectCode = item.CatePrefixCode,
                    FromPackageId = fromPackageId
                    //AccountancyAccountNumberDu = item.AccountancyAccountNumberDu
                });
            }


            var newInventory = new InventoryOutModel
            {
                StockId = _model.StockId,
                //InventoryCode = string.Format("PX_TonDau_{0}", DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
                Date = _model.IssuedDate,
                Shipper = string.Empty,
                Content = "Xuất kho từ excel",
                CustomerId = null,
                Department = string.Empty,
                StockKeeperUserId = null,
                BillCode = string.Empty,
                BillSerial = string.Empty,
                BillDate = _model.IssuedDate,
                FileIdList = null,
                OutProducts = newInventoryOutProductModel,
                // AccountancyAccountNumber = _model.AccountancyAccountNumber
            };

            return newInventory;
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
