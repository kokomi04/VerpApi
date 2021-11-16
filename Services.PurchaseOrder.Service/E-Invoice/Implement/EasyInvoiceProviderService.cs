using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.Enums.E_Invoice;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.E_Invoice;
using System.Linq;
using VErp.Commons.Constants;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using System.Net.Http;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Globalization;
using VErp.Services.PurchaseOrder.Model.E_Invoice.EasyInvoice;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Service.Voucher;

namespace VErp.Services.PurchaseOrder.Service.E_Invoice.Implement
{
    public interface IEasyInvoiceProviderService
    {
        Task<bool> CancelElectronicInvoice(long voucherTypeId, long voucherBillId);
        Task<(Stream stream, string fileName, string contentType)> GetElectronicInvoicePdf(long voucherTypeId, long voucherBillId, int option);
        Task<bool> IssueElectronicInvoice(long voucherTypeId, long voucherBillId);
    }

    public class EasyInvoiceProviderService : IEasyInvoiceProviderService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _objectActivityLog;
        private readonly IHttpClientFactoryService _httpClient;
        private readonly IVoucherDataService _voucherDataService;
        private readonly ICurrentContextService _currentContextService;

        public EasyInvoiceProviderService(IHttpClientFactoryService httpClient, PurchaseOrderDBContext purchaseOrderDBContext, IMapper mapper, IActivityLogService activityLogService, IVoucherDataService voucherDataService, ICurrentContextService currentContextService)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _mapper = mapper;
            _objectActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.EasyInvoiceProvider);
            _httpClient = httpClient;
            _voucherDataService = voucherDataService;
            _currentContextService = currentContextService;
        }

        private async Task<(string, string, string, EnumElectronicInvoiceType, PageDataTable)> GetInfoVoucherEInvoice(long voucherTypeId, long voucherBillId)
        {
            var voucherData = await _voucherDataService.GetVoucherBillInfoRows((int)voucherTypeId, voucherBillId, "", false, 0, 0);

            var einvoiceType = EnumElectronicInvoiceType.ElectronicInvoiceNormal;
            string einvoicePattern, einvoiceSerial, voucherBillCode;

            einvoicePattern = einvoiceSerial = voucherBillCode = "";

            if (voucherData.List.Count > 0)
            {
                var firstRow = voucherData.List[0];
                if (firstRow.ContainsKey(VoucherConstants.VOUCHER_E_INVOICE_TYPE))
                {
                    einvoiceType = (EnumElectronicInvoiceType)(firstRow[VoucherConstants.VOUCHER_E_INVOICE_TYPE]);
                }

                if (firstRow.ContainsKey(VoucherConstants.VOUCHER_E_INVOICE_PATTERN))
                {
                    einvoicePattern = firstRow[VoucherConstants.VOUCHER_E_INVOICE_PATTERN]?.ToString();
                }

                if (firstRow.ContainsKey(VoucherConstants.VOUCHER_E_INVOICE_SERIAL))
                {
                    einvoiceSerial = firstRow[VoucherConstants.VOUCHER_E_INVOICE_SERIAL]?.ToString();
                }

                if (firstRow.ContainsKey(VoucherConstants.VOUCHER_BILL_CODE))
                {
                    voucherBillCode = firstRow[VoucherConstants.VOUCHER_BILL_CODE]?.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(einvoicePattern) || string.IsNullOrWhiteSpace(einvoiceSerial))
                throw ElectronicInvoiceProviderErrorCode.NotFoundPatternOrSerialOfEInvoice.BadRequest();

            return (voucherBillCode, einvoicePattern, einvoiceSerial, einvoiceType, voucherData);
        }

        public async Task<bool> IssueElectronicInvoice(long voucherTypeId, long voucherBillId)
        {
            var (_, pattern, serial, einvoiceType, voucherData) = await GetInfoVoucherEInvoice(voucherTypeId, voucherBillId);

            return einvoiceType switch
            {
                EnumElectronicInvoiceType.ElectronicInvoiceNormal => await IssueElectronicInvoice(pattern, serial, voucherTypeId, voucherBillId, voucherData.List),
                EnumElectronicInvoiceType.ElectronicInvoiceModify => await ModifyElectronicInvoice(pattern, serial, voucherTypeId, voucherBillId, voucherData.List),
                EnumElectronicInvoiceType.ElectronicInvoiceReplace => await ReplaceElectronicInvoice(pattern, serial, voucherTypeId, voucherBillId, voucherData.List),
                _ => throw new ArgumentException(message: "Not found electronic invoice type", paramName: nameof(einvoiceType))
            };
        }

        private async Task<bool> IssueElectronicInvoice(string pattern, string serial, long voucherTypeId, long voucherBillId, IList<NonCamelCaseDictionary> data)
        {

            var configEntity = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(x => x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice);
            var mappingEntity = await _purchaseOrderDBContext.ElectronicInvoiceMapping.FirstOrDefaultAsync(x =>
                x.ElectronicInvoiceFunctionId == (int)EnumElectronicInvoiceFunction.Issue &&
                x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice &&
                x.VoucherTypeId == voucherTypeId
            );

            if (configEntity == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceConfig.BadRequest();

            if (mappingEntity == null)
                throw ElectronicInvoiceMappingErrorCode.NotFoundElectronicInvoiceMapping.BadRequest();

            var config = _mapper.Map<ElectronicInvoiceProviderModel>(configEntity);
            var mappingFields = _mapper.Map<ElectronicInvoiceMappingModel>(mappingEntity);

            var functionConfig = config.FieldsConfig.FirstOrDefault(x => x.ElectronicInvoiceFunctionId == EnumElectronicInvoiceFunction.Issue);

            if (functionConfig == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceFunction.BadRequest();

            string xmlData = GetXmlDataOfCreateEInvoice(mappingFields, functionConfig, data);
            // var uri = $"{config.EasyInvoiceConnection.HostName.TrimEnd('/')}/api/publish/importInvoice";
            var uri = $"{config.EasyInvoiceConnection.HostName.TrimEnd('/')}/api/publish/importAndIssueInvoice";

            var responseData = await _httpClient.Post<EasyInvoiceResponseModel>(uri, new EasyInvoiceRequestModel()
            {
                XmlData = xmlData,
                Pattern = pattern,
                Serial = serial
            }, request => EasyInvoiceAuthentication(request, nameof(HttpMethod.Post), config.EasyInvoiceConnection.UserName, config.EasyInvoiceConnection.Password), errorHandler: EasyInvoiceErrorHandler);

            if (responseData.Status != 2)
                throw ElectronicInvoiceProviderErrorCode.EInvoiceProcessFailed.BadRequest(responseData.JsonSerialize());

            var invoiceData = responseData.Data.Invoices.FirstOrDefault();

            await UpdateInvoiceInfoForVoucherBill(voucherBillId, invoiceData, config.UrlSearch);

            return await Task.FromResult(true);
        }

        private async Task<bool> ModifyElectronicInvoice(string pattern, string serial, long voucherTypeId, long voucherBillId, IList<NonCamelCaseDictionary> voucherData)
        {
            var configEntity = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(x => x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice);
            var mappingEntity = await _purchaseOrderDBContext.ElectronicInvoiceMapping.FirstOrDefaultAsync(x =>
                x.ElectronicInvoiceFunctionId == (int)EnumElectronicInvoiceFunction.Adjust &&
                x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice &&
                x.VoucherTypeId == voucherTypeId
            );

            if (configEntity == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceConfig.BadRequest();

            if (mappingEntity == null)
                throw ElectronicInvoiceMappingErrorCode.NotFoundElectronicInvoiceMapping.BadRequest();

            var config = _mapper.Map<ElectronicInvoiceProviderModel>(configEntity);
            var mappingFields = _mapper.Map<ElectronicInvoiceMappingModel>(mappingEntity);

            var functionConfig = config.FieldsConfig.FirstOrDefault(x => x.ElectronicInvoiceFunctionId == EnumElectronicInvoiceFunction.Adjust);

            if (functionConfig == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceFunction.BadRequest();


            if (voucherData.Count == 0)
                throw ElectronicInvoiceProviderErrorCode.NotFoundXmlData.BadRequest();

            if (!voucherData.Any(x => x.ContainsKey(VoucherConstants.VOUCHER_E_INVOICE_PARENT) && x[VoucherConstants.VOUCHER_E_INVOICE_PARENT] != null))
                throw ElectronicInvoiceMappingErrorCode.NotFoundElectronicInvoiceParent.BadRequest();

            var voucherParentCode = voucherData.FirstOrDefault()[VoucherConstants.VOUCHER_E_INVOICE_PARENT] as string;

            string xmlData = GetXmlDataOfModifyEInvoice(mappingFields, functionConfig, voucherData);
            var uri = $"{config.EasyInvoiceConnection.HostName.TrimEnd('/')}/api/business/adjustInvoice";

            var responseData = await _httpClient.Post<EasyInvoiceResponseModel>(uri, new EasyInvoiceRequestModel()
            {
                XmlData = xmlData,
                Pattern = pattern,
                Serial = serial,
                Ikey = voucherParentCode
            }, request => EasyInvoiceAuthentication(request, nameof(HttpMethod.Post), config.EasyInvoiceConnection.UserName, config.EasyInvoiceConnection.Password), errorHandler: EasyInvoiceErrorHandler);

            if (responseData.Status != 2)
                throw ElectronicInvoiceProviderErrorCode.EInvoiceProcessFailed.BadRequest(responseData.JsonSerialize());

            var invoiceData = responseData.Data.Invoices.FirstOrDefault();

            await UpdateInvoiceInfoForVoucherBill(voucherBillId, invoiceData, config.UrlSearch);
            await UpdateEInvoiceStatus(voucherParentCode, EnumElectronicInvoiceStatus.EInvoiceAdjusted);

            return true;
        }

        private async Task<bool> ReplaceElectronicInvoice(string pattern, string serial, long voucherTypeId, long voucherBillId, IList<NonCamelCaseDictionary> voucherData)
        {
            var configEntity = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(x => x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice);
            var mappingEntity = await _purchaseOrderDBContext.ElectronicInvoiceMapping.FirstOrDefaultAsync(x =>
                x.ElectronicInvoiceFunctionId == (int)EnumElectronicInvoiceFunction.Replace &&
                x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice &&
                x.VoucherTypeId == voucherTypeId
            );

            if (configEntity == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceConfig.BadRequest();

            if (mappingEntity == null)
                throw ElectronicInvoiceMappingErrorCode.NotFoundElectronicInvoiceMapping.BadRequest();

            var config = _mapper.Map<ElectronicInvoiceProviderModel>(configEntity);
            var mappingFields = _mapper.Map<ElectronicInvoiceMappingModel>(mappingEntity);

            var functionConfig = config.FieldsConfig.FirstOrDefault(x => x.ElectronicInvoiceFunctionId == EnumElectronicInvoiceFunction.Adjust);

            if (functionConfig == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceFunction.BadRequest();


            if (voucherData.Count == 0)
                throw ElectronicInvoiceProviderErrorCode.NotFoundXmlData.BadRequest();

            if (!voucherData.Any(x => x.ContainsKey(VoucherConstants.VOUCHER_E_INVOICE_PARENT) && x[VoucherConstants.VOUCHER_E_INVOICE_PARENT] != null))
                throw ElectronicInvoiceMappingErrorCode.NotFoundElectronicInvoiceParent.BadRequest();

            var voucherParentCode = voucherData.FirstOrDefault()[VoucherConstants.VOUCHER_E_INVOICE_PARENT] as string;

            string xmlData = GetXmlDataOfReplaceEInvoice(mappingFields, functionConfig, voucherData);

            var uri = $"{config.EasyInvoiceConnection.HostName.TrimEnd('/')}/api/business/replaceInvoice";

            var responseData = await _httpClient.Post<EasyInvoiceResponseModel>(uri, new EasyInvoiceRequestModel()
            {
                XmlData = xmlData,
                Ikey = voucherParentCode,
                Pattern = pattern,
                Serial = serial
            }, request => EasyInvoiceAuthentication(request, nameof(HttpMethod.Post), config.EasyInvoiceConnection.UserName, config.EasyInvoiceConnection.Password), errorHandler: EasyInvoiceErrorHandler);

            if (responseData.Status != 2)
                throw ElectronicInvoiceProviderErrorCode.EInvoiceProcessFailed.BadRequest(responseData.JsonSerialize());

            var invoiceData = responseData.Data.Invoices.FirstOrDefault();

            await UpdateInvoiceInfoForVoucherBill(voucherBillId, invoiceData, config.UrlSearch);

            await UpdateEInvoiceStatus(voucherParentCode, EnumElectronicInvoiceStatus.EInvoiceReplaced);
            return true;
        }

        public async Task<bool> CancelElectronicInvoice(long voucherTypeId, long voucherBillId)
        {
            var (voucherBillCode, pattern, serial, _, _) = await GetInfoVoucherEInvoice(voucherTypeId, voucherBillId);

            var configEntity = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(x => x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice);

            if (configEntity == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceConfig.BadRequest();


            var config = _mapper.Map<ElectronicInvoiceProviderModel>(configEntity);

            var functionConfig = config.FieldsConfig.FirstOrDefault(x => x.ElectronicInvoiceFunctionId == EnumElectronicInvoiceFunction.Adjust);

            if (functionConfig == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceFunction.BadRequest();

            var uri = $"{config.EasyInvoiceConnection.HostName.TrimEnd('/')}/api/business/cancelInvoice";

            var responseData = await _httpClient.Post<EasyInvoiceResponseModel>(uri, new EasyInvoiceRequestModel()
            {
                Ikey = voucherBillCode,
                Pattern = pattern,
                Serial = serial
            }, request => EasyInvoiceAuthentication(request, nameof(HttpMethod.Post), config.EasyInvoiceConnection.UserName, config.EasyInvoiceConnection.Password), errorHandler: EasyInvoiceErrorHandler);

            if (responseData.Status != 2)
                throw ElectronicInvoiceProviderErrorCode.EInvoiceProcessFailed.BadRequest(responseData.JsonSerialize());

            await UpdateEInvoiceStatus(voucherBillCode, EnumElectronicInvoiceStatus.EInvoiceCanceled);

            return true;
        }

        public async Task<(Stream stream, string fileName, string contentType)> GetElectronicInvoicePdf(long voucherTypeId, long voucherBillId, int option)
        {
            var (voucherBillCode, pattern, serial, _, _) = await GetInfoVoucherEInvoice(voucherTypeId, voucherBillId);

            var configEntity = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(x => x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice);

            if (configEntity == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceConfig.BadRequest();

            var config = _mapper.Map<ElectronicInvoiceProviderModel>(configEntity);

            var functionConfig = config.FieldsConfig.FirstOrDefault(x => x.ElectronicInvoiceFunctionId == EnumElectronicInvoiceFunction.GetPdf);

            if (functionConfig == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceFunction.BadRequest();


            var uri = $"{config.EasyInvoiceConnection.HostName.TrimEnd('/')}/api/publish/getInvoicePdf";
            var bodyData = new EasyInvoiceRequestModel()
            {
                Pattern = pattern,
                Serial = serial,
                Option = option,
                Ikey = voucherBillCode
            };

            var streamFile = await _httpClient.Download(uri, bodyData, request => EasyInvoiceAuthentication(request, nameof(HttpMethod.Post), config.EasyInvoiceConnection.UserName, config.EasyInvoiceConnection.Password));


            return (streamFile, voucherBillCode, "application/pdf");
        }

        #region private
        private async Task UpdateInvoiceInfoForVoucherBill(long voucherBillId, InvoiceInfo invoiceData, string urlSearch)
        {
            urlSearch ??= "";

            var exSql = @$"UPDATE {VoucherConstants.VOUCHER_VALUE_ROW_TABLE} 
            SET {VoucherConstants.VOUCHER_E_INVOICE_ARISING_DATE} = @{VoucherConstants.VOUCHER_E_INVOICE_ARISING_DATE},
                {VoucherConstants.VOUCHER_E_INVOICE_ISSUE_DATE} = @{VoucherConstants.VOUCHER_E_INVOICE_ISSUE_DATE},
                {VoucherConstants.VOUCHER_E_INVOICE_LOOKUP_CODE} = @{VoucherConstants.VOUCHER_E_INVOICE_LOOKUP_CODE},
                {VoucherConstants.VOUCHER_E_INVOICE_NUMBER} = @{VoucherConstants.VOUCHER_E_INVOICE_NUMBER},
                {VoucherConstants.VOUCHER_E_INVOICE_STATUS} = @{VoucherConstants.VOUCHER_E_INVOICE_STATUS},
                {VoucherConstants.VOUCHER_E_INVOICE_URL_SEARCH} = @{VoucherConstants.VOUCHER_E_INVOICE_URL_SEARCH}
            WHERE {VoucherConstants.VOUCHER_BILL_F_Id} = @{VoucherConstants.VOUCHER_BILL_F_Id};
            ";
            var sqlParams = new SqlParameter[] {
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_ARISING_DATE}", DateTime.ParseExact(invoiceData.ArisingDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)),
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_ISSUE_DATE}", DateTime.ParseExact(invoiceData.IssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)),
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_LOOKUP_CODE}", invoiceData.LookupCode),
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_NUMBER}", invoiceData.No),
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_STATUS}", ConvertEInvoiceStatusOfProviderIntoSystem(invoiceData.InvoiceStatus)),
                new SqlParameter($"@{VoucherConstants.VOUCHER_BILL_F_Id}", voucherBillId),
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_URL_SEARCH}", string.Format(urlSearch, invoiceData.LookupCode)),
            };

            var _ = await _purchaseOrderDBContext.Database.ExecuteSqlRawAsync(exSql, sqlParams);
        }

        private async Task UpdateEInvoiceStatus(string voucherBillCode, EnumElectronicInvoiceStatus status)
        {
            var exSql = @$"UPDATE {VoucherConstants.VOUCHER_VALUE_ROW_TABLE} 
            SET {VoucherConstants.VOUCHER_E_INVOICE_STATUS} = @{VoucherConstants.VOUCHER_E_INVOICE_STATUS}
            WHERE {VoucherConstants.VOUCHER_BILL_CODE} = @{VoucherConstants.VOUCHER_BILL_CODE}
            ";
            var sqlParams = new SqlParameter[] {
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_STATUS}", status),
                new SqlParameter($"@{VoucherConstants.VOUCHER_BILL_CODE}", voucherBillCode),
            };

            var _ = await _purchaseOrderDBContext.Database.ExecuteSqlRawAsync(exSql, sqlParams);
        }

        private string GetXmlDataOfCreateEInvoice(ElectronicInvoiceMappingModel mappingFields, ElectronicInvoiceProviderFieldsConfigModel functionConfig, IEnumerable<NonCamelCaseDictionary> data)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

            var body = doc.CreateElement("Invoices");
            doc.AppendChild(body);

            var gVoucherData = data.GroupBy(x => x[PurchaseOrderConstants.BILL_CODE]);
            foreach (var voucherData in gVoucherData)
            {
                var inv = doc.CreateElement("Inv");
                var invoice = doc.CreateElement("Invoice");
                var products = doc.CreateElement("Products");

                var mapFieldInfo = mappingFields.MappingFields.Info.ToDictionary(k => k.DestinationField, v => v.SourceField);
                for (int i = 0; i < functionConfig.Info.Count(); i++)
                {
                    var field = functionConfig.Info[i];
                    ValidAndAppendChildXml(field, doc, voucherData, mapFieldInfo, invoice);
                }

                var mapFieldDetail = mappingFields.MappingFields.Details.ToDictionary(k => k.DestinationField, v => v.SourceField);
                for (int v = 0; v < voucherData.Count(); v++)
                {
                    var eData = voucherData.ElementAt(v);
                    var product = doc.CreateElement("Product");
                    for (int i = 0; i < functionConfig.Details.Count(); i++)
                    {
                        var field = functionConfig.Details[i];
                        ValidAndAppendChildXml(field, doc, voucherData, mapFieldDetail, product);
                    }

                    products.AppendChild(product);
                }

                invoice.AppendChild(products);
                inv.AppendChild(invoice);
                body.AppendChild(inv);
            }

            return doc.OuterXml;
        }

        private string GetXmlDataOfModifyEInvoice(ElectronicInvoiceMappingModel mappingFields, ElectronicInvoiceProviderFieldsConfigModel functionConfig, IEnumerable<NonCamelCaseDictionary> data)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

            var body = doc.CreateElement("AdjustInv");
            doc.AppendChild(body);

            var gVoucherData = data.GroupBy(x => x[PurchaseOrderConstants.BILL_CODE]);
            foreach (var voucherData in gVoucherData)
            {
                var products = doc.CreateElement("Products");

                var mapFieldInfo = mappingFields.MappingFields.Info.ToDictionary(k => k.DestinationField, v => v.SourceField);
                for (int i = 0; i < functionConfig.Info.Count(); i++)
                {
                    var field = functionConfig.Info[i];
                    ValidAndAppendChildXml(field, doc, voucherData, mapFieldInfo, body);
                }

                // XmlElement typeAdjust = doc.CreateElement("Type");
                // typeAdjust.AppendChild(doc.CreateTextNode("2"));
                // body.AppendChild(typeAdjust);

                var mapFieldDetail = mappingFields.MappingFields.Details.ToDictionary(k => k.DestinationField, v => v.SourceField);
                for (int v = 0; v < voucherData.Count(); v++)
                {
                    var eData = voucherData.ElementAt(v);
                    var product = doc.CreateElement("Product");
                    for (int i = 0; i < functionConfig.Details.Count(); i++)
                    {
                        var field = functionConfig.Details[i];
                        ValidAndAppendChildXml(field, doc, voucherData, mapFieldDetail, product);
                    }

                    products.AppendChild(product);
                }

                body.AppendChild(products);
            }

            return doc.OuterXml;
        }

        private string GetXmlDataOfReplaceEInvoice(ElectronicInvoiceMappingModel mappingFields, ElectronicInvoiceProviderFieldsConfigModel functionConfig, IEnumerable<NonCamelCaseDictionary> data)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

            var body = doc.CreateElement("ReplaceInv");
            doc.AppendChild(body);

            var gVoucherData = data.GroupBy(x => x[PurchaseOrderConstants.BILL_CODE]);
            foreach (var voucherData in gVoucherData)
            {
                var products = doc.CreateElement("Products");

                var mapFieldInfo = mappingFields.MappingFields.Info.ToDictionary(k => k.DestinationField, v => v.SourceField);
                for (int i = 0; i < functionConfig.Info.Count(); i++)
                {
                    var field = functionConfig.Info[i];
                    ValidAndAppendChildXml(field, doc, voucherData, mapFieldInfo, body);
                }

                var mapFieldDetail = mappingFields.MappingFields.Details.ToDictionary(k => k.DestinationField, v => v.SourceField);
                for (int v = 0; v < voucherData.Count(); v++)
                {
                    var eData = voucherData.ElementAt(v);
                    var product = doc.CreateElement("Product");
                    for (int i = 0; i < functionConfig.Details.Count(); i++)
                    {
                        var field = functionConfig.Details[i];
                        ValidAndAppendChildXml(field, doc, voucherData, mapFieldDetail, product);
                    }

                    products.AppendChild(product);
                }

                body.AppendChild(products);
            }

            return doc.OuterXml;
        }

        private void ValidAndAppendChildXml(ElectronicInvoiceFieldConfigModel field, XmlDocument doc, IGrouping<object, NonCamelCaseDictionary> voucherData, Dictionary<string, string> mapFieldDetail, XmlElement parent)
        {

            var sourceField = mapFieldDetail.ContainsKey(field.FieldName) ? mapFieldDetail[field.FieldName] : "";

            if (string.IsNullOrWhiteSpace(sourceField)) return;

            var value = !string.IsNullOrWhiteSpace(sourceField) ? voucherData.ElementAt(0)[sourceField] : "";

            if (field.IsRequired && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
                throw GeneralCode.InvalidParams.BadRequest($"Trường thông tin \"{field.Title}\" là bắt buộc");

            if (field.DataTypeId == EnumDataType.Date)
            {
                long? valueInNumber = long.Parse(value?.ToString());
                value = valueInNumber.UnixToDateTime(_currentContextService.TimeZoneOffset)?.ToString("dd/MM/yyyy");
            }
            else
            {
                value = field.DataTypeId.GetSqlValue(value);
            }

            XmlElement element = doc.CreateElement(field.FieldName);
            XmlText textValue = doc.CreateTextNode(value?.ToString());
            element.AppendChild(textValue);
            parent.AppendChild(element);
        }

        private string GenerateToken(string httpMethod, string username, string password)
        {
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = DateTime.UtcNow - epochStart;
            string timestamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();
            string nonce = Guid.NewGuid().ToString("N").ToLower();
            string signatureRawData = $"{httpMethod.ToUpper()}{timestamp}{nonce}";

            using (MD5 md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(signatureRawData));
                var signature = Convert.ToBase64String(hash);
                return $"{signature}:{nonce}:{timestamp}:{username}:{password}";
            }
        }

        private EnumElectronicInvoiceStatus ConvertEInvoiceStatusOfProviderIntoSystem(int eInvoiceStatus) => eInvoiceStatus switch
        {
            -1 => EnumElectronicInvoiceStatus.EInvoiceNotExists,
            0 => EnumElectronicInvoiceStatus.EInvoiceWithoutDigitalSignature,
            1 => EnumElectronicInvoiceStatus.EInvoiceWithDigitalSignature,
            2 => EnumElectronicInvoiceStatus.EInvoiceDeclaredTax,
            3 => EnumElectronicInvoiceStatus.EInvoiceReplaced,
            4 => EnumElectronicInvoiceStatus.EInvoiceAdjusted,
            5 => EnumElectronicInvoiceStatus.EInvoiceCanceled,
            6 => EnumElectronicInvoiceStatus.EInvoiceApproved,
            _ => throw new ArgumentOutOfRangeException(nameof(eInvoiceStatus), $"Not expected direction value: {eInvoiceStatus}"),
        };

        private ApiErrorResponse EasyInvoiceErrorHandler(string response)
        {
            var result = response.JsonDeserialize<EasyInvoiceResponseModel>();

            if (result.Data == null)
                return new ApiErrorResponse() { Message = result.Message };

            if (result.Data.KeyInvoiceMsg.Count > 0)
                return new ApiErrorResponse() { Message = result.Data.KeyInvoiceMsg.FirstOrDefault().Value };

            return null;
        }

        private void EasyInvoiceAuthentication(HttpRequestMessage httpRequest, string httpMethod, string username, string password)
        {
            httpRequest.Headers.TryAddWithoutValidation(Headers.Authentication, GenerateToken(httpMethod, username, password));
        }

        #endregion 
    }
}