using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepRoleClientModel : IMapFrom<ProductionStepRoleClient>
    {
        public long ContainerId { get; set; }
        public int ContainerTypeId { get; set; }
        public string ClientData { get; set; }
    }

    public class RoleClientData
    {
        public string key { get; set; }
        public bool value { get; set; }
        public float pos_x { get; set; }
        public float pos_y { get; set; }
    }
}
