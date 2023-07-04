using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.PurchaseOrder;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class OutsourcePartRequestDetailModel : IMapFrom<OutsourcePartRequestDetail>
    {
        public long OutsourcePartRequestDetailId { get; set; }
        public long OutsourcePartRequestId { get; set; }
        public int ProductId { get; set; }
        public string PathProductIdInBom { get; set; }
        public decimal Quantity { get; set; }
        public long? OutsourcePartRequestDetailFinishDate { get; set; }

        public decimal QuantityProcessed { get; set; }
        public List<PurchaseOrderSimple> PurchaseOrder { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<OutsourcePartRequestDetailModel, OutsourcePartRequestDetail>()
            .ForMember(m => m.OutsourcePartRequestDetailFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestDetailFinishDate.UnixToDateTime()))
            .ReverseMapCustom()
            .ForMember(m => m.OutsourcePartRequestDetailFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestDetailFinishDate.GetUnix()));
        }
    }
}
