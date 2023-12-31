﻿using System.Collections.Generic;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestOutput : OutsourceStepRequestPrivateKey
    {
        public long ProductionOrderId { get; set; }
        public long OutsourceStepRequestFinishDate { get; set; }
        public long OutsourceStepRequestDate { get; set; }
        public bool IsInvalid { get; set; }
        public int OutsourceStepRequestStatusId { get; set; }
        public OutsourceStepSetting Setting { get; set; }

        public ICollection<OutsourceStepRequestDetailOutput> DetailOutputs { get; set; }
        public ICollection<long> ProductionStepIds { get; set; }
        public List<OutsourceStepRequestDetailOutput> DetailInputs { get; set; }
    }

    public class OutsourceStepRequestDetailOutput
    {
        public long ProductionStepLinkDataId { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalOutsourceOrderQuantity { get; set; }
        public int RoleType { get; set; }
        public string ProductionStepTitle { get; set; }

        public string PurchaseOrderCode { get; set; }
        public string PurchaseOrderId { get; set; }
    }

    public class OutsourceStepRequestMaterialsConsumption
    {
        public long OutsourceStepRequestId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
    }


}
