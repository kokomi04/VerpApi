using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionPlaningOrderModel : ProductionPlaningOrderBaseModel, IMapFrom<ProductionPlaningOrderEntity>
    {
        public long VoucherDate { get; set; }
        public long? FinishDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionPlaningOrderEntity, ProductionPlaningOrderModel>()
                .ForMember(dest => dest.VoucherDate, opt => opt.MapFrom(source => source.VoucherDate.GetUnix()))
                .ForMember(dest => dest.FinishDate, opt => opt.MapFrom(source => source.FinishDate.GetUnix()));
        }
    }

    public class ProductionPlaningOrderEntity : ProductionPlaningOrderBaseModel
    {
        public DateTime VoucherDate { get; set; }
        public DateTime? FinishDate { get; set; }
    }

    public class ProductionPlaningOrderBaseModel
    {
        public int ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
    }

    public class ProductionPlaningOrderDetailModel
    {
        public int ProductionOrderDetailId { get; set; }
        public int TotalQuantity { get; set; }
        public string ProductTitle { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string UnitName { get; set; }
        public int PlannedQuantity { get; set; }
        public string OrderCode { get; set; }
        public string PartnerTitle { get; set; }
        public long ProductionStepId { get; set; }
    }
}
