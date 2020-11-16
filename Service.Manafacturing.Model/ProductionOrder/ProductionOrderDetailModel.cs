using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionOrderDetailOutputModel : ProductionOrderExtraInfo
    {
        public int ProductionOrderId { get; set; }
        public int? Quantity { get; set; }
        public int? ReserveQuantity { get; set; }
        public string Note { get; set; }
        public int Status { get; set; }
    }

    public class ProductionOrderDetailInputModel :  IMapFrom<ProductionOrderDetail>
    {
        public int? ProductionOrderDetailId { get; set; }
        public int ProductionOrderId { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; }
        public int? ReserveQuantity { get; set; }
        public string Note { get; set; }
        public long? OrderDetailId { get; set; }
    }


    public class ProductionOrderExtraInfo
    {
        public int? ProductionOrderDetailId { get; set; }
        public long OrderDetailId { get; set; }
        public decimal? OrderQuantity { get; set; }
        public int? OrderedQuantity { get; set; }
        public string PartnerId { get; set; }
        public string ProductTitle { get; set; }
        public string OrderCode { get; set; }
        public int? ProductId { get; set; }
        public string PartnerTitle { get; set; }
        public string UnitName { get; set; }
    }
}
