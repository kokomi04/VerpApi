using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class ProductionScheduleTurnShiftModel : IMapFrom<ProductionScheduleTurnShift>
    {
        public long? ProductionScheduleTurnShiftId { get; set; }
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }
        public decimal? Hours { get; set; }
        public Dictionary<int, ProductionScheduleTurnShiftUserModel> Users { get; set; }

        private DateTime? UnixToDateTime(long? unix)
        {
            return unix.UnixToDateTime();

        }

        private long? DateTimeToUnix(DateTime? date)
        {
            return date.GetUnix();
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionScheduleTurnShift, ProductionScheduleTurnShiftModel>()
                .ForMember(s => s.FromDate, d => d.MapFrom(m => DateTimeToUnix(m.FromDate)))
                .ForMember(s => s.ToDate, d => d.MapFrom(m => DateTimeToUnix(m.ToDate)))
                .ForMember(s => s.Users, d => d.Ignore())
                .ReverseMap()
                .ForMember(s => s.FromDate, d => d.MapFrom(m => UnixToDateTime(m.FromDate)))
                .ForMember(s => s.ToDate, d => d.MapFrom(m => UnixToDateTime(m.ToDate)))
                .ForMember(s => s.ProductionScheduleTurnShiftUser, d => d.Ignore());

        }

    }

    public class ProductionScheduleTurnShiftUserModel : IMapFrom<ProductionScheduleTurnShiftUser>
    {        
        public decimal? Quantity { get; set; }
        public decimal? Money { get; set; }
    }
}
