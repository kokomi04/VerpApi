using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Services.Accountancy.Model.Input;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetRawModel : IMapFrom<TimeSheetRaw>
    {
        public const string GroupName = "Thông tin chấm công";

        public long TimeSheetRawId { get; set; }
        public long EmployeeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày chấm công")]
        [Display(Name = "Ngày chấm công", GroupName = GroupName)]
        [AllowedDataType(EnumDataType.Date)]
        public long Date { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giờ chấm công")]
        [Display(Name = "Giờ chấm công", GroupName = GroupName)]
        [AllowedDataType(EnumDataType.Time)]
        public double Time { get; set; }

        [Display(Name = "Hình thức chấm công", GroupName = GroupName)]
        [AllowedDataType(EnumDataType.Enum)]
        public TimeKeepingMethodType TimeKeepingMethod { get; set; }

        [Display(Name = "Người thực hiện chấm công", GroupName = GroupName)]
        [AllowedDataType(EnumDataType.Text)]
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

    public class TimeSheetRawExportModel : TimeSheetRawFilterModel
    {
        public IList<string> FieldNames { get; set; }
    }
    public class TimeSheetRawImportFieldModel : TimeSheetRawModel
    {
        [Display(Name = "Mã nhân viên")]
        public string so_ct { get; set; }

        [Display(Name = "Mã chấm công")]
        public string ma_cham_cong { get; set; }

    }
}
