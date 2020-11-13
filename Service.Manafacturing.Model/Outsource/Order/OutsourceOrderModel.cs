using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderModel: IMapFrom<OutsourceOrder>
    {
        public int OutsoureOrderId { get; set; }
        public int RequestObjectId { get; set; }
        public EnumProductionProcess.OutsourceOrderRequestObjectType RequestObjectTypeId { get; set; }
        public string OutsoureOrderCode { get; set; }
        public string ProviderName { get; set; }
        public string ProviderReceiver { get; set; }
        public string ProviderAddress { get; set; }
        public string ProviderPhone { get; set; }
        public string TransportToReceiver { get; set; }
        public string TransportToCompany { get; set; }
        public string TransportToAdress { get; set; }
        public string TransportToPhone { get; set; }
        public string OutsoureRequired { get; set; }
        public string Note { get; set; }
        public decimal FreigthCost { get; set; }
        public decimal OtherCost { get; set; }
        public long? CreateDateOrder { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourceOrderModel>()
                .ForMember(m => m.CreateDateOrder, v => v.MapFrom(m => m.CreateDateOrder.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.CreateDateOrder, v => v.MapFrom(m => m.CreateDateOrder.Value.UnixToDateTime()));
        }

    }

    public class OutsourceOrderInfo: OutsourceOrderModel, IMapFrom<OutsourceOrder>
    {
        public IList<OutsourceOrderDetailModel> OutsourceOrderDetails { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourceOrderInfo>()
                .ForMember(m => m.CreateDateOrder, v => v.MapFrom(m => m.CreateDateOrder.GetUnix()))
                .ForMember(m => m.OutsourceOrderDetails, v => v.MapFrom(m => m.OutsourceOrderDetail))
                .ReverseMap()
                .ForMember(m => m.CreateDateOrder, v => v.MapFrom(m => m.CreateDateOrder.Value.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderDetail, v => v.Ignore());
        }
    }


}
