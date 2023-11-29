using AutoMapper;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Organization;
using VErp.Commons.Library;
using ModelWorkingDate = VErp.Infrastructure.EF.OrganizationDB.WorkingDate;


namespace VErp.Services.Organization.Model.WorkingDate
{
    public class WorkingDateModel : IMapFrom<ModelWorkingDate>
    {
        public int WorkingDateId { get; set; }

        public int UserId { get; set; }

        public int SubsidiaryId { get; set; }

        public bool? IsIgnoreFilterAccountant { get; set; }

        public bool? IsAutoUpdateWorkingDate { get; set; }

        public long? WorkingFromDate { get; set; }

        public long? WorkingToDate { get; set; }
        public string WorkingDateConfig { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ModelWorkingDate, WorkingDateModel>()
                .ForMember(m=> m.IsIgnoreFilterAccountant, m=> m.MapFrom(v=> v.WorkingDateConfig.JsonDeserialize<WorkingDateModel>().IsIgnoreFilterAccountant))
                .ForMember(m => m.IsAutoUpdateWorkingDate, m => m.MapFrom(v => v.WorkingDateConfig.JsonDeserialize<WorkingDateModel>().IsAutoUpdateWorkingDate))
                .ForMember(m => m.WorkingFromDate, m => m.MapFrom(v => v.WorkingFromDate.GetUnix()))
                .ForMember(m => m.WorkingToDate, m => m.MapFrom(v => v.WorkingToDate.GetUnix()))
                .ReverseMapCustom()
                .ForMember(m => m.WorkingFromDate, m => m.MapFrom(v => v.WorkingFromDate.UnixToDateTime()))
                .ForMember(m => m.WorkingToDate, m => m.MapFrom(v => v.WorkingToDate.UnixToDateTime()))
                .ForMember(m=> m.WorkingDateConfig, m=> m.MapFrom(v=> JsonUtils.JsonSerialize(new { v.IsIgnoreFilterAccountant, v.IsAutoUpdateWorkingDate})));
                
        }
    }
}
