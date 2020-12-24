using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Model.Report
{
    public class ProductionScheduleReportModel : ProductionScheduleModel
    {
        public decimal CompletedQuantity { get; set; }
        public string UnfinishedStepTitle { get; set; }
    }
}
