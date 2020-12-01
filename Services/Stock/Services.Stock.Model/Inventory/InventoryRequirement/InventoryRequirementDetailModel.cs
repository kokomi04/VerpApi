using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Inventory.InventoryRequirement
{
    public class InventoryRequirementDetailModel : IMapFrom<InventoryRequirementDetail>
    {
        public long InventoryRequirementDetailId { get; set; }
        public int SubsidiaryId { get; set; }
        public long InventoryRequirementId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public string Pocode { get; set; }
        public string ProductionOrderCode { get; set; }
        public int? SortOrder { get; set; }

        public InventoryRequirementDetailModel()
        {
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<InventoryRequirementDetail, InventoryRequirementDetailModel>()
                .ReverseMap()
                .ForMember(dest => dest.InventoryRequirementDetailId, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryRequirementId, opt => opt.Ignore());
        }
    }
}
