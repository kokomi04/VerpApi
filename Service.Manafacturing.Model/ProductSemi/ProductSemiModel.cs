using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using ProductSemiEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductSemi;

namespace VErp.Services.Manafacturing.Model.ProductSemi
{
    public class ProductSemiModel: IMapFrom<ProductSemiEntity>
    {
        public long ProductSemiId { get; set; }
        public long ContainerId { get; set; }
        public EnumProductionProcess.ContainerType ContainerTypeId { get; set; }
        public string Title { get; set; }
    }
}
