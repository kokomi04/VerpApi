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
        public TimeSheetModel()
        {
            TimeSheetDetails = new List<TimeSheetDetailModel>();
            TimeSheetAggregates = new List<TimeSheetAggregateModel>();
            TimeSheetDayOffs = new List<TimeSheetDayOffModel>();
        }

        public long TimeSheetId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Note { get; set; }
        public bool IsApprove { get; set; }

        public long? BeginDate { get; set; }
        public long? EndDate { get; set; }

        public IList<TimeSheetDetailModel> TimeSheetDetails { get; set; }
        public IList<TimeSheetAggregateModel> TimeSheetAggregates { get; set; }
        public IList<TimeSheetDayOffModel> TimeSheetDayOffs { get; set; }
        public IList<TimeSheetOvertimeModel> TimeSheetOvertimes { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<TimeSheet, TimeSheetModel>()
            .ForMember(m => m.TimeSheetDetails, v => v.MapFrom(m => m.TimeSheetDetail))
            .ForMember(m => m.TimeSheetDayOffs, v => v.MapFrom(m => m.TimeSheetDayOff))
            .ForMember(m => m.TimeSheetAggregates, v => v.MapFrom(m => m.TimeSheetAggregate))
            .ForMember(m => m.TimeSheetOvertimes, v => v.MapFrom(m => m.TimeSheetOvertime))
            .ForMember(m => m.BeginDate, v => v.MapFrom(m => m.BeginDate.GetUnix()))
            .ForMember(m => m.EndDate, v => v.MapFrom(m => m.EndDate.GetUnix()))
            .ReverseMap()
            .ForMember(m => m.TimeSheetDetail, v => v.Ignore())
            .ForMember(m => m.TimeSheetDayOff, v => v.Ignore())
            .ForMember(m => m.BeginDate, v => v.MapFrom(m => m.BeginDate.UnixToDateTime()))
            .ForMember(m => m.EndDate, v => v.MapFrom(m => m.EndDate.UnixToDateTime()))
            .ForMember(m => m.TimeSheetAggregate, v => v.Ignore())
            .ForMember(m => m.TimeSheetOvertime, v => v.Ignore());
        }
    }

    public class TimeSheetImportFieldModel
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin nhân viên")]
        [Display(Name = "Mã nhân viên", GroupName = "Thông tin nhân viên")]
        public string EmployeeCode { get; set; }
        
        [Display(Name = "Tên nhân viên", GroupName = "Thông tin nhân viên")]
        public string EmployeeName { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 1", GroupName = "TT chấm công")]
        // public string TimeKeepingDay1 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 2", GroupName = "TT chấm công")]
        // public string TimeKeepingDay2 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 3", GroupName = "TT chấm công")]
        // public string TimeKeepingDay3 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 4", GroupName = "TT chấm công")]
        // public string TimeKeepingDay4 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 5", GroupName = "TT chấm công")]
        // public string TimeKeepingDay5 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 6", GroupName = "TT chấm công")]
        // public string TimeKeepingDay6 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 7", GroupName = "TT chấm công")]
        // public string TimeKeepingDay7 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 8", GroupName = "TT chấm công")]
        // public string TimeKeepingDay8 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 9", GroupName = "TT chấm công")]
        // public string TimeKeepingDay9 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 10", GroupName = "TT chấm công")]
        // public string TimeKeepingDay10 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 11", GroupName = "TT chấm công")]
        // public string TimeKeepingDay11 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 12", GroupName = "TT chấm công")]
        // public string TimeKeepingDay12 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 13", GroupName = "TT chấm công")]
        // public string TimeKeepingDay13 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 14", GroupName = "TT chấm công")]
        // public string TimeKeepingDay14 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 15", GroupName = "TT chấm công")]
        // public string TimeKeepingDay15 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 16", GroupName = "TT chấm công")]
        // public string TimeKeepingDay16 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 17", GroupName = "TT chấm công")]
        // public string TimeKeepingDay17 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 18", GroupName = "TT chấm công")]
        // public string TimeKeepingDay18 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 19", GroupName = "TT chấm công")]
        // public string TimeKeepingDay19 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 20", GroupName = "TT chấm công")]
        // public string TimeKeepingDay20 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 21", GroupName = "TT chấm công")]
        // public string TimeKeepingDay21 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 22", GroupName = "TT chấm công")]
        // public string TimeKeepingDay22 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 23", GroupName = "TT chấm công")]
        // public string TimeKeepingDay23 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 24", GroupName = "TT chấm công")]
        // public string TimeKeepingDay24 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 25", GroupName = "TT chấm công")]
        // public string TimeKeepingDay25 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 26", GroupName = "TT chấm công")]
        // public string TimeKeepingDay26 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 27", GroupName = "TT chấm công")]
        // public string TimeKeepingDay27 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 28", GroupName = "TT chấm công")]
        // public string TimeKeepingDay28 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 29", GroupName = "TT chấm công")]
        // public string TimeKeepingDay29 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 30", GroupName = "TT chấm công")]
        // public string TimeKeepingDay30 { get; set; }
        
        // [Display(Name = "Thời gian chấm công ngày 31", GroupName = "TT chấm công")]
        // public string TimeKeepingDay31 { get; set; }
        
        [Display(Name = "Tổng thời gian(phút) về muộn", GroupName = "Vào trễ")]
        public long MinsLate { get; set; }
        [Display(Name = "Tổng thời gian(phút) về sớm", GroupName = "Vào trễ")]
        public int CountedLate { get; set; }
        
        [Display(Name = "Tổng thời gian(phút) về sớm", GroupName = "Ra sớm")]
        public long MinsEarly { get; set; }

        [Display(Name = "Tổng thời gian(phút) về sớm", GroupName = "Ra sớm")]
        public int CountedEarly { get; set; }

        
        [Display(Name = "Ngày công thường", GroupName = "Ngày công")]
        public decimal CountedWeekday { get; set; }
        
        [Display(Name = "Ngày công cuối tuần", GroupName = "Ngày công")]
        public decimal CountedWeekend { get; set; }
        
        [Display(Name = "Tổng thời gian(giờ) ngày công thường", GroupName = "Giờ công")]
        public decimal CountedWeekdayHour { get; set; }
        
        [Display(Name = "Tổng thời gian(giờ) ngày công cuối tuần", GroupName = "Giờ công")]
        public decimal CountedWeekendHour { get; set; }
        
        // [Display(Name = "Tổng thời gian(giờ) làm tăng ca 1", GroupName = "Tăng ca(giờ)")]
        // public decimal Overtime1 { get; set; }
        
        // [Display(Name = "Tổng thời gian(giờ) làm tăng ca 2", GroupName = "Tăng ca(giờ)")]
        // public decimal Overtime2 { get; set; }
        
        // [Display(Name = "Tổng thời gian(giờ) làm tăng ca 3", GroupName = "Tăng ca(giờ)")]
        // public decimal Overtime3 { get; set; }
        
        [Display(Name = "Tổng số buổi vắng không phép", GroupName = "Vắng KP")]
        public int CountedAbsence { get; set; }
    }
}
