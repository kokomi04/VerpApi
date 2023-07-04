using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Product.Pu;
using static Verp.Resources.Stock.Product.ProductValidationMessage;
using VErp.Services.Stock.Service.Products.Implement.ProductFacade;
using Microsoft.EntityFrameworkCore;
using Verp.Resources.Stock.Product;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Services.Stock.Service.Inventory.Implement.Abstract;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;

namespace VErp.Services.Stock.Service.Products.Implement.PuFacade
{
    public interface IPuImportFacadeService
    {
        Task<bool> Import(ImportExcelMapping mapping, Stream stream);
    }

    public class PuImportFacadeService : PuConversionValidateAbstract, IPuImportFacadeService
    {
        const int DECIMAL_PLACE_DEFAULT = 11;

        private MasterDBContext _masterDBContext;
        private ObjectActivityLogFacade _productActivityLog;
        private IProductService _productService;

        public PuImportFacadeService(
            StockDBContext stockContext
            , MasterDBContext masterDBContext
            , IActivityLogService activityLogService
            , IProductService productService
        ) : base(stockContext)
        {
            _masterDBContext = masterDBContext;
            _productActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Product);
            _productService = productService;
        }


        IDictionary<string, int> units = null;
        IDictionary<int, Unit> unitInfos = null;

