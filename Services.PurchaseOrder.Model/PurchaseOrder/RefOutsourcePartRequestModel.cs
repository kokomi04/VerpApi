using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model
{
    public class RefOutsourcePartRequestModel : IMapFrom<RefOutsourcePartRequest>
    {
       public long OutsourcePartRequestId { get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public int RootProductId { get; set; }
        public int ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public long? OutsourcePartRequestDetailFinishDate { get; set; }
        public decimal QuantityProcessed { get; set; } = decimal.Zero;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RefOutsourcePartRequestModel, RefOutsourcePartRequest>()
            .ForMember(m => m.OutsourcePartRequestDetailFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestDetailFinishDate.UnixToDateTime()))
            .ReverseMap()
            .ForMember(m => m.OutsourcePartRequestDetailFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestDetailFinishDate.GetUnix()));
        }
    }
}