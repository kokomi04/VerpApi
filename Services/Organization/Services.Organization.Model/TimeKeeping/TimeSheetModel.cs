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
    public class TimeSheetModel : IMapFrom<TimeSheet>
    {
        public long TimeSheetId { get; set; }
        public long EmployeeId { get; set; }
        public long Date { get; set; }
        public double TimeIn { get; set; }
        public double TimeOut { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<TimeSheetModel, TimeSheet>()
            .ForMember(m=>m.Date, v=>v.MapFrom(m=>m.Date.UnixToDateTime()))
            .ForMember(m=>m.TimeIn, v=>v.MapFrom(m=>TimeSpan.FromSeconds(m.TimeIn)))
            .ForMember(m => m.TimeOut, v => v.MapFrom(m => TimeSpan.FromSeconds(m.TimeOut)))
            .ReverseMap()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.GetUnix()))
            .ForMember(m => m.TimeIn, v => v.MapFrom(m => m.TimeIn.TotalSeconds))
            .ForMember(m => m.TimeOut, v => v.MapFrom(m => m.TimeOut.TotalSeconds));
        }
    }

    public class TimeSheetImportFieldModel
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin nhân viên")]
        [Display(Name = "Nhân viên", GroupName = "TT nhân viên")]
        public long EmployeeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày chấm công")]
        [Display(Name = "Ngày", GroupName = "TT chấm công")]
        public long Date { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập giờ vào")]
        [Display(Name = "Giờ vào", GroupName = "TT chấm công")]
        public double TimeIn { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập giờ ra")]
        [Display(Name = "Giờ ra", GroupName = "TT chấm công")]
        public double TimeOut { get; set; }

    }
}
