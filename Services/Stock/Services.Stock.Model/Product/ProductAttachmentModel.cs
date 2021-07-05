using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductAttachmentModel : IMapFrom<ProductAttachment>
    {
        public long? ProductAttachmentId { get; set; }
        public int ProductId { get; set; }
        public long AttachmentFileId { get; set; }
        public string Title { get; set; }
    }
}
