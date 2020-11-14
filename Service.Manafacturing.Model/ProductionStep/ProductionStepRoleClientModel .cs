using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepRoleClientModel : IMapFrom<ProductionStepRoleClient>
    {
        public int ContainerId { get; set; }
        public int ContainerTypeId { get; set; }
        public string ClientData { get; set; }
    }
}
