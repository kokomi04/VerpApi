using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.WorkloadPlanModel
{
    public class MonthlyImportStockModel
    {
        public long ProductId { get; set; }
        public decimal PlanQuantity { get; set; }
        public decimal ImportQuantity { get; set; }
    }

}
