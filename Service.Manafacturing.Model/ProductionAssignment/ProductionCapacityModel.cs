using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class ProductionCapacityModel
    {
        public decimal OtherCapacity { get; set; }
        public decimal ScheduleStepWorkload { get; set; }
    }

}
