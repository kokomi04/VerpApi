using AutoMapper;
using System;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetDetailShiftCountedModel : IMapFrom<TimeSheetDetailShiftCounted>
    {
        public long TimeSheetDetailShiftCountedId { get; set; }

        public long TimeSheetDetailId { get; set; }

        public int ShiftConfigurationId { get; set; }

        public int CountedSymbolId { get; set; }
    }
}
