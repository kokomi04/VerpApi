using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionOrder {
    public class ProductionOrderAttachmentModel: IMapFrom<ProductionOrderAttachment> {
        public int ProductionOrderAttachmentId { get; set; }
        public long ProductionOrderId { get; set; }
        public string Title { get; set; }
        public long AttachmentFileId { get; set; }
    }
}
