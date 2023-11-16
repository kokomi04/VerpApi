using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Config
{
    public class DataConfigModel : IMapFrom<DataConfig>
    {
        public int SubsidiaryId { get; set; }
        public long ClosingDate { get; set; }
        public bool AutoClosingDate { get; set; }
        public long? WorkingFromDate { get; set; }
        public long? WorkingToDate { get; set; }
        public bool? IsIgnoreAccountant { get; set; }
        public bool? IsAutoUpdateWorkingDate { get; set; }
        public FreqClosingDate FreqClosingDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<DataConfig, DataConfigModel>()
                .ForMember(m => m.FreqClosingDate, m => m.MapFrom(v => v.FreqClosingDate.JsonDeserialize<FreqClosingDate>()))
                .ForMember(d => d.IsIgnoreAccountant, m => m.MapFrom(v => v.IsIgnoreAccountant))
                .ForMember(m => m.IsAutoUpdateWorkingDate, m => m.MapFrom(v => v.IsAutoUpdateWorkingDate))
                .ForMember(m => m.ClosingDate, m => m.MapFrom(v => v.ClosingDate.GetUnix()))
                .ForMember(m => m.WorkingFromDate, m => m.MapFrom(v => v.WorkingFromDate.GetUnix()))
                .ForMember(m => m.WorkingToDate, m => m.MapFrom(v => v.WorkingToDate.GetUnix()))
                .ReverseMapCustom()
                .ForMember(d => d.FreqClosingDate, m => m.MapFrom(v => v.FreqClosingDate.JsonSerialize()))
                .ForMember(m => m.ClosingDate, m => m.MapFrom(v => v.ClosingDate.UnixToDateTime()))
                .ForMember(m => m.WorkingFromDate, m => m.MapFrom(v => v.WorkingFromDate.UnixToDateTime()))
                .ForMember(m => m.WorkingToDate, m => m.MapFrom(v => v.WorkingToDate.UnixToDateTime()));
        }
    }
}
