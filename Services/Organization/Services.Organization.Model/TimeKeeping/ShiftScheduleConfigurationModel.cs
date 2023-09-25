using AutoMapper;
using Newtonsoft.Json;
using Services.Organization.Model.TimeKeeping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class ShiftScheduleConfigurationModel : IMapFrom<ShiftScheduleConfiguration>
{
    public long ShiftScheduleId { get; set; }

    public int ShiftConfigurationId { get; set; }

    public EnumShiftScheduleMode ShiftScheduleMode { get; set; }

    public bool? IsMonday { get; set; }

    public bool? IsTuesday { get; set; }

    public bool? IsWednesday { get; set; }

    public bool? IsThursday { get; set; }

    public bool? IsFriday { get; set; }

    public bool? IsSaturday { get; set; }

    public bool? IsSunday { get; set; }

    public List<long?> ShiftAssignedDate { get; set; }

    public int? CycleRepeat { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMapCustom<ShiftScheduleConfiguration, ShiftScheduleConfigurationModel>()
        .AfterMap((src, dest) =>
        {
            dest.ShiftAssignedDate = src.ShiftAssignedDate.Select(d => d.GetUnix()).ToList();
        })
        .ReverseMapCustom()
        .AfterMap((src, dest) =>
        {
            dest.ShiftAssignedDate = src.ShiftAssignedDate.Select(d => d.UnixToDateTime()).ToList();
        });
    }
}
