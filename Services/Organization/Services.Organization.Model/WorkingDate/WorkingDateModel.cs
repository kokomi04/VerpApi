using AutoMapper;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using ModelWorkingDate = VErp.Infrastructure.EF.OrganizationDB.WorkingDate;


namespace VErp.Services.Organization.Model.WorkingDate
{
    public class WorkingDateModel : IMapFrom<ModelWorkingDate>
    {
        public int WorkingDateId { get; set; }

        public int UserId { get; set; }

        public int SubsidiaryId { get; set; }

        public bool? IsIgnoreAccountant { get; set; }

        public bool? IsAutoUpdateWorkingDate { get; set; }

        public long? WorkingFromDate { get; set; }

        public long? WorkingToDate { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ModelWorkingDate, WorkingDateModel>()
                .ForMember(d => d.IsIgnoreAccountant, m => m.MapFrom(v => v.IsIgnoreAccountant))
                .ForMember(m => m.IsAutoUpdateWorkingDate, m => m.MapFrom(v => v.IsAutoUpdateWorkingDate))
                .ForMember(m => m.WorkingFromDate, m => m.MapFrom(v => v.WorkingFromDate.GetUnix()))
                .ForMember(m => m.WorkingToDate, m => m.MapFrom(v => v.WorkingToDate.GetUnix()))
                .ReverseMapCustom()
                .ForMember(m => m.WorkingFromDate, m => m.MapFrom(v => v.WorkingFromDate.UnixToDateTime()))
                .ForMember(m => m.WorkingToDate, m => m.MapFrom(v => v.WorkingToDate.UnixToDateTime()))
                .ForMember(d => d.IsIgnoreAccountant, m => m.MapFrom(v => v.IsIgnoreAccountant))
                .ForMember(m => m.IsAutoUpdateWorkingDate, m => m.MapFrom(v => v.IsAutoUpdateWorkingDate));
                
        }
    }
}