        public async Task<bool> Import(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            // Lấy thông tin field
            var fields = typeof(Product).GetProperties(BindingFlags.Public);


            unitInfos = _masterDBContext.Unit.ToList().ToDictionary(u => u.UnitId, u => u);

            units = unitInfos.GroupBy(u => u.Value.UnitName.NormalizeAsInternalName()).ToDictionary(u => u.Key, u => u.First().Key);

            var data = reader.ReadSheetEntity<PuConversionImportRow>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return false;

                if (propertyName == nameof(PuConversionImportRow.ProductUnitConversionName))
                {
                    entity.ProductUnitConversionInternalName = value.NormalizeAsInternalName();
                }

                return false;
            });

            if (data.Count == 0)
            {
                throw GeneralCode.InvalidParams.BadRequest("Không có dòng nào được cập nhật!");
            }

            var propertyMaps = reader.GetPropertyPathMap();


            await LoadProducts(data, reader);

            foreach (var d in data)
            {
                if (d.DecimalPlace.HasValue && d.DecimalPlace.Value > 12)
                {
                    propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PuConversionImportRow>(x => x.DecimalPlace), out var puDecMap);

                    throw GeneralCode.InvalidParams.BadRequest($"Độ chính xác đơn vị tính {d.ProductUnitConversionName} được thiết lập cho mặt hàng {d.ProductInfo.ProductCode} {d.ProductInfo.ProductName}, dòng {d.RowNumber}, cột {puDecMap?.Column} phải nằm trong khoảng 0-12");
                }
            }

            var duplicate = data.GroupBy(d => new
            {
                ProductId = d.ProductInfo.ProductId,
                NameInternal = d.ProductUnitConversionInternalName?.NormalizeAsInternalName()
            }).Where(d => d.Count() > 1)
              .FirstOrDefault();

            if (duplicate != null)
            {

                propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PuConversionImportRow>(x => x.ProductUnitConversionName), out var puNameMap);

                var duplicateModel = duplicate.Skip(1).Take(1).First();
                throw GeneralCode.InvalidParams.BadRequest($"Có nhiều hơn 1 đơn vị tính {duplicateModel.ProductUnitConversionName} được thiết lập cho mặt hàng {duplicateModel.ProductInfo.ProductCode} {duplicateModel.ProductInfo.ProductName}, dòng {duplicateModel.RowNumber}, cột {puNameMap?.Column}");
            }

            var includeUnits = new List<Unit>();

            foreach (var row in data)
            {
                if (!string.IsNullOrWhiteSpace(row.ProductUnitConversionName)
                    && !units.ContainsKey(row.ProductUnitConversionName.NormalizeAsInternalName())
                    && !includeUnits.Any(u => u.UnitName.NormalizeAsInternalName() == row.ProductUnitConversionName.NormalizeAsInternalName())
                    && row.IsDefault
                    )
                {
                    includeUnits.Add(new Unit
                    {
                        UnitName = row.ProductUnitConversionName,
                        UnitStatusId = (int)EnumUnitStatus.Using,
                        DecimalPlace = row.DecimalPlace.GetValueOrDefault()

                    });
                }
            }

            _masterDBContext.Unit.AddRange(includeUnits);

            _masterDBContext.SaveChanges();
            _stockDbContext.SaveChanges();

            foreach (var unit in includeUnits)
            {
                unitInfos.Add(unit.UnitId, unit);
                units.Add(unit.UnitName.NormalizeAsInternalName(), unit.UnitId);
            }

            // Validate unique product code
            var productIds = data.Select(p => p.ProductInfo.ProductId).ToList();

            var productEntities = (await _stockDbContext.Product.Where(p => productIds.Contains(p.ProductId))
                .Include(p => p.ProductUnitConversion)
                .ToListAsync()
                ).ToDictionary(p => p.ProductId, p => p);

            var puModelsByProduct = data.GroupBy(p => p.ProductInfo.ProductId)
                .ToDictionary(p => p.Key, p => p.ToList());
            if (puModelsByProduct.Any(p => p.Value.Count(d => d.IsDefault) > 1))
            {
                var errorProduct = puModelsByProduct.First(p => p.Value.Count(d => d.IsDefault) > 1);
                var errorModel = errorProduct.Value.Last(p => p.IsDefault);


                propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PuConversionImportRow>(x => x.IsDefault), out var isDefaultMap);

                throw GeneralCode.InvalidParams.BadRequest($"Có nhiều hơn 1 đơn vị tính chính được thiết lập cho mặt hàng {errorModel.ProductInfo.ProductCode} {errorModel.ProductInfo.ProductName}, dòng {errorModel.RowNumber}, cột {isDefaultMap?.Column}");
            }


            //check product is in used
            if (mapping.ConfirmFlag != true && productEntities.Count() > 0)
            {
                var listProductIds = GetProductIdsHasUnitChange(puModelsByProduct, productEntities);
                if (listProductIds.Count() > 0)
                {

                    var productTopUsed = await _productService.GetProductTopInUsed(listProductIds, true);
                    if (productTopUsed.Count > 0)
                    {
                        productEntities.TryGetValue((int)productTopUsed.First().Id, out var usedUnitProduct);
                        throw GeneralCode.ItemInUsed.BadRequestFormatWithData(productTopUsed, CanNotUpdateUnitProductWhichInUsed, usedUnitProduct?.ProductCode + " " + productTopUsed.First().Description);
                    }
                }
            }


            using var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                using (var logBatch = _productActivityLog.BeginBatchLog())
                {
                    var changeRatePuIds = new List<long>();

                    foreach (var puByProduct in puModelsByProduct)
                    {
                        productEntities.TryGetValue(puByProduct.Key, out var productInfo);
                        if (productInfo != null)
                        {
                            var puEntities = productInfo.ProductUnitConversion.GroupBy(c => c.ProductUnitConversionName.NormalizeAsInternalName())
                                .ToDictionary(c => c.Key, c => c.First());

                            var toRemoveModels = new List<PuConversionImportRow>();
                            foreach (var puModel in puByProduct.Value)
                            {
                                var nameNormalize = puModel.ProductUnitConversionName.NormalizeAsInternalName();

                                ProductUnitConversion entity = null;

                                puEntities.TryGetValue(puModel.ProductUnitConversionInternalName, out entity);

                                if (entity == null && string.IsNullOrWhiteSpace(puModel.FactorExpression))
                                {
                                    throw GeneralCode.InvalidParams.BadRequest($"Vui lòng chọn tỷ lệ/biểu thức chuyển đổi cho đơn vị {puModel.ProductUnitConversionName}, mặt hàng {puModel.ProductInfo.ProductCode} {puModel.ProductInfo.ProductName}, dòng {puModel.RowNumber}");
                                }

                                if (entity == null || !string.IsNullOrWhiteSpace(puModel.FactorExpression))
                                {
                                    var exp = puModel.FactorExpression ?? "";
                                    var unit = puModel.ProductUnitConversionName;

                                    propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PuConversionImportRow>(x => x.FactorExpression), out var expMap);


                                    try
                                    {
                                        var eval = EvalUtils.EvalPrimaryQuantityFromProductUnitConversionQuantity(1, exp);
                                        if (!(eval > 0))
                                        {
                                            throw PuConversionExpressionInvalid.BadRequestFormat(unit + " (" + exp + ")", productInfo?.ProductCode + " " + productInfo?.ProductName + " (dòng " + puModel.RowNumber + ", cột " + expMap?.Column + ")");
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        throw PuConversionExpressionError.BadRequestFormat(unit + " (" + exp + ")", productInfo?.ProductCode + " " + productInfo?.ProductName + " (dòng " + puModel.RowNumber + ", cột " + expMap?.Column + ")");
                                    }

                                }

                                if (entity != null)
                                {
                                    switch (mapping.ImportDuplicateOptionId)
                                    {
                                        case EnumImportDuplicateOption.Denied:
                                            propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PuConversionImportRow>(x => x.ProductUnitConversionName), out var nameMap);

                                            throw GeneralCode.InvalidParams.BadRequest($"Đơn vị tính {puModel.ProductUnitConversionName} thuộc mặt hàng {productInfo.ProductCode}  {productInfo.ProductName} đã tồn tại, dòng " + puModel.RowNumber + ", cột " + nameMap?.Column);
                                        case EnumImportDuplicateOption.IgnoreBill:
                                            toRemoveModels.Add(puModel);
                                            break;
                                        case EnumImportDuplicateOption.Update:
                                            puModel.ProductUnitConversionId = entity.ProductUnitConversionId;

                                            if (puModel.IsDefault)
                                            {
                                                foreach (var puEntity in productInfo.ProductUnitConversion)
                                                {
                                                    if (puEntity.IsDefault)
                                                    {
                                                        puEntity.ConversionDescription = "";
                                                    }
                                                    puEntity.IsDefault = false;
                                                }

                                                if (!entity.IsDefault)
                                                {
                                                    entity.IsDefault = true;
                                                    entity.FactorExpression = "1";
                                                    changeRatePuIds.Add(entity.ProductUnitConversionId);
                                                }


                                                entity.UpdateIfAvaiable(v => v.ProductUnitConversionName, puModel.ProductUnitConversionName);
                                                entity.UpdateIfAvaiable(v => v.DecimalPlace, puModel.DecimalPlace);

                                                entity.ConversionDescription = "";


                                                productInfo.UnitId = units[nameNormalize];
                                            }
                                            else
                                            {
                                                if (entity.IsDefault && !string.IsNullOrWhiteSpace(puModel.FactorExpression) && puModel.FactorExpression?.Trim() != "1")
                                                {
                                                    throw GeneralCode.InvalidParams.BadRequest($"Không thể thiết lập tỷ lệ cho đơn vị chính, mặt hàng {productInfo.ProductCode}, Đơn vị {puModel.ProductUnitConversionName}");
                                                }

                                                entity.UpdateIfAvaiable(v => v.DecimalPlace, puModel.DecimalPlace);
                                                if (!string.IsNullOrWhiteSpace(puModel.FactorExpression))
                                                {
                                                    if (entity.FactorExpression?.Trim() != puModel.FactorExpression?.Trim())
                                                    {
                                                        changeRatePuIds.Add(entity.ProductUnitConversionId);
                                                        entity.UpdateIfAvaiable(v => v.FactorExpression, puModel.FactorExpression);
                                                    }

                                                }
                                                entity.UpdateIfAvaiable(v => v.ConversionDescription, puModel.ConversionDescription);
                                            }

                                            break;

                                    }
                                }
                                else
                                {
                                    if (puModel.IsDefault)
                                    {
                                        if (puModel.IsDefault && !string.IsNullOrWhiteSpace(puModel.FactorExpression) && puModel.FactorExpression?.Trim() != "1")
                                        {
                                            throw GeneralCode.InvalidParams.BadRequest($"Không thể thiết lập tỷ lệ cho đơn vị chính, mặt hàng {productInfo.ProductCode}, Đơn vị {puModel.ProductUnitConversionName}");
                                        }

                                        foreach (var puEntity in productInfo.ProductUnitConversion)
                                        {
                                            puEntity.IsDefault = false;
                                        }

                                        puModel.FactorExpression = "1";
                                        puModel.ConversionDescription = "";
                                        productInfo.UnitId = units[nameNormalize];
                                    }

                                    productInfo.ProductUnitConversion.Add(new ProductUnitConversionUpdate()
                                    {
                                        ProductId = productInfo.ProductId,
                                        ProductUnitConversionName = puModel.ProductUnitConversionName,
                                        SecondaryUnitId = units.ContainsKey(nameNormalize) ? units[nameNormalize] : 0,
                                        ConversionDescription = puModel.ConversionDescription,
                                        FactorExpression = puModel.FactorExpression,
                                        IsDefault = puModel.IsDefault,
                                        IsFreeStyle = false,
                                        DecimalPlace = puModel.DecimalPlace >= 0 ? puModel.DecimalPlace.Value : DECIMAL_PLACE_DEFAULT,
                                        UploadDecimalPlace = puModel.DecimalPlace
                                    });
                                }


                            }
                            foreach (var puModel in toRemoveModels)
                            {
                                puByProduct.Value.Remove(puModel);
                            }
                            if (puByProduct.Value.Count > 0)
                            {

                                await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.UpdateProductUnitConversionFromExcel)
                                      .MessageResourceFormatDatas(productInfo.ProductCode)
                                      .ObjectId(productInfo.ProductId)
                                      .JsonData(puByProduct.Value.JsonSerialize())
                                      .CreateLog();
                            }


                            if (productInfo.ProductUnitConversion.Where(u => u.IsDefault).Count() != 1)
                            {
                                throw GeneralCode.InvalidParams.BadRequest($"Mặt hàng phải có duy nhất 1 đơn vị chính {productInfo.ProductCode}");
                            }
                        }
                    }

                    await PuRateChangeValidateExistingInventoryData(changeRatePuIds);

                    _stockDbContext.SaveChanges();

                    await trans.CommitAsync();
                    await logBatch.CommitAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                trans.TryRollbackTransaction();
                throw;
            }
        }


        private async Task LoadProducts(IList<PuConversionImportRow> rowDatas, ExcelReader reader)
        {

            var propertyMaps = reader.GetPropertyPathMap();
            propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PuConversionImportRow>(x => x.ProductInfo.ProductCode), out var productCodeMap);
            propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PuConversionImportRow>(x => x.ProductInfo.ProductName), out var productNameMap);
            propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PuConversionImportRow>(x => x.ProductUnitConversionName), out var puNameMap);


            var productCodes = rowDatas.Select(r => r.ProductInfo.ProductCode).ToList();
            var productInternalNames = rowDatas.Select(r => r.ProductInfo.ProductName?.NormalizeAsInternalName()).ToList();

            var productInfos = await _productService.GetListByCodeAndInternalNames(new ProductQueryByProductCodeOrInternalNameRequest()
            {
                ProductCodes = productCodes,
                ProductInternalNames = productInternalNames,
            });

            var productInfoByCode = productInfos.GroupBy(p => p.ProductCode)
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            var productInfoByInternalName = productInfos.GroupBy(p => p.ProductName.NormalizeAsInternalName())
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            foreach (var item in rowDatas)
            {
                var productInternalName = item.ProductInfo?.ProductName?.NormalizeAsInternalName();

                IList<ProductModel> itemProducts = null;
                if (!string.IsNullOrWhiteSpace(item.ProductInfo.ProductCode) && productInfoByCode.ContainsKey(item.ProductInfo.ProductCode?.ToLower()))
                {
                    itemProducts = productInfoByCode[item.ProductInfo.ProductCode?.ToLower()];
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(productInternalName) && productInfoByInternalName.ContainsKey(productInternalName))
                    {
                        itemProducts = productInfoByInternalName[productInternalName];
                    }
                }

                if (itemProducts == null || itemProducts.Count == 0)
                {
                    throw ProductInfoNotFound.BadRequestFormat($"{item.ProductInfo.ProductCode} {item.ProductInfo.ProductName} dòng {item.RowNumber}, cột {productCodeMap?.Column} {productNameMap?.Column}");
                }

                if (itemProducts.Count > 1)
                {
                    itemProducts = itemProducts.Where(p => p.ProductName == item.ProductInfo.ProductName).ToList();

                    if (itemProducts.Count != 1)
                        throw FoundNumberOfProduct.BadRequestFormat(itemProducts.Count, $"{item.ProductInfo.ProductCode} {item.ProductInfo.ProductName} dòng {item.RowNumber}, cột {productCodeMap?.Column} {productNameMap?.Column}");
                }


                item.ProductInfo.ProductId = itemProducts[0].ProductId.Value;
            }
        }

        private List<int> GetProductIdsHasUnitChange(Dictionary<int, List<PuConversionImportRow>> puModelsByProduct, Dictionary<int, Product> existsProduct)
        {
            var listProductIds = new List<int>();

            foreach (var row in puModelsByProduct)
            {

                if (!existsProduct.ContainsKey(row.Key))
                {
                    throw new BadRequestException(GeneralCode.InternalError, "Existed product not found!");
                }

                var productInfo = existsProduct[row.Key];

                var defaultUnitModel = row.Value.FirstOrDefault(v => v.IsDefault);
                if (defaultUnitModel != null)
                {
                    var unitIdKey = defaultUnitModel.ProductUnitConversionName.NormalizeAsInternalName();
                    if (!string.IsNullOrEmpty(defaultUnitModel.ProductUnitConversionName) && units.ContainsKey(unitIdKey) && units[unitIdKey] != productInfo.UnitId)
                    {
                        listProductIds.Add(productInfo.ProductId);
                    }
                }
            }
            return listProductIds;
        }
    }
}
