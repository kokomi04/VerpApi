using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using OutsourceOrderMaterialsEntity = VErp.Infrastructure.EF.ManufacturingDB.OutsourceOrderMaterials;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderMaterialsModel: IMapFrom<OutsourceOrderMaterialsEntity>
    {
        public long OutsourceOrderMaterialsId { get; set; }
        public long OutsourceOrderId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public long? OutsourceRequestId { get; set; }
        public long? ProductionStepLinkDataId { get; set; }
    }
}
