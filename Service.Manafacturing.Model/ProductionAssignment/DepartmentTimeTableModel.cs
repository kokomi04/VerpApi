using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class DepartmentTimeTableModel : IMapFrom<DepartmentTimeTable>
    {
        public int DepartmentId { get; set; }
        public long WorkDate { get; set; }
        public decimal? HourPerDay { get; set; }

        public DepartmentTimeTableModel()
        {
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DepartmentTimeTable, DepartmentTimeTableModel>()
                .ForMember(s => s.WorkDate, d => d.MapFrom(m => m.WorkDate.GetUnix()))
                .ReverseMap()
                .ForMember(s => s.WorkDate, d => d.MapFrom(m => m.WorkDate.UnixToDateTime()));
        }
    }

}
