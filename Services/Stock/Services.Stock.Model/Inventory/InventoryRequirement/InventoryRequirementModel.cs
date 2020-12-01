using AutoMapper;
using System;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using InventoryRequirementEntity = VErp.Infrastructure.EF.StockDB.InventoryRequirement;

namespace VErp.Services.Stock.Model.Inventory.InventoryRequirement
{
    public class InventoryRequirementModel : IMapFrom<InventoryRequirementEntity>
    {
        public long InventoryRequirementId { get; set; }
        public string InventoryRequirementCode { get; set; }
        public string Content { get; set; }
        public long Date { get; set; }
        public int? DepartmentId { get; set; }

        public long? ProductionHandoverId { get; set; }
        public virtual ICollection<InventoryRequirementDetailModel> InventoryRequirementDetail { get; set; }
        public virtual ICollection<InventoryRequirementFileModel> InventoryRequirementFile { get; set; }

        public InventoryRequirementModel()
        {
            InventoryRequirementFile = new List<InventoryRequirementFileModel>();
            InventoryRequirementDetail = new List<InventoryRequirementDetailModel>();
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<InventoryRequirementEntity, InventoryRequirementModel>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(source => source.Date.GetUnix()))
                .ReverseMap()
                .ForMember(dest => dest.InventoryRequirementId, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryRequirementDetail, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryRequirementFile, opt => opt.Ignore())
                .ForMember(dest => dest.Date, opt => opt.MapFrom(source => source.Date.UnixToDateTime()));
        }
    }
}
