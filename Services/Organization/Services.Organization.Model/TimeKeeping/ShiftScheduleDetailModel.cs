using AutoMapper;
using Services.Organization.Model.TimeKeeping;
using System;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class ShiftScheduleDetailModel : IMapFrom<ShiftScheduleDetail>
{
    public long ShiftScheduleId { get; set; }

    public int ShiftConfigurationId { get; set; }

    public long AssignedDate { get; set; }

    public int EmployeeId { get; set; }

    public bool HasOvertimePlan { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMapCustom<ShiftScheduleDetail, ShiftScheduleDetailModel>()
        .ForMember(m => m.AssignedDate, v => v.MapFrom(m => m.AssignedDate.GetUnix()))
        .ReverseMapCustom()
        .ForMember(m => m.AssignedDate, v => v.MapFrom(m => m.AssignedDate.UnixToDateTime()));
    }
}