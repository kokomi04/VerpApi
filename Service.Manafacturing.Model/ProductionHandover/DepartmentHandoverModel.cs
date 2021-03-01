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
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionDate { get; set; }
        public long FinishDate { get; set; }
        public long ProductionOrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public long ProductionStepId { get; set; }
        public string StepName { get; set; }
        public string Material { get; set; }
        public string InOutType { get; set; }
        public string ReciprocalStep { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public decimal HandoverQuantity { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DepartmentHandoverEntity, DepartmentHandoverModel>()
                .ForMember(m => m.ProductionDate, v => v.MapFrom(m => m.ProductionDate.GetUnix()))
                .ForMember(m => m.FinishDate, v => v.MapFrom(m => m.FinishDate.GetUnix()));
        }
    }

    public class DepartmentHandoverEntity
    {
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public DateTime ProductionDate { get; set; }
        public DateTime FinishDate { get; set; }
        public long ProductionOrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public long ProductionStepId { get; set; }
        public string StepName { get; set; }
        public string Material { get; set; }
        public string InOutType { get; set; }
        public string ReciprocalStep { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public decimal HandoverQuantity { get; set; }
    }
}
