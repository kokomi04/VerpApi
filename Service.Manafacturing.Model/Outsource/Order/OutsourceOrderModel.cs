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
    public class OutsourceOrderModel: IMapFrom<OutsourceOrder>
    {
        public long OutsourceOrderId { get; set; }
        public EnumOutsourceOrderType OutsourceTypeId { get; set; }
        public string OutsourceOrderCode { get; set; }
        public string ProviderName { get; set; }
        public string ProviderReceiver { get; set; }
        public string ProviderAddress { get; set; }
        public string ProviderPhone { get; set; }
        public string TransportToReceiver { get; set; }
        public string TransportToCompany { get; set; }
        public string TransportToAddress { get; set; }
        public string TransportToPhone { get; set; }
        public string OutsoureRequired { get; set; }
        public string Note { get; set; }
        public decimal FreightCost { get; set; }
        public decimal OtherCost { get; set; }
        public long? OutsourceOrderDate { get; set; }
        [Required]
        public long OutsourceOrderFinishDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourceOrderModel>()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.GetUnix()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.Value.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.UnixToDateTime()));
        }

    }

    public class OutsourceOrderInfo: OutsourceOrderModel, IMapFrom<OutsourceOrder>
    {
        public IList<OutsourceOrderDetailInfo> OutsourceOrderDetail { get; set; }

        public new void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourceOrderInfo>()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.GetUnix()))
                .ForMember(m => m.OutsourceOrderDetail, v => v.MapFrom(m => m.OutsourceOrderDetail))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.Value.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderDetail, v => v.Ignore())
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.UnixToDateTime()));
        }
    }


}
