using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionStepHandoverByDepartmentModel
    {
        public int RowNumber { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionOrderDate { get; set; }
        public long ProductionOrderStartDate { get; set; }
        public long ProductionOrderEndDate { get; set; }

        public int? ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string OrderCode { get; set; }
        public decimal? ProductQuantity { get; set; }

        public int StepId { get; set; }
        public string StepName { get; set; }
        public long ProductionStepId { get; set; }
        public string ProductionStepName { get; set; }
        public long? AssignStartDate { get; set; }
        public long? AssignEndDate { get; set; }

        public EnumProductionStepLinkDataObjectType InputLinkDataObjectTypeId { get; set; }
        public long? InputLinkDataObjectId { get; set; }
        public string InputLinkDataObjectCode { get; set; }
        public string InputLinkDataObjectName { get; set; }
        public decimal InputAssignmentQuantity { get; set; }
        public decimal InputHandoverQuantity { get; set; }
        public EnumAssignedProgressStatus AssignedInputStatus { get; set; }


        public EnumProductionStepLinkDataObjectType OutputLinkDataObjectTypeId { get; set; }
        public long? OutputLinkDataObjectId { get; set; }
        public string OutputLinkDataObjectCode { get; set; }
        public string OutputLinkDataObjectName { get; set; }
        public decimal OutputAssignmentQuantity { get; set; }
        public decimal OutputHandoverQuantity { get; set; }
        public EnumAssignedProgressStatus AssignedProgressStatus { get; set; }
    }
}
