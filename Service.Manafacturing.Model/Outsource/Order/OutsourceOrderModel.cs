using AutoMapper;
using System;
using System.Collections.Generic;
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
        public OutsourceOrderType OutsourceTypeId { get; set; }
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
        public decimal FreigthCost { get; set; }
        public decimal OtherCost { get; set; }
        public long? CreateDateOrder { get; set; }
        public long DateRequiredComplete { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourceOrderModel>()
                .ForMember(m => m.CreateDateOrder, v => v.MapFrom(m => m.CreateDateOrder.GetUnix()))
                .ForMember(m => m.DateRequiredComplete, v => v.MapFrom(m => m.DateRequiredComplete.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.CreateDateOrder, v => v.MapFrom(m => m.CreateDateOrder.Value.UnixToDateTime()))
                .ForMember(m => m.DateRequiredComplete, v => v.MapFrom(m => m.DateRequiredComplete.UnixToDateTime()));
        }

    }

    public class OutsourceOrderInfo: OutsourceOrderModel, IMapFrom<OutsourceOrder>
    {
        public IList<OutsourceOrderDetailModel> OutsourceOrderDetail { get; set; }

        public new void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourceOrderInfo>()
                .ForMember(m => m.CreateDateOrder, v => v.MapFrom(m => m.CreateDateOrder.GetUnix()))
                .ForMember(m => m.OutsourceOrderDetail, v => v.MapFrom(m => m.OutsourceOrderDetail))
                .ForMember(m => m.DateRequiredComplete, v => v.MapFrom(m => m.DateRequiredComplete.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.CreateDateOrder, v => v.MapFrom(m => m.CreateDateOrder.Value.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderDetail, v => v.Ignore())
                .ForMember(m => m.DateRequiredComplete, v => v.MapFrom(m => m.DateRequiredComplete.UnixToDateTime()));
        }
    }


}
