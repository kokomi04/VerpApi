using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetModel : IMapFrom<TimeSheet>
    {
        public long TimeSheetId { get; set; }

        public string Title { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public long BeginDate { get; set; }

        public long EndDate { get; set; }

        public string Note { get; set; }

        public bool IsApprove { get; set; }

        public IList<TimeSheetAggregateModel> TimeSheetAggregate { get; set; } = new List<TimeSheetAggregateModel>();

        public IList<TimeSheetDepartmentModel> TimeSheetDepartment { get; set; } = new List<TimeSheetDepartmentModel>();

        public IList<TimeSheetDetailModel> TimeSheetDetail { get; set; } = new List<TimeSheetDetailModel>();


        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<TimeSheet, TimeSheetModel>()
            .ForMember(m => m.TimeSheetDetail, v => v.Ignore())
            .ForMember(m => m.TimeSheetAggregate, v => v.Ignore())
            .ForMember(m => m.TimeSheetDepartment, v => v.MapFrom(m => m.TimeSheetDepartment))
            .ForMember(m => m.BeginDate, v => v.MapFrom(m => m.BeginDate.GetUnix()))
            .ForMember(m => m.EndDate, v => v.MapFrom(m => m.EndDate.GetUnix()))
            .ReverseMapCustom()
            .ForMember(m => m.TimeSheetDetail, v => v.Ignore())
            .ForMember(m => m.BeginDate, v => v.MapFrom(m => m.BeginDate.UnixToDateTime()))
            .ForMember(m => m.EndDate, v => v.MapFrom(m => m.EndDate.UnixToDateTime()))
            .ForMember(m => m.TimeSheetAggregate, v => v.Ignore())
            .ForMember(m => m.TimeSheetDepartment, v => v.MapFrom(m => m.TimeSheetDepartment));
        }
    }

    public class TimeSheetRequestModel : TimeSheetFilterModel
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class TimeSheetFilterModel
    {
        public string? Keyword { get; set; }
        public string? OrderBy { get; set; }
        public bool Asc { get; set; } = true;
        public List<int?> DepartmentIds { get; set; }
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }
        public Clause? ColumnsFilters { get; set; }
    }

    public class TimeSheetByEmployeeModel
    {
        public long EmployeeId { get; set; }
        public List<TimeSheetDetailModel> TimeSheetDetail { get; set; } = new List<TimeSheetDetailModel>();
    }

    public class TimeSheetByEmployeeRequestModel
    {
        public long TimeSheetId { get; set; }
        public int[] DepartmentIds { get; set; }
        public long BeginDate { get; set; }
        public long EndDate { get; set; }
    }

    public class TimeSheetDetailRequestModel
    {
        public TimeSheetDetailModel TimeSheetDetail { get; set; }

        public double? TimeIn { get; set; }

        public double? TimeOut { get; set; }
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

        [Display(Name = "Tổng thời gian(phút) vào muộn", GroupName = "Vào trễ")]
        public long MinsLate { get; set; }
        [Display(Name = "Số lần vào", GroupName = "Vào trễ")]
        public int CountedLate { get; set; }

        [Display(Name = "Tổng thời gian(phút) về sớm", GroupName = "Ra sớm")]
        public long MinsEarly { get; set; }

        [Display(Name = "Số lần về sớm", GroupName = "Ra sớm")]
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
