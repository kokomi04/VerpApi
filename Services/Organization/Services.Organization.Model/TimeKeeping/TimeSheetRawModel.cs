using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using AutoMapper;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
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
            profile.CreateMap<TimeSheetRawModel, TimeSheetRaw>()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.UnixToDateTime()))
            .ForMember(m => m.Time, v => v.MapFrom(m => TimeSpan.FromSeconds(m.Time)))
            .ReverseMap()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.GetUnix()))
            .ForMember(m => m.Time, v => v.MapFrom(m => m.Time.TotalSeconds));
        }
    }

    public class TimeSheetRawImportFieldModel
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin nhân viên")]
        [Display(Name = "Nhân viên", GroupName = "TT nhân viên")]
        public long EmployeeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày chấm công")]
        [Display(Name = "Ngày", GroupName = "TT chấm công")]
        public long Date { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập giờ chấm công")]
        [Display(Name = "Giờ chấm công", GroupName = "TT chấm công")]
        public double Time { get; set; }
    }
}
