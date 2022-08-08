using AutoMapper;
using System;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetRawModel : IMapFrom<TimeSheetRaw>
    {
        public long TimeSheetRawId { get; set; }
        public long EmployeeId { get; set; }
        public long Date { get; set; }
        public double Time { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapIgnoreNoneExist<TimeSheetRawModel, TimeSheetRaw>()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.UnixToDateTime()))
            .ForMember(m => m.Time, v => v.MapFrom(m => TimeSpan.FromSeconds(m.Time)))
            .ReverseMapIgnoreNoneExist()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.GetUnix()))
            .ForMember(m => m.Time, v => v.MapFrom(m => m.Time.TotalSeconds));
        }
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
