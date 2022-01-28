using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionPlan
{
    public class ProductionPlanExportModel
    {
        public int[] ProductCateIds { get; set; }
        public string MonthPlanName { get; set; }
        public string Note { get; set; }
    }

}
