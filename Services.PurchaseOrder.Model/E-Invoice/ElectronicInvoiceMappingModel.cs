using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.E_Invoice
{
    public class ElectronicInvoiceMappingModel : IMapFrom<ElectronicInvoiceMapping>
    {
        public int ElectronicInvoiceMappingId { get; set; }
        public int ElectronicInvoiceProviderId { get; set; }
        public int ElectronicInvoiceFunctionId { get; set; }
        public int VoucherTypeId { get; set; }
        public ElectronicInvoiceMappingFieldsModel MappingFields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ElectronicInvoiceMappingModel, ElectronicInvoiceMapping>()
              .ForMember(d => d.MappingFields, s => s.MapFrom(m => m.MappingFields.JsonSerialize()))
              .ReverseMapCustom()
              .ForMember(d => d.MappingFields, s => s.MapFrom(m => m.MappingFields.JsonDeserialize<ElectronicInvoiceMappingFieldsModel>()));
        }
    }

    public class ElectronicInvoiceMappingFieldsModel
    {
        public IList<ElectronicInvoiceMappingFieldModel> Info { get; set; }
        public IList<ElectronicInvoiceMappingFieldModel> Details { get; set; }

    }

    public class ElectronicInvoiceMappingFieldModel
    {
        public string SourceField { get; set; }
        public string DestinationField { get; set; }
    }
}