using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionOrderConfigurationModel: IMapFrom<ProductionOrderConfiguration>
    {
        public bool IsEnablePlanEndDate { get; set; } = false;
        public int NumberOfDayPed { get; set; } = 0;
    }
}