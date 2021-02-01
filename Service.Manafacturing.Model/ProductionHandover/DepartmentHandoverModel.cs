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
    public class DepartmentHandoverModel 
    {
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string ScheduleCode { get; set; }
        public long ScheduleTurnId { get; set; }
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
