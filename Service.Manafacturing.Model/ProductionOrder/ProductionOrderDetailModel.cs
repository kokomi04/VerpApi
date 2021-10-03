﻿using AutoMapper;
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
        public long ProductionOrderId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? ReserveQuantity { get; set; }
        public string Note { get; set; }
        //public int Status { get; set; }
        public long? OutsourcePartRequestId { get; set; }
    }

    public class ProductionOrderDetailInputModel : IMapFrom<ProductionOrderDetail>
    {
        public long? ProductionOrderDetailId { get; set; }
        public long ProductionOrderId { get; set; }
        public int? ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? ReserveQuantity { get; set; }
        public string Note { get; set; }
        //public long? OrderDetailId { get; set; }
        public string OrderCode { get; set; }
        public string PartnerId { get; set; }

    }


    public class ProductionOrderExtraInfo
    {
        public long? ProductionOrderDetailId { get; set; }
        // public long OrderId { get; set; }
        // public decimal? OrderQuantity { get; set; }
        // public decimal? OrderedQuantity { get; set; }
        public string PartnerId { get; set; }
        // public string PartnerCode { get; set; }
        // public string PartnerName { get; set; }
        // public string PartnerTitle { get; set; }
        // public string ProductTitle { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public long MainImageFileId { get; set; }
        public long VoucherTypeId { get; set; }
        public string OrderCode { get; set; }
        // public string ContainerNumber { get; set; }
        // public int ContainerQuantity { get; set; }
        public int? ProductId { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string Specification { get; set; }
        public bool InvalidPlan { get; set; }
        // public string CustomerPO { get; set; }
    }
}
