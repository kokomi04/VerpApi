using AutoMapper;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model
{
    public class purchaseOrderTrackedModel : IMapFrom<PurchaseOrderTracked>
    {
        public long PurchaseOrderTrackedId { get; set; }
        public long PurchaseOrderId { get; set; }
        public long Date { get; set; }
        public long? ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public string Description { get; set; }
        public EnumPurchaseOrderTrackStatus Status { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<purchaseOrderTrackedModel, PurchaseOrderTracked>()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.UnixToDateTime()))
            .ReverseMap()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.GetUnix()));
        }
    }
}