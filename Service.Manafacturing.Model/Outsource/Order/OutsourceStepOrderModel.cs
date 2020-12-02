using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceStepOrderModel: OutsourceOrderModel, IMapFrom<OutsourceOrder>
    {
        public IList<OutsourceStepOrderDetailModel> outsourceOrderDetail { get; set; }

        public new void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourceStepOrderModel>()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.GetUnix()))
                .ForMember(m => m.outsourceOrderDetail, v => v.MapFrom(m => m.OutsourceOrderDetail))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.Value.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderDetail, v => v.Ignore())
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.UnixToDateTime()));
        }
    }

    public class OutsourceStepOrderSeach
    {
        public long OutsourceOrderId { get; set; }
        public string OutsourceOrderCode { get; set; }
        public long OutsourceOrderFinishDate { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public long OutsourceStepRequestId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public string ProductionStepTitle { get; set; }
    }
}
