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
        public int RequestContainerId { get; set; }
        public EnumProductionProcess.OutsourceOrderRequestContainerType RequestContainerTypeId { get; set; }
        public string OutsoureOrderCode { get; set; }
        public string ProviderName { get; set; }
        public string ProviderReceiver { get; set; }
        public string ProviderAddress { get; set; }
        public string ProviderPhone { get; set; }
        public string TransportToReceiver { get; set; }
        public string TransportToCompany { get; set; }
        public string TransportToAdress { get; set; }
        public string TransportToPhone { get; set; }
        public string RequestInfo { get; set; }
        public string Note { get; set; }

    }

    public class OutsoureOrderInfo: OutsourceOrderModel, IMapFrom<OutsourceOrder>
    {
        public long CreateDateOutsourceOrder { get; set; }
        public IList<OutsourceOrderDetail> OutsourceOrderDetail { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsoureOrderInfo>()
                .ForMember(m => m.OutsourceOrderDetail, v => v.MapFrom(m => m.OutsourceOrderDetail))
                .ForMember(m => m.CreateDateOutsourceOrder, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.OutsourceOrderDetail, v => v.Ignore())
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore());
        }
    }
}
