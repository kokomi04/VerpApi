using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class IgnoreAllocationModel : IMapFrom<IgnoreAllocation>
    {
        public long ProductionOrderId { get; set; }
        public string InventoryCode { get; set; }
        public int ProductId { get; set; }
    }
}
