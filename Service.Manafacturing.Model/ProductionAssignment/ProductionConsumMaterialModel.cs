using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class ProductionConsumMaterialModel : IMapFrom<ProductionConsumMaterial>
    {
        public long? ProductionConsumMaterialDetailId { get; set; }
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }
        public Dictionary<int, Dictionary<long,ProductionConsumMaterialDetailModel>> Details { get; set; }

        public ProductionConsumMaterialModel()
        {
            Details = new Dictionary<int, Dictionary<long,ProductionConsumMaterialDetailModel>>();
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionConsumMaterial, ProductionConsumMaterialModel>()
                .ForMember(s => s.FromDate, d => d.MapFrom(m => m.FromDate.GetUnix()))
                .ForMember(s => s.ToDate, d => d.MapFrom(m => m.ToDate.GetUnix()))
                .ForMember(s => s.Details, d => d.Ignore())
                .ReverseMap()
                .ForMember(s => s.FromDate, d => d.MapFrom(m => m.FromDate.UnixToDateTime()))
                .ForMember(s => s.ToDate, d => d.MapFrom(m => m.ToDate.UnixToDateTime()))
                .ForMember(s => s.ProductionConsumMaterialDetail, d => d.Ignore());
        }

    }

    public class ProductionConsumMaterialDetailModel : IMapFrom<ProductionConsumMaterialDetail>
    {
        public decimal? Quantity { get; set; }
    }
}
