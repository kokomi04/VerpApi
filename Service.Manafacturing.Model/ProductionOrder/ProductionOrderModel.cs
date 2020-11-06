using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using ProductionOrderEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionOrder;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionOrderModel : ProductionOrderListModel
    {
        public ProductionOrderModel()
        {
            ProductionOrderDetail = new HashSet<ProductionOrderDetailModel>();
        }

        public virtual ICollection<ProductionOrderDetailModel> ProductionOrderDetail { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionOrderModel, ProductionOrderEntity>()
                .ForMember(dest => dest.ProductionOrderDetail, opt => opt.Ignore());
        }
    }

    public class ProductionOrderListModel : IMapFrom<ProductionOrderEntity>
    {
        public int ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public DateTime VoucherDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public long? PurchaseOrderId { get; set; }
        public int? CustomerId { get; set; }
        public int? ProductionLineId { get; set; }
        public string Description { get; set; }
    }
}
