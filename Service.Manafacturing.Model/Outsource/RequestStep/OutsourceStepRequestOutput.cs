using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestOutput: OutsourceStepRequestPrivateKey
    {
        public long ProductionOrderId { get; set; }
        public long OutsourceStepRequestFinishDate { get; set; }
        public long OutsourceStepRequestDate { get; set; }
        public bool IsInvalid { get; set; }
        public int OutsourceStepRequestStatusId { get; set; }

        public ICollection<OutsourceStepRequestDetailOutput> DetailInputs { get; set; }
        public ICollection<long> ProductionStepIds { get; set; }
        public ICollection<ProductionStepLinkDataRoleModel> Roles { get; set; }
    }

    public class OutsourceStepRequestDetailOutput
    {
        public long ProductionStepLinkDataId { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalOutsourceOrderQuantity { get; set; }
        public int RoleType { get; set; }
    }
}
