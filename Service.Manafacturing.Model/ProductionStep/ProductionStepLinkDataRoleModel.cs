using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepLinkDataRoleModel : IMapFrom<ProductionStepLinkDataRole>
    {
        public long ProductionStepId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public ProductionStepLinkDataRoleType ProductionStepLinkDataRoleTypeId { get; set; }
    }
}
