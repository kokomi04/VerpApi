using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Verp.Resources.Enums.ErrorCodes.PO;
using Verp.Resources.PurchaseOrder.Po;
using VErp.Commons.Constants;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;
using VErp.Commons.GlobalObject.Org;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Services.PurchaseOrder.Model.Request;
using static Verp.Resources.PurchaseOrder.Po.PurchaseOrderParseExcelValidationMessage;
using static VErp.Commons.GlobalObject.InternalDataInterface.ProductModel;
using PurchaseOrderEntity = VErp.Infrastructure.EF.PurchaseOrderDB.PurchaseOrder;


namespace VErp.Services.PurchaseOrder.Service.Po.Implement.Facade
{
    public interface IPurchaseOrderImportExcelFacadeService
    {
        Task<bool> Import(ImportExcelMapping mapping, Stream stream, IPurchaseOrderService purchaseOrderService);

    }

    public class PurchaseOrderImportExcelFacadeService : IPurchaseOrderImportExcelFacadeService
    {
        public const int DECIMAL_PLACE_DEFAULT = 11;

        private readonly IProductHelperService _productHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IUserHelperService _userHelperService;
        private readonly ICategoryHelperService _categoryHelperService;
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ObjectActivityLogFacade _poActivityLog;

        private CurrencyData DefaultCurrency = null;

        public PurchaseOrderImportExcelFacadeService(IProductHelperService productHelperService, ICurrentContextService currentContextService, IOrganizationHelperService organizationHelperService, IUserHelperService userHelperService, ICategoryHelperService categoryHelperService, PurchaseOrderDBContext purchaseOrderDBContext, IActivityLogService activityLogService)
        {
            _productHelperService = productHelperService;
            _currentContextService = currentContextService;
            _organizationHelperService = organizationHelperService;
            _userHelperService = userHelperService;
            _categoryHelperService = categoryHelperService;
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _poActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PurchaseOrder);
        }

