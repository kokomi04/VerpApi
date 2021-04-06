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
        public IList<OutsourceStepOrderDetailModel> OutsourceOrderDetail { get; set; }
        public IList<OutsourceOrderMaterialsModel> OutsourceOrderMaterials { get; set; }

        public new void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourceStepOrderModel>()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.GetUnix()))
                .ForMember(m => m.OutsourceOrderDetail, v => v.MapFrom(m => m.OutsourceOrderDetail))
                .ForMember(m => m.OutsourceOrderMaterials, v => v.MapFrom(m => m.OutsourceOrderMaterials))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.GetUnix()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonDeserialize<DeliveryDestinationModel>()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonDeserialize<SuppliersModel>()))
                .ReverseMap()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.Value.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderDetail, v => v.Ignore())
                .ForMember(m => m.OutsourceOrderMaterials, v => v.Ignore())
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.UnixToDateTime()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonSerialize()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonSerialize()));
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
