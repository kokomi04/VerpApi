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
    public class ProductionPlanningOrderModel : ProductionPlanningOrderBaseModel, IMapFrom<ProductionPlanningOrderEntity>
    {
        public long ProductionDate { get; set; }
        public long? FinishDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionPlanningOrderEntity, ProductionPlanningOrderModel>()
                .ForMember(dest => dest.ProductionDate, opt => opt.MapFrom(source => source.ProductionDate.GetUnix()))
                .ForMember(dest => dest.FinishDate, opt => opt.MapFrom(source => source.FinishDate.GetUnix()));
        }
    }

    public class ProductionPlanningOrderEntity : ProductionPlanningOrderBaseModel
    {
        public DateTime ProductionDate { get; set; }
        public DateTime? FinishDate { get; set; }
    }

    public class ProductionPlanningOrderBaseModel
    {
        public int ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
    }

    public class ProductionPlanningOrderDetailModel
    {
        public int ProductionOrderDetailId { get; set; }
        public int TotalQuantity { get; set; }
        public string ProductTitle { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitName { get; set; }
        public int PlannedQuantity { get; set; }
        public string OrderCode { get; set; }
        public string PartnerTitle { get; set; }
        public long ProductionStepId { get; set; }
    }
}
