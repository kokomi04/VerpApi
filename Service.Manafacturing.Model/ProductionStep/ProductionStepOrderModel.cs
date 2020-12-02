using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepOrderModel: IMapFrom<ProductionStepOrder>
    {
        public long ProductionStepId { get; set; }
        public string ProductionStepCode { get; set; }
        public long ProductionOrderDetailId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepOrder, ProductionStepOrderModel>()
                .ForMember(m => m.ProductionStepCode, v => v.MapFrom(m => m.ProductionStep.ProductionStepCode))
                .ReverseMap()
                .ForMember(m => m.ProductionStep, v => v.Ignore());
        }
    }
}
