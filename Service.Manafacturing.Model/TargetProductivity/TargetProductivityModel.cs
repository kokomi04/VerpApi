using System.Collections.Generic;
using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model
{
    public class TargetProductivityModel : IMapFrom<TargetProductivity>
    {
        public int TargetProductivityId { get; set; }
        public string TargetProductivityCode { get; set; }
        public long TargetProductivityDate { get; set; }
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

    public class TargetProductivityDetailModel: IMapFrom<TargetProductivityDetail>
    {
        public int TargetProductivityDetailId { get; set; }
        public int TargetProductivityId { get; set; }
        public decimal TargetProductivity { get; set; }
        public int ProductionStepId { get; set; }
    }
}
