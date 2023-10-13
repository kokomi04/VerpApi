using AutoMapper;
using System;
using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetDetailModel : IMapFrom<TimeSheetDetail>
    {
        public long TimeSheetDetailId { get; set; }

        public long TimeSheetId { get; set; }

        public long EmployeeId { get; set; }

        public long Date { get; set; }

        public EnumTimeSheetDateType TimeSheetDateType { get; set; }

        public bool IsScheduled { get; set; }

        public IList<TimeSheetDetailShiftModel> TimeSheetDetailShift { get; set; } = new List<TimeSheetDetailShiftModel>();

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<TimeSheetDetailModel, TimeSheetDetail>()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.UnixToDateTime()))
            .ForMember(m => m.TimeSheetDetailShift, v => v.MapFrom(m => m.TimeSheetDetailShift))
            .ReverseMapCustom()
            .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.GetUnix()))
            .ForMember(m => m.TimeSheetDetailShift, v => v.MapFrom(m => m.TimeSheetDetailShift));
        }
    }
}
