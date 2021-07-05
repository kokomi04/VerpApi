using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionPlan
{
    public class ProductionWeekPlanDetailModel: IMapFrom<ProductionWeekPlanDetail>
    {
        public int ProductCateId { get; set; }
        public decimal MaterialQuantity { get; set; }
    }
}