﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using VErp.Commons.Enums.E_Invoice;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.E_Invoice
{
    public class ElectronicInvoiceProviderModel : IMapFrom<ElectronicInvoiceProvider>
    {
        public EnumElectronicInvoiceProvider ElectronicInvoiceProviderId { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string UrlSearch { get; set; }
        public EnumElectronicInvoiceProviderStatus ElectronicInvoiceProviderStatusId { get; set; }
        public EasyInvoiceConnectionConfigModel EasyInvoiceConnection { get; set; }
        public CyberBillConnectionConfiModel CyberBillConnection { get; set; }
        public IList<ElectronicInvoiceProviderFieldsConfigModel> FieldsConfig { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ElectronicInvoiceProviderModel, ElectronicInvoiceProvider>()
               .ForMember(d => d.ElectronicInvoiceProviderId, s => s.MapFrom(m => (int)m.ElectronicInvoiceProviderId))
               .ForMember(d => d.ElectronicInvoiceProviderStatusId, s => s.MapFrom(m => (int)m.ElectronicInvoiceProviderStatusId))
               .ForMember(d => d.ConnectionConfig, s => s.MapFrom(m => SerializeConnection(m)))
               .ForMember(d => d.FieldsConfig, s => s.MapFrom(m => m.FieldsConfig.JsonSerialize()))
               .ReverseMapCustom()
               .ForMember(d => d.ElectronicInvoiceProviderId, s => s.MapFrom(m => (EnumElectronicInvoiceProvider)m.ElectronicInvoiceProviderId))
               .ForMember(d => d.ElectronicInvoiceProviderStatusId, s => s.MapFrom(m => (EnumElectronicInvoiceProviderStatus)m.ElectronicInvoiceProviderStatusId))
               .ForMember(d => d.EasyInvoiceConnection, s => s.MapFrom(m => DeserializeConnection(m, EnumElectronicInvoiceProvider.EasyInvoice)))
               .ForMember(d => d.CyberBillConnection, s => s.MapFrom(m => DeserializeConnection(m, EnumElectronicInvoiceProvider.CyberBill)))
               .ForMember(d => d.FieldsConfig, s => s.MapFrom(m => DeserializeFieldsConfig(m)));
        }

        private static string SerializeConnection(ElectronicInvoiceProviderModel model)
        {
            switch (model.ElectronicInvoiceProviderId)
            {
                case EnumElectronicInvoiceProvider.EasyInvoice:
                    return model.EasyInvoiceConnection.JsonSerialize();
                case EnumElectronicInvoiceProvider.CyberBill:
                    return model.CyberBillConnection.JsonSerialize();
                default:
                    throw new NotSupportedException();
            }
        }

        public object DeserializeConnection(ElectronicInvoiceProvider entity, EnumElectronicInvoiceProvider electronicInvoiceProviderId)
        {
            if (entity.ElectronicInvoiceProviderId != (int)electronicInvoiceProviderId) return null;

            switch (electronicInvoiceProviderId)
            {
                case EnumElectronicInvoiceProvider.EasyInvoice:
                    if (string.IsNullOrWhiteSpace(entity.ConnectionConfig)) return new EasyInvoiceConnectionConfigModel();
                    return entity.ConnectionConfig.JsonDeserialize<EasyInvoiceConnectionConfigModel>();

                case EnumElectronicInvoiceProvider.CyberBill:
                    if (string.IsNullOrWhiteSpace(entity.ConnectionConfig)) return new CyberBillConnectionConfiModel();
                    return entity.ConnectionConfig.JsonDeserialize<CyberBillConnectionConfiModel>();
                default:
                    throw new NotSupportedException();
            }
        }

        public IList<ElectronicInvoiceProviderFieldsConfigModel> DeserializeFieldsConfig(ElectronicInvoiceProvider entity)
        {
            IList<ElectronicInvoiceProviderFieldsConfigModel> functions = new List<ElectronicInvoiceProviderFieldsConfigModel>();
            if (!string.IsNullOrWhiteSpace(entity.FieldsConfig))
            {
                functions = entity.FieldsConfig.JsonDeserialize<IList<ElectronicInvoiceProviderFieldsConfigModel>>();
            }

            var members = EnumExtensions.GetEnumMembers<EnumElectronicInvoiceFunction>().ToList();
            foreach (var m in members)
            {
                if (!functions.Any(f => f.ElectronicInvoiceFunctionId == m.Enum))
                {
                    functions.Add(new ElectronicInvoiceProviderFieldsConfigModel()
                    {
                        ElectronicInvoiceFunctionId = m.Enum,
                        //Uri = string.Empty,
                        Info = new List<ElectronicInvoiceFieldConfigModel>(),
                        Details = new List<ElectronicInvoiceFieldConfigModel>()
                    });
                }
            }
            foreach (var f in functions)
            {
                if (f.Info == null)
                {
                    f.Info = new List<ElectronicInvoiceFieldConfigModel>();
                }

                if (f.Details == null)
                {
                    f.Details = new List<ElectronicInvoiceFieldConfigModel>();
                }
            }

            return functions;
        }
    }


    public class EasyInvoiceConnectionConfigModel
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class CyberBillConnectionConfiModel
    {
        public string TaxIdNumber { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class ElectronicInvoiceProviderFieldsConfigModel
    {
        public EnumElectronicInvoiceFunction ElectronicInvoiceFunctionId { get; set; }
        //public string Uri { get; set; }
        public IList<ElectronicInvoiceFieldConfigModel> Info { get; set; }
        public IList<ElectronicInvoiceFieldConfigModel> Details { get; set; }

    }

    public class ElectronicInvoiceFieldConfigModel
    {
        public int SortOrder { get; set; }
        public string FieldName { get; set; }
        public string Title { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public int DecimalPlace { get; set; }
        public bool IsRequired { get; set; }
        public string Description { get; set; }
    }
}