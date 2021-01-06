using AutoMapper;
using System;
using System.Collections.Generic;
using VErp.Commons.Enums.Stock;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using InventoryRequirementEntity = VErp.Infrastructure.EF.StockDB.InventoryRequirement;

namespace VErp.Services.Stock.Model.Inventory.InventoryRequirement
{
    public class InventoryRequirementBaseModel
    {
        public string InventoryRequirementCode { get; set; }
        public string Content { get; set; }
        public long Date { get; set; }
        public int? DepartmentId { get; set; }
        public int CreatedByUserId { get; set; }
        public long? ScheduleTurnId { get; set; }
        public long? ProductionStepId { get; set; }
    }

    public class InventoryRequirementListModel : InventoryRequirementBaseModel, IMapFrom<InventoryRequirementEntity>
    {
        public long InventoryRequirementId { get; set; }
        public int? CensorByUserId { get; set; }
        public long? CensorDatetimeUtc { get; set; }
        public EnumInventoryRequirementStatus CensorStatus { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InventoryRequirementEntity, InventoryRequirementListModel>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(source => source.Date.GetUnix()))
                .ForMember(dest => dest.CensorDatetimeUtc, opt => opt.MapFrom(source => source.CensorDatetimeUtc.GetUnix()))
                .ForMember(dest => dest.CensorStatus, opt => opt.MapFrom(source => (EnumInventoryRequirementStatus)source.CensorStatus));
        }
    }

    public class InventoryRequirementInputModel : InventoryRequirementBaseModel, IMapFrom<InventoryRequirementEntity>
    {
        public virtual ICollection<InventoryRequirementDetailInputModel> InventoryRequirementDetail { get; set; }
        public virtual ICollection<InventoryRequirementFileInputModel> InventoryRequirementFile { get; set; }

        public InventoryRequirementInputModel()
        {
            InventoryRequirementFile = new List<InventoryRequirementFileInputModel>();
            InventoryRequirementDetail = new List<InventoryRequirementDetailInputModel>();
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<InventoryRequirementInputModel, InventoryRequirementEntity>()
                .ForMember(dest => dest.InventoryRequirementDetail, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryRequirementFile, opt => opt.Ignore())
                .ForMember(dest => dest.Date, opt => opt.MapFrom(source => source.Date.UnixToDateTime()));
        }

        public OutsideImportMappingData OutsideImportMappingData { get; set; }
    }

    public class OutsideImportMappingData
    {
        public string MappingFunctionKey { get; set; }
        public string ObjectId { get; set; }
    }

    public class InventoryRequirementOutputModel : InventoryRequirementListModel
    {
        public virtual ICollection<InventoryRequirementDetailOutputModel> InventoryRequirementDetail { get; set; }
        public virtual ICollection<InventoryRequirementFileOutputModel> InventoryRequirementFile { get; set; }

        public InventoryRequirementOutputModel()
        {
            InventoryRequirementFile = new List<InventoryRequirementFileOutputModel>();
            InventoryRequirementDetail = new List<InventoryRequirementDetailOutputModel>();
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<InventoryRequirementEntity, InventoryRequirementOutputModel>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(source => source.Date.GetUnix()))
                .ForMember(dest => dest.CensorDatetimeUtc, opt => opt.MapFrom(source => source.CensorDatetimeUtc.GetUnix()))
                .ForMember(dest => dest.CensorStatus, opt => opt.MapFrom(source => (EnumInventoryRequirementStatus)source.CensorStatus));
        }
    }
}
