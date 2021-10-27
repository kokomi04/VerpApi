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
using Newtonsoft.Json;

namespace VErp.Services.PurchaseOrder.Service.E_Invoice.Implement
{
    public interface IEasyInvoiceProviderService
    {
        Task<CreateElectronicInvoiceSuccess> CreateElectronicInvoice(string pattern, string serial, long voucherTypeId, long voucherBillId, IEnumerable<NonCamelCaseDictionary> data);
        Task<(Stream stream, string fileName, string contentType)> GetElectronicInvoicePdf(string ikey, string pattern, int option);
        Task<ModifyElectronicInvoiceSuccess> ModifyElectronicInvoice(string ikey, string pattern, string serial, long voucherTypeId, IEnumerable<NonCamelCaseDictionary> data);
        Task<PublishElectronicInvoiceSuccess> PublishElectronicInvoice(IList<string> ikeys, string pattern, string serial, string signature);
        Task<PublishElectronicInvoiceSuccess> PublishTempElectronicInvoice(IList<string> ikeys, string pattern, string serial, string certString);
    }

    public class EasyInvoiceProviderService : IEasyInvoiceProviderService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _objectActivityLog;
        private readonly IHttpClientFactoryService _httpClient;

        

        public EasyInvoiceProviderService(IHttpClientFactoryService httpClient, PurchaseOrderDBContext purchaseOrderDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _mapper = mapper;
            _objectActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.EasyInvoiceProvider);
            _httpClient = httpClient;
        }

        public async Task<CreateElectronicInvoiceSuccess> CreateElectronicInvoice(string pattern, string serial, long voucherTypeId, long voucherBillId, IEnumerable<NonCamelCaseDictionary> data)
        {
            var configEntity = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(x => x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice);
            var mappingEntity = await _purchaseOrderDBContext.ElectronicInvoiceMapping.FirstOrDefaultAsync(x =>
                x.ElectronicInvoiceFunctionId == (int)EnumElectronicInvoiceFunction.Create &&
                x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice &&
                x.VoucherTypeId == voucherTypeId
            );

            if (configEntity == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceConfig.BadRequest();

            if (mappingEntity == null)
                throw ElectronicInvoiceMappingErrorCode.NotFoundElectronicInvoiceMapping.BadRequest();

            var config = _mapper.Map<ElectronicInvoiceProviderModel>(configEntity);
            var mappingFields = _mapper.Map<ElectronicInvoiceMappingModel>(mappingEntity);

            var functionConfig = config.FieldsConfig.FirstOrDefault(x => x.ElectronicInvoiceFunctionId == EnumElectronicInvoiceFunction.Create);

            if (functionConfig == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceFunction.BadRequest();


            var uri = $"{config.EasyInvoiceConnection.HostName.TrimEnd('/')}/api/publish/importInvoice";
            string xmlData = GetXmlDataOfCreateEInvoice(mappingFields, functionConfig, data);

            var objectData = await _httpClient.Post<ElectronicInvoiceResponseModel<CreateElectronicInvoiceSuccess>>(uri, new ElectronicInvoiceRequestModel(xmlData, pattern, serial), request =>
            {
                request.Headers.TryAddWithoutValidation(Headers.Authentication, GenerateToken(nameof(HttpMethod.Post), config.EasyInvoiceConnection.UserName, config.EasyInvoiceConnection.Password));
            }, new Newtonsoft.Json.JsonSerializerSettings {
                DateFormatString = "dd/MM/yyyy"
            });

            if (objectData.Status != 2)
                throw ElectronicInvoiceProviderErrorCode.EInvoiceProcessFailed.BadRequest(objectData.JsonSerialize());

            var invoiceData = objectData.Data.Invoices.FirstOrDefault();

            var exSql = @$"UPDATE {VoucherConstants.VOUCHER_VALUE_ROW_TABLE} 
            SET {VoucherConstants.VOUCHER_E_INVOICE_ARISING_DATE} = @{VoucherConstants.VOUCHER_E_INVOICE_ARISING_DATE},
                {VoucherConstants.VOUCHER_E_INVOICE_ISSUE_DATE} = @{VoucherConstants.VOUCHER_E_INVOICE_ISSUE_DATE},
                {VoucherConstants.VOUCHER_E_INVOICE_LOOKUP_CODE} = @{VoucherConstants.VOUCHER_E_INVOICE_LOOKUP_CODE},
                {VoucherConstants.VOUCHER_E_INVOICE_NUMBER} = @{VoucherConstants.VOUCHER_E_INVOICE_NUMBER},
                {VoucherConstants.VOUCHER_E_INVOICE_STATUS} = @{VoucherConstants.VOUCHER_E_INVOICE_STATUS}
            WHERE {VoucherConstants.VOUCHER_BILL_F_Id} = @{VoucherConstants.VOUCHER_BILL_F_Id}
            ";
            var sqlParams = new SqlParameter[] {
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_ARISING_DATE}", invoiceData.ArisingDate),
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_ISSUE_DATE}", invoiceData.IssueDate),
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_LOOKUP_CODE}", invoiceData.LookupCode),
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_NUMBER}", invoiceData.No),
                new SqlParameter($"@{VoucherConstants.VOUCHER_E_INVOICE_STATUS}", ConvertEInvoiceStatusOfProviderIntoSystem(invoiceData.InvoiceStatus)),
                new SqlParameter($"@{VoucherConstants.VOUCHER_BILL_F_Id}", voucherBillId),
            };

            var _ = await _purchaseOrderDBContext.Database.ExecuteSqlRawAsync(exSql, sqlParams);

            return objectData.Data;
        }

        public async Task<ModifyElectronicInvoiceSuccess> ModifyElectronicInvoice(string ikey, string pattern, string serial, long voucherTypeId, IEnumerable<NonCamelCaseDictionary> data)
        {
            var configEntity = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(x => x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice);
            var mappingEntity = await _purchaseOrderDBContext.ElectronicInvoiceMapping.FirstOrDefaultAsync(x =>
                x.ElectronicInvoiceFunctionId == (int)EnumElectronicInvoiceFunction.Modify &&
                x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice &&
                x.VoucherTypeId == voucherTypeId
            );

            if (configEntity == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceConfig.BadRequest();

            if (mappingEntity == null)
                throw ElectronicInvoiceMappingErrorCode.NotFoundElectronicInvoiceMapping.BadRequest();

            var config = _mapper.Map<ElectronicInvoiceProviderModel>(configEntity);
            var mappingFields = _mapper.Map<ElectronicInvoiceMappingModel>(mappingEntity);

            var functionConfig = config.FieldsConfig.FirstOrDefault(x => x.ElectronicInvoiceFunctionId == EnumElectronicInvoiceFunction.Modify);

            if (functionConfig == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceFunction.BadRequest();


            var uri = $"{config.EasyInvoiceConnection.HostName.TrimEnd('/')}/api/business/adjustInvoice";
            string xmlData = GetXmlDataOfModifyEInvoice(mappingFields, functionConfig, data);
            var bodyData = new ElectronicInvoiceRequestModel(xmlData, pattern, serial);
            bodyData.Ikey = ikey;

            var objectData = await _httpClient.Post<ElectronicInvoiceResponseModel<ModifyElectronicInvoiceSuccess>>(uri, bodyData, request =>
            {
                request.Headers.TryAddWithoutValidation(Headers.Authentication, GenerateToken(nameof(HttpMethod.Post), config.EasyInvoiceConnection.UserName, config.EasyInvoiceConnection.Password));
            }, new JsonSerializerSettings{
                DateFormatString = "dd/MM/yyyy"
            });

            if (objectData.Status != 2)
                throw ElectronicInvoiceProviderErrorCode.EInvoiceProcessFailed.BadRequest(objectData.JsonSerialize());

            return objectData.Data;
        }

        // public async Task<PublishElectronicInvoiceSuccess> PublishTempElectronicInvoice(IList<string> ikeys, string pattern, string serial, string certString)
        // {
        //     var configEntity = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(x => x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice);
        //     if (configEntity == null)
        //         throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceConfig.BadRequest();


        //     var config = _mapper.Map<ElectronicInvoiceProviderModel>(configEntity);

        //     var functionConfig = config.FieldsConfig.FirstOrDefault(x => x.ElectronicInvoiceFunctionId == EnumElectronicInvoiceFunction.PublishTemp);

        //     if (functionConfig == null)
        //         throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceFunction.BadRequest();


        //     var uri = $"{config.EasyInvoiceConnection.HostName.TrimEnd('/')}/{functionConfig.Uri}";
        //     var bodyData = new ElectronicInvoiceRequestModel(null, pattern, serial);
        //     bodyData.Ikeys = ikeys;
        //     bodyData.CertString = certString;

        //     var objectData = await _httpClient.Post<ElectronicInvoiceResponseModel<PublishElectronicInvoiceSuccess>>(uri, bodyData, request =>
        //     {
        //         request.Headers.TryAddWithoutValidation(Headers.Authentication, GenerateToken(nameof(HttpMethod.Post), config.EasyInvoiceConnection.UserName, config.EasyInvoiceConnection.Password));
        //     });

        //     if (objectData.Status != 2)
        //         throw ElectronicInvoiceProviderErrorCode.EInvoiceProcessFailed.BadRequest(objectData.JsonSerialize());

        //     return objectData.Data;
        // }

        // public async Task<PublishElectronicInvoiceSuccess> PublishElectronicInvoice(IList<string> ikeys, string pattern, string serial, string signature)
        // {
        //     var configEntity = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(x => x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice);

        //     if (configEntity == null)
        //         throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceConfig.BadRequest();

        //     var config = _mapper.Map<ElectronicInvoiceProviderModel>(configEntity);

        //     var functionConfig = config.FieldsConfig.FirstOrDefault(x => x.ElectronicInvoiceFunctionId == EnumElectronicInvoiceFunction.Publish);

        //     if (functionConfig == null)
        //         throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceFunction.BadRequest();


        //     var uri = $"{config.EasyInvoiceConnection.HostName.TrimEnd('/')}/api/publish/issueInvoices";
        //     var bodyData = new ElectronicInvoiceRequestModel(null, pattern, serial);
        //     bodyData.Ikeys = ikeys;
        //     bodyData.Signature = signature;

        //     var objectData = await _httpClient.Post<ElectronicInvoiceResponseModel>(uri, bodyData, request =>
        //     {
        //         request.Headers.TryAddWithoutValidation(Headers.Authentication, GenerateToken(nameof(HttpMethod.Post), config.EasyInvoiceConnection.UserName, config.EasyInvoiceConnection.Password));
        //     });

        //     if (objectData.Status != 2)
        //         throw ElectronicInvoiceProviderErrorCode.EInvoiceProcessFailed.BadRequest(objectData.JsonSerialize());

        //     return objectData.Data.JsonDeserialize<PublishElectronicInvoiceSuccess>();
        // }

        public async Task<(Stream stream, string fileName, string contentType)> GetElectronicInvoicePdf(string ikey, string pattern, int option)
        {
            var configEntity = await _purchaseOrderDBContext.ElectronicInvoiceProvider.FirstOrDefaultAsync(x => x.ElectronicInvoiceProviderId == (int)EnumElectronicInvoiceProvider.EasyInvoice);

            if (configEntity == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceConfig.BadRequest();

            var config = _mapper.Map<ElectronicInvoiceProviderModel>(configEntity);

            var functionConfig = config.FieldsConfig.FirstOrDefault(x => x.ElectronicInvoiceFunctionId == EnumElectronicInvoiceFunction.GetPdf);

            if (functionConfig == null)
                throw ElectronicInvoiceConfigErrorCode.NotFoundElectronicInvoiceFunction.BadRequest();


            var uri = $"{config.EasyInvoiceConnection.HostName.TrimEnd('/')}/api/publish/getInvoicePdf";
            var bodyData = new ElectronicInvoiceRequestModel(null, pattern, null);
            bodyData.Option = option;
            bodyData.Ikey = ikey;

            var objectData = await _httpClient.Download(uri, bodyData, request =>
            {
                request.Headers.TryAddWithoutValidation(Headers.Authentication, GenerateToken(nameof(HttpMethod.Post), config.EasyInvoiceConnection.UserName, config.EasyInvoiceConnection.Password));
            });


            return (objectData, ikey, "application/pdf");
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

            // if (!mapFieldDetail.ContainsKey(field.FieldName))
            //     throw GeneralCode.InvalidParams.BadRequest();
            
            
            var sourceField = mapFieldDetail.ContainsKey(field.FieldName) ? mapFieldDetail[field.FieldName] : "";

            var value = !string.IsNullOrWhiteSpace(sourceField) ? voucherData.ElementAt(0)[sourceField] : "";

            if (field.IsRequired && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
                throw GeneralCode.InvalidParams.BadRequest();

            if (field.DataTypeId == EnumDataType.Date)
            {
                long valueInNumber = long.Parse(value.ToString());
                value = valueInNumber.UnixToDateTime()?.ToString("dd/MM/yyyy");
            }

            XmlElement element = doc.CreateElement(field.FieldName);
            XmlText textValue = doc.CreateTextNode(value.ToString());
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

        private EnumElectronicInvoiceStatus ConvertEInvoiceStatusOfProviderIntoSystem(int eInvoiceStatus) => eInvoiceStatus switch {
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

        public Task<PublishElectronicInvoiceSuccess> PublishElectronicInvoice(IList<string> ikeys, string pattern, string serial, string signature)
        {
            throw new NotImplementedException();
        }

        public Task<PublishElectronicInvoiceSuccess> PublishTempElectronicInvoice(IList<string> ikeys, string pattern, string serial, string certString)
        {
            throw new NotImplementedException();
        }
    }
}