using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionOrderMaterialsModel
    {
        public long ProductionStepId { get; set; }
        public string ProductionStepTitle { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? AssignmentQuantity { get; set; }
        public int? DepartmentId { get; set; }
        public decimal RateQuantity { get; set; }
    }
}
