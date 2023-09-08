using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Services.Accountancy.Model.Input;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetRawModel : IMapFrom<TimeSheetRaw>
    {
        public long TimeSheetRawId { get; set; }
        public long EmployeeId { get; set; }
        public long Date { get; set; }
        public double Time { get; set; }
        public TimeKeepingMethodType TimeKeepingMethod { get; set; }
        public string TimeKeepingRecorder { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<TimeSheetRawModel, TimeSheetRaw>()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.UnixToDateTime()))
            .ForMember(m => m.Time, v => v.MapFrom(m => TimeSpan.FromSeconds(m.Time)))
            .ReverseMapCustom()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.GetUnix()))
            .ForMember(m => m.Time, v => v.MapFrom(m => m.Time.TotalSeconds));
        }
    }

    public class TimeSheetRawViewModel : TimeSheetRawModel, IMapFrom<TimeSheetRaw>
    {
        public NonCamelCaseDictionary Employee { get; set; }
    }

    public class TimeSheetRawRequestModel : TimeSheetRawFilterModel
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class TimeSheetRawFilterModel
    {
        public string Keyword { get; set; }
        public string OrderBy { get; set; }
        public bool Asc { get; set; } = true;
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }
        public Clause ColumnsFilters { get; set; }

        public HrTypeBillsFilterModel? HrTypeFilters { get; set; }
    }

    public class TimeSheetRawImportFieldModel
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin nhân viên")]
        [Display(Name = "Nhân viên (Mã nhân viên hoặc email nhân viên)", GroupName = "TT nhân viên")]
        public long EmployeeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày chấm công")]
        [Display(Name = "Ngày chấm công", GroupName = "TT chấm công")]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập giờ chấm công")]
        [Display(Name = "Giờ chấm công (Định dạng hh:mm)", GroupName = "TT chấm công")]
        public TimeSpan Time { get; set; }
    }
}
