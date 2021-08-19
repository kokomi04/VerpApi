using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model
{
    public class RefOutsourceStepRequestModel : IMapFrom<RefOutsourceStepRequest>
    {
        public long OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionStepId { get; set; }
        public int? StepId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsImportant { get; set; }
        public int ProductionStepLinkDataRoleTypeId { get; set; }
        public long OutsourceStepRequestFinishDate { get; set; }
        public decimal QuantityProcessed { get; set; } = decimal.Zero;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RefOutsourceStepRequestModel, RefOutsourceStepRequest>()
            .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.UnixToDateTime()))
            .ReverseMap()
            .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.GetUnix()));
        }
    }
}