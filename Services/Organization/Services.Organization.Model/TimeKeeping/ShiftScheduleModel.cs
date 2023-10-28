using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Services.Organization.Model.TimeKeeping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class ShiftScheduleModel : IMapFrom<ShiftSchedule>
{
    public long ShiftScheduleId { get; set; }

    public string Title { get; set; }

    public long FromDate { get; set; }

    public long ToDate { get; set; }

    public EnumOvertimeMode OvertimeMode { get; set; }

    public EnumApplicableMode ApplicableMode { get; set; }

    public IList<ShiftScheduleConfigurationModel> ShiftScheduleConfiguration { get; set; } = new List<ShiftScheduleConfigurationModel>();

    public IList<ShiftScheduleDetailModel> ShiftScheduleDetail { get; set; } = new List<ShiftScheduleDetailModel>();

    public void Mapping(Profile profile)
    {
        profile.CreateMapCustom<ShiftSchedule, ShiftScheduleModel>()
        .ForMember(m => m.ShiftScheduleConfiguration, v => v.MapFrom(m => m.ShiftScheduleConfiguration))
        .ForMember(m => m.ShiftScheduleDetail, v => v.MapFrom(m => m.ShiftScheduleDetail))
        .ReverseMapCustom()
        .ForMember(m => m.ShiftScheduleConfiguration, v => v.MapFrom(m => m.ShiftScheduleConfiguration))
        .ForMember(m => m.ShiftScheduleDetail, v => v.MapFrom(m => m.ShiftScheduleDetail));
    }
}

public class ShiftScheduleFilterModel
{
    public string Keyword { get; set; }
    public string OrderBy { get; set; }
    public bool Asc { get; set; } = true;
    public List<int> DepartmentIds { get; set; }
    public long? FromDate { get; set; }
    public long? ToDate { get; set; }
    public Clause ColumnsFilters { get; set; }
}
public class ShiftScheduleRequestModel : ShiftScheduleFilterModel
{
    public int Page { get; set; }
    public int Size { get; set; }
}

public class EmployeeViolationModel
{
    public long EmployeeId { get; set; }
    public long AssignedDate { get; set; }
    public List<long> ShiftScheduleIds { get; set; } = new List<long>();
}

public class ShiftScheduleImportModel
{
    [Display(Name = "Mã nhân viên")]
    public string EmployeeCode { get; set; }

    [Display(Name = "Ngày phân ca")]
    public long AssignedDate { get; set; }

    [Display(Name = "Mã ca")]
    public string ShiftCodes { get; set; }
}