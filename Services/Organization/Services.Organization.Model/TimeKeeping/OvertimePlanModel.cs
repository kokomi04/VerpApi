using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.Attributes;
using VErp.Commons.GlobalObject.DataAnnotationsExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using AutoMapper;

namespace Services.Organization.Model.TimeKeeping
{
    public class OvertimePlanModel : IMapFrom<OvertimePlan>
    {
        public long EmployeeId { get; set; }

        public long AssignedDate { get; set; }

        public int OvertimeLevelId { get; set; }

        public int OvertimeMins { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<OvertimePlanModel, OvertimePlan>()
            .ForMember(m => m.AssignedDate, v => v.MapFrom(m => m.AssignedDate.UnixToDateTime()))
            .ReverseMapCustom()
            .ForMember(m => m.AssignedDate, v => v.MapFrom(m => m.AssignedDate.GetUnix()));
        }
    }

    public class OvertimePlanRequestModel
    {
        public List<OvertimePlanModel> OvertimePlans { get; set; } = new List<OvertimePlanModel> ();
        public long FromDate { get; set; }
        public long ToDate { get; set; }
        public List<int> DepartmentIds { get; set; } = new List<int>();
    }
}