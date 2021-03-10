using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class DepartmentHandoverModel : IMapFrom<DepartmentHandoverEntity>
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public long ProductionStepId { get; set; }
        public string StepName { get; set; }

        public string ProductTitle { get; set; }

        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public string Material { get; set; }
        public string InOutType { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public decimal HandoveredQuantity { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DepartmentHandoverEntity, DepartmentHandoverModel>()
                .ForMember(m => m.StartDate, v => v.MapFrom(m => m.StartDate.GetUnix()))
                .ForMember(m => m.EndDate, v => v.MapFrom(m => m.EndDate.GetUnix()));
        }
    }

    public class DepartmentHandoverEntity
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public long ProductionStepId { get; set; }
        public string StepName { get; set; }

        public string ProductTitle { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Material { get; set; }
        public string InOutType { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public decimal HandoveredQuantity { get; set; }
    }

}