        public async Task<bool> Import(ImportExcelMapping mapping, Stream stream, IPurchaseOrderService purchaseOrderService)
        {
            var models = await GetModels(mapping, stream);
            if (models.Count == 0)
            {
                throw GeneralCode.InvalidParams.BadRequest("Không có dòng nào được cập nhật!");
            }

            using (var logBatch = _poActivityLog.BeginBatchLog())
            {

                using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
                {

                    var result = new Dictionary<PurchaseOrderInput, PurchaseOrderEntity>();
                    foreach (var model in models)
                    {
                        var entity = await purchaseOrderService.CreateToDb(model);
                        result.Add(model, entity);

                        await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.Import)
                          .MessageResourceFormatDatas(entity.PurchaseOrderCode)
                          .ObjectId(entity.PurchaseOrderId)
                          .JsonData((new { purchaseOrderType = EnumPurchasingOrderType.Default, model }).JsonSerialize())
                          .CreateLog();
                    }


                    trans.Commit();
                }

                await logBatch.CommitAsync();
                return true;

            }
        }

        private async Task<IList<PurchaseOrderInput>> GetModels(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var rowDatas = await ReadExcel(reader, mapping);

            var groupByCode = rowDatas.GroupBy(d => d.PurchaseOrderCode)
                .ToList();

            var poCodes = groupByCode.Select(g => g.Key.ToUpper()).ToList();

            var existstedCodes = (await _purchaseOrderDBContext.PurchaseOrder
                .Where(po => poCodes.Contains(po.PurchaseOrderCode))
                .Select(po => po.PurchaseOrderCode)
                .ToListAsync()
                )
                .Distinct()
                .Select(code => code.ToUpper())
                .ToHashSet();

            await LoadDefaultCurrency();

            var defaultCurrencyDecimalPlace = DefaultCurrency?.DecimalPlace ?? DECIMAL_PLACE_DEFAULT;



            var models = new List<PurchaseOrderInput>();

            var userIds = rowDatas.Select(d => d.DeliveryInfo.DeliveryUser.DeliveryUserId)
                .Where(d => d.HasValue)
                .Select(d => d.Value)
                .Distinct()
                .ToList();
            var userInfos = await _userHelperService.GetByIds(userIds);


            var customerIds = rowDatas.Select(d => d.DeliveryInfo.DeliveryCustomer.CustomerId)
                .Where(d => d.HasValue)
                .Select(d => d.Value)
                .Distinct()
                .ToList();

            var customerInfos = await _organizationHelperService.CustomerByIds(customerIds);


            var propertyMaps = reader.GetPropertyPathMap();

            foreach (var group in groupByCode)
            {
                var isExisted = existstedCodes.Contains(group.Key.ToUpper());

                if (isExisted)
                {
                    switch (mapping.ImportDuplicateOptionId)
                    {
                        case EnumImportDuplicateOption.Update:
                            throw GeneralCode.NotYetSupported.BadRequest();
                        case EnumImportDuplicateOption.Ignore:
                            continue;
                        case EnumImportDuplicateOption.Denied:
                            throw PurchaseOrderErrorCode.PoCodeAlreadyExisted.BadRequest(PurchaseOrderErrorCodeDescription.PoCodeAlreadyExisted + " " + group.Key);
                    }
                }

                var model = new PurchaseOrderInput();
                models.Add(model);

                model.PurchaseOrderCode = group.Key;

                var details = group.ToList();

                model.Date = details.GetFirstValueNotNull(x => x.Date)?.GetUnixUtc(_currentContextService.TimeZoneOffset) ?? 0;


                model.DeliveryDate = details.GetFirstValueNotNull(x => x.DeliveryDate)?.GetUnixUtc(_currentContextService.TimeZoneOffset);

                model.CustomerId = details.GetFirstValueNotNull(x => x.CustomerInfo.CustomerId) ?? 0;

                model.PoDescription = details.GetFirstValueNotNull(x => x.PoDescription);

                model.DeliveryUserId = details.GetFirstValueNotNull(x => x.DeliveryInfo.DeliveryUser.DeliveryUserId);
                var deliveryUserInfo = userInfos.FirstOrDefault(u => u.UserId == model.DeliveryUserId);

                model.DeliveryCustomerId = details.GetFirstValueNotNull(x => x.DeliveryInfo.DeliveryCustomer.CustomerId);
                var deliveryCustomerInfo = customerInfos.FirstOrDefault(u => u.CustomerId == model.DeliveryCustomerId);



                var deliverTo = details.GetFirstValueNotNull(x => x.DeliveryInfo.DeliverTo);
                var telephone = details.GetFirstValueNotNull(x => x.DeliveryInfo.Telephone);

                var company = details.GetFirstValueNotNull(x => x.DeliveryInfo.Company);
                var address = details.GetFirstValueNotNull(x => x.DeliveryInfo.Address);

                var fax = details.GetFirstValueNotNull(x => x.DeliveryInfo.Fax);
                var additionNote = details.GetFirstValueNotNull(x => x.DeliveryInfo.AdditionNote);

                if (deliverTo.IsNullObject())
                {
                    deliverTo = deliveryUserInfo?.FullName;
                }

                if (company.IsNullObject())
                {
                    company = deliveryCustomerInfo?.CustomerName;
                }

                if (address.IsNullObject())
                {
                    address = deliveryCustomerInfo?.Address;
                    if (address.IsNullObject())
                    {
                        address = deliveryUserInfo?.Address;
                    }
                }

                model.DeliveryDestination = new DeliveryDestinationModel()
                {
                    DeliverTo = deliverTo,
                    Telephone = telephone,
                    Company = company,
                    Address = address,
                };


                model.CurrencyId = details.GetFirstValueNotNull(x => x.Currency.F_Id);

                model.ExchangeRate = details.GetFirstValueNotNull(x => x.ExchangeRate);

                if (model.CurrencyId > 0 && (!model.ExchangeRate.HasValue || model.ExchangeRate <= 0))
                {
                    throw GeneralCode.InvalidParams.BadRequest($"Tỷ giá không hợp lệ, đơn mua {model.PurchaseOrderCode}, " +
                    $", dòng {details.First().RowNumber}");
                }

                model.DeliveryFee = details.GetFirstValueNotNull(x => x.DeliveryFee);

                model.OtherFee = details.GetFirstValueNotNull(x => x.OtherFee);

                model.TaxInPercent = details.GetFirstValueNotNull(x => x.TaxInPercent);

                model.Requirement = details.GetFirstValueNotNull(x => x.Requirement);

                model.AttachmentBill = details.GetFirstValueNotNull(x => x.AttachmentBill);

                model.OtherPolicy = details.GetFirstValueNotNull(x => x.OtherPolicy);

                model.DeliveryMethod = details.GetFirstValueNotNull(x => x.DeliveryMethod);

                model.DeliveryPolicy = details.GetFirstValueNotNull(x => x.DeliveryPolicy);

                model.PaymentMethod = details.GetFirstValueNotNull(x => x.PaymentMethod);

                model.PurchaseOrderType = details.GetFirstValueNotNull(x => x.PurchaseOrderType);

                model.Excess = new List<PurchaseOrderExcessModel>();

                model.Materials = new List<PurchaseOrderMaterialsModel>();

                model.Details = new List<PurchaseOrderInputDetail>();


                var currency = details.FirstOrDefault(d => d.Currency.F_Id > 0)?.Currency;
                var currencyDecimalPlace = currency?.DecimalPlace ?? DECIMAL_PLACE_DEFAULT;

                decimal sumMoney = 0;
                foreach (var detail in details)
                {
                    var detailModel = new PurchaseOrderInputDetail();

                    detailModel.ProviderProductName = detail.ProductProviderName;

                    detailModel.ProductId = detail.ProductInfo.ProductId;

                    detailModel.PrimaryQuantity = detail.PrimaryQuantity ?? 0;

                    detailModel.PrimaryUnitPrice = detail.PrimaryPrice ?? 0;

                    detailModel.ProductUnitConversionId = detail.PuInfo.ProductUnitConversionId;

                    detailModel.ProductUnitConversionQuantity = detail.ProductUnitConversionQuantity ?? 0;

                    detailModel.ProductUnitConversionPrice = detail.ProductUnitConversionPrice ?? 0;

                    detailModel.PoProviderPricingCode = detail.PoProviderPricingCode;

                    detailModel.OrderCode = detail.OrderCode;

                    detailModel.ProductionOrderCode = detail.ProductionOrderCode;

                    detailModel.Description = detail.Description;

                    detailModel.IntoMoney = detail.IntoMoney;

                    detailModel.ExchangedMoney = detail.ExchangedMoney;

                    detailModel.SortOrder = detail.SortOrder;


                    detailModel.IsSubCalculation = false;

                    detailModel.SubCalculations = new List<PurchaseOrderDetailSubCalculationModel>();

                    detailModel.OutsourceMappings = new List<PurchaseOrderOutsourceMappingModel>();


                    if (detailModel.PrimaryQuantity == 0 || detailModel.ProductUnitConversionQuantity == 0)
                    {
                        var calcModel = new QuantityPairInputModel()
                        {
                            PrimaryQuantity = detailModel.PrimaryQuantity,
                            PrimaryDecimalPlace = detail.PuDefault?.DecimalPlace ?? 12,

                            PuQuantity = detailModel.ProductUnitConversionQuantity,
                            PuDecimalPlace = detail.PuInfo.DecimalPlace,

                            FactorExpression = detail.PuInfo.FactorExpression,

                            FactorExpressionRate = null
                        };

                        var (isSuccess, primaryQuantity, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(calcModel);

                        if (isSuccess)
                        {
                            detailModel.PrimaryQuantity = primaryQuantity;
                            detailModel.ProductUnitConversionQuantity = pucQuantity;
                        }
                        else
                        {

                            propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PurchaseOrderImportModel>(x => x.ProductUnitConversionName), out var puMap);

                            throw GeneralCode.InvalidParams.BadRequest($"Lỗi tính đơn vị chuyển đổi {pucQuantity} {detail.PuInfo?.ProductUnitConversionName} = {primaryQuantity} {detail.PuDefault?.ProductUnitConversionName} mặt hàng {detail.ProductInfo.ProductCode}, dòng {detail.RowNumber}, cột {puMap?.Column}");
                        }
                    }

                    if (!detailModel.IntoMoney.HasValue || detailModel.IntoMoney == 0)
                    {
                        detailModel.IntoMoney = detailModel.PrimaryQuantity * detailModel.PrimaryUnitPrice;
                        if (detailModel.IntoMoney == 0)
                        {
                            detailModel.IntoMoney = detailModel.ProductUnitConversionQuantity * detailModel.ProductUnitConversionPrice;
                        }
                    }

                    detailModel.IntoMoney = detailModel.IntoMoney?.RoundBy(currencyDecimalPlace);

                    if (!detailModel.ExchangedMoney.HasValue || detailModel.ExchangedMoney == 0)
                    {
                        detailModel.ExchangedMoney = (model.ExchangeRate ?? 1) * detailModel.IntoMoney;
                    }
                    detailModel.ExchangedMoney = detailModel.ExchangedMoney?.RoundBy(defaultCurrencyDecimalPlace);


                    if (detailModel.PrimaryUnitPrice == 0)
                    {
                        detailModel.PrimaryUnitPrice = (detailModel.ExchangedMoney ?? 0) / detailModel.PrimaryQuantity;
                    }

                    if (detailModel.ProductUnitConversionPrice == 0)
                    {
                        detailModel.ProductUnitConversionPrice = (detailModel.ExchangedMoney ?? 0) / detailModel.ProductUnitConversionQuantity;
                    }

                    sumMoney += detailModel.ExchangedMoney ?? 0;

                    if (detailModel.PrimaryQuantity <= 0)
                    {
                        propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PurchaseOrderImportModel>(x => x.PrimaryQuantity), out var primaryQuantityMap);

                        propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PurchaseOrderImportModel>(x => x.ProductUnitConversionQuantity), out var puQuantityMap);


                        throw GeneralCode.InvalidParams.BadRequest($"Số lượng {detailModel.PrimaryQuantity} không hợp lệ, vui lòng nhập ít nhất số lượng 1 đơn vị tính {detail.PuInfo?.ProductUnitConversionName}, {detail.PuDefault?.ProductUnitConversionName} " +
                            $"mặt hàng {detail.ProductInfo.ProductCode} {detail.ProductInfo.ProductName}" +
                            $", dòng {detail.RowNumber} {primaryQuantityMap?.Column} {puQuantityMap?.Column}");
                    }

                    if (detailModel.PrimaryUnitPrice <= 0 || detailModel.IntoMoney <= 0)
                    {
                        propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PurchaseOrderImportModel>(x => x.PrimaryPrice), out var primaryPriceMap);

                        propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PurchaseOrderImportModel>(x => x.ProductUnitConversionPrice), out var puPriceMap);

                        propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PurchaseOrderImportModel>(x => x.IntoMoney), out var moneyMap);

                        throw GeneralCode.InvalidParams.BadRequest($"Đơn giá hoặc thành tiền không hợp lệ, vui lòng nhập ít nhất 1 đơn giá hoặc thành tiền, mặt hàng {detail.ProductInfo.ProductCode} {detail.ProductInfo.ProductName}, " +
                            $", dòng {detail.RowNumber} {primaryPriceMap?.Column} {puPriceMap?.Column} {moneyMap?.Column}");
                    }

                    model.Details.Add(detailModel);
                }

                model.TaxInPercent = details.GetFirstValueNotNull(x => x.TaxInPercent);
                model.TaxInMoney = details.GetFirstValueNotNull(x => x.TaxInMoney);
                if ((!model.TaxInMoney.HasValue || model.TaxInMoney == 0) && model.TaxInPercent > 0)
                {
                    model.TaxInMoney = (model.TaxInPercent * sumMoney / 100.0M)?.RoundBy(defaultCurrencyDecimalPlace);
                }

                model.TotalMoney = details.GetFirstValueNotNull(x => x.TotalMoney);

                if (model.TotalMoney == 0)
                {
                    model.TotalMoney += ((sumMoney + model.TaxInMoney)?.RoundBy(defaultCurrencyDecimalPlace)) ?? 0;
                }
            }

            return models;
        }



        /*
        private T ValidateMoreThanOne<T, TKey>(IList<T> lst, Expression<Func<T, TKey>> func, ConcurrentDictionary<string, PropertyPathSeparateByPoint> propertyMaps, string code) where T : MappingDataRowAbstract
        {
            var fn = func.Compile();
            var distinctValues = lst.Where(x =>
            {
                var valid = fn(x);
                return !valid.IsNullObject();
            }).GroupBy(fn)
              .ToDictionary(x => x.Key, x => x.First());


            if (distinctValues.Count > 1)
            {
                var propMapString = ExcelUtils.GetFullPropertyPath(func);
                propertyMaps.TryGetValue(propMapString, out var propMap);

                var nextValue = distinctValues.Skip(1).First();

                throw GeneralCode.InvalidParams.BadRequest($"Có nhiều hơn 1 {propMap?.DisplayTitle} {nextValue.Key}, dòng {nextValue.Value.RowNumber}, cột {propMap?.Column} cùng đơn {code}");
            }
            if (distinctValues.Count == 1)
            {
                return distinctValues.FirstOrDefault().Value;
            }

            return null;

        }*/

        public async Task<IList<PurchaseOrderImportModel>> ReadExcel(ExcelReader reader, ImportExcelMapping mapping)
        {

            var deliveryUserPropertyPath = ExcelUtils.GetFullPropertyPath<PurchaseOrderImportModel>(x => x.DeliveryInfo.DeliveryUser);
            var deliveryCustomerPropertyPath = ExcelUtils.GetFullPropertyPath<PurchaseOrderImportModel>(x => x.DeliveryInfo.DeliveryCustomer);

            if (!mapping.MappingFields.Any(m => m.FieldName == nameof(PurchaseOrderImportModel.PurchaseOrderCode)))
            {
                throw GeneralCode.InvalidParams.BadRequest(PurchaseOrderCodeIsRequired);
            }

            if (!mapping.MappingFields.Any(m => m.FieldName == nameof(PurchaseOrderImportModel.Date)))
            {
                throw GeneralCode.InvalidParams.BadRequest(PurchaseOrderDateIsRequired);
            }
            if (!mapping.MappingFields.Any(m => m.FieldName == nameof(PurchaseOrderImportModel.DeliveryDate)))
            {
                throw GeneralCode.InvalidParams.BadRequest(PurchaseOrderDeliveryDateIsRequired);
            }

            var rowDatas = await reader.ReadSheetEntity<PurchaseOrderImportModel>(mapping, async (entity, propertyName, value, refObj, refPropertyName, refPropertyPathSeparateByPoint) =>
            {
                if (propertyName == nameof(PurchaseOrderImportModel.PurchaseOrderCode))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw GeneralCode.InvalidParams.BadRequest(PurchaseOrderCodeIsRequired);
                    }
                    return false;
                }

                if (string.IsNullOrWhiteSpace(value)) return true;
                var normalizeValue = value.NormalizeAsInternalName();


                if (propertyName == nameof(PurchaseOrderImportModel.CustomerInfo))
                {
                    await ReadProvider(refObj, refPropertyName, value, normalizeValue);

                    return true;
                }

                if (propertyName == nameof(PurchaseOrderImportModel.DeliveryInfo))
                {
                    if (refPropertyPathSeparateByPoint.StartsWith(deliveryUserPropertyPath))
                    {
                        await ReadDeliveryInfoUser(refObj, refPropertyName, value, normalizeValue);
                        return true;
                    }

                    if (refPropertyPathSeparateByPoint.StartsWith(deliveryCustomerPropertyPath))
                    {
                        await ReadDeliveryInfoCustomer(refObj, refPropertyName, value, normalizeValue);
                        return true;
                    }

                    return false;
                }

                if (propertyName == nameof(PurchaseOrderImportModel.Currency))
                {
                    await ReadCurrency(entity, refPropertyName, value, refPropertyPathSeparateByPoint);
                    return true;
                }


                return false;
            });

            rowDatas = rowDatas.OrderBy(d => d.SortOrder).ToList();

            var propertyMaps = reader.GetPropertyPathMap();
            propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PurchaseOrderImportModel>(x => x.ProductInfo.ProductCode), out var productCodeMap);
            propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PurchaseOrderImportModel>(x => x.ProductInfo.ProductName), out var productNameMap);
            propertyMaps.TryGetValue(ExcelUtils.GetFullPropertyPath<PurchaseOrderImportModel>(x => x.ProductUnitConversionName), out var puNameMap);

            await LoadProducts(rowDatas, productCodeMap, productNameMap, puNameMap);
            return rowDatas;
        }


        IDictionary<string, List<BasicCustomerListModel>> customerByCodes = null;
        IDictionary<string, List<BasicCustomerListModel>> customerByNames = null;

        private async Task EnsureGetAllCustomers()
        {
            if (customerByCodes != null) return;

            var allCustomer = await _organizationHelperService.AllCustomers();

            customerByCodes = allCustomer.GroupBy(c => c.CustomerCode?.NormalizeAsInternalName())
               .ToDictionary(c => c.Key, c => c.ToList());

            customerByNames = allCustomer.GroupBy(c => c.CustomerName?.NormalizeAsInternalName())
            .ToDictionary(c => c.Key, c => c.ToList());
        }

        private async Task ReadProvider(object refObj, string refPropertyName, string value, string normalizeValue)
        {
            await EnsureGetAllCustomers();

            var obj = (ProviderCustomerImportModel)refObj;
            if (refPropertyName == nameof(ProviderCustomerImportModel.CustomerCode))
            {
                if (!customerByCodes.ContainsKey(normalizeValue)) throw CustomerCodeNotFound.BadRequestFormat(value);
                var customerInfos = customerByCodes[normalizeValue];
                var customerInfo = customerInfos.OrderByDescending(c => c.CustomerCode == value).First();
                obj.CustomerCode = value;
                obj.CustomerId = customerInfo.CustomerId;

                return;
            }

            if (refPropertyName == nameof(ProviderCustomerImportModel.CustomerName))
            {
                if (!customerByNames.ContainsKey(normalizeValue)) throw CustomerNameNotFound.BadRequestFormat(value);
                var customerInfos = customerByNames[normalizeValue];
                var customerInfo = customerInfos.OrderByDescending(c => c.CustomerName == value).First();
                obj.CustomerName = value;
                obj.CustomerId = customerInfo.CustomerId;
                return;
            }
        }



        IDictionary<string, List<EmployeeBasicNameModel>> userByCodes = null;
        IDictionary<string, List<EmployeeBasicNameModel>> userByFullNames = null;
        private async Task EnsureGetAllUsers()
        {
            if (userByCodes != null) return;

            var allUsers = await _userHelperService.GetAll();

            userByCodes = allUsers.GroupBy(c => c.EmployeeCode?.NormalizeAsInternalName())
                .Where(c => !string.IsNullOrWhiteSpace(c.Key))
               .ToDictionary(c => c.Key, c => c.ToList());

            userByFullNames = allUsers.GroupBy(c => c.FullName?.NormalizeAsInternalName())
                .Where(c => !string.IsNullOrWhiteSpace(c.Key))
                .ToDictionary(c => c.Key, c => c.ToList());
        }

        private async Task ReadDeliveryInfoUser(object refObj, string refPropertyName, string value, string normalizeValue)
        {
            await EnsureGetAllUsers();
            var obj = (DeliveryUserImportModel)refObj;
            if (refPropertyName == nameof(DeliveryUserImportModel.EmployeeCode))
            {
                if (!userByCodes.ContainsKey(normalizeValue)) throw EmployeeCodeNotFound.BadRequestFormat(value);
                var userInfos = userByCodes[normalizeValue];
                var userInfo = userInfos.OrderByDescending(c => c.EmployeeCode == value).First();
                obj.EmployeeCode = value;
                obj.DeliveryUserId = userInfo.UserId;

                return;
            }

            if (refPropertyName == nameof(DeliveryUserImportModel.FullName))
            {
                if (!userByFullNames.ContainsKey(normalizeValue)) throw EmployeeFullNameNotFound.BadRequestFormat(value);
                var userInfos = userByFullNames[normalizeValue];
                var userInfo = userInfos.OrderByDescending(c => c.FullName == value).First();
                obj.FullName = value;
                obj.DeliveryUserId = userInfo.UserId;
                return;
            }
        }

        private async Task ReadDeliveryInfoCustomer(object refObj, string refPropertyName, string value, string normalizeValue)
        {
            await EnsureGetAllCustomers();

            var obj = (DeliveryCustomerImportModel)refObj;
            if (refPropertyName == nameof(DeliveryCustomerImportModel.CustomerCode))
            {
                if (!customerByCodes.ContainsKey(normalizeValue)) throw DeliveryCustomerCodeNotFound.BadRequestFormat(value);
                var customerInfos = customerByCodes[normalizeValue];
                var customerInfo = customerInfos.OrderByDescending(c => c.CustomerCode == value).First();
                obj.CustomerCode = value;
                obj.CustomerId = customerInfo.CustomerId;

                return;
            }

            if (refPropertyName == nameof(DeliveryCustomerImportModel.CustomerName))
            {
                if (!customerByNames.ContainsKey(normalizeValue)) throw DeliveryCustomerNameNotFound.BadRequestFormat(value);
                var customerInfos = customerByNames[normalizeValue];
                var customerInfo = customerInfos.OrderByDescending(c => c.CustomerName == value).First();
                obj.CustomerName = value;
                obj.CustomerId = customerInfo.CustomerId;
                return;
            }
        }


        private async Task ReadCurrency(PurchaseOrderImportModel entity, string refPropertyName, string value, string refPropertyPathSeparateByPoint)
        {
            var fieldInfos = await _categoryHelperService.GetReferFields(new[] { CurrencyCateConstants.CurrencyCategoryCode }, new[] { refPropertyName });
            if (fieldInfos == null || fieldInfos.Count == 0)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy trường dữ liệu {refPropertyName} thuộc {refPropertyPathSeparateByPoint}");
            }
            var fieldInfo = fieldInfos.First();
            var dataTypeId = (EnumDataType)fieldInfo.DataTypeId;

            var clause = new SingleClause()
            {
                DataType = dataTypeId,
                FieldName = refPropertyName,
                Operator = EnumOperator.Equal,
                Value = value.ConvertValueByType(dataTypeId)
            };

            var rowInfo = await _categoryHelperService.GetDataRows(CurrencyCateConstants.CurrencyCategoryCode, new CategoryFilterModel()
            {
                Filters = clause,
                Page = 1,
                Size = 2
            });
            if (rowInfo.List.Count == 0)
            {
                throw CurrencyNotFound.BadRequestFormat(fieldInfo.CategoryFieldTitle, value);
            }

            if (rowInfo.List.Count > 1)
            {
                throw CurrencyFoundMoreThanOne.BadRequestFormat(fieldInfo.CategoryFieldTitle, value);
            }


            entity.Currency = GetCurrency(rowInfo.List[0]);


        }

        private async Task LoadDefaultCurrency()
        {
            var defaultCurrency = await _categoryHelperService.GetDataRows(CurrencyCateConstants.CurrencyCategoryCode, new CategoryFilterModel()
            {
                Filters = new SingleClause()
                {
                    DataType = EnumDataType.Boolean,
                    FieldName = CurrencyCateConstants.IsPrimary,
                    Operator = EnumOperator.Equal,
                    Value = true
                },
                Page = 1,
                Size = 2
            });

            DefaultCurrency = GetCurrency(defaultCurrency.List.Count > 0 ? defaultCurrency.List[0] : null);
        }

        private CurrencyData GetCurrency(NonCamelCaseDictionary data)
        {
            if (data == null) return null;
            var decimalPlace = data[CurrencyCateConstants.DecimalPlace];
            return new CurrencyData()
            {
                F_Id = Convert.ToInt64(data[CategoryFieldConstants.F_Id]),
                CurrencyCode = data[CurrencyCateConstants.CurrencyCode]?.ToString(),
                CurrencyName = data[CurrencyCateConstants.CurrencyName]?.ToString(),
                DecimalPlace = decimalPlace.IsNullObject() ? DECIMAL_PLACE_DEFAULT : Convert.ToInt32(decimalPlace),
            };
        }

        private async Task LoadProducts(IList<PurchaseOrderImportModel> rowDatas, PropertyMappingInfo productCodeMap, PropertyMappingInfo productNameMap, PropertyMappingInfo puNameMap)
        {
            var productCodes = rowDatas.Select(r => r.ProductInfo.ProductCode).ToList();
            var productInternalNames = rowDatas.Select(r => r.ProductInternalName).ToList();

            var productInfos = await _productHelperService.GetListByCodeAndInternalNames(productCodes, productInternalNames);

            var productInfoByCode = productInfos.GroupBy(p => p.ProductCode)
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            var productInfoByInternalName = productInfos.GroupBy(p => p.ProductName.NormalizeAsInternalName())
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            foreach (var item in rowDatas)
            {
                IList<ProductModel> itemProducts = null;
                if (!string.IsNullOrWhiteSpace(item.ProductInfo.ProductCode) && productInfoByCode.ContainsKey(item.ProductInfo.ProductCode?.ToLower()))
                {
                    itemProducts = productInfoByCode[item.ProductInfo.ProductCode?.ToLower()];
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(item.ProductInternalName) && productInfoByInternalName.ContainsKey(item.ProductInternalName))
                    {
                        itemProducts = productInfoByInternalName[item.ProductInternalName];
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

                ProductModelUnitConversion productUnitConversion = null;

                var puDefault = itemProducts[0].StockInfo.UnitConversions.FirstOrDefault(u => u.IsDefault);

                if (!string.IsNullOrWhiteSpace(item.ProductUnitConversionName))
                {
                    var pus = itemProducts[0].StockInfo.UnitConversions
                            .Where(u => u.ProductUnitConversionName.NormalizeAsInternalName() == item.ProductUnitConversionName.NormalizeAsInternalName())
                            .ToList();

                    if (pus.Count != 1)
                    {
                        pus = itemProducts[0].StockInfo.UnitConversions
                           .Where(u => u.ProductUnitConversionName.Contains(item.ProductUnitConversionName) || item.ProductUnitConversionName.Contains(u.ProductUnitConversionName))
                           .ToList();

                        if (pus.Count > 1)
                        {
                            pus = itemProducts[0].StockInfo.UnitConversions
                             .Where(u => u.ProductUnitConversionName.Equals(item.ProductUnitConversionName, StringComparison.OrdinalIgnoreCase))
                             .ToList();
                        }
                    }

                    if (pus.Count == 0)
                    {
                        throw PuOfProductNotFound.BadRequestFormat(item.ProductUnitConversionName, $"{item.ProductInfo.ProductCode} {item.ProductInfo.ProductName} dòng {item.RowNumber}, cột {puNameMap?.Column}");
                    }
                    if (pus.Count > 1)
                    {
                        throw FoundNumberOfPuConversion.BadRequestFormat(pus.Count, item.ProductUnitConversionName, $"{item.ProductInfo.ProductCode} {item.ProductInfo.ProductName} dòng {item.RowNumber}, cột {puNameMap?.Column}");
                    }

                    productUnitConversion = pus[0];

                }
                else
                {

                    if (puDefault == null)
                    {
                        throw PrimaryPuOfProductNotFound.BadRequestFormat($"{item.ProductInfo.ProductCode} {item.ProductInfo.ProductName} dòng {item.RowNumber}, cột {productCodeMap?.Column} {productNameMap?.Column}");

                    }
                    productUnitConversion = puDefault;
                }

                item.ProductInfo.ProductId = itemProducts[0].ProductId.Value;
                item.PuDefault = puDefault;
                item.PuInfo = productUnitConversion;
            }
        }

    }
}
