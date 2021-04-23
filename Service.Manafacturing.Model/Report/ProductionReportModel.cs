using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Model.Report
{
    public class ProductionReportModel : IMapFrom<ProductionOrderListEntity>
    {
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public string PartnerTitle { get; set; }
        public int? ProductId { get; set; }
        public string ProductTitle { get; set; }
        public decimal Quantity { get; set; }
        public decimal CompletedQuantity { get; set; }
        public EnumProductionStatus ProductionOrderStatus { get; set; }
        public string UnfinishedStepTitle { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionOrderListEntity, ProductionReportModel>()
                .ForMember(dest => dest.ProductionOrderCode, opt => opt.MapFrom(source => source.ProductionOrderCode))
                .ForMember(dest => dest.ProductionOrderStatus, opt => opt.MapFrom(source => (EnumProductionStatus)source.ProductionOrderStatus))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.GetUnix()));
        }
    }

}
