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
    public class ImportProductModel
    {
        public long ProductId { get; set; }
        public decimal PlanQuantity { get; set; }
        public decimal ImportQuantity { get; set; }
        public string OrderCode { get; set; }

        public decimal LastestDateImportQuantity { get; set; }
        public decimal LastestWeekImportQuantity { get; set; }
    }
}
