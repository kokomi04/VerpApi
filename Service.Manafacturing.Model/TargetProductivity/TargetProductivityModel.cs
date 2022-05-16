using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model
{
    public class TargetProductivityModel : IMapFrom<TargetProductivity>
    {
        public int TargetProductivityId { get; set; }
        public string TargetProductivityCode { get; set; }
        public long TargetProductivityDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; } = false;
        public string Note { get; set; } = string.Empty;

        public IList<TargetProductivityDetailModel> TargetProductivityDetail { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<TargetProductivity, TargetProductivityModel>()
            .ForMember(m => m.TargetProductivityDate, v => v.MapFrom(m => m.TargetProductivityDate.GetUnix()))
            .ForMember(m => m.TargetProductivityDetail, v => v.MapFrom(m => m.TargetProductivityDetail))
            .ReverseMap()
            .ForMember(m => m.TargetProductivityDetail, v => v.Ignore());
        }
    }

    [Display(Name = "Chi tiết năng suất mục tiêu")]
    public class TargetProductivityDetailModel : MappingDataRowAbstract, IMapFrom<TargetProductivityDetail>
    {
        [FieldDataIgnore]
        public int TargetProductivityDetailId { get; set; }
        [FieldDataIgnore]
        public int TargetProductivityId { get; set; }
        [Display(Name = "Công đoạn")]
        public int ProductionStepId { get; set; }
        [Display(Name = "Năng suất mục tiêu")]
        public decimal TargetProductivity { get; set; }        
    }
}
