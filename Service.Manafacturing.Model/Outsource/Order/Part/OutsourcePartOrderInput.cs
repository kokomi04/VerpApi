using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order.Part
{
    public class OutsourcePartOrderInput : OutsourceOrderBase, IMapFrom<OutsourceOrder>
    {
        public long? OutsourceOrderDate { get; set; }
        public long OutsourceOrderFinishDate { get; set; }

        public IList<OutsourcePartOrderDetailInput> OutsourceOrderDetails { get; set; }
        public IList<OutsourceOrderMaterialsModel> OutsourceOrderMaterials { get; set; }
        public IList<OutsourceOrderExcessModel> OutsourceOrderExcesses { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourcePartOrderInput>()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.GetUnix()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.GetUnix()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonDeserialize<DeliveryDestinationModel>()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonDeserialize<SuppliersModel>()))
                .ForMember(m => m.AttachmentFileId, v => v.MapFrom(m => m.AttachmentFileId))
                .ReverseMap()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.Value.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.UnixToDateTime()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonSerialize()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonSerialize()))
                .ForMember(m => m.AttachmentFileId, v => v.MapFrom(m => m.AttachmentFileId));
        }
    }
}
