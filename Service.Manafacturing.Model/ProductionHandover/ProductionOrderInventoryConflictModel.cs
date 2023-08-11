using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionOrderInventoryConflictModel : IMapFrom<ProductionOrderInventoryConflict>
    {
        public long ProductionOrderId { get; set; }

        public long InventoryDetailId { get; set; }

        public int ProductId { get; set; }

        public int InventoryTypeId { get; set; }

        public long InventoryId { get; set; }

        public long InventoryDate { get; set; }

        public string InventoryCode { get; set; }

        public decimal InventoryQuantity { get; set; }

        public long? InventoryRequirementDetailId { get; set; }

        public long? InventoryRequirementId { get; set; }

        public decimal? RequireQuantity { get; set; }

        public string InventoryRequirementCode { get; set; }

        public string Content { get; set; }
        public decimal? HandoverInventoryQuantitySum { get; set; }
        public EnumConflictAllowcationStatus ConflictAllowcationStatusId { get; set; }
    }

}
