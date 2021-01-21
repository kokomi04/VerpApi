using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderModel: OutsourceOrderBase, IMapFrom<OutsourceOrder>
    {
        public long? OutsourceOrderDate { get; set; }
        [Required]
        public long OutsourceOrderFinishDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourceOrderModel>()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.GetUnix()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.GetUnix()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonDeserialize<DeliveryDestinationModel>()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonDeserialize<SuppliersModel>()))
                .ReverseMap()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.Value.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.UnixToDateTime()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonSerialize()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonSerialize()));
        }

    }

    public class OutsourceOrderInfo: OutsourceOrderModel, IMapFrom<OutsourceOrder>
    {
        public OutsourceOrderInfo()
        {
            OutsourceOrderDetail = new List<OutsourceOrderDetailInfo>();
        }
        public IList<OutsourceOrderDetailInfo> OutsourceOrderDetail { get; set; }

        public new void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourceOrderInfo>()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.GetUnix()))
                .ForMember(m => m.OutsourceOrderDetail, v => v.Ignore())
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.GetUnix()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.GetUnix()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonDeserialize<DeliveryDestinationModel>()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonDeserialize<SuppliersModel>()))
                .ReverseMap()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.Value.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderDetail, v => v.Ignore())
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.UnixToDateTime()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonSerialize()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonSerialize()));
        }
    }

    public class OutsourceOrderBase
    {
        public long OutsourceOrderId { get; set; }
        public EnumOutsourceType OutsourceTypeId { get; set; }
        public string OutsourceOrderCode { get; set; }
        public string ProviderName { get; set; }
        public string ProviderReceiver { get; set; }
        public string ProviderAddress { get; set; }
        public string ProviderPhone { get; set; }
        public string TransportToReceiver { get; set; }
        public string TransportToCompany { get; set; }
        public string TransportToAddress { get; set; }
        public string TransportToPhone { get; set; }
        public string OutsourceRequired { get; set; }
        public string Note { get; set; }
        public decimal FreightCost { get; set; }
        public decimal OtherCost { get; set; }
        public int? CustomerId { get; set; }
        public DeliveryDestinationModel DeliveryDestination { get; set; }
        public SuppliersModel Suppliers { get; set; }
    }

    public class SuppliersModel
    {
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string LegalRepresentative { get; set; }
        public string PhoneNumber { get; set; }
        public string Fax { get; set; }
    }

    public class DeliveryDestinationModel
    {
        public string DeliverTo { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
    }


}
