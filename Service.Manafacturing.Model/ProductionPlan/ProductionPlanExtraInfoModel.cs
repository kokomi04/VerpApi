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
    public class ProductionPlanExtraInfoModel : IMapFrom<ProductionPlanExtraInfo>
    {
        public int MonthPlanId { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public int SortOrder { get; set; }
        public string Note { get; set; }

        public ProductionPlanExtraInfoModel()
        {
        }
    }

}
